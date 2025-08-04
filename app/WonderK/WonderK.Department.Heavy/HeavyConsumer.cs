using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Heavy
{
    public class HeavyConsumer(IQueueProcessor queue, IProcessLogger processLogger) : Consumer(queue, processLogger)
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
