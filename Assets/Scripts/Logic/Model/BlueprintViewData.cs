#nullable enable

using UnityEngine;

namespace MVZ2Logic.Models
{
    public struct BlueprintViewData
    {
        public bool empty;
        public string cost;
        public Sprite? icon;
        public bool triggerActive;
        public bool iconGrayscale;

        // Styles.
        public Sprite? standaloneBackground;
        public Sprite? mobileBackground;
        public Sprite? mobileFrameTop;
        public Sprite? mobileFrameBottom;

        public static readonly BlueprintViewData Empty = new BlueprintViewData { empty = true };
    }
}