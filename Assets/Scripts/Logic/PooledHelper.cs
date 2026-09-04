#nullable enable

using System.Collections.Generic;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.SeedPacks;
using PVZEngine.Tools;

namespace MVZ2Logic
{
    public static class PooledHelper
    {
        public static PoolItem<List<ClassicSeedPack?>> GetAllSeedPacksDisposeable(this LevelEngine level)
        {
            var seedPacksItem = ListPool<ClassicSeedPack?>.Rent();
            level.GetAllSeedPacks(seedPacksItem.Value);
            return seedPacksItem;
        }
        public static PoolItem<List<ConveyorSeedPack>> GetAllConveyorSeedPacksDisposeable(this LevelEngine level)
        {
            var seedPacksItem = ListPool<ConveyorSeedPack>.Rent();
            level.GetAllConveyorSeedPacks(seedPacksItem.Value);
            return seedPacksItem;
        }
        public static PoolItem<List<LawnGrid>> GetAllGridsDisposeable(this LevelEngine level)
        {
            var seedPacksItem = ListPool<LawnGrid>.Rent();
            level.GetAllGrids(seedPacksItem.Value);
            return seedPacksItem;
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
        public static PoolItem<List<T>> GetBehavioursDisposeable<T>(this EntityDefinition definition)
        {
            var poolItem = ListPool<T>.Rent();
            definition.GetBehaviours<T>(poolItem.Value);
            return poolItem;
        }
    }
}