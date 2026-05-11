using System.Collections.Generic;
using MVZ2Logic.Models;
using UnityEditor;

namespace MVZ2.Editor
{
    [CustomEditor(typeof(ModelGroupArea))]
    public class ModelGroupAreaEditor : ModelGroupEditor
    {
        protected override void GetPropertyFields(List<SerializedProperty> list)
        {
            base.GetPropertyFields(list);
            list.Add(serializedObject.FindProperty("renderers"));
            list.Add(serializedObject.FindProperty("particles"));
        }
    }
}
