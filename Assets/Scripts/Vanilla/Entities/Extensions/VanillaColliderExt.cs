using PVZEngine.Damages;
using PVZEngine.Entities;

namespace MVZ2.Vanilla.Entities
{
    public static class VanillaColliderExt
    {
        public static bool IsShield(this IEntityCollider collider)
        {
            return collider.Name == EntityCollisionHelper.NAME_SHIELD;
        }
        public static bool IsMain(this IEntityCollider collider)
        {
            return collider.Name == EntityCollisionHelper.NAME_MAIN;
        }
        public static DamageInput GetDamageInput(this IEntityCollider collider, float amount, DamageEffectList effects, Entity source)
        {
            if (collider.IsShield())
            {
                return new DamageInput(amount, effects, collider.Entity, new EntityReferenceChain(source), toBody: false, toShield: true);
            }
            else if (collider.IsMain())
            {
                return new DamageInput(amount, effects, collider.Entity, new EntityReferenceChain(source));
            }
            return null;
        }
        public static DamageOutput TakeDamage(this IEntityCollider collider, float amount, DamageEffectList effects, Entity source)
        {
            return collider.TakeDamage(amount, effects, new EntityReferenceChain(source));
        }
        public static DamageOutput TakeDamage(this IEntityCollider collider, float amount, DamageEffectList effects, EntityReferenceChain source)
        {
            if (collider.IsShield())
            {
                return collider.Entity.TakeDamage(amount, effects, source, toBody: false, toShield: true);
            }
            else if (collider.IsMain())
            {
                return collider.Entity.TakeDamage(amount, effects, source);
            }
            return null;
        }
    }
}
