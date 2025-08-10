using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace WonderK.Common.Libraries
{
    public class RedisQueueProcessor : IQueueProcessor
    {
        private const string DataFieldName = "data";

        private readonly ILogger<RedisQueueProcessor> _logger;

        private IDatabase Db { get; }

        public RedisQueueProcessor(ILogger<RedisQueueProcessor> logger)
        {
            _logger = logger;
            var port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
            var redis = ConnectionMultiplexer.Connect("redis:" + port);
            Db = redis.GetDatabase();
        }

        public async Task<string> Produce(string streamKey, string data)
        {
            RedisValue messageId = await Db.StreamAddAsync(streamKey, [new NameValueEntry(DataFieldName, data)]);
            return messageId.ToString();
        }

        public async Task Consume(string streamKey, string groupName, string consumerName, Action<string> action)
        {
            _logger.LogDebug($"Listening to stream '{streamKey}' by '{groupName}|{consumerName}'...");

            long ten_seconds = (long)TimeSpan.FromSeconds(10).TotalMilliseconds;

            while (true)
            {
                // Try to create the group (ignore if exists)
                try
                {
                    await Db.StreamCreateConsumerGroupAsync(streamKey, groupName, "$" /* start at new messages */);
                    _logger.LogDebug($"Consumer group '{groupName}' created.");
                }
                catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
                {
                    _logger.LogDebug($"Consumer group '{groupName}' already exists.");
                }

                while (true)
                {
                    var entries = await Db.StreamReadGroupAsync(streamKey, groupName, consumerName, count: 10);

                    if (entries.Length == 0)
                    {
                        // Auto-claim pending messages older than 10 seconds
                        var result = await Db.StreamAutoClaimAsync(streamKey, groupName, consumerName,
                            minIdleTimeInMs: ten_seconds, // Idle time threshold for claiming old messages
                            startAtId: "0-0",
                            count: 1);
                        entries = result.ClaimedEntries;
                    }

                    foreach (var entry in entries)
                    {
                        string? data = entry.Values.FirstOrDefault(v => v.Name == DataFieldName).Value;

                        if (data == null)
                        {
                            _logger.LogDebug($"No '{DataFieldName}' field found in message {entry.Id}. Skipping.");

                            await Db.StreamAcknowledgeAsync(streamKey, groupName, entry.Id);
                        }
                        else
                        {
                            _logger.LogDebug($"Claimed message {entry.Id} by consumer {groupName}-{consumerName}, data: {data}");

                            try
                            {
                                action(data);

                                await Db.StreamAcknowledgeAsync(streamKey, groupName, entry.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error processing message {entry.Id}");
                            }
                        }
                    }

                    await Task.Delay(250); // Polling interval
                }
            }
        }

        public async Task<List<(string Key, string Value)>> Status(string streamKey)
        {
            List<(string Key, string Value)> result = [];

            string[] validKeys = ["consumers", "pending", "entries-read", "lag"];

            var data = await Db.ExecuteAsync("XINFO", "GROUPS", streamKey);

            foreach (var groupInfo in (RedisResult[])data)
            {
                var groupProps = (RedisResult[])groupInfo;
                for (int i = 0; i < groupProps.Length - 1; i += 2)
                {
                    string key = groupProps[i].ToString();
                    if (validKeys.Contains(key))
                    {
                        result.Add((key.ToUpper(), groupProps[i + 1].ToString()));
                    }
                }
            }

            return result;
        }
    }
}
