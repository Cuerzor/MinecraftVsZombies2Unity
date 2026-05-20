#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using MVZ2.IO;
using MVZ2.Localization;
using MVZ2.ResourcePacks;
using MVZ2.Sprites;
using MVZ2Logic.Models;
using MVZ2Logic.UI;
using Newtonsoft.Json;
using PVZEngine;
using PVZEngine.Tools.Unity;
using TMPro;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MVZ2.Editor
{
    public static class ResourcePackMenu
    {
        public static void UpdateAllResolverComponents()
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
                UpdateResolverComponents(path);
            }
            var removedResolvers = 0;
            foreach (string path in filePaths)
            {
                var fullPath = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
                CheckRedundantComponents(path);
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
        private static int UpdateResolverComponent<TSource, TResolver>(string path, Predicate<TSource>? predicate = null) where TSource : Component where TResolver : Component
        {
            var obj = PrefabHelper.ModifyPrefabStart(path);

            var spriteRenderers = obj.GetComponentsInChildren<TSource>(true);
            int addedComponents = 0;
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (predicate != null && !predicate(spriteRenderer))
                    continue;
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
        private static int UpdateResolverComponents(string path)
        {
            Debug.Log($"{path}: Starting to add resolver components.");

            var obj = PrefabHelper.ModifyPrefabStart(path);

            var transforms = obj.GetComponentsInChildren<Transform>(true);
            int addedComponents = 0;
            foreach (var transform in transforms)
            {
                addedComponents += UpdateResolverComponent<SpriteRenderer, SpriteRendererResolver>(transform) ? 1 : 0;
                addedComponents += UpdateResolverComponent<Image, ImageResolver>(transform) ? 1 : 0;
                addedComponents += UpdateResolverComponent<ParticleSystem, ParticleRendererResolver>(transform) ? 1 : 0;
                if (!HasTextComponents(transform))
                {
                    addedComponents += UpdateResolverComponent<MeshRenderer, MeshRendererResolver>(transform) ? 1 : 0;
                }
            }

            var successfully = PrefabHelper.ModifyPrefabEnd(obj, path);
            if (successfully && addedComponents > 0)
            {
                Debug.Log($"{path}: Added {addedComponents} resolver components.");
            }
            return addedComponents;
        }
        private static bool UpdateResolverComponent<TSource, TResolver>(Transform transform) where TSource : Component where TResolver : Component
        {
            var component = transform.GetComponent<TSource>();
            if (!component)
                return false;
            if (transform.GetComponent<TResolver>().Exists())
                return false;
            transform.gameObject.AddComponent<TResolver>();

            var pathList = new List<string>();
            var trans = transform;
            while (trans)
            {
                pathList.Insert(0, trans.gameObject.name);
                trans = trans.parent;
            }
            var path = string.Join("/", pathList);
            Debug.Log($"{path}: Added resolver components ({typeof(TResolver).Name})");
            return true;
        }
        private static int CheckRedundantComponents(string path)
        {
            Debug.Log($"{path}: Starting to remove redundant resolver components.");

            var obj = PrefabHelper.ModifyPrefabStart(path);

            var transforms = obj.GetComponentsInChildren<Transform>(true);
            int removedComponents = 0;
            foreach (var transform in transforms)
            {
                removedComponents += CheckRedundantComponent<SpriteRendererResolver>(transform, transform.GetComponent<SpriteRenderer>() ? 1 : 0);
                removedComponents += CheckRedundantComponent<ImageResolver>(transform, transform.GetComponent<Image>() ? 1 : 0);
                removedComponents += CheckRedundantComponent<ParticleRendererResolver>(transform, transform.GetComponent<ParticleSystem>() ? 1 : 0);
                removedComponents += CheckRedundantComponent<MeshRendererResolver>(transform, transform.GetComponent<MeshRenderer>() && !HasTextComponents(transform) ? 1 : 0);
            }

            var successfully = PrefabHelper.ModifyPrefabEnd(obj, path);
            if (successfully && removedComponents > 0)
            {
                Debug.Log($"{path}: Removed {removedComponents} redundant resolver components.");
            }
            return removedComponents;
        }
        private static int CheckRedundantComponent<T>(Transform transform, int targetCount) where T : Component
        {
            var component = transform.GetComponent<T>();
            if (!component)
                return 0;
            var resolvers = component.gameObject.GetComponents<T>();
            if (resolvers.Length <= targetCount)
                return 0;
            int count = 0;
            for (int i = resolvers.Length - 1; i >= targetCount; i--)
            {
                var resolver = resolvers[i];
                Object.DestroyImmediate(resolver, true);
                count++;
            }
            return count;
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
        private static bool HasTextComponents(Transform transform)
        {
            return transform.GetComponent<TextMeshPro>() || transform.GetComponent<TextMeshProUGUI>();
        }

        public static void UpdateResourcePackManifest()
        {
            // 根据语言包内的已有贴图和项目内的已有贴图生成spriteManifest.json，并将其保存到Localization\pack\assets\mvz2\en-US中。
            var path = LocalizationMenu.GetResourcePackDirectory();

            var packDir = Path.Combine(Application.dataPath, "Localization", "pack");

            var packAssetsDir = Path.Combine(packDir, "assets");
            var databaseDir = Path.Combine("Assets", "GameContent", "Assets");

            var factories = new SpriteDataProviderFactories();
            factories.Init();

            List<ResourcePackTexture> resoucePackTextures = new List<ResourcePackTexture>();
            List<ResourcePackLanguage> resourcePackLanguages = new List<ResourcePackLanguage>();

            foreach (var nspDir in Directory.EnumerateDirectories(packAssetsDir))
            {
                var nsp = Path.GetRelativePath(packAssetsDir, nspDir);

                // 贴图。
                var spritesDir = Path.Combine(nspDir, "sprites");
                foreach (var spritePath in Directory.EnumerateFiles(spritesDir, "*.png", SearchOption.AllDirectories))
                {
                    List<ResourcePackSprite> resoucePackSprites = new List<ResourcePackSprite>();

                    // 获取贴图路径相对于sprites文件夹的路径。
                    var spriteRelativePath = Path.GetRelativePath(spritesDir, spritePath).Replace("\\", "/");

                    // 去掉文件名的扩展名。
                    var extension = Path.GetExtension(spriteRelativePath);
                    var spriteTexturePathWithoutExt = Path.ChangeExtension(spriteRelativePath, string.Empty).TrimEnd('.');

                    // 分割路径的tag。
                    SpriteHelper.ParseIDAndTags(spriteTexturePathWithoutExt, out var spriteIDPath, out var tags);

                    // 寻找Assets中的原贴图。
                    var originSpriteFilename = $"{spriteIDPath}{extension}";
                    var originSpritePath = Path.Combine(databaseDir, nsp, "sprites", originSpriteFilename).Replace("\\", "/");

                    // 新建贴图ID。
                    var id = new NamespaceID(nsp, spriteIDPath);

                    // 从Assets中导入原贴图。
                    var sprites = SpriteMenu.GetOrderedSpriteSheet(originSpritePath, factories);
                    if (sprites != null)
                    {
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            var sprite = sprites[i];
                            ResourcePackSprite packSprite;
                            if (sprites.Length <= 1)
                            {
                                packSprite = new ResourcePackSprite()
                                {
                                    tags = tags,
                                    single = true,
                                    pivotX = sprite.pivot.x / sprite.rect.width,
                                    pivotY = sprite.pivot.y / sprite.rect.height,
                                };
                            }
                            else
                            {
                                var y = SpriteHelper.FromBottomToFromTopY((int)sprite.rect.y, sprite.texture.height, (int)sprite.rect.height);
                                packSprite = new ResourcePackSprite()
                                {
                                    tags = tags,
                                    single = false,
                                    x = (int)sprite.rect.x,
                                    y = y,
                                    width = (int)sprite.rect.width,
                                    height = (int)sprite.rect.height,
                                    pivotX = sprite.pivot.x / sprite.rect.width,
                                    pivotY = sprite.pivot.y / sprite.rect.height,
                                };
                            }
                            resoucePackSprites.Add(packSprite);
                        }
                    }

                    // 获取包中的Texture相对路径。
                    var texturePath = Path.GetRelativePath(packAssetsDir, spritePath).Replace("\\", "/");
                    var originTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(originSpritePath);
                    int filterMode = originTexture ? (int)originTexture.filterMode : (int)FilterMode.Bilinear;
                    resoucePackTextures.Add(new ResourcePackTexture()
                    {
                        texture = texturePath,
                        key = id.ToString(),
                        filter = filterMode,
                        slices = resoucePackSprites.ToArray()
                    });
                }


                // 翻译文本。
                var catalogsDir = Path.Combine(nspDir, "catalogs");
                foreach (var langDir in Directory.EnumerateDirectories(catalogsDir))
                {
                    var code = Path.GetRelativePath(catalogsDir, langDir);

                    List<string> resourcePackCatalogs = new List<string>();
                    foreach (var catalogPath in Directory.EnumerateFiles(langDir, "*.mo", SearchOption.AllDirectories))
                    {
                        resourcePackCatalogs.Add(Path.GetRelativePath(packAssetsDir, catalogPath).Replace("\\", "/"));
                    }
                    resourcePackLanguages.Add(new ResourcePackLanguage()
                    {
                        code = code,
                        catalogs = resourcePackCatalogs.ToArray(),
                    });
                }
            }

            ResourcePackLocalization resoucePackLocalization = new ResourcePackLocalization()
            {
                languages = resourcePackLanguages.ToArray()
            };

            ResourcePackTextures textures = new ResourcePackTextures()
            {
                textures = resoucePackTextures.ToArray()
            };
            var texturesJson = JsonConvert.SerializeObject(textures, Newtonsoft.Json.Formatting.Indented);
            var manifestPath = Path.Combine(packDir, ResourcePackReaderV1.TEXTURE_MANIFEST_FILENAME);
            WriteResourcePackManifest(manifestPath, texturesJson);

            var localizationJson = JsonConvert.SerializeObject(resoucePackLocalization, Newtonsoft.Json.Formatting.Indented);
            var localizationPath = Path.Combine(packDir, ResourcePackReaderV1.LOCALIZATION_MANIFEST_FILENAME);
            WriteResourcePackManifest(localizationPath, localizationJson);

            Debug.Log($"语言包清单已更新：{manifestPath}");
        }
        private static void WriteResourcePackManifest(string path, string json)
        {
            FileHelper.ValidateDirectory(path);
            using var stream = File.Open(path, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(json);
        }
        public static void UpdateResourcePackDemo()
        {
            // 根据语言包内的已有贴图和项目内的已有贴图生成spriteManifest.json，并将其保存到Localization\pack\assets\mvz2\en-US中。
            UpdateResourcePackManifest();

            // 将spriteManifest.json复制到LanguagePack\templates中。
            Debug.Log("更新语言包贴图清单……");
            var sourceManifestPath = Path.Combine(Application.dataPath, "Localization", "pack", ResourcePackReaderV1.TEXTURE_MANIFEST_FILENAME);
            var templatesDir = Path.Combine(Application.dataPath, "ResourcePack", "templates");
            if (File.Exists(sourceManifestPath))
            {
                var destPath = Path.Combine(templatesDir, ResourcePackReaderV1.TEXTURE_MANIFEST_FILENAME);
                FileHelper.ValidateDirectory(destPath);
                File.Copy(sourceManifestPath, destPath, true);
            }

            // 将*.pot文件复制到LanguagePack\templates\text_templates中。
            Debug.Log("更新语言包文本模板……");
            CopyDirectory(LocalizationMenu.GetPoTemplateDirectory(), Path.Combine(templatesDir, "text_templates"), "*.pot");

            // 将英文的*.po文件复制到LanguagePack\templates\text_en中。
            Debug.Log("更新语言包英文文本翻译……");
            CopyDirectory(LocalizationMenu.GetPoDirectory("en-US"), Path.Combine(templatesDir, "text_en"), "*.po");

            Debug.Log("语言包示例更新完成。");
        }
        private static void CopyDirectory(string source, string dest, string pattern)
        {
            if (!Directory.Exists(source))
                return;
            if (Directory.Exists(dest))
            {
                Directory.Delete(dest, true);
            }
            Directory.CreateDirectory(dest);
            foreach (var sourcePath in Directory.EnumerateFiles(source, pattern, SearchOption.AllDirectories))
            {
                if (Path.GetExtension(sourcePath) == ".meta")
                    continue;
                var relativePath = Path.GetRelativePath(source, sourcePath);
                var destPath = Path.Combine(dest, relativePath);
                FileHelper.ValidateDirectory(destPath);
                File.Copy(sourcePath, destPath, true);
            }
        }
    }
}
