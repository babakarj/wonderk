using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Insurance
{
    public class InsuranceConsumer(IQueueProcessor queue, IProcessLogger processLogger) : Consumer(queue, processLogger)
    {
        public override async Task Process(Package package)
        {
            await base.Process(package);

            package.Metadata.AddLast($"Signed by the insurance department at {DateTime.UtcNow}");

            string payload = package.ToString();

            Console.WriteLine($"Insurance consumed package: {payload}");

            await ProcessLogger.LogAsync("Insurance", payload);
        }
    }
}
