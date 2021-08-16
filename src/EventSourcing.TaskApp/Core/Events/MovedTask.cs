using System;

namespace EventSourcing.TaskApp.Core.Events
{
    public class MovedTask
    {
        public Guid TaskId { get; set; }
        public string MovedBy { get; set; }
        public BoardSections Section { get; set; }
    }
}