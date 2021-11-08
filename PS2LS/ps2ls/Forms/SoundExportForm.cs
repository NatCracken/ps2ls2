﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets.Pack;

namespace ps2ls.Forms
{
    public partial class SoundExportForm : Form
    {
        public SoundExportForm()
        {
            InitializeComponent();
        }

        public List<String> FileNames { get; set; }

        private GenericLoadingForm loadingForm;
        private BackgroundWorker exportBackgroundWorker = new BackgroundWorker();

        private FMOD.FILE_OPEN_CALLBACK myopen = new FMOD.FILE_OPEN_CALLBACK(OPENCALLBACK);
        private FMOD.FILE_CLOSE_CALLBACK myclose = new FMOD.FILE_CLOSE_CALLBACK(CLOSECALLBACK);
        private FMOD.FILE_READ_CALLBACK myread = new FMOD.FILE_READ_CALLBACK(READCALLBACK);
        private FMOD.FILE_SEEK_CALLBACK myseek = new FMOD.FILE_SEEK_CALLBACK(SEEKCALLBACK);

        static MemoryStream memStream;
        private static FMOD.RESULT OPENCALLBACK(IntPtr name, ref uint filesize, ref IntPtr handle, IntPtr userdata)
        {
            FMOD.StringWrapper stringWrapper = new FMOD.StringWrapper(name);
            memStream = AssetManager.Instance.CreateAssetMemoryStreamByName(stringWrapper);
            if (memStream == null) return FMOD.RESULT.ERR_FILE_NOTFOUND;
            memStream = Utils.FixSoundHeader(memStream);
            filesize = (uint)memStream.Length;

            return FMOD.RESULT.OK;
        }

        private static FMOD.RESULT CLOSECALLBACK(IntPtr handle, IntPtr userdata)
        {
            memStream.Close();

            return FMOD.RESULT.OK;
        }

        private static FMOD.RESULT READCALLBACK(IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata)
        {
            byte[] readbuffer = new byte[sizebytes];

            bytesread = (uint)memStream.Read(readbuffer, 0, (int)sizebytes);
            if (bytesread == 0)
            {
                return FMOD.RESULT.ERR_FILE_EOF;
            }

            Marshal.Copy(readbuffer, 0, buffer, (int)sizebytes);

            return FMOD.RESULT.OK;
        }

        private static FMOD.RESULT SEEKCALLBACK(IntPtr handle, uint pos, IntPtr userdata)
        {
            memStream.Seek(pos, SeekOrigin.Begin);
            return FMOD.RESULT.OK;
        }


        FMOD.System system;
        FMOD.Sound fsb;
        private void initFmod()
        {
            FMOD.RESULT res;

            res = FMOD.Factory.System_Create(out system);

            system.setOutput(FMOD.OUTPUTTYPE.WAVWRITER);

            system.init(32, FMOD.INITFLAGS.STREAM_FROM_UPDATE, (IntPtr)null);

            system.setFileSystem(myopen, myclose, myread, myseek, null, null, 2048);
        }

        private void loadSound(string name)
        {
            FMOD.RESULT res = system.createSound(name, (FMOD.MODE._2D | FMOD.MODE.DEFAULT | FMOD.MODE.CREATESTREAM), out fsb);

            if (res != FMOD.RESULT.OK)
            {
                MessageBox.Show("Cannot load " + name + ".  Reason: " + res.ToString(), "FMOD Load Error", MessageBoxButtons.OK);
            }

        }


        private int DoExportSound(object sender, object arg)
        {
            List<object> arguments = (List<object>)arg;

            String directory = (String)arguments[0];
            List<String> fileNames = (List<String>)arguments[1];
            string wav = (string)arguments[2];

            BackgroundWorker backgroundWorker = (BackgroundWorker)sender;

            initFmod();

            Int32 result = 0;

            //TODO export sound

            return result;
        }

    }
}
