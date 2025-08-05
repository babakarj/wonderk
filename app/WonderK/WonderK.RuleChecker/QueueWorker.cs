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
        private readonly IProcessLogger _processLogger;
        private readonly string _rulebookFile;
        private ImmutableList<Rule> _rules;
        private FileSystemWatcher? _watcher;

        private readonly string _streamKey = "parcel-stream";
        private readonly string _groupName = "rule-checker-group";
        private readonly string _consumerName = "rc-" + Guid.NewGuid().ToString();

        public QueueWorker(IWebHostEnvironment env, ILogger<QueueWorker> logger, IQueueProcessor queue, IProcessLogger processLogger)
        {
            _env = env;
            _logger = logger;
            _queue = queue;
            _processLogger = processLogger;
            _rulebookFile = Path.Combine(_env.ContentRootPath, "rules-book.txt");

            _streamKey = "parcel-stream";
            _groupName = "rule-checker-group";
            _consumerName = "rc-" + Guid.NewGuid().ToString();

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
            Interlocked.Exchange(ref _rules, GetRules());
            _logger.LogInformation("Rules reloaded from rules-book.txt.");
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

            await _queue.Consume(_streamKey, _groupName, _consumerName, async (data) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine("Cancellation requested, stopping processing.");
                    return;
                }

                XmlSerializer serializer = new(typeof(Parcel));
                using StringReader reader = new(data);
                Parcel parcel = (Parcel)serializer.Deserialize(reader);
                Console.WriteLine($"{_consumerName} ** Recipient: {parcel.Receipient.Name}, Weight: {parcel.Weight}, Value: {parcel.Value}");

                var departments = Volatile.Read(ref _rules).GetDepartments(parcel);
                Console.WriteLine("Matching departments: " + string.Join(", ", departments));

                await Send(parcel, departments);
            });
        }

        async Task Send(Parcel parcel, HashSet<string> departments)
        {
            Package package = new(parcel, departments);

            string streamKey = departments.First() + "-stream";

            string payload = package.ToString();

            string messageId = await _queue.Produce(streamKey, payload);

            await _processLogger.LogAsync("RuleChecker", payload);

            Console.WriteLine($"Message added to stream with ID: {messageId}");
        }
    }
}
