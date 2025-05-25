using System.Linq;
using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Buffs.Level;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Pickups;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2.Vanilla.Stats;
using MVZ2Logic;
using MVZ2Logic.Games;
using MVZ2Logic.IZombie;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Stages
{
    public abstract class IZombieBehaviour : StageBehaviour
    {
        public IZombieBehaviour(StageDefinition stageDef) : base(stageDef)
        {
            stageDef.SetProperty(VanillaLevelProps.FURNACE_DROP_REDSTONE_COUNT, 8);

            stageDef.AddTrigger(LevelCallbacks.POST_ENTITY_INIT, PostEnemyInitCallback, 0, EntityTypes.ENEMY);
        }
        public override void Setup(LevelEngine level)
        {
            base.Setup(level);
            var layoutID = GetNewLayout(level.CurrentFlag, level.GetRoundRNG());
            SetCurrentLayout(level, layoutID);
            var layout = level.Content.GetIZombieLayoutDefinition(layoutID);
            GenerateMap(level, layout);
            SetRoundTimer(level, new FrameTimer(ROUND_COOLDOWN));
        }
        public override void Start(LevelEngine level)
        {
            base.Start(level);
            level.SetEnergy(level.GetStartEnergy());
            level.SetSeedSlotCount(10);
            level.SetStarshardActive(false);
            if (!AllowPickaxe)
            {
                level.SetPickaxeActive(false);
            }
            level.SetTriggerActive(false);
            level.WaveState = STATE_NORMAL;

            var layoutID = GetCurrentLayout(level);
            var layout = level.Content.GetIZombieLayoutDefinition(layoutID);
            ReplaceBlueprints(level, layout);
        }
        public override void Update(LevelEngine level)
        {
            base.Update(level);
            if (level.WaveState == STATE_NORMAL)
            {
                CheckGameOver(level);

                if (level.FindEntities(VanillaEffectID.izObserver).All(e => IZObserver.IsPass(e)))
                {
                    level.CurrentFlag++;
                    var maxRound = GetMaxRounds();
                    var currentRound = level.CurrentFlag;

                    if (maxRound <= 0 || maxRound > currentRound)
                    {
                        level.WaveState = STATE_NEXT_ROUND;
                        if (maxRound <= 0)
                        {
                            level.ShowAdvicePlural(VanillaStrings.CONTEXT_ADVICE, VanillaStrings.ADVICE_IZ_STREAK, currentRound, 0, 150, currentRound.ToString());
                        }
                        else
                        {
                            var remained = maxRound - currentRound;
                            level.ShowAdvicePlural(VanillaStrings.CONTEXT_ADVICE, VanillaStrings.ADVICE_IZ_ROUNDS_LEFT, remained, 0, 150, remained.ToString());
                        }
                        var roundTimer = GetRoundTimer(level);
                        roundTimer.Reset();

                        level.UpdateLevelName();

                        var diamondInterval = 3;
                        if (level.CurrentFlag % diamondInterval == 0)
                        {
                            int money = 250;
                            var difficulty = level.Difficulty;
                            var difficultyMeta = Global.Game.GetDifficultyMeta(difficulty);
                            if (difficultyMeta != null)
                            {
                                money = difficultyMeta.PuzzleMoney;
                            }
                            var x = level.GetLawnCenterX();
                            var z = level.GetLawnCenterZ();
                            var y = level.GetGroundY(x, z);
                            GemEffect.SpawnGemEffects(level, money, new Vector3(x, y, z), null);
                        }
                    }
                    else
                    {
                        level.WaveState = STATE_FINISHED;
                        var x = level.GetEntityColumnX(Mathf.FloorToInt(level.GetMaxColumnCount() * 0.5f));
                        var z = level.GetEntityLaneZ(Mathf.FloorToInt(level.GetMaxLaneCount() * 0.5f));
                        var y = level.GetGroundY(x, z);
                        var position = new Vector3(x, y, z);
                        level.Produce(VanillaPickupID.clearPickup, position, null);
                    }
                }
            }
            else if (level.WaveState == STATE_NEXT_ROUND)
            {
                var roundTimer = GetRoundTimer(level);
                if (roundTimer != null)
                {
                    roundTimer.Run();
                    if (roundTimer.Expired)
                    {
                        NextRound(level);
                    }
                }
            }
        }
        protected virtual int GetMaxRounds() => 1;
        protected abstract void ReplaceBlueprints(LevelEngine level, IZombieLayoutDefinition layout);
        protected abstract NamespaceID GetNewLayout(int round, RandomGenerator rng);
        private void NextRound(LevelEngine level)
        {
            foreach (var ent in level.GetEntities())
            {
                ent.Remove();
            }
            level.AddBuff<FlashWhiteBuff>();
            level.PlaySound(VanillaSoundID.hugeWave);
            var layoutID = GetNewLayout(level.CurrentFlag, level.GetRoundRNG());
            SetCurrentLayout(level, layoutID);
            var layout = level.Content.GetIZombieLayoutDefinition(layoutID);
            GenerateMap(level, layout);
            ReplaceBlueprints(level, layout);
            if (level.IsPickaxeCountLimited())
            {
                var pickaxeCount = level.GetPickaxeRemainCount();
                pickaxeCount = Mathf.Min(MaxPickaxeCount, pickaxeCount + 1);
                level.SetPickaxeRemainCount(pickaxeCount);
            }
            level.WaveState = STATE_NORMAL;

            if (level.IsEndless())
            {
                if (Global.GetSaveStat(VanillaStats.CATEGORY_MAX_ENDLESS_FLAGS, level.StageID) < level.CurrentFlag)
                {
                    Global.SetSaveStat(VanillaStats.CATEGORY_MAX_ENDLESS_FLAGS, level.StageID, level.CurrentFlag);
                }
            }
        }
        private void GenerateMap(LevelEngine level, IZombieLayoutDefinition layout)
        {
            var map = new IZombieMap(level, layout.Columns, level.GetMaxLaneCount(), level.CurrentFlag);
            if (layout != null)
            {
                layout.Fill(map, level.GetSpawnRNG());
            }
            map.Apply();

            for (int lane = 0; lane < map.Lanes; lane++)
            {
                var x = level.GetColumnX(map.Columns);
                var z = level.GetLaneZ(lane) + level.GetGridHeight() * 0.5f;
                var y = level.GetGroundY(x, z);
                var pos = new Vector3(x, y, z);
                level.Spawn(VanillaEffectID.redline, pos, null);

                var observerX = level.GetColumnX(0) - level.GetGridWidth() * 0.5f;
                var observerY = level.GetGroundY(observerX, z);
                var observerPos = new Vector3(observerX, y, z);
                level.Spawn(VanillaEffectID.izObserver, observerPos, null);
            }
        }
        private void PostEnemyInitCallback(EntityCallbackParams param, CallbackResult result)
        {
            var entity = param.entity;
            var level = entity.Level;
            if (!level.IsIZombie())
                return;
            if (entity.Definition.HasBehaviour<MeleeEnemy>())
            {
                entity.AddBuff<IZombieAttackBoosterBuff>();
            }
            foreach (var buff in entity.GetBuffs<RandomEnemySpeedBuff>())
            {
                RandomEnemySpeedBuff.SetSpeed(buff, ZOMBIE_RANDOM_SPEED);
            }
        }
        private void CheckGameOver(LevelEngine level)
        {
            if (level.HasBuff<FlashWhiteBuff>())
                return;
            bool cannotAfford = false;
            if (level.GetSeedPackCount() <= 0)
            {
                cannotAfford = true;
            }
            else
            {
                var minSeedPackEnergy = level.GetAllSeedPacks().Where(e => e != null).Select(s => s.GetCost()).Min();
                if (level.Energy < minSeedPackEnergy)
                {
                    cannotAfford = true;
                }
            }
            if (cannotAfford)
            {
                // 存在有效怪物
                if (level.EntityExists(e => e.Type == EntityTypes.ENEMY && e.IsHostileEntity() && !e.IsNotActiveEnemy()))
                    return;
                // 存在能量掉落物
                if (level.EntityExists(e => e.Type == EntityTypes.PICKUP && e.GetEnergyValue() > 0))
                    return;
                // 可以使用铁镐并且存在熔炉
                if (level.CanUsePickaxe() && level.EntityExists(e => e.IsEntityOf(VanillaContraptionID.furnace)))
                    return;
                level.GameOver(GameOverTypes.INSTANT, null, VanillaStrings.DEATH_MESSAGE_IZ_LOSE_ALL_ENEMIES);
            }
        }
        public NamespaceID GetCurrentLayout(LevelEngine level) => level.GetBehaviourField<NamespaceID>(PROP_CURRENT_LAYOUT);
        public void SetCurrentLayout(LevelEngine level, NamespaceID value) => level.SetBehaviourField(PROP_CURRENT_LAYOUT, value);
        public FrameTimer GetRoundTimer(LevelEngine level) => level.GetBehaviourField<FrameTimer>(PROP_ROUND_TIMER);
        public void SetRoundTimer(LevelEngine level, FrameTimer value) => level.SetBehaviourField(PROP_ROUND_TIMER, value);

        #region 属性字段
        private const string PROP_REGION = "i_zombie_stage";
        public const int ROUND_COOLDOWN = 150;
        [LevelPropertyRegistry(PROP_REGION)]
        public static readonly VanillaLevelPropertyMeta PROP_ROUND_TIMER = new VanillaLevelPropertyMeta("RoundTimer");
        [LevelPropertyRegistry(PROP_REGION)]
        public static readonly VanillaLevelPropertyMeta PROP_CURRENT_LAYOUT = new VanillaLevelPropertyMeta("CurrentLayout");
        public const int STATE_NORMAL = VanillaLevelStates.STATE_IZ_NORMAL;
        public const int STATE_NEXT_ROUND = VanillaLevelStates.STATE_IZ_NEXT;
        public const int STATE_FINISHED = VanillaLevelStates.STATE_IZ_FINISHED;
        public const float ZOMBIE_RANDOM_SPEED = 1.5f;
        public virtual int MaxPickaxeCount => 3;
        public virtual bool AllowPickaxe => false;
        #endregion
    }
}
