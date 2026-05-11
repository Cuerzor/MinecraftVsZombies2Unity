using System.Collections.Generic;
using MVZ2Logic.Models;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    public abstract class ModelGroupEditor : UnityEditor.Editor
    {
        private List<SerializedProperty> serializeProps = new List<SerializedProperty>();
        private void OnEnable()
        {
            serializeProps.Clear();
            GetPropertyFields(serializeProps);
        }
        public override void OnInspectorGUI()
        {
            var group = target as ModelGroup;
            foreach (var prop in serializeProps)
            {
                EditorGUILayout.PropertyField(prop);
            }
            if (GUILayout.Button("Update Elements"))
            {
                group.UpdateElements();
                EditorUtility.SetDirty(group);
            }
            serializedObject.ApplyModifiedProperties();
        }
        protected virtual void GetPropertyFields(List<SerializedProperty> list)
        {
            list.Add(serializedObject.FindProperty("testMode"));
            list.Add(serializedObject.FindProperty("modelAnchors"));
            list.Add(serializedObject.FindProperty("transforms"));
            list.Add(serializedObject.FindProperty("animators"));
        }
    }
}
