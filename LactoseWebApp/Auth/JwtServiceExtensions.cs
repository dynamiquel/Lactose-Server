using System.Text;
using Lactose.Identity.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth;

public static class JwtServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, AuthOptions authOptions)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(GetJwtTokenKey(authOptions.JwtTokenKey)),
                    ValidIssuer = authOptions.JwtIssuer,
                    ValidAudience = authOptions.JwtAudience,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
                
                // Remove the stupid translation ASP.NET does for claim names.
                // I.e. by default, "email" would translate to "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email"
                options.MapInboundClaims = false;
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Check for the token in the Authorization header first. Assumes 'Bearer {token}' format.
                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                        
                        // If no token found, check for a cookie with the token.
                        if (string.IsNullOrEmpty(token))
                            token = context.Request.Cookies[AuthDefaults.JwtAccessTokenCookieName];
                        
                        context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });
        
        return services;
    }

    public static string? GetJwtAccessToken(this HttpContext context)
    {
        // This is fucking stupid but whatever. ASP.NET abstraction bs.
        if (context.User.Identity is CaseSensitiveClaimsIdentity identity)
            return identity.SecurityToken.ToString();
         
        // Check for the token in the Authorization header first. Assumes 'Bearer {token}' format.
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                        
        // If no token found, check for a cookie with the token.
        if (string.IsNullOrEmpty(token))
            token = context.Request.Cookies[AuthDefaults.JwtAccessTokenCookieName];

        return token;
    }

    public static byte[] GetJwtTokenKey(string tokenKey)
    {
        if (string.IsNullOrWhiteSpace(tokenKey))
            throw new SecurityTokenEncryptionKeyNotFoundException("No JwtTokenKey found");
        
        if (tokenKey.Contains('/'))
            return File.ReadAllBytes(tokenKey);
        
        return Encoding.UTF8.GetBytes(tokenKey);
    }
}