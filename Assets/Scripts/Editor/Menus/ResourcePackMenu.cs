#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MVZ2.IO;
using MVZ2.Localization;
using MVZ2.Metas;
using MVZ2.ResourcePacks;
using MVZ2.Sprites;
using MVZ2.UI.MusicRoom;
using MVZ2.Vanilla;
using MVZ2Logic.Models;
using MVZ2Logic.Resources;
using MVZ2Logic.UI;
using Newtonsoft.Json;
using PVZEngine;
using PVZEngine.Tools.Unity;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MVZ2.Editor
{
    public static class ResourcePackMenu
    {
        #region Resolver Components
        /// <summary>
        /// 更新所有Prefab中的Resolver组件。
        /// </summary>
        public static void UpdateAllResolverComponents()
        {
            Debug.Log("Updating resolver components...");

            // 搜索所有Prefabs文件夹，以及GameContent中的prefab。
            var prefabsDirectory = Path.Combine(Application.dataPath, "Prefabs");
            var nsp = "mvz2";
            var modelDirectory = Path.Combine(GetGameContentAssetsDirectory(), nsp);
            var filePaths = new List<string>();
            PrefabHelper.SearchPrefabPaths(prefabsDirectory, filePaths);
            PrefabHelper.SearchPrefabPaths(modelDirectory, filePaths);

            // 更新所有Resolver组件。
            var addedResolvers = 0;
            foreach (string path in filePaths)
            {
                var fullPath = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
                UpdateResolverComponents(path);
            }

            // 移除所有冗余的Resolver组件。
            var removedResolvers = 0;
            foreach (string path in filePaths)
            {
                var fullPath = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
                CheckRedundantComponents(path);
            }
            Debug.Log($"Update resolver components completed: added {addedResolvers}, removed {removedResolvers} duplicated, totally {addedResolvers - removedResolvers}.");
        }
        /// <summary>
        /// 更新路径上的Prefab的Resolver组件。
        /// </summary>
        /// <param name="path">Prefab路径。</param>
        /// <returns>添加的Resolver组件数量。</returns>
        private static int UpdateResolverComponents(string path)
        {
            Debug.Log($"{path}: Starting to add resolver components.");

            var obj = PrefabHelper.ModifyPrefabStart(path);

            var transforms = obj.GetComponentsInChildren<Transform>(true);
            int addedComponents = 0;
            foreach (var transform in transforms)
            {
                // SpriteRendererResolver。
                addedComponents += UpdateResolverComponent<SpriteRenderer, SpriteRendererResolver>(transform) ? 1 : 0;
                // ImageResolver。
                addedComponents += UpdateResolverComponent<Image, ImageResolver>(transform) ? 1 : 0;
                // ParticleRendererResolver。
                addedComponents += UpdateResolverComponent<ParticleSystem, ParticleRendererResolver>(transform) ? 1 : 0;
                // MeshRendererResolver。
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

            var path = transform.GetPath();
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
                // SpriteRendererResolver。
                removedComponents += CheckRedundantComponent<SpriteRendererResolver>(transform, transform.GetComponent<SpriteRenderer>() ? 1 : 0);
                // ImageResolver。
                removedComponents += CheckRedundantComponent<ImageResolver>(transform, transform.GetComponent<Image>() ? 1 : 0);
                // ParticleRendererResolver。
                removedComponents += CheckRedundantComponent<ParticleRendererResolver>(transform, transform.GetComponent<ParticleSystem>() ? 1 : 0);
                // MeshRendererResolver。
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
        private static bool HasTextComponents(Transform transform)
        {
            return transform.GetComponent<TextMeshPro>() || transform.GetComponent<TextMeshProUGUI>();
        }
        #endregion

        #region Resource Pack
        /// <summary>
        /// 更新资源包的内容清单。
        /// </summary>
        public static void UpdateResourcePackManifest()
        {
            var packDir = Path.Combine(Application.dataPath, "Localization", "pack");

            var factories = new SpriteDataProviderFactories();
            factories.Init();

            var resourcePackManifests = new ResourcePackManifests();
            UpdateResourcePackManifestsFromLanguagePack(packDir, factories, resourcePackManifests);
            resourcePackManifests.Write(packDir);

            Debug.Log($"内置资源包清单已更新：{packDir}");
        }
        /// <summary>
        /// 更新资源包的教程资源。
        /// </summary>
        public static void UpdateResourcePackTutorialResources()
        {
            var packDir = GetResourcePackResourcesDirectory();

            var factories = new SpriteDataProviderFactories();
            factories.Init();

            if (Directory.Exists(packDir))
            {
                Directory.Delete(packDir, true);
            }

            // 将GameContent中的贴图、音效、音乐复制到ResourcePack/resources文件夹中。
            var gameContentAssetsDirectory = Path.Combine(GetGameContentAssetsDirectory());
            var resourcesDestDirectory = Path.Combine(GetResourcePackResourcesDirectory(), "assets");
            CopyDirectory(gameContentAssetsDirectory, resourcesDestDirectory, "*.*", false, ".png", ".jpg", ".mp3", ".wav", ".ogg");

            // 将语言贴图复制到ResourcePack/resources/sprites文件夹中。
            var languagePackDirectory = Path.Combine(LocalizationMenu.GetBuiltinLanguagePackDirectory(), "assets");
            CopyDirectory(languagePackDirectory, resourcesDestDirectory, "*.*" ,false, ".png", ".jpg");

            Debug.Log($"资源包资源已复制：{packDir}");


            // 更新资源包清单。
            var resourcePackManifests = new ResourcePackManifests();

            UpdateResourcePackManifestsFromLanguagePack(packDir, factories, resourcePackManifests);
            UpdateResourcePackManifestsFromGameContent(packDir, factories, resourcePackManifests);

            resourcePackManifests.Write(packDir);

            Debug.Log($"资源包清单已更新：{packDir}");

            Debug.Log("资源包资源更新完成。");
        }
        /// <summary>
        /// 更新资源包的教程模板。
        /// </summary>
        public static void UpdateResourcePackTutorialTemplates()
        {
            var packDir = Path.Combine(Application.dataPath, "Localization", "pack");
            var templatesDir = Path.Combine(Application.dataPath, "ResourcePack", "template");

            // 将*.pot文件复制到LanguagePack\templates\text_templates中。
            Debug.Log("更新资源包文本模板……");
            var potSource = LocalizationMenu.GetPoTemplateDirectory();
            var potDest = Path.Combine(templatesDir, "catalog_templates");
            CopyDirectory(potSource, potDest, "*.pot");

            // 将英文的*.po文件复制到LanguagePack\templates\text_en中。
            Debug.Log("更新资源包英文文本翻译……");
            var poSource = LocalizationMenu.GetPoDirectory("en-US");
            var poDest = Path.Combine(templatesDir, "catalog_en");
            CopyDirectory(poSource, poDest, "*.po");

            Debug.Log("资源包示例更新完成。");
        }
        private static void UpdateResourcePackManifestsFromLanguagePack(string packDir, SpriteDataProviderFactories factories, ResourcePackManifests resourcePackManifests)
        {
            var packAssetsDir = Path.Combine(packDir, "assets");
            if (!Directory.Exists(packAssetsDir))
                return;

            foreach (var nspDir in Directory.EnumerateDirectories(packAssetsDir))
            {
                var nsp = Path.GetRelativePath(packAssetsDir, nspDir);

                // 贴图。
                var spritesDir = Path.Combine(nspDir, "sprites");
                UpdateResourcePackTextureManifestFromLanguagePack(spritesDir, nsp, factories, resourcePackManifests);

                // 翻译文本。
                var catalogsDir = Path.Combine(nspDir, "catalogs");
                UpdateResourcePackCatalogManifest(catalogsDir, packAssetsDir, resourcePackManifests);
            }
        }
        private static void UpdateResourcePackTextureManifestFromLanguagePack(string spritesDir, string nsp, SpriteDataProviderFactories factories, ResourcePackManifests resourcePackManifests)
        {
            if (!Directory.Exists(spritesDir))
                return;
            var gameContentDir = GetGameContentAssetsDirectory();

            // 贴图。
            foreach (var spritePath in Directory.EnumerateFiles(spritesDir, "*.*", SearchOption.AllDirectories))
            {
                // 获取贴图路径相对于sprites文件夹的路径。
                var spriteRelativePath = Path.GetRelativePath(spritesDir, spritePath).Replace("\\", "/");
                var extension = Path.GetExtension(spriteRelativePath);
                if (extension != ".png" && extension != ".jpg")
                    continue;

                // 去掉文件名的扩展名。
                var spriteTexturePathWithoutExt = FileHelper.RemoveExtension(spriteRelativePath);

                // 分割路径的tag。
                SpriteHelper.ParseIDAndTags(spriteTexturePathWithoutExt, out var spriteIDPath, out var tags);

                // 获取包中的Texture相对路径。
                var texturePath = Path.Combine(nsp, "sprites", spriteRelativePath).Replace("\\", "/");

                // 寻找GameContent中的原贴图。
                var fullPath = Path.Combine(gameContentDir, nsp, "sprites", $"{spriteIDPath}{extension}").Replace("\\", "/");

                // 新建贴图ID。
                var id = new NamespaceID(nsp, spriteIDPath);

                var packTexture = GetResourcePackSpritesFromSpriteAsset(texturePath, fullPath, tags, id, factories);
                resourcePackManifests.textures.Add(packTexture);
            }
        }
        private static void UpdateResourcePackCatalogManifest(string catalogsDir, string packAssetsDir, ResourcePackManifests resourcePackManifests)
        {
            if (!Directory.Exists(catalogsDir))
                return;
            foreach (var langDir in Directory.EnumerateDirectories(catalogsDir))
            {
                var code = Path.GetRelativePath(catalogsDir, langDir);

                List<string> resourcePackCatalogs = new List<string>();
                foreach (var catalogPath in Directory.EnumerateFiles(langDir, "*.mo", SearchOption.AllDirectories))
                {
                    resourcePackCatalogs.Add(Path.GetRelativePath(packAssetsDir, catalogPath).Replace("\\", "/"));
                }
                var language = new ResourcePackLanguage()
                {
                    code = code,
                    catalogs = resourcePackCatalogs.ToArray(),
                };
                resourcePackManifests.languages.Add(language);
            }
        }

        private static void UpdateResourcePackManifestsFromGameContent(string packDir, SpriteDataProviderFactories factories, ResourcePackManifests resourcePackManifests)
        {
            var gameContentDir = GetGameContentAssetsDirectory();
            if (!Directory.Exists(gameContentDir))
                return;

            foreach (var nspDir in Directory.EnumerateDirectories(gameContentDir))
            {
                var nsp = Path.GetRelativePath(gameContentDir, nspDir);

                // meta
                UpdateResourcePackManifestsFromMusicMeta(gameContentDir, nsp, "musics.xml", resourcePackManifests);
                UpdateResourcePackManifestsFromSoundsMeta(gameContentDir, nsp, "sounds.xml", resourcePackManifests);
                UpdateResourcePackManifestsFromFragmentsMeta(gameContentDir, nsp, "fragments.xml", resourcePackManifests);
                UpdateResourcePackManifestsFromCharactersMeta(gameContentDir, nsp, "talkcharacters.xml", resourcePackManifests);

                // 音乐。
                var musicDir = Path.Combine(nspDir, "music");
                UpdateResourcePackAudioManifestFromGameContent(musicDir, gameContentDir, nsp, resourcePackManifests);

                // 音效。
                var soundDir = Path.Combine(nspDir, "sounds");
                UpdateResourcePackAudioManifestFromGameContent(soundDir, gameContentDir, nsp, resourcePackManifests);

                // 贴图。
                var spritesDir = Path.Combine(nspDir, "sprites");
                UpdateResourcePackTextureManifestFromGameContent(spritesDir, gameContentDir, nsp, factories, resourcePackManifests);
            }
        }
        private static void UpdateResourcePackTextureManifestFromGameContent(string spritesDir, string gameContentDir, string nsp, SpriteDataProviderFactories factories, ResourcePackManifests resourcePackManifests)
        {
            if (!Directory.Exists(spritesDir))
                return;
            foreach (var spritePath in Directory.EnumerateFiles(spritesDir, "*.*", SearchOption.AllDirectories))
            {
                // 获取贴图路径相对于sprites文件夹的路径。
                var spriteRelativePath = Path.GetRelativePath(spritesDir, spritePath).Replace("\\", "/");
                var extension = Path.GetExtension(spriteRelativePath);
                if (extension != ".png" && extension != ".jpg")
                    continue;

                // 去掉文件名的扩展名。
                var spriteTexturePathWithoutExt = FileHelper.RemoveExtension(spriteRelativePath);

                // 分割路径的tag。
                SpriteHelper.ParseIDAndTags(spriteTexturePathWithoutExt, out var spriteIDPath, out var tags);

                // 获取包中的Texture相对路径。
                var texturePath = Path.GetRelativePath(gameContentDir, spritePath).Replace("\\", "/");

                // 寻找GameContent中的贴图。
                var fullPath = spritePath.Replace("\\", "/");

                // 新建贴图ID。
                var id = new NamespaceID(nsp, spriteIDPath);

                var packTexture = GetResourcePackSpritesFromSpriteAsset(texturePath, fullPath, tags, id, factories);
                resourcePackManifests.textures.Add(packTexture);
            }
        }
        
        private static ResourcePackTexture GetResourcePackSpritesFromSpriteAsset(string texturePath, string assetPath, string[] tags, NamespaceID id, SpriteDataProviderFactories factories)
        {
            var assetRelativePath = AssetsMenu.FileToAssetPath(assetPath);
            var originTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetRelativePath);
            int filterMode = originTexture ? (int)originTexture.filterMode : (int)FilterMode.Bilinear;

            // 从Assets中导入原贴图。
            List<ResourcePackSprite> resoucePackSprites = new List<ResourcePackSprite>();
            var sprites = SpriteMenu.GetOrderedSpriteSheet(assetRelativePath, factories);
            if (sprites != null)
            {
                FillResourcePackSpritesFromSpriteAsset(sprites, originTexture, tags, resoucePackSprites);
            }

            return new ResourcePackTexture()
            {
                texture = texturePath,
                key = id.ToString(),
                filter = filterMode,
                slices = resoucePackSprites.Count > 0 ? resoucePackSprites.ToArray() : null
            };
        }
        private static void FillResourcePackSpritesFromSpriteAsset(Sprite[] sprites, Texture2D texture, string[] tags, List<ResourcePackSprite> resoucePackSprites)
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
                    var y = SpriteHelper.FromBottomToFromTopY((int)sprite.rect.y, texture.height, (int)sprite.rect.height);
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


        private static void UpdateResourcePackManifestsFromMusicMeta(string gameContentDir, string nsp, string filename, ResourcePackManifests resourcePackManifests)
        {
            var xmlDocument = LocalizationMenu.LoadMetaXmlDocument(nsp, filename);
            var musicMetaList = MusicMetaList.FromXmlNode(xmlDocument["musics"], VanillaMod.spaceName);
            foreach (var musicMeta in musicMetaList.metas)
            {
                var id = new NamespaceID(nsp, musicMeta.ID);

                var track = new ResourcePackMusicTrackV1()
                {
                    id = id.ToString(),
                    mainTrack = musicMeta.MainTrack?.ToString(),
                    subTrack = musicMeta.SubTrack?.ToString()
                };
                resourcePackManifests.musictracks.Add(track);
            }
        }
        private static void UpdateResourcePackManifestsFromSoundsMeta(string gameContentDir, string nsp, string filename, ResourcePackManifests resourcePackManifests)
        {
            var xmlDocument = LocalizationMenu.LoadMetaXmlDocument(nsp, filename);
            var metaList = SoundMetaList.FromXmlNode(xmlDocument["sounds"], VanillaMod.spaceName);
            foreach (var soundMeta in metaList.metas)
            {
                var id = new NamespaceID(nsp, soundMeta.name);

                var sound = new ResourcePackSoundV1()
                {
                    id = id.ToString(),
                    maxCount = soundMeta.maxCount,
                    priority =  soundMeta.priority,
                    samples = soundMeta.samples.Select(s => new ResourcePackSoundSampleV1() { id = s.path.ToString(), weight = s.weight}).ToArray(),
                    loopVolume = soundMeta.loopVolume,
                    loopPitchStart = soundMeta.loopPitchStart,
                    loopPitchEnd = soundMeta.loopPitchEnd,
                    loopFadeInSpeed = soundMeta.loopFadeInSpeed,
                    loopFadeOutSpeed = soundMeta.loopFadeOutSpeed,
                };
                resourcePackManifests.sounds.Add(sound);
            }
        }
        private static void UpdateResourcePackAudioManifestFromGameContent(string audioDir, string gameContentDir, string nsp, ResourcePackManifests resourcePackManifests)
        {
            foreach (var clipPath in Directory.EnumerateFiles(audioDir, "*.*", SearchOption.AllDirectories))
            {
                // 获取音频路径相对于music文件夹的路径。
                var audioRelativePath = Path.GetRelativePath(audioDir, clipPath).Replace("\\", "/");
                var extension = Path.GetExtension(audioRelativePath);
                if (extension != ".wav" && extension != ".mp3" && extension != ".ogg")
                    continue;

                // 去掉文件名的扩展名。
                var musicRelativePathWithoutExt = FileHelper.RemoveExtension(audioRelativePath);

                // 获取包中的Texture相对路径。
                var audioPathInPack = Path.GetRelativePath(gameContentDir, clipPath).Replace("\\", "/");

                // 寻找GameContent中的贴图。
                var fullPath = clipPath.Replace("\\", "/");

                // 新建贴图ID。
                var id = new NamespaceID(nsp, musicRelativePathWithoutExt);

                var importer = AssetImporter.GetAtPath(fullPath) as AudioImporter;
                bool streaming = false;
                if (importer != null)
                {
                    streaming = importer.defaultSampleSettings.loadType == AudioClipLoadType.Streaming;
                }

                var track = new ResourcePackAudioClipSampleV1()
                {
                    id = id.ToString(),
                    path = audioPathInPack,
                    streaming = streaming
                };
                resourcePackManifests.audioSamples.Add(track);
            }
        }
        private static void UpdateResourcePackManifestsFromFragmentsMeta(string gameContentDir, string nsp, string filename, ResourcePackManifests resourcePackManifests)
        {
            var xmlDocument = LocalizationMenu.LoadMetaXmlDocument(nsp, filename);
            var metaList = FragmentMetaList.FromXmlNode(xmlDocument["fragments"]);
            foreach (var fragmentMeta in metaList.metas)
            {
                var id = new NamespaceID(nsp, fragmentMeta.ID);
                var gradient = fragmentMeta.Gradient;
                bool blend = false;
                ResourcePackColorKeyV1[]? colorKeys = null;
                ResourcePackAlphaKeyV1[]? alphaKeys = null;
                if (gradient != null)
                {
                    blend = gradient.mode == GradientMode.Blend;
                    if (gradient.colorKeys.Length > 0)
                    {
                        colorKeys = new ResourcePackColorKeyV1[gradient.colorKeys.Length];
                        for (int i = 0; i < colorKeys.Length; i++)
                        {
                            var gradientColorKey = gradient.colorKeys[i];
                            colorKeys[i] = new ResourcePackColorKeyV1()
                            {
                                code = ColorUtility.ToHtmlStringRGB(gradientColorKey.color),
                                time = gradientColorKey.time
                            };
                        }
                    }
                    if (gradient.alphaKeys.Length > 0)
                    {
                        alphaKeys = new ResourcePackAlphaKeyV1[gradient.alphaKeys.Length];
                        for (int i = 0; i < alphaKeys.Length; i++)
                        {
                            var gradientAlphaKey = gradient.alphaKeys[i];
                            alphaKeys[i] = new ResourcePackAlphaKeyV1()
                            {
                                alpha = gradientAlphaKey.alpha,
                                time = gradientAlphaKey.time
                            };
                        }
                    }
                }

                var fragment = new ResourcePackFragmentV1()
                {
                    id = id.ToString(),
                    blend = blend,
                    colorKeys = colorKeys,
                    alphaKeys = alphaKeys,
                };
                resourcePackManifests.fragments.Add(fragment);
            }
        }
        private static void UpdateResourcePackManifestsFromCharactersMeta(string gameContentDir, string nsp, string filename, ResourcePackManifests resourcePackManifests)
        {
            var xmlDocument = LocalizationMenu.LoadMetaXmlDocument(nsp, filename);
            var metaList = TalkCharacterMetaList.FromXmlNode(xmlDocument["characters"], VanillaMod.spaceName);
            foreach (var characterMeta in metaList.metas)
            {
                var id = new NamespaceID(nsp, characterMeta.id);

                ResourcePackCharacterVariantV1[]? variants = null;
                if (characterMeta.variants.Count > 0)
                {
                    List<ResourcePackCharacterVariantV1> variantList = new List<ResourcePackCharacterVariantV1>();
                    foreach (var metaVariant in characterMeta.variants)
                    {
                        ResourcePackCharacterLayerV1[]? layers = null;
                        if (metaVariant.layers.Count > 0)
                        {
                            var layerList = new List<ResourcePackCharacterLayerV1>();
                            foreach (var metaLayer in metaVariant.layers)
                            {
                                if (!SpriteReference.IsValid(metaLayer.sprite))
                                    continue;
                                var layer = new ResourcePackCharacterLayerV1()
                                {
                                    sprite = metaLayer.sprite.ToString(),
                                    x = metaLayer.positionX,
                                    y = metaLayer.positionY,
                                };
                                layerList.Add(layer);
                            }
                            layers = layerList.ToArray();
                        }
                        var variant = new ResourcePackCharacterVariantV1()
                        {
                            id = metaVariant.id.ToString(),
                            width = metaVariant.width,
                            height = metaVariant.height,
                            extendLeft = metaVariant.widthExtend.x,
                            extendRight = metaVariant.widthExtend.y,
                            pivotX = metaVariant.pivotX,
                            pivotY = metaVariant.pivotY,
                            layers = layers
                        };
                        variantList.Add(variant);
                    }
                    variants = variantList.ToArray();
                }
                
                var fragment = new ResourcePackCharacterV1()
                {
                    id = id.ToString(),
                    faceRight = characterMeta.faceRight,
                    variants = variants
                };
                resourcePackManifests.characters.Add(fragment);
            }
        }

        #endregion

        private static string GetGameContentAssetsDirectory()
        {
            return Path.Combine(Application.dataPath, "GameContent", "Assets");
        }
        private static string GetResourcePackSubmoduleDirectory()
        {
            var directory = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(directory, "ResourcePack");
        }
        private static string GetResourcePackResourcesDirectory()
        {
            return Path.Combine(GetResourcePackSubmoduleDirectory(), "resources");
        }
        private static void CopyDirectory(string source, string dest, string pattern, bool delete = true, params string[] extensions)
        {
            if (!Directory.Exists(source))
                return;
            if (Directory.Exists(dest) && delete)
            {
                Directory.Delete(dest, true);
            }
            Directory.CreateDirectory(dest);
            foreach (var sourcePath in Directory.EnumerateFiles(source, pattern, SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(sourcePath);
                if (!extensions.Contains(extension))
                    continue;
                var relativePath = Path.GetRelativePath(source, sourcePath);
                var destPath = Path.Combine(dest, relativePath);
                FileHelper.ValidateDirectory(destPath);
                File.Copy(sourcePath, destPath, true);
            }
        }
        private static void CopyDirectory(string source, string dest, string pattern, bool delete = true)
        {
            if (!Directory.Exists(source))
                return;
            if (Directory.Exists(dest) && delete)
            {
                Directory.Delete(dest, true);
            }
            Directory.CreateDirectory(dest);
            foreach (var sourcePath in Directory.EnumerateFiles(source, pattern, SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(sourcePath);
                if (Path.GetExtension(sourcePath) == ".meta")
                    continue;
                var relativePath = Path.GetRelativePath(source, sourcePath);
                var destPath = Path.Combine(dest, relativePath);
                FileHelper.ValidateDirectory(destPath);
                File.Copy(sourcePath, destPath, true);
            }
        }

        private class ResourcePackManifests
        {
            public void Write(string packDir)
            {
                if (textures.Count > 0)
                {
                    ResourcePackTextures resourcePackTextures = new ResourcePackTextures()
                    {
                        textures = textures.ToArray()
                    };
                    var texuturesPath = Path.Combine(packDir, ResourcePackReaderV1.TEXTURE_MANIFEST_FILENAME);
                    WriteResourcePackManifest(texuturesPath, resourcePackTextures);
                }

                if (languages.Count > 0)
                {
                    ResourcePackLocalization resoucePackLocalization = new ResourcePackLocalization()
                    {
                        languages = languages.ToArray()
                    };
                    var localizationPath = Path.Combine(packDir, ResourcePackReaderV1.LOCALIZATION_MANIFEST_FILENAME);
                    WriteResourcePackManifest(localizationPath, resoucePackLocalization);
                }

                if (musictracks.Count > 0 || sounds.Count > 0 || audioSamples.Count > 0)
                {
                    ResourcePackMusicTrackV1[]? resourcePackMusictracks = null;
                    ResourcePackSoundV1[]? resourcePackSounds = null;
                    ResourcePackAudioClipSampleV1[]? resourcePackAudioSamples = null;
                    if (musictracks.Count > 0)
                    {
                        resourcePackMusictracks = musictracks.ToArray();
                    }
                    if (sounds.Count > 0)
                    {
                        resourcePackSounds = sounds.ToArray();
                    }
                    if (audioSamples.Count > 0)
                    {
                        resourcePackAudioSamples = audioSamples.ToArray();
                    }
                    ResourcePackAudiosV1 resourcePackAudios = new ResourcePackAudiosV1()
                    {
                        musictracks = resourcePackMusictracks,
                        sounds = resourcePackSounds,
                        samples = resourcePackAudioSamples
                    };
                    var localizationPath = Path.Combine(packDir, ResourcePackReaderV1.AUDIOS_MANIFEST_FILENAME);
                    WriteResourcePackManifest(localizationPath, resourcePackAudios);
                }

                if (fragments.Count > 0)
                {
                    ResourcePackFragmentsV1 resourcePackFragments = new ResourcePackFragmentsV1()
                    {
                        fragments = fragments.ToArray()
                    };
                    var localizationPath = Path.Combine(packDir, ResourcePackReaderV1.FRAGMENT_MANIFEST_FILENAME);
                    WriteResourcePackManifest(localizationPath, resourcePackFragments);
                }

                if (characters.Count > 0)
                {
                    ResourcePackCharactersV1 resourcePackCharacters = new ResourcePackCharactersV1()
                    {
                        characters = characters.ToArray()
                    };
                    var localizationPath = Path.Combine(packDir, ResourcePackReaderV1.CHARACTERS_MANIFEST_FILENAME);
                    WriteResourcePackManifest(localizationPath, resourcePackCharacters);
                }
            }
            private void WriteResourcePackManifest(string path, object obj)
            {
                var json = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
                WriteResourcePackManifestJson(path, json);
            }
            private void WriteResourcePackManifestJson(string path, string json)
            {
                FileHelper.ValidateDirectory(path);
                using var stream = File.Open(path, FileMode.Create);
                using var writer = new StreamWriter(stream);
                writer.Write(json);
            }
            public List<ResourcePackTexture> textures = new List<ResourcePackTexture>();
            public List<ResourcePackLanguage> languages = new List<ResourcePackLanguage>();
            public List<ResourcePackMusicTrackV1> musictracks = new List<ResourcePackMusicTrackV1>();
            public List<ResourcePackSoundV1> sounds = new List<ResourcePackSoundV1>();
            public List<ResourcePackAudioClipSampleV1> audioSamples = new List<ResourcePackAudioClipSampleV1>();
            public List<ResourcePackFragmentV1> fragments = new List<ResourcePackFragmentV1>();
            public List<ResourcePackCharacterV1> characters = new List<ResourcePackCharacterV1>();
        }
    }
}
