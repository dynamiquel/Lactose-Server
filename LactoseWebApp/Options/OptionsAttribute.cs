using System.Text;

namespace LactoseWebApp.Options;

[AttributeUsage(AttributeTargets.Class)]
public class OptionsAttribute : Attribute
{
    string _sectionName = string.Empty;
    
    const char Separator = ':';
    const int MaximumExpectedCapitals = 8;

    public string SectionName
    {
        get => _sectionName;
        set
        {
            var sb = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                bool bReplaceSeparator = character.Equals('/') || character.Equals('.') || character.Equals(' ');
                // Service.Options or Service/Options becomes Service:Options.
                sb.Append(bReplaceSeparator ? Separator : character);
            }

            // Trim any Options suffix. I.e. Service:Options -> Service.
            sb.TrimFromEnd(":Options");
            _sectionName = sb.ToString();
        }
    }

    public void SetSectionNameViaReflection(Type type)
    {
        string name = type.Name;
        
        // Replace capitals with spaces.
        var sb = new StringBuilder(name.Length + MaximumExpectedCapitals);
        char previousCharacter = '\0';
        foreach (var character in name)
        {
            // ServiceOptions becomes Service:Options
            bool bReplaceCapitalWithSeparator = char.IsUpper(character) && !previousCharacter.Equals('\0') && !previousCharacter.Equals(Separator);
            if (bReplaceCapitalWithSeparator)
                sb.Append(Separator);
            
            sb.Append(character);
            previousCharacter = sb[^1];
        }

        var newName = sb.ToString();
        SectionName = newName;
    }
}