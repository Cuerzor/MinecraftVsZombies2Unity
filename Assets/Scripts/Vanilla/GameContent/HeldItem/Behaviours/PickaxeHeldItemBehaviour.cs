using MVZ2.GameContent.Damages;
using MVZ2.GameContent.HeldItem;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.HeldItems;
using PVZEngine.Damages;
using PVZEngine.Entities;

namespace MVZ2.GameContent.HeldItems
{
    public class PickaxeHeldItemBehaviour : ToEntityHeldItemBehaviour
    {
        public PickaxeHeldItemBehaviour(HeldItemDefinition definition) : base(definition)
        {
        }
        protected override bool CanUseOnEntity(Entity entity)
        {
            if (!entity.ExistsAndAlive())
                return false;
            if (entity.Type != EntityTypes.PLANT)
                return false;
            if (entity.NoHeldTarget())
                return false;
            return entity.GetFaction() == entity.Level.Option.LeftFaction && !entity.CannotDig();
        }
        protected override void UseOnEntity(Entity entity)
        {
            var effects = new DamageEffectList(VanillaDamageEffects.SELF_DAMAGE);
            entity.Die(effects);
            if (entity.Level.IsPickaxeCountLimited())
            {
                entity.Level.AddPickaxeRemainCount(-1);
            }
        }
    }
}
