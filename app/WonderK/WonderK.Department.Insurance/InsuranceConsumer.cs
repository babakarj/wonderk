using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Insurance
{
    public class InsuranceConsumer(IQueueProcessor queue) : Consumer(queue)
    {
        public override void Process(Package package)
        {
            base.Process(package);

            package.Metadata.AddLast($"Signed by the insurance department at {DateTime.UtcNow}");

            Console.WriteLine($"Insurance consumed package: {package}");
        }
    }
}
