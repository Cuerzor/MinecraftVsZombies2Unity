using System;
using System.Collections.Generic;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.Vanilla.Detections
{
    public static class Detection
    {
        public static bool Intersects(this Hitbox self, Hitbox other)
        {
            Bounds selfBounds = self.GetBounds();
            Bounds otherBounds = other.GetBounds();
            return selfBounds.Intersects(otherBounds);
        }
        public static bool Intersects(Vector3 center1, Vector3 size1, Vector3 center2, Vector3 size2)
        {
            var bounds1 = new Bounds(center1, size1);
            var bounds2 = new Bounds(center2, size2);
            return bounds1.Intersects(bounds2);
        }
        public static bool InFrontShooterRange(Entity self, Entity target, float projectileZSpan, float heightOffset)
        {
            return IsInFrontOf(self, target) &&
                target.MainHitbox.CoincidesYDown(self.Position.y + heightOffset) &&
                IsZCoincide(self.Position.z, projectileZSpan, target.Position.z, target.GetSize().z);
        }

        public static bool InFrontShooterRange(Entity self, Entity target, float projectileZSpan, float heightOffset, float frontRange)
        {
            return IsInFrontOf(self, target, 0, frontRange) &&
                target.MainHitbox.CoincidesYDown(self.Position.y + heightOffset) &&
                IsZCoincide(self.Position.z, projectileZSpan, target.Position.z, target.GetSize().z);
        }

        public static bool CanDetect(Entity entity)
        {
            return !entity.IsInvisible();
        }

        #region X related
        public static bool IsInFrontOf(Entity self, float x, float rangeStart = 0)
        {
            if (self.IsFacingLeft())
            {
                return x <= self.Position.x - rangeStart;
            }
            else
            {
                return x >= self.Position.x + rangeStart;
            }
        }
        public static bool IsInFrontOf(Entity self, Entity other, float rangeStart, float rangeLength)
        {
            float nearRange = rangeStart;
            float farRange = rangeStart + rangeLength;
            if (self.IsFacingLeft())
            {
                return other.Position.x <= self.Position.x - nearRange && other.Position.x > self.Position.x - farRange;
            }
            else
            {
                return other.Position.x >= self.Position.x + nearRange && other.Position.x < self.Position.x + farRange;
            }
        }

        public static bool IsInFrontOf(Entity self, Entity other, float rangeStart = 0)
        {
            return IsInFrontOf(self, other.Position.x, rangeStart);
        }

        public static bool IsXCoincide(float x1, float xLength1, float x2, float xLength2)
        {
            float extent1 = xLength1 / 2;
            float extent2 = xLength2 / 2;
            return MathTool.DoRangesIntersect(x1 - extent1, x1 + extent1, x2 - extent2, x2 + extent2);
        }
        #endregion

        #region Y related
        public static bool CoincidesYDown(this Bounds bounds, float y)
        {
            return bounds.min.y < y;
        }
        public static bool CoincidesYDown(this Hitbox hitbox, float y)
        {
            return hitbox.GetBounds().min.y < y;
        }

        public static bool CoincidesYUp(this Bounds bounds, float y)
        {
            return bounds.max.y > y;
        }
        public static bool CoincidesYUp(this Hitbox hitbox, float y)
        {
            return hitbox.GetBounds().max.y > y;
        }

        public static bool IsYCoincide(float y1, float yLength1, float y2, float yLength2)
        {
            return MathTool.DoRangesIntersect(y1, y1 + yLength1, y2, y2 + yLength2);
        }
        #endregion

        #region Z related
        public static bool IsInSameRow(Entity self, Entity other)
        {
            return self.GetLane() == other.GetLane();
        }
        public static bool IsZCoincide(float z1, float zLength1, float z2, float zLength2)
        {
            float extent1 = zLength1 / 2;
            float extent2 = zLength2 / 2;
            return MathTool.DoRangesIntersect(z1 - extent1, z1 + extent1, z2 - extent2, z2 + extent2);
        }
        #endregion

        public static bool IsInSphere(this Hitbox hitbox, Vector3 center, float radius)
        {
            var bounds = hitbox.GetBounds();
            return MathTool.CollideBetweenCubeAndSphere(center, radius, bounds.center, bounds.size);
        }
        public static bool IsInSphere(this EntityCollider collider, Vector3 center, float radius)
        {
            for (int i = 0; i < collider.GetHitboxCount(); i++)
            {
                var hitbox = collider.GetHitbox(i);
                if (hitbox.IsInSphere(center, radius))
                    return true;
            }
            return false;
        }
        public static void OverlapGridGroundNonAlloc(this LevelEngine level, int column, int lane, int faction, int hostileMask, int friendlyMask, List<EntityCollider> results)
        {
            var minX = level.GetColumnX(column);
            var minZ = level.GetLaneZ(lane);
            var sizeX = minX + level.GetGridWidth();
            var sizeY = 200;
            var sizeZ = minZ + level.GetGridHeight();
            var centerX = minX + sizeX * 0.5f;
            var centerZ = minZ + sizeZ * 0.5f;
            var centerY = level.GetGroundY(centerX, centerZ) - sizeY * 0.5f;
            var center = new Vector3(centerX, centerY, centerZ);
            var size = new Vector3(sizeX, sizeY, sizeZ);
            level.OverlapBoxNonAlloc(center, size, faction, hostileMask, friendlyMask, results);
        }
    }
}