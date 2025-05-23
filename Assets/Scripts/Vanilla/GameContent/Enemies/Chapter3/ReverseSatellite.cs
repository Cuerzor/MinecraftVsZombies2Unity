using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.reverseSatellite)]
    public class ReverseSatellite : StateEnemy
    {
        public ReverseSatellite(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 80);

            entity.Level.AddLoopSoundEntity(VanillaSoundID.morseCodeReverse, entity.ID);

            SetLeaveTimer(entity, new FrameTimer(LEAVE_TIME));

            entity.Velocity = new Vector3(entity.RNG.Next(-1f, 1f), 0, entity.RNG.Next(-1f, 1f));
        }
        protected override void UpdateAI(Entity enemy)
        {
            base.UpdateAI(enemy);
            if (!enemy.Level.HasBuff<ReverseSatelliteBuff>())
            {
                enemy.Level.AddBuff<ReverseSatelliteBuff>();
            }

            if (enemy.State == STATE_LEAVING)
            {
                var velocity = enemy.Velocity;
                if (velocity.x < 6)
                {
                    velocity.x += 0.1f;
                }
                enemy.Velocity = velocity;

                var pos = enemy.Position;
                if (pos.x > VanillaLevelExt.RIGHT_BORDER || pos.y > 640f || pos.z < -40f)
                {
                    enemy.Remove();
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            entity.RemoveBuffs<FlyBuff>();
        }
        public override void PostContactGround(Entity entity, Vector3 velocity)
        {
            base.PostContactGround(entity, velocity);
            if ((entity.IsDead || entity.IsAIFrozen()) && !entity.IsOnWater())
            {
                float damageMutliplier = 1 + entity.Level.GetEnemyAILevel();
                float radius = entity.GetRange();
                var damage = entity.GetDamage() * damageMutliplier;
                if (damage >= 0)
                {
                    entity.Level.Explode(entity.GetCenter(), radius, entity.GetFaction(), damage, new DamageEffectList(VanillaDamageEffects.EXPLOSION), entity);
                }
                var param = entity.GetSpawnParams();
                param.SetProperty(EngineEntityProps.SIZE, entity.GetScaledSize());
                var explosion = entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
                entity.PlaySound(VanillaSoundID.explosion);

                entity.Remove();
            }
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            if (state == VanillaEntityStates.ATTACK)
            {
                state = STATE_STAY;
            }
            if (state == STATE_STAY)
            {
                var leavingTimer = GetLeaveTimer(enemy);
                if (leavingTimer == null || leavingTimer.Expired || enemy.Level.WaveState == VanillaLevelStates.STATE_AFTER_FINAL_WAVE || enemy.Level.IsAllEnemiesCleared())
                {
                    state = STATE_LEAVING;
                }
            }
            return state;
        }
        protected override void UpdateStateWalk(Entity enemy)
        {
            var pos = enemy.Position;
            var velocity = enemy.Velocity;
            var centerX = VanillaLevelExt.LAWN_CENTER_X;
            var centerZ = enemy.Level.GetLawnCenterZ();
            var backDistanceX = 200;
            var backDistanceZ = 80;
            if (pos.x > centerX + backDistanceX)
            {
                if (velocity.x > -3)
                {
                    velocity.x -= 0.1f;
                }
            }
            else if (pos.x < centerX - backDistanceX)
            {
                if (velocity.x < 3)
                {
                    velocity.x += 0.1f;
                }
            }
            if (pos.z > centerZ + backDistanceZ)
            {
                if (velocity.z > -3)
                {
                    velocity.z -= 0.1f;
                }
            }
            else if (pos.z < centerZ - backDistanceZ)
            {
                if (velocity.z < 3)
                {
                    velocity.z += 0.1f;
                }
            }
            var magnitude = velocity.magnitude;
            magnitude += 0.05f;
            enemy.Velocity = velocity.normalized * magnitude;

            var leaveTimer = GetLeaveTimer(enemy);
            leaveTimer.Run();
        }
        public static FrameTimer GetLeaveTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, FIELD_LEAVE_TIMER);
        public static void SetLeaveTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, FIELD_LEAVE_TIMER, value);
        public const int STATE_STAY = VanillaEntityStates.WALK;
        public const int STATE_LEAVING = VanillaEntityStates.ENEMY_SPECIAL;
        public const int LEAVE_TIME = 900;
        public static readonly VanillaEntityPropertyMeta FIELD_LEAVE_TIMER = new VanillaEntityPropertyMeta("LeaveTimer");
        private static readonly NamespaceID ID = VanillaEnemyID.reverseSatellite;
    }
}
