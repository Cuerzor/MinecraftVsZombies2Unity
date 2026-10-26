#nullable enable

using System;
using System.Collections.Generic;
using MVZ2Logic.Games;
using MVZ2Logic.Options;
using PVZEngine;

namespace MVZ2.Options
{
    public abstract class OptionContext : IOptionContext
    {
        public void CacheOptionBool(NamespaceID id, bool value)
        {
            cacheOptionBool[id] = value;
        }
        public void CacheOptionInt(NamespaceID id, int value)
        {
            cacheOptionInt[id] = value;
        }
        public void CacheOptionFloat(NamespaceID id, float value)
        {
            cacheOptionFloat[id] = value;
        }
        public void CacheOptionString(NamespaceID id, string value)
        {
            cacheOptionString[id] = value;
        }
        public void CacheOptionID(NamespaceID id, NamespaceID? value)
        {
            cacheOptionID[id] = value;
        }

        public bool TryGetCachedOptionBool(NamespaceID id, out bool value)
        {
            return cacheOptionBool.TryGetValue(id, out value);
        }
        public bool TryGetCachedOptionInt(NamespaceID id, out int value)
        {
            return cacheOptionInt.TryGetValue(id, out value);
        }
        public bool TryGetCachedOptionFloat(NamespaceID id, out float value)
        {
            return cacheOptionFloat.TryGetValue(id, out value);
        }
        public bool TryGetCachedOptionString(NamespaceID id, out string value)
        {
            return cacheOptionString.TryGetValue(id, out value);
        }
        public bool TryGetCachedOptionID(NamespaceID id, out NamespaceID? value)
        {
            return cacheOptionID.TryGetValue(id, out value);
        }
        public void FlushCachedOptions(IGlobalOptions options)
        {
            foreach (var pair in cacheOptionBool)
            {
                options.SetOptionBool(pair.Key, pair.Value);
            }
            foreach (var pair in cacheOptionInt)
            {
                options.SetOptionInt(pair.Key, pair.Value);
            }
            foreach (var pair in cacheOptionFloat)
            {
                options.SetOptionFloat(pair.Key, pair.Value);
            }
            foreach (var pair in cacheOptionString)
            {
                options.SetOptionString(pair.Key, pair.Value);
            }
            foreach (var pair in cacheOptionID)
            {
                options.SetOptionID(pair.Key, pair.Value);
            }
            cacheOptionBool.Clear();
            cacheOptionInt.Clear();
            cacheOptionFloat.Clear();
            cacheOptionString.Clear();
            cacheOptionID.Clear();
        }
        public void SetNeedReload()
        {
            needReload = true;
        }
        public bool NeedsReload()
        {
            return needReload;
        }
        public void UpdateDialog()
        {
            OnDialogUpdateRequested?.Invoke(this);
        }
        public event Action<IOptionContext>? OnDialogUpdateRequested;
        private Dictionary<NamespaceID, bool> cacheOptionBool = new Dictionary<NamespaceID, bool>();
        private Dictionary<NamespaceID, int> cacheOptionInt = new Dictionary<NamespaceID, int>();
        private Dictionary<NamespaceID, float> cacheOptionFloat = new Dictionary<NamespaceID, float>();
        private Dictionary<NamespaceID, string> cacheOptionString = new Dictionary<NamespaceID, string>();
        private Dictionary<NamespaceID, NamespaceID?> cacheOptionID = new Dictionary<NamespaceID, NamespaceID?>();
        public bool needReload;
    }
}