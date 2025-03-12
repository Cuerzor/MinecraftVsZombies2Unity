using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Difficulties;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.frankenstein)]
    public partial class Frankenstein : BossBehaviour
    {
        public Frankenstein(string nsp, string name) : base(nsp, name)
        {
        }

        #region 回调
        public override void Init(Entity boss)
        {
            base.Init(boss);
            SetDetectTimer(boss, new FrameTimer());
            SetShockRNG(boss, new RandomGenerator(boss.RNG.Next()));
            SetBulletRNG(boss, new RandomGenerator(boss.RNG.Next()));
            SetJumpRNG(boss, new RandomGenerator(boss.RNG.Next()));

            stateMachine.Init(boss);
            stateMachine.StartState(boss, STATE_WAKING);

            if (boss.Level.Difficulty == VanillaDifficulties.hard)
            {
                EnterSteelPhase(boss);
            }
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            if (entity.IsDead)
                return;
            if (CanTransformPhase(entity) && entity.State != STATE_FAINT)
            {
                stateMachine.StartState(entity, STATE_FAINT);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(60);
                entity.PlaySound(VanillaSoundID.powerOff);
            }
            stateMachine.UpdateAI(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            UpdateAim(entity);
            stateMachine.UpdateLogic(entity);
            entity.SetAnimationBool("MissileVisible", entity.State == STATE_MISSILE && stateMachine.GetSubState(entity) == SUBSTATE_MISSILE_AIM);
            entity.SetAnimationFloat("ActionSpeed", GetFrankensteinActionSpeed(entity));
        }
        public override void PostDeath(Entity boss, DeathInfo damageInfo)
        {
            base.PostDeath(boss, damageInfo);
            stateMachine.StartState(boss, STATE_DEAD);
        }
        public override void PreTakeDamage(DamageInput damageInfo)
        {
            base.PreTakeDamage(damageInfo);
            if (damageInfo.Amount > 600)
            {
                damageInfo.SetAmount(600);
            }
        }
        #endregion 事件

        private static void EnterSteelPhase(Entity boss)
        {
            boss.PlaySound(VanillaSoundID.bloody);
            var level = boss.Level;
            var center = boss.Position + new Vector3(-50, 80, 0);

            level.Spawn(VanillaEffectID.gore, center, boss);

            var bloodPart = level.Spawn(VanillaEffectID.bloodParticles, center, boss);
            var bloodColor = boss.GetBloodColor();
            bloodPart.SetTint(bloodColor);

            SetSteelPhase(boss, true);
        }
        private void UpdateAim(Entity boss)
        {
            Entity target = boss.Target;
            var substate = stateMachine.GetSubState(boss);
            // 内手臂，发射子弹
            Vector3 innerArmRootPosition = boss.Position + innerArmRootOffset;

            Vector3 innerDir = boss.GetFacingDirection();
            if (boss.State == STATE_GUN && target != null)
            {
                innerDir = (target.GetCenter() - innerArmRootPosition).normalized;
            }
            var gunDir = GetGunDirection(boss);
            SetGunDirection(boss, Vector3.Lerp(gunDir, innerDir, 0.3f));

            // 外手臂，发射火箭
            Vector3 outerArmRootPosition = boss.Position + innerArmRootOffset;

            Vector3 outerDir = boss.GetFacingDirection();
            if (boss.State == STATE_MISSILE && substate == SUBSTATE_MISSILE_AIM && target != null)
            {
                outerDir = (target.GetCenter() - outerArmRootPosition).normalized;
            }
            var missileDir = GetMissileDirection(boss);
            SetMissileDirection(boss, Vector3.Lerp(missileDir, outerDir, 0.3f));
        }
        private static Entity FindMissileTarget(Entity boss)
        {
            return missileDetector.DetectEntityWithTheMost(boss, t => Mathf.Abs(boss.Position.x - t.GetCenter().x));
        }
        private static Entity FindPunchTarget(Entity boss)
        {
            return boss.Level.FindFirstEntity(e => IsPunchable(boss, e));
        }

        /// <summary>
        /// 寻找机枪目标。
        /// </summary>
        private static Entity FindGunTarget(Entity boss)
        {
            return gunDetector.DetectEntityWithTheLeast(boss, t => Mathf.Abs(boss.Position.x - t.GetCenter().x));
        }
        private static Entity FindShockingTarget(Entity boss)
        {
            return boss.Level.FindFirstEntity(e => IsShockable(boss, e));
        }
        private static bool IsPunchable(Entity boss, Entity target)
        {
            if (!target.IsVulnerableEntity())
                return false;
            return boss.IsHostile(target) && Detection.IsInFrontOf(boss, target, 20, 80) && target.GetLane() == boss.GetLane();
        }
        private static bool IsShockable(Entity boss, Entity target)
        {
            if (target.IsEntityOf(VanillaContraptionID.tnt))
                return true;
            return target.Type == EntityTypes.PLANT && boss.IsHostile(target) && Detection.IsInFrontOf(boss, target, 20) && target.CanDeactive();
        }

        public static void DoTransformationEffects(Entity boss)
        {
            // 震动特效
            Vector3 centerPos = boss.Position + awakeOffset;

            var level = boss.Level;
            level.Thunder();
            boss.PlaySound(VanillaSoundID.thunder);
            boss.PlaySound(VanillaSoundID.smash);
            level.Spawn(VanillaEffectID.thunderBolt, centerPos, boss);

            var explosion = level.Spawn(VanillaEffectID.explosion, centerPos, boss);
            explosion.SetSize(Vector3.one * 120);

            const int arcCounts = 8;
            const float arcAngle = 360 / arcCounts;

            for (int i = 0; i < arcCounts; i++)
            {
                float rad = i * arcAngle * Mathf.Deg2Rad;

                var arc = level.Spawn(VanillaEffectID.electricArc, centerPos + Vector3.up, boss);
                Vector3 arcTargetPos = centerPos + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * 100;
                ElectricArc.Connect(arc, arcTargetPos);
                ElectricArc.UpdateArc(arc);
            }

            level.ShakeScreen(20, 0, 15);
        }
        public static void Paralyze(Entity boss, Entity source)
        {
            boss.TakeDamage(1200, new DamageEffectList(VanillaDamageEffects.LIGHTNING), source);
            if (!boss.IsDead)
            {
                SetParalyzed(boss, true);
                stateMachine.StartState(boss, STATE_FAINT);
                var substateTimer = stateMachine.GetSubStateTimer(boss);
                substateTimer.ResetTime(300);
                boss.PlaySound(VanillaSoundID.powerOff);
            }
        }
        private static bool CanTransformPhase(Entity boss)
        {
            if (boss.Level.Difficulty == VanillaDifficulties.easy)
                return false;
            return boss.Health <= boss.GetMaxHealth() * 0.5f && !IsSteelPhase(boss);
        }

        public static float GetFrankensteinActionSpeed(Entity boss)
        {
            return boss.Level.Difficulty == VanillaDifficulties.hard ? 2 : 1;
        }

        #region 属性

        public static bool IsParalyzed(Entity boss)
        {
            return boss.GetBehaviourField<bool>(ID, PROP_PARALYZED);
        }
        public static void SetParalyzed(Entity boss, bool value)
        {
            boss.SetBehaviourField(ID, PROP_PARALYZED, value);
        }

        public static bool IsSteelPhase(Entity boss)
        {
            return boss.GetBehaviourField<bool>(ID, PROP_STEEL_PHASE);
        }
        public static void SetSteelPhase(Entity boss, bool value)
        {
            boss.SetBehaviourField(ID, PROP_STEEL_PHASE, value);
            if (value)
            {
                boss.AddBuff<FrankensteinSteelBuff>();
            }
            else
            {
                boss.RemoveBuffs<FrankensteinSteelBuff>();
            }
            boss.SetAnimationBool("Steel", value);
        }

        public static FrameTimer GetDetectTimer(Entity boss)
        {
            return boss.GetBehaviourField<FrameTimer>(ID, PROP_DETECT_TIMER);
        }
        private static void SetDetectTimer(Entity boss, FrameTimer value)
        {
            boss.SetBehaviourField(ID, PROP_DETECT_TIMER, value);
        }

        public static RandomGenerator GetShockRNG(Entity boss)
        {
            return boss.GetBehaviourField<RandomGenerator>(ID, PROP_SHOCK_RNG);
        }
        private static void SetShockRNG(Entity boss, RandomGenerator value)
        {
            boss.SetBehaviourField(ID, PROP_SHOCK_RNG, value);
        }

        public static RandomGenerator GetJumpRNG(Entity boss)
        {
            return boss.GetBehaviourField<RandomGenerator>(ID, PROP_JUMP_RNG);
        }
        private static void SetJumpRNG(Entity boss, RandomGenerator value)
        {
            boss.SetBehaviourField(ID, PROP_JUMP_RNG, value);
        }

        public static RandomGenerator GetBulletRNG(Entity boss)
        {
            return boss.GetBehaviourField<RandomGenerator>(ID, PROP_BULLET_RNG);
        }
        private static void SetBulletRNG(Entity boss, RandomGenerator value)
        {
            boss.SetBehaviourField(ID, PROP_BULLET_RNG, value);
        }

        private static Vector3 GetGunDirection(Entity boss)
        {
            return boss.GetBehaviourField<Vector3>(ID, PROP_GUN_DIRECTION);
        }
        private static void SetGunDirection(Entity boss, Vector3 direction)
        {
            boss.SetBehaviourField(ID, PROP_GUN_DIRECTION, direction);
            float innerAngle = Vector2.SignedAngle(boss.GetFacingDirection(), direction);
            boss.SetAnimationFloat("InnerArmAngle", innerAngle);
        }

        private static Vector3 GetMissileDirection(Entity boss)
        {
            return boss.GetBehaviourField<Vector3>(ID, PROP_MISSILE_DIRECTION);
        }
        private static void SetMissileDirection(Entity boss, Vector3 direction)
        {
            boss.SetBehaviourField(ID, PROP_MISSILE_DIRECTION, direction);
            float innerAngle = Vector2.SignedAngle(boss.GetFacingDirection(), direction);
            boss.SetAnimationFloat("OuterArmAngle", innerAngle);
        }

        private static Vector3 GetJumpTarget(Entity boss)
        {
            return boss.GetBehaviourField<Vector3>(ID, PROP_JUMP_TARGET);
        }
        private static void SetJumpTarget(Entity boss, Vector3 target)
        {
            boss.SetBehaviourField(ID, PROP_JUMP_TARGET, target);
        }

        #endregion 属性

        #region 常量
        public static readonly NamespaceID ID = VanillaBossID.frankenstein;
        private const int detectIntervalFrames = 3;
        private const int shootingPeriodFrames = 60;
        private const float jumpFlyingTime = 15;
        //private const float bulletSpeed = 10;
        //private const float missileSpeed = 8;
        private const float upperArmLength = 60;
        private static readonly Vector3 innerArmRootOffset = new Vector3(-12, 150, 0);
        private static readonly Vector3 outerArmRootOffset = new Vector3(10, 134, 0);
        private static readonly Vector3 headOffset = new Vector3(-12.5f, 153, 0);
        private static readonly Vector3 awakeOffset = new Vector3(-50, 0, 0);
        private static Detector gunDetector = new FrankensteinGunDetector(VanillaProjectileID.bullet);
        private static Detector missileDetector = new FrankensteinGunDetector(VanillaProjectileID.missile);

        private static readonly VanillaEntityPropertyMeta PROP_PARALYZED = new VanillaEntityPropertyMeta("Paralyzed");

        private static readonly VanillaEntityPropertyMeta PROP_JUMP_TARGET = new VanillaEntityPropertyMeta("JumpTarget");
        private static readonly VanillaEntityPropertyMeta PROP_STEEL_PHASE = new VanillaEntityPropertyMeta("SteelPhase");

        private static readonly VanillaEntityPropertyMeta PROP_GUN_DIRECTION = new VanillaEntityPropertyMeta("GunDirection");
        private static readonly VanillaEntityPropertyMeta PROP_MISSILE_DIRECTION = new VanillaEntityPropertyMeta("MissileDirection");

        private static readonly VanillaEntityPropertyMeta PROP_DETECT_TIMER = new VanillaEntityPropertyMeta("DetectTimer");

        private static readonly VanillaEntityPropertyMeta PROP_SHOCK_RNG = new VanillaEntityPropertyMeta("ShockRNG");
        private static readonly VanillaEntityPropertyMeta PROP_JUMP_RNG = new VanillaEntityPropertyMeta("JumpRNG");
        private static readonly VanillaEntityPropertyMeta PROP_BULLET_RNG = new VanillaEntityPropertyMeta("BulletRNG");

        private const int STATE_IDLE = VanillaEntityStates.FRANKENSTEIN_IDLE;
        private const int STATE_JUMP = VanillaEntityStates.FRANKENSTEIN_JUMP;
        private const int STATE_GUN = VanillaEntityStates.FRANKENSTEIN_GUN;
        private const int STATE_DEAD = VanillaEntityStates.FRANKENSTEIN_DEAD;
        private const int STATE_MISSILE = VanillaEntityStates.FRANKENSTEIN_MISSILE;
        private const int STATE_PUNCH = VanillaEntityStates.FRANKENSTEIN_PUNCH;
        private const int STATE_SHOCK = VanillaEntityStates.FRANKENSTEIN_SHOCK;
        private const int STATE_WAKING = VanillaEntityStates.FRANKENSTEIN_WAKING;
        private const int STATE_FAINT = VanillaEntityStates.FRANKENSTEIN_FAINT;

        private const int SUBSTATE_AWAKE_START = 0;
        private const int SUBSTATE_AWAKE_LAUGH = 1;
        private const int SUBSTATE_AWAKE_RISE = 2;

        private const int SUBSTATE_GUN_READY = 0;
        private const int SUBSTATE_GUN_FIRE = 1;

        private const int SUBSTATE_DEAD_STAND = 0;
        private const int SUBSTATE_DEAD_HEAD_DROPPED = 1;

        private const int SUBSTATE_MISSILE_AIM = 0;
        private const int SUBSTATE_MISSILE_FIRED = 1;

        private const int SUBSTATE_PUNCH_READY = 0;
        private const int SUBSTATE_PUNCH_FIRE = 1;
        private const int SUBSTATE_PUNCH_FINISHED = 2;

        private const int SUBSTATE_SHOCK_READY = 0;
        private const int SUBSTATE_SHOCK_FINISHED = 1;

        private const int SUBSTATE_JUMP_READY = 0;
        private const int SUBSTATE_JUMP_IN_AIR = 1;
        #endregion 常量

        private static FrankensteinStateMachine stateMachine = new FrankensteinStateMachine();
    }
}
