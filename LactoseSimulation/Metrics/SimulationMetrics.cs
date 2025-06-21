using System.Diagnostics.Metrics;

namespace Lactose.Simulation.Metrics;

public static class SimulationMetrics
{
    public const string MeterName = "Lactose.Simulation";
    
    static readonly Meter Meter = new(MeterName, "1.0.0");
    
    public static Counter<long> CropHarvestedCounter { get; } = 
        Meter.CreateCounter<long>("simulation.crops.harvested.total", description: "Total number of crop harvests.");
    
    public static Counter<long> CropCreatedCounter { get; } = 
        Meter.CreateCounter<long>("simulation.crops.created.total", description: "Total number of crops created.");
    
    public static Counter<long> CropsSeededCounter { get; } = 
        Meter.CreateCounter<long>("simulation.crops.seeded.total", description: "Total number of crops seeded.");
    
    public static Counter<long> CropsDestroyedCounter { get; } = 
        Meter.CreateCounter<long>("simulation.crops.destroyed.total", description: "Total number of crops destroyed.");
    
    public static Counter<long> CropsFertilisedCounter { get; } = 
        Meter.CreateCounter<long>("simulation.crops.fertilised.total", description: "Total number of crops fertilised.");
    
    
    public static Counter<long> CropsSimulatedCounter { get; } = 
        Meter.CreateCounter<long>("simulation.crops.simulated.total", description: "Total number of crops simulated.");
}