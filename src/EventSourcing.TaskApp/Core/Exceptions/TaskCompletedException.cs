using System;

namespace EventSourcing.TaskApp.Core.Exceptions
{
    public class TaskCompletedException:Exception
    {
        public TaskCompletedException():base("Task is completed."){}
    }
}