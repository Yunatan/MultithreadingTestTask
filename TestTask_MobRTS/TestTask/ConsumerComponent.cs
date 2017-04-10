using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TestTask
{
    internal class ConsumerComponent
    {
        public readonly Queue<HierarchicalLink<FileSystemInfo>> FsEntriesQueue;

        private readonly AutoResetEvent fsEntryScannedEvent;

        public ConsumerComponent(AutoResetEvent fsEntryScannedEvent)
        {
            FsEntriesQueue = new Queue<HierarchicalLink<FileSystemInfo>>();
            this.fsEntryScannedEvent = fsEntryScannedEvent;
        }

        internal delegate void NewEntryHandler(HierarchicalLink<FileSystemInfo> link);

        internal void BeginConsume(NewEntryHandler fsEntriesHandler)
        {
            while (true)
            {
                MonitorQueue(fsEntriesHandler);
            }
        }

        private void MonitorQueue(NewEntryHandler fsEntriesHandler)
        {
            if (FsEntriesQueue.Count > 0)
            {
                HierarchicalLink<FileSystemInfo> newFsEntry;
                try
                {
                    Monitor.Enter(FsEntriesQueue);
                    newFsEntry = FsEntriesQueue.Dequeue();
                }
                finally
                {
                    Monitor.Exit(FsEntriesQueue);
                }

                fsEntriesHandler.Invoke(newFsEntry);
            }
            else
            {
                fsEntryScannedEvent.WaitOne();
            }
        }
    }
}
