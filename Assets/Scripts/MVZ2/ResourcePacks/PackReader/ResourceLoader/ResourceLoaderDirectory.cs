#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Cysharp.Threading.Tasks;
using MVZ2.Audios;
using MVZ2.IO;
using NGettext;
using UnityEngine;

namespace MVZ2.ResourcePacks
{
    public class ResourceLoaderDirectory : ResourceLoader
    {
        public ResourceLoaderDirectory(string directory, string filename) : base(filename)
        {
            this.directory = directory;
        }
        public override bool Exists(string relativePath)
        {
            return File.Exists(GetFullPath(relativePath));
        }
        public override string? LoadString(string relativePath)
        {
            if (!Exists(relativePath))
                return null;
            var path = GetFullPath(relativePath);
            using var stream = File.Open(path, FileMode.Open);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public override byte[]? LoadBytes(string relativePath)
        {
            if (!Exists(relativePath))
                return null;
            var fullPath = GetFullPath(relativePath);
            using var stream = File.Open(fullPath, FileMode.Open);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);

            return memory.ToArray();
        }
        public override async UniTask<string?> LoadStringAsync(string relativePath)
        {
            if (!Exists(relativePath))
                return null;
            var path = GetFullPath(relativePath);
            using var stream = File.Open(path, FileMode.Open);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        public override async UniTask<byte[]?> LoadBytesAsync(string relativePath)
        {
            if (!Exists(relativePath))
                return null;
            var fullPath = GetFullPath(relativePath);
            using var stream = File.Open(fullPath, FileMode.Open);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            return memory.ToArray();
        }
        public override async UniTask<Catalog?> LoadCatalogAsync(string relativePath, string lang)
        {
            if (!Exists(relativePath))
                return null;
            var path = GetFullPath(relativePath);
            using (var stream = File.Open(path, FileMode.Open))
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    await stream.CopyToAsync(memory);
                    memory.Seek(0, SeekOrigin.Begin);
                    return new Catalog(memory, new CultureInfo(lang));
                }
            }
        }
        public override async UniTask<AudioClip?> LoadAudioClipAsync(string relativePath, bool streaming)
        {
            if (!Exists(relativePath))
                return null;
            var fullPath = GetFullPath(relativePath);
            var url = FileHelper.GetFilePathURL(fullPath);
            var audioType = AudioHelper.GetAudioTypeFromPath(fullPath);
            return await AudioHelper.LoadAudioClip(url, audioType, streaming);
        }
        public override IEnumerable<string> EnumerateAllFiles()
        {
            foreach (var path in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                yield return Path.GetRelativePath(directory, path);
            }
        }
        public string GetFullPath(string relativePath)
        {
            return Path.Combine(directory, relativePath);
        }
        public string directory;
    }
}