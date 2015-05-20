using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Organizer
{
	public partial class AboutDialog : Form
	{
		public AboutDialog()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Hide();
			Dispose();
		}
	}
}