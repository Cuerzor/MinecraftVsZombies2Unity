using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Modifiers;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.Boss.seijaGap)]
    public class SeijaGapBuff : BuffDefinition
    {
        public SeijaGapBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new IntModifier(EngineEntityProps.COLLISION_DETECTION, NumberOperator.Set, EntityCollisionHelper.DETECTION_IGNORE, VanillaModifierPriorities.FORCE));
            AddModifier(new BooleanModifier(EngineEntityProps.INVINCIBLE, true));
            AddModifier(new Vector3Modifier(VanillaEntityProps.SHADOW_SCALE, NumberOperator.Multiply, PROP_SHADOW_SCALE));
        }

        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var time = buff.GetProperty<int>(PROP_TIME);
            time++;
            time = Mathf.Clamp(time, 0, MAX_TIME);
            buff.SetProperty(PROP_TIME, time);
            buff.SetProperty(PROP_SHADOW_SCALE, Vector3.one * Mathf.Clamp01(1 - time / (float)TIME_THRESOLD));
        }
        public const int TIME_THRESOLD = 20;
        public const int MAX_TIME = 40;
        public static readonly VanillaBuffPropertyMeta PROP_TIME = new VanillaBuffPropertyMeta("Time");
        public static readonly VanillaBuffPropertyMeta PROP_SHADOW_SCALE = new VanillaBuffPropertyMeta("ShadowScale");
    }
}
