using System.IO;
using CustomBuilder;
using CustomBuilder.Editor;
using MVZ2.IO;
using UnityEditor;
using UnityEngine;

namespace MVZ2.Editor
{
    [BuilderStep]
    public class StepMoveReadme : IBuilderStep
    {
        public void ActionAfter(string targetDirectory)
        {
            var sourceDir = Path.Combine(Application.dataPath, "Readme");
            if (!Directory.Exists(sourceDir))
                return;
            var destDir = Path.Combine(Path.GetDirectoryName(targetDirectory), "必读 Readme");
            foreach (var file in Directory.EnumerateFiles(sourceDir))
            {
                if (Path.GetExtension(file) == ".meta")
                    continue;
                var relativePath = Path.GetRelativePath(sourceDir, file);
                if (Path.GetExtension(relativePath) == ".md")
                {
                    relativePath = Path.ChangeExtension(relativePath, "txt");
                }
                var destPath = Path.Combine(destDir, relativePath);
                FileHelper.ValidateDirectory(destPath);
                File.Copy(file, destPath, true);
            }
        }

        public void ActionBefore(object option)
        {
        }
    }
}
