#nullable enable

using System.IO;
using MVZ2.Sprites;
using PVZEngine.Tools.Unity;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    [CustomPropertyDrawer(typeof(GeneratedAudioClip))]
    public class GeneratedAudioClipDrawer : PropertyDrawer
    {
        public const float MAX_WIDTH = 200;
        public const float MAX_HEIGHT = 200;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            var clipProperty = property.FindPropertyRelative("clip");
            var clip = clipProperty.objectReferenceValue as AudioClip;
            if (clip != null)
            {
                var preview = AssetPreview.GetAssetPreview(clip);
                var size = CalculateSize(preview);
                height += size.y;
            }
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var clipProperty = property.FindPropertyRelative("clip");
            var clip = clipProperty.objectReferenceValue as AudioClip;
            if (clip == null)
                return;
            var namePos = position;
            namePos.height = 18;

            var colorBefore = GUI.color;
            var color = colorBefore;
            if (clip.loadState != AudioDataLoadState.Loaded || clip.length <= 0)
            {
                color = Color.red;
            }
            GUI.color = color;
            GUI.Label(namePos, label);
            GUI.color = colorBefore;

            var previewPos = position;
            previewPos.y += 18;
            var preview = AssetPreview.GetAssetPreview(clip);
            var size = CalculateSize(preview);
            previewPos.size = size;

            GUI.Label(previewPos, preview);
        }
        private Vector2 CalculateSize(Texture2D? texture)
        {
            if (!texture.Exists())
                return Vector2.zero;
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
    }
}
