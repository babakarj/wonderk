using WonderK.RuleChecker;

namespace Wonderk.Tests.RuleChecker
{
    [TestFixture]
    public class ParseRulesTests
    {
        [Test]
        public void ParseRules_ValidSingleRule_ReturnsRule()
        {
            var ruleText = "Dept1:value > 1000";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(1));
            var rule = rules[0];
            Assert.That(rule.Name, Is.EqualTo("Dept1"));
            Assert.That(rule.Property, Is.EqualTo("value"));
            Assert.That(rule.Operator, Is.EqualTo(">"));
            Assert.That(rule.Value, Is.EqualTo(1000));
        }

        [Test]
        public void ParseRules_MultipleValidRules_ReturnsAllRules()
        {
            var ruleText = "Dept1:weight >= 10\nDept2:value < 50";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(2));

            Assert.That(rules[0].Name, Is.EqualTo("Dept1"));
            Assert.That(rules[0].Property, Is.EqualTo("weight"));
            Assert.That(rules[0].Operator, Is.EqualTo(">="));
            Assert.That(rules[0].Value, Is.EqualTo(10));

            Assert.That(rules[1].Name, Is.EqualTo("Dept2"));
            Assert.That(rules[1].Property, Is.EqualTo("value"));
            Assert.That(rules[1].Operator, Is.EqualTo("<"));
            Assert.That(rules[1].Value, Is.EqualTo(50));
        }


        [Test]
        public void ParseRules_PropertyValue_CaseInsensitive()
        {
            var ruleText = "Dept1:Value > 1000\nDept2:VALUE < 50\nDept3:vaLUe = 123.45\nDept4:value = 42";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(4));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("value"));
            Assert.That(rules[1].Property.ToLower(), Is.EqualTo("value"));
            Assert.That(rules[2].Property.ToLower(), Is.EqualTo("value"));
            Assert.That(rules[3].Property.ToLower(), Is.EqualTo("value"));
        }

        [Test]
        public void ParseRules_PropertyWeight_CaseInsensitive()
        {
            var ruleText = "Dept1:Weight > 10\nDept2:WEIGHT < 5\nDept3:weIGHt = 7.5\nDept4:weight = 42";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(4));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[1].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[2].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[3].Property.ToLower(), Is.EqualTo("weight"));
        }

        [Test]
        public void ParseRules_InvalidRule_IgnoresInvalidLine()
        {
            var ruleText = "InvalidRuleFormat\nValid:value = 123.45";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Name, Is.EqualTo("Valid"));
            Assert.That(rules[0].Property, Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo("="));
            Assert.That(rules[0].Value, Is.EqualTo(123.45));
        }

        [Test]
        public void ParseRules_InvalidOperator()
        {
            // Operator != is not supported, should be ignored
            var ruleText = "Invalid:value != 42\nValid:value = 123.45";

            var rules = Rule.ParseRules(ruleText);

            // Invalid operator should be ignored
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Name, Is.EqualTo("Valid"));
            Assert.That(rules[0].Property, Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo("="));
            Assert.That(rules[0].Value, Is.EqualTo(123.45));
        }

        [Test]
        public void ParseRules_InvalidProperty()
        {
            // Property 'theAnswer' is not valid, should be ignored
            var ruleText = "Invalid:theAnswer = 42\nValid:value = 123.45";

            var rules = Rule.ParseRules(ruleText);
            
            Assert.That(rules, Has.Count.EqualTo(1));

            Assert.That(rules[0].Name, Is.EqualTo("Valid"));
            Assert.That(rules[0].Property, Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo("="));
            Assert.That(rules[0].Value, Is.EqualTo(123.45));
        }

        [Test]
        public void ParseRules_EmptyInput_ReturnsEmptyList()
        {
            var ruleText = "";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Is.Empty);
        }

        [Test]
        public void ParseRules_ValueIsNotNumber_IgnoresRule()
        {
            var ruleText = "Dept1:value > abc\nDept2:value < 50";

            var rules = Rule.ParseRules(ruleText);

            // Only the valid rule should be parsed
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Name, Is.EqualTo("Dept2"));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo("<"));
            Assert.That(rules[0].Value, Is.EqualTo(50));
        }

        [Test]
        public void ParseRules_WeightIsNotNumber_IgnoresRule()
        {
            var ruleText = "Dept1:weight >= ten\nDept2:weight < 5";

            var rules = Rule.ParseRules(ruleText);

            // Only the valid rule should be parsed
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Name, Is.EqualTo("Dept2"));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[0].Operator, Is.EqualTo("<"));
            Assert.That(rules[0].Value, Is.EqualTo(5));
        }

        [Test]
        public void ParseRules_MixedInvalidNumericValues_OnlyValidRulesParsed()
        {
            var ruleText = "Dept1:value > abc\nDept2:weight < xyz\nDept3:value >= 100\nDept4:weight <= 200";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(2));
            Assert.That(rules[0].Name, Is.EqualTo("Dept3"));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo(">="));
            Assert.That(rules[0].Value, Is.EqualTo(100));

            Assert.That(rules[1].Name, Is.EqualTo("Dept4"));
            Assert.That(rules[1].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[1].Operator, Is.EqualTo("<="));
            Assert.That(rules[1].Value, Is.EqualTo(200));
        }

        [Test]
        public void ParseRules_ComplexBusinessRules_AllParsedCorrectly()
        {
            var ruleText = @"Insurance: Value>1000
Mail: Weight<0.5
Regular: Weight>=0.5
Regular: Weight<10
Heavy: Weight>=10";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(5));

            Assert.That(rules[0].Name, Is.EqualTo("Insurance"));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo(">"));
            Assert.That(rules[0].Value, Is.EqualTo(1000));

            Assert.That(rules[1].Name, Is.EqualTo("Mail"));
            Assert.That(rules[1].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[1].Operator, Is.EqualTo("<"));
            Assert.That(rules[1].Value, Is.EqualTo(0.5));

            Assert.That(rules[2].Name, Is.EqualTo("Regular"));
            Assert.That(rules[2].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[2].Operator, Is.EqualTo(">="));
            Assert.That(rules[2].Value, Is.EqualTo(0.5));

            Assert.That(rules[3].Name, Is.EqualTo("Regular"));
            Assert.That(rules[3].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[3].Operator, Is.EqualTo("<"));
            Assert.That(rules[3].Value, Is.EqualTo(10));

            Assert.That(rules[4].Name, Is.EqualTo("Heavy"));
            Assert.That(rules[4].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[4].Operator, Is.EqualTo(">="));
            Assert.That(rules[4].Value, Is.EqualTo(10));
        }
    }
}
