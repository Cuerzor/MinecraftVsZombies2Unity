#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using MVZ2.Audios;
using MVZ2.IO;
using MVZ2.Resources;
using NGettext;
using UnityEngine;

namespace MVZ2.ResourcePacks
{
    public class ResourceLoaderZip : ResourceLoader
    {
        public ResourceLoaderZip(Stream stream, string filename) : base(filename)
        {
            archive = new ZipArchive(stream);
        }
        public override bool Exists(string relativePath)
        {
            var entry = GetEntry(relativePath);
            return entry != null;
        }
        public override string? LoadString(string relativePath)
        {
            var entry = GetEntry(relativePath);
            if (entry == null)
                return null;
            return entry.ReadString(Encoding.UTF8);
        }
        public override byte[]? LoadBytes(string relativePath)
        {
            var textureEntry = GetEntry(relativePath);
            if (textureEntry == null)
                return null;
            return textureEntry.ReadBytes();
        }
        public override async UniTask<string?> LoadStringAsync(string relativePath)
        {
            var entry = GetEntry(relativePath);
            if (entry == null)
                return null;
            return await entry.ReadStringAsync(Encoding.UTF8);
        }
        public override async UniTask<byte[]?> LoadBytesAsync(string relativePath)
        {
            var textureEntry = GetEntry(relativePath);
            if (textureEntry == null)
                return null;
            return await textureEntry.ReadBytesAsync();
        }
        public override async UniTask<Catalog?> LoadCatalogAsync(string relativePath, string lang)
        {
            var catalogEntry = GetEntry(relativePath);
            if (catalogEntry == null)
                return null;
            return await catalogEntry.ReadCatalogAsync(lang);
        }
        public override async UniTask<AudioClip?> LoadAudioClipAsync(string relativePath, bool streaming)
        {
            var tempPath = await CopyToTempPath(relativePath);
            if (string.IsNullOrEmpty(tempPath))
                return null;
            var url = FileHelper.GetFilePathURL(tempPath);
            var audioType = AudioHelper.GetAudioTypeFromPath(tempPath);
            return await AudioHelper.LoadAudioClip(url, audioType, streaming);
        }
        public override IEnumerable<string> EnumerateAllFiles()
        {
            var entries = archive.Entries.ToArray();
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                if (string.IsNullOrEmpty(Path.GetExtension(entry.FullName)))
                    continue;

                yield return entry.FullName;
            }
        }
        public async UniTask<string?> CopyToTempPath(string relativePath)
        {
            var entry = GetEntry(relativePath);
            if (entry == null)
                return null;
            var basePath = ResourcePackHelper.GetTempDirectroy(FileName);
            var fullPath = Path.Combine(basePath, relativePath);
            if (!fullPath.StartsWith(basePath))
                throw new UnauthorizedAccessException("·Ç·¨Â·¾¶");
            await copier.CopyZipEntryTo(entry, fullPath);
            return fullPath;
        }
        public ZipArchiveEntry? GetEntry(string relativePath)
        {
            return archive.Entries.FirstOrDefault(e => e.FullName.Replace("/", "\\") == relativePath.Replace("/", "\\"));
        }
        public ZipArchive archive;
        private FileCopier copier = new FileCopier();
    }
}