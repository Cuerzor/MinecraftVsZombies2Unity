#nullable enable

using System.IO;
using MVZ2.IO;
using UnityEngine;

namespace MVZ2.ResourcePacks
{
    public static class ResourcePackHelper
    {
        public static string GetTempDirectroy(string name)
        {
            return FileHelper.CombineFullPathSecure(Application.temporaryCachePath, "resource_packs", name);
        }
    }
}