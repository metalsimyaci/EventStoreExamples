using System.Threading.Tasks;
using Couchbase;
using EventSourcing.TaskApp.Infrastructure.Buckets;
using EventStore.ClientAPI;

namespace EventSourcing.TaskApp.Infrastructure
{
    public class CheckpointRepository
    {
        private readonly IBucket _bucket;

        public CheckpointRepository(ICheckpointBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucketAsync().GetAwaiter().GetResult();
        }

        public async Task<Position?> GetAsync(string key)
        {
            var existsResult = await _bucket.DefaultCollection().ExistsAsync(key);
            if (!existsResult.Exists)
                return null;

            var result = (await _bucket.DefaultCollection().GetAsync(key))?.ContentAs<CheckpointDocument>();

            if (result == null)
                return null;

            return result.Position;
        }

        public async Task<bool> SaveAsync(string key, Position position)
        {
            var document = new CheckpointDocument
            {
                Key = key,
                Position = position
            };

            var result = await _bucket.DefaultCollection().UpsertAsync(key, document);

            return result.Cas > 0;
        }
    }
}