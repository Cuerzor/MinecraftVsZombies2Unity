#nullable enable

using System;
using System.IO;
using System.Text;
using MVZ2.GameContent.Effects;
using MVZ2.IO;
using MVZ2.Managers;
using MVZ2.Modding;
using MVZ2.OldSaves;
using MVZ2.Saves;
using MVZ2.Vanilla.Saves;
using MVZ2Logic.Saves;
using MVZ2Logic.Serialization;
using NBTUtility;
using UnityEngine;
using UnityEngine.Android;

namespace MVZ2.OldSave
{
    public class OldSaveDataImporter : IOldSaveDataImporter
    {
        public void Import()
        {
            var save = ImportGlobalSave();
            if (save == null)
                return;
            ImportOldSaveData(save);
        }
        public OldGlobalSave? ImportGlobalSave()
        {
            var platform = Application.platform;
            var thisDirectory = Application.persistentDataPath;
            string oldDirectory;
            if (platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer)
            {
                var cuerzorDirectory = Path.GetDirectoryName(thisDirectory);
                var localLowDirectory = Path.GetDirectoryName(cuerzorDirectory);
                oldDirectory = Path.Combine(localLowDirectory, "Tocmic Studio", "MinecraftVSZombies2");
            }
            else
            {
                return null;
            }
            var oldSaveDirectory = Path.Combine(oldDirectory, "saves");
            if (!Directory.Exists(oldSaveDirectory))
                return null;
            var usersPath = Path.Combine(oldSaveDirectory, "users.dat");
            OldUserList? userList = ImportOldUserList(usersPath);
            if (userList == null)
                return null;

            OldSaveData[] saveDatas = new OldSaveData[userList.usernames.Length];
            for (int i = 0; i < userList.usernames.Length; i++)
            {
                var userName = userList.usernames[i];
                if (string.IsNullOrEmpty(userName))
                    continue;
                var userDirectory = Path.Combine(oldSaveDirectory, $"user{i}");
                var loader = new OldSaveDataLoader(userDirectory);
                var saveData = loader.Load();
                saveDatas[i] = saveData;
            }
            return new OldGlobalSave()
            {
                userList = userList,
                saveDatas = saveDatas
            };
        }
        public static OldUserList? ImportOldUserList(string path)
        {
            if (!File.Exists(path))
                return null;
            try
            {
                using var stream = File.Open(path, FileMode.Open);
                return OldUserList.ReadStream(stream);
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occured while importing users.dat from the old version: {e}");
                return null;
            }
        }
        #region 导入旧存档
        private void ImportOldSaveData(OldGlobalSave saveData)
        {
            if (saveData == null)
                return;

            // 用户列表
            var newUserDataList = ImportOldUserList(saveData.userList);
            if (newUserDataList == null)
                return;

            var saveDataMetaPath = Main.SaveManager.GetUserListPath();
            FileHelper.ValidateDirectoryExists(saveDataMetaPath);
            var seriUserList = newUserDataList.ToSerializable();
            var userListJson = seriUserList.ToBson();
            Main.FileManager.WriteStringFile(saveDataMetaPath, userListJson);

            // 存档。
            var saveDatas = ImportOldUserData(newUserDataList, saveData.saveDatas);
            if (saveDatas == null)
                return;

            var spaceName = Main.BuiltinNamespace;
            for (int i = 0; i < saveDatas.Length; i++)
            {
                var modSaveData = saveDatas[i];
                if (modSaveData == null)
                    continue;
                Main.SaveManager.WriteSaveData(i, spaceName, modSaveData);
            }
        }
        private UserDataList? ImportOldUserList(OldUserList userList)
        {
            if (userList?.usernames == null)
                return null;

            // 用户列表
            var newUserDataList = new UserDataList(SaveManager.MAX_USER_COUNT);
            newUserDataList.CurrentUserIndex = -1;
            for (int i = 0; i < userList.usernames.Length; i++)
            {
                var name = userList.usernames[i];
                if (string.IsNullOrEmpty(name))
                    continue;
                var userItem = newUserDataList.Create(i);
                if (userItem != null)
                {
                    userItem.Username = name;
                    if (newUserDataList.CurrentUserIndex < 0)
                    {
                        newUserDataList.CurrentUserIndex = i;
                    }
                }
            }
            if (newUserDataList.CurrentUserIndex < 0)
            {
                newUserDataList.CurrentUserIndex = 0;
            }
            return newUserDataList;
        }
        private VanillaSaveData[]? ImportOldUserData(UserDataList userList, OldSaveData[] userDatas)
        {
            if (userDatas == null)
                return null;

            ModInfo? vanillaMod = null;
            foreach (var mod in Main.ModManager.GetAllModInfos())
            {
                if (mod == null || mod.Logic == null)
                    continue;
                if (mod.Namespace != Main.BuiltinNamespace)
                    continue;
                vanillaMod = mod;
                break;
            }
            if (vanillaMod == null)
                return null;

            var allUsers = userList.GetAllUsers();
            VanillaSaveData[] results = new VanillaSaveData[allUsers.Length];
            for (int i = 0; i < allUsers.Length; i++)
            {
                var user = allUsers[i];
                if (user == null)
                    continue;
                var oldData = userDatas[i];
                if (oldData == null)
                    continue;
                var data = vanillaMod.Logic?.CreateSaveData();
                if (data is not VanillaSaveData vanilla)
                    continue;
                OldSaveDataConvertor.ImportUserDataFromOld(vanilla, oldData);
                results[i] = vanilla;
            }
            return results;
        }
        #endregion
        public static MainManager Main => MainManager.Instance;
    }
}
