using System;
using System.Linq;
using System.Reflection;
using MVZ2.GameContent.Implements;
using MVZ2.GameContent.Seeds;
using MVZ2.GameContent.Spawns;
using MVZ2.GameContent.Stages;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Saves;
using MVZ2.Vanilla.SeedPacks;
using MVZ2Logic;
using MVZ2Logic.Artifacts;
using MVZ2Logic.Entities;
using MVZ2Logic.Games;
using MVZ2Logic.Level;
using MVZ2Logic.Modding;
using MVZ2Logic.Saves;
using MVZ2Logic.SeedPacks;
using MVZ2Logic.Spawns;
using PVZEngine;
using PVZEngine.Base;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.Vanilla
{
    public class VanillaMod : Mod
    {
        public VanillaMod() : base(spaceName)
        {
            var assemblies = new Assembly[] { Assembly.GetAssembly(typeof(VanillaMod)) };
            RegisterEntityProperties(assemblies);
            LoadEntityMetas();
            LoadSpawnMetas();
            LoadStageMetas();
            LoadDefinitionsFromAssemblies(assemblies);
            LoadAreaProperties();
            LoadStageProperties();
            LoadArtifactProperties();
            LoadSeedOptionProperties();
            AddEntityBehaviours();
            AddOptionSeeds();

            ImplementCallbacks(new GemStageImplements());
            ImplementCallbacks(new StatsImplements());
            ImplementCallbacks(new EntityImplements());
            ImplementCallbacks(new DifficultyImplements());
            ImplementCallbacks(new CartToMoneyImplements());
            ImplementCallbacks(new TalkActionImplements());
            ImplementCallbacks(new BlueprintRecommendImplements());
            ImplementCallbacks(new WaterImplements());
            ImplementCallbacks(new AchievementsImplements());
            ImplementCallbacks(new RandomChinaImplements());
        }
        public override void PostGameInit()
        {
            base.PostGameInit();
            SerializeHelper.RegisterClass<SerializableVanillaSaveData>();
        }
        public override ModSaveData CreateSaveData()
        {
            return new VanillaSaveData(spaceName);
        }
        public override ModSaveData LoadSaveData(string json)
        {
            var serializable = SerializeHelper.FromBson<SerializableVanillaSaveData>(json);
            return serializable.Deserialize();
        }
        private void ImplementCallbacks(VanillaImplements implements)
        {
            implements.Implement(this);
        }
        private void LoadEntityMetas()
        {
            foreach (IEntityMeta meta in Global.Game.GetModEntityMetas(spaceName))
            {
                if (meta == null)
                    continue;
                var name = meta.ID;
                var entityDefinition = new MetaEntityDefinition(meta.Type, spaceName, name);
                foreach (var pair in meta.Properties)
                {
                    entityDefinition.SetProperty(PropertyMapper.ConvertFromName(pair.Key), pair.Value);
                }
                AddDefinition(entityDefinition);

                var seedDef = new EntitySeed(Namespace,
                                             name,
                                             entityDefinition.GetCost(),
                                             entityDefinition.GetRechargeID(),
                                             entityDefinition.IsTriggerActive(),
                                             entityDefinition.CanInstantTrigger(),
                                             entityDefinition.IsUpgradeBlueprint());
                AddDefinition(seedDef);
            }
        }
        private void LoadSpawnMetas()
        {
            foreach (ISpawnMeta meta in Global.Game.GetModSpawnMetas(spaceName))
            {
                if (meta == null)
                    continue;
                var name = meta.ID;
                var spawnLevel = meta.SpawnLevel;
                var terrain = meta.Terrain;
                var weight = meta.Weight;
                var excludedTags = terrain?.ExcludedAreaTags ?? Array.Empty<NamespaceID>();
                var water = meta.Terrain?.Water ?? false;

                var spawnDef = new VanillaSpawnDefinition(Namespace, name, spawnLevel, new NamespaceID(Namespace, name), excludedTags);
                spawnDef.SetProperty(VanillaSpawnProps.PREVIEW_COUNT, meta.PreviewCount);
                if (weight != null)
                {
                    spawnDef.SetProperty(VanillaSpawnProps.WEIGHT_BASE, weight.Base);
                    spawnDef.SetProperty(VanillaSpawnProps.WEIGHT_DECAY_START, weight.DecreaseStart);
                    spawnDef.SetProperty(VanillaSpawnProps.WEIGHT_DECAY_END, weight.DecreaseEnd);
                    spawnDef.SetProperty(VanillaSpawnProps.WEIGHT_DECAY, weight.DecreasePerFlag);
                }
                spawnDef.CanSpawnAtWaterLane = water;
                AddDefinition(spawnDef);
            }
        }
        private void LoadStageMetas()
        {
            foreach (var meta in Global.Game.GetModStageMetas(spaceName))
            {
                if (meta == null)
                    continue;
                switch (meta.Type)
                {
                    case StageTypes.TYPE_NORMAL:
                        {
                            var stage = new ClassicStage(spaceName, meta.ID);
                            AddDefinition(stage);
                        }
                        break;
                    case StageTypes.TYPE_ENDLESS:
                        {
                            var stage = new EndlessStage(spaceName, meta.ID);
                            AddDefinition(stage);
                        }
                        break;
                }
            }
            AddDefinition(new WhackAGhostStage(spaceName, VanillaStageNames.halloween6));
            AddDefinition(new BreakoutStage(spaceName, VanillaStageNames.dream6));
            AddDefinition(new LittleZombieStage(spaceName, VanillaStageNames.castle6));
            AddDefinition(new SeijaStage(spaceName, VanillaStageNames.castle7));
        }
        protected void RegisterEntityProperties(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                PropertyMapper.InitPropertyMaps(string.Empty, types);
            }
        }
        protected void LoadDefinitionsFromAssemblies(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var definitionAttr = type.GetCustomAttribute<DefinitionAttribute>();
                    if (definitionAttr == null || type.IsAbstract)
                        continue;

                    var name = definitionAttr.Name;
                    var constructor = type.GetConstructor(new Type[] { typeof(string), typeof(string) });
                    var definitionObj = constructor?.Invoke(new object[] { Namespace, name });
                    if (definitionObj is Definition def)
                    {
                        AddDefinition(def);
                    }
                }
            }
        }
        private void LoadAreaProperties()
        {
            foreach (IAreaMeta meta in Global.Game.GetModAreaMetas(spaceName))
            {
                if (meta == null)
                    continue;
                var area = this.GetAreaDefinition(new NamespaceID(spaceName, meta.ID));
                if (area == null)
                    continue;
                area.SetProperty(VanillaAreaProps.MODEL_ID, meta.ModelID);
                area.SetProperty(VanillaLevelProps.MUSIC_ID, meta.MusicID);
                area.SetProperty(EngineAreaProps.CART_REFERENCE, meta.Cart);
                area.SetProperty(EngineAreaProps.AREA_TAGS, meta.Tags);
                area.SetProperty(VanillaAreaProps.STARSHARD_ICON, meta.StarshardIcon);

                area.SetProperty(EngineAreaProps.ENEMY_SPAWN_X, meta.EnemySpawnX);
                area.SetProperty(VanillaAreaProps.DOOR_Z, meta.DoorZ);

                area.SetProperty(VanillaAreaProps.BACKGROUND_LIGHT, meta.BackgroundLight);
                area.SetProperty(VanillaAreaProps.GLOBAL_LIGHT, meta.GlobalLight);

                area.SetProperty(EngineAreaProps.GRID_WIDTH, meta.GridWidth);
                area.SetProperty(EngineAreaProps.GRID_HEIGHT, meta.GridHeight);
                area.SetProperty(EngineAreaProps.GRID_LEFT_X, meta.GridLeftX);
                area.SetProperty(EngineAreaProps.GRID_BOTTOM_Z, meta.GridBottomZ);
                area.SetProperty(EngineAreaProps.MAX_LANE_COUNT, meta.Lanes);
                area.SetProperty(EngineAreaProps.MAX_COLUMN_COUNT, meta.Columns);

                area.SetGridLayout(meta.Grids.Select(m => m.ID).ToArray());
            }
        }
        private void LoadStageProperties()
        {
            foreach (var meta in Global.Game.GetModStageMetas(spaceName))
            {
                if (meta == null)
                    continue;
                var stage = this.GetStageDefinition(new NamespaceID(spaceName, meta.ID));
                if (stage == null)
                    continue;
                stage.SetLevelName(meta.Name);
                stage.SetDayNumber(meta.DayNumber);

                stage.SetProperty(VanillaLevelProps.MUSIC_ID, meta.MusicID);

                stage.SetProperty(VanillaStageProps.NO_START_TALK_MUSIC, meta.NoStartTalkMusic);
                stage.SetProperty(VanillaStageProps.TALKS, meta.Talks);

                stage.SetProperty(VanillaStageProps.CLEAR_PICKUP_MODEL, meta.ClearPickupModel);
                stage.SetProperty(VanillaStageProps.CLEAR_PICKUP_BLUEPRINT, meta.ClearPickupBlueprint);
                stage.SetProperty(VanillaStageProps.END_NOTE_ID, meta.EndNote);

                stage.SetProperty(VanillaStageProps.START_CAMERA_POSITION, (int)meta.StartCameraPosition);
                stage.SetProperty(VanillaStageProps.START_TRANSITION, meta.StartTransition);

                stage.SetProperty(EngineStageProps.TOTAL_FLAGS, meta.TotalFlags);
                stage.SetProperty(EngineStageProps.FIRST_WAVE_TIME, meta.FirstWaveTime);

                stage.SetProperty(VanillaLevelProps.ENEMY_POOL, meta.Spawns);
                stage.SetProperty(VanillaLevelProps.CONVEYOR_POOL, meta.ConveyorPool);

                stage.SetNeedBlueprints(meta.NeedBlueprints);
                stage.SetSpawnPointPower(meta.SpawnPointsPower);
                stage.SetSpawnPointMultiplier(meta.SpawnPointsMultiplier);
                stage.SetSpawnPointAddition(meta.SpawnPointsAddition);

                foreach (var pair in meta.Properties)
                {
                    stage.SetProperty(PropertyMapper.ConvertFromName(pair.Key), pair.Value);
                }
            }
        }
        private void LoadSeedOptionProperties()
        {
            foreach (var meta in Global.Game.GetModSeedOptionMetas(spaceName))
            {
                if (meta == null)
                    continue;
                var seedOptionDefinition = this.GetSeedOptionDefinition(new NamespaceID(spaceName, meta.ID));
                if (seedOptionDefinition == null)
                    continue;
                seedOptionDefinition.SetProperty(LogicSeedOptionProps.COST, meta.Cost);
                seedOptionDefinition.SetProperty(LogicSeedOptionProps.ICON, meta.Icon);
            }
        }
        private void LoadArtifactProperties()
        {
            foreach (IArtifactMeta meta in Global.Game.GetModArtifactMetas(spaceName))
            {
                if (meta == null)
                    continue;
                var name = meta.ID;
                var artifact = this.GetArtifactDefinition(new NamespaceID(spaceName, name));
                if (artifact == null)
                    continue;
                artifact.SetUnlockID(meta.Unlock);
                artifact.SetSpriteReference(meta.Sprite);
            }
        }
        private void AddEntityBehaviours()
        {
            foreach (var behaviour in GetDefinitions<EntityBehaviourDefinition>(EngineDefinitionTypes.ENTITY_BEHAVIOUR))
            {
                var entity = this.GetEntityDefinition(behaviour.GetMatchEntityID());
                if (entity == null)
                    continue;
                if (entity.HasBehaviour(behaviour))
                {
                    Debug.LogWarning($"Entity {entity.GetID()} has multiple Entity Behaviours.");
                }
                else
                {
                    entity.AddBehaviour(behaviour);
                }
            }
        }
        private void AddOptionSeeds()
        {
            foreach (var option in GetDefinitions<SeedOptionDefinition>(LogicDefinitionTypes.SEED_OPTION))
            {
                var seedDef = new OptionSeed(Namespace, option.Name, option.GetCost());
                AddDefinition(seedDef);
            }
        }

        public const string spaceName = "mvz2";
    }
}
