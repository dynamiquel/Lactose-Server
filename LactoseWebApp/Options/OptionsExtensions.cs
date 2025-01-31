using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Serilog;

namespace LactoseWebApp.Options;

/// <summary>
/// The Options system uses Reflection to auto-bind <see cref="IConfiguration"/> sections to
/// <see cref="OptionsServiceCollectionExtensions"/>.
/// 
/// Simply add the <see cref="OptionsAttribute"/> to a class containing the desired properties and it will determine
/// which part of the Configuration file it relates to.
///
/// <seealso cref="LactoseWebApp.Service.ServiceOptions"/>
/// </summary>
public static class OptionsExtensions
{
    static string GetSectionNameForOptions(Type optionsType)
    {
        var optionsAttribute = optionsType.GetCustomAttribute<OptionsAttribute>();
        if (optionsAttribute is null)
            throw new InvalidOptionsTypeException(optionsType);

        if (string.IsNullOrWhiteSpace(optionsAttribute.SectionName))
            optionsAttribute.SetSectionNameViaReflection(optionsType);
        
        return optionsAttribute.SectionName;
    }
    
    /**
     * Uses Reflection to register every Type with the Aurora Options Attribute to the normal Microsoft Options system.
     */
    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var typesWithOptionsAttribute = CommonExtensions.GetTypesWithAttribute<OptionsAttribute>();
        foreach (var type in typesWithOptionsAttribute)
        {
            // Only Classes are supported.
            if (!type.IsClass)
                continue;

            MethodInfo? desiredMethod = typeof(OptionsExtensions).GetMethod(nameof(ConfigureInternal));

            // Converts the desired method into a generic one using the provided type as the generic parameter.
            MethodInfo? genericMethod = desiredMethod?.MakeGenericMethod(type);
            if (genericMethod is null)
                throw new NullReferenceException("Generic Configure Method is null");

            var configSectionName = GetSectionNameForOptions(type);
            IConfigurationSection configSection = configuration.GetSection(configSectionName);
            if (string.IsNullOrEmpty(configSection.Key))
                throw new Exception($"Configuration Section {configSectionName} is not valid for {nameof(type)}");
            
            genericMethod.Invoke(null, new object[] { services, configSection });
            Log.Information("Added Options for {Type}", type);
        }

        return services;
    }
    
   /**
    * Forwards to Microsoft's Options Configure. Required as the original method ends up being ambiguous for the
    * reflection-based MakeGenericMethod.
    */
    public static IServiceCollection ConfigureInternal<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this IServiceCollection services, 
        IConfiguration config) where TOptions : class
            => services.Configure<TOptions>(string.Empty, config);

   static void SetOptionFieldFromConfigDirect<T>(IConfigurationSection configurationSection, PropertyInfo property, T options)
   {
       object? value = configurationSection.GetValue(property.PropertyType, property.Name);
       if (value is null) 
           return;
       
       property.SetValue(options, value);
   }
   
   static void SetOptionFieldFromConfigArray<T>(IConfigurationSection configurationSection, PropertyInfo property, T options)
   {
       if (!property.PropertyType.IsArray)
           throw new InvalidOperationException($"The provided Type '{property.PropertyType}' is not an Array type");

       var elementType = property.PropertyType.GetElementType();
       if (elementType is null)
           throw new NullReferenceException("Property Element Type is null");
       
       IConfigurationSection collectionSection = configurationSection.GetSection(property.Name);
       
       if (string.IsNullOrEmpty(collectionSection.Key) && property.GetCustomAttribute<RequiredMemberAttribute>() is not null)
           throw new RequiredOptionsFieldNotFoundException<T>(property);

       var tempList = new ArrayList();
       foreach (var element in collectionSection.GetChildren())
       {
           object? elementValue = collectionSection.GetValue(elementType, element.Key);
           if (elementValue is not null)
               tempList.Add(elementValue);
       }
       
       property.SetValue(options, tempList.ToArray(elementType));
   }

   static T CreateOptionsFromConfigSection<T>(IConfigurationSection configurationSection) where T : new()
   {
       var options = new T();
       
       foreach (var property in typeof(T).GetProperties())
       {
           if (property.PropertyType.IsArray)
               SetOptionFieldFromConfigArray(configurationSection, property, options);
           else
               SetOptionFieldFromConfigDirect(configurationSection, property, options);
       }

       return options;
   }

   public static T? TryGetOptions<T>(this IConfiguration configuration) where T : new()
   {
       var configSectionName = GetSectionNameForOptions(typeof(T));
       IConfigurationSection configSection = configuration.GetSection(configSectionName);
       return string.IsNullOrEmpty(configSection.Key) ? default : CreateOptionsFromConfigSection<T>(configSection);
   }
   
   public static T GetOptions<T>(this IConfiguration configuration) where T : new()
   {
       T? options = TryGetOptions<T>(configuration);
       if (options is null)
           throw new OptionsNotFoundException<T>();

       return options;
   }
   
   public class InvalidOptionsTypeException : Exception
   {
       public InvalidOptionsTypeException(Type type)
           : base($"{type} is not a valid Options type as it missing the Options attribute")
       { }
   }
   
   public class InvalidOptionsTypeException<T> : InvalidOptionsTypeException
   {
       public InvalidOptionsTypeException()
           : base(typeof(T))
       { }
   }
   
   public class OptionsNotFoundException<T> : Exception
   {
       public OptionsNotFoundException()
           : base($"Could not find Options for {typeof(T)}. Looked for Section '{GetSectionNameForOptions(typeof(T))}'")
       { }
   }
   
   public class RequiredOptionsFieldNotFoundException<T> : Exception
   {
       public RequiredOptionsFieldNotFoundException(MemberInfo memberInfo)
           : base($"Could not find value for required Options field {typeof(T)}.{memberInfo.Name}")
       { }
   }
   
   /**
    * If the string is a file, return the contents of the file, otherwise, return the string.
    * Useful for options containing secrets.
    */
   public static string? GetRawOrFileString(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        
        if (raw.Contains('/'))
            return File.ReadAllText(raw);
        
        return raw;
    }
}