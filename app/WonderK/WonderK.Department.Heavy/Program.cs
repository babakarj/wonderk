using WonderK.Common.Libraries;
using WonderK.Department.Heavy;

const string DepartmentName = "Heavy";

Console.WriteLine($"Hello, from {DepartmentName} department!");

IQueueProcessor queue = new RedisQueueProcessor();

string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "heavy-" + Guid.NewGuid().ToString();

HeavyConsumer consumer = new(queue);
await consumer.Listen(streamKey, groupName, consumerName);