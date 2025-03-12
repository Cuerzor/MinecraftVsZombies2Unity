using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.fireBreath)]
    public class FireBreath : EffectBehaviour
    {
        #region 公有方法
        public FireBreath(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new Vector3Modifier(VanillaEntityProps.LIGHT_RANGE, NumberOperator.Multiply, PROP_LIGHT_RANGE_MULTIPLIER));
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY | EntityCollisionHelper.MASK_OBSTACLE | EntityCollisionHelper.MASK_BOSS;
            entity.Level.AddLoopSoundEntity(VanillaSoundID.fireBreath, entity.ID);
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            var parent = entity.Parent;
            bool existing = parent != null && parent.Exists();
            if (existing)
            {
                entity.Timeout = MAX_TIMEOUT;
                var cooldown = GetDamageCooldown(entity);
                cooldown--;
                if (cooldown <= 0)
                {
                    collisionBuffer.Clear();
                    entity.GetCurrentCollisions(collisionBuffer);
                    foreach (var collision in collisionBuffer)
                    {
                        collision.OtherCollider.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.FIRE), entity);
                    }
                    cooldown = DAMAGE_COOLDOWN;
                }
                SetDamageCooldown(entity, cooldown);
            }
            entity.SetAnimationBool("Burning", existing);
            var lightPercentage = Mathf.Max(0, (entity.Timeout / (float)MAX_TIMEOUT) * 3 - 2);
            entity.SetProperty(PROP_LIGHT_RANGE_MULTIPLIER, Vector3.one * lightPercentage);
        }
        public static int GetDamageCooldown(Entity entity) => entity.GetBehaviourField<int>(ID, PROP_DAMAGE_COOLDOWN);
        public static void SetDamageCooldown(Entity entity, int value) => entity.SetBehaviourField(ID, PROP_DAMAGE_COOLDOWN, value);
        #endregion
        private static readonly NamespaceID ID = VanillaEffectID.fireBreath;
        public const int DAMAGE_COOLDOWN = 15;
        public const int MAX_TIMEOUT = 30;
        public static readonly VanillaEntityPropertyMeta PROP_LIGHT_RANGE_MULTIPLIER = new VanillaEntityPropertyMeta("LightRangeMultiplier");
        private static readonly VanillaEntityPropertyMeta PROP_DAMAGE_COOLDOWN = new VanillaEntityPropertyMeta("DamageCooldown");
        private List<EntityCollision> collisionBuffer = new List<EntityCollision>();
    }
}