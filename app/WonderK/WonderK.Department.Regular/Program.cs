using WonderK.Common.Libraries;
using WonderK.Department.Regular;

const string DepartmentName = "Regular";

Console.WriteLine($"Hello, from {DepartmentName} department!");

IQueueProcessor queue = new RedisQueueProcessor();
IProcessLogger processLogger = new RedisProcessLogger();

string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "rg-" + Guid.NewGuid().ToString();

RegularConsumer consumer = new(queue, processLogger);
await consumer.Listen(streamKey, groupName, consumerName);