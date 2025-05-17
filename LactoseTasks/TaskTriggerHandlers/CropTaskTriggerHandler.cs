using System.Text.Json.Serialization;
using Lactose.Economy;
using Lactose.Events;
using Lactose.Simulation.Dtos.UserCrops;
using Lactose.Tasks.Models;
using LactoseWebApp;

namespace Lactose.Tasks.TaskTriggerHandlers;

public class CropTaskTriggerConfig : TaskHandlerConfig
{
    public required string CropId { get; set; } 
    public float ProgressMultiplier { get; set; } = 1;
}

public class CropTaskTriggerHandler(
    CropsClient cropsClient,
    UserCropsClient userCropsClient) : ITaskTriggerHandler
{
    public string Name => "crop";
    public Type ConfigType => typeof(CropTaskTriggerConfig);
    public Type EventType => typeof(UserCropInstancesEvent);

    public async Task<float> CalculateTaskProgress(Trigger trigger, UserEvent eventPayload)
    {
        var config = (CropTaskTriggerConfig)trigger.Config!;
        var cropEvent = (UserCropInstancesEvent)eventPayload;

        var userCrops = await userCropsClient.GetCropsById(new GetUserCropsByIdRequest
        {
            UserId = cropEvent.UserId,
            CropInstanceIds = cropEvent.CropInstanceIds
        });

        if (userCrops.Value is null || userCrops.Value.CropInstances.IsEmpty())
            return 0;

        float progress = config.CropId == "any"
            ? userCrops.Value.CropInstances.Count
            : userCrops.Value.CropInstances.Count(cropInst => cropInst.CropId == config.CropId);
        
        return progress * config.ProgressMultiplier;
    }
}