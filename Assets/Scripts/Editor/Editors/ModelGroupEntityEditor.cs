using System.Collections.Generic;
using MVZ2Logic.Models;
using UnityEditor;

namespace MVZ2.Editor
{
    [CustomEditor(typeof(ModelGroupEntity))]
    public class ModelGroupEntityEditor : ModelGroupAreaEditor
    {
        protected override void GetPropertyFields(List<SerializedProperty> list)
        {
            base.GetPropertyFields(list);
            list.Add(serializedObject.FindProperty("subSortingGroups"));
        }
    }
}
