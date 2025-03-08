namespace PVZEngine.Modifiers
{
    public class NamespaceIDModifier : PropertyModifier<NamespaceID>
    {
        public NamespaceIDModifier(PropertyKey propertyName, NamespaceID valueConst, int priority = 0) : base(propertyName, valueConst, priority)
        {
        }

        public NamespaceIDModifier(PropertyKey propertyName, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, buffPropertyName, priority)
        {
        }
        public override ModifierCalculator GetCalculator()
        {
            return CalculatorMap.namespaceIDCalculator;
        }
    }
}
