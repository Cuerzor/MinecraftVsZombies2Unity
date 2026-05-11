using PVZEngine.Base;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    [CustomPropertyDrawer(typeof(NamespaceIDReference))]
    public class NamespaceIDDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var spacename = property.FindPropertyRelative("spacename");
            var path = property.FindPropertyRelative("path");

            EditorGUI.BeginProperty(position, label, property);

            // 绘制整体标签并获取剩余矩形区域
            var propertyRect = EditorGUI.PrefixLabel(position, label);

            // 计算 spacename 的矩形（占40%宽度）
            var spaceRect = new Rect(propertyRect.x, propertyRect.y,
                                     propertyRect.width * 0.4f, propertyRect.height);
            // 使用 PropertyField 并隐藏标签，自动处理多选
            EditorGUI.PropertyField(spaceRect, spacename, GUIContent.none);

            // 计算 path 的矩形（剩余宽度，留5像素间距）
            var pathRect = new Rect(propertyRect.x + propertyRect.width * 0.4f + 5,
                                    propertyRect.y,
                                    propertyRect.width - propertyRect.width * 0.4f - 5,
                                    propertyRect.height);
            EditorGUI.PropertyField(pathRect, path, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
