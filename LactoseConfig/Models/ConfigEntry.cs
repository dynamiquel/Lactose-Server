using LactoseWebApp;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Config.Models;

public class ConfigEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string? Id { get; set; }

    [BsonRequired]
    public required string Key { get; set; }
    
    [BsonRequired]
    public required string Value { get; set; }

    public ConfigEntryConditions? Conditions { get; set; } = default;
}

public record struct ConfigEntryConditions
{
    public ConfigEntryEnvironment Environment { get; set; }
    public string? Location { get; set; } = default;

    public ConfigEntryConditions()
    { }
}

public enum ConfigEntryConditionMatchType
{
    NotMatched,
    SourceUsingDefaultValue,
    Matched
}

public enum ConfigEntryEnvironment
{
    Production,
    Testing,
    Development,
    Prototype
}

public enum ConfigEntryLocationMatchType
{
    NotMatched,
    SourceUsingDefaultValue,
    MatchedContinent,
    MatchedCountry
}

public enum ConfigEntryEnvironmentMatchType
{
    NotMatched,
    SourceUsingDefaultValue,
    MatchedPartially,
    MatchedExactly
}

public static class ConfigEntryConditionsExtensions
{
    public static int Matches(this ConfigEntryConditions? source, ConfigEntryConditions? checker)
    {
        int score = 1;

        if (!source.HasValue)
            return score;

        // The Source requires checks but there is nothing to check.
        if (!checker.HasValue)
            return 0;
        
        if (!UpdateMatchScore(ref score, MatchesEnvironment(source.Value, checker.Value)))
            return 0;
        
        if (!UpdateMatchScore(ref score, MatchesLocation(source.Value, checker.Value)))
            return 0;
        
        return score;
    }

    static bool UpdateMatchScore<T>(ref int score, T matchedResult) where T: Enum
    {
        if (matchedResult.EqualsOrGreaterThan(ConfigEntryConditionMatchType.Matched))
            score += matchedResult.ToInt() - 1;
        else if (matchedResult.EqualsTo(ConfigEntryConditionMatchType.NotMatched))
            return false;

        return true;
    }
    
    static ConfigEntryLocationMatchType MatchesLocation(ConfigEntryConditions source, ConfigEntryConditions checker)
    {
        if (string.IsNullOrEmpty(source.Location))
            return ConfigEntryLocationMatchType.SourceUsingDefaultValue;

        if (source.Location == checker.Location)
            return ConfigEntryLocationMatchType.MatchedCountry;

        if (checker.Location != null && checker.Location.StartsWith(source.Location))
            return ConfigEntryLocationMatchType.MatchedContinent;

        return ConfigEntryLocationMatchType.NotMatched;
    }
    
    static ConfigEntryEnvironmentMatchType MatchesEnvironment(ConfigEntryConditions source, ConfigEntryConditions checker)
    {
        if (source.Environment == checker.Environment)
            return ConfigEntryEnvironmentMatchType.MatchedExactly;
        
        if (source.Environment < checker.Environment)
            return ConfigEntryEnvironmentMatchType.MatchedPartially;

        return ConfigEntryEnvironmentMatchType.NotMatched;
    }
}
