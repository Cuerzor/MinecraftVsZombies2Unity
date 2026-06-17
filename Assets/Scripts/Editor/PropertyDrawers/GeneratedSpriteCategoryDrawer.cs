using MVZ2.Sprites;
using PVZEngine.Tools.Unity;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    [CustomPropertyDrawer(typeof(GeneratedSpriteCategory))]
    public class GeneratedSpriteCategoryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            height += 18; // Save all Button;

            var spritesProperty = property.FindPropertyRelative("sprites");
            height += EditorGUI.GetPropertyHeight(spritesProperty);

            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var namePos = position;
            namePos.height = 18;
            GUI.Label(namePos, label);

            var saveAsPos = position;
            saveAsPos.y += 18;
            saveAsPos.height = 18;
            bool saveAll = GUI.Button(saveAsPos, "Save All");

            var spritesProperty = property.FindPropertyRelative("sprites");
            var spritesPos = position;
            spritesPos.y += 36;
            EditorGUI.PropertyField(spritesPos, spritesProperty);

            if (saveAll)
            {
                SaveAll(property);
            }
        }
        private void SaveAll(SerializedProperty property)
        {
            var folderPath = EditorUtility.SaveFolderPanel("Save All Sprites", Application.dataPath, "");
            if (string.IsNullOrEmpty(folderPath))
                return;
            var spritesProperty = property.FindPropertyRelative("sprites");
            if (!spritesProperty.isArray)
                return;
            for (int i = 0; i < spritesProperty.arraySize; i++)
            {
                var previewProperty = spritesProperty.GetArrayElementAtIndex(i);

                var nameProperty = previewProperty.FindPropertyRelative("name");
                if (nameProperty == null)
                {
                    Log.LogWarning("name property of a sprite preview is missing.");
                    continue;
                }
                var name = SpritePreviewDrawer.ToFilename(nameProperty.stringValue);
                var spriteProperty = previewProperty.FindPropertyRelative("sprite");
                if (spriteProperty == null)
                {
                    Log.LogWarning($"sprite property of sprite preview {name} is missing.");
                    continue;
                }
                var sprite = spriteProperty.objectReferenceValue as Sprite;
                if (!sprite.Exists())
                {
                    Log.LogWarning($"sprite of sprite preview {name} is missing.");
                    continue;
                }
                var path = $"{folderPath}/{name}.png";
                sprite.texture.SaveToPath(path);
                Debug.Log($"ÒÑ±£´æÌùÍ¼{path}¡£");
            }
            Debug.Log("ÌùÍ¼±£´æÍê±Ï¡£");
        }
    }
}
