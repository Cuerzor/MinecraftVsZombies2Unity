﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVZ2.UI
{
    public class TextSlider : MonoBehaviour
    {
        public TextMeshProUGUI Text => text;
        public Slider Slider => slider;
        [SerializeField]
        private TextMeshProUGUI text;
        [SerializeField]
        private Slider slider;
    }
}
