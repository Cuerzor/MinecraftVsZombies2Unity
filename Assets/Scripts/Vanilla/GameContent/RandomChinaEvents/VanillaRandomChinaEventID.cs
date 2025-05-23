using MVZ2.Vanilla;
using PVZEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    public static class VanillaRandomChinaEventNames
    {
        public const string obsidianPrison = "obsidian_prison";
        public const string chinaTown = "china_town";
        public const string theTower = "the_tower";
        public const string redstoneReady = "redstone_ready";
        public const string aceOfDiamonds = "ace_of_diamonds";
        public const string hellMetal = "hell_metal";

        public const string wrathOfTheSmall = "wrath_of_the_small";
        public const string worldwideCelebration = "worldwide_celebration";
        public const string superRecharge = "super_recharge";
        public const string raceCars = "race_cars";
        public const string anvilShower = "anvil_shower";
        public const string theHangedMan = "the_hanged_man";
    }
    public static class VanillaRandomChinaEventID
    {
        public static readonly NamespaceID obsidianPrison = Get(VanillaRandomChinaEventNames.obsidianPrison);
        public static readonly NamespaceID chinaTown = Get(VanillaRandomChinaEventNames.chinaTown);
        public static readonly NamespaceID theTower = Get(VanillaRandomChinaEventNames.theTower);
        public static readonly NamespaceID redstoneReady = Get(VanillaRandomChinaEventNames.redstoneReady);
        public static readonly NamespaceID aceOfDiamonds = Get(VanillaRandomChinaEventNames.aceOfDiamonds);
        public static readonly NamespaceID hellMetal = Get(VanillaRandomChinaEventNames.hellMetal);
        private static NamespaceID Get(string name)
        {
            return new NamespaceID(VanillaMod.spaceName, name);
        }
    }
}
