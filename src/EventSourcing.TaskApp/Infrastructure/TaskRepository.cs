using System;
using System.Collections.Generic;
using Couchbase;
using EventSourcing.TaskApp.Core;
using EventSourcing.TaskApp.Core.Events;
using EventSourcing.TaskApp.Infrastructure.Buckets;
using MutateInSpec = Couchbase.KeyValue.MutateInSpec;

namespace EventSourcing.TaskApp.Infrastructure
{
    public class TaskRepository
    {
        private readonly IBucket _bucket;

        public TaskRepository(ITaskBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();
        }

        public void Save(object @event)
        {
            switch (@event)
            {
                case CreatedTask x: OnCreated(x); break;
                case AssingedTask x: OnAssigned(x); break;
                case MovedTask x: OnMoved(x); break;
                case CompletedTask x: OnCompleted(x); break;
            }
        }

        public async System.Threading.Tasks.Task<TaskDocument> Get(Guid taskId)
        {
            var documentResult = (await _bucket.DefaultCollection().GetAsync(taskId.ToString()))?.ContentAs<TaskDocument>();
            return documentResult;
        }

        private async void OnCreated(CreatedTask @event)
        {
            var document = new TaskDocument
            {
                Id = @event.TaskId,
                Title = @event.Title,
                Section = BoardSections.Open
            };

            await _bucket.DefaultCollection().InsertAsync(@event.TaskId.ToString(), document);
        }

        private async void OnMoved(MovedTask @event)
        {
            await _bucket.DefaultCollection().MutateInAsync(@event.TaskId.ToString(),
                new List<MutateInSpec>
                {
                    MutateInSpec.Replace("section", @event.Section)
                });
        }

        private async void OnAssigned(AssingedTask @event)
        {
            await _bucket.DefaultCollection().MutateInAsync(@event.TaskId.ToString(),
                new List<MutateInSpec>
                {
                    MutateInSpec.Replace("assignedTo", @event.AssignedTo)
                }
            );
        }

        private async void OnCompleted(CompletedTask @event)
        {
            await _bucket.DefaultCollection().MutateInAsync(@event.TaskId.ToString(),
                new List<MutateInSpec>
                {
                    MutateInSpec.Replace("completedBy", @event.CompletedBy)
                });
        }
    }
}