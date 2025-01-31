namespace Lactose.Identity;

public static class Permissions
{
    public static readonly string Admin = "identity-admin";
    public static readonly string ReadSelf = "identity-self-r";
    public static readonly string WriteSelf = "identity-self-w";
    public static readonly string ReadOthers = "identity-others-r";
    public static readonly string WriteOthers = "identity-others-w";
    public static readonly string WriteRoles = "identity-roles-w";
}