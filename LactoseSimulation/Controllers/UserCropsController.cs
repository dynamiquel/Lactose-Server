using Lactose.Economy.Transactions;
using Lactose.Economy.UserItems;
using Lactose.Simulation.Data.Repos;
using Lactose.Simulation.Dtos.UserCrops;
using Lactose.Simulation.Mapping;
using Lactose.Simulation.Metrics;
using Lactose.Simulation.Models;
using Lactose.Simulation.Options;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Lactose.Simulation.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCropsController(
    ILogger<CropsController> logger,
    IUserCropsRepo userCropsRepo,
    ICropsRepo cropsRepo,
    IOptions<UserCropsOptions> options,
    TransactionsClient transactionsClient,
    IMqttClient mqttClient) : ControllerBase, IUserCropsController
{
    [Authorize]
    [HttpPost(Name = "Get User Crops")]
    public async Task<ActionResult<GetUserCropsResponse>> GetCrops(GetUserCropsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanReadThisUserCrops(request))
            return Unauthorized("You cannot view crops for this user");

        UserCropInstances? userCrops = await userCropsRepo.Get(request.UserId);

        if (userCrops is null)
        {
            var newUserCrops = new UserCropInstances()
            {
                Id = request.UserId,
                CropInstances = new List<CropInstance>()
            };

            userCrops = await userCropsRepo.Set(newUserCrops);
            if (userCrops is null)
                return StatusCode(500, $"Could not create User Crops for User with ID '{request.UserId}'");
        }
        
        return Ok(UserCropsMapper.ToDto(userCrops));
    }

    [Authorize]
    [HttpPost("byid", Name = "Get User Crops by ID")]
    public async Task<ActionResult<GetUserCropsResponse>> GetCropsById(GetUserCropsByIdRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanReadThisUserCrops(request))
            return Unauthorized("You cannot view crops for this user");
        
        List<CropInstance> foundCropInstances = await userCropsRepo.GetUserCropsById(request.UserId, request.CropInstanceIds);
        return Ok(new GetUserCropsResponse
        {
            CropInstances = foundCropInstances
        });
    }

    [HttpPost("simulate", Name = "Simulate User Crops")]
    public async Task<ActionResult<SimulateUserCropsResponse>> SimulateCrops(SimulateUserCropsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        UserCropInstances? userCrops = await userCropsRepo.Get(request.UserId);
        if (userCrops is null)
            return BadRequest($"No User could be found with ID '{request.UserId}'");
        
        var timeNow = DateTime.UtcNow;
        var previousSimulationTime = userCrops.PreviousSimulationTime;
        var secondsDelta = (DateTime.UtcNow - previousSimulationTime).TotalSeconds;

        if (secondsDelta < options.Value.MinimumSimulationDeltaSeconds)
            return StatusCode(StatusCodes.Status429TooManyRequests, $"The most recent simulation happened too recently. It must be at least {options.Value.MinimumSimulationDeltaSeconds} seconds but received {secondsDelta} seconds");

        secondsDelta = Math.Min(secondsDelta, options.Value.MaxSimulationDeltaSeconds);

        foreach (CropInstance userCrop in userCrops.CropInstances)
        {
            if (userCrop.State != CropInstanceStates.Growing)
                continue;
            
            double currentHarvestSeconds = userCrop.RemainingHarvestSeconds;
            
            // Decrease the remaining Harvest time by the delta time while honouring the effects of Fertilisation.
            // I feel like there's a better formula for this...
            double fertiliseSeconds = Math.Min(userCrop.RemainingFertiliserSeconds, secondsDelta);
            double nonFertiliseSeconds = secondsDelta - fertiliseSeconds;
            double secondsToRemove = nonFertiliseSeconds + fertiliseSeconds * options.Value.FertilisationHarvestSpeedMultiplier;

            userCrop.RemainingFertiliserSeconds = Math.Max(userCrop.RemainingFertiliserSeconds - secondsDelta, 0);
            userCrop.RemainingHarvestSeconds = Math.Max(userCrop.RemainingHarvestSeconds - secondsToRemove, 0);
            
            if (userCrop.RemainingHarvestSeconds <= 0)
            {
                userCrop.State = CropInstanceStates.Harvestable;
            }
            
            logger.LogInformation("Simulated User Crop with ID {UserCropId}. From {CurrentHarvestSeconds} Harvest seconds to {UserCropRemainingHarvestSeconds} Harvest seconds", 
                userCrop.Id, currentHarvestSeconds, userCrop.RemainingHarvestSeconds);
            
            SimulationMetrics.CropsSimulatedCounter.Add(1, new KeyValuePair<string, object?>("cropId", userCrop.CropId));
        }

        userCrops.PreviousSimulationTime = timeNow;
        userCrops = await userCropsRepo.Set(userCrops);

        if (userCrops is null)
            return StatusCode(StatusCodes.Status500InternalServerError, $"Could not update Crops for User '{request.UserId}'");
        
        logger.LogInformation("Simulated User Crops for User with ID {RequestUserId} by {SecondsDelta} seconds", 
            request.UserId, secondsDelta);
        
        return Ok(new SimulateUserCropsResponse
        {
            NewSimulationTime = userCrops.PreviousSimulationTime,
            PreviousSimulationTime = previousSimulationTime
        });
    }

    [Authorize]
    [HttpPost("create", Name = "Create User Crop")]
    public async Task<ActionResult<CreateUserCropResponse>> CreateCrop(CreateUserCropRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanWriteThisUserCrops(request))
            return Unauthorized("You cannot create crops for this user");
        
        Crop? crop;
        if (string.IsNullOrEmpty(request.CropId))
        {
            // Empty crop ID means it is an empty plot.
            crop = null;
        }
        else
        {
            crop = await cropsRepo.Get(request.CropId);
            if (crop is null)
                return BadRequest($"No Crop could be found with ID: {request.CropId}");

            var tradeRequest = new TradeRequest
            {
                UserA = new UserTradeRequest
                {
                    UserId = request.UserId,
                    Items = crop.CostItems
                },
                UserB = new UserTradeRequest { Items = [] }
            };
            
            var tradeResponse = await transactionsClient.Trade(tradeRequest);
            if (tradeResponse.Reason != "Success")
                return BadRequest($"Could not remove Cost Items from User '{request.UserId}' for Crop '{crop.Id}'");
        }

        var userCrops = await userCropsRepo.Get(request.UserId) ?? new UserCropInstances
        {
            Id = request.UserId,
            PreviousSimulationTime = DateTime.UtcNow
        };
        
        var cropInstance = new CropInstance
        {
            Id = Guid.NewGuid().ToString(),
            CropId = request.CropId,
            Location = request.CropLocation,
            Rotation = request.CropRotation,
            State = crop is not null ? CropInstanceStates.Growing : CropInstanceStates.Empty,
            RemainingHarvestSeconds = crop?.HarvestSeconds ?? 0
        };
        
        userCrops.CropInstances.Add(cropInstance);

        userCrops = await userCropsRepo.Set(userCrops);
        if (userCrops is null)
            return StatusCode(StatusCodes.Status500InternalServerError, $"Could not create Crop '{request.CropId}' for User '{request.UserId}'");
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/simulation/usercrops/{request.UserId}/created")
            .WithPayload(new UserCropInstancesEvent
            {
                UserId = request.UserId,
                CropInstanceIds = [cropInstance.Id]
            }.ToJson())
            .Build());
        
        SimulationMetrics.CropCreatedCounter.Add(1, new KeyValuePair<string, object?>("cropName", crop?.Name));

        return Ok(new CreateUserCropResponse
        {
            UserCropInstanceId = cropInstance.Id
        });
    }

    [Authorize]
    [HttpPost("harvest", Name = "Harvest User Crops")]
    public async Task<ActionResult<HarvestUserCropsResponse>> HarvestCrops(HarvestUserCropsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanWriteThisUserCrops(request))
            return Unauthorized("You cannot harvest crops for this user");

        UserCropInstances? userCrops = await userCropsRepo.Get(request.UserId);
        if (userCrops is null)
            return BadRequest($"No User could be found with ID '{request.UserId}'");

        ICollection<string> cropIds = userCrops.CropInstances.Select(cropInstance => cropInstance.CropId).ToList();
        ICollection<Crop> crops = await cropsRepo.Get(cropIds);

        List<string> harvestedCropInstanceIds = [];

        foreach (string requestedCropInstanceId in request.CropInstanceIds)
        {
            var foundCropInstance =
                userCrops.CropInstances.FirstOrDefault(cropInstance => cropInstance.Id == requestedCropInstanceId);

            if (foundCropInstance is null)
            {
                logger.LogWarning($"User wanted to harvest a User Crop with ID '{requestedCropInstanceId}' but it doesn't exist");
                continue;
            }
            
            Crop? crop = crops.FirstOrDefault(crop => crop.Id == foundCropInstance.CropId);
            if (crop is null)
            {
                logger.LogWarning($"User wanted to harvest User Crop with ID '{requestedCropInstanceId}' but no Crop exists with ID '{foundCropInstance.CropId}'");
                continue;
            }

            if (foundCropInstance.State != CropInstanceStates.Harvestable)
            {
                logger.LogWarning($"User wanted to harvest User Crop with ID '{requestedCropInstanceId}' but it is not Harvestable");
                continue;
            }

            foundCropInstance.State = crop.Type == CropTypes.Plot ? CropInstanceStates.Empty : CropInstanceStates.Growing;
            foundCropInstance.RemainingHarvestSeconds = crop.Type == CropTypes.Plot ? crop.HarvestSeconds : crop.HarvestSeconds / 2;
            
            var tradeRequest = new TradeRequest
            {
                UserA = new UserTradeRequest
                {
                    Items = crop.HarvestItems
                },
                UserB = new UserTradeRequest
                {
                    UserId = request.UserId,
                    Items = []
                }
            };

            var tradeResponse = await transactionsClient.Trade(tradeRequest);
            if (tradeResponse.Reason != "Success")
            {
                logger.LogError("Could not transfer Harvest Items from Crop '{CropId}' to User '{RequestUserId}'", crop.Id, request.UserId);
                continue;
            }
            
            harvestedCropInstanceIds.Add(requestedCropInstanceId);
            logger.LogInformation("User has harvested User Crop with ID '{RequestedCropInstanceId}'. Next harvest in {RemainingHarvestSeconds} seconds", requestedCropInstanceId, foundCropInstance.RemainingHarvestSeconds);
            
            SimulationMetrics.CropHarvestedCounter.Add(1, new KeyValuePair<string, object?>("cropName", crop.Name));
        }

        if (!harvestedCropInstanceIds.IsEmpty())
        {
            userCrops = await userCropsRepo.Set(userCrops);
            
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"/simulation/usercrops/{request.UserId}/harvested")
                .WithPayload(new UserCropInstancesEvent
                {
                    UserId = request.UserId, 
                    CropInstanceIds = harvestedCropInstanceIds
                }.ToJson())
                .Build());
        }

        return Ok(new HarvestUserCropsResponse
        {
            HarvestedCropInstanceIds = harvestedCropInstanceIds
        });
    }

    [Authorize]
    [HttpPost("destroy", Name = "Destroy User Crops")]
    public async Task<ActionResult<DestroyUserCropsResponse>> DestroyCrops(DestroyUserCropsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanWriteThisUserCrops(request))
            return Unauthorized("You cannot destroy crops for this user");
        
        UserCropInstances? userCrops = await userCropsRepo.Get(request.UserId);
        if (userCrops is null)
            return BadRequest($"No User could be found with ID '{request.UserId}'");

        ICollection<string> cropIds = userCrops.CropInstances.Select(cropInstance => cropInstance.CropId).ToList();
        ICollection<Crop> crops = await cropsRepo.Get(cropIds);

        List<string> destroyedCropInstanceIds = [];

        foreach (string requestedCropInstanceId in request.CropInstanceIds)
        {
            var foundCropInstance =
                userCrops.CropInstances.FirstOrDefault(cropInstance => cropInstance.Id == requestedCropInstanceId);

            if (foundCropInstance is null)
            {
                logger.LogWarning("User wanted to destroy a User Crop with ID '{RequestedCropInstanceId}' but it doesn't exist", requestedCropInstanceId);
                continue;
            }
            
            destroyedCropInstanceIds.Add(requestedCropInstanceId);
            userCrops.CropInstances.Remove(foundCropInstance);
            logger.LogInformation("User has destroyed User Crop with ID '{RequestedCropInstanceId}'", requestedCropInstanceId);
            
            Crop? crop = crops.FirstOrDefault(crop => crop.Id == foundCropInstance.CropId);
            
            SimulationMetrics.CropsDestroyedCounter.Add(1, new KeyValuePair<string, object?>("cropName", crop?.Name));
            
            if (/* bCropHasDestroyItems = */ crop is not null && !crop.DestroyItems.IsEmpty())
            {
                var tradeRequest = new TradeRequest
                {
                    UserA = new UserTradeRequest
                    {
                        Items = crop!.DestroyItems!
                    },
                    UserB = new UserTradeRequest
                    {
                        UserId = request.UserId,
                        Items = []
                    }
                };
            
                var tradeResponse = await transactionsClient.Trade(tradeRequest);
                if (tradeResponse.Reason != "Success")
                {
                    logger.LogError("Could not transfer Destroy Items from Crop '{CropId}' to User '{RequestUserId}'", crop.Id, request.UserId);
                }
            }
        }

        if (!destroyedCropInstanceIds.IsEmpty())
        {
            userCrops = await userCropsRepo.Set(userCrops);
            
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"/simulation/usercrops/{request.UserId}/destroyed")
                .WithPayload(new UserCropInstancesEvent
                {
                    UserId = request.UserId, 
                    CropInstanceIds = destroyedCropInstanceIds 
                }.ToJson())
                .Build());
        }

        return Ok(new DestroyUserCropsResponse
        {
            DestroyedCropInstanceIds = destroyedCropInstanceIds
        });
    }

    [Authorize]
    [HttpPost("fertilise", Name = "Fertilise User Crops")]
    public async Task<ActionResult<FertiliseUserCropsResponse>> FertiliseCrops(FertiliseUserCropsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanWriteThisUserCrops(request))
            return Unauthorized("You cannot fertilise crops for this user");

        UserCropInstances? userCrops = await userCropsRepo.Get(request.UserId);
        if (userCrops is null)
            return BadRequest($"No User could be found with ID '{request.UserId}'");

        ICollection<string> cropIds = userCrops.CropInstances.Select(cropInstance => cropInstance.CropId).ToList();
        ICollection<Crop> crops = await cropsRepo.Get(cropIds);

        List<string> fertilisedCropInstanceIds = [];

        foreach (string requestedCropInstanceId in request.CropInstanceIds)
        {
            var foundCropInstance =
                userCrops.CropInstances.FirstOrDefault(cropInstance => cropInstance.Id == requestedCropInstanceId);

            if (foundCropInstance is null)
            {
                logger.LogWarning("User wanted to fertilise a User Crop with ID '{RequestedCropInstanceId}' but it doesn't exist", requestedCropInstanceId);
                continue;
            }
            
            Crop? crop = crops.FirstOrDefault(crop => crop.Id == foundCropInstance.CropId);
            if (crop is null)
            {
                logger.LogWarning("User wanted to fertilise User Crop with ID '{RequestedCropInstanceId}' but no Crop exists with ID '{CropId}'", requestedCropInstanceId, foundCropInstance.CropId);
                continue;
            }

            if (foundCropInstance.State != CropInstanceStates.Growing)
            {
                logger.LogWarning("User wanted to fertilise User Crop with ID '{RequestedCropInstanceId}' but it is not Growing", requestedCropInstanceId);
                continue;
            }

            if (string.IsNullOrEmpty(crop.FertiliserItemId))
            {
                logger.LogWarning("User wanted to fertilise User Crop but Crop '{CropId}' does not have a Fertiliser", foundCropInstance.CropId);
                continue;
            }

            // Max Fertilise time should not exceed the remaining Harvest time / harvest speed multiplier.
            // i.e.
            // harvest speed multiplier = 2.
            // harvest time = 100.
            // max fertiliser time = harvest time / harvest speed multiplier = 50.
            
            double maxFertiliserSeconds = foundCropInstance.RemainingHarvestSeconds / Math.Max(options.Value.FertilisationHarvestSpeedMultiplier, 1);
            double currentFertiliserSeconds = foundCropInstance.RemainingFertiliserSeconds;
            if (currentFertiliserSeconds >= maxFertiliserSeconds)
            {
                logger.LogWarning("User wanted to fertilise User Crop with ID '{RequestedCropInstanceId}' but has already reached max fertilisation", requestedCropInstanceId);
                continue;
            }
            
            var tradeRequest = new TradeRequest
            {
                UserA = new UserTradeRequest
                {
                    UserId = request.UserId,
                    Items = [new() { ItemId = crop.FertiliserItemId }]
                },
                UserB = new UserTradeRequest
                {
                    Items = []
                }
            };
            
            var tradeResponse = await transactionsClient.Trade(tradeRequest);
            if (tradeResponse.Reason != "Success")
            {
                logger.LogError("Could not remove Fertilise Items from User '{RequestUserId}' for Crop '{CropId}'", request.UserId, crop.Id);
                continue;
            }

            var fertiliseSeconds = crop.HarvestSeconds * options.Value.FertilisationToHarvestPerc;
            
            foundCropInstance.RemainingFertiliserSeconds = Math.Min(
                foundCropInstance.RemainingFertiliserSeconds + fertiliseSeconds, 
                maxFertiliserSeconds);
            
            fertilisedCropInstanceIds.Add(requestedCropInstanceId);
            logger.LogInformation("User has fertilised User Crop with ID '{RequestedCropInstanceId}'. From {CurrentFertiliserSeconds} seconds to {RemainingFertiliserSeconds} seconds", requestedCropInstanceId, currentFertiliserSeconds, foundCropInstance.RemainingFertiliserSeconds);
            SimulationMetrics.CropsFertilisedCounter.Add(1, new KeyValuePair<string, object?>("cropName", crop.Name));
        }

        if (!fertilisedCropInstanceIds.IsEmpty())
        {
            userCrops = await userCropsRepo.Set(userCrops);
            
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"/simulation/usercrops/{request.UserId}/fertilised")
                .WithPayload(new UserCropInstancesEvent
                {
                    UserId = request.UserId, 
                    CropInstanceIds = fertilisedCropInstanceIds
                }.ToJson())
                .Build());
        }

        return Ok(new FertiliseUserCropsResponse
        {
            FertilisedCropInstanceIds = fertilisedCropInstanceIds
        });
    }

    [Authorize]
    [HttpPost("seed", Name = "Seed User Crops")]
    public async Task<ActionResult<SeedUserCropsResponse>> SeedCrop(SeedUserCropRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"'{request.UserId}' is not a valid User ID");
        
        if (!CanWriteThisUserCrops(request))
            return Unauthorized("You cannot seed crops for this user");

        UserCropInstances? userCrops = await userCropsRepo.Get(request.UserId);
        if (userCrops is null)
            return BadRequest($"No User could be found with ID '{request.UserId}'");

        Crop? crop = await cropsRepo.Get(request.CropId);
        if (crop is null)
            return BadRequest($"No Crop could be found with ID: {request.CropId}");

        List<string> seededCropInstanceIds = [];

        foreach (string requestedCropInstanceId in request.CropInstanceIds)
        {
            var foundCropInstance =
                userCrops.CropInstances.FirstOrDefault(cropInstance => cropInstance.Id == requestedCropInstanceId);

            if (foundCropInstance is null)
            {
                logger.LogWarning("User wanted to seed a User Crop with ID '{RequestedCropInstanceId}' but it doesn't exist", requestedCropInstanceId);
                continue;
            }

            if (foundCropInstance.State != CropInstanceStates.Empty)
            {
                logger.LogWarning("User wanted to seed a User Crop with ID '{RequestedCropInstanceId}' but it is unavailable to seed", requestedCropInstanceId);
                continue;
            }

            // If there's an existing Crop, ensure it is a Plot type.
            // This should never really happen as only Plots should be able to have an Empty state.
            if (!string.IsNullOrEmpty(foundCropInstance.CropId))
            {
                Crop? existingCrop = await cropsRepo.Get(foundCropInstance.CropId);
                if (existingCrop?.Type != CropTypes.Plot)
                {
                    logger.LogError("Attempted to seed an empty Crop but it is not a Plot. Crop Instance '{RequestedCropInstanceId}, Existing Crop: {CropId}", requestedCropInstanceId, foundCropInstance.CropId);
                    continue;
                }
            }
            
            var tradeRequest = new TradeRequest
            {
                UserA = new UserTradeRequest
                {
                    UserId = request.UserId,
                    Items = crop.CostItems
                },
                UserB = new UserTradeRequest
                {
                    Items = []
                }
            };

            var tradeResponse = await transactionsClient.Trade(tradeRequest);
            if (tradeResponse.Reason != "Success")
            {
                logger.LogError("Could not remove Cost Items from User '{RequestUserId}' for Crop '{CropId}'", request.UserId, crop.Id);
                continue;
            }
            
            // Plant the seed and change the Crop info.
            foundCropInstance.CropId = request.CropId;
            foundCropInstance.State = CropInstanceStates.Growing;
            foundCropInstance.RemainingHarvestSeconds = crop.HarvestSeconds;

            seededCropInstanceIds.Add(requestedCropInstanceId);
            
            SimulationMetrics.CropsSeededCounter.Add(1, new KeyValuePair<string, object?>("cropName", crop.Name));
        }
        
        userCrops = await userCropsRepo.Set(userCrops);
        if (userCrops is null)
            return StatusCode(StatusCodes.Status500InternalServerError, $"Could not seed Crop '{request.CropId}' for User '{request.UserId}'");
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/simulation/usercrops/{request.UserId}/seeded")
            .WithPayload(new UserCropInstancesEvent
            {
                UserId = request.UserId, 
                CropInstanceIds = seededCropInstanceIds
            }.ToJson())
            .Build());

        return Ok(new SeedUserCropsResponse
        {
            SeededCropInstanceIds = seededCropInstanceIds
        });
    }
    
    bool CanWriteThisUserCrops(UserRequest request)
    {
        return User.MatchesId(request.UserId) && User.HasBoolClaim(Permissions.WriteSelf) ||
               User.HasBoolClaim(Permissions.WriteOthers);
    }

    bool CanReadThisUserCrops(UserRequest request)
    {
        return User.MatchesId(request.UserId) && User.HasBoolClaim(Permissions.ReadSelf) ||
               User.HasBoolClaim(Permissions.ReadOthers);
    }
}