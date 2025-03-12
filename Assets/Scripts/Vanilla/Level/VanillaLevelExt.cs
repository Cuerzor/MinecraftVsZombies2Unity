using System;
using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Areas;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.HeldItems;
using MVZ2.GameContent.Pickups;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using MVZ2.Vanilla.HeldItems;
using MVZ2.Vanilla.Saves;
using MVZ2.Vanilla.SeedPacks;
using MVZ2Logic;
using MVZ2Logic.Games;
using MVZ2Logic.Level;
using MVZ2Logic.SeedPacks;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.SeedPacks;
using PVZEngine.Triggers;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MVZ2.Vanilla.Level
{
    public static partial class VanillaLevelExt
    {
        public static bool IsDay(this LevelEngine level)
        {
            var areaTags = level.GetAreaTags();
            return areaTags.Contains(VanillaAreaTags.day);
        }
        public static void SpawnCarts(this LevelEngine game, NamespaceID cartRef, float x, float xInterval)
        {
            var carts = game.GetEntities(EntityTypes.CART);
            for (int i = 0; i < game.GetMaxLaneCount(); i++)
            {
                if (carts.Any(c => c.GetLane() == i && c.State == VanillaEntityStates.IDLE))
                    continue;
                Entity cart = game.Spawn(cartRef, new Vector3(x - i * xInterval, 0, game.GetEntityLaneZ(i)), null);
            }
        }
        public static void CheckGameOver(this LevelEngine level)
        {
            var gameOverEnemies = level.FindEntities(e => e.Position.x < GetBorderX(false) && e.CanEntityEnterHouse());
            if (gameOverEnemies.Length > 0)
            {
                level.GameOver(GameOverTypes.ENEMY, gameOverEnemies.FirstOrDefault(), null);
            }
        }
        public static DamageOutput[] Explode(this LevelEngine level, Vector3 center, float radius, int faction, float amount, DamageEffectList effects, Entity source)
        {
            return level.Explode(center, radius, faction, amount, effects, new EntityReferenceChain(source));
        }
        public static DamageOutput[] Explode(this LevelEngine level, Vector3 center, float radius, int faction, float amount, DamageEffectList effects, EntityReferenceChain source)
        {
            List<DamageOutput> damageOutputs = new List<DamageOutput>();
            foreach (IEntityCollider entityCollider in level.OverlapSphere(center, radius, faction, EntityCollisionHelper.MASK_VULNERABLE, 0))
            {
                var damageOutput = entityCollider.TakeDamage(amount, effects, source);
                if (damageOutput != null)
                {
                    damageOutputs.Add(damageOutput);
                }
            }
            return damageOutputs.ToArray();
        }
        public static NamespaceID GetHeldSeedEntityID(this LevelEngine level)
        {
            var heldType = level.GetHeldItemType();
            var heldDefinition = level.Content.GetHeldItemDefinition(heldType);
            if (heldDefinition == null)
                return null;
            var seed = heldDefinition.GetSeedPack(level, level.GetHeldItemData());
            var seedDef = seed?.Definition;
            if (seedDef == null)
                return null;
            if (seedDef.GetSeedType() != SeedTypes.ENTITY)
                return null;
            return seedDef.GetSeedEntityID();
        }
        public static bool IsHoldingItem(this LevelEngine level)
        {
            var type = level.GetHeldItemType();
            return NamespaceID.IsValid(type) && type != BuiltinHeldTypes.none;
        }
        public static bool IsHoldingPickaxe(this LevelEngine level)
        {
            return level.GetHeldItemType() == VanillaHeldTypes.pickaxe;
        }
        public static bool IsHoldingStarshard(this LevelEngine level)
        {
            return level.GetHeldItemType() == VanillaHeldTypes.starshard;
        }
        public static bool IsHoldingBlueprint(this LevelEngine level, SeedPack seedPack)
        {
            if (seedPack is ClassicSeedPack classic)
            {
                var classicIndex = level.GetSeedPackIndex(classic);
                if (classicIndex >= 0)
                {
                    return level.IsHoldingClassicBlueprint(classicIndex);
                }
            }
            if (seedPack is ConveyorSeedPack conveyor)
            {
                var conveyorIndex = level.GetConveyorSeedPackIndex(conveyor);
                if (conveyorIndex >= 0)
                {
                    return level.IsHoldingConveyorBlueprint(conveyorIndex);
                }
            }
            return false;
        }
        public static bool IsHoldingClassicBlueprint(this LevelEngine level, int i)
        {
            return level.GetHeldItemType() == BuiltinHeldTypes.blueprint && level.GetHeldItemID() == i;
        }
        public static bool IsHoldingConveyorBlueprint(this LevelEngine level, int i)
        {
            return level.GetHeldItemType() == BuiltinHeldTypes.conveyor && level.GetHeldItemID() == i;
        }
        public static bool IsHoldingTrigger(this LevelEngine level)
        {
            return level.GetHeldItemType() == VanillaHeldTypes.trigger;
        }
        public static bool IsHoldingEntity(this LevelEngine level, Entity entity)
        {
            return level.GetHeldItemType() == VanillaHeldTypes.entity && level.GetHeldItemID() == entity.ID;
        }
        public static void CreatePreviewEnemies(this LevelEngine level, IEnumerable<NamespaceID> spawnsID, Rect region)
        {
            List<SpawnDefinition> spawnToCreate = new List<SpawnDefinition>();
            foreach (var spawnID in spawnsID)
            {
                var spawnDefinition = level.Content.GetSpawnDefinition(spawnID);
                int count = spawnDefinition.GetPreviewCount();

                for (int i = 0; i < count; i++)
                {
                    spawnToCreate.Add(spawnDefinition);
                }
            }

            List<Entity> createdEnemies = new List<Entity>();
            float radius = 80;
            while (spawnToCreate.Count > 0)
            {
                var creatingSpawnDef = spawnToCreate.ToArray();
                foreach (var spawnDef in creatingSpawnDef)
                {
                    var x = Random.Range(region.xMin, region.xMax);
                    var z = Random.Range(region.yMin, region.yMax);
                    var y = level.GetGroundY(x, z);
                    Vector3 pos = new Vector3(x, y, z);

                    if (radius > 0 && createdEnemies.Any(e => Vector3.Distance(e.Position, pos) < radius))
                        continue;

                    var param = new SpawnParams();
                    param.SetProperty(VanillaEnemyProps.PREVIEW_ENEMY, true);
                    Entity enm = level.Spawn(spawnDef.EntityID, pos, null, param);
                    createdEnemies.Add(enm);

                    spawnToCreate.Remove(spawnDef);
                }
                radius--;
            }
        }
        #region Waves
        public static void PostWaveFinished(this LevelEngine level, int wave)
        {
            if (level.IsHugeWave(wave))
            {
                level.CurrentFlag++;
            }
            level.Triggers.RunCallbackFiltered(LevelCallbacks.POST_WAVE_FINISHED, wave, c => c(level, wave));
        }
        public static void NextWave(this LevelEngine level)
        {
            level.PostWaveFinished(level.CurrentWave);
            level.CurrentWave++;
            level.RunWave();
        }
        public static void RunWave(this LevelEngine level)
        {
            level.SpawnWaveEnemies(level.CurrentWave);

            var wave = level.CurrentWave;
            level.StageDefinition.PostWave(level, wave);
            level.Triggers.RunCallback(LevelCallbacks.POST_WAVE, c => c(level, wave));
        }
        public static int GetLevelTotalWaves(this LevelEngine level, int wave, int flags)
        {
            var wavesPerFlag = level.GetWavesPerFlag();
            var waveModular = (wave - 1) % wavesPerFlag + 1;
            try
            {
                return checked(waveModular + flags * wavesPerFlag);
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
        }
        /// <summary>
        /// 波次生成敌人所需要的总等级花费。
        /// </summary>
        /// <param name="level"></param>
        /// <param name="wave"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static float CalculateSpawnPoints(this LevelEngine level, int wave, int flags)
        {
            var totalWave = level.GetLevelTotalWaves(wave, flags);
            var basePoints = Mathf.FloorToInt(totalWave * 0.8f) / 2 + 1;
            var power = level.GetSpawnPointPower();
            var multiplier = level.GetSpawnPointMultiplier();
            var addition = level.GetSpawnPointAddition();
            if (level.IsHugeWave(wave) && (level.WaveState == VanillaLevelStates.STATE_STARTED || level.WaveState == VanillaLevelStates.STATE_FINAL_WAVE))
            {
                multiplier *= 2.5f;
            }
            return Mathf.Ceil(Mathf.Min(Mathf.Pow(basePoints, power) * multiplier + addition, 500));
        }
        public static float CalculateSpawnPoints(this LevelEngine level)
        {
            return level.CalculateSpawnPoints(level.CurrentWave, level.CurrentFlag);
        }
        /// <summary>
        /// 本波最高生成的敌人等级，防止前期生成无法处理的怪物。
        /// </summary>
        /// <param name="level"></param>
        /// <param name="wave"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static float GetWaveSpawnLevelLimit(this LevelEngine level, int wave, int flags)
        {
            var totalWave = level.GetLevelTotalWaves(wave, flags);
            var limit = totalWave * 0.4f + 1;
            if (level.IsHugeWave(wave))
            {
                limit *= 2.5f;
            }
            return Mathf.Floor(limit);
        }
        public static void SpawnWaveEnemies(this LevelEngine level, int wave)
        {
            // 获取本波的生成点数。
            var totalPoints = level.CalculateSpawnPoints();

            // 本波最高生成的敌人等级，防止前期生成无法处理的怪物。
            var currentWaveLevelLimit = level.GetWaveSpawnLevelLimit(wave, level.CurrentFlag);

            // 当前的有效敌人池。
            var pool = level.GetEnemyPool();
            var spawnDefs = pool.Select(id => level.Content.GetSpawnDefinition(id)).Where(def => def != null && def.SpawnLevel > 0);

            // 已生成怪物数量。
            int spawnedCount = 0;

            // 没有剩余点数，或者生成的敌人数量大于50只，则中断。
            while (totalPoints > 0 && spawnedCount < 50)
            {
                // 当前所有可以生成的敌人。
                // 不被当前波次限制。
                var possibleSpawnDefs = spawnDefs.Where(def => def.SpawnLevel <= totalPoints);

                // 没有可生成的敌人了，跳出
                if (possibleSpawnDefs.Count() <= 0)
                    break;

                IEnumerable<SpawnDefinition> finalSpawnPool = possibleSpawnDefs;
                // 根据当前波次的等级限制获取有效的敌人。
                var limitedSpawnDefs = possibleSpawnDefs.Where(def => def.SpawnLevel <= currentWaveLevelLimit);

                // 如果根据当前波次限制之后，没有可以生成的敌人，则不进行限制。
                if (limitedSpawnDefs.Count() > 0)
                {
                    finalSpawnPool = limitedSpawnDefs;
                }

                // 随机生成一个敌人。
                var rng = level.GetSpawnRNG();
                var spawnDef = finalSpawnPool.WeightedRandom(i => i.GetWeight(level), rng);
                level.SpawnEnemyAtRandomLane(spawnDef);
                totalPoints -= spawnDef.SpawnLevel;
                spawnedCount++;
            }

            if (level.IsFinalWave(wave))
            {
                // 最后一波如果还有没生成过的敌人，强制全部生成一次
                var notSpawnedDefs = spawnDefs.Where(def => !level.IsEnemySpawned(def.GetID()));
                foreach (var notSpawnedDef in notSpawnedDefs)
                {
                    level.SpawnEnemyAtRandomLane(notSpawnedDef);
                }
            }
        }
        public static bool WillEnemySpawn(this LevelEngine level, NamespaceID spawnID)
        {
            var pool = level.GetEnemyPool();
            if (pool == null)
                return false;
            return pool.Any(e => e == spawnID);
        }
        public static Entity SpawnEnemyAtRandomLane(this LevelEngine level, SpawnDefinition spawnDef)
        {
            if (spawnDef == null)
                return null;
            var lane = spawnDef.GetRandomSpawnLane(level);
            return level.SpawnEnemy(spawnDef, lane);
        }
        public static Entity SpawnEnemy(this LevelEngine level, SpawnDefinition spawnDef, int lane)
        {
            if (spawnDef == null)
                return null;
            var x = level.GetEnemySpawnX();
            var z = level.GetEntityLaneZ(lane);
            var y = level.GetGroundY(x, z);
            var pos = new Vector3(x, y, z);
            var enemy = level.Spawn(spawnDef.EntityID, pos, null);
            level.AddSpawnedEnemyID(spawnDef.GetID());
            level.StageDefinition.PostEnemySpawned(enemy);
            level.Triggers.RunCallback(LevelCallbacks.POST_ENEMY_SPAWNED, c => c(enemy));
            return enemy;
        }
        public static Entity SpawnFlagZombie(this LevelEngine level)
        {
            var lane = level.GetRandomEnemySpawnLane();
            return level.SpawnFlagZombie(lane);
        }
        public static Entity SpawnFlagZombie(this LevelEngine level, int lane)
        {
            var x = level.GetEnemySpawnX();
            var z = level.GetEntityLaneZ(lane);
            var y = level.GetGroundY(x, z);
            var pos = new Vector3(x, y, z);
            var enemy = level.Spawn(VanillaEnemyID.flagZombie, pos, null);
            return enemy;
        }
        #endregion

        #region 预览敌人
        public static void CreatePreviewEnemies(this LevelEngine level, Rect region)
        {
            var pool = level.GetEnemyPool();
            var spawnIDs = pool.Where(e => NamespaceID.IsValid(e));
            level.CreatePreviewEnemies(spawnIDs, region);
        }
        public static void RemovePreviewEnemies(this LevelEngine level)
        {
            foreach (var enemy in level.FindEntities(e => e.IsPreviewEnemy()))
            {
                enemy.Remove();
            }
        }
        #endregion

        #region Positions

        public static float GetLeftUIBorderX(this LevelEngine level)
        {
            if (Global.IsMobile())
            {
                return 160;
            }
            return GetBorderX(false);
        }
        public static Vector2 GetMoneyPanelEntityPosition(this LevelEngine level)
        {
            var x = level.GetLeftUIBorderX() + MONEY_PANEL_X_TO_LEFT;
            var y = MONEY_PANEL_Y_TO_BOTTOM;
            return new Vector2(x, y);
        }
        public static Vector2 GetStarshardEntityPosition(this LevelEngine level)
        {
            var x = level.GetLeftUIBorderX() + STARSHARD_X_TO_LEFT;
            var y = STARSHARD_Y_TO_BOTTOM;
            return new Vector2(x, y);
        }
        public static Vector2 GetEnergySlotEntityPosition(this LevelEngine level)
        {
            var x = level.GetLeftUIBorderX() + ENERGY_SLOT_WIDTH * 0.5f;
            var y = GetScreenHeight() - ENERGY_SLOT_WIDTH * 0.5f;
            return new Vector2(x, y);
        }
        public static Vector2 GetScreenCenterPosition(this LevelEngine level)
        {
            var x = level.GetLeftUIBorderX() + SCREEN_WIDTH * 0.5f;
            var y = SCREEN_HEIGHT * 0.5f;
            return new Vector2(x, y);
        }
        public static Rect GetEnemySpawnRect()
        {
            return new Rect(MIN_PREVIEW_X, MIN_PREVIEW_Y, MAX_PREVIEW_X - MIN_PREVIEW_X, MAX_PREVIEW_Y - MIN_PREVIEW_Y);
        }
        public static float GetScreenHeight()
        {
            return SCREEN_HEIGHT;
        }
        public static float GetBorderX(bool right)
        {
            return right ? RIGHT_BORDER : LEFT_BORDER;
        }
        public static float GetAttackBorderX(bool right)
        {
            return right ? ATTACK_RIGHT_BORDER : ATTACK_LEFT_BORDER;
        }
        public static float GetPickupBorderX(bool right)
        {
            return right ? PICKUP_RIGHT_BORDER : PICKUP_LEFT_BORDER;
        }
        public static float GetEnemyRightBorderX()
        {
            return ENEMY_RIGHT_BORDER;
        }
        public const float ENERGY_SLOT_WIDTH = 48;

        public const float MONEY_PANEL_X_TO_LEFT = 16;
        public const float MONEY_PANEL_Y_TO_BOTTOM = 32;

        public const float STARSHARD_X_TO_LEFT = 128 + 165 + 16;
        public const float STARSHARD_Y_TO_BOTTOM = 32;

        public const float MIN_PREVIEW_X = 1080;
        public const float MAX_PREVIEW_X = 1300;
        public const float MIN_PREVIEW_Y = 50;
        public const float MAX_PREVIEW_Y = 450;
        public const float GRID_SIZE = 80;
        public const float LAWN_HEIGHT = 600;
        public const float LEVEL_WIDTH = 1400;
        public const float CART_START_X = 150;
        public const float CART_TARGET_X = LEFT_BORDER;
        public const float SCREEN_WIDTH = 800;
        public const float SCREEN_HEIGHT = 600;
        public const float LEFT_BORDER = 220;
        public const float RIGHT_BORDER = LEFT_BORDER + SCREEN_WIDTH;
        public const float LAWN_CENTER_X = (LEFT_BORDER + RIGHT_BORDER) * 0.5f;
        public const float PICKUP_LEFT_BORDER = LEFT_BORDER + 50;
        public const float PICKUP_RIGHT_BORDER = RIGHT_BORDER - 50;
        public const float ATTACK_LEFT_BORDER = LEFT_BORDER;
        public const float ATTACK_RIGHT_BORDER = RIGHT_BORDER;

        public const float ENEMY_LEFT_BORDER = LEFT_BORDER - 60;
        public const float ENEMY_RIGHT_BORDER = RIGHT_BORDER + 60;


        public const float PROJECTILE_LEFT_BORDER = LEFT_BORDER - 40;
        public const float PROJECTILE_RIGHT_BORDER = RIGHT_BORDER + 40;
        public const float PROJECTILE_UP_BORDER = 540;
        public const float PROJECTILE_DOWN_BORDER = -40;
        public const float PROJECTILE_TOP_BORDER = 1000;
        public const float PROJECTILE_BOTTOM_BORDER = -1000;
        #endregion

        #region Wave
        public static void CheckClearUpdate(this LevelEngine level)
        {
            var lastEnemy = level.FindFirstEntity(e => e.IsAliveEnemy());
            if (lastEnemy != null)
            {
                level.SetLastEnemyPosition(lastEnemy.Position);
            }
            else
            {
                level.PostWaveFinished(level.CurrentWave);
                level.SetNoEnergy(true);
                if (!level.IsAllEnemiesCleared())
                {
                    level.SetAllEnemiesCleared(true);
                    var lastEnemyPosition = level.GetLastEnemyPosition();
                    Vector3 position;
                    if (lastEnemyPosition.x <= VanillaLevelExt.GetBorderX(false))
                    {
                        var x = level.GetEnemySpawnX();
                        var z = level.GetEntityLaneZ(Mathf.CeilToInt(level.GetMaxLaneCount() * 0.5f));
                        var y = level.GetGroundY(x, z);
                        position = new Vector3(x, y, z);
                    }
                    else
                    {
                        position = lastEnemyPosition;
                    }
                    level.Produce(VanillaPickupID.clearPickup, position, null);
                }
            }
        }
        public static T GetStageBehaviour<T>(this LevelEngine level) where T : StageBehaviour
        {
            if (level == null || level.StageDefinition == null)
                return null;
            return level.StageDefinition.GetBehaviour<T>();
        }
        #endregion
        public static SeedPack ConveyRandomSeedPack(this LevelEngine level)
        {
            if (level.CanConveySeedPack())
            {
                var id = level.DrawConveyorSeed();
                var seedPack = level.AddConveyorSeedPack(id ?? VanillaContraptionID.dispenser);
                if (seedPack != null)
                {
                    seedPack.SetDrawnConveyorSeed(seedPack.GetDefinitionID());
                }
                else
                {
                    level.PutSeedToConveyorPool(id);
                }
                return seedPack;
            }
            return null;
        }
        public static NamespaceID DrawConveyorSeed(this LevelEngine level)
        {
            var entries = level.GetConveyorPool();
            if (entries.Count() <= 0)
                return null;
            var index = level.GetConveyorRNG().WeightedRandom(entries.Select(e => Mathf.Max(e.MinCount, e.Count - level.GetSpentSeedFromConveyorPool(e.ID))).ToArray());
            var entry = entries[index];
            level.SpendSeedFromConveyorPool(entry.ID);
            return entry.ID;
        }
        public static void Thunder(this LevelEngine level)
        {
            level.AddBuff<ThunderBuff>();
            foreach (var ghost in level.GetEntities())
            {
                foreach (var buff in ghost.GetBuffs<GhostBuff>())
                {
                    GhostBuff.Illuminate(buff);
                }
            }
            level.PlaySound(VanillaSoundID.thunder);
        }
        public static void StartRain(this LevelEngine level)
        {
            level.Spawn(VanillaEffectID.rain, new Vector3(VanillaLevelExt.LEVEL_WIDTH * 0.5f, 0, 0), null);
        }
        public static IEnumerable<int> GetAllLanes(this LevelEngine level)
        {
            return Enumerable.Range(0, level.GetMaxLaneCount());
        }
        public static IEnumerable<int> GetWaterLanes(this LevelEngine level)
        {
            return level.GetAllLanes().Where(l => level.IsWaterLane(l));
        }
        public static bool IsWaterLane(this LevelEngine level, int lane)
        {
            for (int column = 0; column < level.GetMaxColumnCount(); column++)
            {
                var grid = level.GetGrid(column, lane);
                if (grid == null)
                    continue;
                if (grid.IsWater())
                    return true;
            }
            return false;
        }
        public static bool IsWaterGrid(this LevelEngine level, int column, int lane)
        {
            var grid = level.GetGrid(column, lane);
            if (grid == null)
                return false;
            return grid.IsWater();
        }
        public static bool IsWaterAt(this LevelEngine level, float x, float z)
        {
            var column = level.GetColumn(x);
            var lane = level.GetLane(z);
            return level.IsWaterGrid(column, lane);
        }
        public static void GetConnectedWaterGrids(this LevelEngine level, Vector3 pos, int xExpand, int yExpand, HashSet<LawnGrid> results)
        {
            var column = level.GetColumn(pos.x);
            var lane = level.GetLane(pos.z);
            level.GetConnectedWaterGrids(column, lane, xExpand, yExpand, results);
        }
        public static void GetConnectedWaterGrids(this LevelEngine level, int column, int lane, int xExpand, int yExpand, HashSet<LawnGrid> results)
        {
            for (int xOff = -xExpand; xOff <= xExpand; xOff++)
            {
                for (int yOff = -yExpand; yOff <= yExpand; yOff++)
                {
                    var col = column + xOff;
                    var lan = lane + yOff;
                    var grid = level.GetGrid(col, lan);
                    if (grid != null && grid.IsWater())
                    {
                        results.Add(grid);
                    }
                }
            }
        }
        public static void UpdatePersistentLevelUnlocks(this LevelEngine level)
        {
            var game = Global.Game;
            level.SetSeedSlotCount(game.GetBlueprintSlots());
            level.SetStarshardSlotCount(game.GetStarshardSlots());
            level.SetArtifactSlotCount(game.GetArtifactSlots());
        }
    }
}
