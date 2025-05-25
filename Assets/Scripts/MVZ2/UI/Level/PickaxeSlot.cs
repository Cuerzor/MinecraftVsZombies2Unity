using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVZ2.Level.UI
{
    public class PickaxeSlot : LevelUIUnit, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler, ITooltipTarget
    {
        public void SetSelected(bool selected)
        {
            animator.SetBool("Selected", selected);
        }
        public void SetDisabled(bool selected)
        {
            animator.SetBool("Disabled", selected);
        }
        public void SetNumberText(PickaxeNumberText info)
        {
            numberText.gameObject.SetActive(info.show);
            numberText.text = info.text;
            numberText.color = info.color;
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            OnPointerDown?.Invoke(eventData);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnter?.Invoke(eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnPointerExit?.Invoke(eventData);
        }

        public event Action<PointerEventData> OnPointerEnter;
        public event Action<PointerEventData> OnPointerExit;
        public event Action<PointerEventData> OnPointerDown;
        TooltipAnchor ITooltipTarget.Anchor => tooltipAnchor;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private TooltipAnchor tooltipAnchor;
        [SerializeField]
        private TextMeshProUGUI numberText;
    }
    public struct PickaxeNumberText
    {
        public bool show;
        public string text;
        public Color color;
        public PickaxeNumberText(bool show, string text, Color color)
        {
            this.show = show;
            this.text = text;
            this.color = color;
        }
    }
}