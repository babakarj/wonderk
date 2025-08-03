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
            Assert.That(rule.Value, Is.EqualTo("1000"));
        }

        [Test]
        public void ParseRules_DuplicateRules_BothParsed()
        {
            var ruleText = "Dept1:value = 100\nDept1:value = 100";
            var rules = Rule.ParseRules(ruleText);
            Assert.That(rules, Has.Count.EqualTo(2));
        }

        [Test]
        public void ParseRules_QuotedNumericValue_ParsesAsString()
        {
            var ruleText = "Dept1:value = \"1000\"";
            var rules = Rule.ParseRules(ruleText);
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Value, Is.EqualTo("1000"));
        }

        [Test]
        public void ParseRules_ExtremeNumericValues_ParsesCorrectly()
        {
            var ruleText = "Dept1:value > 999999.999999\nDept2:weight < -999999.999999";
            var rules = Rule.ParseRules(ruleText);
            Assert.That(rules, Has.Count.EqualTo(2));
            Assert.That(rules[0].Value, Is.EqualTo("999999.999999"));
            Assert.That(rules[1].Value, Is.EqualTo("-999999.999999"));
        }

        [Test]
        public void ParseRules_MixedLineEndings_ParsesAll()
        {
            var ruleText = "Dept1:value = 1\r\nDept2:weight = 2\nDept3:value = 3";
            var rules = Rule.ParseRules(ruleText);
            Assert.That(rules, Has.Count.EqualTo(3));
            Assert.That(rules[0].Name, Is.EqualTo("Dept1"));
            Assert.That(rules[1].Name, Is.EqualTo("Dept2"));
            Assert.That(rules[2].Name, Is.EqualTo("Dept3"));
        }

        [Test]
        public void ParseRules_InvalidCharactersInName_Throws()
        {
            var ruleText = "Dept$:value = 100";
            Assert.Throws<FormatException>(() => Rule.ParseRules(ruleText));
        }

        [TestCase("Hello")]
        [TestCase("Dept1:value")]
        [TestCase("Dept1:value>")]
        [TestCase("Dept1:value>ten")]
        [TestCase("Dept1:value>abc")]
        [TestCase("Dept1:value>1e10")]
        [TestCase("InvalidRuleFormat\nValid:value = 123.45")]
        [TestCase("Dept1:value > abc\nDept2:weight < xyz\nDept3:value >= 100\nDept4:weight <= 200")]
        public void ParseRules_InvalidRule(string ruleText)
        {
            Assert.Throws<FormatException>(() => Rule.ParseRules(ruleText));
        }

        [TestCase("Dept1:  value   >   1000")]
        [TestCase("Dept1: \t value \t  > \t  1000")]
        public void ParseRules_WhitespaceAroundTokens_ParsesCorrectly(string ruleText)
        {
            var rules = Rule.ParseRules(ruleText);
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Name, Is.EqualTo("Dept1"));
            Assert.That(rules[0].Property, Is.EqualTo("value"));
            Assert.That(rules[0].Operator, Is.EqualTo(">"));
            Assert.That(rules[0].Value, Is.EqualTo("1000"));
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
            Assert.That(rules[0].Value, Is.EqualTo("10"));

            Assert.That(rules[1].Name, Is.EqualTo("Dept2"));
            Assert.That(rules[1].Property, Is.EqualTo("value"));
            Assert.That(rules[1].Operator, Is.EqualTo("<"));
            Assert.That(rules[1].Value, Is.EqualTo("50"));
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

        [TestCase("Invalid:theAnswer = 42")]
        [TestCase("Invalid:theAnswer = 42\nValid:value = 123.45")]
        [TestCase("Valid:value = 123.45\nInvalid:theAnswer = 42")]
        public void ParseRules_InvalidProperty(string ruleText)
        {
            Assert.Throws<ArgumentException>(() => Rule.ParseRules(ruleText));
        }

        [TestCase("Invalid:value != 42")]
        [TestCase("Invalid:value != 42\nValid:value = 123.45")]
        [TestCase("Valid:value = 123.45\nInvalid:value != 42")]
        public void ParseRules_InvalidOperator(string ruleText)
        {
            Assert.Throws<ArgumentException>(() => Rule.ParseRules(ruleText));
        }

        [Test]
        public void ParseRules_EmptyInput_ReturnsEmptyList()
        {
            var ruleText = "";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Is.Empty);
        }

        [Test]
        public void ParseRules_ReceipientName_ParsesCorrectly()
        {
            var ruleText = "Dept1:receipient.name = \"Ford Prefect\"";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(1));
            var rule = rules[0];
            Assert.That(rule.Name, Is.EqualTo("Dept1"));
            Assert.That(rule.Property.ToLower(), Is.EqualTo("receipient.name"));
            Assert.That(rule.Operator, Is.EqualTo("="));
            Assert.That(rule.Value, Is.EqualTo("Ford Prefect"));
        }

        [Test]
        public void ParseRules_EscapedQuotesInStringValue_NotAllowed()
        {
            var ruleText = "Dept1:receipient.name = \"Ford \\\"Prefect\\\"\"";
            Assert.Throws<FormatException>(() => Rule.ParseRules(ruleText));
        }

        [Test]
        public void ParseRules_ReceipientAddressCity_ParsesCorrectly()
        {
            var ruleText = "Dept1:receipient.address.city = \"New York\"";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(1));
            var rule = rules[0];
            Assert.That(rule.Name, Is.EqualTo("Dept1"));
            Assert.That(rule.Property.ToLower(), Is.EqualTo("receipient.address.city"));
            Assert.That(rule.Operator, Is.EqualTo("="));
            Assert.That(rule.Value, Is.EqualTo("New York"));
        }

        [Test]
        public void ParseRules_MixedReceipientProperties_ParsesAllCorrectly()
        {
            var ruleText = "Dept1:receipient.name = \"Alice\"\nDept2:receipient.address.city = \"Seattle\"\nMail: Weight<0.5";

            var rules = Rule.ParseRules(ruleText);

            Assert.That(rules, Has.Count.EqualTo(3));
            Assert.That(rules[0].Property.ToLower(), Is.EqualTo("receipient.name"));
            Assert.That(rules[0].Value, Is.EqualTo("Alice"));
            Assert.That(rules[1].Property.ToLower(), Is.EqualTo("receipient.address.city"));
            Assert.That(rules[1].Value, Is.EqualTo("Seattle"));
            Assert.That(rules[2].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[2].Value, Is.EqualTo("0.5"));
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
            Assert.That(rules[0].Value, Is.EqualTo("1000"));

            Assert.That(rules[1].Name, Is.EqualTo("Mail"));
            Assert.That(rules[1].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[1].Operator, Is.EqualTo("<"));
            Assert.That(rules[1].Value, Is.EqualTo("0.5"));

            Assert.That(rules[2].Name, Is.EqualTo("Regular"));
            Assert.That(rules[2].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[2].Operator, Is.EqualTo(">="));
            Assert.That(rules[2].Value, Is.EqualTo("0.5"));

            Assert.That(rules[3].Name, Is.EqualTo("Regular"));
            Assert.That(rules[3].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[3].Operator, Is.EqualTo("<"));
            Assert.That(rules[3].Value, Is.EqualTo("10"));

            Assert.That(rules[4].Name, Is.EqualTo("Heavy"));
            Assert.That(rules[4].Property.ToLower(), Is.EqualTo("weight"));
            Assert.That(rules[4].Operator, Is.EqualTo(">="));
            Assert.That(rules[4].Value, Is.EqualTo("10"));
        }
    }
}
