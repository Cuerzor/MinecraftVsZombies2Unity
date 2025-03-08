using System.Collections.Generic;
using System.Linq;

namespace PVZEngine.Modifiers
{
    public abstract class SetModifierCalculator<T> : ModifierCalculator<T, PropertyModifier<T>>
    {
        public override T CalculateGeneric(T value, IEnumerable<ModifierContainerItem> modifiers)
        {
            if (modifiers.Count() == 0)
                return value;

            var validModifiers = modifiers.Where(m => m.modifier is PropertyModifier<T>);
            if (validModifiers.Count() == 0)
                return value;
            var last = validModifiers.OrderBy(m => m.modifier.Priority).LastOrDefault();
            var buff = last.container;
            var modifier = last.modifier as PropertyModifier<T>;
            return modifier.GetModifierValueGeneric(buff);
        }
    }
    public class BooleanCalculator : SetModifierCalculator<bool>
    {
    }
    public class NamespaceIDCalculator : SetModifierCalculator<NamespaceID>
    {
    }
    public class StringCalculator : SetModifierCalculator<string>
    {
    }
}
