namespace PVZEngine.Modifiers
{
    public class FloatModifier : NumberModifier<float>
    {
        public FloatModifier(PropertyKey propertyName, NumberOperator op, float valueConst, int priority = 0) : base(propertyName, op, valueConst, priority)
        {
        }

        public FloatModifier(PropertyKey propertyName, NumberOperator op, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, op, buffPropertyName, priority)
        {
        }
        public override ModifierCalculator GetCalculator()
        {
            return CalculatorMap.floatCalculator;
        }
    }
}
