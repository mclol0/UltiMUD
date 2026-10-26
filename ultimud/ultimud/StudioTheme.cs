using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Ultimud
{
    /// <summary>
    /// Modern Game Maker / MUD Studio visual theme, custom ToolStrip renderer, and UI helpers.
    /// </summary>
    internal static class StudioTheme
    {
        public static readonly Color DarkBarBg = Color.FromArgb(15, 23, 42);
        public static readonly Color SubBarBg = Color.FromArgb(30, 41, 59);
        public static readonly Color LightBarBg = Color.FromArgb(241, 245, 249);
        public static readonly Color SurfaceBg = Color.FromArgb(248, 250, 252);
        public static readonly Color CardBg = Color.White;
        public static readonly Color BorderSubtle = Color.FromArgb(203, 213, 225);
        public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);

        public static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
        public static readonly Color AccentGreen = Color.FromArgb(22, 163, 74);
        public static readonly Color AccentPurple = Color.FromArgb(124, 58, 237);
        public static readonly Color AccentAmber = Color.FromArgb(217, 119, 6);
        public static readonly Color AccentTeal = Color.FromArgb(13, 148, 136);
        public static readonly Color AccentRed = Color.FromArgb(220, 38, 38);

        public static readonly Font FontBase = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.25F, FontStyle.Regular);
        public static readonly Font FontHeader = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontMono = new Font("Consolas", 9.5F, FontStyle.Regular);

        private static ToolStripRenderer lightRenderer_;
        private static ToolStripRenderer darkRenderer_;

        public static ToolStripRenderer LightRenderer
        {
            get
            {
                if (lightRenderer_ == null)
                    lightRenderer_ = new StudioToolStripRenderer(false);
                return lightRenderer_;
            }
        }

        public static ToolStripRenderer DarkRenderer
        {
            get
            {
                if (darkRenderer_ == null)
                    darkRenderer_ = new StudioToolStripRenderer(true);
                return darkRenderer_;
            }
        }

        public static void StyleToolStrip(ToolStrip strip, bool dark)
        {
            if (strip == null)
                return;
            strip.GripStyle = ToolStripGripStyle.Hidden;
            strip.Font = FontBase;
            strip.Padding = new Padding(6, 3, 6, 3);
            strip.BackColor = dark ? SubBarBg : LightBarBg;
            strip.ForeColor = dark ? Color.FromArgb(241, 245, 249) : TextPrimary;
            strip.Renderer = dark ? DarkRenderer : LightRenderer;
        }

        public static void StyleMenuStrip(MenuStrip menu)
        {
            if (menu == null)
                return;
            menu.Font = FontBase;
            menu.Padding = new Padding(6, 3, 6, 3);
            menu.BackColor = DarkBarBg;
            menu.ForeColor = Color.FromArgb(226, 232, 240);
            menu.Renderer = DarkRenderer;
            foreach (ToolStripItem item in menu.Items)
            {
                item.ForeColor = Color.FromArgb(241, 245, 249);
            }
        }

        public static void StyleStatusStrip(StatusStrip status)
        {
            if (status == null)
                return;
            status.Font = FontSmall;
            status.BackColor = DarkBarBg;
            status.ForeColor = Color.FromArgb(203, 213, 225);
            status.Renderer = DarkRenderer;
            status.SizingGrip = true;
            foreach (ToolStripItem item in status.Items)
            {
                item.ForeColor = Color.FromArgb(226, 232, 240);
            }
        }

        public static void StylePropertyGrid(PropertyGrid grid)
        {
            if (grid == null)
                return;
            grid.Font = FontBase;
            grid.ViewBackColor = Color.White;
            grid.ViewForeColor = TextPrimary;
            grid.LineColor = Color.FromArgb(226, 232, 240);
            grid.CategoryForeColor = Color.FromArgb(30, 58, 138);
            grid.HelpBackColor = Color.FromArgb(241, 245, 249);
            grid.HelpForeColor = TextPrimary;
            grid.HelpVisible = true;
            grid.ToolbarVisible = true;
        }

        public static void StyleTreeView(TreeView tree)
        {
            if (tree == null)
                return;
            tree.Font = FontBase;
            tree.ItemHeight = 23;
            tree.BackColor = Color.White;
            tree.ForeColor = TextPrimary;
            tree.BorderStyle = BorderStyle.None;
            tree.FullRowSelect = true;
            tree.HideSelection = false;
            tree.ShowLines = true;
            tree.LineColor = Color.FromArgb(203, 213, 225);
        }

        public static void StyleListView(ListView list)
        {
            if (list == null)
                return;
            list.Font = FontBase;
            list.BackColor = Color.White;
            list.ForeColor = TextPrimary;
            list.BorderStyle = BorderStyle.None;
            list.FullRowSelect = true;
        }

        public static Button CreatePillButton(string text, Color backColor, Color foreColor, int width, EventHandler onClick, ToolTip tip, string tooltipText)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Width = width;
            btn.Height = 26;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = ControlPaint.Light(backColor, 0.15f);
            btn.BackColor = backColor;
            btn.ForeColor = foreColor;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(0, 0, 5, 0);
            if (onClick != null)
                btn.Click += onClick;
            if (tip != null && !string.IsNullOrEmpty(tooltipText))
                tip.SetToolTip(btn, tooltipText);
            return btn;
        }

        private class StudioColorTable : ProfessionalColorTable
        {
            private readonly bool dark_;

            public StudioColorTable(bool dark)
            {
                this.dark_ = dark;
                this.UseSystemColors = false;
            }

            public override Color ToolStripBorder
            {
                get { return this.dark_ ? Color.FromArgb(30, 41, 59) : Color.FromArgb(203, 213, 225); }
            }

            public override Color ToolStripGradientBegin
            {
                get { return this.dark_ ? SubBarBg : LightBarBg; }
            }

            public override Color ToolStripGradientMiddle
            {
                get { return this.dark_ ? SubBarBg : LightBarBg; }
            }

            public override Color ToolStripGradientEnd
            {
                get { return this.dark_ ? SubBarBg : LightBarBg; }
            }

            public override Color MenuStripGradientBegin
            {
                get { return DarkBarBg; }
            }

            public override Color MenuStripGradientEnd
            {
                get { return DarkBarBg; }
            }

            public override Color MenuItemSelected
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color MenuItemBorder
            {
                get { return this.dark_ ? Color.FromArgb(71, 85, 105) : Color.FromArgb(147, 197, 253); }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get { return this.dark_ ? Color.FromArgb(30, 41, 59) : Color.FromArgb(191, 219, 254); }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get { return this.dark_ ? Color.FromArgb(30, 41, 59) : Color.FromArgb(191, 219, 254); }
            }

            public override Color ToolStripDropDownBackground
            {
                get { return Color.White; }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return Color.FromArgb(248, 250, 252); }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return Color.FromArgb(248, 250, 252); }
            }

            public override Color ImageMarginGradientEnd
            {
                get { return Color.FromArgb(248, 250, 252); }
            }

            public override Color ButtonSelectedHighlight
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonSelectedGradientBegin
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonSelectedGradientMiddle
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonSelectedGradientEnd
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonSelectedBorder
            {
                get { return this.dark_ ? Color.FromArgb(100, 116, 139) : Color.FromArgb(96, 165, 250); }
            }

            public override Color ButtonCheckedGradientBegin
            {
                get { return this.dark_ ? Color.FromArgb(37, 99, 235) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonCheckedGradientMiddle
            {
                get { return this.dark_ ? Color.FromArgb(37, 99, 235) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonCheckedGradientEnd
            {
                get { return this.dark_ ? Color.FromArgb(37, 99, 235) : Color.FromArgb(219, 234, 254); }
            }

            public override Color ButtonPressedGradientBegin
            {
                get { return this.dark_ ? Color.FromArgb(29, 78, 216) : Color.FromArgb(191, 219, 254); }
            }

            public override Color ButtonPressedGradientMiddle
            {
                get { return this.dark_ ? Color.FromArgb(29, 78, 216) : Color.FromArgb(191, 219, 254); }
            }

            public override Color ButtonPressedGradientEnd
            {
                get { return this.dark_ ? Color.FromArgb(29, 78, 216) : Color.FromArgb(191, 219, 254); }
            }

            public override Color SeparatorDark
            {
                get { return this.dark_ ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225); }
            }

            public override Color SeparatorLight
            {
                get { return Color.Transparent; }
            }
        }

        private class StudioToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly bool dark_;

            public StudioToolStripRenderer(bool dark)
                : base(new StudioColorTable(dark))
            {
                this.dark_ = dark;
                this.RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (e.ToolStrip is ToolStripDropDownMenu)
                {
                    e.TextColor = TextPrimary;
                }
                else if (this.dark_ && e.Item.ForeColor == SystemColors.ControlText)
                {
                    e.TextColor = Color.FromArgb(241, 245, 249);
                }
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is StatusStrip || e.ToolStrip is MenuStrip)
                    return;

                using (Pen p = new Pen(this.dark_ ? Color.FromArgb(51, 65, 85) : BorderSubtle))
                {
                    e.Graphics.DrawLine(p, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
                }
            }
        }
    }
}
