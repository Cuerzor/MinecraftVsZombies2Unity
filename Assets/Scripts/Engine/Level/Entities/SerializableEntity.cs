using System;
using System.Collections.Generic;
using PVZEngine.Armors;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace PVZEngine.Entities
{
    [Serializable]
    public class SerializableEntity
    {
        public long id;
        public int type;
        public int state;
        public long target;
        public long parent;
        public int initSeed;
        public SerializableRNG rng;
        public SerializableRNG dropRng;
        public NamespaceID definitionID;
        public NamespaceID modelID;
        public EntityReferenceChain spawnerReference;
        public SerializableArmor EquipedArmor;
        public Vector3 previousPosition;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 scale;
        public int collisionMaskHostile;
        public int collisionMaskFriendly;
        public Vector3 renderRotation;
        public Vector3 renderScale;
        public Dictionary<string, int> takenConveyorSeeds;
        public int timeout;

        #region 影子
        public bool shadowVisible;
        public float shadowAlpha;
        public Vector3 shadowScale;
        public Vector3 shadowOffset;
        #endregion

        public bool isDead;
        public float health;
        public bool isOnGround;
        public long currentBuffID;
        public SerializablePropertyBlock properties;
        public SerializableBuffList buffs;
        public List<long> children;
        public List<TakenGridInfo> takenGrids;

        public SerializableAuraEffect[] auras;

        [Serializable]
        public class TakenGridInfo
        {
            public int grid;
            public NamespaceID[] layers;
        }
    }
}
