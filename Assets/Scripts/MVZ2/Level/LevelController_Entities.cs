using System;
using System.Collections.Generic;
using System.Linq;
using MukioI18n;
using MVZ2.Collisions;
using MVZ2.Entities;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Effects;
using MVZ2.Level.UI;
using MVZ2.Metas;
using MVZ2.Supporters;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Almanacs;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Saves;
using MVZ2Logic;
using MVZ2Logic.HeldItems;
using MVZ2Logic.Level;
using PVZEngine.Entities;
using PVZEngine.Level.Collisions;
using PVZEngine.SeedPacks;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

namespace MVZ2.Level
{
    public partial class LevelController : MonoBehaviour, IDisposable
    {
        #region 公有方法
        public EntityController GetEntityController(Entity entity)
        {
            return entities.FirstOrDefault(e => e.Entity == entity);
        }
        #endregion

        #region 私有方法

        #region 事件回调
        private void Engine_OnEntitySpawnCallback(Entity entity)
        {
            CreateControllerForEntity(entity);
        }
        private void Engine_OnEntityRemoveCallback(Entity entity)
        {
            RemoveControllerFromEntity(entity);
        }
        private void Engine_PostUseEntityBlueprintCallback(SeedPack blueprint, Entity entity)
        {
            if (!Main.OptionsManager.ShowSponsorNames())
                return;
            if (entity.IsEntityOf(VanillaContraptionID.furnace))
            {
                ShowFurnaceSponsorName(entity);
            }
            else if (entity.IsEntityOf(VanillaContraptionID.moonlightSensor))
            {
                ShowMoonlightSensorSponsorName(entity);
            }
        }
        private void UI_OnEntityPointerEnterCallback(EntityController entityCtrl, PointerEventData eventData)
        {
            SetHoveredEntity(entityCtrl);
            if (IsGameRunning())
                return;
            // 显示查看图鉴提示
            if (!entityCtrl.Entity.IsPreviewEnemy() || !CanChooseBlueprints())
                return;
            ShowTooltip(new EntityTooltipSource(this, entityCtrl));
        }
        private void UI_OnEntityPointerExitCallback(EntityController entity, PointerEventData eventData)
        {
            SetHoveredEntity(null);
            // 隐藏查看图鉴提示
            if (entity.Entity.IsPreviewEnemy())
            {
                HideTooltip();
            }
        }
        private void UI_OnEntityPointerDownCallback(EntityController entityCtrl, PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            var entity = entityCtrl.Entity;
            if (IsGameRunning())
            {
                var target = entityCtrl.GetHeldItemTarget(eventData);
                level.UseHeldItem(target, PointerInteraction.Press);
            }
            else
            {
                // 打开图鉴
                if (entity.IsPreviewEnemy() && Main.SaveManager.IsAlmanacUnlocked() && CanChooseBlueprints())
                {
                    var entityID = entityCtrl.Entity.GetDefinitionID();
                    if (Main.ResourceManager.IsEnemyInAlmanac(entityID) && Main.SaveManager.IsEnemyUnlocked(entityID))
                    {
                        HideTooltip();
                        OpenEnemyAlmanac(entity.GetDefinitionID());
                        Main.SoundManager.Play2D(VanillaSoundID.tap);
                    }
                }
            }
        }
        #endregion

        #region 叫声
        private void UpdateEnemyCry()
        {
            cryTimeCheckTimer.Run();
            if (cryTimeCheckTimer.Expired)
            {
                cryTimeCheckTimer.Reset();

                var crySoundEnemies = GetCrySoundEnemies();
                var enemyCount = crySoundEnemies.Count();
                float t = Mathf.Clamp01((float)(enemyCount - MinEnemyCryCount) / (MaxEnemyCryCount - MinEnemyCryCount));
                maxCryTime = (int)Mathf.Lerp(MaxCryInterval, MinCryInterval, t);
            }
            cryTimer.Run();
            if (cryTimer.MaxFrame - cryTimer.Frame >= maxCryTime)
            {
                cryTimer.Reset();

                var crySoundEnemies = GetCrySoundEnemies();
                var enemyCount = crySoundEnemies.Count();
                if (enemyCount <= 0)
                    return;
                var crySoundEnemy = crySoundEnemies.Random(rng);
                crySoundEnemy.PlaySound(crySoundEnemy.GetCrySound(), crySoundEnemy.GetCryPitch());
            }
        }
        private IEnumerable<Entity> GetCrySoundEnemies()
        {
            var enemies = level.GetEntities(EntityTypes.ENEMY);
            if (enemies.Length <= 0)
                return Enumerable.Empty<Entity>();
            return enemies.Where(e => e.GetCrySound() != null);
        }
        #endregion

        #region 控制器
        private EntityController CreateControllerForEntity(Entity entity)
        {
            var entityController = Instantiate(entityTemplate.gameObject, LawnToTrans(entity.Position), Quaternion.identity, entitiesRoot).GetComponent<EntityController>();
            entityController.Init(this, entity);
            entityController.OnPointerEnter += UI_OnEntityPointerEnterCallback;
            entityController.OnPointerExit += UI_OnEntityPointerExitCallback;
            entityController.OnPointerDown += UI_OnEntityPointerDownCallback;
            entities.Add(entityController);
            return entityController;
        }
        private bool RemoveControllerFromEntity(Entity entity)
        {
            var entityController = GetEntityController(entity);
            if (entityController)
            {
                entityController.OnPointerEnter -= UI_OnEntityPointerEnterCallback;
                entityController.OnPointerExit -= UI_OnEntityPointerExitCallback;
                entityController.OnPointerDown -= UI_OnEntityPointerDownCallback;
                Destroy(entityController.gameObject);
                return entities.Remove(entityController);
            }
            return false;
        }
        #endregion

        #region 赞助者
        private void ShowFurnaceSponsorName(Entity furnace)
        {
            var names = Main.SponsorManager.GetSponsorPlanNames(SponsorPlans.Furnace.TYPE, SponsorPlans.Furnace.FURNACE);
            if (names.Length <= 0)
                return;
            var text = furnace.Spawn(VanillaEffectID.floatingText, furnace.GetCenter(), rng.Next());
            var name = names.Random(text.RNG);
            FloatingText.SetText(text, name);
        }
        private void ShowMoonlightSensorSponsorName(Entity sensor)
        {
            var names = Main.SponsorManager.GetSponsorPlanNames(SponsorPlans.Sensor.TYPE, SponsorPlans.Sensor.MOONLIGHT_SENSOR);
            if (names.Length <= 0)
                return;
            var text = sensor.Spawn(VanillaEffectID.floatingText, sensor.GetCenter(), rng.Next());
            var name = names.Random(text.RNG);
            FloatingText.SetText(text, name);
        }
        #endregion

        private void SetHoveredEntity(EntityController entity)
        {
            hoveredEntity = entity;
            UpdateEntityHighlight();
        }
        private void UpdateEntityHighlight()
        {
            if (!hoveredEntity || hoveredEntity.GetHoveredPointerCount() <= 0)
            {
                SetHighlightedEntity(null);
                return;
            }
            var pointerId = hoveredEntity.GetHoveredPointerId(0);
            var pointerPosition = Main.InputManager.GetPointerPosition(pointerId);
            var worldPosition = levelCamera.Camera.ScreenToWorldPoint(pointerPosition);
            var target = hoveredEntity.GetHeldItemTarget(worldPosition);
            var highlight = level.GetHeldHighlight(target);
            if (highlight == HeldHighlight.Entity)
            {
                SetHighlightedEntity(hoveredEntity);
            }
            else if (highlight == HeldHighlight.ProtectedEntity)
            {
                var protectTarget = hoveredEntity.Entity.GetProtectingTarget();
                if (protectTarget != null)
                {
                    var protectCtrl = GetEntityController(protectTarget);
                    SetHighlightedEntity(protectCtrl);
                }
            }
        }
        private void SetHighlightedEntity(EntityController entity)
        {
            if (highlightedEntity)
            {
                highlightedEntity.SetHighlight(false);
            }
            highlightedEntity = entity;
            if (highlightedEntity)
            {
                highlightedEntity.SetHighlight(true);
            }
        }

        private ICollisionSystem GetCollisionSystem()
        {
            return unityCollisionSystem;
        }

        #endregion

        #region 属性字段
        public const int SelfFaction = 0;
        public const int EnemyFaction = 1;
        public const int MinEnemyCryCount = 1;
        public const int MaxEnemyCryCount = 20;
        public const int MinCryInterval = 60;
        public const int MaxCryInterval = 300;

        [TranslateMsg("实体提示", VanillaStrings.CONTEXT_ENTITY_TOOLTIP)]
        public const string VIEW_IN_ALMANAC = "在图鉴中查看";

        private List<EntityController> entities = new List<EntityController>();
        private EntityController hoveredEntity;
        private EntityController highlightedEntity;

        #region 保存属性
        private FrameTimer cryTimer = new FrameTimer(MaxCryInterval);
        private FrameTimer cryTimeCheckTimer = new FrameTimer(7);
        private int maxCryTime = MaxCryInterval;
        #endregion

        [Header("Entities")]
        [SerializeField]
        private EntityController entityTemplate;
        [SerializeField]
        private Transform entitiesRoot;
        [SerializeField]
        private UnityCollisionSystem unityCollisionSystem;
        #endregion

        private class EntityTooltipSource : ITooltipSource
        {
            private LevelController controller;
            private EntityController entityCtrl;

            public EntityTooltipSource(LevelController controller, EntityController entityCtrl)
            {
                this.controller = controller;
                this.entityCtrl = entityCtrl;
            }

            public ITooltipTarget GetTarget(LevelController level)
            {
                return entityCtrl;
            }

            public TooltipViewData GetViewData(LevelController level)
            {
                var main = controller.Main;
                var name = main.ResourceManager.GetEntityName(entityCtrl.Entity.GetDefinitionID());
                var description = string.Empty;
                if (main.SaveManager.IsAlmanacUnlocked())
                {
                    var entityID = entityCtrl.Entity.GetDefinitionID();
                    if (main.ResourceManager.IsEnemyInAlmanac(entityID) && main.SaveManager.IsEnemyUnlocked(entityID))
                    {
                        description = main.LanguageManager._p(VanillaStrings.CONTEXT_ENTITY_TOOLTIP, VIEW_IN_ALMANAC);
                    }
                }
                return new TooltipViewData()
                {
                    name = name,
                    description = description
                };
            }
        }
    }
}
