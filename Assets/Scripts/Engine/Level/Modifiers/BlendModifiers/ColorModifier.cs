using UnityEngine;

namespace PVZEngine.Modifiers
{
    public class ColorModifier : PropertyModifier<Color>
    {
        public ColorModifier(PropertyKey propertyName, Color valueConst, int priority = 0) : this(propertyName, BlendOperator.SrcAlpha, BlendOperator.OneMinusSrcAlpha, valueConst, priority)
        {
        }
        public ColorModifier(PropertyKey propertyName, PropertyKey buffPropertyName, int priority = 0) : this(propertyName, BlendOperator.SrcAlpha, BlendOperator.OneMinusSrcAlpha, buffPropertyName, priority)
        {
        }
        public ColorModifier(PropertyKey propertyName, BlendOperator src, BlendOperator dest, Color valueConst, int priority = 0) : base(propertyName, valueConst, priority)
        {
            SrcOperator = src;
            DstOperator = dest;
        }

        public ColorModifier(PropertyKey propertyName, BlendOperator src, BlendOperator dest, PropertyKey buffPropertyName, int priority = 0) : base(propertyName, buffPropertyName, priority)
        {
            SrcOperator = src;
            DstOperator = dest;
        }
        public BlendOperator SrcOperator { get; private set; }
        public BlendOperator DstOperator { get; private set; }
        public static ColorModifier Multiply(PropertyKey propertyName, Color valueConst)
        {
            return new ColorModifier(propertyName, BlendOperator.DstColor, BlendOperator.Zero, valueConst);
        }
        public static ColorModifier Multiply(PropertyKey propertyName, PropertyKey buffPropertyName)
        {
            return new ColorModifier(propertyName, BlendOperator.DstColor, BlendOperator.Zero, buffPropertyName);
        }
        public override ModifierCalculator GetCalculator()
        {
            return CalculatorMap.colorCalculator;
        }
    }
}
