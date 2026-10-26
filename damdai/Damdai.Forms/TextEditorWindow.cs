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
        bool syntaxHighlight_;
        LuaHighlighter highlighter_;
        bool updating_;

        public bool SyntaxHighlight
        {
            get { return this.syntaxHighlight_; }
            set
            {
                this.syntaxHighlight_ = value;
                if (this.syntaxHighlight_ && this.highlighter_ == null)
                {
                    this.highlighter_ = new LuaHighlighter(new RtfParser(this.richTextBox1));
                }
                if (this.syntaxHighlight_)
                {
                    this.HighlightText();
                }
            }
        }

        public TextEditorWindow()
            : this(false)
        {
        }

        public TextEditorWindow(bool syntaxHighlight)
        {
            InitializeComponent();
            base.Text = "Text Editor";
            this.toolStripStatusLabel1.Text = null;
            this.SyntaxHighlight = syntaxHighlight;
            this.richTextBox1.TextChanged += new EventHandler(this.richTextBox1_TextChanged);
        }

        public override string Text
        {
            get { return this.richTextBox1 != null ? this.richTextBox1.Text : base.Text; }
            set
            {
                if (this.richTextBox1 != null)
                {
                    this.richTextBox1.Text = value ?? string.Empty;
                    if (this.syntaxHighlight_)
                    {
                        this.HighlightText();
                    }
                }
                else
                {
                    base.Text = value;
                }
            }
        }

        private void HighlightText()
        {
            if (!this.syntaxHighlight_ || this.highlighter_ == null || this.updating_)
                return;

            try
            {
                this.updating_ = true;
                int selectionStart = this.richTextBox1.SelectionStart;
                int selectionLength = this.richTextBox1.SelectionLength;
                this.richTextBox1.Rtf = this.highlighter_.Parse(this.richTextBox1.Text);
                this.richTextBox1.SelectionStart = selectionStart;
                this.richTextBox1.SelectionLength = selectionLength;
            }
            finally
            {
                this.updating_ = false;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (this.syntaxHighlight_)
            {
                this.HighlightText();
            }
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
