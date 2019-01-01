using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Damdai.Forms
{
    public partial class TextEditorWindow : Form
    {
        public TextEditorWindow()
        {
            InitializeComponent();
            base.Text = "Text Editor";
            this.toolStripStatusLabel1.Text = null;
        }

        public override string Text
        {
            get { return this.richTextBox1.Text; }
            set { this.richTextBox1.Text = value; }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.richTextBox1.WordWrap = this.toolStripButton3.Checked;
        }

        private void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Line: " + (this.richTextBox1.GetLineFromCharIndex(this.richTextBox1.SelectionStart) + 1);
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Line: " + (this.richTextBox1.GetLineFromCharIndex(this.richTextBox1.SelectionStart) + 1);
        }
    }
}