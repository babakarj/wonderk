using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Department.Regular
{
    public class RegularConsumer(IQueueProcessor queue) : Consumer(queue)
    {
        public override void Process(Package package)
        {
            base.Process(package);

            Console.WriteLine($"Regular consumed package: {package}");
        }
    }
}
