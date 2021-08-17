﻿using System;
using EventSourcing.TaskApp.Core;

namespace EventSourcing.TaskApp.Infrastructure
{
    public class TaskDocument
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedTo { get; set; }
        public BoardSections Section { get; set; }
        public string CompletedBy { get; set; }
    }
}