#nullable enable

using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;

namespace MVZ2.IO
{
    public class FileCopier
    {
        public async UniTask CopyZipEntryTo(ZipArchiveEntry entry, string path)
        {
            // 如果已经有拷贝任务在进行，直接等待它的结果
            if (_copyTasks.TryGetValue(path, out var existingTask))
            {
                await existingTask;
                return;
            }

            var task = CopyZipEntryToInternal(entry, path);
            _copyTasks[path] = task;

            try
            {
                await task;
            }
            finally
            {
                _copyTasks.TryRemove(path, out _);
            }
        }
        private async UniTask CopyZipEntryToInternal(ZipArchiveEntry entry, string path)
        {
            FileHelper.ValidateDirectoryExists(path);

            // 原子性优化：先写到临时文件，成功后再重命名
            var tempWritePath = path + ".tmp";
            try
            {
                using (var entryStream = entry.Open())
                {
                    using var stream = new FileStream(tempWritePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true);
                    await entryStream.CopyToAsync(stream);
                }
                // 拷贝成功后原子替换
                FileHelper.Move(tempWritePath, path, overwrite: true);
            }
            catch
            {
                // 清理临时文件
                if (File.Exists(tempWritePath))
                    File.Delete(tempWritePath);
                throw;
            }
        }
        private ConcurrentDictionary<string, UniTask> _copyTasks = new();
    }
}