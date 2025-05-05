namespace Lactose.Economy;

public static class Permissions
{
    public static readonly string Admin = "economy-admin";
    public static readonly string Read = "economy-r";
    public static readonly string Write = "economy-w";
    public static readonly string ReadUserSelf = "economy-user-self-r";
    public static readonly string WriteUserSelf = "economy-user-self-w";
    public static readonly string ReadUserOthers = "economy-user-others-r";
    public static readonly string WriteUserOthers = "economy-user-others-w";
    public static readonly string WriteTransactions = "economy-transactions-w";
    public static readonly string ReadTransactions = "economy-transactions-r";
    public static readonly string WriteVendors = "economy-vendors-w";
}