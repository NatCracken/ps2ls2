using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets;
using FMOD;
using ps2ls.IO;

namespace ps2ls.Forms
{
    public partial class SoundExportForm : Form
    {
        public SoundExportForm()
        {
            InitializeComponent();
        }

        private FILE_OPEN_CALLBACK myopen = new FILE_OPEN_CALLBACK(OPENCALLBACK);
        private FILE_CLOSE_CALLBACK myclose = new FILE_CLOSE_CALLBACK(CLOSECALLBACK);
        private FILE_READ_CALLBACK myread = new FILE_READ_CALLBACK(READCALLBACK);
        private FILE_SEEK_CALLBACK myseek = new FILE_SEEK_CALLBACK(SEEKCALLBACK);

        static MemoryStream memStream;
        private static RESULT OPENCALLBACK(IntPtr name, ref uint filesize, ref IntPtr handle, IntPtr userdata)
        {
            StringWrapper stringWrapper = new StringWrapper(name);
            memStream = AssetManager.Instance.CreateAssetMemoryStreamByName(stringWrapper);
            if (memStream == null) return FMOD.RESULT.ERR_FILE_NOTFOUND;
            memStream = Utils.FixSoundHeader(memStream);
            filesize = (uint)memStream.Length;

            return RESULT.OK;
        }

        private static RESULT CLOSECALLBACK(IntPtr handle, IntPtr userdata)
        {
            memStream.Close();

            return RESULT.OK;
        }

        private static RESULT READCALLBACK(IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata)
        {
            byte[] readbuffer = new byte[sizebytes];

            bytesread = (uint)memStream.Read(readbuffer, 0, (int)sizebytes);
            if (bytesread == 0)
            {
                return RESULT.ERR_FILE_EOF;
            }

            Marshal.Copy(readbuffer, 0, buffer, (int)sizebytes);

            return RESULT.OK;
        }

        private static RESULT SEEKCALLBACK(IntPtr handle, uint pos, IntPtr userdata)
        {
            memStream.Seek(pos, SeekOrigin.Begin);
            return RESULT.OK;
        }

        public List<String> FileNames { get; set; }

        private GenericLoadingForm loadingForm;
        private BackgroundWorker exportBackgroundWorker = new BackgroundWorker();
        private SoundExportOptions soundExportOptions = new SoundExportOptions();
        class SoundExportOptions
        {
            public SoundExporterStatic.SoundFormatInfo soundFormat;
        }

        private void exportDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = exportSounds(sender, e.Argument);
        }

        private void exportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loadingForm.Close();

            Close();

            MessageBox.Show("Successfully exported " + (Int32)e.Result + " sounds.");
        }

        private void exportProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (loadingForm != null)
            {
                loadingForm.SetLabelText((string)e.UserState);
                loadingForm.SetProgressBarPercent(e.ProgressPercentage);
            }
        }

        FMOD.System system;
        private int exportSounds(object sender, object argument)
        {
            List<object> arguments = (List<object>)argument;

            string directory = (string)arguments[0];
            List<string> fileNames = (List<string>)arguments[1];
            SoundExportOptions exportOptions = (SoundExportOptions)arguments[2];

            Factory.System_Create(out system);

            system.init(32, INITFLAGS.NORMAL, (IntPtr)null);

            system.setFileSystem(myopen, myclose, myread, myseek, null, null, 2048);

            system.setOutput(OUTPUTTYPE.AUTODETECT);

            int result = 0;

            foreach (string textureString in fileNames)
                if (exportSound(textureString, directory)) result++;

            system.release();

            return result;
        }


        private bool exportSound(string textureString, string directory)
        {
            RESULT res = system.createSound(textureString, MODE._2D | MODE.DEFAULT | MODE.CREATESAMPLE, out Sound fsb);

            if (res != RESULT.OK)
            {
                MessageBox.Show("Cannot load " + textureString + ".  Reason: " + res.ToString(), "FMOD Load Error", MessageBoxButtons.OK);
                return false;
            }

            fsb.getSubSound(0, out Sound sound);
            bool toReturn = SoundExporterStatic.exportSound(sound, textureString, directory, soundExportOptions.soundFormat);
            fsb.release();
            return toReturn;
        }

        private void loadSoundFormatComboBox()
        {
            soundFormatComboBox.Items.Clear();

            foreach (SoundExporterStatic.SoundFormatInfo soundFormat in SoundExporterStatic.SoundFormats)
            {
                soundFormatComboBox.Items.Add(soundFormat);
            }

            soundFormatComboBox.SelectedIndex = 0;
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
                    soundExportOptions
                };

                loadingForm = new GenericLoadingForm();
                loadingForm.Show();

                exportBackgroundWorker.RunWorkerAsync(argument);
            }
        }

        private void applyCurrentStateToExportOptions()
        {
            soundExportOptions.soundFormat = (SoundExporterStatic.SoundFormatInfo)soundFormatComboBox.SelectedItem;
        }

        private void SoundExportForm_Load(object sender, EventArgs e)
        {
            if (ModelExporterStatic.outputDirectory == null) ModelExporterStatic.outputDirectory = Application.StartupPath;
            exportFolderBrowserDialog.SelectedPath = ModelExporterStatic.outputDirectory;

            exportBackgroundWorker.WorkerReportsProgress = true;
            exportBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(exportProgressChanged);
            exportBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(exportRunWorkerCompleted);
            exportBackgroundWorker.DoWork += new DoWorkEventHandler(exportDoWork);

            loadSoundFormatComboBox();
        }

        private void soundFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            soundExportOptions.soundFormat = (SoundExporterStatic.SoundFormatInfo)soundFormatComboBox.SelectedItem;
        }
    }
}
