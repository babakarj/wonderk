using System.Xml.Serialization;
using WonderK.Common.Data;
using WonderK.Common.Libraries;
using WonderK.RuleChecker;

Console.WriteLine("Hello, from WonderK.RuleChecker!");

string ruleText = File.ReadAllText("Rules.txt");
List<Rule> rules = Rule.ParseRules(ruleText);

IQueueProcessor queue = new RedisQueueProcessor();

string streamKey = "parcel-stream";
string groupName = "rule-checker-group";
string consumerName = "rc-" + Guid.NewGuid().ToString();

await queue.Consume(streamKey, groupName, consumerName, async (data) =>
{
    XmlSerializer serializer = new(typeof(Parcel));
    using StringReader reader = new(data);
    Parcel parcel = (Parcel)serializer.Deserialize(reader);
    Console.WriteLine($"Recipient: {parcel.Receipient.Name}, Weight: {parcel.Weight}, Value: {parcel.Value}");

    var departments = rules.GetDepartments(parcel);
    Console.WriteLine("Matching departments: " + string.Join(", ", departments));

    await Send(parcel, departments);
});

async Task Send(Parcel parcel, HashSet<string> departments)
{
    Package package = new(parcel, departments);

    string streamKey = departments.First() + "-stream";

    string messageId = await queue.Produce(streamKey, package.ToString());

    Console.WriteLine($"Message added to stream with ID: {messageId}");
}