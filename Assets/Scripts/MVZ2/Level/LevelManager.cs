using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MVZ2.IO;
using MVZ2.Managers;
using MVZ2.Scenes;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Level;
using MVZ2Logic;
using PVZEngine;
using PVZEngine.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MVZ2.Level
{
    public class LevelManager : MonoBehaviour, ILevelManager
    {
        public LevelController GetLevel()
        {
            return controller;
        }
        public void InitLevel(NamespaceID areaID, NamespaceID stageID, float beginningDelay = 0)
        {
            if (!controller)
                return;
            Main.SaveManager.SaveModDatas();
            controller.StartStageID = stageID;
            controller.StartAreaID = areaID;
            if (HasLevelState(stageID))
            {
                LoadLevel(areaID, stageID);
            }
            else
            {
                controller.InitLevel(Main.Game, areaID, stageID);
                controller.StartLevelIntro(beginningDelay);
            }
        }
        #region 关卡存读
        public void SaveLevel()
        {
            if (!controller)
                return;
            var stageID = controller.GetStartStageID();
            var path = GetLevelStatePath(stageID);
            FileHelper.ValidateDirectory(path);
            var seri = controller.SaveGame();
            var json = seri.ToBson();
            Main.FileManager.WriteJsonFile(path, json);

            UpdateCurrentEndlessFlags(stageID, controller.GetCurrentFlag());
        }
        public void LoadLevel(NamespaceID areaID, NamespaceID stageID)
        {
            if (!controller)
                return;
            if (!HasLevelState(stageID))
                return;
            try
            {
                controller.SetActive(true);
                var seri = LoadLevelStateData(stageID);
                controller.LoadGame(seri, Main.Game, areaID, stageID);
                UpdateCurrentEndlessFlags(stageID, controller.GetCurrentFlag());
            }
            catch (Exception e)
            {
                controller.ShowLevelErrorLoadingDialog(e);
            }
        }
        public SerializableLevelController LoadLevelStateData(NamespaceID stageID)
        {
            var path = GetLevelStatePath(stageID);
            var json = Main.FileManager.ReadJsonFile(path);
            return SerializeHelper.FromBson<SerializableLevelController>(json);
        }
        public bool HasLevelState(NamespaceID stageID)
        {
            var path = GetLevelStatePath(stageID);
            return File.Exists(path);
        }
        public void RemoveLevelState(NamespaceID stageID)
        {
            var path = GetLevelStatePath(stageID);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            UpdateCurrentEndlessFlags(stageID, 0);
        }
        public string GetLevelStatePath(NamespaceID stageID)
        {
            var userIndex = Main.SaveManager.GetCurrentUserIndex();
            var dir = Main.SaveManager.GetUserModSaveDataDirectory(userIndex, stageID.SpaceName);
            return Path.Combine(dir, "level", $"{stageID.Path}.lvl");
        }
        public LevelDataIdentifierList GetLevelStateIdentifierList()
        {
            var mods = Main.ModManager.GetAllModInfos();
            return new LevelDataIdentifierList(mods.Select(m => new LevelDataIdentifier(m.Namespace, m.LevelDataVersion)));
        }
        public string GetStageName(NamespaceID stageID)
        {
            var meta = Main.Game.GetStageDefinition(stageID);
            if (meta == null)
                return Main.LanguageManager._p(VanillaStrings.CONTEXT_LEVEL_NAME, VanillaStrings.LEVEL_NAME_UNKNOWN);
            var levelName = Main.LanguageManager._p(VanillaStrings.CONTEXT_LEVEL_NAME, meta.GetLevelName());
            var dayNumber = meta.GetDayNumber();
            if (dayNumber > 0)
            {
                levelName = Main.LanguageManager._p(VanillaStrings.CONTEXT_LEVEL_NAME, VanillaStrings.LEVEL_NAME_DAY_TEMPLATE, levelName, dayNumber);
            }
            return levelName;
        }
        public string GetStageName(LevelEngine level)
        {
            string name = level?.GetLevelName();
            if (string.IsNullOrEmpty(name))
                name = VanillaStrings.LEVEL_NAME_UNKNOWN;
            var levelName = Main.LanguageManager._p(VanillaStrings.CONTEXT_LEVEL_NAME, name);
            int dayNumber = level.GetDayNumber();
            if (dayNumber > 0)
            {
                levelName = Main.LanguageManager._p(VanillaStrings.CONTEXT_LEVEL_NAME, VanillaStrings.LEVEL_NAME_DAY_TEMPLATE, levelName, dayNumber);
            }
            if (level.IsEndless() && level.CurrentFlag > 0)
            {
                levelName = Main.LanguageManager._p(VanillaStrings.CONTEXT_LEVEL_NAME, VanillaStrings.LEVEL_NAME_ENDLESS_FLAGS_TEMPLATE, levelName, level.CurrentFlag);
            }
            return levelName;
        }
        #endregion
        public Vector3 LawnToTrans(Vector3 pos)
        {
            if (controller)
            {
                return controller.LawnToTrans(pos);
            }
            pos *= LawnToTransScale;
            return new Vector3(pos.x, pos.z + pos.y, pos.z);
        }
        public Vector3 TransToLawn(Vector3 pos)
        {
            if (controller)
            {
                return controller.TransToLawn(pos);
            }
            Vector3 vector = new Vector3(pos.x, pos.y - pos.z, pos.z);
            vector *= TransToLawnScale;
            return vector;
        }
        public async Task GotoLevelSceneAsync()
        {
            var sceneName = "Level";
            var oldScene = Scene.GetSceneInstance(sceneName);
            var oldController = controller;

            var newScene = (await Scene.LoadSceneAsync(sceneName, LoadSceneMode.Additive));
            foreach (var go in newScene.Scene.GetRootGameObjects())
            {
                var ctrl = go.GetComponent<LevelController>();
                if (ctrl)
                {
                    controller = ctrl;
                    break;
                }
            }
            if (controller)
            {
                controller.SetActive(false);
            }
            if (oldScene.Scene.IsValid())
            {
                await Scene.UnloadSceneAsync(oldScene);
            }
        }

        Coroutine ILevelManager.GotoLevelSceneCoroutine()
        {
            return Main.CoroutineManager.ToCoroutine(GotoLevelSceneAsync());
        }
        public async Task ExitLevelSceneAsync()
        {
            var sceneName = "Level";
            if (!Scene.IsSceneLoaded(sceneName))
                return;
            await Scene.UnloadSceneAsync(sceneName);
            controller = null;
        }
        private void UpdateCurrentEndlessFlags(NamespaceID stageID, int flags)
        {
            var stageDef = Main.Game.GetStageDefinition(stageID);
            if (stageDef != null && stageDef.IsEndless())
            {
                Main.SaveManager.SetCurrentEndlessFlag(stageID, flags);
            }
        }
        public const int CURRENT_DATA_VERSION = 2;
        public float LawnToTransScale => 1 / transToLawnScale;
        public float TransToLawnScale => transToLawnScale;
        public MainManager Main => main;
        public SceneLoadingManager Scene => main.SceneManager;
        [SerializeField]
        private MainManager main;
        [SerializeField]
        private float transToLawnScale = 100;
        private LevelController controller;
    }
    [Serializable]
    public class LevelDataIdentifierList
    {
        public LevelDataIdentifierList(IEnumerable<LevelDataIdentifier> identifiers)
        {
            this.identifiers.AddRange(identifiers);
        }
        public bool Compare(LevelDataIdentifierList list)
        {
            if (list == null)
                return false;
            if (list.identifiers.Count != identifiers.Count)
                return false;
            return list.identifiers.Any(i => identifiers.Contains(i));
        }
        public override int GetHashCode()
        {
            var hash = 0;
            foreach (var id in identifiers)
            {
                hash = hash * 31 + id.GetHashCode();
            }
            return hash;
        }
        public List<LevelDataIdentifier> identifiers = new List<LevelDataIdentifier>();
    }
    [Serializable]
    public class LevelDataIdentifier
    {
        public LevelDataIdentifier(string name, int version)
        {
            spaceName = name;
            dataVersion = version;
        }
        public string spaceName;
        public int dataVersion;

        public override bool Equals(object obj)
        {
            return obj is LevelDataIdentifier identifier &&
                   spaceName == identifier.spaceName &&
                   dataVersion == identifier.dataVersion;
        }

        public override int GetHashCode()
        {
            var hash = spaceName.GetHashCode();
            hash = hash * 31 + dataVersion.GetHashCode();
            return hash;
        }

        public static bool operator ==(LevelDataIdentifier left, LevelDataIdentifier right)
        {
            return EqualityComparer<LevelDataIdentifier>.Default.Equals(left, right);
        }

        public static bool operator !=(LevelDataIdentifier left, LevelDataIdentifier right)
        {
            return !(left == right);
        }
    }
}
