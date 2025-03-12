using System;
using System.Collections.Generic;
using MVZ2.Cursors;
using MVZ2.HeldItems;
using MVZ2.Level;
using MVZ2.Level.UI;
using MVZ2.Managers;
using MVZ2.Models;
using MVZ2.Vanilla.Almanacs;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic;
using MVZ2Logic.HeldItems;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Armors;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Models;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVZ2.Entities
{
    public class EntityController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ILevelRaycastReceiver, ITooltipTarget
    {
        #region 公有方法
        public void Init(LevelController level, Entity entity)
        {
            Level = level;
            Entity = entity;
            rng = new RandomGenerator(entity.InitSeed);
            gameObject.name = entity.ToString();
            entity.PostInit += PostInitCallback;
            entity.PostPropertyChanged += PostPropertyChangedCallback;
            entity.OnChangeModel += OnChangeModelCallback;

            entity.OnEquipArmor += OnArmorEquipCallback;
            entity.OnDestroyArmor += OnArmorDestroyCallback;
            entity.OnRemoveArmor += OnArmorRemoveCallback;

            bodyModelInterface = new BodyModelInterface(this);
            armorModelInterface = new ArmorModelInterface(this);
            entity.SetModelInterface(bodyModelInterface, armorModelInterface);
            SetModel(Entity.ModelID);
            if (Model)
            {
                Model.ClearArmorModel();
            }

            transform.position = Level.LawnToTrans(Entity.Position);
            lastPosition = transform.position;
        }

        #region 模型
        public void SetModel(NamespaceID modelId)
        {
            if (Model)
            {
                Destroy(Model.gameObject);
                Model = null;
            }
            var model = Models.Model.Create(modelId, transform, Level.GetCamera(), Entity.InitSeed);
            Model = model;
            if (!Model)
                return;
            UpdateEntityModel();
            modelPropertyCache.UpdateAll(this);
            UpdateArmorModel();
            Model.UpdateFrame(0);
        }
        public void SetSimulationSpeed(float simulationSpeed)
        {
            if (Model)
            {
                Model.SetSimulationSpeed(simulationSpeed);
            }
        }
        public void ChangeArmorModel(NamespaceID modelID)
        {
            if (!Model)
                return;
            Model.RemoveArmor();
            Model.CreateArmor(modelID);
        }
        #endregion

        #region 更新
        public void UpdateFixed()
        {
            if (Model)
            {
                Model.UpdateFixed();
            }
            if (Level.IsGameRunning())
            {
                UpdateHoldAndStreak();
            }
        }
        public void UpdateFrame(float deltaTime)
        {
            var posOffset = GetTransformOffset();

            var pos = Entity.Position;
            var currentTransPos = Level.LawnToTrans(pos);
            transform.position = Vector3.Lerp(lastPosition, currentTransPos + posOffset, 0.5f);
            var finalOffset = transform.position - currentTransPos;
            lastPosition = transform.position;

            UpdateShadow(finalOffset);
            if (Model)
            {
                UpdateEntityModel();
                UpdateArmorModel();
                Model.UpdateFrame(deltaTime);
            }
        }
        private void UpdateHoldAndStreak()
        {
            bool pressed = false;
            bool holding = false;
            foreach (var id in hoveredPointerId)
            {
                if (!pressed && pressedPointerId.Contains(id))
                {
                    pressed = true;
                }
                if (!holding && Main.InputManager.IsPointerHolding(id))
                {
                    holding = true;
                }
            }
            if (pressed)
            {
                // 按住
                var target = GetHeldItemTarget();
                Entity.Level.UseHeldItem(target, PointerInteraction.Hold);
            }
            else if (holding)
            {
                // 划过
                var target = GetHeldItemTarget();
                Entity.Level.UseHeldItem(target, PointerInteraction.Streak);
            }
        }
        #endregion

        public void SetHighlight(bool highlight)
        {
            isHighlight = highlight;
            modelPropertyCache.SetDirtyProperty(EntityPropertyCache.PropertyName.ColorOffset);
        }
        public bool IsHovered()
        {
            return hoveredPointerId.Count > 0;
        }
        public bool IsPressed()
        {
            return pressedPointerId.Count > 0;
        }
        public int GetHoveredPointerCount()
        {
            return hoveredPointerId.Count;
        }
        public int GetHoveredPointerId(int index)
        {
            return hoveredPointerId[index];
        }
        public Vector2 TransformWorld2ColliderPosition(Vector3 worldPosition)
        {
            if (Model is not SpriteModel spriteModel)
                return Vector2.zero;
            var collider = spriteModel.Collider;
            if (collider is not BoxCollider2D boxCollider)
                return Vector2.zero;
            var pos2D = (Vector2)(collider.transform.position + collider.transform.TransformDirection(collider.offset));
            var lossyScale = (Vector2)collider.transform.lossyScale;
            var size = boxCollider.size;
            var lossySize = Vector2.Scale(size, lossyScale);
            var origin = pos2D - lossySize * 0.5f;

            var relativeWorldPos = (Vector2)worldPosition - origin;
            var colliderX = relativeWorldPos.x / lossySize.x;
            var colliderY = relativeWorldPos.y / lossySize.y;

            return new Vector2(colliderX, colliderY);
        }
        public HeldItemTargetEntity GetHeldItemTarget(Vector3 worldPosition)
        {
            var pos = TransformWorld2ColliderPosition(worldPosition);
            return new HeldItemTargetEntity(Entity, pos);
        }
        public HeldItemTargetEntity GetHeldItemTarget(PointerEventData data = null)
        {
            var pos = Vector2.zero;
            if (data != null)
            {
                pos = TransformWorld2ColliderPosition(data.pointerCurrentRaycast.worldPosition);
            }
            return new HeldItemTargetEntity(Entity, pos);
        }
        public SerializableEntityController ToSerializable()
        {
            return new SerializableEntityController()
            {
                id = Entity.ID,
                model = Model ? Model.ToSerializable() : null
            };
        }
        public void LoadFromSerializable(SerializableEntityController serializable)
        {
            if (Model && serializable.model != null)
            {
                Model.LoadFromSerializable(serializable.model);
            }
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Update()
        {
            var engine = Entity.Level;
            bool cursorValid = IsHovered() && Level.IsGameRunning() && !engine.IsHoldingItem();
            if (cursorValid)
            {
                if (_cursorSource == null)
                {
                    _cursorSource = new EntityCursorSource(this, CursorType.Point);
                    Main.CursorManager.AddCursorSource(_cursorSource);
                }
            }
            else
            {
                if (_cursorSource != null)
                {
                    Main.CursorManager.RemoveCursorSource(_cursorSource);
                    _cursorSource = null;
                }
            }
        }
        #endregion

        #region 事件回调
        private void PostInitCallback()
        {
            UpdateFrame(0);
        }
        private void PostPropertyChangedCallback(PropertyKey key)
        {
            modelPropertyCache.SetDirtyProperty(key);
        }
        private void OnChangeModelCallback(NamespaceID modelID)
        {
            SetModel(modelID);
        }
        private void OnArmorEquipCallback(Armor armor)
        {
            CreateArmorModel(armor);
        }
        private void OnArmorDestroyCallback(Armor armor, ArmorDamageResult result)
        {
        }
        private void OnArmorRemoveCallback(Armor armor)
        {
            RemoveArmorModel();
        }
        #endregion

        #region 接口实现
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            hoveredPointerId.Add(eventData.pointerId);
            OnPointerEnter?.Invoke(this, eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            hoveredPointerId.Remove(eventData.pointerId);
            OnPointerExit?.Invoke(this, eventData);
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            pressedPointerId.Add(eventData.pointerId);
            OnPointerDown?.Invoke(this, eventData);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            pressedPointerId.Remove(eventData.pointerId);
            OnPointerUp?.Invoke(this, eventData);
        }
        bool ILevelRaycastReceiver.IsValidReceiver(LevelEngine level, HeldItemDefinition definition, IHeldItemData data, PointerEventData d)
        {
            if (Entity.IsPreviewEnemy())
            {
                return true;
            } 
            if (definition == null)
                return false;
            var target = GetHeldItemTarget(d);
            return definition.IsValidFor(target, data);
        }
        int ILevelRaycastReceiver.GetSortingLayer()
        {
            return Model.GraphicGroup.SortingLayerID;
        }
        int ILevelRaycastReceiver.GetSortingOrder()
        {
            return Model.GraphicGroup.SortingOrder;
        }
        #endregion

        #region 位置
        protected void UpdateShadow(Vector3 posOffset)
        {
            var pos = Entity.Position;
            var groundY = Entity.GetGroundY();
            var relativeY = pos.y - groundY;
            var shadowPos = pos;
            shadowPos.y = groundY;
            shadowPos += modelPropertyCache.ShadowOffset;

            float scale = 1 + relativeY / 300;
            float alpha = 1 - relativeY / 300;
            var shadowTransform = Shadow.transform;
            shadowTransform.position = Level.LawnToTrans(shadowPos) + posOffset;
            shadowTransform.localScale = modelPropertyCache.ShadowScale * scale;
            Shadow.gameObject.SetActive(!modelPropertyCache.ShadowHidden);
            Shadow.SetAlpha(modelPropertyCache.ShadowAlpha * alpha);
        }
        protected float GetZOffset()
        {
            float zOffset = 0;
            if (zOffsetDict.TryGetValue(Entity.Type, out float offset))
            {
                zOffset = offset * Level.LawnToTransScale;
            }
            return zOffset;
        }
        protected Vector3 GetTransformOffset()
        {
            float zOffset = GetZOffset();
            return Vector3.back * zOffset;
        }
        #endregion

        #region 护甲
        private void CreateArmorModel(Armor armor)
        {
            if (armor?.Definition == null)
                return;
            var modelID = armor.Definition.GetModelID();
            CreateArmorModel(modelID);
        }
        private void CreateArmorModel(NamespaceID modelID)
        {
            if (!Model)
                return;
            if (!NamespaceID.IsValid(modelID))
                return;
            Model.CreateArmor(modelID);
        }
        private void RemoveArmorModel()
        {
            if (!Model)
                return;
            Model.RemoveArmor();
        }
        private void UpdateArmorModel()
        {
            if (!Model)
                return;
            var armor = Entity.EquipedArmor;
            if (armor == null)
                return;
            var armorModel = Model.GetArmorModel();
            if (!armorModel)
                return;
            armorModel.GraphicGroup.SetTint(armor.GetTint());
            armorModel.GraphicGroup.SetColorOffset(armor.GetColorOffset());
        }
        #endregion

        #region 模型
        private void UpdateEntityModel()
        {
            if (!Model)
                return;

            if (Level.IsGameOver() && (Entity.Type == EntityTypes.ENEMY || Entity.Type == EntityTypes.BOSS))
            {
                Model.SetAnimatorInt("State", VanillaEntityStates.WALK);
            }
            var groundPos = Entity.Position;
            groundPos.y = Entity.GetGroundY();
            var transGroundPos = Level.LawnToTrans(groundPos);
            var transPos = Level.LawnToTrans(Entity.Position);
            var transOffset = transGroundPos - transPos;

            var rendererGroup = Model.GraphicGroup;
            Model.SetGroundPosition(transform.position + transOffset);
            Model.GetCenterTransform().localEulerAngles = Entity.RenderRotation;

            if (modelPropertyCache.IsDirty)
            {
                modelPropertyCache.Update(this);
            }
        }
        #endregion

        private Color GetColorOffset()
        {
            var color = Entity.GetColorOffset();
            if (isHighlight && Entity.Level.IsHoldingItem())
            {
                color = ColorCalculator.Blend(new Color(1, 1, 1, 0.5f), color, BlendOperator.SrcAlpha, BlendOperator.OneMinusSrcAlpha);
            }
            return color;
        }
        #endregion

        #region View
        public void TriggerView(string name)
        {
            if (Model)
                Model.TriggerAnimator(name);
        }
        public void SetViewBool(string name, bool value)
        {
            if (Model)
                Model.SetAnimatorBool(name, value);
        }
        public void SetViewInt(string name, int value)
        {
            if (Model)
                Model.SetAnimatorInt(name, value);
        }
        public void SetViewFloat(string name, float value)
        {
            if (Model)
                Model.SetAnimatorFloat(name, value);
        }
        #endregion

        #region 事件
        public event Action<EntityController, PointerEventData> OnPointerEnter;
        public event Action<EntityController, PointerEventData> OnPointerExit;
        public event Action<EntityController, PointerEventData> OnPointerDown;
        public event Action<EntityController, PointerEventData> OnPointerUp;
        #endregion

        #region 属性字段
        public static readonly Dictionary<int, float> zOffsetDict = new Dictionary<int, float>()
        {
            { EntityTypes.PLANT, 0 },
            { EntityTypes.OBSTACLE, 0 },
            { EntityTypes.BOSS, 2 },
            { EntityTypes.ENEMY, 3 },
            { EntityTypes.PROJECTILE, 4 },
            { EntityTypes.CART, 5 },
            { EntityTypes.EFFECT, 6 },
            { EntityTypes.PICKUP, 7 },
        };
        public MainManager Main => MainManager.Instance;
        public Model Model { get; private set; }
        public ShadowController Shadow => shadow;
        public Entity Entity { get; private set; }
        public LevelController Level { get; private set; }
        private RandomGenerator rng;
        private List<int> hoveredPointerId = new List<int>();
        private List<int> pressedPointerId = new List<int>();
        private bool isHighlight;
        private EntityCursorSource _cursorSource;
        private Vector3 lastPosition;
        private IModelInterface bodyModelInterface;
        private IModelInterface armorModelInterface;
        private EntityPropertyCache modelPropertyCache = new EntityPropertyCache();
        [SerializeField]
        private ShadowController shadow;
        [SerializeField]
        private TooltipAnchor tooltipAnchor;
        #region shader相关属性
        protected MaterialPropertyBlock propertyBlock;
        #endregion shader相关属性

        TooltipAnchor ITooltipTarget.Anchor => tooltipAnchor;

        #endregion

        #region 内嵌类

        private class EntityPropertyCache
        {
            public void UpdateAll(EntityController entityCtrl)
            {
                var entity = entityCtrl.Entity;
                var model = entityCtrl.Model;
                var rendererGroup = model.GraphicGroup;
                rendererGroup.SetTint(entity.GetTint());
                rendererGroup.SetColorOffset(entityCtrl.GetColorOffset());
                model.transform.localScale = entity.GetDisplayScale();
                rendererGroup.SortingLayerID = entity.GetSortingLayer();
                rendererGroup.SortingOrder = entity.GetSortingOrder();
                if (model is SpriteModel sprModel)
                {
                    sprModel.SetLightVisible(entity.IsLightSource());
                    sprModel.SetLightColor(entity.GetLightColor());
                    var lightScaleLawn = entity.GetLightRange();
                    var lightScale = new Vector2(lightScaleLawn.x, Mathf.Max(lightScaleLawn.y, lightScaleLawn.z)) * entityCtrl.Level.LawnToTransScale;
                    sprModel.SetLightRange(lightScale);
                }

                ShadowHidden = entity.IsShadowHidden();
                ShadowAlpha = entity.GetShadowAlpha();
                ShadowOffset = entity.GetShadowOffset();
                ShadowScale = entity.GetShadowScale();

                dirtyProperties.Clear();
            }
            public void Update(EntityController entityCtrl)
            {
                var entity = entityCtrl.Entity;
                var model = entityCtrl.Model;
                var rendererGroup = model.GraphicGroup;
                foreach (var dirtyProperty in dirtyProperties)
                {
                    switch (dirtyProperty)
                    {
                        case PropertyName.Tint:
                            rendererGroup.SetTint(entity.GetTint());
                            break;
                        case PropertyName.ColorOffset:
                            rendererGroup.SetColorOffset(entityCtrl.GetColorOffset());
                            break;
                        case PropertyName.DisplayScale:
                            model.transform.localScale = entity.GetDisplayScale();
                            break;
                        case PropertyName.SortingLayer:
                            rendererGroup.SortingLayerID = entity.GetSortingLayer();
                            break;
                        case PropertyName.SortingOrder:
                            rendererGroup.SortingOrder = entity.GetSortingOrder();
                            break;

                        case PropertyName.ShadowHidden:
                            ShadowHidden = entity.IsShadowHidden();
                            break;
                        case PropertyName.ShadowAlpha:
                            ShadowAlpha = entity.GetShadowAlpha();
                            break;
                        case PropertyName.ShadowOffset:
                            ShadowOffset = entity.GetShadowOffset();
                            break;
                        case PropertyName.ShadowScale:
                            ShadowScale = entity.GetShadowScale();
                            break;

                        case PropertyName.LightSource:
                            {
                                if (model is SpriteModel sprModel)
                                {
                                    sprModel.SetLightVisible(entity.IsLightSource());
                                }
                            }
                            break;
                        case PropertyName.LightColor:
                            {
                                if (model is SpriteModel sprModel)
                                {
                                    sprModel.SetLightColor(entity.GetLightColor());
                                }
                            }
                            break;
                        case PropertyName.LightRange:
                            {
                                if (model is SpriteModel sprModel)
                                {
                                    var lightScaleLawn = entity.GetLightRange();
                                    var lightScale = new Vector2(lightScaleLawn.x, Mathf.Max(lightScaleLawn.y, lightScaleLawn.z)) * entityCtrl.Level.LawnToTransScale;
                                    sprModel.SetLightRange(lightScale);
                                }
                            }
                            break;
                    }
                }
                dirtyProperties.Clear();
            }
            public void SetDirtyProperty(PropertyName property)
            {
                dirtyProperties.Add(property);
            }
            public void SetDirtyProperty(PropertyKey key)
            {
                foreach (var pair in propertyMap)
                {
                    if (pair.Key == key)
                    {
                        dirtyProperties.Add(pair.Value);
                        break;
                    }
                }
            }
            public bool IsDirty => dirtyProperties.Count > 0;
            public bool ShadowHidden { get; private set; }
            public Vector3 ShadowOffset { get; private set; }
            public Vector3 ShadowScale { get; private set; }
            public float ShadowAlpha { get; private set; }
            private HashSet<PropertyName> dirtyProperties = new HashSet<PropertyName>();
            private static readonly Dictionary<PropertyMeta, PropertyName> propertyMap = new Dictionary<PropertyMeta, PropertyName>()
            {
                { EngineEntityProps.TINT, PropertyName.Tint },
                { EngineEntityProps.COLOR_OFFSET, PropertyName.ColorOffset },
                { EngineEntityProps.DISPLAY_SCALE, PropertyName.DisplayScale },
                { VanillaEntityProps.SORTING_LAYER, PropertyName.SortingLayer },
                { VanillaEntityProps.SORTING_ORDER, PropertyName.SortingOrder },

                { VanillaEntityProps.SHADOW_HIDDEN, PropertyName.ShadowHidden },
                { VanillaEntityProps.SHADOW_OFFSET, PropertyName.ShadowOffset },
                { VanillaEntityProps.SHADOW_SCALE, PropertyName.ShadowScale },
                { VanillaEntityProps.SHADOW_ALPHA, PropertyName.ShadowAlpha },

                { VanillaEntityProps.IS_LIGHT_SOURCE, PropertyName.LightSource },
                { VanillaEntityProps.LIGHT_COLOR, PropertyName.LightColor },
                { VanillaEntityProps.LIGHT_RANGE, PropertyName.LightRange },
            };
            public enum PropertyName
            {
                Tint,
                ColorOffset,
                DisplayScale,
                SortingLayer,
                SortingOrder,

                ShadowHidden,
                ShadowOffset,
                ShadowScale,
                ShadowAlpha,

                LightSource,
                LightColor,
                LightRange,
            }
        }
        #endregion
    }
    public class EntityCursorSource : CursorSource
    {
        public EntityCursorSource(EntityController target, CursorType type, int priority = 0)
        {
            this.target = target;
            this.type = type;
            this.priority = priority;
        }

        public override bool IsValid()
        {
            return target;
        }

        public EntityController target;
        private int priority;
        public override int Priority => priority;
        private CursorType type;
        public override CursorType CursorType => type;
    }
    public class SerializableEntityController
    {
        public long id;
        public SerializableModelData model;
    }
}
