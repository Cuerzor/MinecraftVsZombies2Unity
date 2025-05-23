using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Modifiers;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Base;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.theGiant)]
    public partial class TheGiant : BossBehaviour
    {
        public TheGiant(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(EngineEntityProps.FLIP_X, PROP_FLIP_X, priority: VanillaModifierPriorities.FORCE));
        }

        #region 回调
        public override void Init(Entity boss)
        {
            base.Init(boss);
            stateMachine.Init(boss);
            stateMachine.StartState(boss, STATE_IDLE);

            boss.CollisionMaskHostile |=
                EntityCollisionHelper.MASK_PLANT |
                EntityCollisionHelper.MASK_ENEMY |
                EntityCollisionHelper.MASK_OBSTACLE |
                EntityCollisionHelper.MASK_BOSS |
                EntityCollisionHelper.MASK_CART;
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            stateMachine.UpdateAI(entity);
            if (entity.IsDead)
                return;

            var cryTimer = GetCryTimer(entity);
            if (cryTimer == null)
            {
                cryTimer = new FrameTimer(CRY_INTERVAL);
                SetCryTimer(entity, cryTimer);
            }
            if (!entity.HasBuff<TheGiantInactiveBuff>())
            {
                cryTimer.Run();
            }
            if (cryTimer.Expired)
            {
                cryTimer.ResetTime(CRY_INTERVAL);
                entity.PlaySound(VanillaSoundID.zombieCry, 0.5f);
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            stateMachine.UpdateLogic(entity);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var other = collision.Other;
            var self = collision.Entity;
            if (self.IsDead)
                return;
            if (!other.Exists() || !other.IsHostile(self))
                return;
            if (self.State == STATE_SNAKE)
            {
                var substate = stateMachine.GetSubState(self);
                if (substate != SnakeState.SUBSTATE_SNAKE)
                    return;
                if (other.IsInvincible())
                    return;
                var damageEffects = new DamageEffectList(VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE);
                collision.OtherCollider.TakeDamage(self.GetDamage() * SNAKE_DAMAGE_MULTIPLIER, damageEffects, self);
                return;
            }
            if (self.State == STATE_PACMAN)
            {
                if (IsPacmanGhost(other) && IsPacman(self))
                {
                    var damageEffects = new DamageEffectList(VanillaDamageEffects.MUTE);
                    self.TakeDamage(PACMAN_GHOST_DAMAGE, damageEffects, other);
                    KillPacman(self);
                    return;
                }
                if (!other.IsInvincible() && !IsPacmanPanic(self))
                {
                    var damageEffects = new DamageEffectList(VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.REMOVE_ON_DEATH, VanillaDamageEffects.MUTE);
                    var result = collision.OtherCollider.TakeDamage(self.GetDamage() * PACMAN_DAMAGE_MULTIPLIER, damageEffects, self);
                    if (result != null)
                    {
                        var level = self.Level;
                        self.HealEffects(result.GetTotalSpendAmount() * PACMAN_HEAL_MULTIPLIER, other);
                        if (!level.IsPlayingSound(VanillaSoundID.pacmanAttack))
                        {
                            level.PlaySound(VanillaSoundID.pacmanAttack);
                        }
                        if (result.HasAnyFatal())
                        {
                            other.PlaySound(VanillaSoundID.pacmanKill);
                        }
                    }
                }
                return;
            }
            if (other.Type == EntityTypes.CART)
            {
                other.Die(self);
                other.PlaySound(VanillaSoundID.smash);
                return;
            }
            var otherCollider = collision.OtherCollider;
            if (!other.IsInvincible() && other.Type == EntityTypes.PLANT)
            {
                var crushDamage = 1000000;
                var result = otherCollider.TakeDamage(crushDamage, new DamageEffectList(VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), self);
                if (result != null && result.HasAnyFatal())
                {
                    other.PlaySound(VanillaSoundID.smash);
                }
            }
        }
        public override void PreTakeDamage(DamageInput damageInfo, CallbackResult result)
        {
            base.PreTakeDamage(damageInfo, result);
            var entity = damageInfo.Entity;
            var malleable = GetMalleable(entity);
            if (malleable >= 0)
            {
                damageInfo.Multiply(1 - malleable / MAX_MALLEABLE_DAMAGE);
            }
            if (damageInfo.Amount > 600)
            {
                damageInfo.SetAmount(600);
            }
        }
        public override void PostTakeDamage(DamageOutput result)
        {
            base.PostTakeDamage(result);
            var entity = result.Entity;
            if (entity.Level.GetBossAILevel() <= 0)
                return;
            var bodyResult = result.BodyResult;
            if (bodyResult != null)
            {
                var malleable = GetMalleable(entity);
                SetMalleable(entity, malleable + bodyResult.Amount);
            }
        }
        #endregion 事件

        #region 字段
        public static FrameTimer GetCryTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_CRY_TIMER);
        public static void SetCryTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(PROP_CRY_TIMER, value);
        public static int GetPhase(Entity entity) => entity.GetBehaviourField<int>(PROP_PHASE);
        public static void SetPhase(Entity entity, int value) => entity.SetBehaviourField(PROP_PHASE, value);
        public static bool IsFlipX(Entity entity) => entity.GetBehaviourField<bool>(PROP_FLIP_X);
        public static void SetFlipX(Entity entity, bool value) => entity.SetBehaviourField(PROP_FLIP_X, value);
        public static int GetTargetGridIndex(Entity entity) => entity.GetBehaviourField<int>(PROP_TARGET_GRID_INDEX);
        public static void SetTargetGridIndex(Entity entity, int value) => entity.SetBehaviourField(PROP_TARGET_GRID_INDEX, value);
        public static int GetAttackFlag(Entity entity) => entity.GetBehaviourField<int>(PROP_ATTACK_FLAG);
        public static void SetAttackFlag(Entity entity, int value) => entity.SetBehaviourField(PROP_ATTACK_FLAG, value);
        public static float GetMalleable(Entity entity) => entity.GetBehaviourField<float>(PROP_MALLEABLE);
        public static void SetMalleable(Entity entity, float value) => entity.SetBehaviourField(PROP_MALLEABLE, value);
        public static void ResetMalleable(Entity entity) => SetMalleable(entity, 0);

        #region 僵尸块
        public static List<EntityID> GetZombieBlocks(Entity entity) => entity.GetBehaviourField<List<EntityID>>(PROP_ZOMBIE_BLOCKS);
        public static void SetZombieBlocks(Entity entity, List<EntityID> value) => entity.SetBehaviourField(PROP_ZOMBIE_BLOCKS, value);
        public static void AddZombieBlock(Entity entity, EntityID value)
        {
            var blocks = GetZombieBlocks(entity);
            if (blocks == null)
            {
                blocks = new List<EntityID>();
                SetZombieBlocks(entity, blocks);
            }
            blocks.Add(value);
        }
        public static bool RemoveZombieBlock(Entity entity, EntityID value)
        {
            var blocks = GetZombieBlocks(entity);
            if (blocks == null)
            {
                return false;
            }
            return blocks.Remove(value);
        }
        public static bool HasZombieBlock(Entity entity, EntityID value)
        {
            var blocks = GetZombieBlocks(entity);
            if (blocks == null)
            {
                return false;
            }
            return blocks.Contains(value);
        }
        public static void ClearZombieBlocks(Entity entity)
        {
            var blocks = GetZombieBlocks(entity);
            if (blocks == null)
            {
                return;
            }
            blocks.Clear();
        }
        #endregion

        #region 贪吃蛇尾巴
        public static List<EntityID> GetSnakeTails(Entity entity) => entity.GetBehaviourField<List<EntityID>>(PROP_SNAKE_TAILS);
        public static void SetSnakeTails(Entity entity, List<EntityID> value) => entity.SetBehaviourField(PROP_SNAKE_TAILS, value);
        public static void AddSnakeTail(Entity entity, EntityID value)
        {
            var blocks = GetSnakeTails(entity);
            if (blocks == null)
            {
                blocks = new List<EntityID>();
                SetSnakeTails(entity, blocks);
            }
            blocks.Add(value);
        }
        public static bool RemoveSnakeTail(Entity entity, EntityID value)
        {
            var blocks = GetSnakeTails(entity);
            if (blocks == null)
            {
                return false;
            }
            return blocks.Remove(value);
        }
        public static bool HasSnakeTail(Entity entity, EntityID value)
        {
            var blocks = GetSnakeTails(entity);
            if (blocks == null)
            {
                return false;
            }
            return blocks.Contains(value);
        }
        public static void ClearSnakeTails(Entity entity)
        {
            var blocks = GetSnakeTails(entity);
            if (blocks == null)
            {
                return;
            }
            blocks.Clear();
        }
        #endregion

        #endregion

        public static void SetInactive(Entity entity, bool value)
        {
            entity.SetAnimationBool("Active", !value);
            if (value)
            {
                if (!entity.HasBuff<TheGiantInactiveBuff>())
                    entity.AddBuff<TheGiantInactiveBuff>();
            }
            else
            {
                entity.RemoveBuffs<TheGiantInactiveBuff>();
            }
        }
        public static bool AtLeft(Entity entity)
        {
            return entity.Position.x < VanillaLevelExt.LAWN_CENTER_X;
        }
        public static bool CanBeStunned(Entity entity)
        {
            return !unstunnableStates.Contains(entity.State);
        }

        #region 僵尸块
        public static int GetZombieBlockLeftStartColumn(Entity entity)
        {
            return ZOMBIE_BLOCK_LEFT_COLUMN_START + ZOMBIE_BLOCK_COLUMNS - 1;
        }
        public static int GetZombieBlockRightStartColumn(Entity entity)
        {
            return entity.Level.GetMaxColumnCount() - ZOMBIE_BLOCK_COLUMNS;
        }
        public static int GetZombieBlockStartColumn(Entity entity, int x, bool atLeft)
        {
            int column;
            if (atLeft)
            {
                column = GetZombieBlockLeftStartColumn(entity) - x;
            }
            else
            {
                column = GetZombieBlockRightStartColumn(entity) + x;
            }
            return column;
        }
        public static int GetZombieBlockEndColumn(Entity entity, int x, bool atLeft)
        {
            int columnEnd;
            if (atLeft)
            {
                columnEnd = GetZombieBlockRightStartColumn(entity) - x + (ZOMBIE_BLOCK_COLUMNS - 1);
            }
            else
            {
                columnEnd = GetZombieBlockLeftStartColumn(entity) + x - (ZOMBIE_BLOCK_COLUMNS - 1);
            }
            return columnEnd;
        }
        public static Vector3 GetZombieBlockPosition(Entity entity, int index, bool atLeft)
        {
            int x = index % ZOMBIE_BLOCK_COLUMNS;
            int y = index / ZOMBIE_BLOCK_COLUMNS;
            int column = GetZombieBlockStartColumn(entity, x, atLeft);
            int columnEnd = GetZombieBlockEndColumn(entity, x, atLeft);
            var lane = y;
            return entity.Level.GetEntityGridPosition(column, lane);
        }
        private static Entity SpawnZombieBlock(Entity spawner, Vector3 position)
        {
            var block = spawner.Spawn(VanillaEffectID.zombieBlock, position, spawner.GetSpawnParams());
            block.SetParent(spawner);
            AddZombieBlock(spawner, new EntityID(block));
            return block;
        }
        private static bool AreAllZombieBlocksReached(Entity parent)
        {
            var blocks = GetZombieBlocks(parent);
            if (blocks == null)
                return true;
            foreach (var blockID in blocks)
            {
                var block = blockID.GetEntity(parent.Level);
                if (!block.ExistsAndAlive())
                    continue;
                if (!ZombieBlock.IsReached(block))
                    return false;
            }
            return true;
        }
        private static void RemoveAllZombieBlocks(Entity parent)
        {
            var blocks = GetZombieBlocks(parent);
            if (blocks == null)
                return;
            foreach (var blockID in blocks)
            {
                var block = blockID.GetEntity(parent.Level);
                if (block == null || !block.Exists())
                    continue;
                block.Remove();
            }
            blocks.Clear();
        }
        #endregion

        #region 僵尸块
        private static Entity SpawnSnakeTail(Entity spawner, Vector3 position, Entity parent)
        {
            var tail = spawner.Spawn(VanillaBossID.theGiantSnakeTail, position, spawner.GetSpawnParams());
            tail.SetParent(parent);
            TheGiantSnakeTail.SetChildTail(parent, new EntityID(tail));
            AddSnakeTail(spawner, new EntityID(tail));
            return tail;
        }
        private static void RemoveAllSnakeTails(Entity parent)
        {
            var tails = GetSnakeTails(parent);
            if (tails == null)
                return;
            foreach (var tailID in tails)
            {
                var tail = tailID.GetEntity(parent.Level);
                if (tail == null || !tail.Exists())
                    continue;
                tail.Remove();
            }
            tails.Clear();
        }
        #endregion

        #region 拼合位置
        public static float GetCombineX(Entity entity, bool atLeft)
        {
            var level = entity.Level;
            int startColumn;
            int endColumn;
            if (atLeft)
            {
                startColumn = GetZombieBlockLeftStartColumn(entity);
                endColumn = startColumn - ZOMBIE_BLOCK_COLUMNS;
            }
            else
            {
                startColumn = GetZombieBlockRightStartColumn(entity);
                endColumn = startColumn + ZOMBIE_BLOCK_COLUMNS;
            }
            var startX = level.GetEntityColumnX(startColumn);
            var endX = level.GetEntityColumnX(endColumn);
            return (startX + endX) * 0.5f;
        }
        public static float GetCombineZ(Entity entity)
        {
            return entity.Level.GetLawnCenterZ();
        }
        public static Vector3 GetCombinePosition(Entity entity, bool atLeft)
        {
            var level = entity.Level;
            var x = GetCombineX(entity, atLeft);
            var z = GetCombineZ(entity);
            var y = level.GetGroundY(x, z);
            return new Vector3(x, y, z);
        }
        #endregion

        public static Entity FindEyeBulletTarget(Entity entity, bool outerEye)
        {
            var detector = outerEye ? outerEyeBulletDetector : innerEyeBulletDetector;
            return detector.DetectEntityWithTheLeast(entity, e => e.Position.x - entity.Position.x);
        }
        public static void Smash(Entity entity, bool outerEye)
        {
            var detector = outerEye ? outerArmDetector : innerArmDetector;
            smashDetectBuffer.Clear();
            detector.DetectMultiple(entity, smashDetectBuffer);
            bool damaged = false;
            for (int i = 0; i < smashDetectBuffer.Count; i++)
            {
                var collider = smashDetectBuffer[i];
                collider.TakeDamage(entity.GetDamage() * SMASH_DAMAGE_MULTIPLIER, new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY), entity);
                damaged = true;
                entity.Level.ShakeScreen(10, 0, 15);
                entity.PlaySound(VanillaSoundID.thump);
            }
            if (damaged)
            {
                entity.PlaySound(VanillaSoundID.smash);
            }
        }
        public static bool CanArmsAttack(Entity entity)
        {
            return outerArmDetector.DetectExists(entity) || innerArmDetector.DetectExists(entity);
        }
        public static bool CanRoarStun(Entity entity, Entity target)
        {
            return target.Type == EntityTypes.PLANT && target.IsHostile(entity) && target.CanDeactive();
        }

        #region 吃豆人
        public static bool IsPacman(Entity entity)
        {
            var state = stateMachine.GetEntityState(entity);
            var subState = stateMachine.GetSubState(entity);
            return state == STATE_PACMAN && subState == PacmanState.SUBSTATE_PACMAN;
        }
        public static bool IsPacmanPanic(Entity entity)
        {
            return IsPacmanGhost(entity.Target);
        }
        public static Entity GetPacmanPanicDevourer(Entity entity)
        {
            return entity.Level.FindFirstEntityWithTheLeast(IsPacmanGhost, e => (e.Position - entity.Position).sqrMagnitude);
        }
        public static bool IsPacmanGhost(Entity entity)
        {
            return entity.ExistsAndAlive() && entity.IsEntityOf(VanillaContraptionID.devourer) && entity.IsEvoked();
        }
        public static void KillPacman(Entity entity)
        {
            if (!IsPacman(entity))
                return;
            stateMachine.SetSubState(entity, PacmanState.SUBSTATE_PACMAN_DEATH);
            var substateTimer = stateMachine.GetSubStateTimer(entity);
            substateTimer.ResetTime(60);
            entity.PlaySound(VanillaSoundID.pacmanFail);
            entity.AddBuff<TheGiantPacmanKilledBuff>();
        }
        public static Vector3 GetPacmanBlockPosition(Entity entity, int index, bool atLeft)
        {
            var level = entity.Level;
            var gridPositionOffset = pacmanBlockGridOffsets[index];
            var originX = level.GetEntityColumnX(atLeft ? 0 : (level.GetMaxColumnCount() - 1));
            var originZ = entity.Position.z;
            var xOffset = gridPositionOffset.x * level.GetGridWidth();
            var x = originX + (atLeft ? xOffset : -xOffset);
            var z = originZ + gridPositionOffset.y * level.GetGridHeight();
            var y = level.GetGroundY(x, z);
            return new Vector3(x, y, z);
        }
        #endregion


        #region 贪吃蛇
        public static bool IsSnake(Entity entity)
        {
            var state = stateMachine.GetEntityState(entity);
            var subState = stateMachine.GetSubState(entity);
            return state == STATE_SNAKE && subState == SnakeState.SUBSTATE_SNAKE;
        }
        public static bool CanAttractByBlackhole(Entity entity)
        {
            var state = stateMachine.GetEntityState(entity);
            var subState = stateMachine.GetSubState(entity);
            return state == STATE_SNAKE && (subState == SnakeState.SUBSTATE_SNAKE || subState == SnakeState.SUBSTATE_SNAKE_DEATH);
        }
        public static void KillSnake(Entity entity)
        {
            if (!IsSnake(entity))
                return;
            stateMachine.SetSubState(entity, SnakeState.SUBSTATE_SNAKE_DEATH);
            var substateTimer = stateMachine.GetSubStateTimer(entity);
            substateTimer.ResetTime(30);
            entity.PlaySound(VanillaSoundID.pacmanFail);
        }
        public static Vector3 GetSnakeBlockPosition(Entity entity, int index, bool atLeft)
        {
            var level = entity.Level;
            var lanes = level.GetMaxLaneCount();
            var col = index / lanes;
            var lane = index % lanes;
            var column = atLeft ? col : (level.GetMaxColumnCount() - 1 - col);
            var pos = level.GetEntityGridPosition(column, lane);
            if (index >= SNAKE_COMBINE_ZOMBIE_BLOCK_COUNT)
            {
                pos.y += 600;
            }
            return pos;
        }
        #endregion

        #region 常量
        private static readonly VanillaEntityPropertyMeta PROP_CRY_TIMER = new VanillaEntityPropertyMeta("CryTimer");
        private static readonly VanillaEntityPropertyMeta PROP_PHASE = new VanillaEntityPropertyMeta("Phase");
        private static readonly VanillaEntityPropertyMeta PROP_ZOMBIE_BLOCKS = new VanillaEntityPropertyMeta("ZombieBlocks");
        private static readonly VanillaEntityPropertyMeta PROP_SNAKE_TAILS = new VanillaEntityPropertyMeta("SnakeTails");
        private static readonly VanillaEntityPropertyMeta PROP_FLIP_X = new VanillaEntityPropertyMeta("FlipX");
        private static readonly VanillaEntityPropertyMeta PROP_ATTACK_FLAG = new VanillaEntityPropertyMeta("AttackFlag");
        private static readonly VanillaEntityPropertyMeta PROP_TARGET_GRID_INDEX = new VanillaEntityPropertyMeta("TargetGridIndex");
        private static readonly VanillaEntityPropertyMeta PROP_MALLEABLE = new VanillaEntityPropertyMeta("Malleable");


        private static readonly Vector3 OUTER_EYE_BULLET_OFFSET = new Vector3(70, 140, 0);
        private static readonly Vector3 INNER_EYE_BULLET_OFFSET = new Vector3(140, 140, 0);

        public const int STATE_IDLE = VanillaEntityStates.THE_GIANT_IDLE;
        public const int STATE_APPEAR = VanillaEntityStates.THE_GIANT_APPEAR;
        public const int STATE_DISASSEMBLY = VanillaEntityStates.THE_GIANT_DISASSEMBLY;
        public const int STATE_EYES = VanillaEntityStates.THE_GIANT_EYES;
        public const int STATE_ROAR = VanillaEntityStates.THE_GIANT_ROAR;
        public const int STATE_ARMS = VanillaEntityStates.THE_GIANT_ARMS;
        public const int STATE_BREATH = VanillaEntityStates.THE_GIANT_BREATH;
        public const int STATE_STUNNED = VanillaEntityStates.THE_GIANT_STUNNED;
        public const int STATE_PACMAN = VanillaEntityStates.THE_GIANT_PACMAN;
        public const int STATE_SNAKE = VanillaEntityStates.THE_GIANT_SNAKE;
        public const int STATE_FAINT = VanillaEntityStates.THE_GIANT_FAINT;
        public const int STATE_CHASE = VanillaEntityStates.THE_GIANT_CHASE;
        public const int STATE_DEATH = VanillaEntityStates.THE_GIANT_DEATH;

        public const int ANIMATION_STATE_IDLE = 0;
        public const int ANIMATION_STATE_DISASSEMBLY = 1;
        public const int ANIMATION_STATE_EYES = 2;
        public const int ANIMATION_STATE_ARMS = 3;
        public const int ANIMATION_STATE_ROAR = 4;
        public const int ANIMATION_STATE_BREATH = 5;
        public const int ANIMATION_STATE_CHASE = 6;
        public const int ANIMATION_STATE_PACMAN = 7;
        public const int ANIMATION_STATE_SNAKE = 8;
        public const int ANIMATION_STATE_STUNNED = 100;
        public const int ANIMATION_STATE_FAINT = 101;
        public const int ANIMATION_STATE_DEATH = 102;

        public const int PHASE_1 = 0;
        public const int PHASE_2 = 1;
        public const int PHASE_3 = 2;

        public const int ZOMBIE_BLOCK_COLUMNS = 2;
        public const int ZOMBIE_BLOCK_LEFT_COLUMN_START = -1;
        public const int ZOMBIE_BLOCK_MOVE_INTERVAL = 10;
        public const float DARK_HOLE_EFFECT_SCALE = 2.5f;

        public const int EYE_BULLET_INTERVAL = 30;
        public const int EYE_BULLET_COUNT = 4;
        public const float EYE_BULLET_DAMAGE_MULTIPLIER = 3f;
        public const float EYE_BULLET_SPEED = 30;
        public const float SMASH_DAMAGE_MULTIPLIER = 100;
        public const int ROAR_STUN_TIME = 150;

        public const int PACMAN_BLOCK_COUNT = 8;
        public const int PACMAN_DURATION = 300;
        public const float PACMAN_MOVE_SPEED = 3;
        public const float PACMAN_DAMAGE_MULTIPLIER = 0.01f;
        public const float PACMAN_HEAL_MULTIPLIER = 2;
        public const float PACMAN_GHOST_DAMAGE = 600;

        public const int SNAKE_BLOCK_COUNT = 8;
        public const float SNAKE_MOVE_SPEED = 6;
        public const float SNAKE_DAMAGE_MULTIPLIER = 0.03f;
        public const float SNAKE_SELF_EAT_DAMAGE = 600;
        public const float SNAKE_COMBINE_ZOMBIE_BLOCK_COUNT = 3;
        public const float SNAKE_MAX_EAT_COUNT = 8;

        public const float MAX_MALLEABLE_DAMAGE = 3000;
        public const float MALLEABLE_DECAY_PHASE_3 = 10;

        public const int CRY_INTERVAL = 300;

        public static int[] unstunnableStates = new int[]
        {
            STATE_DISASSEMBLY,
            STATE_PACMAN,
            STATE_SNAKE,
            STATE_STUNNED,
            STATE_FAINT
        };
        public static Vector2Int[] pacmanBlockGridOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(2, 1),
            new Vector2Int(2, -1)
        };
        public static Detector outerEyeBulletDetector = new TheGiantEyeDetector(true);
        public static Detector innerEyeBulletDetector = new TheGiantEyeDetector(false);
        public static Detector outerArmDetector = new TheGiantArmDetector(true);
        public static Detector innerArmDetector = new TheGiantArmDetector(false);
        public static Detector pacmanDetector = new DevourerEvokedDetector();
        private static ArrayBuffer<IEntityCollider> smashDetectBuffer = new ArrayBuffer<IEntityCollider>(8);

        #endregion 常量

        private static TheGiantStateMachine stateMachine = new TheGiantStateMachine();
    }
}
