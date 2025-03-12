using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.mesmerizer)]
    public class Mesmerizer : MeleeEnemy
    {
        public Mesmerizer(string nsp, string name) : base(nsp, name)
        {
            detector = new DispenserDetector()
            {
                ignoreBoss = true,
                ignoreHighEnemy = false,
                ignoreLowEnemy = false,
                colliderFilter = (p, c) => ColliderFilter(p.entity, c)
            };
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.EquipArmor<MesmerizerCrown>();
            SetStateTimer(entity, new FrameTimer(CAST_COOLDOWN));
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            if (state == STATE_WALK && IsCasting(enemy))
            {
                return STATE_CAST;
            }
            return state;
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(2));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            if (entity.State == STATE_WALK)
            {
                var stateTimer = GetStateTimer(entity);
                stateTimer.Run(entity.GetAttackSpeed());
                if (stateTimer.Expired)
                {
                    var target = detector.DetectEntityWithTheLeast(entity, t => Mathf.Abs(entity.Position.x - t.Position.x));
                    if (target == null)
                    {
                        stateTimer.Frame = CONTROL_DETECT_TIME;
                    }
                    else
                    {
                        var param = entity.GetShootParams();
                        param.damage = 0;
                        var orb = entity.ShootProjectile(param);
                        orb.Target = target;
                        orb.SetParent(entity);
                        SetOrb(entity, new EntityID(orb));
                        SetCasting(entity, true);
                    }
                }
            }
            else if (entity.State == STATE_CAST)
            {
                var orbID = GetOrb(entity);
                var orb = orbID?.GetEntity(entity.Level);
                if (orb == null || !orb.Exists() || orb.IsDead)
                {
                    EndCasting(entity);
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (entity.State == STATE_CAST)
            {
                EndCasting(entity);
            }
        }
        private void EndCasting(Entity entity)
        {
            var stateTimer = GetStateTimer(entity);
            stateTimer.Reset();
            SetCasting(entity, false);
        }
        private bool ColliderFilter(Entity self, IEntityCollider collider)
        {
            if (!collider.IsMain())
                return false;
            var target = collider.Entity;
            if (!CompellingOrb.CanControl(target))
                return false;
            if (target.IsFloor())
                return false;
            return true;
        }

        public static void SetCasting(Entity entity, bool timer) => entity.SetBehaviourField(ID, PROP_CASTING, timer);
        public static bool IsCasting(Entity entity) => entity.GetBehaviourField<bool>(ID, PROP_CASTING);
        public static void SetOrb(Entity entity, EntityID value) => entity.SetBehaviourField(ID, PROP_ORB, value);
        public static EntityID GetOrb(Entity entity) => entity.GetBehaviourField<EntityID>(ID, PROP_ORB);
        public static void SetStateTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        public static FrameTimer GetStateTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);

        #region 常量
        private const int CAST_COOLDOWN = 300;
        private const int CONTROL_DETECT_TIME = 30;

        public const int STATE_WALK = VanillaEntityStates.WALK;
        public const int STATE_ATTACK = VanillaEntityStates.ATTACK;
        public const int STATE_CAST = VanillaEntityStates.MESMERIZER_CAST;
        private Detector detector;
        public static readonly NamespaceID ID = VanillaEnemyID.mesmerizer;
        public static readonly VanillaEntityPropertyMeta PROP_CASTING = new VanillaEntityPropertyMeta("Casting");
        public static readonly VanillaEntityPropertyMeta PROP_ORB = new VanillaEntityPropertyMeta("Orb");
        public static readonly VanillaEntityPropertyMeta PROP_STATE_TIMER = new VanillaEntityPropertyMeta("StateTimer");
        #endregion 常量
    }
}
