﻿using System.Collections.Generic;
using System.Text;
using MukioI18n;
using UnityEngine;

namespace MVZ2.Localization
{
    public abstract class TranslateComponentText<T> : TranslateComponent<T>, ITranslateComponent where T : Component
    {
        protected virtual string GetKeyInner() => null;
        protected virtual IEnumerable<string> GetKeysInner() => null;
        public string Context => context;
        public string Comment => comment;
        public string Key
        {
            get
            {
                if (key == null)
                    key = GetKeyInner();
                return key;
            }
        }
        public IEnumerable<string> Keys
        {
            get
            {
                if (keys == null)
                    keys = GetKeysInner();
                return keys;
            }
        }
        public string Path
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                Transform tr = transform;
                do
                {
                    sb.Insert(0, tr.name);
                    sb.Insert(0, "/");
                } while (tr = tr.parent);

                return sb.ToString();
            }
        }
        [SerializeField]
        private string context;
        [SerializeField]
        private string comment;
        private string key;
        private IEnumerable<string> keys;
    }
}
