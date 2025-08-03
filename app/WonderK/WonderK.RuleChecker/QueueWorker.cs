using System.Collections.Immutable;
using System.Xml.Serialization;
using WonderK.Common.Data;
using WonderK.Common.Libraries;

namespace WonderK.RuleChecker
{
    public class QueueWorker : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<QueueWorker> _logger;
        private readonly IQueueProcessor _queue;
        private readonly string _rulebookFile;
        private ImmutableList<Rule> _rules;
        private FileSystemWatcher? _watcher;
        private readonly object _rulesLock = new();

        public QueueWorker(IWebHostEnvironment env, ILogger<QueueWorker> logger, IQueueProcessor queue)
        {
            _env = env;
            _logger = logger;
            _queue = queue;
            _rulebookFile = Path.Combine(_env.ContentRootPath, "rules-book.txt");
            _rules = GetRules();
            SetupFileWatcher();
        }

        private void SetupFileWatcher()
        {
            string filePath = _rulebookFile;
            string directory = Path.GetDirectoryName(filePath)!;
            string fileName = Path.GetFileName(filePath);

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _watcher.Changed += (s, e) => ReloadRules();
            _watcher.EnableRaisingEvents = true;
        }

        private void ReloadRules()
        {
            lock (_rulesLock)
            {
                _rules = GetRules();
                _logger.LogInformation("Rules reloaded from rules-book.txt.");
            }
        }

        private ImmutableList<Rule> GetRules()
        {
            string filePath = Path.Combine(_env.ContentRootPath, "rules-book.txt");
            string ruleText = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "File not found.";

            return Rule.ParseRules(ruleText).ToImmutableList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Hello, from WonderK.RuleChecker!");            

            string streamKey = "parcel-stream";
            string groupName = "rule-checker-group";
            string consumerName = "rc-" + Guid.NewGuid().ToString();

            await _queue.Consume(streamKey, groupName, consumerName, async (data) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine("Cancellation requested, stopping processing.");
                    return;
                }

                XmlSerializer serializer = new(typeof(Parcel));
                using StringReader reader = new(data);
                Parcel parcel = (Parcel)serializer.Deserialize(reader);
                Console.WriteLine($"Recipient: {parcel.Receipient.Name}, Weight: {parcel.Weight}, Value: {parcel.Value}");

                var departments = _rules.GetDepartments(parcel);
                Console.WriteLine("Matching departments: " + string.Join(", ", departments));

                await Send(parcel, departments);
            });
        }

        async Task Send(Parcel parcel, HashSet<string> departments)
        {
            Package package = new(parcel, departments);

            string streamKey = departments.First() + "-stream";

            string messageId = await _queue.Produce(streamKey, package.ToString());

            Console.WriteLine($"Message added to stream with ID: {messageId}");
        }
    }
}
