using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace TestTask
{
    internal class XmlWriter : IConsumer
    {
        private readonly ConsumerComponent consumerComponent;
        private readonly Dictionary<FileSystemInfo, XElement> fsEntriesToXElementsMap;
        private readonly string filePath;
        private XDocument document;

        public XmlWriter(string filePath, ConsumerComponent consumerComponent)
        {
            this.filePath = filePath;
            this.consumerComponent = consumerComponent;
            fsEntriesToXElementsMap = new Dictionary<FileSystemInfo, XElement>();
        }

        public Queue<HierarchicalLink<FileSystemInfo>> GetEntriesQueue()
        {
            return consumerComponent.FsEntriesQueue;
        }

        public void BeginConsume()
        {
            document = new XDocument();
            consumerComponent.BeginConsume(WriteFsEntryToXml);
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
