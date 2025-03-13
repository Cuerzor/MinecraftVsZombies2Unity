using MVZ2.GameContent.Buffs.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.sliceSpark)]
    public class SliceSpark : EffectBehaviour
    {

        #region 公有方法
        public SliceSpark(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.AddBuff<LightFadeoutBuff>();
        }
        #endregion
    }
}