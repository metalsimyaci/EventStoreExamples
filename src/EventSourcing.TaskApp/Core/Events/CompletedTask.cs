using System;

namespace EventSourcing.TaskApp.Core.Events
{
    public class CompletedTask
    {
        public Guid TaskId { get; set; }
        public string CompletedBy { get; set; }
    }
}