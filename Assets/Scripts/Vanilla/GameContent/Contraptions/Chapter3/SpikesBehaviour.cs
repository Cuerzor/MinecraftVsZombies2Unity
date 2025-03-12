using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    public abstract class SpikesBehaviour : ContraptionBehaviour
    {
        public SpikesBehaviour(string nsp, string name) : base(nsp, name)
        {
            detector = new SpikeBlockDetector();
            detectorEvoked = new SpikeBlockDetector(true);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetAttackTimer(entity, new FrameTimer(AttackInterval));
            SetEvocationTimer(entity, new FrameTimer(EvocationDuration));
            SetEvocationAttackTimer(entity, new FrameTimer(EvocationAttackInterval));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (!entity.IsEvoked())
            {
                var timer = GetAttackTimer(entity);
                timer.Run(entity.GetAttackSpeed());
                if (timer.Expired)
                {
                    timer.Reset();
                    detectBuffer.Clear();
                    detector.DetectMultiple(entity, detectBuffer);
                    if (detectBuffer.Count > 0)
                    {
                        foreach (var target in detectBuffer)
                        {
                            target.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.GROUND_SPIKES), entity);
                        }
                        entity.TriggerAnimation("Attack");
                    }
                }
            }
            else
            {
                var evocationTimer = GetEvocationTimer(entity);
                var attackTimer = GetEvocationAttackTimer(entity);
                evocationTimer.Run();
                attackTimer.Run(entity.GetAttackSpeed());
                detectBuffer.Clear();
                detectorEvoked.DetectMultiple(entity, detectBuffer);

                // 造成伤害
                if (attackTimer.Expired)
                {
                    attackTimer.Reset();
                    foreach (var target in detectBuffer)
                    {
                        target.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.GROUND_SPIKES), entity);
                    }
                    entity.TriggerAnimation("Attack");
                }
                // 拉近敌人
                foreach (var target in detectBuffer)
                {
                    var targetEnt = target.Entity;
                    if (targetEnt.Type == EntityTypes.ENEMY)
                    {
                        targetEnt.Position += (entity.Position - targetEnt.Position).normalized * PULL_SPEED;
                    }
                }
                // 产生特效
                var rng = entity.RNG;
                var level = entity.Level;
                var z = entity.Position.z;
                var padding = level.GetGridWidth() * 0.25f;
                var minX = level.GetGridLeftX() + padding;
                var maxX = level.GetGridRightX() - padding;
                for (int i = 0; i < 5; i++)
                {
                    var x = rng.Next(minX, maxX);
                    var y = level.GetGroundY(x, z);
                    var pos = new Vector3(x, y, z);
                    entity.Spawn(SpikeParticleID, pos);
                }
                // 结束
                if (evocationTimer.Expired)
                {
                    entity.SetEvoked(false);
                }
            }
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
            var evocationTimer = GetEvocationTimer(entity);
            evocationTimer.ResetTime(EvocationDuration);
            var attackTimer = GetEvocationAttackTimer(entity);
            attackTimer.ResetTime(EvocationAttackInterval);
        }
        public static FrameTimer GetAttackTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_ATTACK_TIMER);
        public static void SetAttackTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_ATTACK_TIMER, timer);
        public static FrameTimer GetEvocationTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_EVOCATION_TIMER);
        public static void SetEvocationTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_EVOCATION_TIMER, timer);
        public static FrameTimer GetEvocationAttackTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_EVOCATION_ATTACK_TIMER);
        public static void SetEvocationAttackTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_EVOCATION_ATTACK_TIMER, timer);
        public const float PULL_SPEED = 10;
        private const string PROP_REGION = "spikes_behaviour";
        [PropertyRegistry(PROP_REGION)]
        public static readonly VanillaEntityPropertyMeta PROP_ATTACK_TIMER = new VanillaEntityPropertyMeta("AttackTimer");
        [PropertyRegistry(PROP_REGION)]
        public static readonly VanillaEntityPropertyMeta PROP_EVOCATION_TIMER = new VanillaEntityPropertyMeta("EvocationTimer");
        [PropertyRegistry(PROP_REGION)]
        public static readonly VanillaEntityPropertyMeta PROP_EVOCATION_ATTACK_TIMER = new VanillaEntityPropertyMeta("EvocationAttackTimer");
        public virtual int AttackInterval => 30;
        public virtual int EvocationDuration => 120;
        public virtual int EvocationAttackInterval => 4;
        public virtual NamespaceID SpikeParticleID => VanillaEffectID.spikeParticles;
        private Detector detector;
        private Detector detectorEvoked;
        private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
    }
}
