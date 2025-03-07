using System;
using UnityEngine;

namespace MVZ2.Sprites
{
    public static class SpriteHelper
    {
        public static Texture2D LoadTextureFromBytes(byte[] bytes)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            texture.FixTransparency();
            return texture;
        }
        /// <summary>
        /// 修复图片导入时白边问题
        /// </summary>
        /// <param name="texture"></param>
        public static void FixTransparency(this Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            int w = texture.width;
            int h = texture.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    Color32 pixel = pixels[idx];
                    if (pixel.a == 0)
                    {
                        bool done = false;
                        if (!done && x > 0)
                            done = TryAdjacent(ref pixel, pixels[idx - 1]);        // Left pixel
                        if (!done && x > 0 && y < h - 1)
                            done = TryAdjacent(ref pixel, pixels[idx + w - 1]);    // Bottom Left pixel
                        if (!done && y < h - 1)
                            done = TryAdjacent(ref pixel, pixels[idx + w]);        // Bottom pixel
                        if (!done && x < w - 1 && y < h - 1)
                            done = TryAdjacent(ref pixel, pixels[idx + w + 1]);    // Bottom Right pixel
                        if (!done && x < w - 1)
                            done = TryAdjacent(ref pixel, pixels[idx + 1]);        // Right pixel
                        if (!done && x < w - 1 && y > 0)
                            done = TryAdjacent(ref pixel, pixels[idx - w + 1]);    // Top Right pixel
                        if (!done && y > 0)
                            done = TryAdjacent(ref pixel, pixels[idx - w]);        // Top pixel
                        if (!done && x > 0 && y > 0)
                            done = TryAdjacent(ref pixel, pixels[idx - w - 1]);    // Top Left pixel
                        pixels[idx] = pixel;
                    }
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
        }

        public static void FlipColors(this Texture2D texture)
        {
            var colors = texture.GetPixels32();
            var width = texture.width;
            var height = texture.height;

            Color32[] tempRow = new Color32[width];
            for (int y = 0; y < height / 2; y++)
            {
                int topRowIndex = y * width;
                int bottomRowIndex = (height - y - 1) * width;

                // 交换顶部和底部的像素行
                Array.Copy(colors, topRowIndex, tempRow, 0, width);
                Array.Copy(colors, bottomRowIndex, colors, topRowIndex, width);
                Array.Copy(tempRow, 0, colors, bottomRowIndex, width);
            }

            texture.SetPixels32(colors);
        }
        private static bool TryAdjacent(ref Color32 pixel, Color32 adjacent)
        {
            if (adjacent.a == 0)
                return false;

            pixel.r = adjacent.r;
            pixel.g = adjacent.g;
            pixel.b = adjacent.b;
            return true;
        }
    }
}
