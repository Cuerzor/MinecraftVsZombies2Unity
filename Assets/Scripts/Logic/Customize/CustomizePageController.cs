#nullable enable

using System;
using UnityEngine;

namespace MVZ2Logic.Customize
{
    public abstract class CustomizePageController : MonoBehaviour
    {
        public abstract void Display(Camera camera);
        public abstract void Hide();
        public void Return()
        {
            Hide();
            OnReturn?.Invoke();
        }
        public event Action? OnReturn;
    }
}
