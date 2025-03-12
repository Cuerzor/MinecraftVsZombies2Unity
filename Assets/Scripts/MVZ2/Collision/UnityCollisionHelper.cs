using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.Collision
{
    public static class UnityCollisionHelper
    {
        public static int ToEntityLayerMask(int objLayer)
        {
            if (object2EntityLayerMap.TryGetValue(objLayer, out var entityLayer))
                return entityLayer;
            return 0;
        }
        public static int ToObjectLayerMask(int entityLayer)
        {
            var objLayer = 0;
            foreach (var pair in entity2ObjectLayerMap)
            {
                if ((entityLayer & pair.Key) == 0)
                    continue;
                objLayer |= (1 << pair.Value);
            }
            return objLayer;
        }
        public static int ToObjectLayer(int entityType)
        {
            if (typeLayerMap.TryGetValue(entityType, out var layerMap))
            {
                return layerMap;
            }
            return 0;
        }
        private static Dictionary<int, int> typeLayerMap = new Dictionary<int, int>()
        {
            { EntityTypes.PLANT, LAYER_CONTRAPTION },
            { EntityTypes.ENEMY, LAYER_ENEMY },
            { EntityTypes.OBSTACLE, LAYER_OBSTACLE },
            { EntityTypes.BOSS, LAYER_BOSS },
            { EntityTypes.CART, LAYER_CART },
            { EntityTypes.PICKUP, LAYER_PICKUP },
            { EntityTypes.PROJECTILE, LAYER_PROJECTILE },
            { EntityTypes.EFFECT, LAYER_EFFECT },
        };
        private static Dictionary<int, int> entity2ObjectLayerMap = new Dictionary<int, int>()
        {
            { EntityCollisionHelper.MASK_PLANT, LAYER_CONTRAPTION },
            { EntityCollisionHelper.MASK_ENEMY, LAYER_ENEMY },
            { EntityCollisionHelper.MASK_OBSTACLE, LAYER_OBSTACLE },
            { EntityCollisionHelper.MASK_BOSS, LAYER_BOSS },
            { EntityCollisionHelper.MASK_CART, LAYER_CART },
            { EntityCollisionHelper.MASK_PICKUP, LAYER_PICKUP },
            { EntityCollisionHelper.MASK_PROJECTILE, LAYER_PROJECTILE },
            { EntityCollisionHelper.MASK_EFFECT, LAYER_EFFECT },
        };
        private static Dictionary<int, int> object2EntityLayerMap = new Dictionary<int, int>()
        {
            { LAYER_CONTRAPTION, EntityCollisionHelper.MASK_PLANT },
            { LAYER_ENEMY, EntityCollisionHelper.MASK_ENEMY  },
            { LAYER_OBSTACLE, EntityCollisionHelper.MASK_OBSTACLE },
            { LAYER_BOSS, EntityCollisionHelper.MASK_BOSS },
            { LAYER_CART, EntityCollisionHelper.MASK_CART },
            { LAYER_PICKUP, EntityCollisionHelper.MASK_PICKUP },
            { LAYER_PROJECTILE, EntityCollisionHelper.MASK_PROJECTILE },
            { LAYER_EFFECT, EntityCollisionHelper.MASK_EFFECT },
        };
        public const int LAYER_CONTRAPTION = 24;
        public const int LAYER_ENEMY = 25;
        public const int LAYER_OBSTACLE = 26;
        public const int LAYER_BOSS = 27;
        public const int LAYER_CART = 28;
        public const int LAYER_PICKUP = 29;
        public const int LAYER_PROJECTILE = 30;
        public const int LAYER_EFFECT = 31;
    }
}
