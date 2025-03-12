using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.spikeBall)]
    public class SpikeBall : ProjectileBehaviour
    {
        public SpikeBall(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile = 0;
            entity.CollisionMaskFriendly = 0;
        }
        public override void PostContactGround(Entity entity, Vector3 velocity)
        {
            base.PostContactGround(entity, velocity);
            Vector3 position = entity.Position;
            position.y = entity.GetGroundY();
            var spike = entity.Level.Spawn(VanillaEffectID.giantSpike, position, entity);
            spike.PlaySound(VanillaSoundID.giantSpike);
            spike.SetFaction(entity.GetFaction());
            spike.SetDamage(entity.GetDamage());
            foreach (Entity target in entity.Level.FindEntities(e => IsEnemyAndInRange(spike, e)))
            {
                target.TakeDamage(spike.GetDamage(), new DamageEffectList(VanillaDamageEffects.IGNORE_ARMOR), spike);
            }
            entity.Remove();
        }
        private bool IsEnemyAndInRange(Entity self, Entity target)
        {
            if (!target.IsVulnerableEntity())
                return false;
            if (!Detection.CanDetect(target))
                return false;
            if (!self.IsHostile(target))
                return false;
            if (!self.GetBounds().Intersects(target.GetBounds()))
                return false;
            return true;
        }
    }
}
