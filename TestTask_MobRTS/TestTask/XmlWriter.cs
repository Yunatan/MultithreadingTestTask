using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace TestTask
{
    public class XmlWriter
    {
        public readonly Queue<HierarchicalLink<FileSystemInfo>> FsEntriesQueue;

        private readonly Dictionary<FileSystemInfo, XElement> fsEntriesToXmlNodesMap;
        private readonly string filePath;
        private readonly AutoResetEvent fsEntryScannedEvent;
        private XDocument document;

        public XmlWriter(string filePath, AutoResetEvent fsEntryScannedEvent)
        {
            this.filePath = filePath;
            this.fsEntryScannedEvent = fsEntryScannedEvent;
            fsEntriesToXmlNodesMap = new Dictionary<FileSystemInfo, XElement>();
            FsEntriesQueue = new Queue<HierarchicalLink<FileSystemInfo>>();
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
            if (FsEntriesQueue.Any())
            {
                WriteFsEntryToXml(FsEntriesQueue.Dequeue());
            }
            else
            {
                fsEntryScannedEvent.WaitOne();
            }
        }

        private void WriteFsEntryToXml(HierarchicalLink<FileSystemInfo> link)
        {
            var newNode = ToXElement(link.Child);
            if (IsRootElement(link))
            {
                document.Add(newNode);
            }
            else
            {
                var parentNode = fsEntriesToXmlNodesMap[link.Parent];
                parentNode.Add(newNode);
            }
            fsEntriesToXmlNodesMap[link.Child] = newNode;
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

        #region XmlSerialization and calculation of permissions\size

        public static XElement ToXElement(FileSystemInfo directoryInfo)
        {
            if (directoryInfo is DirectoryInfo)
            {
                return ToXElement((DirectoryInfo)directoryInfo);
            }

            if (directoryInfo is FileInfo)
            {
                return ToXElement((FileInfo)directoryInfo);
            }

            return null;
        }

        public static XmlNode XElementToXElement(XElement element)
        {
            using (var xmlReader = element.CreateReader())
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }

        public static XElement ToXElement(DirectoryInfo directoryInfo)
        {
            var info = new XElement("directory",
                GetCommonAttributes(directoryInfo),
                new XAttribute("size", CalculateFolderSize(directoryInfo)),
                new XAttribute("owner", GetOwnerName(directoryInfo.GetAccessControl())),
                new XAttribute("permissions", GetPermissions(directoryInfo.GetAccessControl())));

            return info;
        }

        public static XElement ToXElement(FileInfo fileInfo)
        {
            var info = new XElement("file",
                GetCommonAttributes(fileInfo),
                new XAttribute("size", fileInfo.Length),
                new XAttribute("owner", GetOwnerName(fileInfo.GetAccessControl())),
                new XAttribute("permissions", GetPermissions(fileInfo.GetAccessControl())));

            return info;
        }

        private static object[] GetCommonAttributes(FileSystemInfo directoryInfo)
        {
            return new object[]
            {
                new XAttribute("name", directoryInfo.Name),
                new XAttribute("creation-date", directoryInfo.CreationTime.ToString("d")),
                new XAttribute("modification-date", directoryInfo.LastWriteTime.ToString("d")),
                new XAttribute("last-access-date", directoryInfo.LastAccessTime.ToString("d")),
                new XAttribute("attributes", directoryInfo.Attributes.ToString())
            };
        }

        private static string GetOwnerName(ObjectSecurity systemSecurity)
        {
            var sid = systemSecurity.GetOwner(typeof(SecurityIdentifier));
            var ntAccount = sid.Translate(typeof(NTAccount));
            var owner = ntAccount.Value;
            return owner;
        }

        private static string GetPermissions(CommonObjectSecurity fileSystemSecurity)
        {
            var accessRules = fileSystemSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier))
                .Cast<FileSystemAccessRule>()
                .Where(x => x.IdentityReference == WindowsIdentity.GetCurrent().User)
                .ToArray();

            return GetEffectiveRights(accessRules).ToString();
        }

        private static FileSystemRights GetEffectiveRights(IReadOnlyList<FileSystemAccessRule> accessRules)
        {
            FileSystemRights denyRights = 0;
            FileSystemRights allowRights = 0;

            for (int index = 0, total = accessRules.Count; index < total; index++)
            {
                var rule = accessRules[index];
                switch (rule.AccessControlType)
                {
                    case AccessControlType.Deny:
                        denyRights |= rule.FileSystemRights;
                        break;
                    case AccessControlType.Allow:
                        allowRights |= rule.FileSystemRights;
                        break;
                }
            }

            return (allowRights | denyRights) ^ denyRights;
        }

        private static float CalculateFolderSize(DirectoryInfo folder)
        {
            float folderSize = 0.0f;
            folderSize = folder.GetFiles().Aggregate(folderSize, (current, file) => current + file.Length) + folder.GetDirectories().Sum(dir => CalculateFolderSize(dir));
            return folderSize;
        }

        #endregion
    }
}
