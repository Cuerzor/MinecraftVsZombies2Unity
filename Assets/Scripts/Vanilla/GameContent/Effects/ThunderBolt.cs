using MVZ2.GameContent.Buffs.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.thunderBolt)]
    public class ThunderBolt : EffectBehaviour
    {

        #region 公有方法
        public ThunderBolt(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.Level.PlaySound(VanillaSoundID.thunder);
            entity.AddBuff<LightFadeoutBuff>();
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            var tint = entity.GetTint();
            tint.a = entity.Timeout < 0 ? 1 : entity.Timeout / 30f;
            entity.SetTint(tint);
        }
        #endregion
    }
}