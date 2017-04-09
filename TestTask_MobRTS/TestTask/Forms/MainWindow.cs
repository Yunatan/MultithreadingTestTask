using System;
using System.Threading;
using System.Windows.Forms;

namespace TestTask.Forms
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartMainLoop();
        }

        private void StartMainLoop()
        {
            if (selectDirectoryDialog.ShowDialog() == DialogResult.OK)
            {
                var directoryScanner = new DirectoryScanner(selectDirectoryDialog.SelectedPath);
                var treeViewFiller = new TreeViewFiller(treeView, directoryScanner.FsEntryScannedEvent);

                directoryScanner.PubList.Add(treeViewFiller.FsEntriesQueue);

                if (saveXmlFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var xmlWriter = new XmlWriter(saveXmlFileDialog.FileName, directoryScanner.FsEntryScannedEvent);

                    directoryScanner.PubList.Add(xmlWriter.FsEntriesQueue);

                    new Thread(() => xmlWriter.BeginWriteDirectoryContentToFile()) {IsBackground = true}.Start();
                }

                new Thread(() => directoryScanner.BeginScanDirectoryContent()) {IsBackground = true}.Start();
                new Thread(() => treeViewFiller.BeginListDirectoryContentToTree()) {IsBackground = true}.Start();
            }
        }
    }
}
