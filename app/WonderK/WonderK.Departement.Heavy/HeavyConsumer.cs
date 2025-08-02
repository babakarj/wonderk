using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Departement.Heavy
{
    public class HeavyConsumer(IQueueProcessor queue) : Consumer(queue)
    {
        public override void Process(Package package)
        {
            base.Process(package);

            Console.WriteLine($"Heavy consumed package: {package}");
        }
    }
}
