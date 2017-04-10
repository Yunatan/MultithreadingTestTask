using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TestTask
{
    public class DirectoryScanner
    {
        public readonly AutoResetEvent FsEntryScannedEvent = new AutoResetEvent(false);
        public readonly List<Queue<HierarchicalLink<FileSystemInfo>>> PubList;

        private readonly string path;

        public DirectoryScanner(string path)
        {
            this.path = path;
            PubList = new List<Queue<HierarchicalLink<FileSystemInfo>>>();
        }

        public void BeginScanDirectoryContent()
        {
            var rootDirectoryInfo = new DirectoryInfo(path);
            PublishNewFsEntry(null, rootDirectoryInfo);
            
            ScanDirectoryRecursively(rootDirectoryInfo);
        }

        private void ScanDirectoryRecursively(DirectoryInfo parentDirectoryInfo)
        {
            foreach (var childDirectory in parentDirectoryInfo.GetDirectories())
            {
                PublishNewFsEntry(parentDirectoryInfo, childDirectory);
                try
                {
                    ScanDirectoryRecursively(childDirectory);
                }
                catch (UnauthorizedAccessException)
                {
                    //swallow UnauthorizedAccess exceptions for folders we have no permission to visit
                }
            }
            foreach (var file in parentDirectoryInfo.GetFiles())
            {
                PublishNewFsEntry(parentDirectoryInfo, file);
            }
        }

        private void PublishNewFsEntry(FileSystemInfo parent, FileSystemInfo child)
        {
            var link = new HierarchicalLink<FileSystemInfo>(parent, child);
            foreach (var queue in PubList)
            {
                try
                {
                    Monitor.Enter(queue);
                    queue.Enqueue(link);
                }
                finally
                {
                    Monitor.Exit(queue);
                }
            }
            FsEntryScannedEvent.Set();
        }
    }
}
