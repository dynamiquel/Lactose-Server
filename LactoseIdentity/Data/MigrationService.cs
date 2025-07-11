using Lactose.Identity.Data.Repos;
using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp;
using Microsoft.Extensions.Options;

namespace Lactose.Identity.Data;

public class MigrationService(
    ILogger<MigrationService> logger,
    IRolesRepo rolesRepo,
    IOptions<DefaultRolesOptions> defaultRolesOptions) 
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Assume zero roles means DB is not yet initialised.
        // TODO: replace with proper migrations system.
        bool isFirstRun = (await rolesRepo.Query()).IsEmpty();
        if (!isFirstRun)
            return;
        
        logger.LogInformation("Performing initial database migration");

        foreach (Role role in defaultRolesOptions.Value.Roles)
        {
            Role? createdRole = await rolesRepo.Set(role);
            if (createdRole == null)
                logger.LogError("Failed to create default role {RoleId}", role.Id);
            else
                logger.LogInformation("Successfully created default role {RoleId}", role.Id);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
       return Task.CompletedTask;
    }
}