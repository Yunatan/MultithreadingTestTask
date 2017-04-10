using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TestTask
{
    public class TreeViewFiller
    {
        public readonly Queue<HierarchicalLink<FileSystemInfo>> FsEntriesQueue;

        private readonly TreeView treeView;
        private readonly AutoResetEvent fsEntryScannedEvent;
        private readonly Dictionary<FileSystemInfo, TreeNode> fsEntriesToTreeNodesMap;
        private readonly AddTreeNodeFunc addTreeNodeFunc;

        public TreeViewFiller(TreeView treeView, AutoResetEvent fsEntryScannedEvent)
        {
            FsEntriesQueue = new Queue<HierarchicalLink<FileSystemInfo>>();
            this.treeView = treeView;
            this.fsEntryScannedEvent = fsEntryScannedEvent;
            fsEntriesToTreeNodesMap = new Dictionary<FileSystemInfo, TreeNode>();
            addTreeNodeFunc = AddNodeAsExpanded;
        }

        private delegate int AddTreeNodeFunc(TreeNodeCollection nodes, TreeNode i);

        public void BeginListDirectoryContentToTree()
        {
            treeView.Nodes.Clear();
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
                RenderFsEntryToTreeView(newFsEntry);
            }
            else
            {
                fsEntryScannedEvent.WaitOne();
            }
        }

        private void RenderFsEntryToTreeView(HierarchicalLink<FileSystemInfo> link)
        {
            var newNode = new TreeNode(link.Child.Name);
            var parentNodeCollection = link.Parent == null
                ? treeView.Nodes
                : fsEntriesToTreeNodesMap[link.Parent].Nodes;
            treeView.Invoke(addTreeNodeFunc, parentNodeCollection, newNode);
            fsEntriesToTreeNodesMap[link.Child] = newNode;
        }

        private static int AddNodeAsExpanded(TreeNodeCollection nodes, TreeNode newNode)
        {
            var i = nodes.Add(newNode);
            if (newNode.Parent != null && newNode.Parent.IsExpanded == false)
            {
                newNode.Parent.Expand();
            }
            return i;
        }
    }
}
