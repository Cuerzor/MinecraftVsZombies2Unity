using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MukioI18n;
using MVZ2.IO;
using MVZ2.Localization;
using MVZ2.Metas;
using MVZ2.Sprites;
using MVZ2.TalkData;
using MVZ2.Vanilla;
using MVZ2Logic.Localization;
using MVZ2Logic.Resources;
using Newtonsoft.Json;
using PVZEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MVZ2.Editor
{
    public static class LocalizationMenu
    {
        public static void CompressLanguagePack()
        {
            UpdateResourcePackManifest();
            MoveMOFiles();

            var path = GetLanguagePackDirectory();
            var dirPath = Path.Combine(Application.dataPath, "Localization", "pack");
            var destPath = Path.Combine(path, "builtin.bytes");
            LanguageManager.CompressLanguagePack(dirPath, destPath);
            AssetDatabase.Refresh();
            Debug.Log("Langauge Pack Compressed.");
        }
        public static void UpdateAlmanacTranslations()
        {
            var potGenerator = new MukioPotGenerator("MinecraftVSZombies2", "Cuerzor");

            var spaceName = VanillaMod.spaceName;

            var almanacDocument = LoadMetaXmlDocument(spaceName, "almanac.xml");
            var entitiesDocument = LoadMetaXmlDocument(spaceName, "entities.xml");
            var talkcharacterDocument = LoadMetaXmlDocument(spaceName, "talkcharacters.xml");
            var artifactsDocument = LoadMetaXmlDocument(spaceName, "artifacts.xml");
            var blueprintsDocument = LoadMetaXmlDocument(spaceName, "blueprints.xml");

            var almanacEntryList = AlmanacMetaList.FromXmlNode(almanacDocument["almanac"], spaceName);
            var entitiesList = EntityMetaList.FromXmlNode(spaceName, entitiesDocument["entities"], spaceName);
            var characterList = TalkCharacterMetaList.FromXmlNode(talkcharacterDocument["characters"], spaceName);
            var artifactsList = ArtifactMetaList.FromXmlNode(artifactsDocument["artifacts"], spaceName);
            var blueprintsList = BlueprintMetaList.FromXmlNode(spaceName, blueprintsDocument["blueprints"], spaceName);

            var entitiesReference = "Entity meta file";
            var characterReference = "Character meta file";
            var artifactsReference = "Artifact meta file";
            var blueprintsReference = "Blueprints meta file";
            foreach (var tag in almanacEntryList.tags)
            {
                AddTranslation(potGenerator, tag.name, almanacReference, $"Name for almanac tag {tag.id}", LogicStrings.CONTEXT_ALMANAC_TAG_NAME);
                AddTranslation(potGenerator, tag.description, almanacReference, $"Description for almanac tag {tag.id}", LogicStrings.CONTEXT_ALMANAC_TAG_DESCRIPTION);
            }
            foreach (var tagEnum in almanacEntryList.enums)
            {
                foreach (var value in tagEnum.values)
                {
                    AddTranslation(potGenerator, value.name, almanacReference, $"Name for almanac tag enum value {value.value} of enum {tagEnum.id}", LogicStrings.CONTEXT_ALMANAC_TAG_ENUM_NAME);
                    AddTranslation(potGenerator, value.description, almanacReference, $"Description for almanac tag enum value {value.value} of enum {tagEnum.id}", LogicStrings.CONTEXT_ALMANAC_TAG_ENUM_DESCRIPTION);
                }
            }
            foreach (var category in almanacEntryList.categories)
            {
                var categoryName = category.name;
                foreach (var group in category.groups)
                {
                    AddTranslation(potGenerator, group.name, almanacReference, $"Name for almanac group {group.id}", LogicStrings.CONTEXT_ALMANAC_GROUP_NAME);
                    foreach (var entry in group.entries)
                    {
                        AddAlmanacEntryTranslation(potGenerator, entry, categoryName);
                    }
                }
                foreach (var entry in category.entries)
                {
                    AddAlmanacEntryTranslation(potGenerator, entry, categoryName);
                }
            }
            foreach (var meta in entitiesList.counters)
            {
                var id = new NamespaceID(spaceName, meta.ID);
                AddTranslation(potGenerator, meta.Name, entitiesReference, $"Name for entity counter {id}", LogicStrings.CONTEXT_ENTITY_COUNTER_NAME);
            }
            foreach (var meta in entitiesList.metas)
            {
                var id = new NamespaceID(spaceName, meta.ID);
                AddTranslation(potGenerator, meta.Name, entitiesReference, $"Name for entity {id}", LogicStrings.CONTEXT_ENTITY_NAME);
                AddTranslation(potGenerator, meta.Tooltip, entitiesReference, $"Tooltip for entity {id}", LogicStrings.CONTEXT_ENTITY_TOOLTIP);
                AddTranslation(potGenerator, meta.DeathMessage, entitiesReference, $"Death message for entity {id}", LogicStrings.CONTEXT_DEATH_MESSAGE);
            }
            foreach (var meta in characterList.metas)
            {
                var id = new NamespaceID(spaceName, meta.id);
                AddTranslation(potGenerator, meta.name, characterReference, $"Name for character {id}", LogicStrings.CONTEXT_CHARACTER_NAME);
            }
            foreach (var meta in artifactsList.metas)
            {
                var id = new NamespaceID(spaceName, meta.ID);
                AddTranslation(potGenerator, meta.Name, artifactsReference, $"Name for artifact {id}", LogicStrings.CONTEXT_ARTIFACT_NAME);
                AddTranslation(potGenerator, meta.Tooltip, artifactsReference, $"Tooltip for artifact {id}", LogicStrings.CONTEXT_ARTIFACT_TOOLTIP);
            }
            foreach (var error in blueprintsList.Errors)
            {
                AddTranslation(potGenerator, error.Message, blueprintsReference, $"Message for blueprint error {error.ID}", LogicStrings.CONTEXT_BLUEPRINT_ERROR);
            }
            foreach (var option in blueprintsList.Options)
            {
                AddTranslation(potGenerator, option.Name, blueprintsReference, $"Name for blueprint option {option.ID}", LogicStrings.CONTEXT_BLUEPRINT_OPTION_NAME);
                AddTranslation(potGenerator, option.Tooltip, blueprintsReference, $"Tooltip for blueprint option {option.ID}", LogicStrings.CONTEXT_BLUEPRINT_OPTION_TOOLTIP);
            }
            foreach (var entity in blueprintsList.Entities)
            {
                AddTranslation(potGenerator, entity.Name, blueprintsReference, $"Name for entity blueprint {entity.ID}", LogicStrings.CONTEXT_ENTITY_NAME);
                AddTranslation(potGenerator, entity.Tooltip, blueprintsReference, $"Tooltip for entity blueprint {entity.ID}", LogicStrings.CONTEXT_ENTITY_TOOLTIP);
            }
            potGenerator.WriteOut(GetPoTemplatePath("almanac.pot"));
            Debug.Log("Almanac Translations Updated.");
        }
        public static void UpdateAllTranslations()
        {
            UpdateGeneralTranslations();
            UpdateAlmanacTranslations();
            UpdateTalkTranslations();
        }
        public static void UpdateGeneralTranslations()
        {
            var potGenerator = new MukioPotGenerator("MinecraftVSZombies2", "Cuerzor");

            // 场景
            var active = SceneManager.GetActiveScene().path;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var group in settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (!entry.IsScene)
                        continue;
                    var scenePath = entry.AssetPath;

                    var sc = EditorSceneManager.OpenScene(scenePath);
                    if (sc == null)
                        continue;

                    var objects = sc.GetRootGameObjects();
                    foreach (var item in objects)
                    {
                        SearchObject(item.transform, potGenerator);
                    }
                }
            }
            // 代码
            TranslateMsgAttributeFinder.FindAll(potGenerator);

            // 统计
            var spaceName = VanillaMod.spaceName;
            // 关卡
            var stageDocument = LoadMetaXmlDocument(spaceName, "stages.xml");
            var stageList = StageMetaList.FromXmlNode(stageDocument["stages"], spaceName);
            var stageReference = "Stage meta file";
            foreach (var meta in stageList.metas)
            {
                var id = new NamespaceID(spaceName, meta.ID);
                AddTranslation(potGenerator, meta.Name, stageReference, $"Name for stage {id}", LogicStrings.CONTEXT_LEVEL_NAME);
            }
            // 统计
            var statsDocument = LoadMetaXmlDocument(spaceName, "stats.xml");
            var statsEntryList = StatMetaList.FromXmlNode(statsDocument["stats"], spaceName);
            var statsReference = "Stats meta file";
            foreach (var category in statsEntryList.categories)
            {
                var id = new NamespaceID(spaceName, category.ID);
                AddTranslation(potGenerator, category.Name, statsReference, $"Name for stat category {id}", LogicStrings.CONTEXT_STAT_CATEGORY);
            }
            foreach (var entry in statsEntryList.entries)
            {
                var id = new NamespaceID(spaceName, entry.ID);
                AddTranslation(potGenerator, entry.Name, statsReference, $"Name for stat entry {id}", LogicStrings.CONTEXT_STAT_ENTRY);
            }
            // 成就
            var achievementsDocument = LoadMetaXmlDocument(spaceName, "achievements.xml");
            var achievementList = AchievementMetaList.FromXmlNode(achievementsDocument["achievements"], spaceName);
            var achievementsReference = "Achievements meta file";
            foreach (var achievement in achievementList.metas)
            {
                var id = new NamespaceID(spaceName, achievement.ID);
                AddTranslation(potGenerator, achievement.Name, achievementsReference, $"Name for achievement {id}", LogicStrings.CONTEXT_ACHIEVEMENT);
                AddTranslation(potGenerator, achievement.Description, achievementsReference, $"Description for achievement {id}", LogicStrings.CONTEXT_ACHIEVEMENT);
            }
            // 音乐
            var musicsDocument = LoadMetaXmlDocument(spaceName, "musics.xml");
            var musicsList = MusicMetaList.FromXmlNode(musicsDocument["musics"], spaceName);
            var musicsReference = "Music meta file";
            foreach (var music in musicsList.metas)
            {
                var id = new NamespaceID(spaceName, music.ID);
                AddTranslation(potGenerator, music.Name, musicsReference, $"Name for music {id}", LogicStrings.CONTEXT_MUSIC_NAME);
                AddTranslation(potGenerator, music.Source, musicsReference, $"Source for music {id}", LogicStrings.CONTEXT_MUSIC_SOURCE);
                AddTranslation(potGenerator, music.Origin, musicsReference, $"Origin for music {id}", LogicStrings.CONTEXT_MUSIC_ORIGIN);
                AddTranslation(potGenerator, music.Author, musicsReference, $"Author for music {id}", LogicStrings.CONTEXT_MUSIC_AUTHOR);
                AddTranslation(potGenerator, music.Description, musicsReference, $"Description for music {id}", LogicStrings.CONTEXT_MUSIC_DESCRIPTION);
            }
            // 档案
            var archiveDocument = LoadMetaXmlDocument(spaceName, "archive.xml");
            var archiveList = ArchiveMetaList.FromXmlNode(archiveDocument["archive"], spaceName);
            var archiveReference = "Archive meta file";
            foreach (var tag in archiveList.Tags)
            {
                var id = new NamespaceID(spaceName, tag.ID);
                AddTranslation(potGenerator, tag.Name, archiveReference, $"Name for archive tag {id}", LogicStrings.CONTEXT_ARCHIVE_TAG_NAME);
            }
            // 网格
            {
                var gridDocument = LoadMetaXmlDocument(spaceName, "grids.xml");
                var gridList = GridMetaList.FromXmlNode(gridDocument["grids"], spaceName);
                var gridReference = "Grid meta file";
                foreach (var error in gridList.errors)
                {
                    var id = new NamespaceID(spaceName, error.ID);
                    AddTranslation(potGenerator, error.Message, gridReference, $"Message for grid error {id}", LogicStrings.CONTEXT_ADVICE);
                }
            }
            // 商店
            var storeDocument = LoadMetaXmlDocument(spaceName, "store.xml");
            var storeList = StoreMetaList.FromXmlNode(storeDocument["store"], spaceName);
            var storeReference = "Store meta file";
            foreach (var chatGroup in storeList.Chats)
            {
                foreach (var chat in chatGroup.Chats)
                {
                    AddTranslation(potGenerator, chat.Text, storeReference, $"Store chat from character {chatGroup.Character}", LogicStrings.CONTEXT_STORE_TALK);
                }
            }
            foreach (var product in storeList.Products)
            {
                var id = new NamespaceID(spaceName, product.ID);
                foreach (var talk in product.Talks)
                {
                    AddTranslation(potGenerator, talk.Text, storeReference, $"Store item description of {id} by character {talk.Character}", LogicStrings.CONTEXT_STORE_TALK);
                }
                foreach (var stage in product.Stages)
                {
                    AddTranslation(potGenerator, stage.Text, storeReference, $"Store item text of {id}");
                }
            }
            // 制作人员名单
            {
                var creditsDocument = LoadMetaXmlDocument(spaceName, "credits.xml");
                var creditsList = CreditMetaList.FromXmlNode(creditsDocument["credits"], spaceName);
                var creditsReference = "Credits meta file";
                foreach (var category in creditsList.categories)
                {
                    AddTranslation(potGenerator, category.Name, creditsReference, $"Name for credits category {category.Name}", LogicStrings.CONTEXT_CREDITS_CATEGORY);
                    foreach (var entry in category.Entries)
                    {
                        AddTranslation(potGenerator, entry, creditsReference, $"Name for credit staff {category.Name}", LogicStrings.CONTEXT_STAFF_NAME);
                    }
                }
            }
            // 命令
            {
                var document = LoadMetaXmlDocument(spaceName, "commands.xml");
                var metaList = CommandMetaList.FromXmlNode(document["commands"], spaceName);
                var reference = "Commands meta file";
                foreach (var meta in metaList.metas)
                {
                    var id = new NamespaceID(spaceName, meta.ID);
                    AddTranslation(potGenerator, meta.Description, reference, $"Description for command \"{id}\"", LogicStrings.CONTEXT_COMMAND_DESCRIPTION);

                    foreach (var variant in meta.Variants)
                    {
                        AddTranslation(potGenerator, variant.Description, reference, $"Description for command variant \"{id} {variant.Subname}\"", LogicStrings.CONTEXT_COMMAND_VARIANT_DESCRIPTION);
                        foreach (var param in variant.Parameters)
                        {
                            AddTranslation(potGenerator, param.Description, reference, $"Description for parameter {param.Name} of command \"{id} {variant.Subname}\"", LogicStrings.CONTEXT_COMMAND_PARAMETER_DESCRIPTION);
                        }
                    }
                }
            }
            // 难度
            {
                var document = LoadMetaXmlDocument(spaceName, "difficulties.xml");
                var list = DifficultyMetaList.FromXmlNode(document["difficulties"], spaceName);
                var reference = "Difficulty meta file";
                foreach (var meta in list.metas)
                {
                    AddTranslation(potGenerator, meta.Name, reference, $"Name for difficulty {meta.Name}", LogicStrings.CONTEXT_DIFFICULTY);
                }
            }
            // 选项
            {
                var document = LoadMetaXmlDocument(spaceName, "options.xml");
                var list = OptionMetaList.FromXmlNode(document["options"], spaceName);
                var reference = "Option meta file";
                foreach (var meta in list.categories)
                {
                    AddTranslation(potGenerator, meta.Label, reference, $"Label for option category {meta.Label}", LogicStrings.CONTEXT_OPTION_CATEGORY);
                }
                foreach (var meta in list.widgets)
                {
                    AddTranslation(potGenerator, meta.Label, reference, $"Label for option widget {meta.Label}", LogicStrings.CONTEXT_OPTION_NAME);
                    AddTranslation(potGenerator, meta.Tooltip, reference, $"Tooltip for option widget {meta.Tooltip}", LogicStrings.CONTEXT_OPTION_TOOLTIP);
                }
            }

            potGenerator.WriteOut(GetPoTemplatePath("general.pot"));
            Debug.Log("Script Translations Updated.");
            EditorSceneManager.OpenScene(active);
        }
        public static void UpdateTalkTranslations()
        {
            var potGenerator = new MukioPotGenerator("MinecraftVSZombies2", "Cuerzor");

            var spaceName = VanillaMod.spaceName;

            var talkDir = Path.Combine(GetMetaDirectory(spaceName), "talks");
            foreach (var filePath in Directory.GetFiles(talkDir, "*.xml", SearchOption.TopDirectoryOnly))
            {
                using FileStream stream = File.Open(filePath, FileMode.Open);
                var document = stream.ReadXmlDocument();
                var metaList = TalkMeta.FromXmlDocument(document, spaceName);
                var reference = "Talk meta file";
                foreach (var group in metaList.groups)
                {
                    var groupName = group.archive.name;
                    var groupID = new NamespaceID(spaceName, group.id);
                    var groupContext = LogicStrings.GetTalkTextContext(groupID);
                    AddTranslation(potGenerator, groupName, reference, $"Archive name for talk group {group.id}", LogicStrings.CONTEXT_ARCHIVE);
                    for (int i = 0; i < group.sections.Length; i++)
                    {
                        var section = group.sections[i];
                        AddTranslation(potGenerator, section.archiveText, reference, $"Archive text for talk group {group.id}'s section {i}", LogicStrings.CONTEXT_ARCHIVE);
                        for (int j = 0; j < section.sentences.Length; j++)
                        {
                            var sentence = section.sentences[j];
                            AddTranslation(potGenerator, sentence.text, reference, $"Text for talk sentence {j} in talk group {group.id}'s section {i}", groupContext);
                            AddTranslation(potGenerator, sentence.description, reference, $"Description for talk sentence {j} in talk group {group.id}'s section {i}", LogicStrings.CONTEXT_ARCHIVE);
                            AddTranslation(potGenerator, sentence.speakerName, reference, $"Speaker Name for talk sentence {j} in talk group {group.id}'s section {i}", LogicStrings.CONTEXT_CHARACTER_NAME);
                        }
                    }
                }
            }
            potGenerator.WriteOut(GetPoTemplatePath("talk.pot"));
            Debug.Log("Talk Translations Updated.");
        }
        public static void UpdateResourcePackManifest()
        {
            // 根据语言包内的已有贴图和项目内的已有贴图生成spriteManifest.json，并将其保存到Localization\pack\assets\mvz2\en-US中。
            var path = GetLanguagePackDirectory();

            var packDir = Path.Combine(Application.dataPath, "Localization", "pack");
            var manifestPath = Path.Combine(packDir, ResourcePackReaderV1.MANIFEST_FILENAME);

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
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sprite = sprites[i];
                        var replaces = new SpriteReference(id, i);
                        ResourcePackSprite packSprite;
                        if (sprites.Length <= 1)
                        {
                            packSprite = new ResourcePackSprite()
                            {
                                replaces = replaces.ToString(),
                                tags = tags,
                                single = true,
                                pivotX = sprite.pivot.x / sprite.rect.width,
                                pivotY = sprite.pivot.y / sprite.rect.height,
                            };
                        }
                        else
                        {
                            packSprite = new ResourcePackSprite()
                            {
                                replaces = replaces.ToString(),
                                tags = tags,
                                single = false,
                                x = (int)sprite.rect.x,
                                y = (int)sprite.rect.y,
                                width = (int)sprite.rect.width,
                                height = (int)sprite.rect.height,
                                pivotX = sprite.pivot.x / sprite.rect.width,
                                pivotY = sprite.pivot.y / sprite.rect.height,
                            };
                        }
                        resoucePackSprites.Add(packSprite);
                    }

                    // 获取包中的Texture相对路径。
                    var texturePath = Path.GetRelativePath(packAssetsDir, spritePath).Replace("\\", "/");
                    resoucePackTextures.Add(new ResourcePackTexture()
                    {
                        texture = texturePath,
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

            ResourcePackManifest manifest = new ResourcePackManifest()
            {
                textures = resoucePackTextures.ToArray(),
                localization = resoucePackLocalization
            };
            var manifestJson = JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.Indented);

            FileHelper.ValidateDirectory(manifestPath);
            using var stream = File.Open(manifestPath, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(manifestJson);

            Debug.Log($"语言包清单已更新：{manifestPath}");
        }
        public static void UpdateLanguagePackDemo()
        {
            // 根据语言包内的已有贴图和项目内的已有贴图生成spriteManifest.json，并将其保存到Localization\pack\assets\mvz2\en-US中。
            UpdateResourcePackManifest();

            // 将spriteManifest.json复制到LanguagePack\templates中。
            Debug.Log("更新语言包贴图清单……");
            var sourceManifestPath = Path.Combine(Application.dataPath, "Localization", "pack", ResourcePackReaderV1.MANIFEST_FILENAME);
            var templatesDir = Path.Combine(Application.dataPath, "LanguagePack", "templates");
            if (File.Exists(sourceManifestPath))
            {
                var destPath = Path.Combine(templatesDir, ResourcePackReaderV1.MANIFEST_FILENAME);
                FileHelper.ValidateDirectory(destPath);
                File.Copy(sourceManifestPath, destPath, true);
            }

            // 将*.pot文件复制到LanguagePack\templates\text_templates中。
            Debug.Log("更新语言包文本模板……");
            CopyDirectory(GetPoTemplateDirectory(), Path.Combine(templatesDir, "text_templates"), "*.pot");

            // 将英文的*.po文件复制到LanguagePack\templates\text_en中。
            Debug.Log("更新语言包英文文本翻译……");
            CopyDirectory(GetPoDirectory("en-US"), Path.Combine(templatesDir, "text_en"), "*.po");

            Debug.Log("语言包示例更新完成。");
        }
        private static void MoveMOFiles()
        {
            var sourceDir = Path.Combine(Application.dataPath, "Localization", "source");
            var targetDir = Path.Combine(Application.dataPath, "Localization", "pack", "assets", "mvz2", "catalogs");
            foreach (var file in Directory.GetFiles(sourceDir, "*.mo", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                var destFile = Path.Combine(targetDir, relativePath);
                File.Copy(file, destFile, true);
            }
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
        public static string GetLanguagePackDirectory()
        {
            return Path.Combine(Application.dataPath, "GameContent", "LanguagePacks");
        }
        static void SearchObject(Transform tr, MukioPotGenerator pot)
        {
            var cp = tr.GetComponent<ITranslateComponent>();
            AddTranslate(pot, cp);

            for (int i = 0; i < tr.childCount; i++)
            {
                SearchObject(tr.GetChild(i), pot);
            }
        }
        private static void AddTranslate(MukioPotGenerator pot, ITranslateComponent cp)
        {
            if (cp == null)
                return;
            if (!string.IsNullOrEmpty(cp.Key))
            {
                PotTranslate translate = new PotTranslate(cp.Key, cp.Path, cp.Comment, cp.Context);
                pot.AddString(translate);
            }
            if (cp.Keys != null)
            {
                foreach (var key in cp.Keys)
                {
                    PotTranslate translate = new PotTranslate(key, cp.Path, cp.Comment, cp.Context);
                    pot.AddString(translate);
                }
            }
        }
        private static string GetPoTemplateDirectory()
        {
            return Path.Combine(Application.dataPath, "Localization", "source");
        }
        private static string GetPoDirectory(string language)
        {
            return Path.Combine(Application.dataPath, "Localization", "source", language);
        }
        private static string GetPoTemplatePath(string fileName)
        {
            return Path.Combine(GetPoTemplateDirectory(), fileName);
        }

        private static string GetMetaDirectory(string nsp)
        {
            return Path.Combine(Application.dataPath, "GameContent", "Assets", nsp, "metas");
        }
        private static XmlDocument LoadMetaXmlDocument(string nsp, string path)
        {
            var metaDirectory = GetMetaDirectory(nsp);
            var absPath = Path.Combine(metaDirectory, path);
            using FileStream stream = File.Open(absPath, FileMode.Open);
            return stream.ReadXmlDocument();
        }
        private static void AddAlmanacEntryTranslation(MukioPotGenerator potGenerator, AlmanacMetaEntry entry, string categoryName)
        {
            AddTranslation(potGenerator, entry.name, almanacReference, $"Name for {categoryName} {entry.id}", LogicStrings.GetAlmanacNameContext(categoryName));
            var context = LogicStrings.GetAlmanacDescriptionContext(categoryName);
            AddTranslation(potGenerator, entry.header, almanacReference, $"Header for {categoryName} {entry.id}", context);
            AddTranslation(potGenerator, entry.properties, almanacReference, $"Properties for {categoryName} {entry.id}", context);
            foreach (var flavor in entry.GetAllFlavors())
            {
                AddTranslation(potGenerator, flavor, almanacReference, $"Flavor for {categoryName} {entry.id}", context);
            }
        }
        private static void AddTranslation(MukioPotGenerator potGenerator, string text, string reference = null, string comment = null, string context = null)
        {
            if (string.IsNullOrEmpty(text))
                return;
            potGenerator.AddString(new PotTranslate(text, reference, comment, context));
        }
        private static string almanacReference = "Almanac meta file";
    }
}
