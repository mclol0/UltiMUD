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
	public class InputBox : RichTextBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public InputBox()
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
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		ArrayList keysPressed_ = new ArrayList();
		 
		public delegate void EnterKeyHandler();
		public event EnterKeyHandler EnterKeyPressedEvent;
		public void EnterKeyPressed() {
			if(EnterKeyPressedEvent != null) {
				EnterKeyPressedEvent();
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			keysPressed_.Add(e.KeyCode);
			base.OnKeyDown(e);
		}
		protected override void OnKeyUp(KeyEventArgs e) {
			keysPressed_.Remove(e.KeyCode);
			base.OnKeyUp(e);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			const int WM_KEYDOWN = 0x100;
			const int WM_SYSKEYDOWN = 0x104;
			if(msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN) {
				switch(keyData) {
				case Keys.Enter:
					this.EnterKeyPressed();
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
