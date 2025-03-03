﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVZ2.Titlescreen
{
    public class TitlescreenUI : MonoBehaviour
    {
        public void SetVersionText(string text)
        {
            versionText.text = text;
        }
        public void SetLoadingProgress(float value)
        {
            buttonFillImage.fillAmount = value;
        }
        public void SetLoadingText(string text)
        {
            buttonText.text = text;
        }
        public void SetButtonInteractable(bool interactable)
        {
            button.interactable = interactable;
        }
        private void Awake()
        {
            button.onClick.AddListener(OnButtonClickCallback);
        }
        private void OnButtonClickCallback()
        {
            OnButtonClick?.Invoke();
        }
        public event Action OnButtonClick;
        #region 属性字段
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image buttonFillImage;
        [SerializeField]
        private TextMeshProUGUI buttonText;
        [SerializeField]
        private TextMeshProUGUI versionText;
        #endregion
    }
}
