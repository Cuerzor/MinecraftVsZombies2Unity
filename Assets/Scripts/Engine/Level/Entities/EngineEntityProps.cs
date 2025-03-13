using UnityEngine;

namespace PVZEngine.Entities
{
    [PropertyRegistryRegion]
    public static class EngineEntityProps
    {
        private static PropertyMeta Get(string name)
        {
            return new PropertyMeta(name);
        }
        public static readonly PropertyMeta MAX_HEALTH = Get("maxHealth");

        public static readonly PropertyMeta SHELL = Get("shell");
        public static readonly PropertyMeta PLACEMENT = Get("placement");

        public static readonly PropertyMeta SCALE = Get("scale");
        public static readonly PropertyMeta DISPLAY_SCALE = Get("displayScale");
        public static readonly PropertyMeta FRICTION = Get("friction");
        public static readonly PropertyMeta GRAVITY = Get("gravity");
        public static readonly PropertyMeta VELOCITY_DAMPEN = Get("velocityDampen");
        public static readonly PropertyMeta SIZE = Get("size");
        public static readonly PropertyMeta BOUNDS_PIVOT = Get("boundsPivot");
        public static readonly PropertyMeta GROUND_LIMIT_OFFSET = Get("groundLimitOffset");

        public static readonly PropertyMeta HEALTH = Get("health");
        public static readonly PropertyMeta TINT = Get("tint");
        public static readonly PropertyMeta COLOR_OFFSET = Get("colorOffset");
        public static readonly PropertyMeta FACE_LEFT_AT_DEFAULT = Get("faceLeftAtDefault");
        public static bool FaceLeftAtDefault(this Entity entity)
        {
            return entity.GetProperty<bool>(FACE_LEFT_AT_DEFAULT);
        }

        public static readonly PropertyMeta FACTION = Get("faction");
        public static readonly PropertyMeta INVINCIBLE = Get("invincible");

        public static readonly PropertyMeta MODEL_ID = Get("modelId");

        public static bool IsInvincible(this Entity entity)
        {
            return entity.GetProperty<bool>(INVINCIBLE);
        }
        public static float GetGravity(this EntityDefinition entity)
        {
            return entity.GetProperty<float>(GRAVITY);
        }
        public static float GetGravity(this Entity entity)
        {
            return entity.GetProperty<float>(GRAVITY);
        }
        public static void SetGravity(this Entity entity, float value)
        {
            entity.SetProperty(GRAVITY, value);
        }
        public static Vector3 GetScale(this Entity entity)
        {
            return entity.GetProperty<Vector3>(SCALE);
        }
        public static void SetScale(this Entity entity, Vector3 value)
        {
            entity.SetProperty(SCALE, value);
        }
        public static Vector3 GetDisplayScale(this Entity entity)
        {
            return entity.GetProperty<Vector3>(DISPLAY_SCALE);
        }
        public static void SetDisplayScale(this Entity entity, Vector3 value)
        {
            entity.SetProperty(DISPLAY_SCALE, value);
        }
        public static Vector3 GetVelocityDampen(this Entity entity)
        {
            return entity.GetProperty<Vector3>(VELOCITY_DAMPEN);
        }
        public static void SetVelocityDampen(this Entity entity, Vector3 value)
        {
            entity.SetProperty(VELOCITY_DAMPEN, value);
        }
        public static float GetFriction(this Entity entity)
        {
            return entity.GetProperty<float>(FRICTION);
        }
        public static void SetFriction(this Entity entity, float value)
        {
            entity.SetProperty(FRICTION, value);
        }
        public static Color GetTint(this Entity entity, bool ignoreBuffs = false)
        {
            return entity.GetProperty<Color>(TINT, ignoreBuffs: ignoreBuffs);
        }
        public static void SetTint(this Entity entity, Color value)
        {
            entity.SetProperty(TINT, value);
        }
        public static Color GetColorOffset(this Entity entity, bool ignoreBuffs = false)
        {
            return entity.GetProperty<Color>(COLOR_OFFSET, ignoreBuffs: ignoreBuffs);
        }
        public static void SetColorOffset(this Entity entity, Color value)
        {
            entity.SetProperty(COLOR_OFFSET, value);
        }
        public static void SetFaction(this Entity entity, int value)
        {
            entity.SetProperty(FACTION, value);
        }
        public static float GetGroundLimitOffset(this Entity entity)
        {
            return entity.GetProperty<float>(EngineEntityProps.GROUND_LIMIT_OFFSET);
        }
        public static Vector3 GetSize(this EntityDefinition entity)
        {
            return entity.GetProperty<Vector3>(SIZE);
        }
        public static Vector3 GetSize(this Entity entity, bool ignoreBuffs = false)
        {
            return entity.GetProperty<Vector3>(SIZE, ignoreBuffs: ignoreBuffs);
        }
        public static void SetSize(this Entity entity, Vector3 value)
        {
            entity.SetProperty(SIZE, value);
        }
        public static Vector3 GetBoundsPivot(this EntityDefinition entity)
        {
            return entity.GetProperty<Vector3>(BOUNDS_PIVOT);
        }
        public static Vector3 GetBoundsPivot(this Entity entity, bool ignoreBuffs = false)
        {
            return entity.GetProperty<Vector3>(BOUNDS_PIVOT, ignoreBuffs: ignoreBuffs);
        }
        public static void SetBoundsPivot(this Entity entity, Vector3 value)
        {
            entity.SetProperty(BOUNDS_PIVOT, value);
        }
        public static float GetMaxHealth(this Entity entity, bool ignoreBuffs = false)
        {
            return entity.GetProperty<float>(MAX_HEALTH, ignoreBuffs: ignoreBuffs);
        }
        public static NamespaceID GetShellID(this Entity entity, bool ignoreBuffs = false)
        {
            return entity.GetProperty<NamespaceID>(SHELL, ignoreBuffs: ignoreBuffs);
        }
        public static NamespaceID GetPlacementID(this EntityDefinition entity)
        {
            return entity.GetProperty<NamespaceID>(PLACEMENT);
        }
        public static void SetShellID(this Entity entity, NamespaceID value)
        {
            entity.SetProperty(SHELL, value);
        }
        public static readonly PropertyMeta COLLISION_DETECTION = Get("collisionDetection");
        public static int GetCollisionDetection(this Entity entity)
        {
            return entity.GetProperty<int>(COLLISION_DETECTION);
        }
        public static readonly PropertyMeta COLLISION_SAMPLE_LENGTH = Get("collisionSampleLength");
        public static float GetCollisionSampleLength(this Entity entity)
        {
            return entity.GetProperty<float>(COLLISION_SAMPLE_LENGTH);
        }
    }
}
