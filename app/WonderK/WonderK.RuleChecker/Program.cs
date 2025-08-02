using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WonderK.Common.Data;
using WonderK.Common.Libraries;
using WonderK.RuleChecker;

Console.WriteLine("Hello, from WonderK.RuleChecker!");

IQueueProcessor queue = new RedisQueueProcessor();

string ruleText = @"
Insurance: Value>1000
Mail: Weight<0.5
Regular: Weight>=0.5
Regular: Weight<10
Heavy: Weight>=10
";

var rules = ParseRules(ruleText);

string streamKey = "parcel-stream";
string groupName = "rule-checker-group";
string consumerName = "rc-" + Guid.NewGuid().ToString();

await queue.Consume(streamKey, groupName, consumerName, async (data) =>
{
    XmlSerializer serializer = new(typeof(Parcel));
    using StringReader reader = new(data);
    Parcel parcel = (Parcel)serializer.Deserialize(reader);
    Console.WriteLine($"Recipient: {parcel.Receipient.Name}, Weight: {parcel.Weight}, Value: {parcel.Value}");

    var departments = GetDepartments(parcel, rules);
    Console.WriteLine("Matching departments: " + string.Join(", ", departments));

    await Send(parcel, departments);
});

static List<Rule> ParseRules(string ruleText)
{
    var rules = new List<Rule>();
    var lines = ruleText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    var regex = new Regex(@"^(?<name>\w+):\s*(?<property>\w+)\s*(?<op>>=|<=|>|<|==)\s*(?<value>[0-9.]+)$");

    foreach (var line in lines)
    {
        var match = regex.Match(line.Trim());
        if (match.Success)
        {
            rules.Add(new Rule
            {
                Name = match.Groups["name"].Value,
                Property = match.Groups["property"].Value,
                Operator = match.Groups["op"].Value,
                Value = double.Parse(match.Groups["value"].Value)
            });
        }
        else
        {
            Console.WriteLine($"Invalid rule format: {line}");
        }
    }

    return rules;
}

static HashSet<string> GetDepartments(Parcel item, List<Rule> rules)
{
    var tags = new HashSet<string>();

    // Group rules by name (e.g., all "Parcel" rules)
    var groupedRules = rules.GroupBy(r => r.Name);

    foreach (var group in groupedRules)
    {
        bool allMatch = group.All(rule => rule.Evaluate(item));
        if (allMatch)
        {
            tags.Add(group.Key);
        }
    }

    return tags;
}

async Task Send(Parcel parcel, HashSet<string> departments)
{
    Package package = new(parcel, departments);

    var streamKey = departments.First() + "-stream";

    string messageId = await queue.Produce(streamKey, package.ToString());

    Console.WriteLine($"Message added to stream with ID: {messageId}");
}