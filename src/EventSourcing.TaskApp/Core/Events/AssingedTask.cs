using System;

namespace EventSourcing.TaskApp.Core.Events
{
    public class AssingedTask
    {
        public Guid TaskId { get; set; }
        public string AssignedBy { get; set; }
        public string AssignedTo { get; set; }
    }
}