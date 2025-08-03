using System.Text.RegularExpressions;
using WonderK.Common.Data;

namespace WonderK.RuleChecker
{
    public record class Rule(
        string Name,
        string Property,
        string Operator,
        double Value
    )
    {
        public static List<Rule> ParseRules(string ruleText)
        {
            var rules = new List<Rule>();
            var lines = ruleText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex(@"^(?<name>\w+):\s*(?<property>value|weight)\s*(?<op>>=|<=|>|<|=)\s*(?<value>[0-9.]+)$", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = regex.Match(line.Trim());
                if (match.Success)
                {
                    rules.Add(new Rule(
                        match.Groups["name"].Value,
                        match.Groups["property"].Value,
                        match.Groups["op"].Value,
                        double.Parse(match.Groups["value"].Value)
                    ));
                }
                else
                {
                    Console.WriteLine($"Invalid rule format: {line}");
                }
            }

            return rules;
        }

        public bool Evaluate(Parcel parcel)
        {
            double propValue = Property.ToLower() switch
            {
                "value" => parcel.Value,
                "weight" => parcel.Weight,
                _ => throw new ArgumentException($"Unknown property: {Property}")
            };

            return Operator switch
            {
                ">" => propValue > Value,
                ">=" => propValue >= Value,
                "<" => propValue < Value,
                "<=" => propValue <= Value,
                "=" => Math.Abs(propValue - Value) < 1e-6,
                _ => throw new ArgumentException($"Unknown operator: {Operator}")
            };
        }
    }
}
