﻿using UnityEngine;

namespace MVZ2.Level
{
    public class AreaModelPreset : MonoBehaviour
    {
        public void SetActive(bool visible)
        {
            gameObject.SetActive(visible);
        }
        public string GetName()
        {
            return presetName;
        }
        [SerializeField]
        private string presetName = "default";
    }
}
