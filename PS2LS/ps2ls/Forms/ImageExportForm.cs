using ps2ls.Assets.Dme;
using ps2ls.Assets.Pack;
using ps2ls.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;


namespace ps2ls.Forms
{
    public partial class ImageExportForm : Form
    {
        public ImageExportForm()
        {
            InitializeComponent();
        }

        public List<String> FileNames { get; set; }

        private GenericLoadingForm loadingForm;
        private BackgroundWorker exportBackgroundWorker = new BackgroundWorker();
        private ImageExportOptions imageExportOptions = new ImageExportOptions();
        class ImageExportOptions
        {
            public TextureExporterStatic.TextureFormatInfo textureFormat;
        }

        private void exportDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = exportTextures(sender, e.Argument);
        }

        private void exportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loadingForm.Close();

            Close();

            MessageBox.Show("Successfully exported " + (Int32)e.Result + " textures.");
        }

        private void exportProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (loadingForm != null)
            {
                loadingForm.SetLabelText((string)e.UserState);
                loadingForm.SetProgressBarPercent(e.ProgressPercentage);
            }
        }

        private int exportTextures(object sender, object argument)
        {
            List<object> arguments = (List<object>)argument;

            string directory = (string)arguments[0];
            List<string> fileNames = (List<string>)arguments[1];
            ImageExportOptions exportOptions = (ImageExportOptions)arguments[2];

            int result = 0;

            foreach (string textureString in fileNames)
                if (TextureExporterStatic.exportTexture(textureString, directory, exportOptions.textureFormat)) result++;

            return result;
        }


        private void loadTextureFormatComboBox()
        {
            textureFormatComboBox.Items.Clear();

            foreach (TextureExporterStatic.TextureFormatInfo textureFormat in TextureExporterStatic.TextureFormats)
            {
                textureFormatComboBox.Items.Add(textureFormat);
            }

            textureFormatComboBox.SelectedIndex = 0;
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            applyCurrentStateToExportOptions();

            if (exportFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                ModelExporterStatic.outputDirectory = exportFolderBrowserDialog.SelectedPath;
                List<object> argument = new List<object>()
                {
                    exportFolderBrowserDialog.SelectedPath,
                    FileNames,
                    imageExportOptions
                };

                loadingForm = new GenericLoadingForm();
                loadingForm.Show();

                exportBackgroundWorker.RunWorkerAsync(argument);
            }
        }
        private void applyCurrentStateToExportOptions()
        {
            imageExportOptions.textureFormat = (TextureExporterStatic.TextureFormatInfo)textureFormatComboBox.SelectedItem;
        }

        private void ImageExportForm_Load(object sender, EventArgs e)
        {
            if (ModelExporterStatic.outputDirectory == null) ModelExporterStatic.outputDirectory = Application.StartupPath;
            exportFolderBrowserDialog.SelectedPath = ModelExporterStatic.outputDirectory;

            exportBackgroundWorker.WorkerReportsProgress = true;
            exportBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(exportProgressChanged);
            exportBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(exportRunWorkerCompleted);
            exportBackgroundWorker.DoWork += new DoWorkEventHandler(exportDoWork);

            loadTextureFormatComboBox();
        }

        private void textureFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            imageExportOptions.textureFormat = (TextureExporterStatic.TextureFormatInfo)textureFormatComboBox.SelectedItem;
        }
    }
}
