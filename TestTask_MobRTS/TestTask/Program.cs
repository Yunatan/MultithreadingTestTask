using System;
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
            Application.Run(new MainWindow()); 
        }
    }
}
