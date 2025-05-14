﻿using PVZEngine.Definitions;

namespace PVZEngine.Level
{
    [PropertyRegistryRegion(PropertyRegions.level)]
    public static class EngineLevelProps
    {
        public static readonly PropertyMeta START_ENERGY = new PropertyMeta("startEnergy");
        public static float GetStartEnergy(this LevelEngine level) => level.GetProperty<float>(START_ENERGY);
        public static void SetStartEnergy(this LevelEngine level, float value) => level.SetProperty(START_ENERGY, value);
        public static void SetStartEnergy(this StageDefinition stage, float value) => stage.SetProperty(START_ENERGY, value);
    }
}
