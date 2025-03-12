using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Difficulties;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Shells;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    public partial class Nightmareaper : BossBehaviour
    {
        #region 状态机
        private class NightmareaperStateMachine : EntityStateMachine
        {
            public NightmareaperStateMachine()
            {
                AddState(new AppearState());
                AddState(new IdleState());
                AddState(new JabState());
                AddState(new SpinState());
                AddState(new DarknessState());
                AddState(new ResurrectState());
                AddState(new EnragedState());
                AddState(new DeathState());
            }
        }
        #endregion
        private static void StopSpinSound(Entity entity)
        {
            entity.Level.RemoveLoopSoundEntity(VanillaSoundID.wheelOfDeathLoop, entity.ID);
        }
        private static int GetOutbound(Entity entity)
        {
            const float leftX = VanillaLevelExt.LEFT_BORDER + 40;
            const float rightX = VanillaLevelExt.RIGHT_BORDER - 40;
            float topY = entity.Level.GetGridTopZ();
            float bottomY = entity.Level.GetGridBottomZ();
            if (entity.Position.x <= leftX)
            {
                return 0;
            }
            else if (entity.Position.z <= bottomY)
            {
                return 1;
            }
            else if (entity.Position.x >= rightX)
            {
                return 2;
            }
            else if (entity.Position.z >= topY)
            {
                return 3;
            }
            return -1;
        }
        private static bool IsInWheelRange(Entity self, Entity target)
        {
            Vector3 pos = self.Position;
            Vector3 otherPos = target.Position;
            Vector2 vector = new Vector2(pos.x - otherPos.x, pos.z - otherPos.z);
            var selfBounds = self.GetBounds();
            var targetBounds = target.GetBounds();
            return vector.magnitude < SPIN_RADIUS && Detection.IsYCoincide(selfBounds.min.y, selfBounds.size.y, targetBounds.min.y, targetBounds.size.y);
        }

        #region 出现
        public class AppearState : EntityStateMachineState
        {
            public AppearState() : base(STATE_APPEAR) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.Position = APPEAR_POSITION;
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(72);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run();
                var t = (stateTimer.Frame / 30f) - 0.25f;
                entity.Position = Vector3.Lerp(APPEAR_POSITION, CENTER_POSITION, t);
                stateMachine.StartState(entity, STATE_IDLE);
            }
        }
        #endregion

        #region 空闲
        public class IdleState : EntityStateMachineState
        {
            public IdleState() : base(STATE_IDLE) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(150);
                entity.SetAnimationBool("FlapWing", true);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run();
                if (stateTimer.Expired)
                {
                    SwitchState(stateMachine, entity);
                }
                UpdateMoveDirection(entity);
                StopSpinSound(entity);
            }
            public void SwitchState(EntityStateMachine stateMachine, Entity entity)
            {
                var lastState = stateMachine.GetPreviousState(entity);
                var lastStateIndex = statePool.IndexOf(lastState);
                var currentStateIndex = lastStateIndex;
                for (int i = 1; i < statePool.Count * 2; i++)
                {
                    currentStateIndex = (currentStateIndex + 1) % statePool.Count;
                    var currentState = statePool[currentStateIndex];
                    switch (currentState)
                    {
                        case STATE_JAB:
                            var jabTarget = FindJabTarget(entity);
                            entity.Target = jabTarget;
                            if (jabTarget != null)
                            {
                                stateMachine.StartState(entity, currentState);
                                stateMachine.SetPreviousState(entity, currentState);
                                return;
                            }
                            break;
                        case STATE_DARKNESS:
                        case STATE_SPIN:
                            stateMachine.StartState(entity, currentState);
                            stateMachine.SetPreviousState(entity, currentState);
                            return;
                        case STATE_REVIVE:
                            var corpsePositions = GetCorpsePositions(entity);
                            if (corpsePositions != null && corpsePositions.Count > 0)
                            {
                                stateMachine.StartState(entity, currentState);
                                stateMachine.SetPreviousState(entity, currentState);
                                return;
                            }
                            break;
                    }
                }

            }
            private void UpdateMoveDirection(Entity entity)
            {
                var moveDirection = GetMoveDirection(entity);

                Vector3 velocity = entity.Velocity;
                float magnitude = velocity.magnitude;
                if (magnitude < 4)
                {
                    magnitude += 0.05f;
                }
                velocity = moveDirection * magnitude;
                velocity.y = entity.Velocity.y;
                entity.Velocity = velocity;

                float leftX = CENTER_POSITION.x + 40;
                float rightX = VanillaLevelExt.RIGHT_BORDER - 40;
                float topY = entity.Level.GetGridTopZ();
                float bottomY = entity.Level.GetGridBottomZ();
                bool outOfRightRegion = entity.Position.x <= leftX || entity.Position.z <= bottomY || entity.Position.x >= rightX || entity.Position.z >= topY;
                if (outOfRightRegion)
                {
                    var center = new Vector3((leftX + rightX) * 0.5f, 0, (topY + bottomY) * 0.5f);
                    moveDirection = (center - entity.Position).normalized;
                }
                else
                {
                    var moveRNG = GetMoveRNG(entity);
                    float dir = moveRNG.Next(-1, 1f);
                    float angle = dir * 10;
                    Quaternion rotation = Quaternion.Euler(Vector3.up * angle);
                    moveDirection = (rotation * moveDirection).normalized;


                    // Fix z to fit current lane.
                    float laneZ = entity.Level.GetEntityLaneZ(entity.GetLane());
                    Vector3 laneFix = (laneZ - entity.Position.z) * 0.133f * Vector3.forward;
                    entity.Position += laneFix;
                }
                SetMoveDirection(entity, moveDirection);
            }
        }
        #endregion

        #region 戳刺
        public class JabState : EntityStateMachineState
        {
            public JabState() : base(STATE_JAB) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.TriggerAnimation("Jab");
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.ResetTime(21);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));
                if (!substateTimer.Expired)
                    return;

                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_READY_1:
                    case SUBSTATE_READY_2:
                    case SUBSTATE_READY_3:
                        Jab(entity, entity.Target);
                        stateMachine.SetSubState(entity, substate + 1);
                        substateTimer.ResetTime(9);
                        break;

                    case SUBSTATE_JAB_1:
                    case SUBSTATE_JAB_2:
                        var jabTarget = FindJabTarget(entity);
                        entity.Target = jabTarget;
                        if (jabTarget != null)
                        {
                            entity.TriggerAnimation("Jab");
                            stateMachine.SetSubState(entity, substate + 1);
                            substateTimer.ResetTime(21);
                        }
                        else
                        {
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;

                    case SUBSTATE_JAB_3:
                        stateMachine.StartState(entity, STATE_IDLE);
                        break;
                }
            }

            private void Jab(Entity entity, Entity target)
            {
                if (target == null)
                    return;
                Vector3 beforePos = entity.Position;
                entity.Position = target.GetCenter() + Vector3.up * 40f;
                entity.Velocity = Vector3.zero;
                // Create shadow trails.
                Vector3 distance = entity.Position - beforePos;
                float magnitude = distance.magnitude;
                Vector3 normalized = distance.normalized;
                for (float i = 0; i < magnitude; i += 16)
                {
                    Vector3 pos = beforePos + normalized * i;
                    var shadow = entity.Spawn(VanillaEffectID.nightmareaperShadow, pos);
                    shadow.Timeout = Mathf.CeilToInt(i / magnitude * 30);
                }
                // Jab.
                bool jabbed = false;
                foreach (IEntityCollider collider in entity.Level.OverlapBox(target.GetCenter(), Vector3.one * 40, entity.GetFaction(), EntityCollisionHelper.MASK_VULNERABLE, 0))
                {
                    collider.TakeDamage(10000, new DamageEffectList(), entity);
                    jabbed = true;
                }
                if (jabbed)
                {
                    entity.PlaySound(VanillaSoundID.smash);
                    entity.Level.ShakeScreen(5, 0, 9);
                }
            }

            public const int SUBSTATE_READY_1 = 0;
            public const int SUBSTATE_JAB_1 = 1;
            public const int SUBSTATE_READY_2 = 2;
            public const int SUBSTATE_JAB_2 = 3;
            public const int SUBSTATE_READY_3 = 4;
            public const int SUBSTATE_JAB_3 = 5;
        }
        private static Entity FindJabTarget(Entity entity)
        {
            targetBuffer.Clear();
            entity.Level.FindEntitiesNonAlloc(c => c.IsVulnerableEntity() && entity.IsHostile(c), targetBuffer);
            if (targetBuffer.Count > 0)
            {
                var actRNG = GetStateRNG(entity);
                return targetBuffer.Random(actRNG);
            }
            return null;
        }

        private static List<Entity> targetBuffer = new List<Entity>();
        #endregion

        #region 旋转
        public class SpinState : EntityStateMachineState
        {
            public SpinState() : base(STATE_SPIN) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.ResetTime(30);
                entity.SetAnimationBool("FlapWing", false);
                entity.PlaySound(VanillaSoundID.wheelOfDeathStart);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));

                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_START:
                        StartOrEndUpdate(entity);
                        if (substateTimer.Expired)
                        {
                            stateMachine.SetSubState(entity, SUBSTATE_LOOP);
                            substateTimer.ResetTime(210);
                        }
                        break;

                    case SUBSTATE_LOOP:
                        LoopUpdate(entity);
                        if (substateTimer.Expired && GetOutbound(entity) < 0)
                        {
                            stateMachine.SetSubState(entity, SUBSTATE_END);
                            substateTimer.ResetTime(30);
                        }
                        break;

                    case SUBSTATE_END:
                        StartOrEndUpdate(entity);
                        if (substateTimer.Expired)
                        {
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;
                }
            }
            public override void OnExit(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnExit(stateMachine, entity);
                StopSpinSound(entity);
            }
            private void SetSpinVelocity(Entity entity)
            {
                const float angelicSpeed = 10;
                const float targetRadius = 240;


                var center2D = new Vector2(CENTER_POSITION.x, CENTER_POSITION.z);
                var pos2D = new Vector2(entity.Position.x, entity.Position.z);
                var velocity2D = new Vector2(entity.Velocity.x, entity.Velocity.z);

                Vector2 pos2Center2D = pos2D - center2D;
                var radius = pos2Center2D.magnitude;
                var direction = pos2Center2D.normalized;

                radius = radius * 0.5f + targetRadius * 0.5f;
                var nextPosition = center2D + direction.RotateClockwise(angelicSpeed) * radius;
                var targetVelocity = nextPosition - pos2D;
                velocity2D = velocity2D * 0.5f + targetVelocity * 0.5f;

                var velocity = new Vector3(velocity2D.x, 0, velocity2D.y);
                entity.Velocity = velocity;
            }
            private void StartOrEndUpdate(Entity entity)
            {
                float magnitude = entity.Velocity.magnitude;
                float acc = 3.333f;
                if (magnitude * (magnitude - acc) <= 0)
                {
                    entity.Velocity = Vector3.zero;
                }
                else
                {
                    entity.Velocity -= entity.Velocity.normalized * acc;
                }
            }
            private void LoopUpdate(Entity entity)
            {
                SetSpinVelocity(entity);

                var spinDamageTimer = GetSpinDamageTimer(entity);
                if (spinDamageTimer == null)
                {
                    spinDamageTimer = new FrameTimer(SPIN_DAMAGE_INTERVAL);
                    SetSpinDamageTimer(entity, spinDamageTimer);
                }
                var level = entity.Level;
                spinDamageTimer.Run();
                if (spinDamageTimer.Expired)
                {
                    spinDamageTimer.Reset();
                    var rng = GetStateRNG(entity);
                    detectBuffer.Clear();
                    level.OverlapCapsuleNonAlloc(entity.GetCenter(), SPIN_RADIUS, SPIN_HEIGHT, entity.GetFaction(), EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer);
                    foreach (IEntityCollider collider in detectBuffer)
                    {
                        var target = collider.Entity;
                        var colliderReference = collider.ToReference();
                        var damage = level.Difficulty == VanillaDifficulties.hard ? SPIN_DAMAGE_HARD : SPIN_DAMAGE;
                        var damageOutput = collider.TakeDamage(damage, new DamageEffectList(VanillaDamageEffects.SLICE), entity);
                        PostSpinDamage(entity, damageOutput);
                    }
                }
                if (!level.HasLoopSoundEntity(VanillaSoundID.wheelOfDeathLoop, entity.ID))
                {
                    level.AddLoopSoundEntity(VanillaSoundID.wheelOfDeathLoop, entity.ID);
                }
            }
            private void PostSpinDamage(Entity entity, DamageOutput damage)
            {
                if (damage.ShieldResult != null)
                {
                    PostSpinDamageByResult(entity, damage.ShieldResult);
                }
                if (damage.ArmorResult != null)
                {
                    PostSpinDamageByResult(entity, damage.ArmorResult);
                }
                if (damage.BodyResult != null)
                {
                    PostSpinDamageByResult(entity, damage.BodyResult);
                }
            }
            private void PostSpinDamageByResult(Entity entity, DamageResult result)
            {
                var rng = GetSparkRNG(entity);
                var targetShell = result.ShellDefinition;
                if (!targetShell.BlocksSlice())
                    return;
                // Create Particle.
                Vector3 relativePos = result.GetPosition() - entity.Position;
                Vector3 particlePos = entity.Position + relativePos.normalized * SPIN_RADIUS;
                var sparkParticle = entity.Spawn(VanillaEffectID.sliceSpark, particlePos);
                // Set Particle Angles.
                float angle = Vector2.SignedAngle(Vector2.left, new Vector2(relativePos.x, relativePos.z));
                Vector3 euler = new Vector3(0, 0, angle);
                sparkParticle.RenderRotation = euler;

                // Create Sound.
                entity.PlaySound(VanillaSoundID.anvil, rng.Next(1.5f, 2.5f));
            }

            public const int SUBSTATE_START = 0;
            public const int SUBSTATE_LOOP = 1;
            public const int SUBSTATE_END = 2;
            private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        }
        #endregion

        #region 黑暗
        public class DarknessState : EntityStateMachineState
        {
            public DarknessState() : base(STATE_DARKNESS) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.TriggerAnimation("Cast");
                entity.PlaySound(VanillaSoundID.reverseVampire);
                entity.PlaySound(VanillaSoundID.confuse);

                SetDarknessTimeout(entity.Level, 480);

                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(30);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run(stateMachine.GetSpeed(entity));

                if (stateTimer.Expired)
                {
                    stateMachine.StartState(entity, STATE_IDLE);
                }
            }
        }
        #endregion

        #region 复活
        public class ResurrectState : EntityStateMachineState
        {
            public ResurrectState() : base(STATE_REVIVE) { }

            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                Resurrect(entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(30);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run(stateMachine.GetSpeed(entity));

                if (stateTimer.Expired)
                {
                    stateMachine.StartState(entity, STATE_IDLE);
                }
            }
            private void Resurrect(Entity entity)
            {
                var corpsePositions = GetCorpsePositions(entity);
                if (corpsePositions != null)
                {
                    foreach (Vector3 position in corpsePositions)
                    {
                        var pos = position;
                        pos.y = entity.Level.GetGroundY(pos.x, pos.z);
                        var skeleton = entity.Spawn(VanillaEnemyID.skeleton, pos);
                        skeleton.SetFactionAndDirection(entity.GetFaction());
                        var boneParticle = entity.Spawn(VanillaEffectID.boneParticles, skeleton.GetCenter());

                        entity.PlaySound(VanillaSoundID.boneWallBuild);
                    }
                    corpsePositions.Clear();
                }

                entity.TriggerAnimation("Cast");
                entity.PlaySound(VanillaSoundID.reviveCast);
                entity.PlaySound(VanillaSoundID.revived);
            }
        }
        #endregion

        #region 激怒
        public class EnragedState : EntityStateMachineState
        {
            public EnragedState() : base(STATE_ENRAGE) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(30);

                entity.PlaySound(VanillaSoundID.nightmareaperRage);
                entity.SetAnimationBool("FlapWing", false);
                entity.SetAnimationBool("Shake", true);
                entity.SetModelProperty("RageState", 0);

                foreach (var wall in entity.Level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    CrushingWalls.Enrage(wall);
                }

                CancelDarkness(entity.Level);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);

                entity.Position = (entity.Position - CENTER_POSITION) * 0.9f + CENTER_POSITION;

                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run(stateMachine.GetSpeed(entity));

                var substate = stateMachine.GetSubState(entity);

                switch (substate)
                {
                    case SUBSTATE_START:
                        if (subStateTimer.Expired)
                        {
                            entity.SetModelProperty("RageState", 1);
                            entity.SetModelProperty("RageProgress", 0);
                            entity.SetAnimationBool("Shake", false);
                            stateMachine.SetSubState(entity, SUBSTATE_EXTEND);
                            subStateTimer.ResetTime(15);
                        }
                        break;
                    case SUBSTATE_EXTEND:

                        entity.SetModelProperty("RageProgress", 1 - (subStateTimer.Frame / 15f));

                        if (subStateTimer.Expired)
                        {
                            entity.Level.ShakeScreen(20, 0, 15);

                            foreach (var wall in entity.Level.FindEntities(VanillaEffectID.crushingWalls))
                            {
                                CrushingWalls.Shake(wall, 20, 0, 15);
                            }
                            entity.PlaySound(VanillaSoundID.smash);
                            subStateTimer.ResetTime(15);
                            stateMachine.SetSubState(entity, SUBSTATE_INSERT);
                        }
                        break;
                    case SUBSTATE_INSERT:
                        entity.SetModelProperty("RageProgress", 1);

                        if (subStateTimer.Expired)
                        {
                            entity.SetModelProperty("RageState", 2);
                            entity.SetModelProperty("RageProgress", 0);
                            foreach (var wall in entity.Level.FindEntities(VanillaEffectID.crushingWalls))
                            {
                                CrushingWalls.Close(wall);
                            }
                            subStateTimer.ResetTime(15);
                            stateMachine.SetSubState(entity, SUBSTATE_PULL);
                        }
                        break;
                    case SUBSTATE_PULL:
                        entity.SetModelProperty("RageProgress", 1 - (subStateTimer.Frame / 15f));
                        break;
                }
            }
            public const int SUBSTATE_START = 0;
            public const int SUBSTATE_EXTEND = 1;
            public const int SUBSTATE_INSERT = 2;
            public const int SUBSTATE_PULL = 3;
        }
        #endregion

        #region 死亡
        public class DeathState : EntityStateMachineState
        {
            public DeathState() : base(STATE_DEATH) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.SetAnimationBool("Shake", true);
                entity.SetAnimationBool("FlapWing", false);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.ResetTime(159);
                entity.RemoveBuffs<FlyBuff>();
                StopSpinSound(entity);
            }
            public override void OnUpdateLogic(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateLogic(stateMachine, entity);

                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run();
                var substate = stateMachine.GetSubState(entity);

                if (substate != SUBSTATE_DROP)
                {
                    entity.Velocity = Vector3.zero;
                    Vector3 center = CENTER_POSITION;
                    center.y = 100;
                    if ((center - entity.Position).magnitude > 20)
                    {
                        entity.Position += 3 * (center - entity.Position).normalized;
                    }
                }

                switch (substate)
                {
                    case SUBSTATE_START:
                        if (subStateTimer.Expired)
                        {
                            entity.SetAnimationBool("Shake", false);
                            subStateTimer.ResetTime(21);
                            stateMachine.SetSubState(entity, SUBSTATE_FAINT);
                        }
                        break;
                    case SUBSTATE_FAINT:
                        if (subStateTimer.Expired)
                        {
                            entity.AddBuff<NightmareaperFallBuff>();
                            subStateTimer.ResetTime(60);
                            stateMachine.SetSubState(entity, SUBSTATE_DROP);
                        }
                        break;
                    case SUBSTATE_DROP:
                        if (subStateTimer.Expired)
                        {
                            entity.Remove();
                        }
                        break;
                }
            }
            public const int SUBSTATE_START = 0;
            public const int SUBSTATE_FAINT = 1;
            public const int SUBSTATE_DROP = 2;
        }
        #endregion

        public static List<int> statePool = new List<int>()
        {
            STATE_JAB,
            STATE_DARKNESS,
            STATE_SPIN,
            STATE_REVIVE
        };
        public const int STATE_APPEAR = VanillaEntityStates.NIGHTMAREAPER_APPEAR;
        public const int STATE_IDLE = VanillaEntityStates.NIGHTMAREAPER_IDLE;
        public const int STATE_JAB = VanillaEntityStates.NIGHTMAREAPER_JAB;
        public const int STATE_SPIN = VanillaEntityStates.NIGHTMAREAPER_SPIN;
        public const int STATE_DARKNESS = VanillaEntityStates.NIGHTMAREAPER_DARKNESS;
        public const int STATE_REVIVE = VanillaEntityStates.NIGHTMAREAPER_REVIVE;
        public const int STATE_ENRAGE = VanillaEntityStates.NIGHTMAREAPER_ENRAGE;
        public const int STATE_DEATH = VanillaEntityStates.NIGHTMAREAPER_DEATH;
    }
}