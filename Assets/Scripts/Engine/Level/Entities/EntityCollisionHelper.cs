using System.Collections.Generic;

namespace PVZEngine.Entities
{
    public static class EntityCollisionHelper
    {
        public const int DETECTION_DISCRETE = 0;
        public const int DETECTION_CONTINUOUS = 1;
        public const int DETECTION_IGNORE = 2;

        public const string NAME_MAIN = "main";
        public const string NAME_SHIELD = "shield";

        public const int MASK_PLANT = 1 << 0;
        public const int MASK_ENEMY = 1 << 1;
        public const int MASK_OBSTACLE = 1 << 2;
        public const int MASK_BOSS = 1 << 3;
        public const int MASK_CART = 1 << 4;
        public const int MASK_PICKUP = 1 << 5;
        public const int MASK_PROJECTILE = 1 << 6;
        public const int MASK_EFFECT = 1 << 7;
        public const int MASK_VULNERABLE = MASK_PLANT | MASK_ENEMY | MASK_OBSTACLE | MASK_BOSS;
        public const int MASK_ALL = MASK_VULNERABLE | MASK_CART | MASK_PICKUP | MASK_PROJECTILE | MASK_EFFECT;

        public const int STATE_ENTER = 0;
        public const int STATE_STAY = 1;
        public const int STATE_EXIT = 2;
        public static bool CanCollide(int collisionMask, Entity entity)
        {
            return (collisionMask & entity.TypeCollisionFlag) > 0;
        }
        public static int GetTypeMask(int type)
        {
            if (typeMaskDict.TryGetValue(type, out var mask))
                return mask;
            return 0;
        }
        private static Dictionary<int, int> typeMaskDict = new Dictionary<int, int>()
        {
            { EntityTypes.PLANT, MASK_PLANT },
            { EntityTypes.ENEMY, MASK_ENEMY },
            { EntityTypes.OBSTACLE, MASK_OBSTACLE },
            { EntityTypes.BOSS, MASK_BOSS },
            { EntityTypes.CART, MASK_CART },
            { EntityTypes.PICKUP, MASK_PICKUP },
            { EntityTypes.PROJECTILE, MASK_PROJECTILE },
            { EntityTypes.EFFECT, MASK_EFFECT },
        };
    }
}
