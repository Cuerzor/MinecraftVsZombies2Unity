using UnityEngine;

namespace PVZEngine.Modifiers
{
    public class Vector3Modifier : NumberModifier<Vector3>
    {
        public Vector3Modifier(PropertyKey propertyName, NumberOperator op, Vector3 valueConst, int priority = 0) : base(propertyName, op, valueConst, priority)
        {
        }

        public Vector3Modifier(PropertyKey propertyName, NumberOperator op, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, op, buffPropertyName, priority)
        {
        }
        public override ModifierCalculator GetCalculator()
        {
            return CalculatorMap.vector3Calculator;
        }
    }
}
