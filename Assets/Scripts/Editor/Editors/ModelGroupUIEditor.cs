using System.Collections.Generic;
using MVZ2Logic.Models;
using UnityEditor;

namespace MVZ2.Editor
{
    [CustomEditor(typeof(ModelGroupUI))]
    public class ModelGroupUIEditor : ModelGroupEditor
    {
        protected override void GetPropertyFields(List<SerializedProperty> list)
        {
            base.GetPropertyFields(list);
            list.Add(serializedObject.FindProperty("images"));
        }
    }
}
