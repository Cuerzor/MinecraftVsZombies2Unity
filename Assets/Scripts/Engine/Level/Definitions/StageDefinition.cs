using System;
using System.Collections.Generic;
using PVZEngine.Base;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace PVZEngine.Definitions
{
    public abstract class StageDefinition : Definition
    {
        public StageDefinition(string nsp, string name) : base(nsp, name)
        {
            SetProperty(EngineStageProps.WAVES_PER_FLAG, 10);
            SetProperty(EngineLevelProps.RECHARGE_SPEED, 1);
        }
        public void Setup(LevelEngine level)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.Setup(level);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行Setup时出现错误：{ex}");
                }
            }
            OnSetup(level);
        }
        public void Start(LevelEngine level)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.Start(level);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行Start时出现错误：{ex}");
                }
            }
            OnStart(level);
        }
        public void Update(LevelEngine level)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.Update(level);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行Update时出现错误：{ex}");
                }
            }
            OnUpdate(level);
        }
        public void PrepareForBattle(LevelEngine level)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.PrepareForBattle(level);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行PrepareForBattle时出现错误：{ex}");
                }
            }
            OnPrepareForBattle(level);
        }
        public void PostWave(LevelEngine level, int wave)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.PostWave(level, wave);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行PostWave时出现错误：{ex}");
                }
            }
            OnPostWave(level, wave);
        }
        public void PostHugeWaveEvent(LevelEngine level)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.PostHugeWaveEvent(level);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行PostHugeWaveEvent时出现错误：{ex}");
                }
            }
            OnPostHugeWave(level);
        }
        public void PostFinalWaveEvent(LevelEngine level)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.PostFinalWaveEvent(level);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行PostFinalWaveEvent时出现错误：{ex}");
                }
            }
            OnPostFinalWave(level);
        }
        public void PostEnemySpawned(Entity entity)
        {
            foreach (var b in behaviours)
            {
                try
                {
                    b.PostEnemySpawned(entity);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"关卡行为{b}在执行PostEnemySpawned时出现错误：{ex}");
                }
            }
            OnPostEnemySpawned(entity);
        }
        public virtual void OnSetup(LevelEngine level) { }
        public virtual void OnStart(LevelEngine level) { }
        public virtual void OnUpdate(LevelEngine level) { }
        public virtual void OnPrepareForBattle(LevelEngine level) { }
        public virtual void OnPostWave(LevelEngine level, int wave) { }
        public virtual void OnPostHugeWave(LevelEngine level) { }
        public virtual void OnPostFinalWave(LevelEngine level) { }
        public virtual void OnPostEnemySpawned(Entity entity) { }
        protected void AddBehaviour(StageBehaviour behaviour)
        {
            behaviours.Add(behaviour);
        }
        public bool HasBehaviour<T>() where T : StageBehaviour
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour is T tBehaviour)
                    return true;
            }
            return false;
        }
        public T GetBehaviour<T>() where T : StageBehaviour
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour is T tBehaviour)
                    return tBehaviour;
            }
            return null;
        }
        public sealed override string GetDefinitionType() => EngineDefinitionTypes.STAGE;
        private List<StageBehaviour> behaviours = new List<StageBehaviour>();
    }
}
