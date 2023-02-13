using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ps2ls
{
    public partial class GenericLoadingForm : Form
    {
        public GenericLoadingForm()
        {
            InitializeComponent();
        }

        public void SetWindowTitle(string title)
        {
            Text = title;
        }

        public void SetProgressBarPercent(int percent)
        {
            progressBar1.Value = percent;
        }

        public void SetLabelText(string text)
        {
            label1.Text = text;
        }
    }
}
