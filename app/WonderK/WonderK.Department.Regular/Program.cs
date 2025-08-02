using WonderK.Common.Libraries;
using WonderK.Department.Regular;

const string DepartmentName = "Regular";

Console.WriteLine($"Hello, from {DepartmentName} department!");

IQueueProcessor queue = new RedisQueueProcessor();
string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "processor-1";

RegularConsumer consumer = new(queue);
await consumer.Listen(streamKey, groupName, consumerName);