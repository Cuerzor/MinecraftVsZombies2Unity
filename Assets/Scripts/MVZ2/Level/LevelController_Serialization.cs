using System;
using System.Linq;
using System.Runtime.Serialization;
using MVZ2.Entities;
using MVZ2.Games;
using MVZ2.Level.Components;
using MVZ2.Level.UI;
using MVZ2.Vanilla.Callbacks;
using MVZ2Logic.Callbacks;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.Level
{
    using VisibleState = MVZ2.Level.UI.LevelUIPreset.VisibleState;
    public partial class LevelController : MonoBehaviour, IDisposable
    {
        #region 公有方法
        public SerializableLevelController SaveGame()
        {
            return new SerializableLevelController()
            {
                rng = rng.ToSerializable(),
                identifiers = LevelManager.GetLevelStateIdentifierList(),

                bannerProgresses = bannerProgresses?.ToArray(),
                levelProgress = levelProgress,
                bossProgress = bossProgress,
                progressBarMode = progressBarMode,
                bossProgressBarStyle = bossProgressBarStyle,

                maxCryTime = maxCryTime,
                cryTimer = cryTimer,
                cryTimeCheckTimer = cryTimeCheckTimer,

                musicID = CurrentMusic,
                musicTime = MusicTime,
                musicVolume = MusicVolume,

                energyActive = EnergyActive,
                blueprintsActive = BlueprintsActive,
                pickaxeActive = PickaxeActive,
                starshardActive = StarshardActive,
                triggerActive = TriggerActive,

                entities = entities.Select(e => e.ToSerializable()).ToArray(),

                parts = parts.Select(p => p.ToSerializable()).ToArray(),
                
                areaModel = model.ToSerializable(),

                level = SerializeLevel(),
                uiPreset = GetUIPreset().ToSerializable()
            };
        }
        public void LoadGame(SerializableLevelController seri, Game game, NamespaceID areaID, NamespaceID stageID)
        {
            if (!LevelManager.GetLevelStateIdentifierList().Compare(seri.identifiers))
            {
                ShowLevelErrorLoadingDialog();
                return;
            }

            try
            {
                rng = RandomGenerator.FromSerializable(seri.rng);
                level = LevelEngine.Deserialize(seri.level, game, game, game, GetCollisionSystem());
                InitLevelEngine(level, game, areaID, stageID);

                level.DeserializeComponents(seri.level);
                bannerProgresses = seri.bannerProgresses?.ToArray();
                levelProgress = seri.levelProgress;
                bossProgress = seri.bossProgress;
                progressBarMode = seri.progressBarMode;
                bossProgressBarStyle = seri.bossProgressBarStyle;

                maxCryTime = seri.maxCryTime;
                cryTimer = seri.cryTimer;
                cryTimeCheckTimer = seri.cryTimeCheckTimer;

                CurrentMusic = seri.musicID;
                MusicTime = seri.musicTime;
                MusicVolume = seri.musicVolume;

                EnergyActive = seri.energyActive;
                BlueprintsActive = seri.blueprintsActive;
                PickaxeActive = seri.pickaxeActive;
                StarshardActive = seri.starshardActive;
                TriggerActive = seri.triggerActive;

                InitGrids();

                var uiPreset = GetUIPreset();
                uiPreset.LoadFromSerializable(seri.uiPreset);
                uiPreset.UpdateFrame(0);
                // uiPreset的animator.Update会导致第一次加载该场景时，蓝图UI的子模型显示状态不正确，所以放在前面

                foreach (var part in parts)
                {
                    var seriPart = seri.parts.FirstOrDefault(p => p.id == part.ID);
                    if (seriPart == null)
                    {
                        Debug.LogWarning($"Could not find serialized LevelControllerPart data with id {part.ID}.");
                        continue;
                    }
                    part.LoadFromSerializable(seriPart);
                }

                foreach (var entity in level.GetEntities())
                {
                    var controller = CreateControllerForEntity(entity);

                    var seriEntity = seri.entities.FirstOrDefault(e => e.id == entity.ID);
                    if (seriEntity == null)
                        throw new SerializationException($"Could not find entity data with id {entity.ID} in the level state data.");
                    controller.LoadFromSerializable(seriEntity);
                    controller.UpdateFrame(0);
                }
                model.LoadFromSerializable(seri.areaModel);
            }
            catch (Exception e)
            {
                ShowLevelErrorLoadingDialog(e);
                Debug.LogException(e);
                return;
            }

            // 设置UI可见状态
            SetUIVisibleState(VisibleState.InLevel);
            // 相机位置
            SetCameraPosition(LevelCameraPosition.Lawn);
            // 手持物品
            level.ResetHeldItem();
            // 关卡名
            UpdateLevelName();
            // 关卡进度条
            RefreshProgressBar();
            // 难度名称
            UpdateDifficultyName();
            // 能量、关卡进度条、手持物品、蓝图状态、星之碎片
            SetStarshardIcon();
            UpdateInLevelUI(0);
            // 金钱
            UpdateMoney();
            UpdateCamera();
            ShowMoney();
            // 光照
            UpdateLighting();

            // 游戏开始状态
            SetGameStarted(true);


            foreach (var component in level.GetComponents())
            {
                if (component is IMVZ2LevelComponent comp)
                {
                    comp.PostLevelLoad();
                }
            }
            foreach (var part in parts)
            {
                part.PostLevelLoad();
            }

            PauseGame();
            ShowLevelLoadedDialog();
            levelLoaded = true;

            level.AreaDefinition.PostLoad(level);
        }

        #endregion

        #region 私有方法

        #region 序列化
        private void AddLevelCallbacks()
        {
            level.OnEntitySpawn += Engine_OnEntitySpawnCallback;
            level.OnEntityRemove += Engine_OnEntityRemoveCallback;
            level.OnGameOver += Engine_OnGameOverCallback;
            
            foreach (var controller in parts)
            {
                controller.AddEngineCallbacks(level);
            }

            level.OnClear += Engine_OnClearCallback;

            level.AddTrigger(LevelCallbacks.POST_WAVE_FINISHED, PostWaveFinishedCallback);
            level.AddTrigger(VanillaCallbacks.POST_HUGE_WAVE_APPROACH, PostHugeWaveApproachCallback);
            level.AddTrigger(VanillaCallbacks.POST_FINAL_WAVE, PostFinalWaveCallback);

            level.AddTrigger(VanillaLevelCallbacks.POST_USE_ENTITY_BLUEPRINT, Engine_PostUseEntityBlueprintCallback);
        }
        private void ApplyComponents(LevelEngine level)
        {
            level.AddComponent(new AdviceComponent(level, this));
            level.AddComponent(new HeldItemComponent(level, this));
            level.AddComponent(new UIComponent(level, this));
            level.AddComponent(new LogicComponent(level, this));
            level.AddComponent(new SoundComponent(level, this));
            level.AddComponent(new TalkComponent(level, this));
            level.AddComponent(new MusicComponent(level, this));
            level.AddComponent(new MoneyComponent(level, this));
            level.AddComponent(new LightComponent(level, this));
            level.AddComponent(new ArtifactComponent(level, this));
            level.AddComponent(new BlueprintComponent(level, this));
        }
        private SerializableLevel SerializeLevel()
        {
            return level.Serialize();
        }
        #endregion

        #endregion
    }
    public class SerializableLevelController
    {
        public SerializableRNG rng;
        public LevelDataIdentifierList identifiers;

        public float levelProgress;
        public float[] bannerProgresses;
        public float bossProgress;
        public NamespaceID bossProgressBarStyle;
        public bool progressBarMode;

        public NamespaceID musicID;
        public float musicTime;
        public float musicVolume;

        public bool energyActive;
        public bool blueprintsActive;
        public bool pickaxeActive;
        public bool starshardActive;
        public bool triggerActive;

        public SerializableLevelControllerPart[] parts;

        public int maxCryTime;
        public FrameTimer cryTimer;
        public FrameTimer cryTimeCheckTimer;

        public SerializableEntityController[] entities;
        public SerializableAreaModelData areaModel;

        public SerializableLevel level;

        public SerializableLevelUIPreset uiPreset;
    }
}
