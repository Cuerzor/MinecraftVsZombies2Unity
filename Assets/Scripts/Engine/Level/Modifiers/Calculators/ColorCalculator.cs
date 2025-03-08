using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PVZEngine.Modifiers
{
    public class ColorCalculator : ModifierCalculator<Color, ColorModifier>
    {
        public override Color CalculateGeneric(Color value, IEnumerable<ModifierContainerItem> modifiers)
        {
            if (modifiers.Count() == 0)
                return value;

            foreach (var item in modifiers.OrderBy(m => m.modifier?.Priority ?? 0))
            {
                var buff = item.container;
                var modi = item.modifier;
                if (modi is not ColorModifier modifier)
                    continue;
                var src = modifier.GetModifierValueGeneric(buff);
                var dst = value;
                var srcOp = modifier.SrcOperator;
                var dstOp = modifier.DstOperator;

                value = Blend(src, dst, srcOp, dstOp);
            }
            return value;
        }
        public static Color Blend(Color src, Color dst, BlendOperator srcOp, BlendOperator dstOp)
        {
            return Blend(src, dst, srcOp, dstOp, srcOp, dstOp);
        }
        public static Color Blend(Color src, Color dst, BlendOperator srcOp, BlendOperator dstOp, BlendOperator srcAOp, BlendOperator dstAOp)
        {
            Color value = Color.black;
            for (int i = 0; i < 3; i++)
            {
                value[i] = src[i] * GetBlendedComponent(src, dst, srcOp, i) + dst[i] * GetBlendedComponent(src, dst, dstOp, i);
            }
            value.a = src.a * GetBlendedComponent(src, dst, srcAOp, 3) + dst.a * GetBlendedComponent(src, dst, dstAOp, 3);
            return value;
        }
        public static float GetBlendedComponent(Color src, Color dst, BlendOperator op, int compIndex)
        {
            switch (op)
            {
                case BlendOperator.One:
                    return 1;
                case BlendOperator.Zero:
                    return 0;
                case BlendOperator.SrcAlpha:
                    return src.a;
                case BlendOperator.DstAlpha:
                    return dst.a;
                case BlendOperator.OneMinusSrcAlpha:
                    return 1 - src.a;
                case BlendOperator.OneMinusDstAlpha:
                    return 1 - dst.a;
                case BlendOperator.SrcColor:
                    return src[compIndex];
                case BlendOperator.DstColor:
                    return dst[compIndex];
                case BlendOperator.OneMinusSrcColor:
                    return 1 - src[compIndex];
                case BlendOperator.OneMinusDstColor:
                    return 1 - dst[compIndex];
            }
            return 0;
        }
    }
}
