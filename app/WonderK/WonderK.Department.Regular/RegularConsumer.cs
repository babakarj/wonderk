using Microsoft.Extensions.Logging;
using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Regular
{
    public class RegularConsumer(IQueueProcessor queue, IProcessLogger processLogger, ILogger<Consumer> logger)
        : Consumer(queue, processLogger, logger)
    {
        public override async Task Process(Package package)
        {
            await base.Process(package);

            string payload = package.ToString();

            Console.WriteLine($"Regular consumed package: {payload}");

            await ProcessLogger.LogAsync("Regular", payload);
        }
    }
}
