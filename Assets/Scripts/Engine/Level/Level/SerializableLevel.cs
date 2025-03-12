using System;
using System.Collections.Generic;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level.Collisions;
using PVZEngine.SeedPacks;
using Tools;

namespace PVZEngine.Level
{
    public class SerializableLevel
    {
        private Entity killerEnemy;
        private string deathMessage;

        private bool isRerun;

        private float gridSize;
        private float gridLeftX;
        private float gridBottomZ;
        private int maxLaneCount;
        private int maxColumnCount;


        public int seed;
        public bool isCleared;
        public NamespaceID stageDefinitionID;
        public NamespaceID areaDefinitionID;
        public NamespaceID difficulty;
        public SerializableLevelOption Option;
        public SerializableRNG levelRandom;
        public SerializableRNG entityRandom;
        public SerializableRNG effectRandom;
        public SerializableRNG roundRandom;
        public SerializableRNG spawnRandom;
        public SerializableRNG conveyorRandom;
        public SerializableRNG debugRandom;
        public SerializableRNG miscRandom;

        public SerializablePropertyBlock properties;

        public SerializableGrid[] grids;
        public float rechargeSpeed;
        public float rechargeTimeMultiplier;
        public SerializableClassicSeedPack[] seedPacks;
        public SerializableClassicSeedPack[] seedPackPool;
        public SerializableConveyorSeedPack[] conveyorSeedPacks;
        public bool requireCards;
        public long currentEntityID = 1;
        public long currentBuffID;
        public long currentSeedPackID;
        public int conveyorSlotCount;
        public SerializableConveyorSeedSpendRecords conveyorSeedSpendRecord;
        public List<SerializableEntity> entities;
        public List<SerializableEntity> entityTrash;
        public float energy;
        public SerializableDelayedEnergy[] delayedEnergyEntities;
        public int currentWave;
        public int currentFlag;
        public int waveState;
        public bool levelProgressVisible;
        public List<int> spawnedLanes;
        public List<NamespaceID> spawnedID;
        public ISerializableCollisionSystem collisionSystem;

        public SerializableBuffList buffs;

        public Dictionary<string, ISerializableLevelComponent> components;
    }
    [Serializable]
    public class SerializableDelayedEnergy
    {
        public long entityId;
        public float energy;
    }
}
