using System.Diagnostics;
using System.Reflection;

namespace LactoseWebApp.Service;

public enum OnlineStatus
{
    Offline,
    Starting,
    Ending,
    Online
}

public interface IServiceInfo
{
    string Name { get; init; }
    
    string Description { get; init; }
    
    string[] Dependencies { get; init; }
    
    Version Version { get; init; }
    
    DateTime BuildTime { get; init; }
    
    public OnlineStatus Status { get; set; }
    
    public string Hostname { get; }
    
    string Runtime { get; }
    
    string OperatingSystem { get; }
    
    DateTime StartTime { get; init; }
    
    TimeSpan Uptime { get; }

    string Id => Name.ToLower().Replace(' ', '-');
}

public class ServiceInfo : IServiceInfo
{
    public string Name { get; init; }
    
    public string Description { get; init; }

    public string[] Dependencies { get; init; }

    public Version Version { get; init; } 

    public DateTime BuildTime { get; init; }

    public OnlineStatus Status { get; set; }

    public string Hostname => Environment.MachineName;
    
    public string Runtime =>
        System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

    public string OperatingSystem =>
        System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
    
    public DateTime StartTime { get; init; }
    
    public TimeSpan Uptime => DateTime.UtcNow - StartTime;

    
    static readonly Version DefaultVersion = new (0, 1);

    public ServiceInfo()
    {
        Name = "Unnamed Service";
        Description = string.Empty;
        Dependencies = [];
        StartTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
        
        // Entry Assembly is the the actual assembly of the application and not the assembly of this library.
        var appAssembly = Assembly.GetEntryAssembly();
        
        if (appAssembly is null)
        {
            BuildTime = DateTime.MinValue;
            Version = DefaultVersion;
        }
        else
        {
            Version = appAssembly.GetName().Version ?? DefaultVersion;
            
            // Try get the build time from the assembly file.
            try
            {
                var appAssemblyFileInfo = new FileInfo(appAssembly.Location);
                BuildTime = appAssemblyFileInfo.LastWriteTimeUtc;
            }
            catch
            {
                BuildTime = DateTime.MinValue;
            }
        }
    }
}

internal class ServiceInfoNotFoundException : Exception
{
    internal ServiceInfoNotFoundException() : base("Unable to retrieve Service Info") { }
}