using LactoseWebApp.Options;

namespace Lactose.Identity.Options;

[Options(SectionName = "Users:NewUsers")]
public class NewUserOptions
{
    public List<string> DefaultRoles { get; set; } = ["player", "user"];
}