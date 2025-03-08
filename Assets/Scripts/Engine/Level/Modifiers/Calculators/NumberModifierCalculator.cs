using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PVZEngine.Modifiers
{
    public abstract class NumberModifierCalculator<T> : ModifierCalculator<T, NumberModifier<T>>
    {
        public override T CalculateGeneric(T value, IEnumerable<ModifierContainerItem> modifiers)
        {
            if (modifiers.Count() == 0)
                return value;

            var grouped = modifiers.GroupBy(m => m.modifier?.Priority ?? 0).OrderBy(g => g.Key);
            foreach (var layerModifiers in grouped)
            {
                T setValue = value;
                T addValue = default;
                T multiple = GetDefaultMultiple();
                T multiply = GetDefaultMultiple();
                foreach (var modi in layerModifiers)
                {
                    var buff = modi.container;
                    if (modi.modifier is not NumberModifier<T> modifier)
                        continue;
                    var modifierValue = modifier.GetModifierValueGeneric(buff);
                    switch (modifier.Operator)
                    {
                        case NumberOperator.Set:
                            setValue = modifierValue;
                            break;
                        case NumberOperator.Add:
                            addValue = AddValue(addValue, modifierValue);
                            break;
                        case NumberOperator.AddMultiplie:
                            multiple = AddValue(multiple, modifierValue);
                            break;
                        case NumberOperator.Multiply:
                            multiply = MultiplyValue(multiply, modifierValue);
                            break;
                    }
                }
                value = setValue;
                value = AddValue(value, addValue);
                value = MultiplyValue(value, multiple);
                value = MultiplyValue(value, multiply);
            }
            return value;
        }
        protected abstract T GetDefaultMultiple();
        protected abstract T AddValue(T value1, T value2);
        protected abstract T MultiplyValue(T value1, T value2);
    }
    public class IntCalculator : NumberModifierCalculator<int>
    {
        protected override int GetDefaultMultiple()
        {
            return 1;
        }

        protected override int AddValue(int value1, int value2)
        {
            return value1 + value2;
        }

        protected override int MultiplyValue(int value1, int value2)
        {
            return value1 * value2;
        }
    }
    public class FloatCalculator : NumberModifierCalculator<float>
    {
        protected override float GetDefaultMultiple()
        {
            return 1;
        }

        protected override float AddValue(float value1, float value2)
        {
            return value1 + value2;
        }

        protected override float MultiplyValue(float value1, float value2)
        {
            return value1 * value2;
        }
    }
    public class Vector3Calculator : NumberModifierCalculator<Vector3>
    {
        protected override Vector3 GetDefaultMultiple()
        {
            return Vector3.one;
        }

        protected override Vector3 AddValue(Vector3 value1, Vector3 value2)
        {
            return value1 + value2;
        }

        protected override Vector3 MultiplyValue(Vector3 value1, Vector3 value2)
        {
            return Vector3.Scale(value1, value2);
        }
    }
}
