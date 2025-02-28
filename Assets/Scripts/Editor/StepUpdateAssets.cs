using CustomBuilder;
using CustomBuilder.Editor;
using UnityEditor;

namespace MVZ2.Editor
{
    [BuilderStep]
    public class StepUpdateAssets : IBuilderStep
    {
        public void ActionAfter(string targetDirectory)
        {
        }

        public void ActionBefore(object option)
        {
            AssetsMenu.UpdateAllAssets();
        }
    }
}
