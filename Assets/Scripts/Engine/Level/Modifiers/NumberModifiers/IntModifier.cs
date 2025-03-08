namespace PVZEngine.Modifiers
{
    public class IntModifier : NumberModifier<int>
    {
        public IntModifier(PropertyKey propertyName, NumberOperator op, int valueConst, int priority = 0) : base(propertyName, op, valueConst, priority)
        {
        }

        public IntModifier(PropertyKey propertyName, NumberOperator op, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, op, buffPropertyName, priority)
        {
        }
        public override ModifierCalculator GetCalculator()
        {
            return CalculatorMap.intCalculator;
        }
    }
}
