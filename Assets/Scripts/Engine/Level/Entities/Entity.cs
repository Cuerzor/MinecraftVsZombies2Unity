using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PVZEngine.Armors;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.Models;
using PVZEngine.Modifiers;
using PVZEngine.Triggers;
using Tools;
using UnityEngine;

namespace PVZEngine.Entities
{
    public sealed class Entity : IBuffTarget, IAuraSource, IModifierContainer, IPropertyModifyTarget
    {
        #region 公有方法

        public Entity(LevelEngine level, long id, EntityReferenceChain spawnerReference, EntityDefinition definition, int seed) : this(level, definition.Type, id, spawnerReference)
        {
            Definition = definition;
            ModelID = definition.GetModelID();
            InitSeed = seed;
            RNG = new RandomGenerator(seed);
            DropRNG = new RandomGenerator(RNG.Next());
            CreateAuraEffects();
            UpdateModifierCaches();
        }
        private Entity(LevelEngine level, int type, long id, EntityReferenceChain spawnerReference)
        {
            Level = level;
            Type = type;
            TypeCollisionFlag = EntityCollisionHelper.GetTypeMask(type);
            ID = id;
            SpawnerReference = spawnerReference;
            MainHitbox = new EntityHitbox(this);
            buffs.OnPropertyChanged += UpdateModifiedProperty;
            buffs.OnModelInsertionAdded += OnBuffModelAddCallback;
            buffs.OnModelInsertionRemoved += OnBuffModelRemoveCallback;
            Cache = new EntityCache();
            properties = new PropertyBlock(this);
        }
        public void Init(Entity spawner)
        {
            OnInit(spawner);
            Definition.Init(this);
            auras.PostAdd();
            Level.Triggers.RunCallbackFiltered(LevelCallbacks.POST_ENTITY_INIT, Type, c => c(this));
            PostInit?.Invoke();
        }
        public void Update()
        {
            try
            {
                OnUpdate();
                // 更新碰撞体位置，用于碰撞检测。
                UpdateColliders();
                Definition.Update(this);
                if (EquipedArmor != null)
                    EquipedArmor.Update();
                auras.Update();
                buffs.Update();
                Level.Triggers.RunCallbackFiltered(LevelCallbacks.POST_ENTITY_UPDATE, Type, c => c(this));
                // 更新碰撞体位置，用于碰撞检测。
                UpdateColliders();
            }
            catch (Exception ex)
            {
                Debug.LogError($"更新实体时出现错误：{ex}");
            }
        }

        private void UpdateColliders()
        {
            MainHitbox.Update();
            foreach (var collider in enabledColliders)
            {
                collider.Update();
            }
        }
        public void SetParent(Entity parent)
        {
            var oldParent = Parent;
            if (oldParent != null)
            {
                oldParent.children.RemoveAll(r => r.ID == ID);
            }
            Parent = parent;
            if (parent != null)
            {
                parent.children.Add(this);
            }
        }
        public Entity[] GetChildren()
        {
            return children.ToArray();
        }
        public bool Exists()
        {
            return !Removed;
        }
        public void Remove()
        {
            if (!Removed)
            {
                Removed = true;
                Level.RemoveEntity(this);
                foreach (var pair in takenConveyorSeeds)
                {
                    Level.PutSeedToConveyorPool(pair.Key, pair.Value);
                }
                takenConveyorSeeds.Clear();
                Definition.PostRemove(this);
                auras.PostRemove();
                Level.Triggers.RunCallbackFiltered(LevelCallbacks.POST_ENTITY_REMOVE, Type, c => c(this));
            }
        }
        public bool IsEntityOf(NamespaceID id)
        {
            return Definition.GetID() == id;
        }

        #region 伤害

        public void Die(Entity source = null, BodyDamageResult damage = null)
        {
            Die(new DamageEffectList(), new EntityReferenceChain(source), damage);
        }
        public void Die(DamageEffectList effects, Entity source = null, BodyDamageResult damage = null)
        {
            Die(effects, new EntityReferenceChain(source), damage);
        }
        public void Die(DamageEffectList effects, EntityReferenceChain source, BodyDamageResult damage = null)
        {
            Die(new DeathInfo(this, effects, source, damage));
        }
        public void Die(DeathInfo info)
        {
            info = info ?? new DeathInfo(this, new DamageEffectList(), new EntityReferenceChain(null), null);
            IsDead = true;
            Definition.PostDeath(this, info);
            Level.Triggers.RunCallbackFiltered(LevelCallbacks.POST_ENTITY_DEATH, Type, c => c(this, info));
        }
        #endregion

        #region 阵营
        public int GetFaction()
        {
            return Cache.Faction;
        }
        public bool IsFriendly(Entity entity)
        {
            if (entity == null)
                return false;
            return IsFriendly(entity.GetFaction());
        }
        public bool IsFriendly(int faction)
        {
            return EngineEntityExt.IsFriendly(GetFaction(), faction);
        }
        public bool IsHostile(Entity entity)
        {
            if (entity == null)
                return false;
            return IsHostile(entity.GetFaction());
        }
        public bool IsHostile(int faction)
        {
            return EngineEntityExt.IsHostile(GetFaction(), faction);
        }
        #endregion 魅惑

        #region 增益属性
        public T GetProperty<T>(PropertyKey name, bool ignoreBuffs = false)
        {
            return properties.GetProperty<T>(name, ignoreBuffs);
        }
        public void SetProperty(PropertyKey name, object value)
        {
            properties.SetProperty(name, value);
        }
        private void GetModifierItems(PropertyKey name, List<ModifierContainerItem> results)
        {
            if (!modifierCaches.TryGetValue(name, out var list))
                return;
            results.AddRange(list);
        }
        private void UpdateAllBuffedProperties()
        {
            properties.UpdateAllModifiedProperties();
        }
        private void UpdateModifiedProperty(PropertyKey name)
        {
            properties.UpdateModifiedProperty(name);
        }
        bool IPropertyModifyTarget.GetFallbackProperty(PropertyKey name, out object value)
        {
            if (Definition == null)
            {
                value = null;
                return false;
            }
            if (Definition.TryGetProperty(name, out var defProp))
            {
                value = defProp;
                return true;
            }

            var behaviourCount = Definition.GetBehaviourCount();
            for (int i = 0; i < behaviourCount; i++)
            {
                var behaviour = Definition.GetBehaviourAt(i);
                if (behaviour.TryGetProperty(name, out var behProp))
                {
                    value = behProp;
                    return true;
                }
            }
            value = null;
            return false;
        }
        IEnumerable<PropertyKey> IPropertyModifyTarget.GetModifiedProperties()
        {
            var entityPropertyNames = modifierCaches.Keys;
            var buffPropertyNames = buffs.GetModifierPropertyNames();
            return entityPropertyNames.Union(buffPropertyNames);
        }
        PropertyModifier[] IPropertyModifyTarget.GetModifiersUsingProperty(PropertyKey name)
        {
            return Definition.GetModifiers().Where(m => m.UsingContainerPropertyName == name).ToArray();
        }
        void IPropertyModifyTarget.GetModifierItems(PropertyKey name, List<ModifierContainerItem> results)
        {
            GetModifierItems(name, results);
            buffs.GetModifierItems(name, results);
        }
        void IPropertyModifyTarget.UpdateModifiedProperty(PropertyKey name, object value)
        {
            Cache.UpdateProperty(this, name, value);
            PostPropertyChanged?.Invoke(name);
        }
        #endregion

        #region 增益
        public Buff CreateBuff<T>() where T : BuffDefinition
        {
            return Level.CreateBuff<T>(AllocBuffID());
        }
        public Buff CreateBuff(BuffDefinition buffDefinition)
        {
            return Level.CreateBuff(buffDefinition, AllocBuffID());
        }
        public Buff CreateBuff(NamespaceID id)
        {
            return Level.CreateBuff(id, AllocBuffID());
        }
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
            var buff = CreateBuff<T>();
            AddBuff(buff);
            return buff;
        }
        public bool RemoveBuff(Buff buff) => buffs.RemoveBuff(buff);
        public int RemoveBuffs(IEnumerable<Buff> buffs) => this.buffs.RemoveBuffs(buffs);
        public int RemoveBuffs<T>() where T : BuffDefinition => buffs.RemoveBuffs<T>();
        public bool HasBuff<T>() where T : BuffDefinition => buffs.HasBuff<T>();
        public bool HasBuff(Buff buff) => buffs.HasBuff(buff);
        public Buff GetFirstBuff<T>() where T : BuffDefinition => buffs.GetFirstBuff<T>();
        public Buff[] GetBuffs<T>() where T : BuffDefinition => buffs.GetBuffs<T>();
        public void GetBuffs<T>(List<Buff> results) where T : BuffDefinition => buffs.GetBuffsNonAlloc<T>(results);
        public void GetAllBuffs(List<Buff> results) => buffs.GetAllBuffs(results);
        public BuffReference GetBuffReference(Buff buff) => new BuffReferenceEntity(ID, buff.ID);
        private long AllocBuffID()
        {
            return currentBuffID++;
        }
        #endregion

        #region 物理

        #region 体积
        public Vector3 GetCenter()
        {
            return MainHitbox.GetBoundsCenter();
        }
        public void SetCenter(Vector3 center)
        {
            var offset = GetCenter() - Position;
            Position = center - offset;
        }
        public Bounds GetBounds()
        {
            return MainHitbox.GetBounds();
        }
        public Vector3 GetScaledSize()
        {
            return MainHitbox.GetBoundsSize();
        }
        #endregion

        #region 相对高度
        public float GetGroundY()
        {
            return Level.GetGroundY(Position.x, Position.z);
        }
        public float GetRelativeY()
        {
            return Position.y - GetGroundY();
        }
        public void SetRelativeY(float value)
        {
            var pos = Position;
            pos.y = value + GetGroundY();
            Position = pos;
        }
        #endregion

        private Vector3 GetNextPosition(float simulationSpeed = 1)
        {
            Vector3 velocity = GetNextVelocity(simulationSpeed);
            velocity.Scale(Vector3.one - Cache.VelocityDampen);
            var nextPos = Position + velocity * simulationSpeed;
            return nextPos;
        }


        private Vector3 GetNextVelocity(float simulationSpeed = 1)
        {
            Vector3 velocity = Velocity;

            // Friction.
            var frictionMulti = Mathf.Pow(Mathf.Max(0, 1 - Cache.Friction), simulationSpeed);
            velocity = new Vector3(velocity.x * frictionMulti, velocity.y, velocity.z * frictionMulti);

            // Gravity.
            velocity.y -= Cache.Gravity * simulationSpeed;

            return velocity;
        }

        public void UpdatePhysics(float simulationSpeed = 1)
        {
            Vector3 nextVelocity = GetNextVelocity(simulationSpeed);
            Vector3 nextPos = GetNextPosition(simulationSpeed);

            // 地面限制。
            float groundY = Level.GetGroundY(nextPos.x, nextPos.z);
            var groundLimit = groundY + Cache.GroundLimitOffset;
            var contactingGround = nextPos.y <= groundY;
            var contactVelocity = nextVelocity;
            if (nextPos.y <= groundLimit)
            {
                nextPos.y = groundLimit;
                nextVelocity.y = Mathf.Max(nextVelocity.y, 0);
            }

            PreviousPosition = prevUpdatePosition;
            Position = nextPos;
            Velocity = nextVelocity;
            prevUpdatePosition = Position;

            if (contactingGround)
            {
                if (!IsOnGround)
                {
                    OnContactGround(contactVelocity);
                    IsOnGround = true;
                }
            }
            else
            {
                if (IsOnGround)
                {
                    OnLeaveGround();
                    IsOnGround = false;
                }
            }
        }
        #endregion

        #region 网格
        public int GetColumn()
        {
            return Level.GetColumn(Position.x);
        }
        public int GetLane()
        {
            return Level.GetLane(Position.z);
        }
        public void GetTakingGridLayers(LawnGrid grid, List<NamespaceID> results)
        {
            var hashSet = GetTakenGridLayerHashSet(grid);
            if (hashSet == null)
                return;
            results.AddRange(hashSet);
        }
        public bool IsTakingGridLayer(LawnGrid grid, NamespaceID layer)
        {
            var hashSet = GetTakenGridLayerHashSet(grid);
            if (hashSet == null)
                return false;
            return hashSet.Contains(layer);
        }
        public void TakeGrid(LawnGrid grid, NamespaceID layer)
        {
            var hashSet = GetOrCreateTakenGridHashSet(grid);
            hashSet.Add(layer);
            grid.AddLayerEntity(layer, this);
        }
        public bool ReleaseGrid(LawnGrid grid, NamespaceID layer)
        {
            var hashSet = GetTakenGridLayerHashSet(grid);
            if (hashSet == null)
                return false;
            if (hashSet.Remove(layer))
            {
                grid.RemoveLayerEntity(layer, this);
                return true;
            }
            return false;
        }
        public void ClearTakenGrids()
        {
            foreach (var pair in takenGrids)
            {
                foreach (var layer in pair.Value)
                {
                    pair.Key.RemoveLayerEntity(layer, this);
                }
            }
            takenGrids.Clear();
        }
        public void GetTakenGrids(List<LawnGrid> results)
        {
            foreach (var pair in takenGrids)
            {
                results.Add(pair.Key);
            }
        }
        public int GetGridIndex()
        {
            return Level.GetGridIndex(GetColumn(), GetLane());
        }
        public LawnGrid GetGrid()
        {
            return Level.GetGrid(GetColumn(), GetLane());
        }
        private HashSet<NamespaceID> GetOrCreateTakenGridHashSet(LawnGrid grid)
        {
            var hashSet = GetTakenGridLayerHashSet(grid);
            if (hashSet == null)
            {
                hashSet = new HashSet<NamespaceID>();
                takenGrids.Add(grid, hashSet);
            }
            return hashSet;
        }
        private HashSet<NamespaceID> GetTakenGridLayerHashSet(LawnGrid grid)
        {
            if (takenGrids.TryGetValue(grid, out var set))
                return set;
            return null;
        }
        #endregion

        #region 碰撞
        public void AddCollider(EntityCollider collider)
        {
            if (GetCollider(collider.Name) != null)
                throw new ArgumentException($"Attempting to add a collider with name \"{collider.Name}\" to an entity while it already has a collider with the same name.");
            colliders.Add(collider);
            if (collider.Enabled)
            {
                enabledColliders.Add(collider);
            }
            collider.PreCollision += PreCollisionCallback;
            collider.PostCollision += PostCollisionCallback;
            collider.OnEnabled += OnColliderEnabledCallback;
            collider.OnDisabled += OnColliderDisabledCallback;
        }
        public bool RemoveCollider(EntityCollider collider)
        {
            if (colliders.Remove(collider))
            {
                if (collider.Enabled)
                {
                    enabledColliders.Remove(collider);
                }
                collider.PreCollision -= PreCollisionCallback;
                collider.PostCollision -= PostCollisionCallback;
                collider.OnEnabled -= OnColliderEnabledCallback;
                collider.OnDisabled -= OnColliderDisabledCallback;
                return true;
            }
            return false;
        }
        public EntityCollider GetCollider(string name)
        {
            return colliders.FirstOrDefault(h => h.Name == name);
        }
        public EntityCollider[] GetAllColliders()
        {
            return colliders.ToArray();
        }
        public int GetEnabledColliderCount()
        {
            return enabledColliders.Count;
        }
        public EntityCollider GetEnabledColliderAt(int i)
        {
            return enabledColliders[i];
        }
        public IEnumerable<EntityCollision> GetCurrentCollisions()
        {
            foreach (var unit in colliders)
            {
                foreach (var reference in unit.GetCollisions())
                {
                    yield return reference;
                }
            }
        }
        public void ExitCollision()
        {
            foreach (var unit in colliders)
            {
                unit.ExitCollision();
            }
        }
        #endregion

        #region 护甲
        public void EquipArmor<T>() where T : ArmorDefinition
        {
            EquipArmor(new Armor(this, Level.Content.GetArmorDefinition<T>()));
        }
        public void EquipArmor(ArmorDefinition definition)
        {
            if (definition == null)
                return;
            EquipArmor(new Armor(this, definition));
        }
        public void EquipArmor(Armor armor)
        {
            if (armor == null)
                return;
            if (EquipedArmor != null)
                EquipedArmor.Destroy(null);
            EquipedArmor = armor;

            Definition.PostEquipArmor(this, armor);
            Level.Triggers.RunCallback(LevelCallbacks.POST_EQUIP_ARMOR, c => c(this, armor));
            OnEquipArmor?.Invoke(armor);
        }
        public void DestroyArmor(Armor armor, ArmorDamageResult result)
        {
            Definition.PostDestroyArmor(this, armor, result);
            Level.Triggers.RunCallback(LevelCallbacks.POST_DESTROY_ARMOR, c => c(this, armor, result));
            OnDestroyArmor?.Invoke(armor, result);
        }
        public void RemoveArmor()
        {
            var armor = EquipedArmor;
            if (armor == null)
                return;
            EquipedArmor = null;
            Definition.PostRemoveArmor(this, armor);
            Level.Triggers.RunCallback(LevelCallbacks.POST_REMOVE_ARMOR, c => c(this, armor));
            OnRemoveArmor?.Invoke(armor);
        }
        public Armor GetShield()
        {
            return null;
        }
        #endregion
        public bool IsFacingLeft() => this.FaceLeftAtDefault() != FlipX;

        #region 模型
        public void SetModelInterface(IModelInterface model, IModelInterface armorModel)
        {
            modelInterface = model;
            armorModelInterface = armorModel;
        }
        public void ChangeModel(NamespaceID id)
        {
            ModelID = id;
            OnChangeModel?.Invoke(id);
        }
        public void SetModelProperty(string name, object value)
        {
            modelInterface.SetModelProperty(name, value);
        }
        public void TriggerModel(string name)
        {
            modelInterface.TriggerModel(name);
        }
        public void SetShaderInt(string name, int value)
        {
            modelInterface.SetShaderInt(name, value);
        }
        public void SetShaderFloat(string name, float value)
        {
            modelInterface.SetShaderFloat(name, value);
        }
        public void SetShaderColor(string name, Color value)
        {
            modelInterface.SetShaderColor(name, value);
        }
        public IModelInterface CreateChildModel(string anchorName, NamespaceID key, NamespaceID modelID)
        {
            return modelInterface.CreateChildModel(anchorName, key, modelID);
        }
        public bool RemoveChildModel(NamespaceID key)
        {
            return modelInterface.RemoveChildModel(key);
        }
        public IModelInterface GetChildModel(NamespaceID key)
        {
            return modelInterface.GetChildModel(key);
        }
        public void TriggerAnimation(string name, EntityAnimationTarget target = EntityAnimationTarget.Entity)
        {
            GetModelTarget(target).TriggerAnimation(name);
        }
        public void SetAnimationBool(string name, bool value, EntityAnimationTarget target = EntityAnimationTarget.Entity)
        {
            GetModelTarget(target).SetAnimationBool(name, value);
        }
        public void SetAnimationInt(string name, int value, EntityAnimationTarget target = EntityAnimationTarget.Entity)
        {
            GetModelTarget(target).SetAnimationInt(name, value);
        }
        public void SetAnimationFloat(string name, float value, EntityAnimationTarget target = EntityAnimationTarget.Entity)
        {
            GetModelTarget(target).SetAnimationFloat(name, value);
        }
        private IModelInterface GetModelTarget(EntityAnimationTarget target)
        {
            return target == EntityAnimationTarget.Armor ? armorModelInterface : modelInterface;
        }
        #endregion

        #region 传送带
        public void AddTakenConveyorSeed(NamespaceID id)
        {
            if (takenConveyorSeeds.ContainsKey(id))
            {
                takenConveyorSeeds[id]++;
            }
            else
            {
                takenConveyorSeeds[id] = 1;
            }
        }
        public bool RemoveTakenConveyorSeed(NamespaceID id)
        {
            if (!takenConveyorSeeds.ContainsKey(id))
            {
                return false;
            }
            takenConveyorSeeds[id]--;
            if (takenConveyorSeeds[id] <= 0)
            {
                takenConveyorSeeds.Remove(id);
            }
            return true;
        }
        #endregion

        #region 序列化
        public SerializableEntity Serialize()
        {
            var seri = new SerializableEntity();
            seri.id = ID;
            seri.initSeed = InitSeed;
            seri.spawnerReference = SpawnerReference;
            seri.type = Type;
            seri.state = State;
            seri.rng = RNG.ToSerializable();
            seri.dropRng = DropRNG.ToSerializable();
            seri.target = Target?.ID ?? 0;

            seri.definitionID = Definition.GetID();
            seri.modelID = ModelID;
            seri.parent = Parent?.ID ?? 0;
            seri.EquipedArmor = EquipedArmor?.Serialize();
            seri.previousPosition = PreviousPosition;
            seri.position = Position;
            seri.velocity = Velocity;
            seri.collisionMaskHostile = CollisionMaskHostile;
            seri.collisionMaskFriendly = CollisionMaskFriendly;
            seri.renderRotation = RenderRotation;
            seri.takenConveyorSeeds = takenConveyorSeeds.ToDictionary(p => p.ToString(), p => p.Value);
            seri.timeout = Timeout;
            seri.colliders = colliders.ConvertAll(g => g.ToSerializable()).ToArray();

            seri.isDead = IsDead;
            seri.health = Health;
            seri.isOnGround = IsOnGround;
            seri.currentBuffID = currentBuffID;
            seri.properties = properties.ToSerializable();
            seri.buffs = buffs.ToSerializable();
            seri.children = children.ConvertAll(e => e?.ID ?? 0);
            seri.takenGrids = new List<SerializableEntity.TakenGridInfo>();
            foreach (var pair in takenGrids)
            {
                var info = new SerializableEntity.TakenGridInfo()
                {
                    grid = pair.Key.GetIndex(),
                    layers = pair.Value.ToArray()
                };
                seri.takenGrids.Add(info);
            }

            seri.auras = auras.GetAll().Select(a => a.ToSerializable()).ToArray();
            return seri;
        }
        public static Entity Deserialize(SerializableEntity seri, LevelEngine level)
        {
            Entity entity = CreateDeserializingEntity(seri, level);
            entity.ApplyDeserialize(seri);
            return entity;
        }
        public void ApplyDeserialize(SerializableEntity seri)
        {
            InitSeed = seri.initSeed;
            RNG = RandomGenerator.FromSerializable(seri.rng);
            DropRNG = RandomGenerator.FromSerializable(seri.dropRng);
            State = seri.state;
            Target = Level.FindEntityByID(seri.target);

            Definition = Level.Content.GetEntityDefinition(seri.definitionID);
            ModelID = seri.modelID;
            Parent = Level.FindEntityByID(seri.parent);
            EquipedArmor = seri.EquipedArmor != null ? Armor.Deserialize(seri.EquipedArmor, this) : null;
            PreviousPosition = seri.previousPosition;
            prevUpdatePosition = seri.position;
            Position = seri.position;
            Velocity = seri.velocity;
            CollisionMaskHostile = seri.collisionMaskHostile;
            CollisionMaskFriendly = seri.collisionMaskFriendly;
            RenderRotation = seri.renderRotation;
            takenConveyorSeeds = seri.takenConveyorSeeds.ToDictionary(p => NamespaceID.ParseStrict(p.Key), p => p.Value);
            Timeout = seri.timeout;

            IsDead = seri.isDead;
            Health = seri.health;
            IsOnGround = seri.isOnGround;
            currentBuffID = seri.currentBuffID;
            properties = PropertyBlock.FromSerializable(seri.properties, this);

            buffs = BuffList.FromSerializable(seri.buffs, Level, this);
            buffs.OnPropertyChanged += UpdateModifiedProperty;
            buffs.OnModelInsertionAdded += OnBuffModelAddCallback;
            buffs.OnModelInsertionRemoved += OnBuffModelRemoveCallback;

            children = seri.children.ConvertAll(e => Level.FindEntityByID(e));
            if (seri.takenGrids != null)
            {
                foreach (var takenGrid in seri.takenGrids)
                {
                    if (takenGrid == null || takenGrid.layers == null)
                        continue;
                    var grid = Level.GetGrid(takenGrid.grid);
                    if (grid == null)
                        continue;
                    takenGrids.Add(grid, takenGrid.layers.ToHashSet());
                }
            }
            for (int i = 0; i < colliders.Count; i++)
            {
                var collider = colliders[i];
                var seriCollider = seri.colliders[i];
                collider.LoadCollisions(Level, seriCollider);
            }
            CreateAuraEffects();
            auras.LoadFromSerializable(Level, seri.auras);

            UpdateModifierCaches();
            UpdateAllBuffedProperties();
            Cache.UpdateAll(this);
            UpdateColliders();
        }
        public static Entity CreateDeserializingEntity(SerializableEntity seri, LevelEngine level)
        {
            var entity = new Entity(level, seri.type, seri.id, seri.spawnerReference);

            foreach (var collider in seri.colliders.Select(s => EntityCollider.FromSerializable(s, entity)))
            {
                entity.AddCollider(collider);
            }
            return entity;
        }
        #endregion

        public override string ToString()
        {
            return $"{ID}({this.Definition.GetID()})";
        }
        #endregion

        #region 私有方法
        private void OnInit(Entity spawner)
        {
            PreviousPosition = Position;
            prevUpdatePosition = Position;
            Health = this.GetMaxHealth();
            Cache.UpdateAll(this);
            var collider = new EntityCollider(this, EntityCollisionHelper.NAME_MAIN, new EntityHitbox(this));
            AddCollider(collider);
            UpdateAllBuffedProperties();
            Cache.UpdateAll(this);
            UpdateColliders();
        }
        private void OnUpdate()
        {
            UpdatePhysics(1);
            Health = Mathf.Min(Health, this.GetMaxHealth());
        }
        private void OnContactGround(Vector3 velocity)
        {
            Definition.PostContactGround(this, velocity);
            Level.Triggers.RunCallback(LevelCallbacks.POST_ENTITY_CONTACT_GROUND, c => c(this, velocity));
        }
        private void OnLeaveGround()
        {
            Definition.PostLeaveGround(this);
            Level.Triggers.RunCallback(LevelCallbacks.POST_ENTITY_LEAVE_GROUND, c => c(this));
        }
        private bool PreCollisionCallback(EntityCollision collision)
        {
            bool canCollide = Definition.PreCollision(collision);
            var result = new TriggerResultBoolean();
            result.Result = canCollide;
            Level.Triggers.RunCallback(LevelCallbacks.PRE_ENTITY_COLLISION, result, c => c(collision, result));
            return result.Result;
        }
        private void PostCollisionCallback(EntityCollision collision, int state)
        {
            Definition.PostCollision(collision, state);
            Level.Triggers.RunCallback(LevelCallbacks.POST_ENTITY_COLLISION, c => c(collision, state));
        }
        private void OnColliderEnabledCallback(EntityCollider collider)
        {
            OnColliderEnabled?.Invoke(collider);
            enabledColliders.Add(collider);
        }
        private void OnColliderDisabledCallback(EntityCollider collider)
        {
            OnColliderDisabled?.Invoke(collider);
            enabledColliders.Remove(collider);
        }
        private void OnBuffModelAddCallback(string anchorName, NamespaceID key, NamespaceID modelID)
        {
            CreateChildModel(anchorName, key, modelID);
        }
        private void OnBuffModelRemoveCallback(NamespaceID key)
        {
            RemoveChildModel(key);
        }

        private void CreateAuraEffects()
        {
            var auraDefs = Definition.GetAuras();
            for (int i = 0; i < auraDefs.Length; i++)
            {
                var auraDef = auraDefs[i];
                auras.Add(Level, new AuraEffect(auraDef, i, this));
            }
        }
        private void UpdateModifierCaches()
        {
            foreach (var modifier in Definition.GetModifiers())
            {
                var propName = modifier.PropertyName;
                if (!modifierCaches.TryGetValue(propName, out var list))
                {
                    list = new List<ModifierContainerItem>();
                    modifierCaches.Add(propName, list);
                }
                list.Add(new ModifierContainerItem(this, modifier));
            }
        }
        IModelInterface IBuffTarget.GetInsertedModel(NamespaceID key) => GetChildModel(key);
        Entity IBuffTarget.GetEntity() => this;
        void IBuffTarget.GetBuffs(List<Buff> results) => buffs.GetAllBuffs(results);
        Buff IBuffTarget.GetBuff(long id) => buffs.GetBuff(id);
        Entity IAuraSource.GetEntity() => this;
        LevelEngine IAuraSource.GetLevel() => Level;
        object IModifierContainer.GetProperty(PropertyKey name) => GetProperty<object>(name);
        #endregion

        #region 事件
        public event Action PostInit;
        public event Action<PropertyKey> PostPropertyChanged;
        public event Action<NamespaceID> OnChangeModel;
        public event Action<Armor> OnEquipArmor;
        public event Action<Armor, ArmorDamageResult> OnDestroyArmor;
        public event Action<Armor> OnRemoveArmor;

        public event Action<EntityCollider> OnColliderEnabled;
        public event Action<EntityCollider> OnColliderDisabled;
        #endregion

        #region 属性字段
        public long ID { get; }
        public int InitSeed { get; private set; }
        public RandomGenerator RNG { get; private set; }
        public RandomGenerator DropRNG { get; private set; }
        public bool Removed { get; private set; }
        public EntityDefinition Definition { get; private set; }
        public NamespaceID ModelID { get; private set; }
        public EntityReferenceChain SpawnerReference { get; private set; }
        public Entity Parent { get; private set; }
        public LevelEngine Level { get; private set; }
        public Armor EquipedArmor { get; private set; }
        public Vector3 PreviousPosition { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 RenderRotation { get; set; } = Vector3.zero;
        public bool FlipX => this.GetScale().x < 0;
        private Vector3 prevUpdatePosition;
        #region Collision
        public int CollisionMaskHostile { get; set; }
        public int CollisionMaskFriendly { get; set; }
        public Hitbox MainHitbox { get; private set; }
        private List<EntityCollider> colliders = new List<EntityCollider>();
        private List<EntityCollider> enabledColliders = new List<EntityCollider>();
        private IModelInterface modelInterface;
        private IModelInterface armorModelInterface;
        #endregion
        public int Timeout { get; set; } = -1;
        public bool IsDead { get; set; }
        public float Health { get; set; }
        public int Type { get; }
        public int State { get; set; }
        public Entity Target { get; set; }
        public bool IsOnGround { get; private set; } = true;
        internal int TypeCollisionFlag { get; }
        internal EntityCache Cache { get; }

        private PropertyBlock properties;
        private long currentBuffID = 1;
        private BuffList buffs = new BuffList();
        private AuraEffectList auras = new AuraEffectList();
        private Dictionary<LawnGrid, HashSet<NamespaceID>> takenGrids = new Dictionary<LawnGrid, HashSet<NamespaceID>>();
        private List<Entity> children = new List<Entity>();
        private Dictionary<NamespaceID, int> takenConveyorSeeds = new Dictionary<NamespaceID, int>();
        private Dictionary<PropertyKey, List<ModifierContainerItem>> modifierCaches = new Dictionary<PropertyKey, List<ModifierContainerItem>>();
        #endregion
    }
}

namespace PVZEngine.Level
{
    public enum EntityAnimationTarget
    {
        Entity,
        Armor
    }
}