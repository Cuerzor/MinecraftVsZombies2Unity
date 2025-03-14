﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVZ2.UI
{
    public class RaycastReceiver : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            OnPointerDown?.Invoke(eventData);
        }
        public event Action<PointerEventData> OnPointerDown;
    }
}
