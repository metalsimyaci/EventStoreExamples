using System;

namespace EventSourcing.TaskApp.Core.Events
{
    public class CreatedTask
    {
        public Guid TaskId { get; set; }
        public string CreatedBy { get; set; }
        public string Title { get; set; }
    }
}