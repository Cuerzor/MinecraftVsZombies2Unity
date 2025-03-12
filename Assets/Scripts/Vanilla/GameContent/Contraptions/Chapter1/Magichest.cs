using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Pickups;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.magichest)]
    public class Magichest : ContraptionBehaviour
    {
        public Magichest(string nsp, string name) : base(nsp, name)
        {
            openDetector = new MagichestDetector(40);
            eatDetector = new MagichestDetector();
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
            entity.SetAnimationBool("Open", IsOpen(entity));
            entity.SetAnimationBool("Flash", GetFlashVisible(entity));
        }

        public override bool CanEvoke(Entity entity)
        {
            return base.CanEvoke(entity) && (entity.State == VanillaEntityStates.MAGICHEST_IDLE || entity.State == VanillaEntityStates.MAGICHEST_OPEN);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
        }
        public static FrameTimer GetStateTimer(Entity entity)
        {
            return entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        }
        public static void SetStateTimer(Entity entity, FrameTimer timer)
        {
            entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        }
        public static bool GetFlashVisible(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, PROP_FLASH_VISIBLE);
        }
        public static void SetFlashVisible(Entity entity, bool timer)
        {
            entity.SetBehaviourField(ID, PROP_FLASH_VISIBLE, timer);
        }
        public static void Eat(Entity entity, Entity target)
        {
            entity.SetModelProperty("FlashScale", target.GetScaledSize());
            entity.SetModelProperty("FlashSourcePosition", target.GetCenter());
            var effects = new DamageEffectList(VanillaDamageEffects.REMOVE_ON_DEATH);
            target.Die(effects, entity);
            SetFlashVisible(entity, true);
            entity.AddBuff<MagichestInvincibleBuff>();
            entity.PlaySound(VanillaSoundID.magical);
        }
        private void AttackUpdate(Entity entity)
        {
            switch (entity.State)
            {
                case VanillaEntityStates.MAGICHEST_IDLE:
                    {
                        if (openDetector.DetectExists(entity))
                        {
                            entity.State = VanillaEntityStates.MAGICHEST_OPEN;
                            entity.PlaySound(VanillaSoundID.chestOpen);
                            var stateTimer = GetStateTimer(entity);
                            stateTimer.ResetTime(15);
                        }

                        break;
                    }

                case VanillaEntityStates.MAGICHEST_OPEN:
                    {
                        var stateTimer = GetStateTimer(entity);
                        stateTimer.Run();
                        if (!openDetector.DetectExists(entity))
                        {
                            entity.State = VanillaEntityStates.IDLE;
                            entity.PlaySound(VanillaSoundID.chestClose);
                        }
                        else if (stateTimer.Expired)
                        {
                            var nearest = eatDetector.DetectEntityWithTheLeast(entity, e => (e.GetCenter() - entity.Position).magnitude);
                            if (nearest != null)
                            {
                                Eat(entity, nearest);
                                entity.State = VanillaEntityStates.MAGICHEST_EAT;
                                stateTimer.ResetTime(30);
                            }
                        }

                        break;
                    }

                case VanillaEntityStates.MAGICHEST_EAT:
                    {
                        var stateTimer = GetStateTimer(entity);
                        stateTimer.Run();
                        if (stateTimer.Expired)
                        {
                            entity.State = VanillaEntityStates.MAGICHEST_CLOSE;
                            stateTimer.ResetTime(30);
                            entity.PlaySound(VanillaSoundID.chestClose);
                        }
                        break;
                    }

                case VanillaEntityStates.MAGICHEST_CLOSE:
                    {
                        var stateTimer = GetStateTimer(entity);
                        stateTimer.Run();
                        if (stateTimer.Expired)
                        {
                            entity.Level.Spawn(VanillaPickupID.starshard, entity.Position, entity);
                            entity.Remove();
                            var effect = entity.Level.Spawn(VanillaEffectID.smokeCluster, entity.GetCenter(), entity);
                            effect.SetTint(new Color(1, 0.8f, 1, 1));
                        }
                        break;
                    }
            }
        }
        private void EvokedUpdate(Entity entity)
        {
            switch (entity.State)
            {
                case VanillaEntityStates.MAGICHEST_IDLE:
                    {
                        entity.State = VanillaEntityStates.MAGICHEST_OPEN;
                        entity.PlaySound(VanillaSoundID.chestOpen);
                        var stateTimer = GetStateTimer(entity);
                        stateTimer.ResetTime(15);
                        break;
                    }

                case VanillaEntityStates.MAGICHEST_OPEN:
                    {
                        var stateTimer = GetStateTimer(entity);
                        stateTimer.Run();
                        if (stateTimer.Expired)
                        {
                            entity.State = VanillaEntityStates.MAGICHEST_LOMS;
                            stateTimer.ResetTime(90);
                            entity.TriggerAnimation("Loms");
                        }
                        break;
                    }

                case VanillaEntityStates.MAGICHEST_LOMS:
                    {
                        var stateTimer = GetStateTimer(entity);
                        stateTimer.Run();
                        if (stateTimer.PassedFrame(30))
                        {
                            var ghast = entity.Level.Spawn(VanillaEnemyID.ghast, entity.GetCenter(), entity);
                            ghast.SetFactionAndDirection(entity.GetFaction());
                            entity.PlaySound(VanillaSoundID.fireCharge);
                        }
                        if (stateTimer.Expired)
                        {
                            entity.State = VanillaEntityStates.MAGICHEST_IDLE;
                            entity.PlaySound(VanillaSoundID.chestClose);
                            entity.SetEvoked(false);
                        }
                        break;
                    }
            }
        }
        private bool IsOpen(Entity entity)
        {
            return entity.State == VanillaEntityStates.MAGICHEST_OPEN || entity.State == VanillaEntityStates.MAGICHEST_EAT;
        }
        public static readonly NamespaceID ID = VanillaContraptionID.magichest;
        public static readonly VanillaEntityPropertyMeta PROP_FLASH_VISIBLE = new VanillaEntityPropertyMeta("FlashVisible");
        public static readonly VanillaEntityPropertyMeta PROP_STATE_TIMER = new VanillaEntityPropertyMeta("StateTimer");
        private Detector openDetector;
        private Detector eatDetector;
    }
}
