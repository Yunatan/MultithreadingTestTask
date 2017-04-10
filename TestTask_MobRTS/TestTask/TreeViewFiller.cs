using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TestTask
{
    internal class TreeViewFiller : IConsumer
    {
        private readonly ConsumerComponent consumerComponent;
        private readonly TreeView treeView;
        private readonly Dictionary<FileSystemInfo, TreeNode> fsEntriesToTreeNodesMap;
        private readonly AddTreeNodeFunc addTreeNodeFunc;

        public TreeViewFiller(TreeView treeView, ConsumerComponent consumerComponent)
        {
            this.treeView = treeView;
            this.consumerComponent = consumerComponent;
            fsEntriesToTreeNodesMap = new Dictionary<FileSystemInfo, TreeNode>();
            addTreeNodeFunc = AddNodeAsExpanded;
            treeView.Nodes.Clear();
        }

        private delegate int AddTreeNodeFunc(TreeNodeCollection nodes, TreeNode i);

        public Queue<HierarchicalLink<FileSystemInfo>> GetEntriesQueue()
        {
            return consumerComponent.FsEntriesQueue;
        }

        public void BeginConsume()
        {
            consumerComponent.BeginConsume(RenderFsEntryToTreeView);
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
