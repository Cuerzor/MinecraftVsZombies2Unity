﻿using System;
using PVZEngine;
using UnityEngine;

namespace MVZ2
{
    [Serializable]
    public class NamespaceIDReference
    {
        public NamespaceID Get()
        {
            if (cache == null || cache.SpaceName != spacename || cache.Path != path)
            {
                cache = new NamespaceID(spacename, path);
            }
            return cache;
        }
        [SerializeField]
        private string spacename;
        [SerializeField]
        private string path;
        private NamespaceID cache;
    }
}
