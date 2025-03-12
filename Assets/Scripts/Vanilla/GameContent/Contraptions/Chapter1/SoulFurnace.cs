using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Triggers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.soulFurnace)]
    public class SoulFurnace : DispenserFamily
    {
        public SoulFurnace(string nsp, string name) : base(nsp, name)
        {
            evocationDetector = new SoulFurnaceEvocationDetector();
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            SetFuel(entity, 0);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var fuel = GetFuel(entity);
            if (fuel > 0)
            {
                if (!entity.IsEvoked())
                {
                    ShootTick(entity);
                }
                else
                {
                    EvokedUpdate(entity);
                }
            }

            UpdateSacrifice(entity);
            var percentage = fuel / (float)MAX_FUEL;
            float light = 0;
            if (fuel > 0)
            {
                light = Mathf.Lerp(0.5f, 1, percentage);
            }
            var bar = GetDisplayFuel(entity);
            bar = bar * 0.5f + (percentage) * 0.5f;
            SetDisplayFuel(entity, bar);

            entity.SetAnimationFloat("Light", light);
            entity.SetAnimationFloat("Bar", bar);

            entity.SetIsFire(fuel > 0);
            entity.SetLightSource(fuel > 0);
            entity.SetLightRange(Vector3.one * (240 * light));
        }
        public override Entity Shoot(Entity entity)
        {
            var projectile = base.Shoot(entity);
            SpendFuel(entity, 1);
            return projectile;
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var fuel = GetFuel(entity);
            fuel = Mathf.Max(REFUEL_THRESOLD, fuel);
            SetFuel(entity, fuel);
            entity.SetEvoked(true);
        }

        public int GetFuel(Entity entity) => entity.GetBehaviourField<int>(ID, PROP_FUEL);
        public void SetFuel(Entity entity, int value) => entity.SetBehaviourField(ID, PROP_FUEL, Mathf.Clamp(value, 0, MAX_FUEL));
        public void AddFuel(Entity entity, int value)
        {
            SetFuel(entity, GetFuel(entity) + value);
        }
        public void SpendFuel(Entity entity, int value)
        {
            var fuelBefore = GetFuel(entity);
            AddFuel(entity, -value);
            var fuelAfter = GetFuel(entity);
            if (fuelAfter <= 0 && fuelBefore > 0)
            {
                entity.PlaySound(VanillaSoundID.fizz);
            }
        }
        public float GetDisplayFuel(Entity entity) => entity.GetBehaviourField<float>(ID, PROP_DISPLAY_FUEL);
        public void SetDisplayFuel(Entity entity, float value) => entity.SetBehaviourField(ID, PROP_DISPLAY_FUEL, value);
        public bool CanSacrifice(Entity entity, Entity soulFurnace)
        {
            var result = new TriggerResultBoolean();
            result.Result = entity.Type == EntityTypes.PLANT && !entity.IsDead && GetFuel(soulFurnace) <= REFUEL_THRESOLD;
            entity.Level.Triggers.RunCallback(VanillaLevelCallbacks.CAN_CONTRAPTION_SACRIFICE, result, c => c(entity, soulFurnace, result));
            return result.Result;
        }
        public static int GetSacrificeFuel(Entity entity, Entity soulFurnace)
        {
            int fuel = 5;

            var game = Global.Game;
            int cost = entity.GetCost();
            var rechargeID = entity.GetRechargeID();

            float fuelMultiplier = 1;
            var rechargeDef = game.GetRechargeDefinition(rechargeID);
            if (rechargeDef != null)
            {
                fuelMultiplier = rechargeDef.GetQuality();
            }

            fuel = Mathf.CeilToInt((fuel + cost / 6f) * fuelMultiplier);
            var result = new TriggerResultInt();
            result.Result = fuel;
            entity.Level.Triggers.RunCallback(VanillaLevelCallbacks.GET_CONTRAPTION_SACRIFICE_FUEL, result, c => c(entity, soulFurnace, result));
            return result.Result;
        }
        public void Sacrifice(Entity entity, Entity soulFurnace, int fuel)
        {
            Global.Game.RunCallbackFiltered(VanillaLevelCallbacks.PRE_CONTRAPTION_SACRIFICE, entity.GetDefinitionID(), c => c(entity, soulFurnace, fuel));

            var effects = new DamageEffectList(VanillaDamageEffects.SACRIFICE, VanillaDamageEffects.SELF_DAMAGE);
            entity.Die(effects, soulFurnace);
            AddFuel(soulFurnace, fuel);
            entity.Level.Spawn(VanillaEffectID.soulfireBurn, entity.GetCenter(), soulFurnace);
            entity.PlaySound(VanillaSoundID.refuel);

            Global.Game.RunCallbackFiltered(VanillaLevelCallbacks.POST_CONTRAPTION_SACRIFICE, entity.GetDefinitionID(), c => c(entity, soulFurnace, fuel));
        }
        private void UpdateSacrifice(Entity furnace)
        {
            if (furnace.IsDead)
                return;
            var column = furnace.GetColumn();
            var lane = furnace.GetLane();
            var targetGrid = furnace.Level.GetGrid(column + 1 * furnace.GetFacingX(), lane);
            if (targetGrid == null)
                return;
            var layers = targetGrid.GetLayers();
            var orderedLayers = layers.OrderByDescending(l => Global.Game.GetGridLayerPriority(l));
            foreach (var layer in orderedLayers)
            {
                var ent = targetGrid.GetLayerEntity(layer);
                if (ent == null || !CanSacrifice(ent, furnace))
                    continue;
                var fuel = GetSacrificeFuel(ent, furnace);
                Sacrifice(ent, furnace, fuel);
                break;
            }
        }
        private void EvokedUpdate(Entity entity)
        {
            detectBuffer.Clear();
            evocationDetector.DetectMultiple(entity, detectBuffer);
            var shootPoint = entity.GetShootPoint();
            Vector3 shootDir;
            if (detectBuffer.Count <= 0)
            {
                shootDir = entity.GetFacingDirection();
            }
            else
            {
                var targetCollider = detectBuffer.Random(entity.RNG);
                var target = targetCollider?.Entity;
                shootDir = (target.Position - shootPoint).normalized;
                shootDir.y = 0;
            }
            var shotspeed = entity.GetShotVelocity().magnitude * 2;
            var velocity = shotspeed * shootDir;
            var projectile = entity.ShootProjectile(entity.GetProjectileID(), velocity);

            SoulfireBall.SetBlast(projectile, true);
            entity.PlaySound(VanillaSoundID.darkSkiesCast);

            SpendFuel(entity, 1);

            if (GetFuel(entity) <= 0)
            {
                entity.SetEvoked(false);
            }
        }

        private static readonly NamespaceID ID = VanillaContraptionID.soulFurnace;
        public static readonly VanillaEntityPropertyMeta PROP_FUEL = new VanillaEntityPropertyMeta("Fuel");
        public static readonly VanillaEntityPropertyMeta PROP_DISPLAY_FUEL = new VanillaEntityPropertyMeta("DisplayFuel");
        public const int MAX_FUEL = 60;
        public const int REFUEL_THRESOLD = 10;
        private Detector evocationDetector;
        private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
    }
}
