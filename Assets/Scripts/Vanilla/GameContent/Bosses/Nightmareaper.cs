using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.nightmareaper)]
    public partial class Nightmareaper : BossBehaviour
    {
        public Nightmareaper(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEnemyDeathCallback, filter: EntityTypes.ENEMY);
        }

        #region 生命周期
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetMoveRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetStateRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetSparkRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetMoveDirection(entity, Vector3.back);
            var flyBuff = entity.AddBuff<FlyBuff>();
            flyBuff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, FLY_HEIGHT);

            stateMachine.Init(entity);

            CheckTimerAndWallsCreation(entity);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.IsDead)
                return;
            stateMachine.UpdateAI(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            stateMachine.UpdateLogic(entity);
        }
        public override void PostDeath(Entity entity, DeathInfo deathInfo)
        {
            base.PostDeath(entity, deathInfo);

            entity.PlaySound(VanillaSoundID.nightmareaperDeath);
            var darkMatter = entity.Spawn(VanillaEffectID.darkMatterParticles, entity.Position);
            darkMatter.SetParent(entity);

            CancelDarkness(entity.Level);

            stateMachine.StartState(entity, STATE_DEATH);

            CheckTimerAndWallsDestruction(entity);
        }
        public override void PreTakeDamage(DamageInput input)
        {
            base.PreTakeDamage(input);
            if (input.Amount > 600)
            {
                input.SetAmount(600);
            }
        }
        #endregion

        private void PostEnemyDeathCallback(Entity entity, DeathInfo deathInfo)
        {
            if (deathInfo.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            if (entity.IsOnWater())
                return;
            foreach (Entity nightmareaper in entity.Level.FindEntities(VanillaBossID.nightmareaper))
            {
                AddCorpsePosition(nightmareaper, entity.Position);
            }
        }
        public static void AddCorpsePosition(Entity entity, Vector3 pos)
        {
            var deathPositions = GetCorpsePositions(entity);
            if (deathPositions == null)
            {
                deathPositions = new List<Vector3>();
                SetCorpsePositions(entity, deathPositions);
            }
            deathPositions.Add(pos);
            while (deathPositions.Count > MAX_REVIVE_COUNT)
            {
                deathPositions.RemoveAt(0);
            }
        }
        public static void Appear(Entity entity)
        {
            stateMachine.StartState(entity, STATE_APPEAR);
        }
        public static void Enrage(Entity entity)
        {
            stateMachine.StartState(entity, STATE_ENRAGE);
        }
        private static void SetDarknessTimeout(LevelEngine level, int timeout)
        {
            var buffs = level.GetBuffs<NightmareaperDarknessBuff>();
            if (buffs.Length <= 0)
            {
                var buff = level.AddBuff<NightmareaperDarknessBuff>();
                buff.SetProperty(NightmareaperDarknessBuff.PROP_TIMEOUT, timeout);
            }
            else
            {
                foreach (var buff in level.GetBuffs<NightmareaperDarknessBuff>())
                {
                    buff.SetProperty(NightmareaperDarknessBuff.PROP_TIMEOUT, timeout);
                }
            }
        }
        private static void CancelDarkness(LevelEngine level)
        {
            foreach (var buff in level.GetBuffs<NightmareaperDarknessBuff>())
            {
                NightmareaperDarknessBuff.CancelDarkness(buff);
            }
        }
        private static void CheckTimerAndWallsCreation(Entity entity)
        {
            var level = entity.Level;
            if (!level.EntityExists(VanillaEffectID.nightmareaperTimer))
            {
                var pos = new Vector3(620, 0, 500);
                entity.Spawn(VanillaEffectID.nightmareaperTimer, pos);
            }
            if (!level.EntityExists(VanillaEffectID.crushingWalls))
            {
                entity.Spawn(VanillaEffectID.crushingWalls, CENTER_POSITION);
            }
        }
        private static void CheckTimerAndWallsDestruction(Entity entity)
        {
            var level = entity.Level;
            var hasAliveReaper = level.EntityExists(e => e != entity && e.IsEntityOf(VanillaBossID.nightmareaper) && !e.IsDead);
            if (!hasAliveReaper)
            {
                foreach (var timer in level.FindEntities(VanillaEffectID.nightmareaperTimer))
                {
                    timer.Remove();
                }
                foreach (var walls in level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    walls.Remove();
                }
            }
        }

        #region 属性
        private static void SetBehaviourProperty(Entity entity, PropertyKey name, object value) => entity.SetBehaviourField(ID, name, value);
        private static T GetBehaviourProperty<T>(Entity entity, PropertyKey name) => entity.GetBehaviourField<T>(ID, name);

        public static Vector3 GetMoveDirection(Entity entity) => GetBehaviourProperty<Vector3>(entity, PROP_MOVE_DIRECTION);
        public static void SetMoveDirection(Entity entity, Vector3 value) => SetBehaviourProperty(entity, PROP_MOVE_DIRECTION, value);

        public static RandomGenerator GetMoveRNG(Entity entity) => GetBehaviourProperty<RandomGenerator>(entity, PROP_MOVE_RNG);
        public static void SetMoveRNG(Entity entity, RandomGenerator value) => SetBehaviourProperty(entity, PROP_MOVE_RNG, value);

        public static RandomGenerator GetStateRNG(Entity entity) => GetBehaviourProperty<RandomGenerator>(entity, PROP_STATE_RNG);
        public static void SetStateRNG(Entity entity, RandomGenerator value) => SetBehaviourProperty(entity, PROP_STATE_RNG, value);

        public static RandomGenerator GetSparkRNG(Entity entity) => GetBehaviourProperty<RandomGenerator>(entity, PROP_SPARK_RNG);
        public static void SetSparkRNG(Entity entity, RandomGenerator value) => SetBehaviourProperty(entity, PROP_SPARK_RNG, value);

        public static List<Vector3> GetCorpsePositions(Entity entity) => GetBehaviourProperty<List<Vector3>>(entity, PROP_CORPSE_POSITIONS);
        public static void SetCorpsePositions(Entity entity, List<Vector3> value) => SetBehaviourProperty(entity, PROP_CORPSE_POSITIONS, value);

        public static FrameTimer GetSpinDamageTimer(Entity entity) => GetBehaviourProperty<FrameTimer>(entity, PROP_SPIN_DAMAGE_TIMER);
        public static void SetSpinDamageTimer(Entity entity, FrameTimer value) => SetBehaviourProperty(entity, PROP_SPIN_DAMAGE_TIMER, value);
        #endregion

        private static readonly VanillaEntityPropertyMeta PROP_MOVE_DIRECTION = new VanillaEntityPropertyMeta("MoveDirection");
        private static readonly VanillaEntityPropertyMeta PROP_MOVE_RNG = new VanillaEntityPropertyMeta("MoveRNG");
        private static readonly VanillaEntityPropertyMeta PROP_STATE_RNG = new VanillaEntityPropertyMeta("StateRNG");
        private static readonly VanillaEntityPropertyMeta PROP_SPARK_RNG = new VanillaEntityPropertyMeta("SparkRNG");
        private static readonly VanillaEntityPropertyMeta PROP_CORPSE_POSITIONS = new VanillaEntityPropertyMeta("CorpsePositions");
        private static readonly VanillaEntityPropertyMeta PROP_SPIN_DAMAGE_TIMER = new VanillaEntityPropertyMeta("SpinDamageTimer");

        private const float FLY_HEIGHT = 20;

        private static readonly Vector3 CENTER_POSITION = new Vector3(620, 0, 300);
        private static readonly Vector3 APPEAR_POSITION = new Vector3(620, 300, 0);
        private const float JAB_DAMAGE = 10000;
        private const float SPIN_DAMAGE_HARD = 7 * SPIN_DAMAGE_INTERVAL;
        private const float SPIN_DAMAGE = 5 * SPIN_DAMAGE_INTERVAL;
        private const int SPIN_DAMAGE_INTERVAL = 3;
        private const float SPIN_RADIUS = 120;
        private const float SPIN_HEIGHT = 50;
        private const int MAX_REVIVE_COUNT = 10;

        private static readonly NamespaceID ID = VanillaBossID.nightmareaper;
        private static EntityStateMachine stateMachine = new NightmareaperStateMachine();
    }
}