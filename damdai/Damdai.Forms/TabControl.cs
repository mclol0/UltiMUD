using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Damdai.Forms {
	/// <summary>
	/// Summary description for UserControl.
	/// </summary>
	public class TabControl : System.Windows.Forms.TabControl {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TabControl() {
			InitializeComponent();
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			this.DrawMode = TabDrawMode.OwnerDrawFixed;
			BackColor = base.BackColor;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.Name = "TabControl";
			this.Size = new System.Drawing.Size(200, 100);
		}
		#endregion

		Color backColor_;
		const int gradientStretch_ = 400;
		bool showTabs_ = true;

		[Browsable(true), Category("Appearance")]
		override public Color BackColor {
			get { return backColor_; }
			set { backColor_ = value; Invalidate(); }
		}
		[Browsable(true), Category("Appearance")]
		public bool ShowTabs {
			get { return showTabs_; }
			set { showTabs_ = value; Invalidate(); }
		}

		Control GetParentWithBackColor(Control parent) {
			if(parent.BackColor == Color.Transparent) {
				return GetParentWithBackColor(parent.Parent);
			}
			return parent;
		}
		void PaintBackground(Graphics g) {
			Control parent = GetParentWithBackColor(Parent);
			if(parent.GetType().Namespace == "Dcd") {
				using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-gradientStretch_, 0, Width+gradientStretch_, Height), parent.BackColor, Color.White, LinearGradientMode.Horizontal)) {
					g.FillRectangle(b, 0, 0, Width, Height);
				}
			} else {
				g.Clear(Parent.BackColor);
			}
		}

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;
            PaintBackground(g);
			int width = this.Width-1;
			int height = this.Height-1;

			Point[] points = {
				new Point(0, 0),
				new Point(width, 0),
				new Point(width, height),
				new Point(0, height),
				new Point(0, 0)
			};

			using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-gradientStretch_, 0, Width+gradientStretch_, Height), BackColor, Color.White, LinearGradientMode.Horizontal)) {
				//g.FillPolygon(new SolidBrush(Color.White), points);
				g.FillPolygon(b, points);
				g.DrawPolygon(new Pen(BackColor, 1), points);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			if(!showTabs_) {
				return;
			}
			Graphics g = e.Graphics;
			for(int i = 0; i < TabCount; ++i) {
				if(!e.ClipRectangle.IntersectsWith(GetTabRect(i))) {
					continue;
				}

				Rectangle itemRect = this.GetTabRect(i);
				int x = itemRect.X;
				int width = itemRect.Width;
				int height = itemRect.Height;

				Point[] points;
				if(i == 0) {
					points = new Point[] {
						new Point(0, 0),
						new Point(width+x+10, 0),
						new Point(width+x, height),
						new Point(0, height),
						new Point(0, 0)
					};
				} else {
					points = new Point[] {
						new Point(x+10, 0),
						new Point(width+x+10, 0),
						new Point(width+x, height),
						new Point(x, height)
					};
				}

				//Region r = new Region(new Rectangle(0, 1, ItemSize.Width, ItemSize.Height));
				//r.Translate(x, 0);
				Color backColor = (i == SelectedIndex? Color.DarkGray : Color.LightGray);
				//g.FillRegion(new SolidBrush(backColor), r);
				//g.DrawLines(new Pen(BackColor), new Point[] {new Point(x, ItemSize.Height), new Point(x+ItemSize.Width-1, ItemSize.Height), new Point(x+ItemSize.Width-1, 0)});
				g.FillPolygon(new SolidBrush(backColor), points);
				g.DrawPolygon(new Pen(Color.Orange), points);

				int stringWidth = (int)g.MeasureString(TabPages[i].Text, Font).Width;
				Point p = new Point(x + ((width-stringWidth)/2)+5, (int)(height-g.MeasureString(TabPages[i].Text, Font).Height)/2);
				g.DrawString(this.TabPages[i].Text, Font, new SolidBrush(this.ForeColor), p);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			Invalidate();
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			if(e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) {
				Invalidate();
			}
		}
	}
}
