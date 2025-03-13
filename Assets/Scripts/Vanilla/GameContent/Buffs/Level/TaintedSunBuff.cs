using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.Level.taintedSun)]
    public class TaintedSunBuff : BuffDefinition
    {
        public TaintedSunBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(ColorModifier.Multiply(VanillaAreaProps.GLOBAL_LIGHT, PROP_LIGHT_MULTIPLIER));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_LIGHT_MULTIPLIER, Color.black);
            buff.SetProperty(PROP_TIMEOUT, MAX_TIMEOUT);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            var light = Mathf.Clamp01(1 - timeout / (float)FADE_MAX_TIMEOUT);
            buff.SetProperty(PROP_LIGHT_MULTIPLIER, new Color(light, light, light, 1));
            if (timeout <= 0)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta PROP_LIGHT_MULTIPLIER = new VanillaBuffPropertyMeta("LightMultiplier");
        public static readonly VanillaBuffPropertyMeta PROP_TIMEOUT = new VanillaBuffPropertyMeta("Timeout");
        public const int MAX_TIMEOUT = 330;
        public const int FADE_MAX_TIMEOUT = 30;
    }
}
