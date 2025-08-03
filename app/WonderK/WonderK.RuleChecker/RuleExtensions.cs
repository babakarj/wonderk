using System.Collections.Immutable;
using WonderK.Common.Data;

namespace WonderK.RuleChecker
{
    public static class RuleExtensions
    {
        public static HashSet<string> GetDepartments(this ImmutableList<Rule> rules, Parcel item)
        {
            var departments = new HashSet<string>();

            // Group rules by name (e.g., all "Parcel" rules)
            var groupedRules = rules.GroupBy(r => r.Name);

            foreach (var group in groupedRules)
            {
                bool allMatch = group.All(rule => rule.Evaluate(item));
                if (allMatch)
                {
                    departments.Add(group.Key);
                }
            }

            return departments;
        }
    }
}
