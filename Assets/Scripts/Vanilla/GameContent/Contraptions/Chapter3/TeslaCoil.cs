using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.teslaCoil)]
    public class TeslaCoil : ContraptionBehaviour
    {
        public TeslaCoil(string nsp, string name) : base(nsp, name)
        {
            detector = new TeslaCoilDetector(ATTACK_HEIGHT);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetAttackTimer(entity, new FrameTimer(ATTACK_COOLDOWN));
        }
        public override void Evoke(Entity entity)
        {
            base.Evoke(entity);
            entity.PlaySound(VanillaSoundID.lightningAttack);
            var pos = entity.Position;
            pos.y += 240;
            var cloud = entity.Spawn(VanillaEffectID.thunderCloud, pos);
            cloud.SetFaction(entity.GetFaction());

            CreateArc(entity, entity.Position + ARC_OFFSET, cloud.GetCenter());
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.State == STATE_IDLE)
            {
                var timer = GetAttackTimer(entity);
                timer.Run(entity.GetAttackSpeed());
                if (timer.Expired)
                {
                    if (detector.DetectExists(entity))
                    {
                        entity.State = STATE_ATTACK;
                        timer.ResetTime(ATTACK_CHARGE);
                        entity.PlaySound(VanillaSoundID.teslaPower);
                    }
                    else
                    {
                        timer.Frame = 7;
                    }
                }
            }
            else if (entity.State == STATE_ATTACK)
            {
                var timer = GetAttackTimer(entity);
                timer.Run(entity.GetAttackSpeed());
                if (timer.Expired)
                {
                    var target = detector.DetectEntityWithTheMost(entity, t => GetTargetPriority(entity, t));
                    if (target != null)
                    {
                        var faction = entity.GetFaction();
                        var damage = entity.GetDamage();
                        var sourcePosition = entity.Position + ARC_OFFSET;
                        var targetPosition = target.Position;
                        var groundY = entity.Level.GetGroundY(targetPosition.x, targetPosition.z);
                        if (targetPosition.y <= groundY)
                        {
                            targetPosition.y = groundY;
                        }
                        Shock(entity, damage, faction, SHOCK_RADIUS, targetPosition);
                        CreateArc(entity, sourcePosition, targetPosition);
                        entity.PlaySound(VanillaSoundID.teslaAttack);
                    }
                    timer.ResetTime(ATTACK_COOLDOWN);
                    entity.State = STATE_IDLE;
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Attacking", entity.State == STATE_ATTACK);
            entity.SetAnimationBool("ShowArc", entity.State != STATE_ATTACK && !entity.IsAIFrozen());
            entity.SetAnimationFloat("AttackSpeed", entity.GetAttackSpeed());
        }
        private float GetTargetPriority(Entity self, Entity target)
        {
            var target2Self = target.Position - self.Position;
            target2Self.y = 0;
            var distance = target2Self.magnitude;
            var priority = -distance;
            if (target.Position.y > self.Position.y + 40)
            {
                priority += 300;
            }
            return priority;
        }
        public static void Shock(Entity source, float damage, int faction, float shockRadius, Vector3 targetPosition, DamageEffectList damageEffects = null)
        {
            damageEffects = damageEffects ?? new DamageEffectList(VanillaDamageEffects.LIGHTNING, VanillaDamageEffects.MUTE);
            var level = source.Level;
            detectBuffer.Clear();
            gridDetectBuffer.Clear();
            level.OverlapSphereNonAlloc(targetPosition, shockRadius, faction, EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer);
            if (targetPosition.y <= level.GetGroundY(targetPosition.x, targetPosition.z) && level.IsWaterAt(targetPosition.x, targetPosition.z))
            {
                level.GetConnectedWaterGrids(targetPosition, 1, 1, gridDetectBuffer);
                foreach (var grid in gridDetectBuffer)
                {
                    var column = grid.Column;
                    var lane = grid.Lane;
                    Detection.OverlapGridGroundNonAlloc(level, column, lane, faction, EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer);
                    var x = level.GetColumnX(column) + level.GetGridWidth() * 0.5f;
                    var z = level.GetLaneZ(lane) + level.GetGridHeight() * 0.5f;
                    var y = level.GetGroundY(x, z);
                    source.Spawn(VanillaEffectID.waterLightningParticles, new Vector3(x, y, z));
                }
            }
            foreach (var collider in detectBuffer)
            {
                collider.TakeDamage(damage, damageEffects, source);
            }
        }
        public static void CreateArc(Entity source, Vector3 sourcePosition, Vector3 targetPosition)
        {
            var arc = source.Spawn(VanillaEffectID.electricArc, sourcePosition);
            ElectricArc.Connect(arc, targetPosition);
            ElectricArc.SetPointCount(arc, 20);
            ElectricArc.UpdateArc(arc);
            arc.Timeout = 30;
        }
        public static FrameTimer GetAttackTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_ATTACK_TIMER);
        public static void SetAttackTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_ATTACK_TIMER, timer);

        public const int ATTACK_COOLDOWN = 65;
        public const int ATTACK_CHARGE = 25;
        public static readonly VanillaEntityPropertyMeta PROP_ATTACK_TIMER = new VanillaEntityPropertyMeta("AttackTimer");
        public const float ATTACK_HEIGHT = 160;
        public static readonly Vector3 ARC_OFFSET = new Vector3(0, 96, 0);
        public const float SHOCK_RADIUS = 20;

        public const int STATE_IDLE = VanillaEntityStates.TESLA_COIL_IDLE;
        public const int STATE_ATTACK = VanillaEntityStates.TESLA_COIL_ATTACK;

        private Detector detector;
        private static List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        private static HashSet<LawnGrid> gridDetectBuffer = new HashSet<LawnGrid>();
        private static readonly NamespaceID ID = VanillaContraptionID.teslaCoil;
    }
}
