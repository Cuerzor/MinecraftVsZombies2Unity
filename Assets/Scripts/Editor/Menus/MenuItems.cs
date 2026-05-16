using UnityEditor;

namespace MVZ2.Editor
{
    public static class MenuItems
    {
        [MenuItem("MVZ2/Assets/Update All Assets", priority = 0)]
        public static void UpdateAllAssetsMenu()
        {
            AssetsMenu.UpdateAllAssets();
            EditorUtility.RequestScriptReload();
        }
        [MenuItem("MVZ2/Assets/Addressables/Update Addressables", priority = 1000)]
        public static void UpdateAddressablesMenu()
        {
            AddressablesMenu.UpdateAddressables();
            EditorUtility.RequestScriptReload();
        }
        [MenuItem("MVZ2/Assets/Localization/Compress Langauge Pack", priority = 2000, secondaryPriority = 0)]
        public static void CompressLanguagePack()
        {
            LocalizationMenu.CompressLanguagePack();
        }
        [MenuItem("MVZ2/Assets/Localization/Update All Translations", priority = 3000, secondaryPriority = 0)]
        public static void UpdateAllTranslations()
        {
            LocalizationMenu.UpdateAllTranslations();
        }
        [MenuItem("MVZ2/Assets/Localization/Update Almanac Translations", priority = 3000, secondaryPriority = 1)]
        public static void UpdateAlmanacTranslations()
        {
            LocalizationMenu.UpdateAlmanacTranslations();
        }
        [MenuItem("MVZ2/Assets/Localization/Update General Translations", priority = 3000, secondaryPriority = 2)]
        public static void UpdateGeneralTranslations()
        {
            LocalizationMenu.UpdateGeneralTranslations();
        }
        [MenuItem("MVZ2/Assets/Localization/Update Talk Translations", priority = 3000, secondaryPriority = 3)]
        public static void UpdateTalkTranslations()
        {
            LocalizationMenu.UpdateTalkTranslations();
        }
        [MenuItem("MVZ2/Assets/Localization/Update Localized Sprite Manifest", priority = 3100, secondaryPriority = 0)]
        public static void UpdateLocalizedSpriteManifest()
        {
            LocalizationMenu.UpdateResourcePackManifest();
        }
        [MenuItem("MVZ2/Assets/Localization/Update Language Pack Demo", priority = 3200, secondaryPriority = 0)]
        public static void UpdateLanguagePackDemo()
        {
            LocalizationMenu.UpdateLanguagePackDemo();
        }
        [MenuItem("MVZ2/Assets/Models/Update Model Elements", priority = 4000, secondaryPriority = 0)]
        public static void UpdateModelElements()
        {
            ModelMenu.UpdateModelElements();
        }
        [MenuItem("MVZ2/Assets/Models/Update Sprite Renderer And Image Resolvers", priority = 4000, secondaryPriority = 1)]
        public static void UpdateSpriteRendererAndImageResolvers()
        {
            ModelMenu.UpdateSpriteRendererAndImageResolvers();
        }
        [MenuItem("MVZ2/Assets/Models/Convert Models to New", priority = 4100, secondaryPriority = 0)]
        public static void ConvertOldModel()
        {
            ModelConversionWindow.ShowWindow();
        }
        [MenuItem("MVZ2/Assets/Sprites/Update Sprite Manifest", priority = 5000, secondaryPriority = 0)]
        public static void UpdateSpriteManifestAtGameContent()
        {
            SpriteMenu.UpdateSpriteManifestAtGameContent();
        }
        [MenuItem("MVZ2/Assets/Sprites/Rename Sprites", priority = 5000, secondaryPriority = 1)]
        public static void RenameSprites()
        {
            SpriteMenu.RenameSprites();
        }
        [MenuItem("MVZ2/Assets/Talk/Format Talk Datas", priority = 6000, secondaryPriority = 0)]
        public static void FormatTalkDatas()
        {
            TalkMenu.FormatTalkDatas();
        }
        [MenuItem("MVZ2/Assets/Talk/Convert Dialogues to XML", priority = 6000, secondaryPriority = 1)]
        public static void ConvertTalkFile()
        {
            TalkMenu.ConvertNaturalLanguageToTalks();
        }
        [MenuItem("MVZ2/Assets/Talk/Convert XML to Dialogues", priority = 6000, secondaryPriority = 2)]
        public static void ConvertTalkXMLToDialogues()
        {
            TalkMenu.ConvertXMLToPlainText();
        }
        [MenuItem("MVZ2/Save Data/Decompress", priority = 10000, secondaryPriority = 0)]
        public static void DecompressSaveData()
        {
            SaveDataMenu.DecompressSaveData();
        }
        [MenuItem("MVZ2/Save Data/Compress", priority = 10000, secondaryPriority = 1)]
        public static void CompressSaveData()
        {
            SaveDataMenu.CompressSaveData();
        }
    }
}
