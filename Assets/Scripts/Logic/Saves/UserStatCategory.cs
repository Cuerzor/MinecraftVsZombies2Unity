﻿using System;
using System.Collections.Generic;
using System.Linq;
using PVZEngine;

namespace MVZ2Logic.Saves
{
    public class UserStatCategory
    {
        public UserStatCategory(string name)
        {
            Name = name;
        }
        public long GetStatValue(NamespaceID id)
        {
            var entry = GetEntry(id);
            if (entry == null)
                return 0;
            return entry.Value;
        }
        public void SetStatValue(NamespaceID id, long value)
        {
            var entry = GetEntry(id);
            if (entry == null)
                entry = CreateEntry(id);
            entry.Value = value;
        }
        public bool HasStat(NamespaceID id)
        {
            return entries.Exists(e => e.ID == id);
        }
        public UserStatEntry[] GetAllEntries()
        {
            return entries.ToArray();
        }
        public long GetSum()
        {
            if (entries.Count <= 0)
                return 0;
            return entries.Sum(e => e.Value);
        }
        public long GetMax()
        {
            if (entries.Count <= 0)
                return 0;
            return entries.Max(e => e.Value);
        }
        public SerializableUserStatCategory ToSerializable()
        {
            return new SerializableUserStatCategory()
            {
                name = Name,
                entries = entries.Select(e => e.ToSerializable()).ToArray()
            };
        }
        public static UserStatCategory FromSerializable(SerializableUserStatCategory serializable)
        {
            var stats = new UserStatCategory(serializable.name);
            stats.entries.AddRange(serializable.entries.Select(e => UserStatEntry.FromSerializable(e)));
            return stats;
        }
        private NamespaceID[] GetAllEntriesID()
        {
            return entries.Select(e => e.ID).ToArray();
        }
        private UserStatEntry GetEntry(NamespaceID id)
        {
            return entries.FirstOrDefault(e => e.ID == id);
        }
        private UserStatEntry CreateEntry(NamespaceID id)
        {
            var entry = new UserStatEntry(id);
            entries.Add(entry);
            return entry;
        }
        public string Name { get; }
        private List<UserStatEntry> entries = new List<UserStatEntry>();
    }
    [Serializable]
    public class SerializableUserStatCategory
    {
        public string name;
        public SerializableUserStatEntry[] entries;
    }
}
