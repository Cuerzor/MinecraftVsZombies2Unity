#nullable enable

using System.IO;
using UnityEngine;

namespace MVZ2.Resources
{
    public static class ResourcePackHelper
    {
        public static string GetTempDirectroy(string name)
        {
            return Path.Combine(Application.temporaryCachePath, "resource_packs", name);
        }
    }
}