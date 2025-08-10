using Microsoft.Extensions.Logging;
using WonderK.Common.Libraries;
using WonderK.Department.Heavy;

const string DepartmentName = "Heavy";

Console.WriteLine($"Hello, from {DepartmentName} department!");

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<RedisQueueProcessor> redisLogger = loggerFactory.CreateLogger<RedisQueueProcessor>();
ILogger<Consumer> logger = loggerFactory.CreateLogger<Consumer>();

IQueueProcessor queue = new RedisQueueProcessor(redisLogger);
IProcessLogger processLogger = new RedisProcessLogger();

string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "heavy-" + Guid.NewGuid().ToString();

HeavyConsumer consumer = new(queue, processLogger, logger);
await consumer.Listen(streamKey, groupName, consumerName);