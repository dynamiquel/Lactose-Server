namespace LactoseWebApp;

public static class EnumExtensions
{
    public static int ToInt<T>(this T value) where T : Enum
    {
        return Convert.ToInt32(value);
    }
    
    public static bool EqualsTo<T, TU>(this T lhs, TU rhs)
        where T : Enum
        where TU : Enum
    {
        return lhs.ToInt() == rhs.ToInt();
    }

    public static bool GreaterThan<T, TU>(this T lhs, TU rhs)
        where T : Enum
        where TU : Enum
    {
        return lhs.ToInt() > rhs.ToInt();
    }

    public static bool EqualsOrGreaterThan<T, TU>(this T lhs, TU rhs) 
        where T : Enum
        where TU : Enum
    {
        return lhs.ToInt() >= rhs.ToInt();
    }
}