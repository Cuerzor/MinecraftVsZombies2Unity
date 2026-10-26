#nullable enable

using System;
using System.Collections.Generic;
using MukioI18n;
using MVZ2.Cameras;
using MVZ2.Managers;
using MVZ2.UI;
using MVZ2Logic;
using MVZ2Logic.Audios;
using MVZ2Logic.Commands;
using MVZ2Logic.Games;
using MVZ2Logic.Level;
using MVZ2Logic.Localization;
using MVZ2Logic.Options;
using MVZ2Logic.Saves;
using PVZEngine;
using PVZEngine.Tools.Unity;
using UnityEngine;
using static MVZ2.UI.OptionsDialogMainPage;

namespace MVZ2.Options
{
    public class OptionsDialogController : MonoBehaviour
    {
        public void Open(IOptionContext context)
        {
            this.context = context;
            context.OnDialogUpdateRequested += (c) =>
            {
                if (IsOpen())
                {
                    UpdateMoreOptionsPageWidgets();
                }
            };

            UpdateMainPageWidgets();
            UpdateMoreOptionsPageWidgets();
        }
        public bool IsOpen()
        {
            return context != null;
        }
        public void Close()
        {
            if (context == null)
                return;
            var needsReload = context.NeedsReload();
            context.FlushCachedOptions(Main.OptionsManager);
            context = null;
            SaveOptions();
            OnClose?.Invoke(needsReload);
        }
        private void Awake()
        {
            ui.Main.OnToggleValueChanged += OnMainPageToggleValueChangedCallback;
            ui.Main.OnSliderValueChanged += OnMainPageSliderValueChangedCallback;
            ui.Main.OnSliderEnd += OnMainPageSliderEndCallback;
            ui.Main.OnButtonClick += OnMainPageButtonClickCallback;
            ui.Main.OnTooltipShow += OnTooltipShowCallback;
            ui.Main.OnTooltipHide += OnTooltipHideCallback;

            ui.MoreOptions.OnBackClick += OnMoreOptionsBackClickCallback;

            ui.MoreOptions.OnToggleValueChanged += OnMoreOptionsToggleValueChangedCallback;
            ui.MoreOptions.OnSliderValueChanged += OnMoreOptionsSliderValueChangedCallback;
            ui.MoreOptions.OnSliderEnd += OnMoreOptionsSliderEndCallback;
            ui.MoreOptions.OnDropdownValueChanged += OnMoreOptionsDropdownValueChangedCallback;
            ui.MoreOptions.OnButtonClick += OnMoreOptionsButtonClickCallback;
            ui.MoreOptions.OnTooltipShow += OnTooltipShowCallback;
            ui.MoreOptions.OnTooltipHide += OnTooltipHideCallback;

            Main.SponsorManager.OnSponsorUpdateFinished += OnSponsorUpdateFinishedCallback;
        }
        private void OnEnable()
        {
            ResolutionManager.OnResolutionChanged += OnResolutionChangedCallback;
        }
        private void OnDisable()
        {
            ResolutionManager.OnResolutionChanged -= OnResolutionChangedCallback;
        }

        #region 事件回调
        private void OnResolutionChangedCallback(int width, int height)
        {
            RefreshResolutionDropdown();
        }
        private void RefreshResolutionDropdown()
        {
            if (context == null)
                return;
            var id = LogicOptionWidgetID.resolution;
            var definition = Main.Game.GetOptionWidgetDefinition(id);
            if (definition is not OptionDropdownDefinition dropdown)
                return;
            var widgets = ui.MoreOptions.GetWidgetsByID(id);
            if (!widgets.Exists())
                return;
            var items = new List<string>();
            dropdown.FillItems(context, items);
            widgets.SetDropdownOptions(items);

            var value = dropdown.GetValue(context);
            widgets.SetDropdownValue(value);
        }

        private void OnMainPageToggleValueChangedCallback(ToggleType type, bool value)
        {
            switch (type)
            {
                case ToggleType.SwapTrigger:
                    {
                        Main.OptionsManager.SetSwapTrigger(value);
                        UpdateSwapTriggerToggle(value);
                        SaveOptions();
                    }
                    break;
                case ToggleType.PauseOnFocusLost:
                    {
                        Main.OptionsManager.SetPauseOnFocusLost(value);
                        UpdatePauseOnFocusLostToggle(value);
                        SaveOptions();
                    }
                    break;
            }
        }
        private void OnMainPageSliderValueChangedCallback(SliderType type, float value)
        {
            switch (type)
            {
                case SliderType.Music:
                    {
                        Main.OptionsManager.SetMusicVolume(value);
                        UpdateMusicSlider(value);
                    }
                    break;
                case SliderType.Sound:
                    {
                        Main.OptionsManager.SetSoundVolume(value);
                        UpdateSoundSlider(value);
                    }
                    break;
                case SliderType.FastForward:
                    {
                        var multi = ValueToFastForwardMultiplier(GetFastwoardMultiplierStart(), value);
                        Main.OptionsManager.SetFastForwardMultiplier(multi);
                        UpdateFastforwardSlider(multi);
                    }
                    break;
            }
        }
        private void OnMainPageSliderEndCallback(SliderType type, float value)
        {
            switch (type)
            {
                case SliderType.Music:
                case SliderType.FastForward:
                    SaveOptions();
                    break;
                case SliderType.Sound:
                    Main.SoundManager.Play2D(LogicSoundID.click);
                    SaveOptions();
                    break;
            }
        }
        private async void OnMainPageButtonClickCallback(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Back:
                    Close();
                    break;
                case ButtonType.Difficulty:
                    {
                        Main.OptionsManager.CycleDifficulty();
                        UpdateDifficultyButton(Main.OptionsManager.GetDifficulty());
                        SaveOptions();
                        var level = Main.LevelManager.GetLevelController();
                        if (level.Exists())
                        {
                            level.UpdateDifficulty();
                        }
                    }
                    break;
                case ButtonType.MoreOptions:
                    ui.SetPage(OptionsDialog.Page.More);
                    break;

                case ButtonType.LeaveLevel:
                    {
                        var level = Main.LevelManager.GetLevelController();
                        if (!level.Exists())
                        {
                            break;
                        }
                        if (level.IsGameStarted() || level.GetCurrentFlag() > 0)
                        {
                            var title = Main.LanguageManager._(LogicStrings.BACK);
                            var desc = Main.LanguageManager._(DIALOG_DESC_LEAVE_LEVEL);

                            var result = await Main.Scene.ShowDialogSelectAsync(title, desc);
                            if (!result)
                                break;
                        }
                        if (level.IsGameStarted())
                        {
                            Main.LevelManager.SaveLevel();
                        }
                        await level.ExitLevel();
                    }
                    break;
                case ButtonType.Restart:
                    {
                        var level = Main.LevelManager.GetLevelController();
                        if (!level.Exists())
                            break;
                        level.ShowRestartConfirmDialog(LevelRestartStatus.InLevel);
                    }
                    break;
            }
        }
        private void OnMoreOptionsBackClickCallback()
        {
            ui.SetPage(OptionsDialog.Page.Main);
        }

        private void OnMoreOptionsToggleValueChangedCallback(OptionWidgets widgets, bool value)
        {
            if (context == null)
                return;
            var id = widgets.GetOptionID();
            var definition = Main.Game.GetOptionWidgetDefinition(id);
            if (definition is not OptionToggleDefinition toggle)
                return;

            toggle.OnValueChanged(context, value);
        }
        private void OnMoreOptionsSliderValueChangedCallback(OptionWidgets widgets, float value)
        {
            if (context == null)
                return;
            var id = widgets.GetOptionID();
            var definition = Main.Game.GetOptionWidgetDefinition(id);
            if (definition is not OptionSliderDefinition slider)
                return;

            slider.OnValueChanged(context, value);

            var label = slider.GetLabelText(context);
            widgets.SetSliderText(label);
        }
        private void OnMoreOptionsSliderEndCallback(OptionWidgets widgets, float value)
        {
            if (context == null)
                return;
            var id = widgets.GetOptionID();
            var definition = Main.Game.GetOptionWidgetDefinition(id);
            if (definition is not OptionSliderDefinition slider)
                return;

            slider.OnEndEdit(context, value);
        }
        private void OnMoreOptionsDropdownValueChangedCallback(OptionWidgets widgets, int value)
        {
            if (context == null)
                return;
            var id = widgets.GetOptionID();
            var definition = Main.Game.GetOptionWidgetDefinition(id);
            if (definition is not OptionDropdownDefinition dropdown)
                return;

            dropdown.OnValueChanged(context, value);
        }
        private void OnMoreOptionsButtonClickCallback(OptionWidgets widgets)
        {
            if (context == null)
                return;
            var id = widgets.GetOptionID();
            var definition = Main.Game.GetOptionWidgetDefinition(id);
            if (definition is not OptionButtonDefinition button)
                return;

            button.OnClick(context);

            widgets.SetButtonText(button.GetLabelText(context));
        }
        private void OnTooltipShowCallback(TooltipHandler handler)
        {
            if (string.IsNullOrEmpty(handler.text))
                return;
            string text;
            if (string.IsNullOrEmpty(handler.context))
            {
                text = Main.LanguageManager._(handler.text);
            }
            else
            {
                text = Main.LanguageManager._p(handler.context, handler.text);
            }
            var camera = ui.GetCamera();
            if (!camera.Exists())
                return;
            Main.Scene.ShowTooltip(new SimpleTooltipSource(camera, handler, new TooltipContent() { description = text }));
        }
        private void OnTooltipHideCallback(TooltipHandler handler)
        {
            Main.Scene.HideTooltip();
        }
        private void OnSponsorUpdateFinishedCallback(bool success)
        {
            if (IsOpen())
            {
                UpdateMoreOptionsPageWidgets();
            }
        }
        #endregion
        private void SaveOptions()
        {
            Main.OptionsManager.SaveOptions();
        }
        #region 主界面
        private void UpdateMainPageWidgets()
        {
            // Main
            UpdateMusicSlider(Main.OptionsManager.GetMusicVolume());
            UpdateSoundSlider(Main.OptionsManager.GetSoundVolume());
            UpdateFastforwardSlider(Main.OptionsManager.GetFastForwardMultiplier());

            UpdateSwapTriggerToggle(Main.OptionsManager.IsTriggerSwapped());
            UpdatePauseOnFocusLostToggle(Main.OptionsManager.GetPauseOnFocusLost());

            UpdateMoreOptionsButton();

            UpdateDifficultyButton(Main.OptionsManager.GetDifficulty());
            UpdateRestartButton();

            UpdateLeaveLevelButton();

            UpdateBackButton();

            switch (context)
            {
                case IOptionContextMap mapContext:
                    ui.Main.SetButtonActive(ButtonType.Restart, false);
                    ui.Main.SetButtonActive(ButtonType.LeaveLevel, false);
                    break;
                case IOptionContextLevel levelContext:
                    {
                        var level = Main.LevelManager.GetLevelController();
                        bool isInLevel = level.Exists() && (level.IsGameStarted() || level.GetCurrentFlag() > 0);
                        ui.Main.SetButtonActive(ButtonType.Difficulty, !isInLevel);
                        ui.Main.SetButtonActive(ButtonType.Restart, isInLevel);
                        ui.Main.SetButtonActive(ButtonType.LeaveLevel, true);
                    }
                    break;
                default:
                    {
                        ui.Main.SetButtonActive(ButtonType.Restart, false);
                        ui.Main.SetButtonActive(ButtonType.LeaveLevel, false);
                    }
                    break;
            }


            var mainPage = ui.Main;
            bool mobile = Main.IsMobile();
            mainPage.SetToggleActive(ToggleType.SwapTrigger, Main.SaveManager.IsTriggerUnlocked());
        }
        protected void UpdateMusicSlider(float value)
        {
            UpdateMainPageSliderValue(value, OPTION_MUSIC, SliderType.Music);
        }
        protected void UpdateSoundSlider(float value)
        {
            UpdateMainPageSliderValue(value, OPTION_SOUND, SliderType.Sound);
        }
        protected void UpdateFastforwardSlider(float multi)
        {
            var sliderMultiplierEnd = FASTFORWARD_MULTIPLIER_END;
            var sliderMultiplierStart = GetFastwoardMultiplierStart();
            var sliderMultiplierRange = sliderMultiplierEnd - sliderMultiplierStart;

            float value = FastForwardMultiplierToValue(sliderMultiplierStart, multi);

            var valueText = LogicMain.GetFloatPercentageText(multi);
            var text = Main.LanguageManager._(OPTION_FASTFORWARD_MULTIPLIER, valueText);

            var sliderStart = 0;
            var sliderEnd = Mathf.RoundToInt(sliderMultiplierRange / FASTFORWARD_STEP);
            ui.Main.SetSliderRange(SliderType.FastForward, sliderStart, sliderEnd, true);
            ui.Main.SetSliderValue(SliderType.FastForward, value);
            ui.Main.SetSliderText(SliderType.FastForward, text);
        }
        protected float ValueToFastForwardMultiplier(float startMultiplier, float value)
        {
            return startMultiplier + FASTFORWARD_STEP * value;
        }
        protected float FastForwardMultiplierToValue(float startMultiplier, float multi)
        {
            return Mathf.RoundToInt((multi - startMultiplier) / FASTFORWARD_STEP);
        }
        private float GetFastwoardMultiplierStart()
        {
            return FASTFORWARD_MULTIPLIER_START;
        }
        private float ValueToAnimationFrequency(float value)
        {
            return value;
        }
        protected float AnimationFrequencyToValue(float frequency)
        {
            return frequency;
        }
        protected void UpdateSwapTriggerToggle(bool value)
        {
            UpdateMainPageToggle(value, OPTION_SWAP_TRIGGER, ToggleType.SwapTrigger);
        }
        private void UpdatePauseOnFocusLostToggle(bool value)
        {
            UpdateMainPageToggle(value, OPTION_PAUSE_ON_FOCUS_LOST, ToggleType.PauseOnFocusLost);
        }
        protected void UpdateMoreOptionsButton()
        {
            var text = Main.LanguageManager._(OPTION_MORE_OPTIONS);
            ui.Main.SetButtonText(TextButtonType.MoreOptions, text);
        }
        protected void UpdateDifficultyButton(NamespaceID value)
        {
            var valueText = GetDifficultyText(value);
            var text = Main.LanguageManager._(OPTION_DIFFICULTY, valueText);
            ui.Main.SetButtonText(TextButtonType.Difficulty, text);
        }
        protected void UpdateRestartButton()
        {
            var text = Main.LanguageManager._(OPTION_RESTART);
            ui.Main.SetButtonText(TextButtonType.Restart, text);
        }
        protected void UpdateLeaveLevelButton()
        {
            var textKey = Main.SaveManager.IsPrologueCleared() ? LogicStrings.BACK_TO_MAP : LogicStrings.BACK_TO_MAINMENU;
            var text = Main.LanguageManager._(textKey);
            ui.Main.SetButtonText(TextButtonType.LeaveLevel, text);
        }
        protected void UpdateBackButton()
        {
            var text = Main.LanguageManager._(OPTION_BACK);
            ui.Main.SetButtonText(TextButtonType.Back, text);
        }
        #endregion

        #region 更多选项
        private void UpdateMoreOptionsPageWidgets()
        {
            if (context == null)
                return;
            OptionWidgetDefinition[] definitions = Main.Game.GetAllOptionWidgetDefinitions();
            Array.Sort<OptionWidgetDefinition>(definitions, (d1, d2) => d1.GetOrder().CompareTo(d2.GetOrder()));
            Dictionary<NamespaceID, List<OptionWidgetsViewData>> categories = new Dictionary<NamespaceID, List<OptionWidgetsViewData>>();
            foreach (var definition in definitions)
            {
                if (!definition.ShouldEnable(context))
                    continue;
                var categoryID = definition.GetCategoryID();
                if (!NamespaceID.IsValid(categoryID))
                    continue;
                if (!categories.TryGetValue(categoryID, out var list))
                {
                    list = new List<OptionWidgetsViewData>();
                    categories.Add(categoryID, list);
                }
                var viewData = GetWidgetViewData(definition);
                list.Add(viewData);
            }
            List<MoreOptionsCategoryViewData> categoryViewDatas = new List<MoreOptionsCategoryViewData>();

            foreach (var pair in categories)
            {
                var categoryMeta = Main.ResourceManager.GetOptionCategoryMeta(pair.Key);
                if (categoryMeta == null)
                    continue;
                var label = Main.LanguageManager._p(LogicStrings.CONTEXT_OPTION_CATEGORY, categoryMeta.Label);
                var viewData = new MoreOptionsCategoryViewData()
                {
                    label = label,
                    widgets = pair.Value.ToArray()
                };
                categoryViewDatas.Add(viewData);
            }
            var moreOptionsViewData = new MoreOptionsViewData()
            {
                categories = categoryViewDatas.ToArray(),
            };
            ui.MoreOptions.UpdateOptions(moreOptionsViewData);
        }
        private OptionWidgetsViewData GetWidgetViewData(OptionWidgetDefinition definition)
        {
            if (context == null)
                return new OptionWidgetsViewData();
            string tooltipText = definition.GetTooltip() ?? string.Empty;
            string tooltipContext = LogicStrings.CONTEXT_OPTION_TOOLTIP;
            switch (definition)
            {
                case OptionToggleDefinition toggle:
                    return new OptionWidgetsViewData()
                    {
                        label = toggle.GetLabelText(context),
                        namespaceID = definition.GetID(),
                        tooltipText = tooltipText,
                        tooltipContext = tooltipContext,
                        type = OptionWidgetType.Toggle,
                        value = toggle.GetValue(context),
                        interactable = toggle.IsInteractable(context),
                    };
                case OptionSliderDefinition slider:
                    {
                        var value = slider.GetValue(context);
                        return new OptionWidgetsViewData()
                        {
                            label = slider.GetLabelText(context),
                            namespaceID = definition.GetID(),
                            tooltipText = tooltipText,
                            tooltipContext = tooltipContext,
                            sliderWholeNumbers = definition.IsSliderWholeNumbers(),
                            sliderMinValue = definition.GetSliderMinValue(),
                            sliderMaxValue = definition.GetSliderMaxValue(),
                            type = OptionWidgetType.Slider,
                            value = value,
                            interactable = slider.IsInteractable(context),
                        };
                    }
                case OptionDropdownDefinition dropdown:
                    {
                        var value = dropdown.GetValue(context);
                        var items = new List<string>();
                        dropdown.FillItems(context, items);
                        return new OptionWidgetsViewData()
                        {
                            label = dropdown.GetLabelText(context),
                            namespaceID = definition.GetID(),
                            tooltipText = tooltipText,
                            tooltipContext = tooltipContext,
                            type = OptionWidgetType.Dropdown,
                            value = value,
                            dropdownOptions = items,
                            interactable = dropdown.IsInteractable(context),
                        };
                    }
                case OptionButtonDefinition button:
                    {
                        return new OptionWidgetsViewData()
                        {
                            label = button.GetLabelText(context),
                            namespaceID = definition.GetID(),
                            tooltipText = tooltipText,
                            tooltipContext = tooltipContext,
                            type = OptionWidgetType.Button,
                            interactable = button.IsInteractable(context),
                        };
                    }
                case OptionLabelDefinition label:
                    {
                        return new OptionWidgetsViewData()
                        {
                            label = label.GetLabelText(context),
                            namespaceID = definition.GetID(),
                            tooltipText = tooltipText,
                            tooltipContext = tooltipContext,
                            type = OptionWidgetType.Label,
                        };
                    }
            }
            throw new NotImplementedException();
        }
        #endregion

        #region 组件更新
        protected string GetValueText(bool value)
        {
            return Main.LanguageManager._(value ? LogicStrings.YES : LogicStrings.NO);
        }
        protected string GetDifficultyText(NamespaceID id)
        {
            return Main.Game.GetDifficultyName(id);
        }
        protected void UpdateMainPageSliderValue(float value, string optionKey, SliderType sliderType)
        {
            var valueText = LogicMain.GetFloatPercentageText(value);
            var text = Main.LanguageManager._(optionKey, valueText);
            ui.Main.SetSliderValue(sliderType, value);
            ui.Main.SetSliderText(sliderType, text);
        }
        protected void UpdateMainPageButtonText(bool value, string optionKey, TextButtonType buttonType)
        {
            var valueText = GetValueText(value);
            var text = Main.LanguageManager._(optionKey, valueText);
            ui.Main.SetButtonText(buttonType, text);
        }
        protected void UpdateMainPageToggle(bool value, string optionKey, ToggleType toggleType)
        {
            var text = Main.LanguageManager._(optionKey);
            ui.Main.SetToggleText(toggleType, text);
            ui.Main.SetToggleOn(toggleType, value);
        }
        #endregion

        #region 常量
        public const float FASTFORWARD_STEP = 0.05f;
        public const float FASTFORWARD_MULTIPLIER_START = 1.1f;
        public const float FASTFORWARD_MULTIPLIER_END = 3;
        #endregion

        #region 翻译文本
        [TranslateMsg("选项")]
        public const string OPTION_SWAP_TRIGGER = "交换触发";
        [TranslateMsg("选项")]
        public const string OPTION_FULLSCREEN = "全屏";
        [TranslateMsg("选项")]
        public const string OPTION_VIBRATION = "设备震动";
        [TranslateMsg("选项")]
        public const string OPTION_PAUSE_ON_FOCUS_LOST = "后台暂停";
        [TranslateMsg("选项")]
        public const string OPTION_MORE_OPTIONS = "更多选项";
        [TranslateMsg("选项，{0}为难度")]
        public const string OPTION_DIFFICULTY = "难度：{0}";
        [TranslateMsg("选项")]
        public const string OPTION_RESTART = "重新开始";
        [TranslateMsg("选项")]
        public const string OPTION_BACK = "返回";


        [TranslateMsg("选项，{0}为量")]
        public const string OPTION_MUSIC = "音乐音量：{0}";
        [TranslateMsg("选项，{0}为量")]
        public const string OPTION_SOUND = "音效音量：{0}";
        [TranslateMsg("选项，{0}为量")]
        public const string OPTION_FASTFORWARD_MULTIPLIER = "加速倍率：{0}";
        [TranslateMsg("对话框内容")]
        public const string DIALOG_DESC_LEAVE_LEVEL = "确认要返回吗？\n你的进度会被保存。";
        #endregion

        public event Action<bool>? OnClose;
        protected MainManager Main => MainManager.Instance;
        private IOptionContext? context;
        [SerializeField]
        private OptionsDialog ui = null!;
    }
}