using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventSourcing.TaskApp.Core.Framework;
using EventStore.ClientAPI;

namespace EventSourcing.TaskApp.Infrastructure
{
    public class AggregateRepository
    {
        private readonly IEventStoreConnection _eventStore;

        public AggregateRepository(IEventStoreConnection eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task SaveAsync<T>(T aggregate) where T : Aggregate, new()
        {
            var events = aggregate.GetChanges().Select(@event =>
                    new EventData(
                        Guid.NewGuid(),
                        @event.GetType().Name, 
                        true,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                        Encoding.UTF8.GetBytes(@event.GetType().FullName)))
                .ToArray();

            if (!events.Any())
            {
                return;
            }
            
            var streamName = GetStreamName(aggregate, aggregate.Id);

            var result = await _eventStore.AppendToStreamAsync(streamName, ExpectedVersion.Any, events);
        }

        public async Task SaveWithTransactionAsync<T>(T aggregate) where T : Aggregate, new()
        {
            var events = aggregate.GetChanges().Select(@event =>
                    new EventData(
                        Guid.NewGuid(),
                        @event.GetType().Name,
                        true,
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                        Encoding.UTF8.GetBytes(@event.GetType().FullName)))
                .ToArray();

            if (!events.Any())
            {
                return;
            }

            var streamName = GetStreamName(aggregate, aggregate.Id);

            using var transaction = await _eventStore.StartTransactionAsync(streamName, ExpectedVersion.Any);
            try
            {
                var result = await _eventStore.AppendToStreamAsync(streamName, ExpectedVersion.Any, events);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               transaction.Rollback();
            }
        }

        public async Task<T> LoadAsync<T>(Guid aggregateId) where T : Aggregate, new()
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(aggregateId));

            var aggregate = new T();
            var streamName = GetStreamName(aggregate, aggregateId);

            var nextPageStart = 0L;

            do
            {
                var page = await _eventStore.ReadStreamEventsForwardAsync(streamName, nextPageStart, 4096, false);
                if (page.Events.Length > 0)
                {
                    aggregate.Load(
                        page.Events.Last().Event.EventNumber,
                        page.Events
                            .Select(@event =>
                                JsonSerializer.Deserialize(
                                    Encoding.UTF8.GetString(@event.OriginalEvent.Data),
                                    Type.GetType(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata)))
                                )
                            .ToArray());
                }

                nextPageStart = !page.IsEndOfStream ? page.NextEventNumber : -1;
            } while (nextPageStart != -1);

            return aggregate;
        }

        public async Task DeleteAsync<T>(T aggregate) where T : Aggregate, new()
        {
            var streamName = GetStreamName(aggregate, aggregate.Id);

            var result = await _eventStore.DeleteStreamAsync(streamName, ExpectedVersion.Any);
        }

        public async Task SubscribeAsync(string streamName)
        {
            var subsscription = await _eventStore.SubscribeToStreamAsync(
                stream: streamName,
                resolveLinkTos: true,
                eventAppeared: async (eventStoreSubscription, resolvedEvent) => Console.WriteLine($"Evente abone olundu.{Encoding.UTF8.GetString(resolvedEvent.Event.Data)}"),
                subscriptionDropped: (eventStoreSubscription, SubscriptionDropReason, exception) =>
                    Console.WriteLine($"Event Takibi bırakılmıştır. {SubscriptionDropReason}")
            );
        }


        private string GetStreamName<T>(T type, Guid aggregateId) => $"{type.GetType().Name}-{aggregateId}";
    }
}