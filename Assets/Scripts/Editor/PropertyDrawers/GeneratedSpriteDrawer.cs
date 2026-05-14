using System.IO;
using MVZ2.Sprites;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    [CustomPropertyDrawer(typeof(GeneratedSprite))]
    public class GeneratedSpriteDrawer : PropertyDrawer
    {
        public const float MAX_WIDTH = 200;
        public const float MAX_HEIGHT = 200;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);

            height += 18; // Save as Button;

            var spriteProperty = property.FindPropertyRelative("sprite");
            var sprite = spriteProperty.objectReferenceValue as Sprite;
            if (sprite != null)
            {
                var preview = AssetPreview.GetAssetPreview(sprite);
                var size = CalculateSize(preview);
                height += size.y;
            }
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var spriteProperty = property.FindPropertyRelative("sprite");
            var sprite = spriteProperty.objectReferenceValue as Sprite;
            if (sprite == null)
                return;
            var namePos = position;
            namePos.height = 18;
            GUI.Label(namePos, label);

            var saveAsPos = position;
            saveAsPos.y += 18;
            saveAsPos.height = 18;
            bool saveAs = GUI.Button(saveAsPos, "Save As");


            var previewPos = position;
            previewPos.y += 36;
            var preview = AssetPreview.GetAssetPreview(sprite);
            var size = CalculateSize(preview);
            previewPos.size = size;

            var backgroundProperty = property.FindPropertyRelative("background");
            var background = backgroundProperty.objectReferenceValue as Sprite;
            var backgroundPreview = AssetPreview.GetAssetPreview(background);
            GUI.Label(previewPos, backgroundPreview);
            GUI.Label(previewPos, preview);

            if (saveAs)
            {
                var nameProperty = property.FindPropertyRelative("name");
                var name = ToFilename(nameProperty.stringValue);
                var path = EditorUtility.SaveFilePanel("Save Sprite", Application.dataPath, name, "png");
                if (!string.IsNullOrEmpty(path))
                {
                    sprite.texture.SaveToPath(path);
                }
            }
        }
        private Vector2 CalculateSize(Texture2D texture)
        {
            if (texture.width > texture.height)
            {
                var width = Mathf.Min(MAX_WIDTH, texture.width);
                return new Vector2(width, width * texture.height / texture.width);
            }
            else
            {
                var height = Mathf.Min(MAX_HEIGHT, texture.height);
                return new Vector2(height * texture.width / texture.height, height);
            }
        }
        public static string ToFilename(string idString)
        {
            var name = idString;
            foreach (var chr in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(chr, '_');
            }
            return name;
        }
    }
}
