using System.IO;
using MVZ2.IO;
using MVZ2.Sprites;
using MVZ2Logic;
using PVZEngine.Tools.Unity;
using UnityEngine;
using UnityEngine.TextCore.Text;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Grids;
using MVZ2Logic.Level;

namespace MVZ2.Editor
{
    public static class DepthTextureMenu
    {
        public static void GenerateDepthTextureForCurrentLevel()
        {
            var level = Global.Level.GetLevel();
            if (level == null)
            {
                Log.LogError("Not in a level!");
            }

            var width = 1400;
            var height = 1020;
            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    var levelX = x;
                    var levelZ = z - 210;
                    var levelY = level.GetGroundY(levelX, levelZ);

                    uint code = (uint)(z - levelY);
                    byte hi = (byte)((((code >> 8) + DEPTH_HIGH_BITS_OFFSET) * DEPTH_HIGH_BITS_STEP) & 0xFF);
                    byte lo = (byte)(code & 0xFF);
                    float h01 = Mathf.InverseLerp(MIN_DEPTH_HEIGHT, MAX_DEPTH_HEIGHT, levelY);
                    byte preview = (byte)Mathf.RoundToInt(h01 * 255f);

                    Color32 c = new Color32(preview, hi, lo, 255);
                    if (level.IsAirAt(levelX, levelZ))
                    {
                        c.a = 0;
                    }
                    texture.SetPixel(x, z, c);
                }
            }
            texture.Apply();

            var filename = level.AreaID.ToString();
            filename = filename.Replace(':', '_');
            foreach (var character in Path.GetInvalidPathChars())
            {
                filename = filename.Replace(character, '_');
            }
            var path = Path.Combine(GetDepthTextureSaveDirectory(), $"{filename}.png");
            FileHelper.ValidateDirectoryExists(path);
            texture.SaveToPath(path);
            Log.LogMessage($"Depth texture saved to {path}!");
        }
        private static string GetDepthTextureSaveDirectory()
        {
            return Path.Combine(Application.dataPath, "DepthTextures");
        }
        public const int MIN_DEPTH_HEIGHT = -1024;
        public const int MAX_DEPTH_HEIGHT = 1023;
        public const int DEPTH_HIGH_BITS_STEP = 36;
        public const int DEPTH_HIGH_BITS_OFFSET = 3;
    }
}
