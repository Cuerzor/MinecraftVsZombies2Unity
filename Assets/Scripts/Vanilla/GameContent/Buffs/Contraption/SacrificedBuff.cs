using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Modifiers;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.sacrificed)]
    public class SacrificedBuff : BuffDefinition
    {
        public SacrificedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineEntityProps.GRAVITY, NumberOperator.Set, -0.5f, VanillaModifierPriorities.FORCE));
            AddModifier(new Vector3Modifier(VanillaEntityProps.LIGHT_RANGE, NumberOperator.Multiply, PROP_LIGHT_RANGE));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var time = buff.GetProperty<int>(PROP_TIME);
            time++;
            buff.SetProperty(PROP_TIME, time);

            float percentage = time / (float)MAX_TIME;
            buff.SetProperty(PROP_LIGHT_RANGE, Vector3.one * (1 - percentage));

            var entity = buff.GetEntity();
            if (entity == null)
                return;
            entity.RenderRotation += Vector3.forward * time;
            entity.SetShaderFloat("_BurnValue", percentage);

            if (time >= MAX_TIME)
            {
                entity.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta PROP_TIME = new VanillaBuffPropertyMeta("Time");
        public static readonly VanillaBuffPropertyMeta PROP_LIGHT_RANGE = new VanillaBuffPropertyMeta("LightRange");
        public const int MAX_TIME = 30;
    }
}
