using System;
using MVZ2.Models;
using MVZ2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MVZ2.Level.UI
{
    public class LevelUIPreset : MonoBehaviour
    {
        #region 公有方法

        #region 基本
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void SetRaycasterMask(LayerMask mask)
        {
            foreach (var raycaster in raycasters)
            {
                raycaster.blockingMask = mask;
            }
        }
        public void UpdateFrame(float deltaTime)
        {
            animator.Update(deltaTime);
            for (int i = 0; i < artifactList.Count; i++)
            {
                var artifact = artifactList.getElement<ArtifactItemUI>(i);
                artifact.UpdateAnimator(deltaTime);
            }
            BlueprintChoose.UpdateFrame(deltaTime);
        }
        public void SetReceiveRaycasts(bool value)
        {
            foreach (var group in canvasGroups)
            {
                group.blocksRaycasts = value;
            }
        }
        #endregion

        #region 能量
        public void SetEnergyActive(bool value)
        {
            energyPanel.gameObject.SetActive(value);
        }
        public void SetEnergy(string value)
        {
            energyPanel.SetEnergy(value);
        }
        #endregion

        #region 铁镐
        public void SetPickaxeActive(bool visible)
        {
            pickaxeSlotObj.SetActive(visible);
        }
        public void SetPickaxeSelected(bool selected)
        {
            pickaxeSlot.SetSelected(selected);
        }
        public void SetPickaxeDisabled(bool selected)
        {
            pickaxeSlot.SetDisabled(selected);
        }
        public void SetPickaxeNumberText(PickaxeNumberText info)
        {
            pickaxeSlot.SetNumberText(info);
        }
        public PickaxeSlot GetPickaxeSlot()
        {
            return pickaxeSlot;
        }
        #endregion

        #region 触发
        public void SetTriggerActive(bool visible)
        {
            triggerSlotObj.SetActive(visible);
            triggerSlotConveyorObj.SetActive(visible);
        }
        public void SetTriggerSelected(bool selected)
        {
            triggerSlot.SetSelected(selected);
            triggerSlotConveyor.SetSelected(selected);
        }
        public TriggerSlot GetCurrentTriggerUI()
        {
            return Blueprints.IsConveyorMode() ? triggerSlotConveyor : triggerSlot;
        }
        #endregion

        #region 钱
        public void SetMoney(string money)
        {
            moneyPanel.SetMoney(money);
        }
        public void HideMoney()
        {
            moneyPanel.Hide();
        }
        public void SetMoneyFade(bool fade)
        {
            moneyPanel.SetFade(fade);
        }
        public void ResetMoneyFadeTime()
        {
            moneyPanel.ResetTimeout();
        }
        #endregion

        #region 星之碎片
        public void SetStarshardActive(bool visible)
        {
            starshardPanelObj.SetActive(visible);
        }
        public void SetStarshardIcon(Sprite icon)
        {
            starshardPanel.SetIconSprite(icon);
        }
        public void SetStarshardCount(int count, int maxCount)
        {
            starshardPanel.SetPoints(count, maxCount);
        }
        public void SetStarshardSelected(bool selected)
        {
            starshardPanel.SetSelected(selected);
        }
        public void SetStarshardDisabled(bool selected)
        {
            starshardPanel.SetDisabled(selected);
        }
        #endregion

        #region 右上角

        #region 游戏难度
        public void SetDifficulty(string difficulty)
        {
            difficultyText.text = difficulty;
        }
        #endregion

        #region 加速
        public void SetSpeedUp(bool speedUp)
        {
            speedUpEnabledObject.SetActive(speedUp);
            speedUpDisabledObject.SetActive(!speedUp);
        }
        #endregion

        #endregion

        #region 关卡进度

        #region 关卡名
        public void SetLevelName(string name)
        {
            levelNameText.text = name;
        }
        #endregion

        #region 关卡进度
        public void SetProgressBarVisible(bool visible)
        {
            progressBarRoot.SetActive(visible);
        }
        public void SetProgressBarMode(bool boss)
        {
            progressBar.gameObject.SetActive(!boss);
            bossProgressBar.gameObject.SetActive(boss);
        }
        public void SetLevelProgress(float progress)
        {
            progressBar.SetProgress(progress);
        }
        public void SetBannerProgresses(float[] progresses)
        {
            progressBar.SetBannerProgresses(progresses);
        }
        public void SetBossProgressTemplate(ProgressBarTemplateViewData template)
        {
            bossProgressBar.UpdateTemplate(template);
        }
        public void SetBossProgress(float progress)
        {
            bossProgressBar.SetProgress(progress);
        }
        #endregion

        #endregion

        #region 提示文本
        public void ShowHugeWaveText()
        {
            animator.SetTrigger("HugeWave");
        }
        public void ShowFinalWaveText()
        {
            animator.SetTrigger("FinalWave");
        }
        public void ShowReadySetBuild()
        {
            animator.SetTrigger("ReadySetBuild");
        }
        public void ShowAdvice(string advice)
        {
            adviceObject.SetActive(true);
            adviceText.text = advice;
        }
        public void HideAdvice()
        {
            adviceObject.SetActive(false);
        }
        #endregion

        #region 提示箭头
        public void SetHintArrowPointToBlueprint(int index)
        {
            var blueprint = Blueprints.GetCurrentModeBlueprint(index);
            if (!blueprint)
            {
                HideHintArrow();
                return;
            }
            hintArrow.SetVisible(true);
            hintArrow.SetTarget(blueprint.transform, hintArrowOffsetBlueprint * 0.01f, hintArrowAngleBlueprint);
        }
        public void SetHintArrowPointToPickaxe()
        {
            hintArrow.SetVisible(true);
            var pickaxe = pickaxeSlot;
            hintArrow.SetTarget(pickaxe.transform, hintArrowOffsetPickaxe * 0.01f, hintArrowAnglePickaxe);
        }
        public void SetHintArrowPointToTrigger()
        {
            hintArrow.SetVisible(true);
            var trigger = GetCurrentTriggerUI();
            hintArrow.SetTarget(trigger.transform, hintArrowOffsetTrigger * 0.01f, hintArrowAngleTrigger);
        }
        public void SetHintArrowPointToStarshard()
        {
            hintArrow.SetVisible(true);
            var starshard = starshardPanel.Icon;
            hintArrow.SetTarget(starshard.transform, hintArrowOffsetStarshard * 0.01f, hintArrowAngleStarshard);
        }
        public void SetHintArrowPointToEntity(Transform transform, float height)
        {
            hintArrow.SetVisible(true);
            hintArrow.SetTarget(transform, new Vector2(0, height + 16) * 0.01f, 180);
        }
        public void HideHintArrow()
        {
            hintArrow.SetVisible(false);
        }
        #endregion

        #region 工具提示
        public void ShowTooltip()
        {
            tooltip.gameObject.SetActive(true);
        }
        public void UpdateTooltip(ITooltipTarget ui, TooltipViewData viewData)
        {
            var anchor = ui.Anchor;
            if (!anchor || anchor.IsDisabled)
                return;
            tooltip.SetData(anchor.transform, anchor.Pivot, viewData);
        }
        public void HideTooltip()
        {
            tooltip.gameObject.SetActive(false);
        }
        #endregion

        #region 制品
        public void SetArtifactCount(int count)
        {
            artifactList.updateList(count, null, obj =>
            {
                var artifact = obj.GetComponent<ArtifactItemUI>();
                artifact.OnPointerEnter += OnArtifactPointerEnterCallback;
                artifact.OnPointerExit += OnArtifactPointerExitCallback;
            },
            obj =>
            {
                var artifact = obj.GetComponent<ArtifactItemUI>();
                artifact.OnPointerEnter -= OnArtifactPointerEnterCallback;
                artifact.OnPointerExit -= OnArtifactPointerExitCallback;
            });
        }
        public void SetArtifactIcon(int index, Sprite value)
        {
            var ui = GetArtifactAt(index);
            if (!ui) return;
            ui.SetIcon(value);
        }
        public void SetArtifactNumber(int index, string number)
        {
            var ui = GetArtifactAt(index);
            if (!ui) return;
            ui.SetNumber(number);
        }
        public void HighlightArtifact(int index)
        {
            var ui = GetArtifactAt(index);
            if (!ui) return;
            ui.Shine();
        }
        public void SetArtifactGrayscale(int index, bool value)
        {
            var ui = GetArtifactAt(index);
            if (!ui) return;
            ui.SetGrayscale(value);
        }
        public void SetArtifactGlowing(int index, bool value)
        {
            var ui = GetArtifactAt(index);
            if (!ui) return;
            ui.SetGlowing(value);
        }
        public ArtifactItemUI GetArtifactAt(int index)
        {
            return artifactList.getElement<ArtifactItemUI>(index);
        }
        #endregion

        public void SetCameraLimitWidth(float t)
        {
            t = Mathf.Clamp01(t);
            limitRegionLayoutElement.minWidth = cameraLimitWidth * t;
        }
        public void SetUIVisibleState(VisibleState state)
        {
            animator.SetInteger("UIState", (int)state);
        }
        public void SetUIDisabled(bool disabled)
        {
            animator.SetBool("DisableUI", disabled);
        }
        public Receiver GetReceiverType(RaycastReceiver receiver)
        {
            if (receiver == sideReceiver)
                return Receiver.Side;
            else if (receiver == bottomReceiver)
                return Receiver.Bottom;
            else
                return Receiver.Lawn;
        }
        public void SetBlueprintsSortingToChoosing(bool choosing)
        {
            blueprints.SetSortingToChoosing(choosing);
        }

        public SerializableLevelUIPreset ToSerializable()
        {
            SerializableAnimator[] artifactAnimators = new SerializableAnimator[artifactList.Count];
            for (int i = 0; i < artifactAnimators.Length; i++)
            {
                var artifact = artifactList.getElement<ArtifactItemUI>(i);
                artifactAnimators[i] = artifact.GetSerializableAnimator();
            }
            return new SerializableLevelUIPreset()
            {
                animator = new SerializableAnimator(animator),
                artifactAnimators = artifactAnimators
            };
        }
        public void LoadFromSerializable(SerializableLevelUIPreset serializable)
        {
            if (serializable == null)
                return;
            serializable.animator?.Deserialize(animator);
            if (serializable.artifactAnimators != null)
            {
                for (int i = 0; i < serializable.artifactAnimators.Length; i++)
                {
                    var artifact = artifactList.getElement<ArtifactItemUI>(i);
                    if (artifact == null)
                        continue;
                    artifact.LoadFromSerializableAnimator(serializable.artifactAnimators[i]);
                }
            }
        }

        public void CallStartGame()
        {
            OnStartGameCalled?.Invoke();
        }
        #endregion

        #region 私有方法
        private void Awake()
        {
            animator.enabled = false;
            sideReceiver.OnPointerDown += (data) => OnRaycastReceiverPointerDown?.Invoke(Receiver.Side, data);
            lawnReceiver.OnPointerDown += (data) => OnRaycastReceiverPointerDown?.Invoke(Receiver.Lawn, data);
            bottomReceiver.OnPointerDown += (data) => OnRaycastReceiverPointerDown?.Invoke(Receiver.Bottom, data);

            starshardPanel.OnPointerDown += (data) => OnStarshardPointerDown?.Invoke(data);

            pickaxeSlot.OnPointerEnter += (data) => OnPickaxePointerEnter?.Invoke(data);
            pickaxeSlot.OnPointerExit += (data) => OnPickaxePointerExit?.Invoke(data);
            pickaxeSlot.OnPointerDown += (data) => OnPickaxePointerDown?.Invoke(data);

            triggerSlot.OnPointerEnter += (data) => OnTriggerPointerEnter?.Invoke(data);
            triggerSlot.OnPointerExit += (data) => OnTriggerPointerExit?.Invoke(data);
            triggerSlot.OnPointerDown += (data) => OnTriggerPointerDown?.Invoke(data);

            if (triggerSlotConveyor != triggerSlot)
            {
                triggerSlotConveyor.OnPointerEnter += (data) => OnTriggerPointerEnter?.Invoke(data);
                triggerSlotConveyor.OnPointerExit += (data) => OnTriggerPointerExit?.Invoke(data);
                triggerSlotConveyor.OnPointerDown += (data) => OnTriggerPointerDown?.Invoke(data);
            }

            menuButton.onClick.AddListener(() => OnMenuButtonClick?.Invoke());
            speedUpButton.onClick.AddListener(() => OnSpeedUpButtonClick?.Invoke());
        }

        private void OnArtifactPointerEnterCallback(ArtifactItemUI item)
        {
            OnArtifactPointerEnter?.Invoke(artifactList.indexOf(item));
        }
        private void OnArtifactPointerExitCallback(ArtifactItemUI item)
        {
            OnArtifactPointerExit?.Invoke(artifactList.indexOf(item));
        }
        #endregion

        #region 事件
        public event Action<Receiver, PointerEventData> OnRaycastReceiverPointerDown;

        public event Action<PointerEventData> OnPickaxePointerEnter;
        public event Action<PointerEventData> OnPickaxePointerExit;
        public event Action<PointerEventData> OnPickaxePointerDown;

        public event Action<int> OnArtifactPointerEnter;
        public event Action<int> OnArtifactPointerExit;

        public event Action<PointerEventData> OnStarshardPointerDown;

        public event Action<PointerEventData> OnTriggerPointerEnter;
        public event Action<PointerEventData> OnTriggerPointerExit;
        public event Action<PointerEventData> OnTriggerPointerDown;
        public event Action OnStartGameCalled;
        public event Action OnMenuButtonClick;
        public event Action OnSpeedUpButtonClick;
        #endregion

        #region 属性字段

        public LevelUIBlueprints Blueprints => blueprints;
        public LevelUIBlueprintChoose BlueprintChoose => blueprintChoose;

        [SerializeField]
        Animator animator;
        [SerializeField]
        GraphicRaycaster[] raycasters;
        [SerializeField]
        CanvasGroup[] canvasGroups;

        [Header("Enabling")]
        [SerializeField]
        GameObject pickaxeSlotObj;
        [SerializeField]
        GameObject starshardPanelObj;
        [SerializeField]
        GameObject triggerSlotObj;
        [SerializeField]
        GameObject triggerSlotConveyorObj;

        [Header("Blueprints")]
        [SerializeField]
        LevelUIBlueprints blueprints;
        [SerializeField]
        LevelUIBlueprintChoose blueprintChoose;


        [Header("Tools")]
        [SerializeField]
        EnergyPanel energyPanel;
        [SerializeField]
        TriggerSlot triggerSlot;
        [SerializeField]
        TriggerSlot triggerSlotConveyor;
        [SerializeField]
        PickaxeSlot pickaxeSlot;

        [Header("Raycast Receivers")]
        [SerializeField]
        RaycastReceiver sideReceiver;
        [SerializeField]
        RaycastReceiver lawnReceiver;
        [SerializeField]
        RaycastReceiver bottomReceiver;

        [Header("CameraLimit")]
        [SerializeField]
        LayoutElement limitRegionLayoutElement;
        [SerializeField]
        float cameraLimitWidth = 220f;

        [Header("Artifacts")]
        [SerializeField]
        ElementList artifactList;

        [Header("Bottom")]
        [SerializeField]
        MoneyPanel moneyPanel;
        [SerializeField]
        StarshardPanel starshardPanel;
        [SerializeField]
        TextMeshProUGUI levelNameText;
        [SerializeField]
        GameObject progressBarRoot;
        [SerializeField]
        ProgressBar progressBar;
        [SerializeField]
        ProgressBar bossProgressBar;

        [Header("Right Top")]
        [SerializeField]
        GameObject topRightObj;
        [SerializeField]
        Button speedUpButton;
        [SerializeField]
        GameObject speedUpEnabledObject;
        [SerializeField]
        GameObject speedUpDisabledObject;
        [SerializeField]
        Button menuButton;
        [SerializeField]
        TextMeshProUGUI difficultyText;

        [Header("Advice")]
        [SerializeField]
        GameObject adviceObject;
        [SerializeField]
        TextMeshProUGUI adviceText;

        [Header("Tooltip")]
        [SerializeField]
        Tooltip tooltip;

        [Header("Hint Arrow")]
        [SerializeField]
        HintArrow hintArrow;
        [SerializeField]
        Vector2 hintArrowOffsetBlueprint;
        [SerializeField]
        Vector2 hintArrowOffsetPickaxe;
        [SerializeField]
        Vector2 hintArrowOffsetStarshard;
        [SerializeField]
        Vector2 hintArrowOffsetTrigger;
        [SerializeField]
        float hintArrowAngleBlueprint;
        [SerializeField]
        float hintArrowAnglePickaxe;
        [SerializeField]
        float hintArrowAngleStarshard;
        [SerializeField]
        float hintArrowAngleTrigger;
        #endregion

        #region 内嵌类
        public enum Receiver
        {
            Side,
            Lawn,
            Bottom
        }
        public enum VisibleState
        {
            Nothing = 0,
            ChoosingBlueprints = 1,
            InLevel = 2,
        }
        #endregion
    }
    [Serializable]
    public class SerializableLevelUIPreset
    {
        public SerializableAnimator animator;
        public SerializableAnimator[] artifactAnimators;
    }
}
