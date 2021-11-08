using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ps2ls
{
    partial class AboutBox : Form
    {
        #region Singleton
        private static AboutBox instance = null;

        public static void CreateInstance()
        {
            instance = new AboutBox();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static AboutBox Instance { get { return instance; } }
        #endregion

        public AboutBox()
        {
            InitializeComponent();
            //TODO retrieve version number
        }
    }
}
