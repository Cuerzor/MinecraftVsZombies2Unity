using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FindMissingScriptsWindow : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<FindMissingScriptsWindow>("Missing Scripts");
    }

    private void OnGUI()
    {
        GUILayout.Label("场景与项目检查", EditorStyles.boldLabel);

        if (GUILayout.Button("扫描当前场景中的 Missing 脚本", GUILayout.Height(35)))
        {
            FindInScene();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Prefab 检查", EditorStyles.boldLabel);

        if (GUILayout.Button("扫描全项目 Prefab 中的 Missing 脚本", GUILayout.Height(35)))
        {
            FindInPrefabs();
        }
    }

    // 扫描场景
    private static void FindInScene()
    {
        GameObject[] goArray = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int missingCount = 0;

        foreach (GameObject go in goArray)
        {
            missingCount += CheckGameObject(go, $"场景物体: {GetGameObjectPath(go)}", go);
        }

        Debug.Log($"<color=green>【场景扫描完毕】共找到 {missingCount} 处 Missing 脚本。</color>");
    }

    // 扫描全项目 Prefab
    private static void FindInPrefabs()
    {
        // 寻找项目中所有的 .prefab 文件
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int missingCount = 0;
        int prefabCount = 0;

        // 显示进度条
        for (int i = 0; i < allPrefabGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(allPrefabGuids[i]);
            EditorUtility.DisplayProgressBar("正在扫描 Prefab", assetPath, (float)i / allPrefabGuids.Length);

            // 加载 Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                prefabCount++;
                // 检查 Prefab 本身及其所有子物体
                Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    int found = CheckGameObject(child.gameObject, $"Prefab 资源: {assetPath} (物体: {child.name})", prefab);
                    if (found > 0)
                    {
                        missingCount += found;
                        // 如果有改动，标记该 Prefab 为“脏数据”，以便保存
                        EditorUtility.SetDirty(prefab);
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets(); // 保存所有被标记修改的 Prefab

        Debug.Log($"<color=yellow>【Prefab 扫描完毕】共检查了 {prefabCount} 个 Prefab，发现 {missingCount} 处 Missing 脚本。</color>");
    }

    // 核心检查逻辑
    private static int CheckGameObject(GameObject go, string logContext, Object assetToSelect)
    {
        int count = 0;
        Component[] components = go.GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null) // 组件为空说明丢失
            {
                count++;
                // 打印错误日志，点击该日志可以直接定位到对应的 Prefab 资产或场景物体
                Debug.LogError($"发现 Missing 脚本！位置 -> {logContext}", assetToSelect);
            }
        }
        return count;
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }
}