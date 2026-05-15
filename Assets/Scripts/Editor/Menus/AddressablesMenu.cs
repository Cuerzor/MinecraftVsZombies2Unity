using System.Collections.Generic;
using System.IO;
using System.Linq;
using PVZEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MVZ2.Editor
{
    public static class AddressablesMenu
    {
        public static void UpdateAddressables()
        {
            Debug.Log("Updating Addressables...");

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            HashSet<string> dependencies = new HashSet<string>();

            var mainGroup = settings.DefaultGroup;
            var languagePackGroup = settings.groups.FirstOrDefault(g => g.name == "language_packs");
            ClearAddressables(mainGroup);
            ClearAddressables(languagePackGroup);

            var mainDirectory = Path.Combine(Application.dataPath, "GameContent", "Assets");
            var assetMarker = new MainAssetMarker();
            UpdateAddressableGroup(mainGroup, mainDirectory, assetMarker, dependencies);

            var sceneDirectory = Path.Combine(Application.dataPath, "GameContent", "Scenes");
            var sceneMarker = new SceneMarker();
            UpdateAddressableGroup(mainGroup, sceneDirectory, sceneMarker, dependencies);

            var languagePackDirectory = LocalizationMenu.GetLanguagePackDirectory();
            var languagePackMarker = new LanguagePackMarker();
            UpdateAddressableGroup(languagePackGroup, languagePackDirectory, languagePackMarker, dependencies);

            Debug.Log($"Update Addressables completed.");

            EditorUtility.ClearProgressBar();
        }
        private static void UpdateAddressableGroup(AddressableAssetGroup group, string directory, AssetMarker marker, HashSet<string> dependencies)
        {
            var filePaths = new List<string>();
            SearchFilesForUpdate(directory, filePaths);
            List<string> markedList = new List<string>();
            foreach (string path in filePaths)
            {
                if (!MarkResource(group, directory, path, marker))
                    continue;
                var assetPath = AssetsMenu.FileToAssetPath(path);
                foreach (var dep in AssetDatabase.GetDependencies(assetPath))
                {
                    dependencies.Add(dep);
                }
                markedList.Add(path);
                EditorUtility.DisplayProgressBar("Marking resources as addressables...", path, (float)markedList.Count / filePaths.Count);
            }
        }
        private static void UpdateDependenciesGroup(AddressableAssetGroup group, IEnumerable<string> assetPaths)
        {
            var totalCount = assetPaths.Count();
            List<string> markedList = new List<string>();
            foreach (string assetPath in assetPaths)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (!asset || asset.hideFlags.HasFlag(HideFlags.DontSave))
                    continue;
                AddressableAssetEntry entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    entry = settings.CreateOrMoveEntry(guid, group, postEvent: false);
                }
                markedList.Add(assetPath);
                EditorUtility.DisplayProgressBar("Marking resources as addressables...", assetPath, (float)markedList.Count / totalCount);
            }
        }
        private static void SearchFilesForUpdate(string folder, List<string> pathList)
        {
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

            int progress = 0;
            foreach (var file in files)
            {
                string extension = Path.GetExtension(file);
                if (extension == ".meta")
                    continue;

                if (extension == ".unity")
                {
                    if (EditorBuildSettings.scenes.Any(s => s.path == file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)))
                        continue;
                }
                pathList.Add(file);
                EditorUtility.DisplayProgressBar("Checking markable assets...", folder, progress / (float)files.Length);
            }
        }

        private static void ClearAddressables(AddressableAssetGroup group)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entries = group.entries.ToArray();
            foreach (var asset in entries)
            {
                group.RemoveAssetEntry(asset, postEvent: false);
            }
        }
        private static bool MarkResource(AddressableAssetGroup group, string rootDirectory, string filePath, AssetMarker marker)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var assetPath = AssetsMenu.FileToAssetPath(filePath);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            var relativePath = Path.GetRelativePath(rootDirectory, filePath);
            if (!marker.Filter(entry, relativePath))
                return false;
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group, postEvent: false);
            }
            marker.MarkAsset(entry, relativePath);
            return true;
        }
    }
    public abstract class AssetMarker
    {
        public virtual bool Filter(AddressableAssetEntry entry, string relativePath) => true;
        public abstract void MarkAsset(AddressableAssetEntry entry, string relativePath);
    }
    public class MainAssetMarker : AssetMarker
    {
        private string[] GetEntryLabels(string type)
        {
            var labels = new List<string>();
            labels.Add("Main");
            switch (type.ToLower())
            {
                case "metas":
                    labels.Add("Meta");
                    break;
                case "lang":
                    labels.Add("Language");
                    break;
                case "music":
                    labels.Add("Music");
                    break;
                case "sounds":
                    labels.Add("Sound");
                    break;
                case "models":
                    labels.Add("Model");
                    break;
                case "spritemanifests":
                    labels.Add("SpriteManifest");
                    break;
                case "sprites":
                    labels.Add("Sprite");
                    break;
                case "mapmodels":
                    labels.Add("MapModel");
                    break;
                case "areamodels":
                    labels.Add("AreaModel");
                    break;
            }
            return labels.ToArray();
        }
        public override bool Filter(AddressableAssetEntry entry, string relativePath)
        {
            var splitedPaths = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (splitedPaths.Contains("init"))
                return false;
            if (splitedPaths.Length < 2)
                return false;
            return true;
        }
        public override void MarkAsset(AddressableAssetEntry entry, string relativePath)
        {
            var splitedPaths = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string keyRoot = splitedPaths[0];
            string nsp = null;
            if (splitedPaths.Length >= 3)
            {
                nsp = splitedPaths[0];
                var type = splitedPaths[1];
                keyRoot = Path.Combine(nsp, type);

                var labels = GetEntryLabels(type);
                foreach (var label in labels)
                {
                    entry.SetLabel(label, true, postEvent: false);
                }
            }
            var keyPath = Path.GetRelativePath(keyRoot, relativePath).Replace("\\", "/");
            var idPath = Path.ChangeExtension(keyPath, "").TrimEnd('.');
            entry.SetAddress(new NamespaceID(nsp, idPath).ToString(), postEvent: false);
        }
    }
    public class LanguagePackMarker : AssetMarker
    {
        public override void MarkAsset(AddressableAssetEntry entry, string relativePath)
        {
            var idPath = Path.GetFileNameWithoutExtension(relativePath);
            entry.SetAddress(idPath, postEvent: false);
            entry.SetLabel("LanguagePack", true, postEvent: false);
        }
    }
    public class SceneMarker : AssetMarker
    {
        public override void MarkAsset(AddressableAssetEntry entry, string relativePath)
        {
            var idPath = Path.GetFileNameWithoutExtension(relativePath);
            entry.SetAddress(idPath, postEvent: false);
            entry.SetLabel("Scene", true, postEvent: false);
        }
    }
    public class DummyMarker : AssetMarker
    {
        public override void MarkAsset(AddressableAssetEntry entry, string relativePath)
        {
        }
    }
}
