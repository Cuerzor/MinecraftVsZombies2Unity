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
        public static string GetPath(this Transform transform)
        {
            var pathList = new List<string>();
            var trans = transform;
            while (trans)
            {
                pathList.Insert(0, trans.gameObject.name);
                trans = trans.parent;
            }
            return string.Join("/", pathList);
        }
    }
}
