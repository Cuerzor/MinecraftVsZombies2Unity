using System;
using UnityEngine;
using UnityEngine.UI;

namespace MVZ2.Almanacs
{
    public class IndexAlmanacPage : AlmanacPage
    {
        protected override void Awake()
        {
            base.Awake();
            viewContraptionButton.onClick.AddListener(() => OnButtonClick?.Invoke(ButtonType.ViewContraption));
            viewEnemyButton.onClick.AddListener(() => OnButtonClick?.Invoke(ButtonType.ViewEnemy));
            viewArtifactButton.onClick.AddListener(() => OnButtonClick?.Invoke(ButtonType.ViewArtifact));
            viewMiscButton.onClick.AddListener(() => OnButtonClick?.Invoke(ButtonType.ViewMisc));
        }
        public void SetArtifactVisible(bool visible)
        {
            artifactRegion.SetActive(visible);
        }
        public event Action<ButtonType> OnButtonClick;
        [SerializeField]
        private GameObject artifactRegion;
        [SerializeField]
        private Button viewContraptionButton;
        [SerializeField]
        private Button viewEnemyButton;
        [SerializeField]
        private Button viewArtifactButton;
        [SerializeField]
        private Button viewMiscButton;
        public enum ButtonType
        {
            ViewContraption,
            ViewEnemy,
            ViewArtifact,
            ViewMisc
        }
    }
}
