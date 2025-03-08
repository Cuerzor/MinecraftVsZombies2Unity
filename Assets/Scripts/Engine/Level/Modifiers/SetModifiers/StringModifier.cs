namespace PVZEngine.Modifiers
{
    public class StringModifier : PropertyModifier<string>
    {
        public StringModifier(PropertyKey propertyName, string valueConst, int priority = 0) : base(propertyName, valueConst, priority)
        {
        }

        public StringModifier(PropertyKey propertyName, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, buffPropertyName, priority)
        {
        }
        public override ModifierCalculator GetCalculator()
        {
            return CalculatorMap.stringCalculator;
        }
    }
}
