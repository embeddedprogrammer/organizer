using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Organizer
{
	public partial class HyperlinkDialog : Form
	{
		public HyperlinkDialog()
		{
			InitializeComponent();
		}

		public String TextToDisplay
		{
			get { return textBox1.Text; }
			set { textBox1.Text = value; }
		}

		public String LinkDestination
		{
			get { return textBox2.Text; }
			set { textBox2.Text = value; }
		}

		private DialogResult result;
		public DialogResult Result
		{
			get { return result; }
		}

		//OK
		private void button1_Click(object sender, EventArgs e)
		{
			result = DialogResult.OK;
			Close();
		}

		//Cancel
		private void button2_Click(object sender, EventArgs e)
		{
			result = DialogResult.Cancel;
			Close();
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			button1.Enabled = !textBox2.Text.Equals("");
		}
	}
}