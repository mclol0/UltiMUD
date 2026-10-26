using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public partial class PlayerMonitor : UserControl
    {
        User user_;

        public event Function_void CloseEvent;

        public PlayerMonitor(User user)
        {
            InitializeComponent();
            this.user_ = user;
            user.InputReceived += this.user_InputReceived;
            this.toolStripButton1.Checked = user.Flags.Get((int)User.UserFlags.Silenced);
        }

        void user_InputReceived(User t1, string t2)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            if (this.InvokeRequired)
                this.Invoke(new Function_void<User, string>(this.user_InputReceived), t1, t2);
            else
            {
                if (this.richTextBox1.TextLength > 0)
                    this.richTextBox1.AppendText("\r\n");
                this.richTextBox1.AppendText(DateTime.Now + ": " + t2);
                this.richTextBox1.ScrollToCaret();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.toolStripButton1.Checked = !this.toolStripButton1.Checked;
            this.user_.Flags.Set((int)User.UserFlags.Silenced, this.toolStripButton1.Checked);
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = File.CreateText(this.saveFileDialog1.FileName))
                {
                    foreach (string str in this.richTextBox1.Lines)
                        writer.WriteLine(str);
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.user_.InputReceived -= this.user_InputReceived;
            if (this.CloseEvent != null)
                this.CloseEvent();
        }
    }
}
