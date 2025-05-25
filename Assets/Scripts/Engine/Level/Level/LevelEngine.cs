using System;
using System.Collections.Generic;
using System.Linq;
using PVZEngine.Armors;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level.Collisions;
using PVZEngine.Models;
using PVZEngine.Modifiers;
using PVZEngine.SeedPacks;
using Tools;
using UnityEngine;

namespace PVZEngine.Level
{
    public partial class LevelEngine : IBuffTarget, IDisposable, IPropertyModifyTarget
    {
        #region 公有方法
        public LevelEngine(IGameContent contentProvider, IGameLocalization translator, IGameTriggerSystem triggers, ICollisionSystem collisionSystem)
        {
            Content = contentProvider;
            Localization = translator;
            Triggers = triggers;
            buffs.OnPropertyChanged += UpdateBuffedProperty;
            properties = new PropertyBlock(this);
            this.collisionSystem = collisionSystem;
        }


        public void Dispose()
        {
            RemoveTriggers(addedTriggers);
        }

        #region 组件
        public void AddComponent(ILevelComponent component)
        {
            levelComponents.Add(component);
        }
        public ILevelComponent[] GetComponents()
        {
            return levelComponents.ToArray();
        }
        public T GetComponent<T>() where T : ILevelComponent
        {
            foreach (var comp in levelComponents)
            {
                if (comp is T tComp)
                    return tComp;
            }
            return default;
        }
        #endregion

        #region 生命周期
        public void Init(NamespaceID areaId, NamespaceID stageId, LevelOption option, int seed = 0)
        {
            Seed = seed == 0 ? Guid.NewGuid().GetHashCode() : seed;

            Option = option;

            levelRandom = new RandomGenerator(Seed);
            entityRandom = CreateRNG();
            effectRandom = CreateRNG();
            roundRandom = CreateRNG();
            spawnRandom = CreateRNG();
            conveyorRandom = CreateRNG();

            miscRandom = CreateRNG();

            ChangeArea(areaId);
            ChangeStage(stageId);

            Energy = this.GetStartEnergy();

            InitAreaProperties();

            // Initalize current stage info.
            var maxLane = GetMaxLaneCount();
            int maxColumn = GetMaxColumnCount();

            grids = new LawnGrid[maxColumn * maxLane];

            var gridDefinitions = AreaDefinition.GetGridLayout().Select(i => Content.GetGridDefinition(i)).ToArray();
            for (int i = 0; i < gridDefinitions.Length; i++)
            {
                var definition = gridDefinitions[i];
                int lane = Mathf.FloorToInt(i / maxColumn);
                int column = i % maxColumn;
                grids[i] = new LawnGrid(this, definition, lane, column);
            }
        }
        public void Setup()
        {
            AreaDefinition.Setup(this);
            StageDefinition.Setup(this);
            Triggers.RunCallback(LevelCallbacks.POST_LEVEL_SETUP, new LevelCallbackParams(this));
        }
        public void Start()
        {
            foreach (var component in levelComponents)
            {
                component.OnStart();
            }
            StageDefinition.Start(this);
            Triggers.RunCallback(LevelCallbacks.POST_LEVEL_START, new LevelCallbackParams(this));
        }
        public void SetDifficulty(NamespaceID difficulty)
        {
            Difficulty = difficulty;
        }
        public void ChangeStage(NamespaceID stageId)
        {
            StageID = stageId;
            StageDefinition = Content.GetStageDefinition(stageId);

        }
        public void ChangeArea(NamespaceID areaId)
        {
            AreaID = areaId;
            AreaDefinition = Content.GetAreaDefinition(areaId);
        }
        public void Update()
        {
            ClearEntityTrash();
            UpdateSeedPacks();
            UpdateDelayedEnergyEntities();

            UpdateEntities();
            CollisionUpdate();

            buffs.Update();
            foreach (var component in levelComponents)
            {
                component.Update();
            }
            AreaDefinition.Update(this);
            StageDefinition.Update(this);
            Triggers.RunCallback(LevelCallbacks.POST_LEVEL_UPDATE, new LevelCallbackParams(this));
        }
        public void Clear()
        {
            IsCleared = true;
            OnClear?.Invoke();
            Triggers.RunCallback(LevelCallbacks.POST_LEVEL_CLEAR, new LevelCallbackParams(this));
        }
        #endregion

        #region 属性
        public T GetProperty<T>(PropertyKey name, bool ignoreBuffs = false)
        {
            return properties.GetProperty<T>(name, ignoreBuffs);
        }
        public bool TryGetProperty<T>(PropertyKey name, out T value, bool ignoreBuffs = false)
        {
            return properties.TryGetProperty<T>(name, out value, ignoreBuffs);
        }
        public void SetProperty(PropertyKey name, object value)
        {
            properties.SetProperty(name, value);
        }
        private void UpdateAllBuffedProperties()
        {
            properties.UpdateAllModifiedProperties();
        }
        private void UpdateBuffedProperty(PropertyKey name)
        {
            properties.UpdateModifiedProperty(name);
        }
        bool IPropertyModifyTarget.GetFallbackProperty(PropertyKey name, out object value)
        {
            if (StageDefinition != null && StageDefinition.TryGetProperty(name, out var stageProp))
            {
                value = stageProp;
                return true;
            }
            if (AreaDefinition != null && AreaDefinition.TryGetProperty(name, out var areaProp))
            {
                value = areaProp;
                return true;
            }
            value = null;
            return false;
        }

        void IPropertyModifyTarget.GetModifierItems(PropertyKey name, List<ModifierContainerItem> results)
        {
            buffs.GetModifierItems(name, results);
        }
        void IPropertyModifyTarget.UpdateModifiedProperty(PropertyKey name, object beforeValue, object afterValue)
        {
        }
        PropertyModifier[] IPropertyModifyTarget.GetModifiersUsingProperty(PropertyKey name)
        {
            return null;
        }
        IEnumerable<PropertyKey> IPropertyModifyTarget.GetModifiedProperties()
        {
            return buffs.GetModifierPropertyNames();
        }
        #endregion

        #region 坐标相关方法
        public int GetGridIndex(int column, int lane)
        {
            return column + lane * GetMaxColumnCount();
        }
        public int GetGridLaneByIndex(int index)
        {
            return index / GetMaxColumnCount();
        }
        public int GetGridColumnByIndex(int index)
        {
            return index % GetMaxColumnCount();
        }
        public float GetGridWidth()
        {
            return gridWidth;
        }
        public float GetGridHeight()
        {
            return gridHeight;
        }
        public float GetGridRightX()
        {
            return GetGridLeftX() + GetMaxColumnCount() * GetGridWidth();
        }
        public float GetGridLeftX()
        {
            return gridLeftX;
        }
        public float GetGridTopZ()
        {
            return GetGridBottomZ() + GetMaxLaneCount() * GetGridHeight();
        }
        public float GetGridBottomZ()
        {
            return gridBottomZ;
        }
        public float GetLawnCenterX()
        {
            return (GetGridLeftX() + GetGridRightX()) * 0.5f;
        }
        public float GetLawnCenterZ()
        {
            return (GetGridBottomZ() + GetGridTopZ()) * 0.5f;
        }
        public int GetMaxLaneCount()
        {
            return maxLaneCount;
        }
        public int GetMaxColumnCount()
        {
            return maxColumnCount;
        }
        public int GetLane(float z)
        {
            return Mathf.FloorToInt((GetGridTopZ() - z) / GetGridHeight());
        }
        public int GetColumn(float x)
        {
            return Mathf.FloorToInt((x - GetGridLeftX()) / GetGridWidth());
        }
        public int GetNearestEntityLane(float z)
        {
            return GetLane(z - entityLaneZOffset + GetGridHeight() * 0.5f);
        }
        public float GetEntityLaneZ(int row)
        {
            return GetLaneZ(row) + entityLaneZOffset;
        }
        public float GetEntityColumnX(int column)
        {
            return GetColumnX(column) + GetGridWidth() * 0.5f;
        }
        public float GetColumnX(int column)
        {
            return GetGridLeftX() + column * GetGridWidth();
        }
        public Vector3 GetEntityGridPosition(int column, int lane)
        {
            var x = GetEntityColumnX(column);
            var z = GetEntityLaneZ(lane);
            var y = GetGroundY(x, z);
            return new Vector3(x, y, z);
        }
        public Vector3 GetEntityGridPositionByIndex(int index)
        {
            var column = GetGridColumnByIndex(index);
            var lane = GetGridLaneByIndex(index);
            return GetEntityGridPosition(column, lane);
        }
        public float GetLaneZ(int lane)
        {
            return GetGridTopZ() - (lane + 1) * GetGridHeight();
        }
        public float GetGroundY(Vector3 pos)
        {
            return GetGroundY(pos.x, pos.z);
        }
        public float GetGroundY(float x, float z)
        {
            return AreaDefinition.GetGroundY(this, x, z);
        }
        public LawnGrid GetGrid(int index)
        {
            if (index < 0 || index >= GetMaxColumnCount() * GetMaxLaneCount())
                return null;
            return grids[index];
        }

        public LawnGrid GetGrid(int column, int lane)
        {
            if (column < 0 || column >= GetMaxColumnCount() || lane < 0 || lane >= GetMaxLaneCount())
                return null;
            return GetGrid(lane * GetMaxColumnCount() + column);
        }

        public LawnGrid GetGrid(Vector2Int pos)
        {
            return GetGrid(pos.x, pos.y);
        }

        public LawnGrid[] GetAllGrids()
        {
            return grids.ToArray();
        }
        #endregion 坐标相关方法

        #region 时间
        public int GetSecondTicks(float second)
        {
            return Mathf.CeilToInt(second * TPS);
        }
        #endregion

        #region 增益
        public bool AddBuff(Buff buff)
        {
            if (buffs.AddBuff(buff))
            {
                buff.AddToTarget(this);
                return true;
            }
            return false;
        }
        public Buff AddBuff<T>() where T : BuffDefinition
        {
            var buff = CreateBuff<T>(AllocBuffID());
            AddBuff(buff);
            return buff;
        }
        public Buff AddBuff(NamespaceID buffID)
        {
            var buffDef = Content.GetBuffDefinition(buffID);
            return AddBuff(buffDef);
        }
        public Buff AddBuff(BuffDefinition buffDef)
        {
            if (buffDef == null)
                return null;
            var buff = CreateBuff(buffDef, AllocBuffID());
            buffs.AddBuff(buff);
            return buff;
        }
        public bool RemoveBuff(Buff buff) => buffs.RemoveBuff(buff);
        public int RemoveBuffs<T>() where T : BuffDefinition => this.buffs.RemoveBuffs<T>();
        public int RemoveBuffs(IEnumerable<Buff> buffs) => this.buffs.RemoveBuffs(buffs);
        public int RemoveBuffs(BuffDefinition buffDef) => this.buffs.RemoveBuffs(buffDef);
        public bool HasBuff<T>() where T : BuffDefinition => buffs.HasBuff<T>();
        public bool HasBuff(Buff buff) => buffs.HasBuff(buff);
        public bool HasBuff(BuffDefinition buffDef) => buffs.HasBuff(buffDef);
        public Buff[] GetBuffs<T>() where T : BuffDefinition => buffs.GetBuffs<T>();
        public void GetBuffs<T>(List<Buff> results) where T : BuffDefinition => buffs.GetBuffsNonAlloc<T>(results);
        public void GetAllBuffs(List<Buff> results) => buffs.GetAllBuffs(results);
        public BuffReference GetBuffReference(Buff buff) => new BuffReferenceLevel(buff.ID);
        private long AllocBuffID()
        {
            long id = currentBuffID;
            currentBuffID++;
            return id;
        }
        public Buff CreateBuff<T>(long buffID) where T : BuffDefinition
        {
            var buffDefinition = Content.GetBuffDefinition<T>();
            return CreateBuff(buffDefinition, buffID);
        }
        public Buff CreateBuff(NamespaceID id, long buffID)
        {
            var buffDefinition = Content.GetBuffDefinition(id);
            return CreateBuff(buffDefinition, buffID);
        }
        public Buff CreateBuff(BuffDefinition buffDef, long buffID)
        {
            if (buffDef == null)
                return null;
            return new Buff(this, buffDef, buffID);
        }

        #endregion

        #region 序列化
        public SerializableLevel Serialize()
        {
            return new SerializableLevel()
            {
                seed = Seed,
                isCleared = IsCleared,
                stageDefinitionID = StageDefinition.GetID(),
                areaDefinitionID = AreaDefinition.GetID(),
                difficulty = Difficulty,
                Option = Option.Serialize(),

                levelRandom = levelRandom.ToSerializable(),
                entityRandom = entityRandom.ToSerializable(),
                effectRandom = effectRandom.ToSerializable(),
                roundRandom = roundRandom.ToSerializable(),
                spawnRandom = spawnRandom.ToSerializable(),
                conveyorRandom = conveyorRandom.ToSerializable(),
                miscRandom = miscRandom.ToSerializable(),

                properties = properties.ToSerializable(),
                grids = grids.Select(g => g.Serialize()).ToArray(),
                seedPacks = seedPacks.Select(g => g != null ? g.Serialize() : null).ToArray(),
                conveyorSeedPacks = conveyorSeedPacks.Select(s => s != null ? s.Serialize() : null).ToArray(),
                conveyorSlotCount = conveyorSlotCount,
                conveyorSeedSpendRecord = conveyorSeedSpendRecord.ToSerializable(),
                collisionSystem = collisionSystem.ToSerializable(),

                currentEntityID = currentEntityID,
                currentBuffID = currentBuffID,
                currentSeedPackID = currentSeedPackID,
                entities = entities.Values.Select(e => e.Serialize()).ToList(),
                entityTrash = entityTrash.Values.Select(e => e.Serialize()).ToList(),

                energy = Energy,
                delayedEnergyEntities = delayedEnergyEntities.Select(d => new SerializableDelayedEnergy() { entityId = d.Key.ID, energy = d.Value }).ToArray(),

                currentWave = CurrentWave,
                currentFlag = CurrentFlag,
                waveState = WaveState,
                levelProgressVisible = LevelProgressVisible,
                spawnedLanes = spawnedLanes,
                spawnedID = spawnedID,

                buffs = buffs.ToSerializable(),

                components = levelComponents.ToDictionary(c => c.GetID().ToString(), c => c.ToSerializable())
            };
        }
        public static LevelEngine Deserialize(SerializableLevel seri, IGameContent provider, IGameLocalization translator, IGameTriggerSystem triggers, ICollisionSystem collisionSystem)
        {
            var level = new LevelEngine(provider, translator, triggers, collisionSystem);
            level.Seed = seri.seed;
            level.levelRandom = RandomGenerator.FromSerializable(seri.levelRandom);
            level.entityRandom = RandomGenerator.FromSerializable(seri.entityRandom);
            level.effectRandom = RandomGenerator.FromSerializable(seri.effectRandom);
            level.roundRandom = RandomGenerator.FromSerializable(seri.roundRandom);
            level.spawnRandom = RandomGenerator.FromSerializable(seri.spawnRandom);
            level.conveyorRandom = RandomGenerator.FromSerializable(seri.conveyorRandom);
            level.miscRandom = RandomGenerator.FromSerializable(seri.miscRandom);

            level.IsCleared = seri.isCleared;
            level.ChangeStage(seri.stageDefinitionID);
            level.ChangeArea(seri.areaDefinitionID);
            level.InitAreaProperties();

            level.Difficulty = seri.difficulty;
            level.Option = LevelOption.Deserialize(seri.Option);
            level.grids = seri.grids.Select(g => LawnGrid.Deserialize(g, level)).ToArray();
            level.properties = PropertyBlock.FromSerializable(seri.properties, level);

            level.seedPacks = seri.seedPacks.Select(g => g != null ? ClassicSeedPack.Deserialize(g, level) : null).ToArray();
            level.conveyorSeedPacks = seri.conveyorSeedPacks.Select(s => s != null ? ConveyorSeedPack.Deserialize(s, level) : null).ToList();
            level.conveyorSlotCount = seri.conveyorSlotCount;
            level.conveyorSeedSpendRecord = ConveyorSeedSpendRecords.ToDeserialized(seri.conveyorSeedSpendRecord);

            level.currentEntityID = seri.currentEntityID;
            level.currentBuffID = seri.currentBuffID;
            level.currentSeedPackID = seri.currentSeedPackID;


            level.CurrentWave = seri.currentWave;
            level.CurrentFlag = seri.currentFlag;
            level.WaveState = seri.waveState;
            level.LevelProgressVisible = seri.levelProgressVisible;
            level.spawnedLanes = seri.spawnedLanes;
            level.spawnedID = seri.spawnedID;

            // 在网格加载后面
            // 加载所有实体。
            foreach (var ent in seri.entities)
            {
                var entity = Entity.CreateDeserializingEntity(ent, level);
                level.entities.Add(ent.id, entity);
            }
            foreach (var ent in seri.entityTrash)
            {
                var entity = Entity.CreateDeserializingEntity(ent, level);
                level.entityTrash.Add(ent.id, entity);
            }
            for (int i = 0; i < level.entities.Count; i++)
            {
                var seriEnt = seri.entities[i];
                var id = seriEnt.id;
                level.entities[id].ApplyDeserialize(seriEnt);
            }
            for (int i = 0; i < level.entityTrash.Count; i++)
            {
                var seriEnt = seri.entityTrash[i];
                var id = seriEnt.id;
                level.entityTrash[id].ApplyDeserialize(seriEnt);
            }
            // 在实体加载后面
            level.collisionSystem.LoadFromSerializable(level, seri.collisionSystem);
            // 加载所有网格的属性。
            for (int i = 0; i < level.grids.Length; i++)
            {
                var grid = level.grids[i];
                var seriGrid = seri.grids[i];
                grid.LoadFromSerializable(seriGrid, level);
            }

            level.Energy = seri.energy;
            level.delayedEnergyEntities = seri.delayedEnergyEntities.ToDictionary(d => level.FindEntityByID(d.entityId), d => d.energy);
            level.buffs = BuffList.FromSerializable(seri.buffs, level, level);
            level.buffs.OnPropertyChanged += level.UpdateBuffedProperty;
            level.UpdateAllBuffedProperties();

            return level;
        }
        public void DeserializeComponents(SerializableLevel seri)
        {
            foreach (var seriComp in seri.components)
            {
                var comp = levelComponents.FirstOrDefault(c => c.GetID().ToString() == seriComp.Key);
                if (comp == null)
                    continue;
                comp.LoadSerializable(seriComp.Value);
            }
        }
        #endregion

        #region 随机数生成器
        public RandomGenerator CreateRNG()
        {
            return new RandomGenerator(levelRandom.Next());
        }

        public RandomGenerator GetSpawnRNG()
        {
            return spawnRandom;
        }
        public RandomGenerator GetRoundRNG()
        {
            return roundRandom;
        }
        public RandomGenerator GetConveyorRNG()
        {
            return conveyorRandom;
        }
        #endregion

        #endregion

        #region 私有方法
        public void InitAreaProperties()
        {
            gridWidth = AreaDefinition.GetProperty<float>(EngineAreaProps.GRID_WIDTH);
            gridHeight = AreaDefinition.GetProperty<float>(EngineAreaProps.GRID_HEIGHT);
            gridLeftX = AreaDefinition.GetProperty<float>(EngineAreaProps.GRID_LEFT_X);
            gridBottomZ = AreaDefinition.GetProperty<float>(EngineAreaProps.GRID_BOTTOM_Z);
            maxLaneCount = AreaDefinition.GetProperty<int>(EngineAreaProps.MAX_LANE_COUNT);
            entityLaneZOffset = AreaDefinition.GetProperty<float>(EngineAreaProps.ENTITY_LANE_Z_OFFSET);
            maxColumnCount = AreaDefinition.GetProperty<int>(EngineAreaProps.MAX_COLUMN_COUNT);
        }
        IModelInterface IBuffTarget.GetInsertedModel(NamespaceID key) => null;
        Entity IBuffTarget.GetEntity() => null;
        Armor IBuffTarget.GetArmor() => null;
        void IBuffTarget.GetBuffs(List<Buff> results) => buffs.GetAllBuffs(results);
        Buff IBuffTarget.GetBuff(long id) => buffs.GetBuff(id);
        Buff IBuffTarget.CreateBuff(NamespaceID id) => CreateBuff(id, AllocBuffID());
        bool IBuffTarget.Exists() => true;
        #endregion

        public event Action OnClear;

        #region 属性字段
        public IGameContent Content { get; private set; }
        public IGameLocalization Localization { get; private set; }
        public int Seed { get; private set; }
        public bool IsRerun { get; set; }
        public bool IsCleared { get; private set; }
        public NamespaceID StageID { get; private set; }
        public StageDefinition StageDefinition { get; private set; }
        public NamespaceID AreaID { get; private set; }
        public AreaDefinition AreaDefinition { get; private set; }
        /// <summary>
        /// 进屋的僵尸。
        /// </summary>
        public Entity KillerEnemy { get; private set; }
        public NamespaceID Difficulty { get; set; }
        public int TPS => Option.TPS;
        public LevelOption Option { get; private set; }
        private RandomGenerator levelRandom;

        private RandomGenerator entityRandom;
        private RandomGenerator effectRandom;

        private RandomGenerator roundRandom;
        private RandomGenerator spawnRandom;
        private RandomGenerator conveyorRandom;

        private RandomGenerator miscRandom;

        private long currentBuffID = 1;
        private string deathMessage;

        private PropertyBlock properties;
        private LawnGrid[] grids;
        private BuffList buffs = new BuffList();

        private float gridWidth;
        private float gridHeight;
        private float gridLeftX;
        private float gridBottomZ;
        private float entityLaneZOffset;
        private int maxLaneCount;
        private int maxColumnCount;

        private List<ILevelComponent> levelComponents = new List<ILevelComponent>();
        #endregion 保存属性
    }
}