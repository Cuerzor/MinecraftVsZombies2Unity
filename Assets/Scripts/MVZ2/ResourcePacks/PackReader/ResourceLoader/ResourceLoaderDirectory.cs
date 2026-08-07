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
            var path = GetFullPath(relativePath);
            return ValidateResourcePath(path);
        }
        public bool ValidateResourcePath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var fullDirectory = Path.GetFullPath(directory);
            if (FileHelper.CheckZipSlip(path, fullDirectory))
            {
                return false;
            }
            return File.Exists(path);
        }
        public override string? LoadString(string relativePath)
        {
            var path = GetFullPath(relativePath);
            if (!ValidateResourcePath(path))
                return null;
            using var stream = File.Open(path, FileMode.Open);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public override byte[]? LoadBytes(string relativePath)
        {
            var path = GetFullPath(relativePath);
            if (!ValidateResourcePath(path))
                return null;
            using var stream = File.Open(path, FileMode.Open);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);

            return memory.ToArray();
        }
        public override async UniTask<string?> LoadStringAsync(string relativePath)
        {
            var path = GetFullPath(relativePath);
            if (!ValidateResourcePath(path))
                return null;
            using var stream = File.Open(path, FileMode.Open);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        public override async UniTask<byte[]?> LoadBytesAsync(string relativePath)
        {
            var path = GetFullPath(relativePath);
            if (!ValidateResourcePath(path))
                return null;
            using var stream = File.Open(path, FileMode.Open);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            return memory.ToArray();
        }
        public override async UniTask<Catalog?> LoadCatalogAsync(string relativePath, string lang)
        {
            var path = GetFullPath(relativePath);
            if (!ValidateResourcePath(path))
                return null;
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
            var path = GetFullPath(relativePath);
            if (!ValidateResourcePath(path))
                return null;
            var url = FileHelper.GetFilePathURL(path);
            var audioType = AudioHelper.GetAudioTypeFromPath(path);
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
            return FileHelper.CombineFullPath(directory, relativePath);
        }
        public string directory;
    }
}