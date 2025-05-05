using System.Text;
using Lactose.Economy;
using Lactose.Economy.Dtos.Transactions;
using Lactose.Events;
using Lactose.Tasks;
using Lactose.Tasks.Data;
using Lactose.Tasks.Models;
using Lactose.Tasks.TaskTriggerHandlers;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Mqtt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Formatter;
using Task = System.Threading.Tasks.Task;

namespace LactoseTasks.Services;

public class UserTaskTracker(
    ILogger<UserTaskTracker> logger,
    IMqttClient mqttClient,
    IOptions<MqttOptions> options,
    MqttClientFactory mqttFactory,
    MongoTasksRepo tasksRepo,
    MongoUserTasksRepo userTasksRepo,
    TaskTriggerHandlerRegistry triggerHandlerRegistry,
    TransactionsClient transactionsClient,
    IApiAuthHandler authHandler) : IHostedService
{
    private IMqttClient _mqttClient = null!; // Should never be null. I just cba doing the proper constructor.
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Wait until authenticated and notify the transactions' client of it.
        // This is so fucking hacky. Really need to look into dealing with client-based
        // auth and DI dependencies.
        while (authHandler.AccessToken is null)
            await Task.Delay(100, cancellationToken);
        transactionsClient.SetAuthToken(authHandler.AccessToken.UnsafeToString());
        
        _mqttClient = mqttFactory.CreateMqttClient();

        var clientOptions = new MqttClientOptionsBuilder()
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithTlsOptions(
                o => o.WithCertificateValidationHandler(
                    // The used public broker sometimes has invalid certificates. This sample accepts all
                    // certificates. This should not be used in live environments.
                    _ => true));

        logger.LogInformation("Attempting to connect to MQTT broker at {IpAddress}:{IpPort}",
            options.Value.ServerAddress,
            options.Value.ServerPort);

        if (options.Value.WebSockets)
        {
            string url = StringExtensions.CombineUrlWithPort(options.Value.ServerAddress, options.Value.ServerPort);
            logger.LogInformation("MQTT WebSocket URL: {WebSocketUri}", url);
            clientOptions.WithWebSocketServer(c => c.WithUri($"wss://{url}"));
        }
        else
        {
            clientOptions.WithTcpServer(options.Value.ServerAddress, options.Value.ServerPort);
        }
        
        MqttClientConnectResult? result = await _mqttClient.ConnectAsync(clientOptions.Build(), cancellationToken);
        if (result?.ResultCode != MqttClientConnectResultCode.Success)
        {
            if (result is not null)
                logger.LogError("Failed to connect to MQTT broker. Reason: {ReasonCode} - {ReasonStr}",
                    result.ResultCode,
                    result.ReasonString);
            else
                logger.LogError("Failed to connect to MQTT broker");

            return;
        }

        await SubscribeToTopics(cancellationToken);
        _mqttClient.ApplicationMessageReceivedAsync += OnTopicMessageReceived;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.TryDisconnectAsync();
    }

    async Task SubscribeToTopics(CancellationToken cancellationToken)
    {
        // Go through every Task and subscribe to the necessary events based on its triggers.

        HashSet<string> allTopics = await tasksRepo.GetAllTriggerTopics();

        MqttClientSubscribeOptionsBuilder subscribeOptionsBuilder = new();

        foreach (var topic in allTopics)
        {
            logger.LogInformation("Found Trigger Topic: '{TopicName}'", topic);
            subscribeOptionsBuilder.WithTopicFilter(topic);
        }
        
        var subscribeOptions = subscribeOptionsBuilder.Build();
        if (subscribeOptions.TopicFilters.IsEmpty())
        {
            logger.LogWarning("Could not find any Topics to subscribe to");
            return;
        }
        
        MqttClientSubscribeResult subscribeResults = await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
        
        foreach (var subscribeResult in subscribeResults.Items)
            if (subscribeResult.ResultCode >= MqttClientSubscribeResultCode.UnspecifiedError)
                logger.LogError("Failed to subscribe to Topic '{TopicName}'", subscribeResult.TopicFilter.Topic);
            else 
                logger.LogInformation("Subscribed to Topic '{TopicName}'", subscribeResult.TopicFilter.Topic);
    }
    
    
    private async Task OnTopicMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
    {
        logger.LogInformation("Topic received: {TopicName}", arg.ApplicationMessage.Topic);

        // Find all tasks that has this topic as a trigger.
        List<Lactose.Tasks.Models.Task> tasksWithTriggerTopic = await tasksRepo.GetTasksWithTriggerTopic(arg.ApplicationMessage.Topic);
        
        if (tasksWithTriggerTopic.IsEmpty())
        {
            logger.LogInformation("No Tasks were found with a Trigger of Topic '{TopicName}'", arg.ApplicationMessage.Topic);
            return;
        }

        var sb = new StringBuilder($"Found {tasksWithTriggerTopic.Count} Task(s) with Trigger Topic '{arg.ApplicationMessage.Topic}':\n");
        foreach (Lactose.Tasks.Models.Task task in tasksWithTriggerTopic)
            sb.AppendLine($"{task.Id} ({task.Name})");
        logger.LogInformation("{Contents}", sb.ToString());
        
        // Get the user ID from the payload.
        UserEvent? userEvent = arg.ApplicationMessage.FromJson<UserEvent>();
        if (userEvent is null)
            throw new NullReferenceException($"Task Trigger Topic '{arg.ApplicationMessage.Topic}' is not a User Event");
        
        // Get all the User Tasks that match any of the Tasks with this Trigger.
        List<UserTask> userTasks = await userTasksRepo.GetUserTasksByTaskId(userEvent.UserId, tasksWithTriggerTopic.Select(t => t.Id!));

        foreach (var task in tasksWithTriggerTopic)
        {
            UserTask? userTask = userTasks.FirstOrDefault(userTask => userTask.TaskId == task.Id);
            
            // Check if the user can make progress on this Task (i.e. it is not already complete).
            if (userTask is { Completed: true })
            {
                logger.LogInformation("User '{UserId}' has already completed Task '{TaskId}' under. No progress will be made", 
                    userEvent!.UserId, task.Id);
                continue;
            }
            
            Trigger? foundTrigger = task.Triggers.FirstOrDefault(t => 
                MqttTopicFilterComparer.Compare(arg.ApplicationMessage.Topic, t.Topic) == MqttTopicFilterCompareResult.IsMatch);
            
            if (foundTrigger is null)
            {
                // This shouldn't happen but handle anyway.
                logger.LogError("Failed to find a Trigger within Task '{TaskId}' ({TaskName}) that matches Topic '{TopicName}'",
                    task.Id, task.Name, arg.ApplicationMessage.Topic);
                continue;
            }

            ITaskTriggerHandler? foundTriggerHandler = triggerHandlerRegistry.FindHandler(foundTrigger.Handler);
            if (foundTriggerHandler is null)
            {
                logger.LogError("Failed to find a Trigger Handler with name '{TriggerHandlerName}'", foundTrigger.Handler);
                continue;
            }

            try
            {
                if (foundTrigger.Config is null)
                    foundTrigger.Config = null;
                else if (foundTrigger.Config.GetType() != foundTriggerHandler.ConfigType)
                    throw new InvalidCastException($"Config is wrong type. Expected '{foundTriggerHandler.ConfigType}'. Received '{foundTrigger.Config.GetType()}'");
            }
            catch (Exception e)
            {
                logger.LogError("Failed to parse the Config for Trigger Handler '{TriggerHandlerName}' into '{ConfigTypeName}'. Exception:\n{Exception}", 
                    foundTriggerHandler.GetType().Name, foundTriggerHandler.ConfigType.Name, e);
                continue;
            }
            
            try
            {
                // Try convert the event message to the type the Trigger Handler expects.
                userEvent = (UserEvent?)arg.ApplicationMessage.FromJson(foundTriggerHandler.EventType);
                if (userEvent is null)
                    throw new NullReferenceException();
            }
            catch (Exception e)
            {
                logger.LogError("Failed to parse the Event Payload for Trigger Handler '{TriggerHandlerName}' into '{EventTypeName}'. Exception:\n{Exception}", 
                    foundTriggerHandler.GetType().Name, foundTriggerHandler.EventType.Name, e);
                continue;
            }

            float taskProgress;
            try
            {
                taskProgress = await foundTriggerHandler.CalculateTaskProgress(foundTrigger, userEvent);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to execute Trigger Handler '{TriggerHandlerName}' for the Task '{TaskId}' ({TaskName}). Reason:\n{Reason}",
                    foundTriggerHandler.GetType().Name, task.Id, task.Name, e.Message);
                continue;
            }
            
            if (taskProgress == 0)
                continue;
            
            logger.LogInformation("User '{UserId}' has made {Progress:N1} progress on Task '{TaskId}' ({TaskName})",
                userEvent.UserId, taskProgress, task.Id, task.Name);

            float prevTaskProgress;
            if (userTask is null)
            {
                prevTaskProgress = 0;

                // Create a new user task with the new progress.
                userTask = new UserTask
                {
                    TaskId = task.Id!,
                    UserId = userEvent.UserId,
                    Progress = taskProgress,
                    Completed = taskProgress >= task.RequiredProgress
                };
            }
            else
            {
                prevTaskProgress = userTask.Progress;
                
                userTask.Progress += taskProgress;
                if (userTask.Progress >= task.RequiredProgress)
                    userTask.Completed = true;
            }

            if (userTask.Completed && task.Rewards.Count > 0)
            {
                var tradeRequest = new TradeRequest
                {
                    UserA = new UserTradeRequest
                    {
                        Items = task.Rewards
                    },
                    UserB = new UserTradeRequest
                    {
                        UserId = userTask.UserId
                    }
                };

                ActionResult<TradeResponse> transactionResult = await transactionsClient.Trade(tradeRequest);
                if (transactionResult.Value?.Reason != TradeResponseReason.Success)
                {
                    logger.LogError("Could not claim Task Rewards from User Task '{UserTaskId}' to User '{UserTaskUserId}'", 
                        userTask.Id, userTask.UserId);
                    continue;
                }
            }
            
            userTask = await userTasksRepo.Set(userTask);
            if (userTask is null)
            {
                logger.LogError("Failed to update User Task");
                continue;
            }
            
            logger.LogInformation("User '{UserId}' has made progress on Task '{TaskId}' ({TaskName}) under User Task '{UserTaskId}'. Completed: {Completed}",
                userEvent.UserId, task.Id, task.Name, userTask.Id, userTask.Completed);
            
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"/tasks/usertasks/{userEvent.UserId}/updated")
                .WithPayload(new UserTaskUpdatedEvent
                {
                    UserId = userEvent.UserId,
                    TaskId = task.Id!,
                    UserTaskId = userTask.Id!,
                    PreviousProgress = prevTaskProgress
                }.ToJson())
                .Build());

            if (userTask.Completed)
            {
                // Not sure if I care about this. It's really just a filtered variation of the updated event.
                // I'll keep it just to play with it.
                await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic($"/tasks/usertasks/{userEvent.UserId}/completed")
                    .WithPayload(new UserTaskUpdatedEvent
                    {
                        UserId = userEvent.UserId,
                        TaskId = task.Id!,
                        UserTaskId = userTask.Id!,
                        PreviousProgress = prevTaskProgress
                    }.ToJson())
                    .Build());
            }
        }
    }
}