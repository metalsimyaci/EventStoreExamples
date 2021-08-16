using System;
using EventSourcing.TaskApp.Core.Events;
using EventSourcing.TaskApp.Core.Exceptions;
using EventSourcing.TaskApp.Core.Framework;

namespace EventSourcing.TaskApp.Core
{
    public class Task : Aggregate
    {
        public string Title { get; set; }
        public BoardSections Section { get; set; }
        public string AssignedTo { get; set; }
        public bool IsCompleted { get; set; }

        protected override void When(object @event)
        {
            switch (@event)
            {
                case CreatedTask x: OnCreated(x); break;
                case AssingedTask x: OnAssigned(x); break;
                case MovedTask x: OnMoved(x); break;
                case CompletedTask x: OnCompleted(x); break;
            }
        }

        public void Create(Guid taskId, string title, string createdBy)
        {
            if (Version >= 0)
            {
                throw new TaskAlreadyCreatedException();
            }

            Apply(new CreatedTask
            {
                TaskId = taskId,
                Title = title,
                CreatedBy = createdBy
            });
        }
        private void OnCreated(CreatedTask @event)
        {
            Id = @event.TaskId;
            Title = @event.Title;
            Section = BoardSections.Open;
        }

        private void OnAssigned(AssingedTask @event)
        {
            AssignedTo = @event.AssignedTo;
        }
        public void Assign(string assignedBy, string assignedTo)
        {
            if (Version == -1)
            {
                throw new TaskNotFoundException();
            }

            if (IsCompleted)
            {
                throw new TaskCompletedException();
            }

            Apply(new AssingedTask()
            {
                TaskId = Id,
                AssignedBy = assignedBy,
                AssignedTo = assignedTo
            });
        }

        private void OnMoved(MovedTask @event)
        {
            Section = @event.Section;
        }
        public void Move(BoardSections section, string movedBy)
        {
            if (Version == -1)
            {
                throw new TaskNotFoundException();
            }

            if (IsCompleted)
            {
                throw new TaskCompletedException();
            }

            Apply(new MovedTask
            {
                TaskId = Id,
                MovedBy = movedBy,
                Section = section
            });
        }

        private void OnCompleted(CompletedTask @event)
        {
            IsCompleted = true;
        }
        public void Complete(string completedBy)
        {
            if (Version == -1)
            {
                throw new TaskNotFoundException();
            }

            if (IsCompleted)
            {
                throw new TaskCompletedException();
            }

            Apply(new CompletedTask
            {
                TaskId = Id,
                CompletedBy = completedBy
            });
        }
    }
}