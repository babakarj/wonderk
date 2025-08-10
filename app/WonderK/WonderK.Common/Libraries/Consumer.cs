using Microsoft.Extensions.Logging;
using WonderK.Common.Data;

namespace WonderK.Common.Libraries
{
    public abstract class Consumer(IQueueProcessor queue, IProcessLogger processLogger, ILogger<Consumer> logger)
    {
        private readonly ILogger<Consumer> _logger = logger;

        public IQueueProcessor Queue { get; } = queue;
        public IProcessLogger ProcessLogger { get; } = processLogger;

        public async Task Listen(string streamKey, string groupName, string consumerName)
        {
            await Queue.Consume(
                streamKey, groupName, consumerName,
                async (data) =>
                {
                    Package package = new(data);

                    await Process(package);

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

        public virtual async Task Forward(Package package)
        {
            if (package.Departments.Count > 0 && package.Departments.First != null)
            {
                string nextConsumer = package.Departments.First.Value;

                string streamKey = nextConsumer + "-stream";

                await Queue.Produce(streamKey, package.ToString());

                _logger.LogDebug($"Forwarding package to {nextConsumer}.");
            }
            else
            {
                _logger.LogDebug("No more consumers to forward the package to.");
            }
        }
    }
}
