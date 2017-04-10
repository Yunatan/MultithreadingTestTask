using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Linq;

namespace TestTask
{
    internal class FsEntryXmlSerializer
    {
        public static XElement ToXElement(FileSystemInfo fsInfo)
        {
            var directoryInfo = fsInfo as DirectoryInfo;
            if (directoryInfo != null)
            {
                return ToXElement(directoryInfo); 
            }

            var fileInfo = fsInfo as FileInfo;
            if (fileInfo != null)
            {
                return ToXElement(fileInfo);
            }

            throw new ArgumentException();
        }

        public static XElement ToXElement(DirectoryInfo directoryInfo)
        {
            var info = new XElement("directory", GetCommonAttributes(directoryInfo));
            try
            {
                info.SetAttributeValue("size", CalculateFolderSize(directoryInfo));
            }
            catch (UnauthorizedAccessException)
            {
                info.SetAttributeValue("size", "N/A");
            }

            try
            {
                info.SetAttributeValue("owner", GetOwnerName(directoryInfo.GetAccessControl()));
            }
            catch (UnauthorizedAccessException)
            {
                info.SetAttributeValue("owner", "N/A");
            }

            try
            {
                info.SetAttributeValue("permissions", GetPermissions(directoryInfo.GetAccessControl()));
            }
            catch (UnauthorizedAccessException)
            {
                info.SetAttributeValue("permissions", "N/A");
            }

            return info;
        }

        public static XElement ToXElement(FileInfo fileInfo)
        {
            var info = new XElement("file", GetCommonAttributes(fileInfo));

            info.SetAttributeValue("size", fileInfo.Length);
            info.SetAttributeValue("owner", GetOwnerName(fileInfo.GetAccessControl()));
            info.SetAttributeValue("permissions", GetPermissions(fileInfo.GetAccessControl()));

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

        private static long CalculateFolderSize(DirectoryInfo folder)
        {
            long folderSize = 0;
            var aggregatedFilesLength = folder.GetFiles().Aggregate(folderSize, (current, file) => current + file.Length);
            var recursiveFolderSizeSum = folder.GetDirectories().Sum(dir => CalculateFolderSize(dir));
            folderSize = aggregatedFilesLength + recursiveFolderSizeSum;
            return folderSize;
        }
    }
}
