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

            if (selectDirectoryDialog.ShowDialog() == DialogResult.OK)
            {
                var selectedPath = selectDirectoryDialog.SelectedPath;

                if (saveXmlFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedFileName = saveXmlFileDialog.FileName;
                    Program.StartMainLoop(selectedPath, selectedFileName, treeView);
                }
            }
        }
    }
}
