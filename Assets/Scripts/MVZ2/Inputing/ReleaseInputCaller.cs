#nullable enable

using System.Collections.Generic;
using System.Linq;
using MVZ2.Level.Inputing;
using MVZ2Logic.Inputs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVZ2.Inputs
{
    public class ReleaseInputCaller : MonoBehaviour
    {
        private void Update()
        {
            UpdatePointerRelease();
        }
        #region Ö¸ÕëÊäÈë
        private void UpdatePointerRelease()
        {
            if (Input.touchCount > 0)
            {
                foreach (var position in InputHelper.GetTouchUps())
                {
                    OnPointerRelease(position);
                }
            }
            else
            {
                foreach (var position in InputHelper.GetMouseUps(MouseButtons.LEFT))
                {
                    OnPointerRelease(position);
                }
            }
        }
        private void OnPointerRelease(PointerPositionParams pointer)
        {
            var eventSystem = EventSystem.current;
            var results = new List<RaycastResult>();
            var pointerId = InputHelper.GetPointerIdByButtonAndType(pointer.button, pointer.type);
            var eventData = new PointerEventData(eventSystem)
            {
                position = pointer.position,
                button = (PointerEventData.InputButton)pointer.button,
                pointerId = pointerId,
            };
            eventSystem.RaycastAll(eventData, results);
            var first = results.FirstOrDefault(r => r.gameObject);
            if (first.isValid)
            {
                eventData.pointerCurrentRaycast = first;
                ExecuteEvents.ExecuteHierarchy<IPointerReleaseHandler>(first.gameObject, eventData, (x, y) => x.OnPointerRelease(ExecuteEvents.ValidateEventData<PointerEventData>(y)));
            }
        }
        #endregion
    }
}