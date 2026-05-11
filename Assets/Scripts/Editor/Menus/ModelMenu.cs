using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MVZ2.Models;
using MVZ2Logic.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVZ2.Editor
{
    public static class ModelMenu
    {
        public static void UpdateModelElements()
        {
            Debug.Log("Updating model elements...");
            var directory = Path.Combine(Application.dataPath, "GameContent", "Assets", "mvz2", "models");
            var filePaths = new List<string>();
            SearchModelPaths(directory, filePaths);

            foreach (string path in filePaths)
            {
                UpdateModelElement(Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path)));
            }

            Debug.Log($"Update model elements completed.");
        }
        private static void SearchModelPaths(string folder, List<string> pathList)
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
        private static void UpdateModelElement(string path)
        {
            var obj = PrefabUtility.LoadPrefabContents(path);
            var model = obj.GetComponent<Model>();
            if (!model)
                return;
            model.UpdateElements();
            model.GraphicGroup.UpdateElements();
            PrefabUtility.SaveAsPrefabAsset(obj, path, out var successfully);
            PrefabUtility.UnloadPrefabContents(obj);
            Debug.Log($"{path}: {successfully}");
        }

        public static void MoveModelColliderToRoot(string path, GameObject colliderPrefab)
        {
            var obj = PrefabUtility.LoadPrefabContents(path);
            MoveModelColliderToRoot(obj, path, colliderPrefab);
            PrefabUtility.UnloadPrefabContents(obj);
        }
        private static void MoveModelColliderToRoot(GameObject obj, string path, GameObject colliderPrefab)
        {
            var model = obj.GetComponent<EntityModel>();
            if (!model)
                return;
            var collider = obj.GetComponentInChildren<Collider2D>();
            if (!collider)
                return;
            if (collider.transform.parent == model.transform)
                return;
            // 如果碰撞体的父物体不是模型本身，则将碰撞体移到模型下
            GameObject newColliderGO;
            if (!colliderPrefab)
            {
                newColliderGO = new GameObject();
            }
            else
            {
                newColliderGO = PrefabUtility.InstantiatePrefab(colliderPrefab, model.transform) as GameObject;
            }
            newColliderGO.name = "Collider";
            newColliderGO.transform.SetParent(model.transform, false);
            newColliderGO.transform.SetAsFirstSibling();
            var colliderType = collider.GetType();
            var newCollider = newColliderGO.GetComponent(colliderType);
            if (!newCollider)
            {
                newCollider = newColliderGO.AddComponent(colliderType);
            }
            EditorUtility.CopySerialized(collider, newCollider);
            Object.DestroyImmediate(collider);

            PrefabUtility.SaveAsPrefabAsset(obj, path, out var successfully);
            Debug.Log($"{path}: {successfully}");
        }

        #region 转换模型
        public static void ConvertModel()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                var path = AssetDatabase.GetAssetPath(gameObject);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning($"{path} is not a variant of the obsolete HeldModel prefab. Skipping conversion.");
                    continue;
                }
                ConvertModelAt(path);
            }
        }
        public static void ConvertModelToBase(string path, string basePath, string newBase, string boneBase, string format = "{0}")
        {
            ConvertModelAtTo(path, basePath, newBase, boneBase, format);
        }
        private static void ConvertModelAt(string path)
        {
            ConvertModelAtTo(path, MODEL_PARENT_OBSOLETE, ENTITY_MODEL_PARENT, BONE_MODEL_PATH);
        }
        private static void ConvertModelAtTo(string path, string basePath, string newBase, string boneBase, string format = "{0}")
        {
            if (!IsPrefabVariantOf(path, basePath))
            {
                Debug.LogWarning($"{path} is not a variant of the obsolete Model prefab. Skipping conversion.");
                return;
            }
            var list = new List<ModificationInfo>();
            var objectReferences = new List<InnerObjectReference>();
            GetPrefabVariantModificationsRelativeTo(path, basePath, list);
            GetPrefabInnerObjectReferences(path, objectReferences);

            var name = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path);
            var extension = Path.GetExtension(path);
            var newPath = Path.Combine(directory, string.Format(format, name)) + extension;
            CreateModelVariant(newBase, boneBase, list, objectReferences, newPath);
        }
        private static void CreateModelVariant(string basePath, string boneBase, List<ModificationInfo> modifications, List<InnerObjectReference> innerReferences, string newPath)
        {
            var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            var newGO = PrefabUtility.InstantiatePrefab(basePrefab, null) as GameObject;
            var name = Path.GetFileNameWithoutExtension(newPath);
            newGO.name = name;

            var bonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(boneBase);
            var boneGO = PrefabUtility.InstantiatePrefab(bonePrefab, newGO.transform) as GameObject;
            boneGO.name = "bone";
            ApplyModelModifications(boneGO, modifications, innerReferences);

            //PrefabUtility.SaveAsPrefabAsset(newGO, newPath, out var successfully);
            //if (successfully)
            //{
            //    Debug.Log($"Created new model variant at {newPath}.");
            //}
            //else
            //{
            //    Debug.LogWarning($"Failed to create new model variant at {newPath}.");
            //}
        }
        #endregion

        #region 应用修改
        private static void ConvertAddedGameObject(GameObject root, ModificationInfo mod)
        {
            var name = Path.GetFileNameWithoutExtension(mod.targetPath);
            var directory = Path.GetDirectoryName(mod.targetPath).Replace('\\', '/');
            var objToCreate = mod.objectReference as GameObject;
            var targetTransform = root.transform.Find(directory);
            if (!targetTransform)
            {
                Debug.LogWarning($"Transform {directory} to create GameObject {name} does not exists.");
            }
            var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(objToCreate);
            GameObject newChildGO;
            if (!prefabRoot)
            {
                newChildGO = GameObject.Instantiate(objToCreate, targetTransform) as GameObject;
            }
            else
            {
                var instantPrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot);
                newChildGO = PrefabUtility.InstantiatePrefab(instantPrefab, targetTransform) as GameObject;

                var list = new List<ModificationInfo>();
                var objectReferences = new List<InnerObjectReference>();
                GetPrefabVariantModifications(prefabRoot, list);
                GetPrefabInnerObjectReferences(prefabRoot, objectReferences);
                ApplyModelModifications(newChildGO, list, objectReferences);
            }
            newChildGO.name = name;
        }
        private static void ConvertAddedComponent(GameObject root, ModificationInfo mod)
        {
            GameObject targetGameObject = GetGameObjectByPath(root, mod.targetPath);
            if (!targetGameObject)
            {
                Debug.LogWarning($"Target GameObject not found for path: {mod.targetPath}. Skipping modification.");
                return;
            }
            var newComp = targetGameObject.AddComponent(mod.targetType);
            var oldComp = mod.objectReference as Component;
            EditorUtility.CopySerialized(oldComp, newComp);
        }
        private static void ConvertPropertyModification(GameObject root, ModificationInfo mod)
        {
            var targetObject = GetObjectByPath(root, mod.targetPath, mod.targetType);
            if (!targetObject)
            {
                Debug.LogWarning($"Target Object {mod.targetType} not found for path: {mod.targetPath}. Skipping modification.");
                return;
            }
            var seriObj = new SerializedObject(targetObject);
            var seriProp = seriObj.FindProperty(mod.propertyPath);
            if (seriProp == null)
            {
                Debug.LogWarning($"Property \"{mod.propertyPath}\" for target Component {mod.targetType} not found for path: {mod.targetPath}. Skipping modification.");
                return;
            }
            if (seriProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                seriProp.objectReferenceValue = mod.objectReference;
            }
            else
            {
                seriProp.boxedValue = mod.value;
            }
            seriObj.ApplyModifiedProperties();
        }
        private static void ConvertRemovedComponent(GameObject root, ModificationInfo mod)
        {
            var targetComponent = GetComponentByPath(root, mod.targetPath, mod.targetType);
            if (!targetComponent)
            {
                Debug.LogWarning($"Target Component {mod.targetType} not found for path: {mod.targetPath}. Skipping modification.");
                return;
            }
            Object.DestroyImmediate(targetComponent);
        }
        private static void ConvertRemovedGameObject(GameObject root, ModificationInfo mod)
        {
            GameObject targetGameObject = GetGameObjectByPath(root, mod.targetPath);
            if (!targetGameObject || targetGameObject == root)
            {
                Debug.LogWarning($"Target GameObject not found for path: {mod.targetPath}. Skipping modification.");
                return;
            }
            Object.DestroyImmediate(targetGameObject);
        }
        private static void ApplyModelModifications(GameObject go, List<ModificationInfo> modifications, List<InnerObjectReference> innerReferences)
        {
            // 处理新增GameObject
            var addedGameObjects = modifications.Where(m => m.type == ModificationType.AddedGameObject);
            foreach (var mod in addedGameObjects)
            {
                ConvertAddedGameObject(go, mod);
            }

            // 处理新增组件
            foreach (var mod in modifications.Where(m => m.type == ModificationType.AddedComponent))
            {
                ConvertAddedComponent(go, mod);
            }

            // 处理属性更改
            foreach (var mod in modifications.Where(m => m.type == ModificationType.Property))
            {
                ConvertPropertyModification(go, mod);
            }

            // 处理移除组件
            foreach (var mod in modifications.Where(m => m.type == ModificationType.RemovedComponent))
            {
                ConvertRemovedComponent(go, mod);
            }
            // 处理移除GameObject
            foreach (var mod in modifications.Where(m => m.type == ModificationType.RemovedGameObject))
            {
                ConvertRemovedGameObject(go, mod);
            }
            // 处理Object引用
            RedirectObjectReferences(go, innerReferences);
        }
        #endregion

        #region 获取修改
        private static void GetPrefabVariantModificationsRelativeTo(string variantPath, string prefabPath, List<ModificationInfo> results)
        {
            var chain = new List<string>();
            GetPrefabInheritanceChain(variantPath, chain);
            if (chain.Count <= 0)
                return;
            var relativeChain = chain.TakeWhile(c => c != prefabPath).Reverse();
            foreach (var parent in relativeChain)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(parent);
                if (prefab == null)
                    continue;
                GetPrefabVariantModifications(prefab, results);
            }
        }
        private static void GetPrefabVariantModifications(GameObject prefab, List<ModificationInfo> list)
        {
            if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.Variant)
            {
                return;
            }

            // 获取属性修改
            GetPrefabVariantPropertyModifications(prefab, list);

            // 获取添加的组件
            GetPrefabVariantAddedComponents(prefab, list);

            // 获取移除的组件
            GetPrefabVariantRemovedComponents(prefab, list);

            // 获取添加的子对象
            GetPrefabVariantAddedGameObjects(prefab, list);

            // 获取移除的对象
            GetPrefabVariantRemovedGameObjects(prefab, list);
        }
        private static void GetPrefabVariantPropertyModifications(GameObject prefab, List<ModificationInfo> list)
        {
            PropertyModification[] propertyMods = PrefabUtility.GetPropertyModifications(prefab);
            if (propertyMods != null)
            {
                foreach (var mod in propertyMods)
                {
                    if (mod.target == null)
                        continue;

                    Type targetType = mod.target.GetType();
                    var propertyPath = mod.propertyPath;

                    // 跳过根物体的 Transform
                    if (mod.target is Transform trans)
                    {
                        if (!trans.parent && mod.propertyPath == "m_Position")
                            continue;
                    }
                    // 跳过名称
                    else if (mod.target is GameObject go)
                    {
                        if (mod.propertyPath == "m_Name")
                            continue;
                    }
                    var targetPath = GetTargetRelativePath(mod.target, prefab.transform);

                    // 重复属性修改
                    list.RemoveAll(e => e.type == ModificationType.Property && e.targetPath == targetPath && e.targetType == targetType && e.propertyPath == propertyPath);

                    Object modifiedObject = GetObjectByPath(prefab, targetPath, targetType);
                    if (modifiedObject == null)
                    {
                        Debug.LogWarning($"Target Object {targetType} not found for path: {targetPath}. Skipping modification.");
                        continue;
                    }

                    var seriObj = new SerializedObject(modifiedObject);
                    var seriProp = seriObj.FindProperty(propertyPath);
                    if (seriProp == null)
                    {
                        Debug.LogWarning($"Property \"{propertyPath}\" of target Component {targetType} not found for path: {targetPath}. Skipping modification.");
                        continue;
                    }
                    var value = seriProp.boxedValue;

                    var modInfo = new ModificationInfo
                    {
                        targetPath = targetPath,
                        targetType = targetType,
                        propertyPath = propertyPath,
                        value = value,
                        objectReference = mod.objectReference,
                        type = ModificationType.Property
                    };

                    list.Add(modInfo);
                }
            }
        }
        private static void GetPrefabVariantAddedComponents(GameObject prefab, List<ModificationInfo> list)
        {
            var addedComponents = PrefabUtility.GetAddedComponents(prefab);
            foreach (var comp in addedComponents)
            {
                var componentType = comp.instanceComponent.GetType();
                var targetPath = GetTargetRelativePath(comp.instanceComponent, prefab.transform);

                var modInfo = new ModificationInfo
                {
                    targetPath = targetPath,
                    targetType = componentType,
                    objectReference = comp.instanceComponent,
                    type = ModificationType.AddedComponent
                };

                list.Add(modInfo);
            }
        }
        private static void GetPrefabVariantRemovedComponents(GameObject prefab, List<ModificationInfo> list)
        {
            var removedComponents = PrefabUtility.GetRemovedComponents(prefab);
            foreach (var comp in removedComponents)
            {
                var componentType = comp.assetComponent.GetType();
                var targetPath = GetTargetRelativePath(comp.assetComponent, prefab.transform);

                var modInfo = new ModificationInfo
                {
                    targetPath = targetPath,
                    targetType = componentType,
                    type = ModificationType.RemovedComponent
                };

                list.Add(modInfo);
            }
        }
        private static void GetPrefabVariantAddedGameObjects(GameObject prefab, List<ModificationInfo> list)
        {
            var addedGameObjects = PrefabUtility.GetAddedGameObjects(prefab);
            foreach (var go in addedGameObjects)
            {
                var targetPath = GetTargetRelativePath(go.instanceGameObject, prefab.transform);

                var modInfo = new ModificationInfo
                {
                    targetPath = targetPath,
                    objectReference = go.instanceGameObject,
                    type = ModificationType.AddedGameObject
                };

                list.Add(modInfo);
            }
        }
        private static void GetPrefabVariantRemovedGameObjects(GameObject prefab, List<ModificationInfo> list)
        {
            var removedGameObjects = PrefabUtility.GetRemovedGameObjects(prefab);
            foreach (var go in removedGameObjects)
            {
                var targetPath = GetTargetRelativePath(go.assetGameObject, prefab.transform);

                var modInfo = new ModificationInfo
                {
                    targetPath = targetPath,
                    type = ModificationType.RemovedGameObject
                };

                list.Add(modInfo);
            }
        }
        #endregion

        #region 获取内部引用
        private static void RedirectObjectReferences(GameObject root, List<InnerObjectReference> innerReferences)
        {
            foreach (var reference in innerReferences)
            {
                var targetComponent = GetComponentByPath(root, reference.targetPath, reference.targetType);
                if (!targetComponent)
                {
                    Debug.LogWarning($"Target Component {reference.targetType} not found for path: {reference.targetPath}. Skipping redirection.");
                    continue;
                }
                var seriObj = new SerializedObject(targetComponent);
                var seriProp = seriObj.FindProperty(reference.propertyPath);
                if (seriProp == null)
                {
                    Debug.LogWarning($"Property \"{reference.propertyPath}\" of target Component {reference.targetType} not found for path: {reference.targetPath}. Skipping redirection.");
                    continue;
                }
                var targetObject = GetObjectByPath(root, reference.referencePath, reference.referenceType);
                if (!targetObject)
                {
                    Debug.LogWarning($"Object reference not found for property: {reference.targetPath}/{reference.propertyPath}.");
                }
                else
                {
                    Debug.Log($"Redirected Object reference {reference.targetPath}/{reference.propertyPath} to {reference.referencePath}/{reference.referenceType}.");
                    seriProp.objectReferenceValue = targetObject;
                }
                seriObj.ApplyModifiedProperties();
            }
        }
        private static void GetPrefabInnerObjectReferences(string prefabPath, List<InnerObjectReference> results)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GetPrefabInnerObjectReferences(prefab, results);
        }
        private static void GetPrefabInnerObjectReferences(GameObject gameObject, List<InnerObjectReference> results)
        {
            foreach (var component in gameObject.GetComponentsInChildren<Component>(true))
            {
                if (component is Transform)
                    continue;
                var seriObj = new SerializedObject(component);
                var iterator = seriObj.GetIterator();
                bool visitChildren = true;
                while (iterator.NextVisible(visitChildren))
                {
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                        continue;
                    var reference = iterator.objectReferenceValue;
                    Transform trans;
                    if (reference is GameObject go)
                    {
                        trans = go.transform;
                    }
                    else if (reference is Component comp)
                    {
                        trans = comp.transform;
                    }
                    else
                    {
                        continue;
                    }
                    if (trans.IsChildOf(gameObject.transform))
                    {
                        results.Add(new InnerObjectReference()
                        {
                            targetPath = GetTargetRelativePath(component.gameObject, gameObject.transform),
                            targetType = component.GetType(),
                            propertyPath = iterator.propertyPath,
                            referencePath = GetTargetRelativePath(trans.gameObject, gameObject.transform),
                            referenceType = reference.GetType()
                        });
                    }
                }
            }
        }
        #endregion

        public static bool IsPrefabVariantOf(string prefabPath, string parentPath)
        {
            string currentPath = prefabPath;
            while (!string.IsNullOrEmpty(currentPath))
            {
                currentPath = GetBasePrefabPath(currentPath);
                if (parentPath == currentPath)
                    return true;
            }
            return false;
        }
        public static void GetPrefabInheritanceChain(string prefabPath, List<string> chain)
        {
            string currentPath = prefabPath;
            while (!string.IsNullOrEmpty(currentPath))
            {
                chain.Add(currentPath);
                currentPath = GetBasePrefabPath(currentPath);
            }
        }
        private static string GetTargetRelativePath(Object target, Transform relativeTo)
        {
            Transform transform;
            if (target is GameObject go)
            {
                transform = go.transform;
            }
            else if (target is Component comp)
            {
                transform = comp.transform;
            }
            else
            {
                return string.Empty;
            }

            var targetPaths = new List<string>();
            while (transform.parent != null)
            {
                if (relativeTo == transform)
                    break;
                targetPaths.Add(transform.gameObject.name);
                transform = transform.parent;
            }
            targetPaths.Reverse();
            return string.Join("/", targetPaths);
        }
        private static Object GetObjectByPath(GameObject root, string path, Type type)
        {
            if (type == typeof(GameObject))
            {
                return GetGameObjectByPath(root, path);
            }
            else
            {
                return GetComponentByPath(root, path, type);
            }
        }
        private static GameObject GetGameObjectByPath(GameObject root, string path)
        {
            GameObject targetGameObject = null;
            if (string.IsNullOrEmpty(path))
            {
                targetGameObject = root;
            }
            else
            {
                var transform = root.transform.Find(path);
                if (transform)
                    targetGameObject = transform.gameObject;
            }
            return targetGameObject;
        }
        private static Component GetComponentByPath(GameObject root, string path, Type type)
        {
            GameObject targetGameObject = GetGameObjectByPath(root, path);
            if (!targetGameObject)
                return null;
            return targetGameObject.GetComponent(type);
        }

        private static string GetBasePrefabPath(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return null;

            var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            if (basePrefab != null)
            {
                return AssetDatabase.GetAssetPath(basePrefab);
            }
            return null;
        }
        public const string HELD_MODEL_PARENT_OBSOLETE = "Assets/Prefabs/Models/Obsolete/HeldModel.prefab";
        public const string MODEL_PARENT_OBSOLETE = "Assets/Prefabs/Models/Obsolete/SpriteModel.prefab";
        public const string ENTITY_MODEL_PARENT = "Assets/Prefabs/Models/EntityModel.prefab";
        public const string BONE_MODEL_PATH = "Assets/GameContent/Assets/mvz2/models/bones/bone.prefab";
        private class ModificationInfo
        {
            public string targetPath;
            public Type targetType;
            public string propertyPath;
            public object value;
            public Object objectReference;
            public ModificationType type;

            public override string ToString()
            {
                switch (type)
                {
                    case ModificationType.AddedGameObject:
                        return $"Added GameObject: {targetPath}";
                    case ModificationType.AddedComponent:
                        return $"Added Component: {targetPath}/{targetType}";
                    case ModificationType.Property:
                        return $"Property: {targetPath}/{targetType}/{propertyPath} = {value}";
                    case ModificationType.RemovedComponent:
                        return $"Removed Component: {targetPath}/{targetType}";
                    case ModificationType.RemovedGameObject:
                        return $"Removed GameObject: {targetPath}";
                }
                return base.ToString();
            }
        }
        private class InnerObjectReference
        {
            public string targetPath;
            public Type targetType;
            public string propertyPath;

            public string referencePath;
            public Type referenceType;
            public override string ToString()
            {
                return $"{targetPath}/{targetType}/{propertyPath} = {referencePath}/{referenceType}";
            }
        }
        public enum ModificationType
        {
            AddedGameObject,
            RemovedGameObject,
            AddedComponent,
            RemovedComponent,
            Property
        }
    }
}
