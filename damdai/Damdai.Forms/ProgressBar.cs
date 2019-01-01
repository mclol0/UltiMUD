using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Damdai.Forms
{
	/// <summary>
	/// Summary description for UserControl.
	/// </summary>
	public class ProgressBar : System.Windows.Forms.UserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProgressBar()
		{
			// This call is required by the Windows.Forms Designer.
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
			// 
			// ProgressBar
			// 
			this.Name = "ProgressBar";
			this.Size = new System.Drawing.Size(200, 16);
		}
		#endregion

		int min_ = 0;	// Minimum value for progress range
		int max_ = 100;	// Maximum value for progress range
		int value_ = 0;		// Current progress

		protected override void OnResize(EventArgs e) {
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e) {
			Graphics g = e.Graphics;
			SolidBrush brush = new SolidBrush(ForeColor);
			float percent = (float)(value_ - min_) / (float)(max_ - min_);
			Rectangle rect = this.ClientRectangle;
			rect.Width = (int)((float)rect.Width * percent);
			g.FillRectangle(new SolidBrush(BackColor), this.ClientRectangle);
			g.FillRectangle(brush, rect);
			Draw3DBorder(g);
		}

		public int Minimum {
			get { return min_; }
			set {
				// Prevent a negative value.
				if(value < 0) {
					min_ = 0;
				}

				// Make sure that the minimum value is never set higher than the maximum value.
				if(value > max_) {
					min_ = value;
				}

				// Ensure value is still in range
				if(value_ < min_) {
					value_ = min_;
				}

				Invalidate();
			}
		}

		public int Maximum {
			get { return max_; }
			set {
				// Make sure that the maximum value is never set lower than the minimum value.
				if(value < min_) {
					min_ = value;
				}

				max_ = value;

				// Make sure that value is still in range.
				if(value_ > max_) {
					value_ = max_;
				}

				Invalidate();
			}
		}

		public int Value {
			get { return value_; }
			set {
				int oldValue = value_;

				// Make sure that the value does not stray outside the valid range.
				if(value < min_) {
					value_ = min_;
				} else if(value > max_) {
					value_ = max_;
				} else {
					value_ = value;
				}

				float percent;

				Rectangle newValueRect = this.ClientRectangle;
				Rectangle oldValueRect = this.ClientRectangle;

				// Use a new value to calculate the rectangle for progress.
				percent = (float)(value_ - min_) / (float)(max_ - min_);
				newValueRect.Width = (int)((float)newValueRect.Width * percent);

				// Use an old value to calculate the rectangle for progress.
				percent = (float)(oldValue - min_) / (float)(max_ - min_);
				oldValueRect.Width = (int)((float)oldValueRect.Width * percent);

				Rectangle updateRect = new Rectangle();

				// Find only the part of the screen that must be updated.
				if (newValueRect.Width > oldValueRect.Width) {
					updateRect.X = oldValueRect.Size.Width;
					updateRect.Width = newValueRect.Width - oldValueRect.Width;
				} else {
					updateRect.X = newValueRect.Size.Width;
					updateRect.Width = oldValueRect.Width - newValueRect.Width;
				}

				updateRect.Height = this.Height;

				// Invalidate the intersection region only.
				this.Invalidate(updateRect);
			}
		}

		private void Draw3DBorder(Graphics g) {
			int PenWidth = (int)Pens.White.Width;

			g.DrawLine(Pens.DarkGray,
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
			g.DrawLine(Pens.DarkGray,
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
			g.DrawLine(Pens.White,
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
			g.DrawLine(Pens.White,
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
		}
	}
}
