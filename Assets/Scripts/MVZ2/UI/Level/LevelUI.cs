using System;
using MVZ2.Level;
using MVZ2.Level.UI;
using MVZ2.Models;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace MVZ2.UI
{
    public class LevelUI : MonoBehaviour, ILevelUI
    {
        public void SetReceiveRaycasts(bool value)
        {
            GetUIPreset().SetReceiveRaycasts(value);
        }
        public void SetLighting(Color background, Color global)
        {
            backgroundLight.color = background * global;
            entityLight.color = global;
        }
        public void SetScreenCover(Color value)
        {
            blackscreenImage.color = value;
        }
        public void SetRaycastDisabled(bool disabled)
        {
            animator.SetBool("RaycastDisabled", disabled);
        }
        public void SetExitingToNote()
        {
            animator.SetTrigger("Exit");
        }
        public void CallExitLevelToNote()
        {
            OnExitLevelToNoteCalled?.Invoke();
        }
        public void ShowYouDied()
        {
            animator.SetTrigger("YouDied");
        }
        public void SetMobile(bool mobile)
        {
            isMobile = mobile;
            var uiPreset = GetUIPreset();
            standaloneUI.SetActive(standaloneUI == uiPreset);
            mobileUI.SetActive(mobileUI == uiPreset);
        }
        public LevelUIPreset GetUIPreset()
        {
            return isMobile ? mobileUI : standaloneUI;
        }
        public void SetUIDisabled(bool disabled)
        {
            var uiPreset = GetUIPreset();
            uiPreset.SetUIDisabled(disabled);
        }

        #region 蓝图
        public void SetBlueprintsSortingToChoosing(bool choosing)
        {
            var uiPreset = GetUIPreset();
            uiPreset.SetBlueprintsSortingToChoosing(choosing);
        }
        #endregion

        #region 手持物品
        public void SetHeldItemPosition(Vector2 worldPos)
        {
            heldItem.transform.position = worldPos;
        }
        public void SetHeldItemModel(Model prefab, Camera camera)
        {
            heldItem.SetModel(prefab, camera);
        }
        public void SetHeldItemTrigger(bool visible, bool trigger)
        {
            heldItem.SetTrigger(visible, trigger);
        }
        public Model GetHeldItemModel()
        {
            return heldItem.GetModel();
        }
        public void UpdateHeldItemModelFixed()
        {
            heldItem.UpdateModelFixed();
        }
        public void UpdateHeldItemModelFrame(float deltaTime)
        {
            heldItem.UpdateModelFrame(deltaTime);
        }
        public void SetHeldItemModelSimulationSpeed(float speed)
        {
            heldItem.SetModelSimulationSpeed(speed);
        }
        #endregion

        #region 暂停对话框
        public void SetPauseDialogActive(bool active)
        {
            pauseDialogObj.SetActive(active);
            ResetPauseDialogPosition();
        }
        public void ResetPauseDialogPosition()
        {
            var rectTrans = pauseDialogObj.transform as RectTransform;
            rectTrans.anchoredPosition = Vector3.zero;
        }
        public void SetPauseDialogImage(Sprite sprite)
        {
            pauseDialog.SetPausedImage(sprite);
        }
        #endregion

        #region 游戏结束对话框
        public void SetGameOverDialogActive(bool active)
        {
            gameOverDialogObj.SetActive(active);
        }
        public void SetGameOverDialogMessage(string text)
        {
            gameOverDialog.SetMessage(text);
        }
        public void SetGameOverDialogInteractable(bool interactable)
        {
            gameOverDialog.SetInteractable(interactable);
        }
        #endregion

        #region 菜单对话框
        public void SetOptionsDialogActive(bool visible)
        {
            optionsDialogObj.SetActive(visible);
            ResetOptionsDialogPosition();
        }
        public void ResetOptionsDialogPosition()
        {
            var rectTrans = optionsDialogObj.transform as RectTransform;
            rectTrans.anchoredPosition = Vector3.zero;
        }
        #endregion

        #region 加载关卡对话框
        public void SetLevelLoadedDialogVisible(bool visible)
        {
            levelLoadedDialogObj.SetActive(visible);
        }
        public void SetLevelErrorLoadingDialogVisible(bool visible)
        {
            levelErrorLoadingDialogObj.SetActive(visible);
        }
        public void SetLevelErrorLoadingDialogDesc(string text)
        {
            levelErrorLoadingDialog.SetDescription(text);
        }
        public void SetLevelErrorLoadingDialogInteractable(bool interactable)
        {
            levelErrorLoadingDialog.SetInteractable(interactable);
        }
        #endregion



        private void Awake()
        {
            pauseDialog.OnResumeClicked += () => OnPauseDialogResumeClicked?.Invoke();

            gameOverDialog.OnRetryButtonClicked += () => OnGameOverRetryButtonClicked?.Invoke();
            gameOverDialog.OnBackButtonClicked += () => OnGameOverBackButtonClicked?.Invoke();

            levelLoadedDialog.OnButtonClicked += (button) => OnLevelLoadedDialogButtonClicked?.Invoke(button);
            levelErrorLoadingDialog.OnButtonClicked += (restart) => OnLevelErrorLoadingDialogButtonClicked?.Invoke(restart);

            standaloneUI.OnStartGameCalled += () => OnStartGameCalled?.Invoke();
            mobileUI.OnStartGameCalled += () => OnStartGameCalled?.Invoke();
        }

        public event Action OnStartGameCalled;
        public event Action OnExitLevelToNoteCalled;
        public event Action OnPauseDialogResumeClicked;
        public event Action OnGameOverRetryButtonClicked;
        public event Action OnGameOverBackButtonClicked;
        public event Action<LevelLoadedDialog.ButtonType> OnLevelLoadedDialogButtonClicked;
        public event Action<bool> OnLevelErrorLoadingDialogButtonClicked;
        public OptionsDialog OptionsDialog => optionsDialog;


        public LevelUIBlueprintChoose BlueprintChoose => GetUIPreset().BlueprintChoose;
        public ILevelBlueprintRuntimeUI Blueprints => GetUIPreset().Blueprints;

        private bool isMobile;
        [SerializeField]
        Animator animator;

        [SerializeField]
        private LevelUIPreset standaloneUI;
        [SerializeField]
        private LevelUIPreset mobileUI;

        [Header("Shading")]
        [SerializeField]
        private Image blackscreenImage;
        [SerializeField]
        private Light2D backgroundLight;
        [SerializeField]
        private Light2D entityLight;

        [Header("HeldItem")]
        [SerializeField]
        HeldItem heldItem;

        [Header("Pause Dialog")]
        [SerializeField]
        GameObject pauseDialogObj;
        [SerializeField]
        PauseDialog pauseDialog;

        [Header("Game Over Dialog")]
        [SerializeField]
        GameObject gameOverDialogObj;
        [SerializeField]
        GameOverDialog gameOverDialog;

        [Header("Options Dialog")]
        [SerializeField]
        GameObject optionsDialogObj;
        [SerializeField]
        OptionsDialog optionsDialog;

        [Header("Level Loaded Dialog")]
        [SerializeField]
        GameObject levelLoadedDialogObj;
        [SerializeField]
        LevelLoadedDialog levelLoadedDialog;

        [Header("Level Error Loading Dialog")]
        [SerializeField]
        GameObject levelErrorLoadingDialogObj;
        [SerializeField]
        LevelErrorLoadingDialog levelErrorLoadingDialog;
    }
}
