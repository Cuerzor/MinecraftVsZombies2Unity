using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Effects
{

    [BuffDefinition(VanillaBuffNames.lightFadeout)]
    public class LightFadeoutBuff : BuffDefinition
    {
        public LightFadeoutBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(ColorModifier.Multiply(VanillaEntityProps.LIGHT_COLOR, PROP_COLOR_MULTIPLIER));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            UpdateMultiplier(buff);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            UpdateMultiplier(buff);
        }
        private void UpdateMultiplier(Buff buff)
        {
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            var alpha = Mathf.Clamp01(entity.Timeout / (float)entity.GetMaxTimeout());
            var color = new Color(1, 1, 1, alpha);
            buff.SetProperty(PROP_COLOR_MULTIPLIER, color);
        }
        public static readonly VanillaBuffPropertyMeta PROP_COLOR_MULTIPLIER = new VanillaBuffPropertyMeta("ColorMultiplier");
    }
}
