﻿using UnityEngine;

namespace MVZ2.Localization
{
    public abstract class TranslateComponentSprite<T> : TranslateComponent<T> where T : Component
    {
        protected virtual Sprite GetKeyInner() => null;
        public Sprite Key
        {
            get
            {
                if (key == null)
                    key = GetKeyInner();
                return key;
            }
        }
        private Sprite key;
    }
    public abstract class TranslateComponentSpriteMultiple<T> : TranslateComponent<T> where T : Component
    {
        protected virtual Sprite[] GetKeysInner() => null;
        public Sprite[] Keys
        {
            get
            {
                if (keys == null)
                    keys = GetKeysInner();
                return keys;
            }
        }
        private Sprite[] keys;
    }
}
