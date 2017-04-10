using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace TestTask
{
    public class XmlWriter
    {
        public readonly Queue<HierarchicalLink<FileSystemInfo>> FsEntriesQueue;

        private readonly Dictionary<FileSystemInfo, XElement> fsEntriesToXElementsMap;
        private readonly string filePath;
        private readonly AutoResetEvent fsEntryScannedEvent;
        private XDocument document;

        public XmlWriter(string filePath, AutoResetEvent fsEntryScannedEvent)
        {
            this.filePath = filePath;
            FsEntriesQueue = new Queue<HierarchicalLink<FileSystemInfo>>();
            this.fsEntryScannedEvent = fsEntryScannedEvent;
            fsEntriesToXElementsMap = new Dictionary<FileSystemInfo, XElement>();
        }

        public void BeginWriteDirectoryContentToFile()
        {
            document = new XDocument();
            while (true)
            {
                MonitorQueue();
            }
        }

        private void MonitorQueue()
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
                WriteFsEntryToXml(newFsEntry);
            }
            else
            {
                fsEntryScannedEvent.WaitOne();
            }
        }

        private void WriteFsEntryToXml(HierarchicalLink<FileSystemInfo> link)
        {
            var newNode = FsEntryXmlSerializer.ToXElement(link.Child);
            if (IsRootElement(link))
            {
                document.Add(newNode);
            }
            else
            {
                var parentElement = fsEntriesToXElementsMap[link.Parent];
                parentElement.Add(newNode);
            }
            fsEntriesToXElementsMap[link.Child] = newNode;
            document.Save(filePath);
        }

        private bool IsRootElement(HierarchicalLink<FileSystemInfo> link)
        {
            if ((link.Parent == null) != (document.Root == null))
            {
                throw new XmlException();
            }

            return link.Parent == null && document.Root == null;
        }
    }
}
