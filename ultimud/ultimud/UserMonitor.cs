using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public partial class UserMonitor : Form
    {
        User user_;

        public UserMonitor(User user)
        {
            InitializeComponent();
            this.user_ = user;
            user.InputReceived += this.user_InputReceived;
            this.toolStripButton1.Checked = user.Flags.Get((int)User.UserFlags.Silenced);
            this.Text = "Monitoring: " + user.ToString();
        }

        void user_InputReceived(User t1, string t2)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Function_void<User, string>(this.user_InputReceived), t1, t2);
            }
            else
            {
                this.listView1.Items.Add(new ListViewItem(new string[] { DateTime.Now.ToString(), t2 }));
                this.listView1.EnsureVisible(this.listView1.Items.Count - 1);
                this.Text = "Monitoring: " + this.user_.ToString();
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
                    foreach (ListViewItem i in this.listView1.Items)
                        writer.WriteLine(i.Text + "\t" + i.SubItems[1].Text);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.user_.InputReceived -= this.user_InputReceived;
            base.OnClosing(e);
        }
    }
}