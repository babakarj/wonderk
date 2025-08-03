using WonderK.Common.Data;
using WonderK.RuleChecker;

namespace Wonderk.Tests.RuleChecker
{
    [TestFixture]
    public class RuleEvaluateTests
    {
        [TestCase("value", ">", 100, 150, true)]
        [TestCase("value", "<", 100, 50, true)]
        [TestCase("value", ">=", 100, 100, true)]
        [TestCase("value", "<=", 100, 100, true)]
        [TestCase("value", "=", 100, 100, true)]
        [TestCase("value", "=", 100, 100.0000001, true)] // within tolerance
        [TestCase("value", "=", 100, 101, false)]
        [TestCase("weight", ">", 10, 15, true)]
        [TestCase("weight", "<", 10, 5, true)]
        [TestCase("weight", ">=", 10, 10, true)]
        [TestCase("weight", "<=", 10, 10, true)]
        [TestCase("weight", "=", 10, 10, true)]
        [TestCase("weight", "=", 10, 9.999999, true)] // within tolerance
        [TestCase("weight", "=", 10, 11, false)]
        public void Evaluate_ValidPropertiesAndOperators_ReturnsExpected(
            string property, string op, double ruleValue, double parcelValue, bool expected)
        {
            var rule = new Rule("TestRule", property, op, ruleValue);
            var parcel = new Parcel
            {
                Value = property == "value" ? parcelValue : 0,
                Weight = property == "weight" ? parcelValue : 0
            };

            var result = rule.Evaluate(parcel);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetDepartments_CorrectDepartmentsReturned()
        {
            var rules = new List<Rule>
            {
                new("Insurance", "Value", ">", 1000),
                new("Mail", "Weight", "<", 0.5),
                new("Regular", "Weight", ">=", 0.5),
                new("Regular", "Weight", "<", 10),
                new("Heavy", "Weight", ">=", 10)
            };

            // Parcel matches Insurance and Mail
            var parcel1 = new Parcel { Value = 1500, Weight = 0.4 };
            var depts1 = rules.GetDepartments(parcel1);
            Assert.That(depts1, Is.EquivalentTo(new[] { "Insurance", "Mail" }));

            // Parcel matches only Mail
            var parcel3 = new Parcel { Value = 500, Weight = 0.4 };
            var depts3 = rules.GetDepartments(parcel3);
            Assert.That(depts3, Is.EquivalentTo(new[] { "Mail" }));

            // Parcel matches Regular
            var parcel4 = new Parcel { Value = 500, Weight = 5 };
            var depts4 = rules.GetDepartments(parcel4);
            Assert.That(depts4, Is.EquivalentTo(new[] { "Regular" }));

            // Parcel matches Insurance and Regular
            var parcel2 = new Parcel { Value = 1500, Weight = 1.0 };
            var depts2 = rules.GetDepartments(parcel2);
            Assert.That(depts2, Is.EquivalentTo(new[] { "Insurance", "Regular" }));

            // Parcel matches Heavy
            var parcel5 = new Parcel { Value = 500, Weight = 15 };
            var depts5 = rules.GetDepartments(parcel5);
            Assert.That(depts5, Is.EquivalentTo(new[] { "Heavy" }));

            // Parcel matches Insurance and Heavy
            var parcel6 = new Parcel { Value = 2000, Weight = 15 };
            var depts6 = rules.GetDepartments(parcel6);
            Assert.That(depts6, Is.EquivalentTo(new[] { "Insurance", "Heavy" }));
        }

        [Test]
        public void Evaluate_UnknownProperty_ThrowsArgumentException()
        {
            var rule = new Rule("TestRule", "unknown", ">", 10);
            var parcel = new Parcel { Value = 5, Weight = 5 };

            Assert.Throws<ArgumentException>(() => rule.Evaluate(parcel));
        }

        [Test]
        public void Evaluate_UnknownOperator_ThrowsArgumentException()
        {
            var rule = new Rule("TestRule", "value", "!=", 10);
            var parcel = new Parcel { Value = 10, Weight = 5 };

            Assert.Throws<ArgumentException>(() => rule.Evaluate(parcel));
        }
    }
}