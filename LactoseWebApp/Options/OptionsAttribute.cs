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
            var sb = new StringBuilder(value.Length + MaximumExpectedCapitals);
            char previousCharacter = '\0';
            foreach (var character in value)
            {
                if (character.Equals('/') || character.Equals('.') || character.Equals(' '))
                {
                    // Service.Options or Service/Options becomes Service:Options.
                    sb.Append(Separator);
                }
                else
                {
                    if (char.IsUpper(character) && !previousCharacter.Equals('\0') && !previousCharacter.Equals(Separator))
                    {
                        // ServiceOptions becomes Service:Options
                        sb.Append(Separator);
                    }

                    sb.Append(character);
                }
                
                previousCharacter = sb[^1];
            }

            // Trim any Options suffix. I.e. Service:Options -> Service.
            sb.TrimFromEnd(":Options");
            _sectionName = sb.ToString();
        }
    }
}