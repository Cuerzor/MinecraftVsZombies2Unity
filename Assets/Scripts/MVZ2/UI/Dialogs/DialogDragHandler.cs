﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVZ2.UI
{
    public class DialogDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!eventData.pointerCurrentRaycast.isValid)
                return;
            var rootCanvas = dragTarget.GetRootCanvasNonAlloc(canvasListCache);
            if (rootCanvas == null)
                return;
            dragging = true;
            var worldPos = rootCanvas.worldCamera.ScreenToWorldPoint(eventData.position);
            dragTargetOffset = worldPos - dragTarget.position;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;
            var rootCanvas = dragTarget.GetRootCanvasNonAlloc(canvasListCache);
            if (rootCanvas == null)
                return;

            var worldPos = rootCanvas.worldCamera.ScreenToWorldPoint(eventData.position);
            dragTarget.position = worldPos - dragTargetOffset;

            dragTarget.LimitInsideScreen(rootCanvas.transform);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }
        [SerializeField]
        private RectTransform dragTarget;
        private bool dragging;
        private Vector3 dragTargetOffset;
        private List<Canvas> canvasListCache = new List<Canvas>();
    }
}
