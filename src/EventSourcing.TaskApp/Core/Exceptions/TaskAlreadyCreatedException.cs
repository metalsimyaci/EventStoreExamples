using System;

namespace EventSourcing.TaskApp.Core.Exceptions
{
    public class TaskAlreadyCreatedException : Exception
    {
        public TaskAlreadyCreatedException() : base("Task already created.") { }
    }
}