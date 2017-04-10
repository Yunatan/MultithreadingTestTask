using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using TestTask.Forms;

namespace TestTask
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SetupExceptionHandling();
            Application.Run(new MainWindow());
        }

        private static void SetupExceptionHandling()
        {
            Application.ThreadException += ExceptionsHandler.HandleUiThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += ExceptionsHandler.HandleUnhandledException;
        }

        internal static void StartMainLoop(string selectedPath, string selectedFileName, TreeView treeView)
        {
            var directoryScanner = new DirectoryScanner(selectedPath);

            var treeViewFiller = new TreeViewFiller(treeView, new ConsumerComponent(directoryScanner.FsEntryScannedEvent));
            var xmlWriter = new XmlWriter(selectedFileName, new ConsumerComponent(directoryScanner.FsEntryScannedEvent));

            directoryScanner.PubList.Add(treeViewFiller.GetEntriesQueue());
            directoryScanner.PubList.Add(xmlWriter.GetEntriesQueue());

            var workers = new List<ThreadStart>
            {
                directoryScanner.BeginScanDirectoryContent,
                xmlWriter.BeginConsume,
                treeViewFiller.BeginConsume
            };
            workers.ForEach(w => new Thread(w) {IsBackground = true}.Start());
        }
    }
}