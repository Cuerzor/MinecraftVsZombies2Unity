﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace MVZ2.UI
{
    public class Deselector : MonoBehaviour
    {
        public void Deselect()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
