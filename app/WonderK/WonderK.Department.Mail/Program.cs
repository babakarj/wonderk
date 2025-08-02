using WonderK.Common.Libraries;
using WonderK.Department.Mail;

const string DepartmentName = "Mail";

Console.WriteLine($"Hello, from {DepartmentName} department!");

IQueueProcessor queue = new RedisQueueProcessor();

string streamKey = $"{DepartmentName}-stream";
string groupName = $"{DepartmentName}-consumer-group";
string consumerName = "mail-" + Guid.NewGuid().ToString();

MailConsumer consumer = new(queue);
await consumer.Listen(streamKey, groupName, consumerName);