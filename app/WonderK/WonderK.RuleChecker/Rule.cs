using System.Globalization;
using System.Text.RegularExpressions;
using WonderK.Common.Data;

namespace WonderK.RuleChecker
{
    public class Rule
    {
        public string Name { get; }
        public string Property { get; }
        public string Operator { get; }
        public string Value { get; }

        public Rule(string name, string property, string op, string value)
        {
            Name = name;
            Property = property;
            Operator = op;
            Value = value;

            switch (property.ToLowerInvariant())
            {
                case "value":
                case "weight":
                    NumericGetter = property.Equals("value", StringComparison.InvariantCultureIgnoreCase)
                        ? p => p.Value
                        : p => p.Weight;
                    NumericValue = double.Parse(value, CultureInfo.InvariantCulture);
                    NumericOp = BuildNumericOp(op);
                    break;

                case "receipient.name":
                case "receipient.address.city":
                    StringGetter = property.Equals("receipient.name", StringComparison.InvariantCultureIgnoreCase)
                        ? p => p.Receipient.Name
                        : p => p.Receipient.Address.City;
                    StringValue = value;
                    StringOp = BuildStringOp(op);
                    break;

                default:
                    throw new ArgumentException($"Unknown property: {property}");
            }
        }

        private Func<Parcel, double>? NumericGetter { get; }
        private Func<Parcel, string>? StringGetter { get; }
        private double? NumericValue { get; }
        private string? StringValue { get; }
        private Func<double, double, bool>? NumericOp { get; }
        private Func<string, string, bool>? StringOp { get; }

        private static Func<double, double, bool> BuildNumericOp(string op) => op switch
        {
            ">" => (l, r) => l > r,
            ">=" => (l, r) => l >= r,
            "<" => (l, r) => l < r,
            "<=" => (l, r) => l <= r,
            "=" => (l, r) => Math.Abs(l - r) < 1e-6,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };

        private static Func<string, string, bool> BuildStringOp(string op) => op switch
        {
            "=" => (l, r) => string.Equals(l, r, StringComparison.OrdinalIgnoreCase),
            "contains" => (l, r) => l.Contains(r, StringComparison.OrdinalIgnoreCase),
            _ => throw new ArgumentException($"Operator '{op}' not supported for strings.")
        };

        public bool Evaluate(Parcel p) => NumericGetter != null
            ? NumericOp!(NumericGetter(p), NumericValue!.Value)
            : StringOp!(StringGetter!(p), StringValue!);

        public static List<Rule> ParseRules(string ruleText)
        {
            var rules = new List<Rule>();
            var lines = ruleText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex(
                @"^(?<name>\w+):\s*(?<property>[\w\.]+)\s*(?<op>[^\d\s""]+)\s*(?<value>(-?[0-9.]+|""[^""]*""))$",
                RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = regex.Match(line.Trim());
                if (match.Success)
                {
                    rules.Add(new Rule(
                        match.Groups["name"].Value,
                        match.Groups["property"].Value,
                        match.Groups["op"].Value,
                        match.Groups["value"].Value.Trim('"')
                    ));
                }
                else
                {
                    throw new FormatException($"Invalid rule format: {line}");
                }
            }

            return rules;
        }
    }
}
