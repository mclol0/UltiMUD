using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Damdai.Forms
{
    public partial class TextEditor : UserControl
    {
        List<KeyValuePair<string, int>> historybuffer_ = new List<KeyValuePair<string, int>>();
        int historyindex_;
        bool updatehistory_ = true;

        public event Function_void Save;
        public new event Function_void TextChanged;

        public override string Text
        {
            get { return this.richTextBoxEx1.Text; }
            set { this.richTextBoxEx1.Text = value; }
        }
        public override Color BackColor
        {
            get
            {
                return this.richTextBoxEx1.BackColor;
            }
            set
            {
                this.richTextBoxEx1.BackColor = value;
            }
        }
        public override Color ForeColor
        {
            get
            {
                return this.richTextBoxEx1.ForeColor;
            }
            set
            {
                this.richTextBoxEx1.ForeColor = value;
            }
        }
        public BorderStyle BorderStyle
        {
            get { return this.richTextBoxEx1.BorderStyle; }
            set { this.richTextBoxEx1.BorderStyle = value; }
        }

        public TextEditor()
        {
            InitializeComponent();
            this.toolStripLabel1.Text = null;
            this.highlighter.Parser = new RtfParser(this.richTextBoxEx1);
        }

        void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (this.Save != null)
                this.Save();
        }
        void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.richTextBoxEx1.WordWrap = this.toolStripButton3.Checked;
        }
        void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.toolStripLabel1.Text = "Line: " + (this.richTextBoxEx1.GetLineFromCharIndex(this.richTextBoxEx1.SelectionStart) + 1);
        }
        void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            this.toolStripLabel1.Text = "Line: " + (this.richTextBoxEx1.GetLineFromCharIndex(this.richTextBoxEx1.SelectionStart) + 1);
        }
        void richTextBoxEx1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        if (this.Save != null)
                            this.Save();
                        break;
                    case Keys.Z:
                        this.Undo();
                        break;
                    case Keys.Y:
                        this.Redo();
                        break;
                }
            }
        }
        void richTextBoxEx1_TextChanged(object sender, EventArgs e)
        {
            this.UpdateHistoryBuffer();
            this.UpdateText();
            if (this.TextChanged != null)
                this.TextChanged();
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.UpdateText();
        }
        void UpdateText()
        {
            if (this.richTextBoxEx1.EnablePaint)
            {
                this.richTextBoxEx1.EnablePaint = false;
                Win32.POINT pos = this.GetScrollPos();
                int start = this.richTextBoxEx1.SelectionStart;
                this.richTextBoxEx1.TextChanged -= this.richTextBoxEx1_TextChanged;
                this.richTextBoxEx1.Rtf = this.highlighter.Parse(this.richTextBoxEx1.Text);
                this.richTextBoxEx1.TextChanged += this.richTextBoxEx1_TextChanged;
                this.richTextBoxEx1.SelectionStart = start;
                this.SetScrollPos(pos);
                this.richTextBoxEx1.EnablePaint = true;
            }
        }
        void UpdateHistoryBuffer()
        {
            if (this.updatehistory_)
            {
                int count = this.historybuffer_.Count - 1;
                if (this.historyindex_ < count)
                    this.historybuffer_.RemoveRange(this.historyindex_ + 1, count - this.historyindex_);

                this.historybuffer_.Add(new KeyValuePair<string, int>(this.richTextBoxEx1.Text, this.richTextBoxEx1.SelectionStart));
                this.historyindex_ = this.historybuffer_.Count - 1;
            }
        }
        void Undo()
        {
            if (this.historyindex_ > 0)
            {
                KeyValuePair<string, int> history = this.historybuffer_[--this.historyindex_];
                this.updatehistory_ = false;
                this.richTextBoxEx1.Text = history.Key;
                this.updatehistory_ = true;
                this.richTextBoxEx1.SelectionStart = history.Value;
            }
        }
        void Redo()
        {
            if (this.historyindex_ < this.historybuffer_.Count - 1)
            {
                KeyValuePair<string, int> history = this.historybuffer_[++this.historyindex_];
                this.updatehistory_ = false;
                this.richTextBoxEx1.Text = history.Key;
                this.updatehistory_ = true;
                this.richTextBoxEx1.SelectionStart = history.Value;
            }
        }

        public void AddButton(string text, Function_void fn)
        {
            ToolStripButton button = new ToolStripButton(text);
            button.Click += delegate(object sender, EventArgs e) { fn(); };
            this.toolStrip1.Items.Add(button);
        }

        #region Scrollbar positions functions
        unsafe Win32.POINT GetScrollPos()
        {
            Win32.POINT res = new Win32.POINT();
            IntPtr ptr = new IntPtr(&res);
            Win32.SendMessage(this.richTextBoxEx1.Handle, Win32.EM_GETSCROLLPOS, 0, ptr);
            return res;
        }

        unsafe void SetScrollPos(Win32.POINT point)
        {
            IntPtr ptr = new IntPtr(&point);
            Win32.SendMessage(this.richTextBoxEx1.Handle, Win32.EM_SETSCROLLPOS, 0, ptr);
        }
        #endregion

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            int start = this.richTextBoxEx1.Find(this.toolStripTextBox1.Text);
            if (start != -1)
                this.richTextBoxEx1.Select(start, this.toolStripTextBox1.Text.Length);
            else
                this.richTextBoxEx1.DeselectAll();
        }
        private void toolStrip1_Enter(object sender, EventArgs e)
        {
            if (this.toolStripTextBox1.ForeColor == Color.Silver)
            {
                this.toolStripTextBox1.Text = null;
                this.toolStripTextBox1.ForeColor = Color.Black;
            }
        }
        private void toolStrip1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.toolStripTextBox1.Text))
            {
                this.toolStripTextBox1.Text = "Search";
                this.toolStripTextBox1.ForeColor = Color.Silver;
            }
        }
        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int start = this.richTextBoxEx1.Find(
                        this.toolStripTextBox1.Text,
                        this.richTextBoxEx1.SelectionStart + this.richTextBoxEx1.SelectionLength,
                        RichTextBoxFinds.None);
                if(start == -1)
                    start = this.richTextBoxEx1.Find(this.toolStripTextBox1.Text);

                if (start != -1)
                    this.richTextBoxEx1.Select(start, this.toolStripTextBox1.Text.Length);
                else
                    this.richTextBoxEx1.DeselectAll();

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}
