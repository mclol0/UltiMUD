using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Damdai.Forms
{
    public partial class TextEditorWindow : Form
    {
        public class ApiEntry
        {
            public string Category;
            public string Label;
            public string InsertText;
            public string Signature;
            public string Summary;
            public string Example;
            public string OwnerPrefix; // e.g., "user", "char", "Command", "server", "area", "color", or "" for global/guide

            public ApiEntry(string category, string ownerPrefix, string label, string insertText, string signature, string summary, string example)
            {
                this.Category = category;
                this.OwnerPrefix = ownerPrefix;
                this.Label = label;
                this.InsertText = insertText;
                this.Signature = signature;
                this.Summary = summary;
                this.Example = example;
            }

            public override string ToString()
            {
                return this.Label + "   —   " + this.Signature;
            }
        }

        bool syntaxHighlight_;
        LuaHighlighter highlighter_;
        bool updating_;

        // VS Code-style UI additions
        SplitContainer mainSplit_;
        Panel editorHostPanel_;
        Label signatureBanner_;
        ListBox intelliSenseList_;
        List<ApiEntry> currentCompletions_ = new List<ApiEntry>();
        string currentPrefix_ = string.Empty;
        int completionReplaceStart_ = 0;
        int completionReplaceLength_ = 0;

        // Right-hand Scripting Guide & API Explorer
        SplitContainer rightSplit_;
        TextBox apiSearchBox_;
        TreeView apiTree_;
        Label docSignatureHeader_;
        RichTextBox apiDocBox_;
        Button btnInsertApi_;
        Button btnCopyExample_;

        // Extra toolbar items
        ToolStripSeparator sepIde_;
        ToolStripDropDownButton btnSnippets_;
        ToolStripDropDownButton btnColors_;
        ToolStripButton btnToggleGuide_;
        ToolStripStatusLabel statusSignatureLabel_;

        static List<ApiEntry> catalog_;

        public static List<ApiEntry> Catalog
        {
            get
            {
                if (catalog_ == null)
                    catalog_ = BuildCatalog();
                return catalog_;
            }
        }

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
                if (this.mainSplit_ != null)
                {
                    this.mainSplit_.Panel2Collapsed = !this.syntaxHighlight_;
                    if (this.btnToggleGuide_ != null)
                        this.btnToggleGuide_.Checked = this.syntaxHighlight_;
                }
                if (this.syntaxHighlight_)
                {
                    this.Text = "UltiMUD Lua Script Studio";
                    this.MinimumSize = new Size(920, 560);
                    if (this.Width < 1100 || this.Height < 680)
                        this.Size = new Size(1180, 740);
                    this.ApplyIdeLayoutProportions();
                    this.HighlightText();
                }
            }
        }

        public ListBox IntelliSensePopup
        {
            get { return this.intelliSenseList_; }
        }

        public TreeView ApiReferenceTree
        {
            get { return this.apiTree_; }
        }

        public int GuidePanelWidth
        {
            get { return this.mainSplit_ != null ? this.mainSplit_.Panel2.Width : 0; }
        }

        public int GuideDocPanelHeight
        {
            get { return this.rightSplit_ != null ? this.rightSplit_.Panel2.Height : 0; }
        }

        public TextEditorWindow()
            : this(false)
        {
        }

        public TextEditorWindow(bool syntaxHighlight)
        {
            InitializeComponent();
            base.Text = syntaxHighlight ? "UltiMUD Lua Script Studio" : "Text Editor";
            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.InitIdeComponents();
            this.SyntaxHighlight = syntaxHighlight;
            this.richTextBox1.TextChanged += new EventHandler(this.richTextBox1_TextChanged);
            this.UpdateStatusBar();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ApplyIdeLayoutProportions();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.ApplyIdeLayoutProportions();
        }

        public void ApplyIdeLayoutProportions()
        {
            if (!this.syntaxHighlight_ || this.mainSplit_ == null)
                return;

            try
            {
                this.PerformLayout();
                this.mainSplit_.PerformLayout();

                int totalWidth = this.mainSplit_.Width > 650 ? this.mainSplit_.Width : this.ClientSize.Width;
                if (totalWidth > 650)
                {
                    this.mainSplit_.Panel1MinSize = 380;
                    this.mainSplit_.Panel2MinSize = 420;
                    int targetRightWidth = System.Math.Max(480, (int)(totalWidth * 0.43));
                    int splitterDist = System.Math.Max(this.mainSplit_.Panel1MinSize, totalWidth - targetRightWidth);
                    if (splitterDist < totalWidth - this.mainSplit_.Panel2MinSize)
                        this.mainSplit_.SplitterDistance = splitterDist;
                }

                this.mainSplit_.Panel2.PerformLayout();
                if (this.rightSplit_ != null && this.rightSplit_.Orientation == Orientation.Horizontal)
                {
                    int rightHeight = this.rightSplit_.Height > 260 ? this.rightSplit_.Height : (this.ClientSize.Height - 105);
                    if (rightHeight > 260)
                    {
                        this.rightSplit_.Panel1MinSize = 130;
                        this.rightSplit_.Panel2MinSize = 180;
                        this.rightSplit_.SplitterDistance = System.Math.Max(160, (int)(rightHeight * 0.42));
                    }
                }
            }
            catch
            {
            }
        }

        private void InitIdeComponents()
        {
            // Rearrange richTextBox1 inside a SplitContainer so the right side hosts the Scripting Guide & API Explorer
            this.Controls.Remove(this.richTextBox1);

            this.mainSplit_ = new SplitContainer();
            this.mainSplit_.Dock = DockStyle.Fill;
            this.mainSplit_.Orientation = Orientation.Vertical;
            this.mainSplit_.SplitterWidth = 6;
            this.mainSplit_.BackColor = SystemColors.ControlLight;
            this.mainSplit_.Panel1.BackColor = SystemColors.Window;
            this.mainSplit_.Panel2.BackColor = SystemColors.Control;

            this.editorHostPanel_ = new Panel();
            this.editorHostPanel_.Dock = DockStyle.Fill;

            this.signatureBanner_ = new Label();
            this.signatureBanner_.Dock = DockStyle.Bottom;
            this.signatureBanner_.Height = 26;
            this.signatureBanner_.UseMnemonic = false;
            this.signatureBanner_.AutoEllipsis = true;
            this.signatureBanner_.BackColor = Color.FromArgb(30, 30, 30);
            this.signatureBanner_.ForeColor = Color.FromArgb(220, 220, 170);
            this.signatureBanner_.Font = new Font("Consolas", 9F, FontStyle.Regular);
            this.signatureBanner_.TextAlign = ContentAlignment.MiddleLeft;
            this.signatureBanner_.Padding = new Padding(8, 0, 8, 0);
            this.signatureBanner_.Text = "[Tip] Type 'user:', 'char:', 'Command.', or '{color:' for auto-complete, or press Ctrl+Space.";

            this.intelliSenseList_ = new ListBox();
            this.intelliSenseList_.Visible = false;
            this.intelliSenseList_.Font = new Font("Consolas", 9.5F, FontStyle.Regular);
            this.intelliSenseList_.BackColor = Color.FromArgb(37, 37, 38);
            this.intelliSenseList_.ForeColor = Color.FromArgb(241, 241, 241);
            this.intelliSenseList_.BorderStyle = BorderStyle.FixedSingle;
            this.intelliSenseList_.IntegralHeight = false;
            this.intelliSenseList_.Size = new Size(440, 170);
            this.intelliSenseList_.SelectedIndexChanged += new EventHandler(this.intelliSenseList_SelectedIndexChanged);
            this.intelliSenseList_.DoubleClick += new EventHandler(this.intelliSenseList_DoubleClick);

            this.richTextBox1.Dock = DockStyle.Fill;
            this.richTextBox1.Font = new Font("Consolas", 10F, FontStyle.Regular);
            this.richTextBox1.KeyDown -= this.richTextBox1_KeyUp;
            this.richTextBox1.KeyDown += new KeyEventHandler(this.richTextBox1_IdeKeyDown);

            // In WinForms, DockStyle.Fill must be at index 0 (BringToFront) so it docks inside remaining space after DockStyle.Bottom
            this.editorHostPanel_.Controls.Add(this.richTextBox1);
            this.editorHostPanel_.Controls.Add(this.signatureBanner_);
            this.editorHostPanel_.Controls.Add(this.intelliSenseList_);
            this.richTextBox1.BringToFront();
            this.intelliSenseList_.BringToFront();

            this.mainSplit_.Panel1.Controls.Add(this.editorHostPanel_);
            this.BuildGuidePanel(this.mainSplit_.Panel2, true);

            this.Controls.Add(this.mainSplit_);
            this.mainSplit_.BringToFront();

            // Upgrade Toolbar with Snippets, Colors, and Guide Toggle
            this.sepIde_ = new ToolStripSeparator();

            this.btnSnippets_ = new ToolStripDropDownButton("+ &Insert Snippet");
            this.btnSnippets_.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnSnippets_.Font = new Font(this.toolStrip1.Font, FontStyle.Bold);
            this.btnSnippets_.ForeColor = Color.FromArgb(25, 85, 45);
            this.PopulateSnippetMenu(this.btnSnippets_);

            this.btnColors_ = new ToolStripDropDownButton("&Color Tags");
            this.btnColors_.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnColors_.ForeColor = Color.FromArgb(130, 50, 120);
            this.PopulateColorMenu(this.btnColors_);

            this.btnToggleGuide_ = new ToolStripButton("&Scripting Guide / API (F1)");
            this.btnToggleGuide_.CheckOnClick = true;
            this.btnToggleGuide_.Checked = true;
            this.btnToggleGuide_.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.btnToggleGuide_.Font = new Font(this.toolStrip1.Font, FontStyle.Bold);
            this.btnToggleGuide_.ForeColor = Color.FromArgb(20, 70, 140);
            this.btnToggleGuide_.Click += delegate(object sender, EventArgs e)
            {
                this.mainSplit_.Panel2Collapsed = !this.btnToggleGuide_.Checked;
                if (!this.mainSplit_.Panel2Collapsed)
                    this.ApplyIdeLayoutProportions();
            };

            this.toolStrip1.Items.AddRange(new ToolStripItem[]
            {
                this.sepIde_,
                this.btnSnippets_,
                this.btnColors_,
                new ToolStripSeparator(),
                this.btnToggleGuide_
            });

            this.statusSignatureLabel_ = new ToolStripStatusLabel();
            this.statusSignatureLabel_.Spring = true;
            this.statusSignatureLabel_.TextAlign = ContentAlignment.MiddleLeft;
            this.statusSignatureLabel_.ForeColor = Color.DarkSlateBlue;
            this.statusStrip1.Items.Add(this.statusSignatureLabel_);
        }

        private void BuildGuidePanel(Control parent, bool allowInsertIntoEditor)
        {
            Panel topSearchPanel = new Panel();
            topSearchPanel.Dock = DockStyle.Top;
            topSearchPanel.Height = 56;
            topSearchPanel.Padding = new Padding(8, 6, 8, 6);
            topSearchPanel.BackColor = Color.FromArgb(235, 241, 248);

            Label lblTitle = new Label();
            lblTitle.Text = "Lua Scripting Guide & API Reference (Filter / Search):";
            lblTitle.UseMnemonic = false;
            lblTitle.AutoEllipsis = true;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 20;
            lblTitle.ForeColor = Color.FromArgb(20, 50, 95);
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.apiSearchBox_ = new TextBox();
            this.apiSearchBox_.Dock = DockStyle.Bottom;
            this.apiSearchBox_.Font = new Font("Segoe UI", 9.5F);
            this.apiSearchBox_.TextChanged += delegate(object s, EventArgs e)
            {
                this.PopulateApiTree(this.apiSearchBox_.Text);
            };

            topSearchPanel.Controls.Add(this.apiSearchBox_);
            topSearchPanel.Controls.Add(lblTitle);

            SplitContainer rightSplit = new SplitContainer();
            this.rightSplit_ = rightSplit;
            rightSplit.Dock = DockStyle.Fill;
            rightSplit.SplitterWidth = 6;
            rightSplit.BackColor = SystemColors.ControlLight;
            rightSplit.Orientation = allowInsertIntoEditor ? Orientation.Horizontal : Orientation.Vertical;

            this.apiTree_ = new TreeView();
            this.apiTree_.Dock = DockStyle.Fill;
            this.apiTree_.Font = new Font("Segoe UI", 9F);
            this.apiTree_.ItemHeight = 20;
            this.apiTree_.HideSelection = false;
            this.apiTree_.ShowNodeToolTips = true;
            this.apiTree_.AfterSelect += new TreeViewEventHandler(this.apiTree_AfterSelect);
            this.apiTree_.NodeMouseDoubleClick += delegate(object s, TreeNodeMouseClickEventArgs e)
            {
                if (allowInsertIntoEditor && e.Node != null && e.Node.Tag is ApiEntry)
                {
                    ApiEntry entry = (ApiEntry)e.Node.Tag;
                    if (!string.IsNullOrEmpty(entry.InsertText))
                        this.InsertAtCaret(entry.InsertText);
                }
            };
            rightSplit.Panel1.Controls.Add(this.apiTree_);

            // Header bar at top of documentation viewer showing the selected function/topic signature
            Panel docHeaderPanel = new Panel();
            docHeaderPanel.Dock = DockStyle.Top;
            docHeaderPanel.Height = 30;
            docHeaderPanel.Padding = new Padding(8, 4, 8, 4);
            docHeaderPanel.BackColor = Color.FromArgb(226, 234, 246);

            this.docSignatureHeader_ = new Label();
            this.docSignatureHeader_.Dock = DockStyle.Fill;
            this.docSignatureHeader_.UseMnemonic = false;
            this.docSignatureHeader_.AutoEllipsis = true;
            this.docSignatureHeader_.TextAlign = ContentAlignment.MiddleLeft;
            this.docSignatureHeader_.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
            this.docSignatureHeader_.ForeColor = Color.FromArgb(15, 45, 95);
            docHeaderPanel.Controls.Add(this.docSignatureHeader_);

            // Bottom action bar using a TableLayoutPanel so buttons scale evenly and never overlap
            Panel docBottomPanel = new Panel();
            docBottomPanel.Dock = DockStyle.Bottom;
            docBottomPanel.Height = 38;
            docBottomPanel.Padding = new Padding(6, 4, 6, 4);
            docBottomPanel.BackColor = Color.FromArgb(235, 241, 248);

            TableLayoutPanel btnGrid = new TableLayoutPanel();
            btnGrid.Dock = DockStyle.Fill;
            btnGrid.RowCount = 1;
            btnGrid.ColumnCount = allowInsertIntoEditor ? 2 : 1;
            btnGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            if (allowInsertIntoEditor)
            {
                btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
                btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            }
            else
            {
                btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            }

            this.btnInsertApi_ = new Button();
            this.btnInsertApi_.Text = "<< Insert at Cursor";
            this.btnInsertApi_.Dock = DockStyle.Fill;
            this.btnInsertApi_.Margin = new Padding(0, 0, 4, 0);
            this.btnInsertApi_.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.btnInsertApi_.BackColor = Color.FromArgb(225, 242, 230);
            this.btnInsertApi_.UseVisualStyleBackColor = false;
            this.btnInsertApi_.Visible = allowInsertIntoEditor;
            this.btnInsertApi_.Click += delegate(object s, EventArgs e)
            {
                if (this.apiTree_.SelectedNode != null && this.apiTree_.SelectedNode.Tag is ApiEntry)
                {
                    ApiEntry entry = (ApiEntry)this.apiTree_.SelectedNode.Tag;
                    string toInsert = !string.IsNullOrEmpty(entry.InsertText) ? entry.InsertText : entry.Example;
                    if (!string.IsNullOrEmpty(toInsert))
                        this.InsertAtCaret(toInsert);
                }
            };

            this.btnCopyExample_ = new Button();
            this.btnCopyExample_.Text = "Copy Example Code";
            this.btnCopyExample_.Dock = DockStyle.Fill;
            this.btnCopyExample_.Margin = new Padding(allowInsertIntoEditor ? 4 : 0, 0, 0, 0);
            this.btnCopyExample_.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this.btnCopyExample_.BackColor = Color.FromArgb(245, 247, 250);
            this.btnCopyExample_.UseVisualStyleBackColor = false;
            this.btnCopyExample_.Click += delegate(object s, EventArgs e)
            {
                if (this.apiTree_.SelectedNode != null && this.apiTree_.SelectedNode.Tag is ApiEntry)
                {
                    ApiEntry entry = (ApiEntry)this.apiTree_.SelectedNode.Tag;
                    string toCopy = !string.IsNullOrEmpty(entry.Example) ? entry.Example : entry.InsertText;
                    if (!string.IsNullOrEmpty(toCopy))
                        Clipboard.SetText(toCopy);
                }
            };

            if (allowInsertIntoEditor)
            {
                btnGrid.Controls.Add(this.btnInsertApi_, 0, 0);
                btnGrid.Controls.Add(this.btnCopyExample_, 1, 0);
            }
            else
            {
                btnGrid.Controls.Add(this.btnCopyExample_, 0, 0);
            }
            docBottomPanel.Controls.Add(btnGrid);

            Panel docBodyHost = new Panel();
            docBodyHost.Dock = DockStyle.Fill;
            docBodyHost.Padding = new Padding(8, 6, 4, 6);
            docBodyHost.BackColor = Color.FromArgb(252, 252, 255);

            this.apiDocBox_ = new RichTextBox();
            this.apiDocBox_.Dock = DockStyle.Fill;
            this.apiDocBox_.ReadOnly = true;
            this.apiDocBox_.WordWrap = true;
            this.apiDocBox_.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.apiDocBox_.BackColor = Color.FromArgb(252, 252, 255);
            this.apiDocBox_.Font = new Font("Consolas", 9.5F);
            this.apiDocBox_.BorderStyle = BorderStyle.None;
            docBodyHost.Controls.Add(this.apiDocBox_);

            // Dock order in Panel2: add Fill first, then Top/Bottom, then BringToFront on Fill
            rightSplit.Panel2.Controls.Add(docBodyHost);
            rightSplit.Panel2.Controls.Add(docHeaderPanel);
            rightSplit.Panel2.Controls.Add(docBottomPanel);
            docBodyHost.BringToFront();

            // Dock order in parent: add Fill first, then Top, then BringToFront on Fill
            parent.Controls.Add(rightSplit);
            parent.Controls.Add(topSearchPanel);
            rightSplit.BringToFront();

            this.PopulateApiTree(string.Empty);
        }

        private void PopulateApiTree(string filter)
        {
            if (this.apiTree_ == null)
                return;

            this.apiTree_.BeginUpdate();
            this.apiTree_.Nodes.Clear();

            Dictionary<string, TreeNode> categoryNodes = new Dictionary<string, TreeNode>();
            string q = (filter ?? string.Empty).Trim().ToLowerInvariant();

            foreach (ApiEntry entry in Catalog)
            {
                if (!string.IsNullOrEmpty(q))
                {
                    bool match =
                        (entry.Label != null && entry.Label.ToLowerInvariant().Contains(q)) ||
                        (entry.Signature != null && entry.Signature.ToLowerInvariant().Contains(q)) ||
                        (entry.Summary != null && entry.Summary.ToLowerInvariant().Contains(q)) ||
                        (entry.Category != null && entry.Category.ToLowerInvariant().Contains(q));
                    if (!match)
                        continue;
                }

                TreeNode catNode;
                if (!categoryNodes.TryGetValue(entry.Category, out catNode))
                {
                    catNode = new TreeNode(entry.Category);
                    categoryNodes[entry.Category] = catNode;
                    this.apiTree_.Nodes.Add(catNode);
                }

                TreeNode itemNode = new TreeNode(entry.Label);
                itemNode.Tag = entry;
                itemNode.ToolTipText = entry.Signature + "\r\n" + entry.Summary;
                catNode.Nodes.Add(itemNode);
            }

            if (!string.IsNullOrEmpty(q))
                this.apiTree_.ExpandAll();
            else if (this.apiTree_.Nodes.Count > 0)
            {
                this.apiTree_.Nodes[0].Expand();
                if (this.apiTree_.Nodes.Count > 1)
                    this.apiTree_.Nodes[1].Expand();
                if (this.apiTree_.Nodes.Count > 3)
                    this.apiTree_.Nodes[3].Expand();
            }

            this.apiTree_.EndUpdate();

            if (this.apiTree_.Nodes.Count > 0 && this.apiTree_.Nodes[0].Nodes.Count > 0)
                this.apiTree_.SelectedNode = this.apiTree_.Nodes[0].Nodes[0];
        }

        private void apiTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;

            if (!(e.Node.Tag is ApiEntry))
            {
                if (this.docSignatureHeader_ != null)
                    this.docSignatureHeader_.Text = e.Node.Text;
                StringBuilder catSb = new StringBuilder();
                catSb.AppendLine("Category: " + e.Node.Text);
                catSb.AppendLine();
                catSb.AppendLine("Select an item under this category in the tree above to view its full documentation, parameters, and Lua example code:");
                catSb.AppendLine();
                foreach (TreeNode child in e.Node.Nodes)
                {
                    if (child.Tag is ApiEntry)
                    {
                        ApiEntry ce = (ApiEntry)child.Tag;
                        catSb.AppendLine("  * " + ce.Signature);
                    }
                }
                this.apiDocBox_.Text = catSb.ToString();
                return;
            }

            ApiEntry entry = (ApiEntry)e.Node.Tag;
            if (this.docSignatureHeader_ != null)
                this.docSignatureHeader_.Text = entry.Signature;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(entry.Summary);
            if (!string.IsNullOrEmpty(entry.Example))
            {
                sb.AppendLine();
                sb.AppendLine("--- Lua Example / Template ---");
                sb.AppendLine(entry.Example);
            }
            this.apiDocBox_.Text = sb.ToString();
        }

        private void PopulateSnippetMenu(ToolStripDropDownButton btn)
        {
            foreach (ApiEntry entry in Catalog)
            {
                if (entry.Category.StartsWith("6.") && !string.IsNullOrEmpty(entry.Example))
                {
                    ApiEntry captured = entry;
                    ToolStripMenuItem item = new ToolStripMenuItem(captured.Label);
                    item.ToolTipText = captured.Summary;
                    item.Click += delegate(object s, EventArgs e)
                    {
                        this.InsertAtCaret(captured.Example);
                    };
                    btn.DropDownItems.Add(item);
                }
            }
        }

        private void PopulateColorMenu(ToolStripDropDownButton btn)
        {
            foreach (ApiEntry entry in Catalog)
            {
                if (entry.OwnerPrefix == "color")
                {
                    ApiEntry captured = entry;
                    ToolStripMenuItem item = new ToolStripMenuItem(captured.Label + "  (" + captured.Summary + ")");
                    item.Click += delegate(object s, EventArgs e)
                    {
                        this.InsertAtCaret(captured.InsertText);
                    };
                    btn.DropDownItems.Add(item);
                }
            }
        }

        public void InsertAtCaret(string text)
        {
            if (string.IsNullOrEmpty(text) || this.richTextBox1 == null)
                return;

            int pos = this.richTextBox1.SelectionStart;
            int len = this.richTextBox1.SelectionLength;
            string current = this.richTextBox1.Text;
            this.richTextBox1.Text = current.Substring(0, pos) + text + current.Substring(pos + len);
            this.richTextBox1.SelectionStart = pos + text.Length;
            this.richTextBox1.SelectionLength = 0;
            this.richTextBox1.Focus();
        }

        public static void ShowScriptingGuide(IWin32Window owner)
        {
            using (Form guideForm = new Form())
            {
                guideForm.Text = "UltiMUD Lua Scripting Guide & API Reference";
                guideForm.MinimumSize = new Size(860, 560);
                guideForm.Size = new Size(1060, 700);
                guideForm.StartPosition = FormStartPosition.CenterParent;
                TextEditorWindow helper = new TextEditorWindow(false);
                helper.BuildGuidePanel(guideForm, false);
                guideForm.Shown += delegate(object s, EventArgs e)
                {
                    if (helper.rightSplit_ != null && helper.rightSplit_.Width > 500)
                    {
                        helper.rightSplit_.Panel1MinSize = 260;
                        helper.rightSplit_.Panel2MinSize = 360;
                        helper.rightSplit_.SplitterDistance = 370;
                    }
                };
                if (owner != null)
                    guideForm.ShowDialog(owner);
                else
                    guideForm.ShowDialog();
            }
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
            if (!this.syntaxHighlight_ || this.highlighter_ == null || this.updating_ || this.richTextBox1 == null || this.richTextBox1.IsDisposed)
                return;

            try
            {
                this.updating_ = true;
                int selectionStart = this.richTextBox1.SelectionStart;
                int selectionLength = this.richTextBox1.SelectionLength;
                this.richTextBox1.Rtf = this.highlighter_.Parse(this.richTextBox1.Text);
                this.richTextBox1.SelectionStart = System.Math.Min(selectionStart, this.richTextBox1.TextLength);
                this.richTextBox1.SelectionLength = selectionLength;
            }
            catch
            {
            }
            finally
            {
                this.updating_ = false;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (this.updating_)
                return;
            if (this.syntaxHighlight_)
            {
                this.HighlightText();
                this.TriggerIntelliSense(false);
            }
        }

        private void richTextBox1_IdeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            if (e.KeyCode == Keys.F1)
            {
                e.SuppressKeyPress = true;
                if (this.btnToggleGuide_ != null)
                {
                    this.btnToggleGuide_.Checked = !this.btnToggleGuide_.Checked;
                    this.mainSplit_.Panel2Collapsed = !this.btnToggleGuide_.Checked;
                }
                return;
            }
            if (e.Control && e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
                this.TriggerIntelliSense(true);
                return;
            }

            if (this.intelliSenseList_ != null && this.intelliSenseList_.Visible)
            {
                if (e.KeyCode == Keys.Down)
                {
                    e.SuppressKeyPress = true;
                    if (this.intelliSenseList_.SelectedIndex < this.intelliSenseList_.Items.Count - 1)
                        this.intelliSenseList_.SelectedIndex++;
                    return;
                }
                if (e.KeyCode == Keys.Up)
                {
                    e.SuppressKeyPress = true;
                    if (this.intelliSenseList_.SelectedIndex > 0)
                        this.intelliSenseList_.SelectedIndex--;
                    return;
                }
                if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    this.AcceptIntelliSenseSelection();
                    return;
                }
                if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    this.intelliSenseList_.Visible = false;
                    return;
                }
            }
        }

        public void TriggerIntelliSense(bool forceShowAll)
        {
            if (this.richTextBox1 == null || this.intelliSenseList_ == null)
                return;

            int caret = this.richTextBox1.SelectionStart;
            string text = this.richTextBox1.Text;
            if (caret < 0 || caret > text.Length)
            {
                this.intelliSenseList_.Visible = false;
                return;
            }

            // Inspect text right before caret on the current line
            int lineStart = text.LastIndexOf('\n', System.Math.Max(0, caret - 1)) + 1;
            string lineBeforeCaret = text.Substring(lineStart, caret - lineStart);

            // Check for {color: prefix
            int colorIdx = lineBeforeCaret.LastIndexOf("{color:", StringComparison.OrdinalIgnoreCase);
            if (colorIdx >= 0 && !lineBeforeCaret.Substring(colorIdx).Contains("}"))
            {
                string partialColor = lineBeforeCaret.Substring(colorIdx);
                this.completionReplaceStart_ = lineStart + colorIdx;
                this.completionReplaceLength_ = partialColor.Length;
                this.ShowFilteredCompletions("color", partialColor);
                return;
            }

            // Extract owner.member or owner:member or standalone identifier
            int i = lineBeforeCaret.Length - 1;
            while (i >= 0 && (char.IsLetterOrDigit(lineBeforeCaret[i]) || lineBeforeCaret[i] == '_'))
                i--;

            string memberPrefix = lineBeforeCaret.Substring(i + 1);
            this.completionReplaceStart_ = lineStart + i + 1;
            this.completionReplaceLength_ = memberPrefix.Length;

            string owner = string.Empty;
            if (i >= 0 && (lineBeforeCaret[i] == '.' || lineBeforeCaret[i] == ':'))
            {
                int sepIdx = i;
                int j = sepIdx - 1;
                while (j >= 0 && (char.IsLetterOrDigit(lineBeforeCaret[j]) || lineBeforeCaret[j] == '_'))
                    j--;
                owner = lineBeforeCaret.Substring(j + 1, sepIdx - (j + 1));
            }

            if (!string.IsNullOrEmpty(owner))
            {
                string normalizedOwner = NormalizeOwner(owner);
                if (!string.IsNullOrEmpty(normalizedOwner))
                {
                    this.ShowFilteredCompletions(normalizedOwner, memberPrefix);
                    return;
                }
            }

            if (forceShowAll || memberPrefix.Length >= 2)
            {
                this.ShowFilteredCompletions(string.Empty, memberPrefix);
            }
            else
            {
                this.intelliSenseList_.Visible = false;
            }
        }

        private static string NormalizeOwner(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return string.Empty;
            string low = owner.ToLowerInvariant();
            if (low == "user" || low == "targetuser")
                return "user";
            if (low == "char" || low == "ch" || low == "actor" || low == "target" || low == "npc")
                return "char";
            if (low == "command")
                return "Command";
            if (low == "server")
                return "server";
            if (low == "area" || low == "destarea")
                return "area";
            return string.Empty;
        }

        private void ShowFilteredCompletions(string ownerFilter, string prefix)
        {
            this.currentCompletions_.Clear();
            string lowPrefix = (prefix ?? string.Empty).ToLowerInvariant();

            foreach (ApiEntry entry in Catalog)
            {
                if (string.IsNullOrEmpty(entry.InsertText))
                    continue; // Skip pure tutorial guide articles in popup

                if (!string.IsNullOrEmpty(ownerFilter))
                {
                    if (!string.Equals(entry.OwnerPrefix, ownerFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (string.IsNullOrEmpty(lowPrefix) ||
                        entry.InsertText.ToLowerInvariant().StartsWith(lowPrefix) ||
                        entry.Label.ToLowerInvariant().Contains(lowPrefix))
                    {
                        this.currentCompletions_.Add(entry);
                    }
                }
                else
                {
                    // Global completion
                    if (entry.OwnerPrefix == "color")
                        continue;
                    if (string.IsNullOrEmpty(lowPrefix) ||
                        entry.Label.ToLowerInvariant().StartsWith(lowPrefix) ||
                        entry.InsertText.ToLowerInvariant().StartsWith(lowPrefix))
                    {
                        this.currentCompletions_.Add(entry);
                    }
                }
            }

            if (this.currentCompletions_.Count == 0)
            {
                this.intelliSenseList_.Visible = false;
                return;
            }

            this.intelliSenseList_.BeginUpdate();
            this.intelliSenseList_.Items.Clear();
            foreach (ApiEntry item in this.currentCompletions_)
                this.intelliSenseList_.Items.Add(item);
            this.intelliSenseList_.SelectedIndex = 0;
            this.intelliSenseList_.EndUpdate();

            Point pt = this.richTextBox1.GetPositionFromCharIndex(this.richTextBox1.SelectionStart);
            int popupX = System.Math.Max(8, System.Math.Min(pt.X + 8, this.editorHostPanel_.Width - this.intelliSenseList_.Width - 8));
            int popupY = pt.Y + 20;
            if (popupY + this.intelliSenseList_.Height > this.editorHostPanel_.Height - 28)
                popupY = System.Math.Max(4, pt.Y - this.intelliSenseList_.Height - 4);

            this.intelliSenseList_.Location = new Point(popupX, popupY);
            this.intelliSenseList_.Visible = true;
            this.intelliSenseList_.BringToFront();
        }

        private void AcceptIntelliSenseSelection()
        {
            if (this.intelliSenseList_ == null || !this.intelliSenseList_.Visible || !(this.intelliSenseList_.SelectedItem is ApiEntry))
                return;

            ApiEntry selected = (ApiEntry)this.intelliSenseList_.SelectedItem;
            this.intelliSenseList_.Visible = false;

            string insert = selected.InsertText;
            string current = this.richTextBox1.Text;
            int start = System.Math.Max(0, System.Math.Min(this.completionReplaceStart_, current.Length));
            int len = System.Math.Max(0, System.Math.Min(this.completionReplaceLength_, current.Length - start));

            this.richTextBox1.Text = current.Substring(0, start) + insert + current.Substring(start + len);
            int nextCursor = start + insert.Length;
            if (insert.EndsWith("()"))
                nextCursor = start + insert.Length;
            else if (insert.EndsWith(")"))
            {
                int openParen = insert.IndexOf('(');
                if (openParen >= 0)
                    nextCursor = start + openParen + 1;
            }
            this.richTextBox1.SelectionStart = System.Math.Min(nextCursor, this.richTextBox1.TextLength);
            this.richTextBox1.SelectionLength = 0;
            this.richTextBox1.Focus();
        }

        private void intelliSenseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.intelliSenseList_.SelectedItem is ApiEntry)
            {
                ApiEntry entry = (ApiEntry)this.intelliSenseList_.SelectedItem;
                this.signatureBanner_.Text = "[API] " + entry.Signature + "  —  " + entry.Summary;
                if (this.statusSignatureLabel_ != null)
                    this.statusSignatureLabel_.Text = entry.Signature;
            }
        }

        private void intelliSenseList_DoubleClick(object sender, EventArgs e)
        {
            this.AcceptIntelliSenseSelection();
        }

        private void UpdateStatusBar()
        {
            if (this.richTextBox1 == null)
                return;

            int sel = this.richTextBox1.SelectionStart;
            int line = this.richTextBox1.GetLineFromCharIndex(sel);
            int firstCharOfLine = this.richTextBox1.GetFirstCharIndexFromLine(line);
            int col = sel - firstCharOfLine + 1;
            this.toolStripStatusLabel1.Text = "Ln " + (line + 1) + ", Col " + col;

            // Update live function hint on current line
            string text = this.richTextBox1.Text;
            if (sel >= 0 && sel <= text.Length)
            {
                int lineStart = text.LastIndexOf('\n', System.Math.Max(0, sel - 1)) + 1;
                int lineEnd = text.IndexOf('\n', sel);
                if (lineEnd < 0) lineEnd = text.Length;
                string currentLine = text.Substring(lineStart, lineEnd - lineStart);

                foreach (ApiEntry entry in Catalog)
                {
                    if (!string.IsNullOrEmpty(entry.InsertText) && entry.OwnerPrefix != "color")
                    {
                        string token = entry.Label.Split('(')[0].Trim();
                        if (token.Length > 2 && currentLine.Contains(token))
                        {
                            this.signatureBanner_.Text = "[API] " + entry.Signature + "  —  " + entry.Summary;
                            if (this.statusSignatureLabel_ != null)
                                this.statusSignatureLabel_.Text = entry.Signature;
                            return;
                        }
                    }
                }
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
            if (this.intelliSenseList_ != null)
                this.intelliSenseList_.Visible = false;
            this.UpdateStatusBar();
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            this.UpdateStatusBar();
        }

        private static List<ApiEntry> BuildCatalog()
        {
            List<ApiEntry> list = new List<ApiEntry>();

            // 1. Guides & Tutorials
            list.Add(new ApiEntry(
                "1. Scripting Guides & Tutorials", "",
                "Quick Start: How UltiMUD Lua Scripting Works", "",
                "UltiMUD Lua 5.1 Scripting Overview",
                "UltiMUD embeds Lua 5.1 (via LuaInterface) across Commands, Items, Abilities, Area Regions, and NPC Triggers.\r\n\r\n" +
                "WHERE SCRIPTS RUN & PRE-BOUND GLOBALS:\r\n" +
                "1. Commands (Commands/*.cmd):\r\n" +
                "   - user     : Ultimud.User (the player who typed the command)\r\n" +
                "   - char     : Ultimud.Character (user.Character)\r\n" +
                "   - area     : Ultimud.Area (char.Area)\r\n" +
                "   - server   : Ultimud.Server (Server.Instance)\r\n" +
                "   - settings : Ultimud.Settings (server.Settings)\r\n" +
                "   - Command  : Ultimud.Command (built-in helper library)\r\n\r\n" +
                "2. Items (Item.OnUse):\r\n" +
                "   - actor / user : The Character using or consuming the item\r\n" +
                "   - item         : The Ultimud.Item being used\r\n\r\n" +
                "3. Abilities / Spells (Ability.OnUse):\r\n" +
                "   - actor   : The Character casting/using the ability\r\n" +
                "   - target  : The target Object/Character\r\n" +
                "   - ability : The Ultimud.Ability being used\r\n\r\n" +
                "4. Area Regions (Region.OnEnter / Region.OnLeave):\r\n" +
                "   - actor  : The Character entering or leaving the region\r\n" +
                "   - region : The Area.Region polygon\r\n" +
                "   - area   : The Area containing the region\r\n\r\n" +
                "EDITOR SHORTCUTS:\r\n" +
                "- Type 'user:', 'char:', 'Command.', or '{color:' to pop up VS Code-style Auto-Complete!\r\n" +
                "- Press Ctrl+Space anytime to force Auto-Complete.\r\n" +
                "- Press Up/Down + Tab/Enter to accept a completion.\r\n" +
                "- Press Ctrl+S to Save and apply your script immediately.",
                "-- Example: Customizing a command\r\nlocal arg = user:GetArgument()\r\nuser:Send('{color:brightcyan}Hello, ' .. char.Name .. '!{color:reset}')\r\nreturn true"));

            list.Add(new ApiEntry(
                "1. Scripting Guides & Tutorials", "",
                "Writing & Customizing Commands", "",
                "Step-by-Step Command Scripting Guide",
                "Every command in UltiMUD is stored as an XML file in data\\muds\\<MudName>\\Commands\\<name>.cmd.\r\n\r\n" +
                "READING PLAYER INPUT:\r\n" +
                "- user:GetArgument() pops the next single word from the command line.\r\n" +
                "- user.Command contains the rest of the line (useful for chat/say/title messages). Set user.Command = nil after reading it.\r\n\r\n" +
                "SENDING MESSAGES:\r\n" +
                "- user:Send('text') sends a message only to the player.\r\n" +
                "- Command.BroadcastRoom(user, selfMsg, roomMsg, mustHear) sends a message to the player AND everyone at their location.\r\n" +
                "- Command.BroadcastTargetRoom(user, target, selfMsg, targetMsg, roomMsg, mustHear) sends tailored messages to actor, target, and bystanders.\r\n\r\n" +
                "RETURN VALUE:\r\n" +
                "- Return true when the command is finished executing.",
                "-- Example: 'Roll Dice' Command\r\nlocal sides = Command.ToInt(user:GetArgument(), 6)\r\nif sides < 2 or sides > 100 then sides = 6 end\r\nlocal roll = math.random(1, sides)\r\nCommand.BroadcastRoom(user,\r\n    '{color:brightyellow}You roll a ' .. sides .. '-sided die and get ' .. roll .. '!{color:reset}',\r\n    '{color:yellow}{1} rolls a ' .. sides .. '-sided die and gets ' .. roll .. '!{color:reset}',\r\n    false)\r\nreturn true"));

            list.Add(new ApiEntry(
                "1. Scripting Guides & Tutorials", "",
                "Color Codes & String Placeholders", "",
                "{color:<name>} and {1}, {2}, {dir} Formatting Tokens",
                "UltiMUD supports inline color tags and dynamic actor/target placeholders in all messages:\r\n\r\n" +
                "BRIGHT COLORS:\r\n" +
                "  {color:brightred}     {color:brightgreen}   {color:brightyellow}\r\n" +
                "  {color:brightblue}    {color:brightmagenta} {color:brightcyan}\r\n" +
                "  {color:brightwhite}\r\n\r\n" +
                "STANDARD COLORS:\r\n" +
                "  {color:red}   {color:green}   {color:yellow}  {color:blue}\r\n" +
                "  {color:magenta} {color:cyan}  {color:white}   {color:gray}\r\n" +
                "  {color:darkgray} {color:reset}\r\n\r\n" +
                "MESSAGE PLACEHOLDERS (in BroadcastRoom / BroadcastTargetRoom / MoveDir):\r\n" +
                "  {1}   -> Replaced with Actor's Name (char.Name)\r\n" +
                "  {2}   -> Replaced with Target's Name (target.Name)\r\n" +
                "  {dir} -> Replaced with movement direction (north, south, etc.)",
                "user:Send('{color:brightcyan}Welcome to {color:brightyellow}' .. server.Settings.Name .. '{color:reset}!')"));

            // 2. user (Ultimud.User)
            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user:GetArgument()", "GetArgument()",
                "string user:GetArgument()",
                "Pops and returns the next space-delimited argument from the user's command input, or nil/empty string if none.",
                "local arg = user:GetArgument()\r\nif arg == nil or arg == '' then\r\n    user:Send('Syntax: command <arg>')\r\n    return true\r\nend"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user.Command", "Command",
                "string user.Command",
                "Gets or sets the remaining unparsed text of the user's command line. Always set user.Command = nil after consuming the full line.",
                "local msg = user.Command\r\nuser.Command = nil"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user:Send(text)", "Send('')",
                "void user:Send(string text)",
                "Sends a formatted message (supporting {color:...} tags) to the player's Telnet client.",
                "user:Send('{color:brightgreen}Quest completed!{color:reset}')"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user:SendError(text)", "SendError('')",
                "void user:SendError(string text)",
                "Sends a red 'ERROR: <text>' message to the player.",
                "user:SendError('You cannot do that right now.')"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user:Save()", "Save()",
                "void user:Save()",
                "Persists the player's character file to disk.",
                "user:Save()\r\nuser:Send('Character saved.')"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user.BankBalance", "BankBalance",
                "int user.BankBalance",
                "Gets or sets the amount of gold stored safely in the player's bank account.",
                "user.BankBalance = user.BankBalance + 100"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user.Title", "Title",
                "string user.Title",
                "Gets or sets the player's custom title displayed in the 'who' list.",
                "user.Title = 'the Dragon Slayer'"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user.IsAfk / user.AfkMessage", "IsAfk",
                "bool user.IsAfk | string user.AfkMessage",
                "Gets or sets the player's Away-From-Keyboard status and auto-reply reason.",
                "user.IsAfk = true\r\nuser.AfkMessage = 'Grabbing coffee'"));

            list.Add(new ApiEntry(
                "2. user (Ultimud.User)", "user",
                "user:IsIgnoring(playerName)", "IsIgnoring('')",
                "bool user:IsIgnoring(string playerName)",
                "Returns true if this user has added playerName to their ignore list.",
                "if targetUser:IsIgnoring(char.Name) then return true end"));

            // 3. char / actor / target (Ultimud.Character)
            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char.Name", "Name",
                "string char.Name",
                "Gets or sets the character's name.",
                "user:Send('Your name is ' .. char.Name)"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char.Value", "Value",
                "int char.Value",
                "Gets or sets the character's gold/money currently carried on hand.",
                "char.Value = char.Value + 250"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char.Description", "Description",
                "string char.Description",
                "Gets or sets the character's multi-line appearance description.",
                "char.Description = 'A battle-hardened adventurer.'"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char.Position", "Position",
                "Positions char.Position",
                "Gets or sets the character's position (Stopped, Moving, Sitting, Resting, Sleeping, Unconscious, Dead).",
                "user:Send('Current position: ' .. tostring(char.Position))"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char.Location", "Location",
                "Location char.Location",
                "Gets or sets the character's Location (Area and Point coordinates).",
                "char.Location = Location(server.MainArea, server.MainArea.Start)"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:GetStatCurrent(statName)", "GetStatCurrent('hp')",
                "long char:GetStatCurrent(string statName)",
                "Returns the current value of the specified stat (e.g. 'hp', 'mp', 'speed', 'sight').",
                "local hp = char:GetStatCurrent('hp')"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:GetStatMax(statName)", "GetStatMax('hp')",
                "long char:GetStatMax(string statName)",
                "Returns the maximum value of the specified stat (e.g. 'hp', 'mp', 'speed').",
                "local maxHp = char:GetStatMax('hp')"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:RemoveStatusEffects(silent)", "RemoveStatusEffects(false)",
                "void char:RemoveStatusEffects(bool silent)",
                "Removes active status effects on the character.",
                "char:RemoveStatusEffects(false)"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:DoCommand(commandText)", "DoCommand('look')",
                "void char:DoCommand(string commandText)",
                "Makes the character execute any MUD command string.",
                "char:DoCommand('look')"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:Stop(silent)", "Stop(false)",
                "void char:Stop(bool silent)",
                "Stops the character's current movement or action.",
                "char:Stop(false)"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:Die(killer)", "Die(char)",
                "void char:Die(Character killer)",
                "Immediately slays the character and triggers death/corpse handling.",
                "target:Die(char)"));

            list.Add(new ApiEntry(
                "3. char / actor (Ultimud.Character)", "char",
                "char:GenerateLookDescription(mapOnly)", "GenerateLookDescription(false)",
                "string char:GenerateLookDescription(bool mapOnly)",
                "Generates the room description, ASCII map, and nearby object list for the character's current location.",
                "user:Send(char:GenerateLookDescription(false))"));

            // 4. Command Helper Library (Ultimud.Command)
            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.ToInt(str, defaultVal)", "ToInt(arg, -1)",
                "int Command.ToInt(string str, int defaultValue)",
                "Safely parses a string (automatically stripping any leading '$') into an integer, or returns defaultValue if invalid.",
                "local amount = Command.ToInt(user:GetArgument(), -1)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.FormatNumber(value)", "FormatNumber(char.Value)",
                "string Command.FormatNumber(long value)",
                "Formats a number using the server's NumberFormat setting (e.g. 1000 -> '1,000').",
                "user:Send('Gold: $' .. Command.FormatNumber(char.Value))"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.GetShop(user)", "GetShop(user)",
                "Shop Command.GetShop(User user)",
                "Returns the Shop object at the user's current location, or nil if no shop is here.",
                "local shop = Command.GetShop(user)\r\nif shop == nil then user:Send('No shop here!') return true end"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.FindItemHereOrInv(user, name)", "FindItemHereOrInv(user, arg)",
                "Item Command.FindItemHereOrInv(User user, string name)",
                "Searches for an Item matching 'name' at the player's location first, then in their inventory.",
                "local item = Command.FindItemHereOrInv(user, arg)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.FindCharacterInRoom(user, name)", "FindCharacterInRoom(user, arg)",
                "Character Command.FindCharacterInRoom(User user, string name)",
                "Finds an NPC or Player at the user's current location matching 'name'.",
                "local target = Command.FindCharacterInRoom(user, arg)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.FindOnlineUser(name)", "FindOnlineUser(arg)",
                "User Command.FindOnlineUser(string name)",
                "Finds a connected player anywhere on the server by character name prefix.",
                "local targetUser = Command.FindOnlineUser(targetName)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.RequireAdmin(user)", "RequireAdmin(user)",
                "bool Command.RequireAdmin(User user)",
                "Checks if user has Administrator privileges. If not, sends an error message and returns false.",
                "if not Command.RequireAdmin(user) then return true end"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.IsSilenced(user)", "IsSilenced(user)",
                "bool Command.IsSilenced(User user)",
                "Returns true if the player has been silenced by an administrator.",
                "if Command.IsSilenced(user) then user:Send('You are silenced.') return true end"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.ModifyStat(char, statName, delta)", "ModifyStat(char, 'hp', 50)",
                "long Command.ModifyStat(Character ch, string statName, long delta)",
                "Adds 'delta' (positive to heal/restore, negative to damage/drain) to a character's stat, clamped between 0 and Max.",
                "local newHp = Command.ModifyStat(char, 'hp', 50)\r\nuser:Send('Healed! HP is now ' .. newHp)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.GiveItemBlueprint(char, itemName)", "GiveItemBlueprint(char, 'Iron Sword')",
                "Item Command.GiveItemBlueprint(Character ch, string itemName)",
                "Spawns a new copy of an Item blueprint by name and places it in the character's inventory.",
                "local item = Command.GiveItemBlueprint(char, 'Iron Sword')"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.SpawnNpcBlueprint(char, npcName)", "SpawnNpcBlueprint(char, 'Goblin Scout')",
                "Npc Command.SpawnNpcBlueprint(Character ch, string npcName)",
                "Spawns a new copy of an NPC blueprint by name at the character's location.",
                "local npc = Command.SpawnNpcBlueprint(char, 'Goblin Scout')"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.MoveDir(user, dir, selfMsg, roomLeave, roomArrive)", "MoveDir(user, 'North', 'You move {dir}.', '{1} leaves {dir}.', '{1} arrives from the {dir}.')",
                "bool Command.MoveDir(User user, string dirName, string selfLeaveMsg, string roomLeaveMsg, string roomArriveMsg)",
                "Moves the player in the specified direction ('North', 'South', 'East', 'West', 'Northeast', 'Northwest', 'Southeast', 'Southwest') with customizable messages.",
                "return Command.MoveDir(user, 'North', 'You stride {dir}.', '{1} strides {dir}.', '{1} arrives from the {dir}.')"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.SetPosition(user, pos, selfMsg, roomMsg, alreadyMsg)", "SetPosition(user, 'Resting', 'You sit down and rest.', '{1} sits down and rests.', 'You are already resting.')",
                "bool Command.SetPosition(User user, string posName, string selfMsg, string roomMsg, string alreadyMsg)",
                "Changes the player's position ('Stopped', 'Sitting', 'Resting', 'Sleeping') and broadcasts messages to the room.",
                "return Command.SetPosition(user, 'Sitting', 'You sit down.', '{1} sits down.', 'You are already sitting.')"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.BroadcastRoom(user, selfMsg, roomMsg, mustHear)", "BroadcastRoom(user, 'You cheer!', '{1} cheers!', false)",
                "void Command.BroadcastRoom(User user, string selfMsg, string roomMsg, bool mustHear)",
                "Sends selfMsg to the player and roomMsg ({1} = player name) to everyone else at the player's location.",
                "Command.BroadcastRoom(user, '{color:brightyellow}You cheer enthusiastically!{color:reset}', '{1} cheers enthusiastically!', false)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.BroadcastTargetRoom(user, target, selfMsg, targetMsg, roomMsg, mustHear)", "BroadcastTargetRoom(user, target, 'You wave at {2}.', '{1} waves at you.', '{1} waves at {2}.', false)",
                "void Command.BroadcastTargetRoom(User user, Character target, string selfMsg, string targetMsg, string roomMsg, bool mustHear)",
                "Sends selfMsg to actor, targetMsg to target, and roomMsg ({1} = actor, {2} = target) to bystanders in the room.",
                "Command.BroadcastTargetRoom(user, target, 'You high-five {2}!', '{1} high-fives you!', '{1} high-fives {2}!', false)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.BroadcastGlobal(user, selfMsg, globalMsg, respectIgnore)", "BroadcastGlobal(user, '[GLOBAL] You: hi', '[GLOBAL] ' .. char.Name .. ': hi', true)",
                "void Command.BroadcastGlobal(User user, string selfMsg, string globalMsg, bool respectIgnore)",
                "Sends selfMsg to user and globalMsg to all connected players on the server.",
                "Command.BroadcastGlobal(user, 'You shout!', char.Name .. ' shouts!', true)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.CalculatePower(character)", "CalculatePower(char)",
                "long Command.CalculatePower(Character ch)",
                "Calculates the sum of all Max stats on a character for combat power comparisons.",
                "local power = Command.CalculatePower(char)"));

            list.Add(new ApiEntry(
                "4. Command (Built-in Helper Library)", "Command",
                "Command.DoLook(user) / Command.DoExamine(user)", "DoLook(user)",
                "bool Command.DoLook(User user) | bool Command.DoExamine(User user)",
                "Invokes the built-in Look or Examine logic.",
                "Command.DoLook(user)"));

            // 5. Color Tags
            string[] colors = new string[]
            {
                "brightred", "brightgreen", "brightyellow", "brightblue", "brightmagenta", "brightcyan", "brightwhite",
                "red", "green", "yellow", "blue", "magenta", "cyan", "white", "gray", "darkgray", "reset"
            };
            foreach (string c in colors)
            {
                string tag = "{color:" + c + "}";
                list.Add(new ApiEntry(
                    "5. Color Tags ({color:...})", "color",
                    tag, tag,
                    tag,
                    c == "reset" ? "Resets text color back to default" : ("Sets text color to " + c),
                    "user:Send('" + tag + "Sample colored text{color:reset}')"));
            }

            // 6. Ready-to-Use Code Snippets & Templates
            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Template: Custom Player Command (Heal / Meditate)", "",
                "Full Custom Player Command Template",
                "A complete command script that checks position, restores HP, and broadcasts a message to the room.",
                @"-- ============================================================================
-- Custom Command: meditate
-- Restores 25 HP when resting or sitting
-- ============================================================================
local HEAL_AMOUNT = 25

if char.FightSystem ~= nil then
    user:Send('You cannot concentrate while fighting!')
    return true
end

local newHp = Command.ModifyStat(char, 'hp', HEAL_AMOUNT)
Command.BroadcastRoom(user,
    '{color:brightcyan}You meditate peacefully and recover ' .. HEAL_AMOUNT .. ' HP (Current HP: ' .. newHp .. ').{color:reset}',
    '{1} closes their eyes and meditates peacefully.',
    false)
return true"));

            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Template: Consumable Healing Potion (Item.OnUse)", "",
                "Item.OnUse Script Template",
                "Use this in an Item's OnUse property (with EquipLocation = None) to create a consumable potion.",
                @"-- ============================================================================
-- Item.OnUse: Healing Potion (+50 HP & cures poison)
-- ============================================================================
local HEAL_AMOUNT = 50
local newHp = Command.ModifyStat(actor, 'hp', HEAL_AMOUNT)
actor:Send('{color:brightgreen}A warm glow fills your body! (+' .. HEAL_AMOUNT .. ' HP, now ' .. newHp .. '){color:reset}')"));

            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Template: Area Region Enter Trigger (Region.OnEnter)", "",
                "Area.Region.OnEnter Script Template",
                "Use this in a Region's OnEnter property inside the Area Editor to trigger messages, traps, or rewards when a player steps into a region.",
                @"-- ============================================================================
-- Region.OnEnter: Safe Haven / Sanctuary Zone
-- ============================================================================
actor:Send('{color:brightyellow}You step into the Sanctuary of Light. Your wounds feel soothed.{color:reset}')
Command.ModifyStat(actor, 'hp', 20)"));

            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Snippet: Parse Command Argument", "",
                "Read & Validate Command Argument",
                "Reads the next word typed by the player and shows usage help if missing.",
                @"local arg = user:GetArgument()
if arg == nil or arg == '' then
    user:Send('Usage: command <argument>')
    return true
end"));

            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Snippet: Find Target Character in Room", "",
                "Find NPC or Player in Room",
                "Finds a character at the player's location by name.",
                @"local targetName = user:GetArgument()
local target = Command.FindCharacterInRoom(user, targetName)
if target == nil then
    user:Send(""They aren't here."")
    return true
end"));

            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Snippet: Broadcast Message to Room", "",
                "Command.BroadcastRoom Call",
                "Sends a colored message to the player and to everyone else at their location.",
                @"Command.BroadcastRoom(user,
    '{color:brightcyan}You perform a heroic pose!{color:reset}',
    '{1} performs a heroic pose!',
    false)"));

            list.Add(new ApiEntry(
                "6. Code Snippets & Templates", "",
                "Snippet: Require Administrator Check", "",
                "Command.RequireAdmin Guard",
                "Ensures only players with the Administrator flag can run the script.",
                @"if not Command.RequireAdmin(user) then
    return true
end"));

            return list;
        }
    }
}
