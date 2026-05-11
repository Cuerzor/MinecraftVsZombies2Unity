using MVZ2Logic.Models;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    [CustomEditor(typeof(Model), editorForChildClasses: true)]
    public class ModelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var model = target as Model;
            if (GUILayout.Button("Update Elements"))
            {
                model.UpdateElements();
                EditorUtility.SetDirty(model);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
