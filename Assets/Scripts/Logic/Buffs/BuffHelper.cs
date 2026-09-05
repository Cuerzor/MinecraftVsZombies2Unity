#nullable enable

using PVZEngine;
using PVZEngine.Buffs;

namespace MVZ2Logic.Buffs
{
    public static class BuffHelper
    {
        public static void SetFirstBuffProperty<TBuff, T>(this IBuffTarget target, PropertyKey<T> key, T? value) where TBuff : BuffDefinition
        {
            var buff = target.GetFirstBuff<TBuff>();
            if (buff == null)
                return;
            buff.SetProperty<T>(key, value);
        }
        public static T? GetFirstBuffProperty<TBuff, T>(this IBuffTarget target, PropertyKey<T> key) where TBuff : BuffDefinition
        {
            var buff = target.GetFirstBuff<TBuff>();
            if (buff == null)
                return key.DefaultValue;
            return buff.GetProperty<T>(key);
        }
    }
}