using MVZ2Logic;
using PVZEngine;
using PVZEngine.Definitions;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.Vanilla.Level
{
    [PropertyRegistryRegion]
    public static class VanillaAreaProps
    {
        private static PropertyMeta Get(string name)
        {
            return new PropertyMeta(name);
        }
        public static readonly PropertyMeta MODEL_ID = Get("modelID");
        public static NamespaceID GetModelID(this LevelEngine game)
        {
            return game.GetProperty<NamespaceID>(MODEL_ID);
        }
        public static NamespaceID GetModelID(this AreaDefinition definition)
        {
            return definition.GetProperty<NamespaceID>(MODEL_ID);
        }
        public static readonly PropertyMeta STARSHARD_ICON = Get("starshardIcon");
        public static SpriteReference GetStarshardIcon(this LevelEngine game)
        {
            return game.GetProperty<SpriteReference>(STARSHARD_ICON);
        }
        public static readonly PropertyMeta WATER_COLOR = Get("waterColor");
        public static readonly PropertyMeta WATER_COLOR_CENSORED = Get("waterColorCensored");
        public static Color GetWaterColorNormal(this LevelEngine game)
        {
            return game.GetProperty<Color>(WATER_COLOR);
        }
        public static Color GetWaterColorCensored(this LevelEngine game)
        {
            return game.GetProperty<Color>(WATER_COLOR_CENSORED);
        }
        public static Color GetWaterColor(this LevelEngine game)
        {
            return Global.HasBloodAndGore() ? game.GetWaterColorNormal() : game.GetWaterColorCensored();
        }

        public static readonly PropertyMeta DOOR_Z = Get("doorZ");
        public static readonly PropertyMeta BACKGROUND_LIGHT = Get("backgroundLight");
        public static readonly PropertyMeta GLOBAL_LIGHT = Get("globalLight");

        public static float GetDoorZ(this LevelEngine game)
        {
            return game.GetProperty<float>(DOOR_Z);
        }
        public static Color GetBackgroundLight(this LevelEngine level)
        {
            return level.GetProperty<Color>(BACKGROUND_LIGHT);
        }
        public static Color GetGlobalLight(this LevelEngine level)
        {
            return level.GetProperty<Color>(GLOBAL_LIGHT);
        }
    }
}
