using Lactose.Identity.Models;
using LactoseWebApp.Options;

namespace Lactose.Identity.Options;

[Options(SectionName = "Roles:Default")]
public class DefaultRolesOptions
{
    public List<Role> Roles { get; set; } = [
        new()
        {
            Id = "user",
            RoleName = "User",
            Permissions = [
                "identity-self-r", "identity-self-w", "identity-others-r"
            ]
        },
        new()
        {
            Id = "player",
            RoleName = "Player",
            InheritedRoles = ["user"],
            Permissions = [
                "simulation-self-r", "simulation-self-w", "simulation-others-r",
                "economy-r", "economy-user-self-r", "economy-user-others-r",
                "config-r"
            ]
        },
        new()
        {
            Id = "admin",
            RoleName = "Admin",
            InheritedRoles = ["user", "player"],
            Permissions = [
                "simulation-admin", "simulation-others-w",
                "identity-admin", "identity-others-w", "identity-roles-w",
                "config-admin", "config-w",
                "economy-admin", "economy-w", "economy-user-others-w", "economy-transactions-r", "economy-vendors-w"
            ]
        }
    ];
}