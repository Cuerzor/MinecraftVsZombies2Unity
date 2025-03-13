using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.Level.darkMatterDark)]
    public class DarkMatterDarkBuff : BuffDefinition
    {
        public DarkMatterDarkBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(ColorModifier.Multiply(VanillaAreaProps.GLOBAL_LIGHT, PROP_LIGHT_MULTIPLIER));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_LIGHT_MULTIPLIER, Color.white);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var multiplier = buff.GetProperty<Color>(PROP_LIGHT_MULTIPLIER);

            multiplier.r = Mathf.Clamp(multiplier.r - DARKNESS_SPEED, MIN_LIGHT, 1);
            multiplier.g = Mathf.Clamp(multiplier.g - DARKNESS_SPEED, MIN_LIGHT, 1);
            multiplier.b = Mathf.Clamp(multiplier.b - DARKNESS_SPEED, MIN_LIGHT, 1);

            buff.SetProperty(PROP_LIGHT_MULTIPLIER, multiplier);
        }
        public static readonly VanillaBuffPropertyMeta PROP_LIGHT_MULTIPLIER = new VanillaBuffPropertyMeta("LightMultiplier");
        public const float DARKNESS_SPEED = 0.03f;
        public const float MIN_LIGHT = 0.02f;
    }
}
