using MVZ2.GameContent.Difficulties;
using MVZ2.GameContent.Maps;
using MVZ2.GameContent.Stages;
using MVZ2.OldSaves;
using MVZ2.Vanilla.Saves;
using MVZ2.Vanilla.Stats;
using MVZ2.Vanilla.Unlocks;
using MVZ2Logic;
using MVZ2Logic.Saves;
using MVZ2Logic.Stats;
using MVZ2Logic.Unlocks;
using PVZEngine;

namespace MVZ2.OldSave
{
    public static class OldSaveDataConvertor
    {
        public static void ImportUserDataFromOld(VanillaSaveData saveData, OldSaveData oldData)
        {
            if (oldData == null)
                return;
            ImportMainUserDataFromOld(saveData, oldData.main);
            ImportAchievementsUserDataFromOld(saveData, oldData.achievements);
            ImportEndlessUserDataFromOld(saveData, oldData.endless);
        }
        private static void ImportMainUserDataFromOld(VanillaSaveData saveData, OldSaveDataMain oldData)
        {
            if (oldData == null)
                return;

            // 关卡进度。
            for (int i = 0; i < oldLevelIDList.Length; i++)
            {
                if (oldData.currentLevel > i)
                {
                    var unlockName = LogicUnlockNames.GetLevelClearUnlock(oldLevelIDList[i]);
                    saveData.Unlock(unlockName);
                }
            }
            if ((oldData.currentLevel == 12 && oldData.sidePlot == 1) || oldData.currentLevel > 12)
            {
                saveData.Unlock(VanillaUnlockNames.enteredDream);
            }
            // 关卡难度。
            for (int i = 0; i < oldData.levelDifficulties.Length; i++)
            {
                var difficulty = oldData.levelDifficulties[i];
                var index = i + 1;
                if (index >= oldLevelIDList.Length || index > oldData.currentLevel)
                    continue;
                var stageID = oldLevelIDList[index];
                NamespaceID diff = VanillaDifficulties.hard;
                switch (difficulty)
                {
                    case 0:
                        diff = VanillaDifficulties.easy;
                        break;
                    case 1:
                        diff = VanillaDifficulties.normal;
                        break;
                }
                saveData.AddLevelDifficultyRecord(stageID, diff);
            }
            // 上一张地图
            if (oldData.lastMap == "mvz2:dream_map")
            {
                saveData.LastMapID = VanillaMapID.dream;
            }
            else
            {
                saveData.LastMapID = VanillaMapID.halloween;
            }
            // 金钱
            saveData.SetMoney(oldData.money);
            // 卡槽
            if (oldData.cardSlots >= 7)
                saveData.Unlock(LogicUnlockNames.blueprintSlot1);
            if (oldData.cardSlots >= 8)
                saveData.Unlock(LogicUnlockNames.blueprintSlot2);
            if (oldData.cardSlots >= 9)
                saveData.Unlock(LogicUnlockNames.blueprintSlot3);
            if (oldData.cardSlots >= 10)
                saveData.Unlock(LogicUnlockNames.blueprintSlot4);
            // 星之碎片槽
            if (oldData.starshardSlots >= 4)
                saveData.Unlock(LogicUnlockNames.starshardSlot1);
            if (oldData.starshardSlots >= 5)
                saveData.Unlock(LogicUnlockNames.starshardSlot2);
            // 升级
            if ((oldData.upgrades & 1) != 0)
            {
                saveData.Unlock(VanillaUnlockNames.infectenser);
            }
            if ((oldData.upgrades & 2) != 0)
            {
                saveData.Unlock(VanillaUnlockNames.forcePad);
            }
            // 梦魇
            if (oldData.nightmare)
            {
                saveData.Unlock(VanillaUnlockNames.dreamIsNightmare);
            }
            // 教程
            if (oldData.moneyFound)
            {
                saveData.Unlock(VanillaUnlockNames.money);
            }
            if (oldData.starshardLearnt)
            {
                saveData.Unlock(VanillaUnlockNames.starshard);
            }
            if (oldData.triggerLearnt)
            {
                saveData.Unlock(VanillaUnlockNames.trigger);
            }
        }
        private static void ImportAchievementsUserDataFromOld(VanillaSaveData saveData, OldSaveDataAchievements oldData)
        {
            if (oldData == null)
                return;
            if (oldData.earned == null || oldData.earned.Length < 1)
                return;
            var earned = oldData.earned[0];
            for (int i = 0; i < oldAchievementIDList.Length; i++)
            {
                if ((earned & (1 << i)) != 0)
                {
                    saveData.Unlock(oldAchievementIDList[i]);
                }
            }
        }
        private static void ImportEndlessUserDataFromOld(VanillaSaveData saveData, OldSaveDataEndless oldData)
        {
            if (oldData == null || oldData.flags == null)
                return;
            foreach (var pair in oldData.flags)
            {
                if (pair.Value == null)
                    continue;
                var stageID = VanillaStageNames.halloweenEndless;
                switch (pair.Key)
                {
                    case "dream":
                        stageID = VanillaStageNames.dreamEndless;
                        break;
                }
                saveData.SetCurrentEndlessFlag(stageID, pair.Value.current);
                saveData.SetStat(LogicStats.CATEGORY_MAX_ENDLESS_FLAGS.Path, new NamespaceID(Global.BuiltinNamespace, stageID), pair.Value.max);
            }
        }
        private static readonly string[] oldLevelIDList = new string[]
        {
            VanillaStageNames.prologue, // 0

            VanillaStageNames.halloween1,
            VanillaStageNames.halloween2,
            VanillaStageNames.halloween3,
            VanillaStageNames.halloween4,
            VanillaStageNames.halloween5,
            VanillaStageNames.halloween6,
            VanillaStageNames.halloween7,
            VanillaStageNames.halloween8,
            VanillaStageNames.halloween9,
            VanillaStageNames.halloween10,
            VanillaStageNames.halloween11, // 11

            VanillaStageNames.dream1,
            VanillaStageNames.dream2,
            VanillaStageNames.dream3,
            VanillaStageNames.dream4,
            VanillaStageNames.dream5,
            VanillaStageNames.dream6,
            VanillaStageNames.dream7,
            VanillaStageNames.dream8,
            VanillaStageNames.dream9,
            VanillaStageNames.dream10,
            VanillaStageNames.dream11, // 22
        };
        private static readonly string[] oldAchievementIDList = new string[]
        {
            VanillaUnlockNames.doubleTrouble,
            VanillaUnlockNames.ghostBuster,
            VanillaUnlockNames.rickrollDrown,
            VanillaUnlockNames.returnToSender,
        };
    }
}
