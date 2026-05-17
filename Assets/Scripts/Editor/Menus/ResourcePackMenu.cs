using System.Collections.Generic;
using System.IO;
using MVZ2Logic.Models;
using MVZ2Logic.UI;
using PVZEngine.Tools.Unity;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MVZ2.Editor
{
    public static class ResourcePackMenu
    {
        public static void UpdateResolverComponents()
        {
            Debug.Log("Updating resolver components...");
            var prefabsDirectory = Path.Combine(Application.dataPath, "Prefabs");
            var modelDirectory = Path.Combine(Application.dataPath, "GameContent", "Assets", "mvz2");
            var filePaths = new List<string>();
            PrefabHelper.SearchPrefabPaths(prefabsDirectory, filePaths);
            PrefabHelper.SearchPrefabPaths(modelDirectory, filePaths);

            var addedResolvers = 0;
            foreach (string path in filePaths)
            {
                var fullPath = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
                addedResolvers += UpdateResolverComponent<SpriteRenderer, SpriteRendererResolver>(fullPath);
                addedResolvers += UpdateResolverComponent<Image, ImageResolver>(fullPath);
                addedResolvers += UpdateResolverComponent<ParticleSystem, ParticleRendererResolver>(fullPath);
            }
            var removedResolvers = 0;
            foreach (string path in filePaths)
            {
                var fullPath = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
                removedResolvers += CheckDuplicateComponents<SpriteRendererResolver>(fullPath);
                removedResolvers += CheckDuplicateComponents<ImageResolver>(fullPath);
                addedResolvers += CheckDuplicateComponents<ParticleRendererResolver>(fullPath);
            }
            Debug.Log($"Update resolver components completed: added {addedResolvers}, removed {removedResolvers} duplicated, totally {addedResolvers - removedResolvers}.");
        }
        private static void UpdateModelElement(string path)
        {
            var obj = PrefabHelper.ModifyPrefabStart(path);
            var model = obj.GetComponent<Model>();
            if (!model)
                return;
            model.UpdateElements();
            model.GraphicGroup.UpdateElements();
            var successfully = PrefabHelper.ModifyPrefabEnd(obj, path);
            Debug.Log($"{path}: {successfully}");
        }
        private static int UpdateResolverComponent<TSource, TResolver>(string path) where TSource : Component where TResolver : Component
        {
            var obj = PrefabHelper.ModifyPrefabStart(path);

            var spriteRenderers = obj.GetComponentsInChildren<TSource>(true);
            int addedComponents = 0;
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer.gameObject.GetComponent<TResolver>().Exists())
                    continue;
                spriteRenderer.gameObject.AddComponent<TResolver>();
                addedComponents++;
            }

            var successfully = PrefabHelper.ModifyPrefabEnd(obj, path);
            if (successfully && addedComponents > 0)
            {
                Debug.Log($"{path}: Added {addedComponents} components ({typeof(TResolver).Name})");
            }
            return addedComponents;
        }
        private static int CheckDuplicateComponents<T>(string path) where T : Component
        {
            var obj = PrefabHelper.ModifyPrefabStart(path);

            var components = obj.GetComponentsInChildren<T>(true);
            int removedComponents = 0;
            foreach (var component in components)
            {
                if (!component)
                    continue;
                var resolvers = component.gameObject.GetComponents<T>();
                if (resolvers.Length <= 1)
                    continue;
                for (int i = resolvers.Length - 1; i >= 1; i--)
                {
                    var resolver = resolvers[i];
                    Object.DestroyImmediate(resolver, true);
                    removedComponents++;
                }
            }

            var successfully = PrefabHelper.ModifyPrefabEnd(obj, path);
            if (successfully && removedComponents > 0)
            {
                Debug.Log($"{path}: Removed {removedComponents} duplicate components ({typeof(T).Name})");
            }
            return removedComponents;
        }
        private static int RemoveComponents<T>(string path) where T : Component
        {
            var obj = PrefabHelper.ModifyPrefabStart(path);

            var components = obj.GetComponentsInChildren<T>(true);
            int removedComponents = 0;
            foreach (var component in components)
            {
                if (!component)
                    continue;
                var resolvers = component.gameObject.GetComponents<T>();
                if (resolvers.Length <= 0)
                    continue;
                for (int i = resolvers.Length - 1; i >= 0; i--)
                {
                    var resolver = resolvers[i];
                    Object.DestroyImmediate(resolver, true);
                    removedComponents++;
                }
            }

            var successfully = PrefabHelper.ModifyPrefabEnd(obj, path);
            if (successfully && removedComponents > 0)
            {
                Debug.Log($"{path}: Removed {removedComponents} components ({typeof(T).Name})");
            }
            return removedComponents;
        }
    }
}
