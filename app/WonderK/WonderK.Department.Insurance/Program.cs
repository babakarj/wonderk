using WonderK.Common.Libraries;
using WonderK.Department.Insurance;

const string DepartmentName = "Insurance";

Console.WriteLine($"Hello, from {DepartmentName} department!");

IQueueProcessor queue = new RedisQueueProcessor();
string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "processor-1";

InsuranceConsumer consumer = new(queue);
await consumer.Listen(streamKey, groupName, consumerName);