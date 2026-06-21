using System.IO;
using UnityEngine;

namespace MVZ2.Editor
{
    public static class AssetsMenu
    {
        public static void UpdateAllAssets()
        {
            LocalizationMenu.CompressLanguagePack();

            ResourcePackMenu.FixWavToOggFiles();
            ResourcePackMenu.UpdateAllResolverComponents();
            ResourcePackMenu.UpdateResourcePackManifest();
            ResourcePackMenu.UpdateResourcePackTutorialResources();
            ResourcePackMenu.UpdateResourcePackTutorialTemplates();

            SpriteMenu.RenameSprites();
            SpriteMenu.UpdateSpriteManifestAtGameContent();

            ModelMenu.UpdateModelElements();

            AddressablesMenu.UpdateAddressables();
        }
        public static string FileToAssetPath(string filePath)
        {
            return Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, filePath));
        }
    }
}
