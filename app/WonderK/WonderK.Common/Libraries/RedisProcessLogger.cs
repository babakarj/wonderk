using StackExchange.Redis;

namespace WonderK.Common.Libraries
{
    public class RedisProcessLogger : IProcessLogger
    {
        private IDatabase Db { get; }
        private const string LogKey = "processlogs";

        public RedisProcessLogger()
        {
            var port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
            var redis = ConnectionMultiplexer.Connect("redis:" + port);
            Db = redis.GetDatabase();
        }

        public async Task<bool> LogAsync(string source, string message)
        {
            var timestamp = DateTime.UtcNow;
            var payload = $"[{Guid.NewGuid()} {timestamp:O}] {source} - {message}";
            var score = new DateTimeOffset(timestamp).ToUnixTimeMilliseconds();

            return await Db.SortedSetAddAsync(LogKey, payload, score);
        }

        public async Task<IEnumerable<string>> GetAsync(int page = 1, int pageSize = 100, string? filterText = null)
        {
            long start = (page - 1) * pageSize;
            long stop = start + pageSize - 1;

            RedisValue[] redisLogs;

            if (string.IsNullOrWhiteSpace(filterText))
            {
                redisLogs = await Db.SortedSetRangeByRankAsync(LogKey, start, stop, Order.Descending);
            }
            else
            {
                // Fetch a larger batch of logs and filter in-memory.
                const int searchLimitFactor = 10; // Fetch 10x the page size to find matches
                long searchStop = start + (pageSize * searchLimitFactor) - 1;

                var allFetchedLogs = await Db.SortedSetRangeByRankAsync(LogKey, start, searchStop, Order.Descending);

                redisLogs = allFetchedLogs
                    .Where(x => x.ToString().Contains(filterText, StringComparison.OrdinalIgnoreCase))
                    .Take(pageSize)
                    .ToArray();
            }

            return redisLogs.Select(x => x.ToString());
        }
    }
}
