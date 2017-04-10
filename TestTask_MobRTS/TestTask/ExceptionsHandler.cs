using System;
using System.Threading;
using System.Windows.Forms;

namespace TestTask
{
    internal class ExceptionsHandler
    {
        internal static void HandleUiThreadException(object sender, ThreadExceptionEventArgs t)
        {
            var result = DialogResult.Cancel;
            try
            {
                result = ShowThreadExceptionDialog("Windows Forms Error", t.Exception);
            }
            catch
            {
                try
                {
                    MessageBox.Show("Fatal Windows Forms Error",
                        "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }

            if (result == DialogResult.Abort)
            {
                Application.Exit();
            }
        }

        internal static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var result = DialogResult.Cancel;
            try
            {
                result = ShowThreadExceptionDialog("Non-UI Error", (Exception) e.ExceptionObject);
            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("Fatal Non-UI Error. Could not report the error to user. Reason: "
                                    + exc.Message, "Fatal Non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }

            if (result == DialogResult.Abort)
            {
                Application.Exit();
            }
        }

        private static DialogResult ShowThreadExceptionDialog(string title, Exception e)
        {
            var errorMsg = "An application error occurred. Please contact the administrator with the following information:\n\n";
            errorMsg = errorMsg + e.Message + "\n\nStack Trace:\n" + e.StackTrace;
            return MessageBox.Show(errorMsg, title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }
    }
}
