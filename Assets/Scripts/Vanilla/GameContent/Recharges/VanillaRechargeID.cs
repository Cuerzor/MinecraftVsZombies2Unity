﻿using MVZ2.Vanilla;
using PVZEngine;

namespace MVZ2.GameContent.Recharges
{
    public static class VanillaRechargeNames
    {
        public const string none = "none";
        public const string shortTime = "short";
        public const string longTime = "long";
        public const string veryLongTime = "very_long";
    }
    public static class VanillaRechargeID
    {
        public static readonly NamespaceID none = Get(VanillaRechargeNames.none);
        public static readonly NamespaceID shortTime = Get(VanillaRechargeNames.shortTime);
        public static readonly NamespaceID longTime = Get(VanillaRechargeNames.longTime);
        public static readonly NamespaceID veryLongTime = Get(VanillaRechargeNames.veryLongTime);
        private static NamespaceID Get(string name)
        {
            return new NamespaceID(VanillaMod.spaceName, name);
        }
    }
}
