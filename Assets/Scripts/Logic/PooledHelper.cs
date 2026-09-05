#nullable enable

using System.Collections.Generic;
using MVZ2Logic.Entities;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.SeedPacks;
using PVZEngine.Tools;
using PVZEngine.Tools.Random;

namespace MVZ2Logic
{
    public static class PooledHelper
    {
        public static PoolItem<List<T>> RandomTakeDisposeable<T>(this IEnumerable<T> list, int count, RandomGenerator rng)
        {
            var poolItem = ListPool<T>.Rent();
            list.RandomTake(count, rng, poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<T>> RandomTakeAllDisposeable<T>(this IEnumerable<T> list, RandomGenerator rng)
        {
            var poolItem = ListPool<T>.Rent();
            list.RandomTakeAll(rng, poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<ClassicSeedPack?>> GetAllSeedPacksDisposeable(this LevelEngine level)
        {
            var poolItem = ListPool<ClassicSeedPack?>.Rent();
            level.GetAllSeedPacks(poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<ConveyorSeedPack>> GetAllConveyorSeedPacksDisposeable(this LevelEngine level)
        {
            var poolItem = ListPool<ConveyorSeedPack>.Rent();
            level.GetAllConveyorSeedPacks(poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<LawnGrid>> GetAllGridsDisposeable(this LevelEngine level)
        {
            var poolItem = ListPool<LawnGrid>.Rent();
            level.GetAllGrids(poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<Entity>> GetLayerEntitiesDisposeable(this LawnGrid grid, NamespaceID id)
        {
            var targetGridEntitiesItem = ListPool<Entity>.Rent();
            grid.GetLayerEntities(id, targetGridEntitiesItem.Value);
            return targetGridEntitiesItem;
        }
        public static PoolItem<List<Entity>> GetEntitiesDisposeable(this LawnGrid grid)
        {
            var targetGridEntitiesItem = ListPool<Entity>.Rent();
            grid.GetEntities(targetGridEntitiesItem.Value);
            return targetGridEntitiesItem;
        }
        public static PoolItem<List<NamespaceID>> GetLayersDisposeable(this LawnGrid grid)
        {
            var targetGridEntitiesItem = ListPool<NamespaceID>.Rent();
            grid.GetLayers(targetGridEntitiesItem.Value);
            return targetGridEntitiesItem;
        }
        public static PoolItem<List<NamespaceID>> GetActiveArmorSlotsDisposeable(this Entity entity)
        {
            var poolItem = ListPool<NamespaceID>.Rent();
            entity.GetActiveArmorSlots(poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<LawnGrid>> GetGridsToTakeDisposeable(this Entity entity)
        {
            var poolItem = ListPool<LawnGrid>.Rent();
            entity.GetGridsToTake(poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<LawnGrid>> GetGridsToTakeDisposeable(this EntityDefinition entity, LawnGrid grid)
        {
            var poolItem = ListPool<LawnGrid>.Rent();
            entity.GetGridsToTake(grid, poolItem.Value);
            return poolItem;
        }
        public static PoolItem<List<T>> GetBehavioursDisposeable<T>(this EntityDefinition definition)
        {
            var poolItem = ListPool<T>.Rent();
            definition.GetBehaviours<T>(poolItem.Value);
            return poolItem;
        }
    }
}