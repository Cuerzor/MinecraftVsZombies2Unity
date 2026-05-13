#nullable enable

using PVZEngine;

namespace MVZ2Logic.Level
{
    public static class LogicTerrainTypes
    {
        public static readonly NamespaceID water = Get("water");
        public static readonly NamespaceID air = Get("air");
        public static readonly NamespaceID slab = Get("slab");
        private static NamespaceID Get(string name)
        {
            return new NamespaceID(Global.BuiltinNamespace, name);
        }
    }
}
