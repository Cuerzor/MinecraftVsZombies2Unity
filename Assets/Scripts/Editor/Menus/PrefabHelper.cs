using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static List<string> SortPrefabsByNesting(List<string> prefabPaths)
        {
            // 1. 过滤 & 去重
            var validPaths = prefabPaths
                .Where(p => !string.IsNullOrEmpty(p) && p.EndsWith(".prefab"))
                .Distinct()
                .ToList();

            var pathSet = new HashSet<string>(validPaths);
            var adjacency = new Dictionary<string, List<string>>(); // 被依赖方 -> 依赖它的
            var inDegree = new Dictionary<string, int>();          // 入度 = 有多少个 Prefab 依赖它（反向语义）

            foreach (var path in validPaths)
            {
                adjacency[path] = new List<string>();
                inDegree[path] = 0;
            }

            // 2. 建边：A 依赖 B 时，边方向为 B -> A
            foreach (var path in validPaths)
            {
                // 只用直接依赖，避免间接依赖污染图
                var deps = AssetDatabase.GetDependencies(path, false);
                var added = new HashSet<string>();
                foreach (var dep in deps)
                {
                    if (dep == path) continue;                // 排除自己
                    if (!pathSet.Contains(dep)) continue;     // 只保留列表内的 Prefab
                    if (!added.Add(dep)) continue;            // 防止重复依赖导致入度多加

                    // 关键：被依赖的 dep 指向依赖方 path
                    adjacency[dep].Add(path);
                    inDegree[path]++;
                }
            }

            // 3. Kahn 拓扑排序：从入度为 0（没有被任何 Prefab 依赖，即最底层）开始
            var queue = new Queue<string>();
            foreach (var kv in inDegree)
                if (kv.Value == 0) queue.Enqueue(kv.Key);

            var sorted = new List<string>(validPaths.Count);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sorted.Add(current);
                foreach (var neighbor in adjacency[current])
                {
                    if (--inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            // 4. 兜底：循环依赖时追加剩余节点
            if (sorted.Count < validPaths.Count)
            {
                var remaining = validPaths.Where(p => !sorted.Contains(p));
                sorted.AddRange(remaining);
                Debug.LogWarning("检测到循环依赖，部分 Prefab 无法完成拓扑排序，已追加到末尾。");
            }

            return sorted;
        }
    }
}
