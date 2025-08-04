using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Mail
{
    public class MailConsumer(IQueueProcessor queue, IProcessLogger processLogger) : Consumer(queue, processLogger)
    {
        public override async Task Process(Package package)
        {
            await base.Process(package);

            string payload = package.ToString();

            Console.WriteLine($"Mail consumed package: {payload}");

            await ProcessLogger.LogAsync("Mail", payload);
        }
    }
}
