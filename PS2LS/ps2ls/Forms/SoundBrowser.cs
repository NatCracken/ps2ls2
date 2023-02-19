using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using SD = System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using FMOD;

namespace ps2ls.Forms
{
    public partial class SoundBrowser : UserControl
    {

        #region Singleton
        private static SoundBrowser instance = null;

        public static void CreateInstance()
        {
            instance = new SoundBrowser();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static SoundBrowser Instance { get { return instance; } }
        #endregion

        public SoundBrowser()
        {
            InitializeComponent();

            soundListBox.Items.Clear();

            Dock = DockStyle.Fill;
        }

        FMOD.System system;
        Sound fsb;
        ChannelGroup channelGroup;
        Channel channel;

        private SOUND_PCMREAD_CALLBACK pcmreadcallback = new SOUND_PCMREAD_CALLBACK(PCMREADCALLBACK);
        private SOUND_PCMSETPOS_CALLBACK pcmsetposcallback = new SOUND_PCMSETPOS_CALLBACK(PCMSETPOSCALLBACK);

        private static float t1 = 0, t2 = 0;        // time
        private static float v1 = 0, v2 = 0;        // velocity

        private static RESULT PCMREADCALLBACK(IntPtr soundraw, IntPtr data, uint datalen)
        {
            unsafe
            {
                uint count;

                short* stereo16bitbuffer = (short*)data.ToPointer();

                for (count = 0; count < (datalen >> 2); count++)        // >>2 = 16bit stereo (4 bytes per sample)
                {
                    *stereo16bitbuffer++ = (short)(Math.Sin(t1) * 32767.0f);    // left channel
                    *stereo16bitbuffer++ = (short)(Math.Sin(t2) * 32767.0f);    // right channel

                    t1 += 0.01f + v1;
                    t2 += 0.0142f + v2;
                    v1 += (float)(Math.Sin(t1) * 0.002f);
                    v2 += (float)(Math.Sin(t2) * 0.002f);
                }
            }
            return RESULT.OK;
        }

        private static RESULT PCMSETPOSCALLBACK(IntPtr soundraw, int subsound, uint pcmoffset, TIMEUNIT postype)
        {
            /*
                This is useful if the user calls Sound::setTime or Sound::setPosition and you want to seek your data accordingly.
            */

            return RESULT.OK;
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



        private void initFmod()
        {
            RESULT res = Factory.System_Create(out system);

            system.init(32, INITFLAGS.NORMAL, (IntPtr)null);

            system.setFileSystem(myopen, myclose, myread, myseek, null, null, 2048);

            system.setOutput(OUTPUTTYPE.AUTODETECT);

            system.createChannelGroup("MyGroup", out channelGroup);

        }

        private void loadSound(string name)
        {
            resetTrackStatus();
            stopPlaying();
            fsb.release();

            RESULT res = system.createSound(name, MODE._2D | MODE.DEFAULT | MODE.CREATESAMPLE, out fsb);

            if (res != RESULT.OK)
            {
                MessageBox.Show("Cannot load " + name + ".  Reason: " + res.ToString(), "FMOD Load Error", MessageBoxButtons.OK);
                return;
            }

            fsb.getSubSound(0, out sound);
            res = sound.getLength(out uint len, TIMEUNIT.MS);
            if (res != RESULT.OK)
            {
                MessageBox.Show("Cannot Get Track Data.  Reason: " + res.ToString(), "FMOD Load Error", MessageBoxButtons.OK);
                return;
            }

            trackName = name;
            trackLength = len;
            refreshTrackStatus();

            createVisualization(IO.SoundExporterStatic.SoundToFloatArray(sound)[0]);
        }

        private void stopPlaying()
        {
            channel.isPlaying(out playing);
            if (playing) channel.stop();
            playing = false;

            progressTimer.Stop();
            paused = false;
            progress = 0;
            refreshProgressStatus();
        }

        bool paused;
        private void togglePause()
        {
            channel.isPlaying(out playing);
            if (!playing) return;

            paused = !paused;
            if (paused)
            {
                progressTimer.Stop();
            }
            else
            {
                progressTimer.Start();
            }
            channel.setPaused(paused);
        }

        public void onEnter(object sender, EventArgs e)
        {
            soundListBox.LoadAndSortAssets();
            resetTrackStatus();
            refreshListBox();
        }

        private int pageNumber = 0;
        private int pageSize = 1000;

        private void refreshListBox()
        {
            soundListBox.FilterBySearch(searchBox.Text ?? "");

            int filtered = soundListBox.MaxFilteredCount;

            int populateStart = pageNumber * pageSize;
            int populateEnd = populateStart + pageSize;
            if (populateEnd > filtered) populateEnd = filtered;
            soundListBox.PopulateBox(populateStart, populateEnd);

            filesListedLabel.Text = "Page " + (pageNumber + 1)
                + ": " + populateStart + " - " + populateEnd + " / " + filtered;

            createVisualization(new float[64]);
        }

        private void nextPageButton_Click(object sender, EventArgs e)
        {
            int maxPageIndex = soundListBox.MaxFilteredCount / pageSize;
            if (++pageNumber > maxPageIndex) pageNumber = maxPageIndex;
            refreshListBox();
        }

        private void lastPageButton_Click(object sender, EventArgs e)
        {
            if (--pageNumber < 0) pageNumber = 0;
            refreshListBox();
        }

        private void SoundBrowser_Load(object sender, EventArgs e)
        {
            initFmod();

            Application.Idle += onIdle;
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (searchBox.Text.Length > 0)
            {
                searchBox.BackColor = SD.Color.Yellow;
                SearchBoxClear.Enabled = true;

            }
            else
            {
                searchBox.BackColor = SD.Color.White;
                SearchBoxClear.Enabled = false;
            }

            refreshTimer.Stop();
            refreshListBox();
        }

        private void SearchBoxClear_Click(object sender, EventArgs e)
        {
            searchBox.Clear();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        FMOD.Sound sound;
        private void PlayPause_Click(object sender, EventArgs e)
        {
            if (playing)
            {
                togglePause();
                return;
            }

            fsb.getSubSound(0, out sound);
            FMOD.RESULT res = system.playSound(sound, channelGroup, false, out channel);

            if (res != FMOD.RESULT.OK)
            {
                MessageBox.Show("Cannot Play file.  Reason: " + res.ToString(), "FMOD Load Error", MessageBoxButtons.OK);
                return;
            }

            playing = true;
            progress = 0;
            refreshProgressStatus();
            progressTimer.Start();
        }

        private string timeSpanToString(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"mm\:ss\:fff");
        }

        bool playing;
        uint trackLength;
        uint progress;
        string trackName;
        private void resetTrackStatus()
        {
            trackLength = 0;
            progress = 0;
            trackName = "No Track";
            refreshTrackStatus();
        }

        private void refreshTrackStatus()
        {
            TrackNameLabel.Text = trackName;
            refreshProgressStatus();
        }

        private void refreshProgressStatus()
        {
            TrackProgressBar.Value = trackLength == 0 ? 0 : (int)(100 * progress / trackLength);
            TrackProgressLabel.Text = timeSpanToString(TimeSpan.FromMilliseconds(progress)) + " / " + timeSpanToString(TimeSpan.FromMilliseconds(trackLength));

        }

        private void progressTimer_Tick(object sender, EventArgs e)
        {
            progress += Convert.ToUInt32(progressTimer.Interval);
            if (progress >= trackLength)
            {
                progress = trackLength;
                progressTimer.Stop();
                playing = false;
            }
            refreshProgressStatus();
        }


        private void onIdle(object sender, EventArgs e)
        {
            system.update();

        }

        private void soundListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Asset asset = null;

            try
            {
                asset = (Asset)soundListBox.SelectedItem;
            }
            catch (InvalidCastException) { return; }

            loadSound(asset.Name);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            stopPlaying();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            List<String> fileNames = new List<string>();

            foreach (object selectedItem in soundListBox.SelectedItems)
            {
                Asset asset = null;

                try
                {
                    asset = (Asset)selectedItem;
                }
                catch (InvalidCastException) { continue; }

                fileNames.Add(asset.Name);
            }

            SoundExportForm modelExportForm = new SoundExportForm();
            modelExportForm.FileNames = fileNames;
            modelExportForm.ShowDialog();
        }

        private void createVisualization(float[] array)
        {
            SD.Rectangle imageSize = new SD.Rectangle(0, 0, VisualizationBox.Width, VisualizationBox.Height);
            int sampleRate = array.Length > imageSize.Width ? (int)Math.Ceiling(array.Length / (double)imageSize.Width) : 1;
            int halfHeight = imageSize.Height / 2;
            SD.Bitmap newMap = new SD.Bitmap(imageSize.Width, imageSize.Height);
            using (SD.Graphics g = SD.Graphics.FromImage(newMap))
            {
                int lastOffset = 0;
                g.FillRectangle(SD.Brushes.Navy, imageSize);
                int x = 0;
                for (int i = 0; i < array.Length; i += sampleRate)
                {
                    int newX = x + 1;
                    int newOffset = (int)(array[i] * halfHeight);
                    g.DrawLine(SD.Pens.LightGray, x, halfHeight + lastOffset, newX, halfHeight + newOffset);
                    lastOffset = newOffset;
                    x = newX;
                }
            }
            VisualizationBox.BackgroundImage = newMap;
        }
    }
}
