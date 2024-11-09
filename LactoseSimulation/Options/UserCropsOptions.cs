using LactoseWebApp.Options;

namespace Lactose.Simulation.Options;

[Options(SectionName = "UserCrops:Simulation")]
public class UserCropsOptions
{
    public double FertilisationToHarvestPerc { get; set; } = 0.2;
    public double FertilisationHarvestSpeedMultiplier { get; set; } = 2;
    public double MinimumSimulationDeltaSeconds { get; set; } = 1;
    public double MaxSimulationDeltaSeconds { get; set; } = TimeSpan.FromDays(2).TotalSeconds;
}