using ps2ls.Assets.Dme;
using ps2ls.Assets.Pack;
using ps2ls.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using DevIL;
using OpenTK;
using ps2ls.Graphics.Materials;
using System.Globalization;

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
            public TextureExporter.TextureFormatInfo textureFormat;
        }

        private void exportDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = exportTexture(sender, e.Argument);
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
                loadingForm.SetLabelText((String)e.UserState);
                loadingForm.SetProgressBarPercent(e.ProgressPercentage);
            }
        }

        private Int32 exportTexture(object sender, object argument)
        {
            List<object> arguments = (List<object>)argument;

            String directory = (String)arguments[0];
            List<String> fileNames = (List<String>)arguments[1];
            ImageExportOptions exportOptions = (ImageExportOptions)arguments[2];

           // BackgroundWorker backgroundWorker = (BackgroundWorker)sender;

            Int32 result = 0;

            ImageImporter imageImporter = new ImageImporter();
            ImageExporter imageExporter = new ImageExporter();

            foreach (string textureString in fileNames)
            {
                MemoryStream textureMemoryStream = AssetManager.Instance.CreateAssetMemoryStreamByName(textureString);

                if (textureMemoryStream == null)
                    continue;

                Image textureImage = imageImporter.LoadImageFromStream(textureMemoryStream);

                if (textureImage == null)
                    continue;

                imageExporter.SaveImage(textureImage, exportOptions.textureFormat.ImageType, directory + @"\" + Path.GetFileNameWithoutExtension(textureString) + @"." + exportOptions.textureFormat.Extension);
                result++;
            }

            imageImporter.Dispose();
            imageExporter.Dispose();

            return result;
        }


        private void loadTextureFormatComboBox()
        {
            textureFormatComboBox.Items.Clear();

            foreach (TextureExporter.TextureFormatInfo textureFormat in TextureExporter.TextureFormats)
            {
                textureFormatComboBox.Items.Add(textureFormat);
            }

            textureFormatComboBox.SelectedIndex = textureFormatComboBox.Items.Count > 0 ? 2 : -1;//2 = png
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


        private void applyCurrentStateToExportOptions()
        {
            imageExportOptions.textureFormat = (TextureExporter.TextureFormatInfo)textureFormatComboBox.SelectedItem;
        }

        private void textureFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            imageExportOptions.textureFormat = (TextureExporter.TextureFormatInfo)textureFormatComboBox.SelectedItem;
        }
    }
}
