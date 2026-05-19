#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using MVZ2.IO;
using NGettext;

namespace MVZ2.Resources
{
    public abstract class ResourceLoader
    {
        public ResourceLoader(string name)
        {
            Name = name;
        }
        public abstract bool Exists(string relativePath);
        public abstract string? LoadString(string relativePath);
        public abstract byte[]? LoadBytes(string relativePath);
        public abstract UniTask<string?> LoadStringAsync(string relativePath);
        public abstract UniTask<byte[]?> LoadBytesAsync(string relativePath);
        public abstract UniTask<Catalog?> LoadCatalogAsync(string relativePath, string lang);
        public abstract IEnumerable<string> EnumerateAllFiles();
        public override string ToString()
        {
            return Name;
        }
        public string Name { get; private set; }
    }
    public class ResourceLoaderZip : ResourceLoader
    {
        public ResourceLoaderZip(Stream stream, string name) : base(name)
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
        public ZipArchiveEntry? GetEntry(string relativePath)
        {
            return archive.Entries.FirstOrDefault(e => e.FullName.Replace("/", "\\") == relativePath.Replace("/", "\\"));
        }
        public ZipArchive archive;
    }
    public class ResourceLoaderDirectory : ResourceLoader
    {
        public ResourceLoaderDirectory(string directory, string name) : base(name)
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