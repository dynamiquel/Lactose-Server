using System.Text;

namespace LactoseWebApp;

public static class StringExtensions
{
    /// <summary>
    /// Returns whether this string ends with the given string.
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool EndsWith(this StringBuilder stringBuilder, string value)
    {
        if (stringBuilder.Length < value.Length)
            return false;

        for (var i = 0; i < value.Length; i++)
        {
            var fromEnd = stringBuilder.Length - value.Length + i;
            if (stringBuilder[fromEnd] != value[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// If the string ends with the specified string, removes it.
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TrimFromEnd(this StringBuilder stringBuilder, string value)
    {
        if (stringBuilder.EndsWith(value))
        {
            stringBuilder.Remove(stringBuilder.Length - value.Length, value.Length);
            return true;
        }

        return false;
    }
    
    public static string CombineUrlWithPort(string addressWithOptionalPath, int port)
    {
        if (string.IsNullOrEmpty(addressWithOptionalPath))
        {
            return string.Empty;
        }

        var parts = addressWithOptionalPath.Split('/', 2); // Split into hostname/domain and the rest of the path (max 2 parts)
        string hostname = parts[0];
        string path = parts.Length > 1 ? "/" + parts[1] : ""; // Add the leading slash back if a path exists

        return $"{hostname}:{port}{path}";
    }
}