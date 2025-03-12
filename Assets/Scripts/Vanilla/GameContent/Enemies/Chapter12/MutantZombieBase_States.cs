using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.Vanilla.Enemies
{
    public abstract partial class MutantZombieBase : EnemyBehaviour
    {
        #region ×´Ì¬»ú
        private class MutantZombieStateMachine : EntityStateMachine
        {
            public MutantZombieStateMachine()
            {
                AddState(new IdleState());
                AddState(new WalkState());
                AddState(new AttackState());
                AddState(new ThrowState());
                AddState(new DeathState());
            }
        }
        #endregion

        private static void UpdateState(Entity zombie)
        {
            var targetState = STATE_WALK;
            if (zombie.IsDead)
            {
                targetState = STATE_DEATH;
            }
            else if (zombie.IsPreviewEnemy())
            {
                targetState = STATE_IDLE;
            }
            else if (CheckThrow(zombie))
            {
                targetState = STATE_THROW;
            }
            else if (CheckAttackTarget(zombie))
            {
                targetState = STATE_ATTACK;
            }
            if (zombie.State != targetState)
            {
                stateMachine.StartState(zombie, targetState);
            }
        }

        #region ¿ÕÏÐ
        public class IdleState : EntityStateMachineState
        {
            public IdleState() : base(STATE_IDLE) { }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                UpdateState(entity);
            }
        }
        #endregion

        #region ÐÐ×ß
        public class WalkState : EntityStateMachineState
        {
            public WalkState() : base(STATE_WALK) { }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                entity.UpdateWalkVelocity();
                UpdateState(entity);
            }
        }
        #endregion

        #region ¹¥»÷
        private static bool CheckAttackTarget(Entity zombie)
        {
            return attackDetector.DetectExists(zombie);
        }
        public class AttackState : EntityStateMachineState
        {
            public AttackState() : base(STATE_ATTACK) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.ResetTime(40);
                entity.PlaySound(VanillaSoundID.mutantCry);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run(entity.GetAttackSpeed());
                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_START:
                        if (subStateTimer.Expired)
                        {
                            subStateTimer.ResetTime(20);
                            stateMachine.SetSubState(entity, SUBSTATE_ATTACKED);
                            Hammer(entity);
                        }
                        break;
                    case SUBSTATE_ATTACKED:
                        if (subStateTimer.Expired)
                        {
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;
                }
            }
            private void Hammer(Entity self)
            {
                var level = self.Level;
                if (self.IsOnGround)
                {
                    var x = self.Position.x + self.GetFacingX() * self.GetRange() * 0.5f;
                    var z = self.Position.z;
                    var soundID = VanillaSoundID.thump;
                    if (level.IsWaterAt(x, z))
                    {
                        soundID = VanillaSoundID.splashBig;
                    }
                    self.PlaySound(soundID);
                    level.ShakeScreen(10, 0, 15);
                }

                detectBuffer.Clear();
                hammerDetector.DetectMultiple(self, detectBuffer);

                foreach (var collider in detectBuffer)
                {
                    var target = collider.Entity;
                    if (target.IsOnWater())
                    {
                        var damageResult = collider.TakeDamage(target.GetTakenCrushDamage(), new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.REMOVE_ON_DEATH, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), self);

                        target.PlaySplashEffect(Vector3.one * 3);
                    }
                    else
                    {
                        var damageResult = collider.TakeDamage(target.GetTakenCrushDamage(), new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), self);
                        if (damageResult != null && damageResult.BodyResult != null && damageResult.BodyResult.Fatal && damageResult.BodyResult.Entity.Type == EntityTypes.PLANT)
                        {
                            damageResult.Entity.PlaySound(VanillaSoundID.smash);
                        }
                    }
                }
            }
            public const int SUBSTATE_START = 0;
            public const int SUBSTATE_ATTACKED = 1;
            private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        }
        #endregion

        #region Í¶ÖÀ
        private static bool CheckThrow(Entity zombie)
        {
            if (!HasImp(zombie))
                return false;
            if (zombie.Health >= zombie.GetMaxHealth() * 0.5f)
                return false;
            var level = zombie.Level;
            var midColumn = level.GetMaxColumnCount() / 2 + 1;
            if (zombie.IsFacingLeft())
            {
                return zombie.Position.x > level.GetColumnX(midColumn);
            }
            return zombie.Position.x < level.GetColumnX(midColumn);
        }
        public class ThrowState : EntityStateMachineState
        {
            public ThrowState() : base(STATE_THROW) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.ResetTime(35);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run(entity.GetAttackSpeed());
                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_START:
                        if (subStateTimer.Expired)
                        {
                            entity.PlaySound(VanillaSoundID.swing);
                            SetHasImp(entity, false);

                            var impPos = entity.Position + new Vector3(entity.GetFacingX() * 24, 155, 0);
                            var impVel = new Vector3(entity.GetFacingX() * 20, 0, 0);
                            var imp = entity.Spawn(VanillaEnemyID.imp, impPos);
                            imp.PlaySound(VanillaSoundID.impLaugh);
                            imp.SetFactionAndDirection(entity.GetFaction());
                            imp.Velocity = impVel;

                            subStateTimer.ResetTime(10);
                            stateMachine.SetSubState(entity, SUBSTATE_THROWN);
                        }
                        break;
                    case SUBSTATE_THROWN:
                        if (subStateTimer.Expired)
                        {
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;
                }
            }
            public const int SUBSTATE_START = 0;
            public const int SUBSTATE_THROWN = 1;
        }
        #endregion
        #region ËÀÍö
        public class DeathState : EntityStateMachineState
        {
            public DeathState() : base(STATE_DEATH) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.ResetTime(120);

                var weapon = entity.Spawn(VanillaEffectID.mutantZombieWeapon, entity.Position + new Vector3(0, 71, 0));
                weapon.SetAnimationInt("Weapon", GetWeapon(entity));
            }
            public override void OnUpdateLogic(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateLogic(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run();
                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_START:
                        if (subStateTimer.Expired)
                        {
                            entity.Level.ShakeScreen(10, 0, 10);
                            entity.PlaySound(VanillaSoundID.thump);
                            subStateTimer.ResetTime(30);
                            stateMachine.SetSubState(entity, SUBSTATE_DROP);
                        }
                        break;
                    case SUBSTATE_DROP:
                        if (subStateTimer.Expired)
                        {
                            var smoke = entity.Spawn(VanillaEffectID.smoke, entity.Position);
                            smoke.SetSize(entity.GetSize());
                            entity.Remove();
                        }
                        break;
                }
            }
            public const int SUBSTATE_START = 0;
            public const int SUBSTATE_DROP = 1;
        }
        #endregion

        public const int STATE_IDLE = VanillaEntityStates.MUTANT_ZOMBIE_IDLE;
        public const int STATE_WALK = VanillaEntityStates.MUTANT_ZOMBIE_WALK;
        public const int STATE_ATTACK = VanillaEntityStates.MUTANT_ZOMBIE_ATTACK;
        public const int STATE_THROW = VanillaEntityStates.MUTANT_ZOMBIE_THROW;
        public const int STATE_DEATH = VanillaEntityStates.MUTANT_ZOMBIE_DEATH;
    }

}