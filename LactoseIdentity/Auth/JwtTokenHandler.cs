using System.Security.Claims;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Lactose.Identity.Auth;

public class JwtTokenHandler
{
    readonly IRefreshTokensRepo _refreshTokensRepo;
    readonly SigningCredentials _tokenSigningCredentials;
    readonly IOptions<AuthOptions> _authOptions;
    readonly JsonWebTokenHandler _tokenHandler = new();

    public JwtTokenHandler(
        IRefreshTokensRepo refreshTokensRepo,
        IOptions<AuthOptions> authOptions)
    {
        _refreshTokensRepo = refreshTokensRepo;
        _authOptions = authOptions;
        
        _tokenSigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(JwtServiceExtensions.GetJwtTokenKey(authOptions.Value.JwtTokenKey)), 
            SecurityAlgorithms.HmacSha256);
    }
    
    public string CreateJwtAccessTokenForUser(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new (JwtRegisteredClaimNames.Name, user.DisplayName),
                new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            }),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_authOptions.Value.JwtExpireMinutes),
            Issuer = _authOptions.Value.JwtIssuer,
            Audience = _authOptions.Value.JwtAudience,
            SigningCredentials = _tokenSigningCredentials,
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    public async Task<string> CreateJwtRefreshTokenForUser(User user)
    {
        var randomTokenId = Guid.NewGuid();
        var issuedAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours);

        // Adds the refresh token to the database.
        var refreshToken = await _refreshTokensRepo.Set(new RefreshToken
        {
            Id = randomTokenId.ToString(),
            UserId = user.Id ?? throw new InvalidOperationException(),
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            Issuer = _authOptions.Value.JwtIssuer
        });

        if (refreshToken is null)
            throw new InvalidOperationException();
        
        // Creates a JWT representing the refresh token that clients will use.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (JwtRegisteredClaimNames.Jti, randomTokenId.ToString()),
                new (JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty)
            }),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours),
            Issuer = _authOptions.Value.JwtIssuer,
            SigningCredentials = _tokenSigningCredentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
    
    public async Task<RefreshToken?> ParseRefreshTokenFromJwt(string refreshTokenJwt)
    {
        var token = _tokenHandler.ReadJsonWebToken(refreshTokenJwt);
        TokenValidationResult? tokenValid = await _tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = _tokenSigningCredentials.Key,
            ValidIssuer = _authOptions.Value.JwtIssuer,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = false
        });

        if (!tokenValid.IsValid)
            return null;

        token.TryGetValue(JwtRegisteredClaimNames.Jti, out string? tokenId);
        if (string.IsNullOrEmpty(tokenId))
            return null;
        token.TryGetValue(JwtRegisteredClaimNames.Sub, out string? userId);
        if (string.IsNullOrEmpty(userId))
            return null;
        
        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            UserId = userId,
            IssuedAt = token.IssuedAt,
            ExpiresAt = token.ValidTo,
            Issuer = token.Issuer,
        };

        return refreshToken;
    }
    
    public async Task<TokenValidationResult?> ValidateAccessToken(string accessToken, string? audience)
    {
        var token = _tokenHandler.ReadJsonWebToken(accessToken);
        TokenValidationResult? tokenValid = await _tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = _tokenSigningCredentials.Key,
            ValidIssuer = _authOptions.Value.JwtIssuer,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = !string.IsNullOrEmpty(audience),
            ValidAudience = audience
        });

        return tokenValid;
    }
    
    public async Task DeleteRefreshToken(string refreshTokenId)
    {
        await _refreshTokensRepo.Delete(refreshTokenId);
    }

    public Task<RefreshToken?> GetRefreshTokenById(string refreshTokenId)
    {
        return _refreshTokensRepo.Get(refreshTokenId);
    }
}