using System;
using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.mummyGas)]
    public class MummyGas : EffectBehaviour
    {

        #region 公有方法
        public MummyGas(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile |=
                EntityCollisionHelper.MASK_PLANT |
                EntityCollisionHelper.MASK_ENEMY |
                EntityCollisionHelper.MASK_BOSS |
                EntityCollisionHelper.MASK_OBSTACLE |
                EntityCollisionHelper.MASK_PROJECTILE |
                EntityCollisionHelper.MASK_EFFECT;
            entity.CollisionMaskFriendly |=
                EntityCollisionHelper.MASK_PLANT |
                EntityCollisionHelper.MASK_ENEMY |
                EntityCollisionHelper.MASK_BOSS |
                EntityCollisionHelper.MASK_OBSTACLE |
                EntityCollisionHelper.MASK_PROJECTILE |
                EntityCollisionHelper.MASK_EFFECT;
            entity.Level.AddLoopSoundEntity(VanillaSoundID.poisonGas, entity.ID);
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            entity.SetModelProperty("Size", entity.GetScaledSize());
            entity.SetModelProperty("Stopped", entity.Timeout <= DISAPPEAR_TIMEOUT);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var entity = collision.Entity;
            var other = collision.Other;
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            if (entity.Timeout <= DISAPPEAR_TIMEOUT)
                return;
            if (other.IsFire())
            {
                entity.PlaySound(VanillaSoundID.fire);
                entity.Timeout = Math.Min(entity.Timeout, DISAPPEAR_TIMEOUT);
                var burning = entity.Level.Spawn(VanillaEffectID.burningGas, entity.Position, entity);
                burning.SetSize(entity.GetSize());

                Burn(entity);
                return;
            }
            if (!other.IsVulnerableEntity() || other.IsDead)
                return;
            if (other.IsUndead())
            {
                other.HealEffects(entity.GetDamage(), entity);
            }
            else if (entity.IsHostile(other))
            {
                var damageEffects = new DamageEffectList(VanillaDamageEffects.IGNORE_ARMOR, VanillaDamageEffects.MUTE);
                other.TakeDamage(entity.GetDamage(), damageEffects, entity);
            }
        }
        private void Burn(Entity gas)
        {
            collisionBuffer.Clear();
            gas.GetCurrentCollisions(collisionBuffer);
            foreach (var collision in collisionBuffer)
            {
                if (!collision.Entity.IsVulnerableEntity())
                    continue;
                if (gas.IsHostile(collision.Entity))
                    continue;
                var damageEffectList = new DamageEffectList(VanillaDamageEffects.FIRE);
                var other = collision.Other;
                var otherCollider = collision.OtherCollider;
                otherCollider.TakeDamage(100, damageEffectList, gas);
            }
        }
        #endregion

        public const int DISAPPEAR_TIMEOUT = 30;
        private List<EntityCollision> collisionBuffer = new List<EntityCollision>();
    }
}