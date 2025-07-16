using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LactoseWebApp;

public static class CommonExtensions
{
    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
    {
        var memoryStream = stream as MemoryStream;
        
        if (memoryStream is null)
            using (memoryStream = new MemoryStream())
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

        return memoryStream.ToArray();
    }

    public static T ConstructIfNull<T>([NotNull] this T? obj) where T : new()
    {
        if (obj is null)
            obj = new T();
        
        return obj;
    }
    
    public static ConfiguredTaskAwaitable AnyContext(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable<T> AnyContext<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    static void GetTypesWithAttributeFromAssembly<T>(Assembly? assembly, List<Type> types)
    {
        if (assembly is not null)
        {
            types.AddRange(assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(T), true).Length > 0));
        }
    }
    
    static void GetTypesWithInterfaceFromAssembly<T>(Assembly? assembly, List<Type> types)
    {
        if (assembly is not null)
        {
            types.AddRange(assembly.GetTypes().Where(type => type.GetInterfaces().Any(t => t == typeof(T))));
        }
    }
    
    public static IEnumerable<Type> GetTypesWithAttribute<T>()
    {
        var typesWithAttribute = new List<Type>();
        
        GetTypesWithAttributeFromAssembly<T>(Assembly.GetEntryAssembly(), typesWithAttribute);
        GetTypesWithAttributeFromAssembly<T>(Assembly.GetCallingAssembly(), typesWithAttribute);

        return typesWithAttribute;
    }
    
    public static IEnumerable<Type> GetTypesWithInterface<T>()
    {
        var typesWithInterface = new List<Type>();
        
        GetTypesWithInterfaceFromAssembly<T>(Assembly.GetEntryAssembly(), typesWithInterface);
        GetTypesWithInterfaceFromAssembly<T>(Assembly.GetCallingAssembly(), typesWithInterface);

        return typesWithInterface;
    }
    
    public static bool IsGeneric(this Type type, Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
    public static bool IsNullable(this Type type) => true /*type.IsGenericType(typeof(Nullable<>)) - doesn't work*/;
    public static bool IsNullable(this object obj) => obj.GetType().IsNullable();
    public static bool IsCollection(this Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    public static bool IsList(this Type type) => typeof(IList).IsAssignableFrom(type);
    public static bool HasRequiredProperties(this Type type) => type.GetProperties().Any(p => p.GetCustomAttribute<RequiredAttribute>() != null);
    public static bool IsDefaultConstructable(this Type type) => type.GetConstructor(Type.EmptyTypes) != null && !type.HasRequiredProperties();
    public static bool Implements<T>(this Type type) => type.GetInterface(nameof(T)) is not null;
}