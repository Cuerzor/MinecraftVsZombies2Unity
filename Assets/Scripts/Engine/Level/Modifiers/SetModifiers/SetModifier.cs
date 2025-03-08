namespace PVZEngine.Modifiers
{
    public abstract class SetModifier<T> : PropertyModifier<T>
    {
        protected SetModifier(PropertyKey propertyName, T constValue, int priority = 0) : base(propertyName, constValue, priority)
        {
        }

        protected SetModifier(PropertyKey propertyName, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, buffPropertyName, priority)
        {
        }
    }
}
