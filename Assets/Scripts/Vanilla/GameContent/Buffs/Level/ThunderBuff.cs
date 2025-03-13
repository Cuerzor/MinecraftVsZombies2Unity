using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.Level.thunder)]
    public class ThunderBuff : BuffDefinition
    {
        public ThunderBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new ColorModifier(VanillaAreaProps.BACKGROUND_LIGHT, PROP_LIGHT_BLEND));
            AddModifier(new ColorModifier(VanillaAreaProps.GLOBAL_LIGHT, PROP_LIGHT_BLEND));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_LIGHT_BLEND, new Color(1, 1, 1, 1));
            buff.SetProperty(PROP_TIMEOUT, MAX_TIMEOUT);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            var light = timeout / (float)MAX_TIMEOUT;
            buff.SetProperty(PROP_LIGHT_BLEND, new Color(1, 1, 1, light));
            if (timeout <= 0)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta PROP_LIGHT_BLEND = new VanillaBuffPropertyMeta("LightBlend");
        public static readonly VanillaBuffPropertyMeta PROP_TIMEOUT = new VanillaBuffPropertyMeta("Timeout");
        public const int MAX_TIMEOUT = 30;
    }
}
