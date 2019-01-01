using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Damdai.Forms
{
    public partial class Dialog : Form
    {
        public Dialog(Control control, string windowTitle)
        {
            InitializeComponent();

            this.splitContainer1.Panel1.Controls.Add(control);
            control.Dock = DockStyle.Fill;
            this.Text = windowTitle;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}