using EventStore.ClientAPI;

namespace EventSourcing.TaskApp.Infrastructure
{
    public class CheckpointDocument
    {
        public string Key { get; set; }
        public Position Position { get; set; }
    }
}