using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.Departement.Mail
{
    public class MailConsumer(IQueueProcessor queue) : Consumer(queue)
    {
        public override void Process(Package package)
        {
            base.Process(package);

            Console.WriteLine($"Mail consumed package: {package}");
        }
    }
}
