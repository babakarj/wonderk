using Microsoft.Extensions.Logging;
using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Heavy
{
    public class HeavyConsumer(IQueueProcessor queue, IProcessLogger processLogger, ILogger<Consumer> logger)
        : Consumer(queue, processLogger, logger)
    {
        public override async Task Process(Package package)
        {
            await base.Process(package);

            string payload = package.ToString();

            Console.WriteLine($"Heavy consumed package: {payload}");

            await ProcessLogger.LogAsync("Heavy", payload);
        }
    }
}
