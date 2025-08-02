using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WonderK.Common.Data;

namespace WonderK.RuleChecker
{
    public class Rule
    {
        public string Name;
        public string Property;
        public string Operator;
        public double Value;

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
                "==" => Math.Abs(propValue - Value) < 1e-6,
                _ => throw new ArgumentException($"Unknown operator: {Operator}")
            };
        }
    }
}
