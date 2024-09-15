using System.Text.Json;

namespace LactoseWebApp;

public static class JsonExtensions
{
    readonly static JsonSerializerOptions IndentedSerializerOptions = new()
    {
        WriteIndented = true
    };
    
    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj);
    }
    
    public static string ToIndentedJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, IndentedSerializerOptions);
    }
}