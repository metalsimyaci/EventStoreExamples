using System;

namespace EventSourcing.TaskApp.Core.Exceptions
{
    public class TaskNotFoundException:Exception
    {
        public TaskNotFoundException():base("Task not found."){}
    }
}