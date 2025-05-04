﻿using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.lightningOrb)]
    public class LightningOrb : ContraptionBehaviour
    {
        public LightningOrb(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_PROJECTILE_HIT, PreProjectileHitCallback);
        }
        protected override void UpdateLogic(Entity contraption)
        {
            base.UpdateLogic(contraption);
            contraption.SetAnimationFloat("Damaged", 1 - contraption.Health / contraption.GetMaxHealth());
            contraption.SetAnimationBool("Absorbing", contraption.HasBuff<LightningOrbEvokedBuff>());
        }
        private void PreProjectileHitCallback(ProjectileHitInput hit, DamageInput damage)
        {
            if (NamespaceID.IsValid(damage.ShieldTarget))
                return;
            var orb = hit.Other;
            if (!orb.Definition.HasBehaviour(this))
                return;

            var projectile = hit.Projectile;

            orb.HealEffects(100, projectile);
            foreach (var buff in orb.GetBuffs<LightningOrbEvokedBuff>())
            {
                LightningOrbEvokedBuff.AddTakenDamage(buff, damage.Amount);
            }
            projectile.Remove();
            hit.Cancel();
            orb.PlaySound(VanillaSoundID.energyShieldHit);
        }
        public override bool CanEvoke(Entity entity)
        {
            if (entity.HasBuff<LightningOrbEvokedBuff>())
                return false;
            return base.CanEvoke(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.PlaySound(VanillaSoundID.lightningAttack);
            entity.AddBuff<LightningOrbEvokedBuff>();
        }
        public const float HEAL_AMOUNT = 100;
    }
}
