using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Organizer
{
	public partial class FindDialog : Form
	{
		public FindDialog()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			TreeNode startingNode = Form1.GetTreeView().SelectedNode;
			string itemToSearchFor = textBox1.Text;
			string textToSearch = Form1.GetRichTextBoxEx().Text;
			int i = Form1.GetRichTextBoxEx().SelectionStart;
			int iStart = i;
			while (true)
			{
				i++;
				//check bounds
				if (i + itemToSearchFor.Length > textToSearch.Length)
				{
					//If out of bounds, return to start of item.
					i = -1;
					if (comboBox1.SelectedItem.Equals("Entire Tree"))
					{
						Form1.GetTreeView().SelectedNode = Form1.GetTreeView().GetNextTreeNode(Form1.GetTreeView().SelectedNode);
						textToSearch = Form1.GetRichTextBoxEx().Text;
					}
				}
				else if (textToSearch.Substring(i, itemToSearchFor.Length).Equals(itemToSearchFor))
				{
					Form1.GetRichTextBoxEx().Select(i, itemToSearchFor.Length);
					break;
				}
				else if (i == iStart && startingNode == Form1.GetTreeView().SelectedNode)
				{
					MessageBox.Show("The text you searched for was not found.");
					break;
				}
			}
		}
	}
}