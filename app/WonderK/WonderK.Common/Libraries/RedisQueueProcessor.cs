using StackExchange.Redis;

namespace WonderK.Common.Libraries
{
    public class RedisQueueProcessor : IQueueProcessor
    {
        private IDatabase Db { get; }

        public RedisQueueProcessor()
        {
            var redis = ConnectionMultiplexer.Connect("redis:6379");
            Db = redis.GetDatabase();
        }

        public async Task<string> Produce(string streamKey, string data)
        {
            string messageId = await Db.StreamAddAsync(streamKey, [new NameValueEntry("data", data)]);
            return messageId;
        }

        public async Task Consume(string streamKey, string groupName, string consumerName, Action<string> action)
        {
            Console.WriteLine($"Listening to stream '{streamKey}' by '{groupName}|{consumerName}'...");

            while (true)
            {
                // Try to create the group (ignore if exists)
                try
                {
                    await Db.StreamCreateConsumerGroupAsync(streamKey, groupName, "0-0");
                    Console.WriteLine($"Consumer group '{groupName}' created.");
                }
                catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
                {
                    Console.WriteLine($"Consumer group '{groupName}' already exists.");
                }

                while (true)
                {
                    var entries = await Db.StreamReadGroupAsync(streamKey, groupName, consumerName, count: 1);

                    if (entries.Length == 0)
                    {
                        // Auto-claim pending messages older than 10 seconds
                        var result = await Db.StreamAutoClaimAsync(streamKey, groupName, consumerName,
                            minIdleTimeInMs: TimeSpan.FromSeconds(10).Milliseconds, // Idle time threshold for claiming old messages
                            startAtId: "0-0",
                            count: 1);
                        entries = result.ClaimedEntries;
                    }

                    foreach (var entry in entries)
                    {
                        Console.WriteLine($"Claimed message {entry.Id}, data: {entry["data"]}");

                        action(entry["data"]);

                        // Acknowledge message
                        await Db.StreamAcknowledgeAsync(streamKey, groupName, entry.Id);
                    }

                    await Task.Delay(1000); // Polling interval
                }
            }
        }
    }
}
