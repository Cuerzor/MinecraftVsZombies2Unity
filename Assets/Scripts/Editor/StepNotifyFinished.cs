using CustomBuilder;
using UnityEditor;

namespace MVZ2.Editor
{
    [BuilderStep]
    public class StepNotifyFinished : IBuilderStep
    {
        public void ActionAfter()
        {
            EditorApplication.Beep();
        }

        public void ActionBefore(object option)
        {
        }
    }
}
