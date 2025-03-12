using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.punchton)]
    public class Punchton : ContraptionBehaviour
    {
        public Punchton(string nsp, string name) : base(nsp, name)
        {
            detector = new PunchtonDetector();
            evokedDetector = new PunchtonDetector()
            {
                infiniteRange = true
            };
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStateTimer(entity, new FrameTimer());
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (!entity.IsEvoked())
            {
                AttackUpdate(entity);
                return;
            }
            EvokedUpdate(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationFloat("Extension", GetArmExtension(entity));
            entity.SetAnimationInt("ArmState", GetArmState(entity));
        }

        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var timer = GetStateTimer(entity);
            timer.ResetTime(30);
            entity.State = VanillaEntityStates.PUNCHTON_IDLE;
            entity.SetEvoked(true);
        }
        private void AttackUpdate(Entity entity)
        {
            if (entity.State == VanillaEntityStates.PUNCHTON_IDLE)
            {
                var extension = GetArmExtension(entity);
                extension = extension * 0.5f;
                SetArmExtension(entity, extension);

                var target = detector.Detect(entity);
                if (target != null)
                {
                    var timer = GetStateTimer(entity);
                    timer.ResetTime(30);
                    entity.State = VanillaEntityStates.PUNCHTON_PUNCH;
                    Punch(entity);
                }
            }
            else if (entity.State == VanillaEntityStates.PUNCHTON_PUNCH)
            {
                var extension = GetArmExtension(entity);
                extension = extension * 0.5f + entity.GetRange() * 0.5f;
                SetArmExtension(entity, extension);

                var timer = GetStateTimer(entity);
                timer.Run();
                if (timer.Expired)
                {
                    timer.ResetTime(RESTORE_TIME);
                    entity.State = VanillaEntityStates.PUNCHTON_BROKEN;

                    // Spawn droken piston palm.
                    var direction = entity.GetFacingDirection();
                    var position = entity.Position + direction * extension;
                    var effect = entity.Level.Spawn(VanillaEffectID.brokenArmor, position, entity);
                    effect.Velocity = direction * -5;
                    effect.SetDisplayScale(entity.GetDisplayScale());
                    effect.ChangeModel(VanillaModelID.pistonPalm);
                }
            }
            else if (entity.State == VanillaEntityStates.PUNCHTON_BROKEN)
            {
                var extension = GetArmExtension(entity);
                extension = extension * 0.5f;
                SetArmExtension(entity, extension);

                var timer = GetStateTimer(entity);
                timer.Run();
                if (timer.Expired)
                {
                    timer.Reset();
                    entity.State = VanillaEntityStates.PUNCHTON_IDLE;
                }
            }
        }
        private void Punch(Entity entity)
        {
            entity.PlaySound(VanillaSoundID.punch);
            detectBuffer.Clear();
            detector.DetectMultiple(entity, detectBuffer);
            foreach (var collider in detectBuffer)
            {
                collider.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.IGNORE_ARMOR, VanillaDamageEffects.PUNCH, VanillaDamageEffects.MUTE), entity);

                var ent = collider.Entity;
                if (ent.Type == EntityTypes.ENEMY)
                {
                    ent.Velocity += entity.GetFacingDirection() * 20;
                    CheckAchievement(ent);
                }
            }
        }
        private void EvokedUpdate(Entity entity)
        {
            if (entity.State == VanillaEntityStates.PUNCHTON_IDLE)
            {
                var extension = GetArmExtension(entity);
                extension = extension * 0.5f;
                SetArmExtension(entity, extension);

                var timer = GetStateTimer(entity);
                timer.Run();
                if (timer.Expired)
                {
                    LongPunch(entity);
                    timer.ResetTime(30);
                    entity.State = VanillaEntityStates.PUNCHTON_PUNCH;
                }
            }
            else if (entity.State == VanillaEntityStates.PUNCHTON_PUNCH)
            {
                var extension = GetArmExtension(entity);
                extension = extension * 0.5f + 1400 * 0.5f;
                SetArmExtension(entity, extension);

                var timer = GetStateTimer(entity);
                timer.Run();
                if (timer.Expired)
                {
                    timer.Reset();
                    entity.State = VanillaEntityStates.PUNCHTON_IDLE;
                    entity.SetEvoked(false);
                }
            }
        }
        private void LongPunch(Entity entity)
        {
            entity.PlaySound(VanillaSoundID.punch);
            detectBuffer.Clear();
            evokedDetector.DetectMultiple(entity, detectBuffer);
            foreach (var collider in detectBuffer)
            {
                collider.TakeDamage(entity.GetDamage() * EVOKED_DAMAGE_MULTIPLIER, new DamageEffectList(VanillaDamageEffects.IGNORE_ARMOR, VanillaDamageEffects.PUNCH, VanillaDamageEffects.MUTE), entity);
                var ent = collider.Entity;
                if (ent.Type == EntityTypes.ENEMY)
                {
                    var pos = ent.Position;
                    pos.x = VanillaLevelExt.GetBorderX(!entity.IsFacingLeft());
                    ent.Position = pos;
                }
            }
        }
        private int GetArmState(Entity entity)
        {
            if (entity.IsEvoked() && entity.State == VanillaEntityStates.PUNCHTON_IDLE)
                return 2;
            if (entity.State == VanillaEntityStates.PUNCHTON_BROKEN)
                return 1;
            return 0;
        }
        public float GetArmExtension(Entity entity) => entity.GetBehaviourField<float>(ID, PROP_ARM_EXTENSION);
        public void SetArmExtension(Entity entity, float value) => entity.SetBehaviourField(ID, PROP_ARM_EXTENSION, value);
        public FrameTimer GetStateTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        public void SetStateTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        private void CheckAchievement(Entity entity)
        {
            if (entity.Type != EntityTypes.ENEMY)
                return;
            if (entity.HasBuff<PunchtonAchievementBuff>())
            {
                Global.Game.Unlock(VanillaUnlockID.doubleTrouble);
            }
            else
            {
                entity.AddBuff<PunchtonAchievementBuff>();
            }
        }
        public const int RESTORE_TIME = 600;
        public const float EVOKED_DAMAGE_MULTIPLIER = 5;
        private static readonly NamespaceID ID = VanillaContraptionID.punchton;
        public static readonly VanillaEntityPropertyMeta PROP_ARM_EXTENSION = new VanillaEntityPropertyMeta("ArmExtension");
        public static readonly VanillaEntityPropertyMeta PROP_STATE_TIMER = new VanillaEntityPropertyMeta("StateTimer");
        private Detector detector;
        private Detector evokedDetector;
        private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
    }
}
