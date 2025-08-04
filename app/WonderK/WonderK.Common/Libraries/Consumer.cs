using WonderK.Common.Data;

namespace WonderK.Common.Libraries
{
    public abstract class Consumer(IQueueProcessor queue, IProcessLogger processLogger)
    {
        public IQueueProcessor Queue { get; } = queue;
        public IProcessLogger ProcessLogger { get; } = processLogger;

        public async Task Listen(string streamKey, string groupName, string consumerName)
        {
            await Queue.Consume(streamKey, groupName, consumerName, async (data) =>
            {
                Package package = new(data);

                Process(package);

                await Forward(package);
            });
        }

        public virtual Task Process(Package package)
        {
            if (package.Departments.Count > 0)
            {
                package.Departments.RemoveFirst();
            }

            return Task.CompletedTask;
        }

        public async Task Forward(Package package)
        {
            if (package.Departments.Count > 0)
            {
                string nextConsumer = package.Departments.First.Value;

                string streamKey = nextConsumer + "-stream";

                await Queue.Produce(streamKey, package.ToString());

                Console.WriteLine($"Forwarding package to {nextConsumer}");
            }
            else
            {
                Console.WriteLine("No more consumers to forward the package to.");
            }
        }
    }
}
