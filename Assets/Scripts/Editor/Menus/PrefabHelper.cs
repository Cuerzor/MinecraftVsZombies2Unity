using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    public static class PrefabHelper
    {
        public static void SearchPrefabPaths(string folder, List<string> pathList)
        {
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string extension = Path.GetExtension(file);
                if (extension != ".prefab")
                    continue;
                pathList.Add(file);
            }
        }
        public static GameObject ModifyPrefabStart(string path)
        {
            return PrefabUtility.LoadPrefabContents(path);
        }
        public static bool ModifyPrefabEnd(GameObject obj ,string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path, out var successfully);
            PrefabUtility.UnloadPrefabContents(obj);
            return successfully;
        }
    }
}
