using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.TaskApp.Core.Events;
using EventSourcing.TaskApp.Infrastructure;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventSourcing.TaskApp.HostedServices
{
    public class TaskHostedService:IHostedService
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly CheckpointRepository _checkpointRepository;
        private readonly TaskRepository _taskRepository;
        private readonly ILogger<TaskHostedService> _logger;

        private EventStoreAllCatchUpSubscription subscription;

        public TaskHostedService(IEventStoreConnection eventStore, CheckpointRepository checkpointRepository, TaskRepository taskRepository, ILogger<TaskHostedService> logger)
        {
            _eventStore = eventStore;
            _checkpointRepository = checkpointRepository;
            _taskRepository = taskRepository;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var lastCheckpoint = await _checkpointRepository.GetAsync("tasks");

            var settings = new CatchUpSubscriptionSettings(
                maxLiveQueueSize: 10000,
                readBatchSize: 500,
                verboseLogging: false,
                resolveLinkTos: false,
                subscriptionName: "Tasks");

            subscription = _eventStore.SubscribeToAllFrom(
                lastCheckpoint: lastCheckpoint,
                settings: settings,
                eventAppeared: async (sub, @event) =>
                {
                    if (@event.OriginalEvent.EventType.StartsWith("$"))
                        return;

                    try
                    {
                        var eventType = Type.GetType(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata));
                        var eventData = JsonSerializer.Deserialize(Encoding.UTF8.GetString(@event.OriginalEvent.Data), eventType);

                        if (eventType != typeof(CreatedTask) && 
                            eventType != typeof(AssingedTask) && 
                            eventType != typeof(MovedTask) &&
                            eventType != typeof(CompletedTask))
                            return;

                        _taskRepository.Save(eventData);

                        await _checkpointRepository.SaveAsync("tasks", @event.OriginalPosition.GetValueOrDefault());
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                },
                liveProcessingStarted: (sub) =>
                {
                    _logger.LogInformation($"{sub.SubscriptionName} subscription started.");
                },
                subscriptionDropped: (sub, subscriptionDropReason, exception) =>
                {
                    _logger.LogWarning($"{sub.SubscriptionName} dropped. Reason: {subscriptionDropReason}");
                }
            );
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            subscription.Stop();

            return  Task.CompletedTask;
        }
    }
}