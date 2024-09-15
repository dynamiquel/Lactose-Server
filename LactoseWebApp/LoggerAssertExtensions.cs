using System.Diagnostics.CodeAnalysis;

namespace LactoseWebApp;

public static class LoggerAssertExtensions
{
    static bool EnsureInternal(this ILogger logger, bool condition, string message)
    {
        if (!condition)
        {
            logger.LogWarning(message);
            
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        return condition;
    }
    
    public static bool Ensure(this ILogger logger, bool condition)
    {
        return logger.EnsureInternal(
            condition, 
            $"Assertion failed:\n{Environment.StackTrace}");
    }
    
    public static bool Ensure(this ILogger logger, bool condition, string message)
    {
        return logger.EnsureInternal(
            condition, 
            $"Assertion failed '{message}':\n{Environment.StackTrace}");
    }
    
    public static bool Ensure(this ILogger logger, bool condition, string message, Exception exception)
    {
        return logger.EnsureInternal(
            condition, 
            $"Assertion failed '{message}':\n{exception}");
    }
    
    public static bool Ensure<T>(this ILogger logger, [NotNullWhen(true)]T? nullableToCheck)
    {
        return logger.EnsureInternal(
            nullableToCheck is null, 
            $"Assertion failed:\n{Environment.StackTrace}");
    }
    
    public static bool Ensure<T>(this ILogger logger, [NotNullWhen(true)]T? nullableToCheck, string message)
    {
        return logger.EnsureInternal(
            nullableToCheck is not null, 
            $"Assertion failed '{message}':\n{Environment.StackTrace}");
    }
}