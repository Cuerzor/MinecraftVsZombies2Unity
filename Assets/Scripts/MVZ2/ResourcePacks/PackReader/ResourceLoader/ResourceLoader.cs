#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NGettext;
using UnityEngine;

namespace MVZ2.ResourcePacks
{
    public abstract class ResourceLoader
    {
        public ResourceLoader(string filename)
        {
            FileName = filename;
        }
        public abstract bool Exists(string relativePath);
        public abstract string? LoadString(string relativePath);
        public abstract byte[]? LoadBytes(string relativePath);
        public abstract UniTask<string?> LoadStringAsync(string relativePath);
        public abstract UniTask<byte[]?> LoadBytesAsync(string relativePath);
        public abstract UniTask<Catalog?> LoadCatalogAsync(string relativePath, string lang);
        public abstract UniTask<AudioClip?> LoadAudioClipAsync(string relativePath, bool streaming);
        public abstract IEnumerable<string> EnumerateAllFiles();
        public override string ToString()
        {
            return FileName;
        }
        public string FileName { get; private set; }
    }
}