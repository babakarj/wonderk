using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Departement.Insurance
{
    public class InsuranceConsumer(IQueueProcessor queue) : Consumer(queue)
    {
        public override void Process(Package package)
        {
            base.Process(package);

            package.Metadata.AddLast($"Insurance processed at {DateTime.UtcNow}");

            Console.WriteLine($"Insurance consumed package: {package}");
        }
    }
}
