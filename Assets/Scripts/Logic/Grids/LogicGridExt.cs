#nullable enable

using System.Linq;
using MVZ2Logic.Blueprints;
using MVZ2Logic.Callbacks;
using MVZ2Logic.Entities;
using MVZ2Logic.HeldItems;
using MVZ2Logic.Placements;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.Placements;
using PVZEngine.SeedPacks;
using PVZEngine.Tools;
using UnityEngine;

namespace MVZ2Logic.Grids
{
    public static class LogicGridExt
    {
        public static HeldHighlight GetSeedHeldHighlight(this LawnGrid grid, SeedDefinition? seedDef)
        {
            if (seedDef == null)
                return HeldHighlight.None;

            if (seedDef.GetSeedType() == SeedTypes.ENTITY)
            {
                var entityID = seedDef.GetSeedEntityID();
                var entityDef = grid.Level.Content.GetEntityDefinition(entityID);
                if (entityDef == null)
                    return HeldHighlight.None;

                var error = grid.GetEntityPlaceStatus(entityDef);
                using var grids = entityDef.GetGridsToTakeDisposeable(grid);

                if (error == null)
                {
                    return HeldHighlight.Green(grids.Value);
                }
                else
                {
                    return HeldHighlight.Red(grids.Value);
                }
            }
            else
            {
                if (grid.CanPlaceBlueprint(seedDef, out _))
                {
                    return HeldHighlight.Green(grid);
                }
                else
                {
                    return HeldHighlight.Red(grid);
                }
            }
        }

        #region 放置音效
        public static NamespaceID? GetPlaceSound(this LawnGrid grid, Entity entity)
        {
            return grid.Definition.GetPlaceSound(entity);
        }
        #endregion

        #region 放置蓝图
        public static void UseEntityBlueprint(this LawnGrid grid, SeedPack seed, IHeldItemData heldItemData)
        {
            var seedDef = seed.Definition;
            var param = new PlaceParams();
            param.SetCommandBlock(seed.IsCommandBlock());
            param.SetVariant(seed.GetVariant());

            var output = grid.PlaceEntityBlueprint(seedDef, param);
            if (output.IsInvalid())
                return;
            var level = grid.Level;
            level.Triggers.RunCallback(LogicLevelCallbacks.POST_USE_ENTITY_BLUEPRINT, new LogicLevelCallbacks.PostUseEntityBlueprintParams(output, seedDef, seed, heldItemData));
        }
        #endregion

        #region 放置蓝图定义
        /// <summary>
        /// 在一个网格的位置上放置一个蓝图。
        /// 如果是实体蓝图，则会调用<see cref="PlaceEntityBlueprint"/>，放置一个实体，或在已有实体上堆叠。
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="seedDef"></param>
        /// <param name="heldItemData"></param>
        public static void UseEntityBlueprintDefinition(this LawnGrid grid, SeedDefinition seedDef, IHeldItemData heldItemData, bool isCommandBlock = false, int variant = 0)
        {
            if (seedDef == null)
                return;
            var param = new PlaceParams();
            param.SetCommandBlock(isCommandBlock);
            param.SetVariant(seedDef.GetVariant());
            var output = grid.PlaceEntityBlueprint(seedDef, param);
            if (output.IsInvalid())
                return;
            var level = grid.Level;
            level.Triggers.RunCallback(LogicLevelCallbacks.POST_USE_ENTITY_BLUEPRINT, new LogicLevelCallbacks.PostUseEntityBlueprintParams(output, seedDef, null, heldItemData));
        }
        #endregion

        #region 生成实体
        public static bool CanSpawnEntity(this LawnGrid grid, NamespaceID entityID)
        {
            return grid.GetEntitySpawnStatus(entityID) == null;
        }
        public static NamespaceID? GetEntitySpawnStatus(this LawnGrid grid, NamespaceID entityID)
        {
            var level = grid.Level;
            var entityDef = level.Content.GetEntityDefinition(entityID);
            if (entityDef == null)
                return null;
            return grid.GetEntitySpawnStatus(entityDef);
        }
        public static NamespaceID? GetEntitySpawnStatus(this LawnGrid grid, EntityDefinition entityDef)
        {
            var level = grid.Level;
            // 可放置。
            var placementID = entityDef.GetPlacementID();
            if (placementID == null)
                return null;
            var placementDef = level.Content.GetPlacementDefinition(placementID);
            if (placementDef == null)
                return null;
            return placementDef.GetSpawnError(grid, entityDef);
        }
        #endregion

        #region 放置实体
        public static bool CanPlaceBlueprint(this LawnGrid grid, NamespaceID seedID, out NamespaceID? error)
        {
            error = null;
            if (!NamespaceID.IsValid(seedID))
                return false;
            var level = grid.Level;
            var seedDef = level.Content.GetSeedDefinition(seedID);
            if (seedDef == null)
                return false;
            return CanPlaceBlueprint(grid, seedDef, out error);
        }
        public static bool CanPlaceBlueprint(this LawnGrid grid, SeedDefinition seedDef, out NamespaceID? error)
        {
            error = null;
            var level = grid.Level;
            if (seedDef.GetSeedType() == SeedTypes.ENTITY)
            {
                var entityID = seedDef.GetSeedEntityID();
                if (entityID == null)
                    return false;
                error = grid.GetEntityPlaceStatus(entityID);
                return error == null;
            }
            return false;
        }
        public static bool CanPlaceEntity(this LawnGrid grid, NamespaceID entityID)
        {
            return grid.GetEntityPlaceStatus(entityID) == null;
        }
        public static NamespaceID? GetEntityPlaceStatus(this LawnGrid grid, NamespaceID entityID)
        {
            var level = grid.Level;
            var entityDef = level.Content.GetEntityDefinition(entityID);
            if (entityDef == null)
                return null;
            return grid.GetEntityPlaceStatus(entityDef);
        }
        public static NamespaceID? GetEntityPlaceStatus(this LawnGrid grid, EntityDefinition entityDef)
        {
            var level = grid.Level;
            // 可放置。
            var placementID = entityDef.GetPlacementID();
            if (placementID == null)
                return null;
            var placementDef = level.Content.GetPlacementDefinition(placementID);
            if (placementDef == null)
                return null;
            return placementDef.GetPlaceError(grid, entityDef);
        }
        public static PlaceOutput PlaceEntityBlueprint(this LawnGrid grid, SeedDefinition seedDef, PlaceParams param)
        {
            var id = seedDef.GetSeedEntityID();
            if (id != null)
                return grid.PlaceEntity(id, param);
            return PlaceOutput.InvalidOutput;
        }
        public static PlaceOutput PlaceEntity(this LawnGrid grid, NamespaceID entityID, PlaceParams param)
        {
            var level = grid.Level;
            var entityDef = level.Content.GetEntityDefinition(entityID);
            if (entityDef != null)
                return grid.PlaceEntity(entityDef, param);
            return PlaceOutput.InvalidOutput;
        }
        public static PlaceOutput PlaceEntity(this LawnGrid grid, EntityDefinition entityDef, PlaceParams param)
        {
            var level = grid.Level;
            var placementID = entityDef.GetPlacementID();
            if (placementID != null)
            {
                var placement = level.Content.GetPlacementDefinition(placementID);
                if (placement != null)
                {
                    return grid.PlaceEntity(entityDef, placement, param);
                }
            }
            return PlaceOutput.InvalidOutput;
        }
        public static PlaceOutput PlaceEntity(this LawnGrid grid, EntityDefinition entityDef, PlacementDefinition placement, PlaceParams param)
        {
            return placement.PlaceEntity(grid, entityDef, param);
        }
        public static Entity? SpawnPlacedEntity(this LawnGrid grid, NamespaceID entityID, SpawnParams? param = null)
        {
            if (!grid.PrePlaceEntity(entityID))
                return null;

            var level = grid.Level;
            var entityDef = level.Content.GetEntityDefinition(entityID);
            if (entityDef == null)
                return null;

            var offset = entityDef.GetStartingPositionOffset() - entityDef.GetGridPivotOffset();
            var x = level.GetEntityColumnX(grid.Column) + offset.x;
            var z = level.GetEntityLaneZ(grid.Lane) + offset.z;
            var y = level.GetGroundY(x, z) + offset.y;

            var position = new Vector3(x, y, z);

            return level.Spawn(entityDef, position, null, param)?.Let(e =>
            {
                e.PlaySoundIfNotNull(grid.GetPlaceSound(e));
                grid.PostPlaceEntity(e);
            });
        }
        public static Entity? SpawnPlacedEntity(this LawnGrid grid, NamespaceID entityID, Vector3 position, SpawnParams? param = null)
        {
            if (!grid.PrePlaceEntity(entityID))
                return null;

            var level = grid.Level;
            return level.Spawn(entityID, position, null, param)?.Let(e =>
            {
                e.PlaySoundIfNotNull(grid.GetPlaceSound(e));
                grid.PostPlaceEntity(e);
            });
        }
        public static Entity? SpawnPlacedEntity(this LawnGrid grid, EntityDefinition entityDef, Vector3 position, SpawnParams? param = null)
        {
            if (!grid.PrePlaceEntity(entityDef.GetID()))
                return null;

            var level = grid.Level;
            return level.Spawn(entityDef, position, null, param)?.Let(e =>
            {
                e.PlaySoundIfNotNull(grid.GetPlaceSound(e));
                grid.PostPlaceEntity(e);
            });
        }
        private static bool PrePlaceEntity(this LawnGrid grid, NamespaceID entityID)
        {
            var level = grid.Level;
            var args = new LogicLevelCallbacks.PlaceEntityParams(grid, entityID);
            return level.Triggers.RunCallbackWithResultFiltered(LogicLevelCallbacks.PRE_PLACE_ENTITY, args, true, entityID);
        }
        private static void PostPlaceEntity(this LawnGrid grid, Entity entity)
        {
            var level = grid.Level;
            level.Triggers.RunCallbackFiltered(LogicLevelCallbacks.POST_PLACE_ENTITY, new LogicLevelCallbacks.PostPlaceEntityParams(grid, entity), entity.GetDefinitionID());
        }
        #endregion
    }
}