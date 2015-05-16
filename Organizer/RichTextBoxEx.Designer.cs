namespace Organizer
{
	partial class RichTextBoxEx
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
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
			this.SuspendLayout();
			// 
			// RichTextBoxEx
			// 
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RichTextBoxEx_KeyDown);
			this.VScroll += new System.EventHandler(this.richTextBoxEx1_VScroll);
			this.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichTextBoxEx_LinkClicked);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextBoxEx_MouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.richTextBoxEx1_MouseDown);
			this.TextChanged += new System.EventHandler(this.RichTextBoxEx_TextChanged);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
