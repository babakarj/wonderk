using WonderK.Common.Libraries;
using WonderK.Department.Insurance;

const string DepartmentName = "Insurance";

Console.WriteLine($"Hello, from {DepartmentName} department!");

IQueueProcessor queue = new RedisQueueProcessor();
IProcessLogger processLogger = new RedisProcessLogger();

string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "ins-" + Guid.NewGuid().ToString();

InsuranceConsumer consumer = new(queue, processLogger);
await consumer.Listen(streamKey, groupName, consumerName);