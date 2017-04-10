﻿using System;
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
            var treeViewFiller = new TreeViewFiller(treeView, directoryScanner.FsEntryScannedEvent);
            var xmlWriter = new XmlWriter(selectedFileName, directoryScanner.FsEntryScannedEvent);

            directoryScanner.PubList.Add(treeViewFiller.FsEntriesQueue);
            directoryScanner.PubList.Add(xmlWriter.FsEntriesQueue);

            var workers = new List<ThreadStart>
            {
                xmlWriter.BeginWriteDirectoryContentToFile,
                directoryScanner.BeginScanDirectoryContent,
                treeViewFiller.BeginListDirectoryContentToTree
            };
            workers.ForEach(w => new Thread(w) {IsBackground = true}.Start());
        }
    }
}