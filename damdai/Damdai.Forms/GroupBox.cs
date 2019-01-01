using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace Damdai.Forms {
	/// <summary>
	/// Summary description for Component.
	/// </summary>
	public class GroupBox : System.Windows.Forms.GroupBox {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GroupBox(System.ComponentModel.IContainer container) {
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public GroupBox() {
			///
			/// This call is required by the Windows.Forms Designer.
			///
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		const int headerHeight_ = 15;
		const int gradientStretch_ = 400;

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
				new Point(3, 0),
				new Point(width-3, 0),
				new Point(width, 3),
				new Point(width, height-3),
				new Point(width-3, height),
				new Point(3, height),
				new Point(0, height-3),
				new Point(0, 3)
			};
			using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-gradientStretch_, 0, Width+gradientStretch_, Height), BackColor, Color.White, LinearGradientMode.Horizontal)) {
				g.FillPolygon(b, points);
				g.DrawPolygon(new Pen(BackColor), points);
			}

			points = new Point[] {
				new Point(3, 0),
				new Point(width-3, 0),
				new Point(width, 3),
				new Point(width, headerHeight_),
				new Point(0, headerHeight_),
				new Point(0, 3),
			};
			using(LinearGradientBrush b = new LinearGradientBrush(new Rectangle(0, 0, Width, headerHeight_), BackColor, Color.White, LinearGradientMode.Horizontal)) {
				g.FillPolygon(b, points);
				g.DrawPolygon(new Pen(BackColor), points);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;
			Point p = new Point(5, (headerHeight_-(int)g.MeasureString(Text, Font).Height)/2);
			g.DrawString(Text, Font, new SolidBrush(this.ForeColor), p);
		}
	}
}
