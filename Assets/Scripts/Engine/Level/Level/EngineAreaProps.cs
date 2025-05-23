using PVZEngine.Level;

namespace PVZEngine.Definitions
{
    [PropertyRegistryRegion(PropertyRegions.level)]
    public static class EngineAreaProps
    {
        private static PropertyMeta Get(string name)
        {
            return new PropertyMeta(name);
        }
        public static readonly PropertyMeta GRID_WIDTH = Get("GridWidth");
        public static readonly PropertyMeta GRID_HEIGHT = Get("GridHeight");
        public static readonly PropertyMeta GRID_LEFT_X = Get("GridLeftX");
        public static readonly PropertyMeta GRID_BOTTOM_Z = Get("GridBottomZ");
        public static readonly PropertyMeta ENEMY_SPAWN_X = Get("enemySpawnX");
        public static readonly PropertyMeta ENTITY_LANE_Z_OFFSET = Get("EntityLawnZOffset");
        public static readonly PropertyMeta MAX_LANE_COUNT = Get("MaxLaneCount");
        public static readonly PropertyMeta MAX_COLUMN_COUNT = Get("MaxColumnCount");
        public static readonly PropertyMeta CART_REFERENCE = Get("cartReference");
        public static readonly PropertyMeta AREA_TAGS = Get("areaTags");
        public static NamespaceID[] GetAreaTags(this AreaDefinition definition)
        {
            return definition.GetProperty<NamespaceID[]>(AREA_TAGS);
        }
        public static NamespaceID[] GetAreaTags(this LevelEngine level)
        {
            return level.GetProperty<NamespaceID[]>(AREA_TAGS);
        }
        public static NamespaceID GetCartReference(this LevelEngine level) => level.GetProperty<NamespaceID>(CART_REFERENCE);
    }
}
