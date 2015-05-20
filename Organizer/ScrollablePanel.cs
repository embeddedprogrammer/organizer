using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Organizer
{
	public partial class ScrollablePanel : TableLayoutPanel, RichTextBoxEx.IMouseable
	{
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
		}

		public ScrollablePanel()
		{
			InitializeComponent();
		}

		TreeObject currentlySelectedObject;
		RichTextBoxEx currentlySelectedTextBox;

		public void storeText(TreeObject treeObject)
		{
			RichTextBoxEx richTextBox = (RichTextBoxEx)Controls[0];
			if(currentlySelectedTextBox != null)
				currentlySelectedObject.entries[Controls.IndexOf(currentlySelectedTextBox)].EntryText = currentlySelectedTextBox.Rtf;

			/*bool containsRtf = richTextBox.ContainsRtf(Form1.runningForm.preferredFont, Form1.runningForm.preferredFontColor);
			if (containsRtf != treeObject.StoredAsRTF && !treeObject.StoredSeparate)
			{
				//currentFileSaved = false;
				treeObject.StoredAsRTF = containsRtf;
			}
			if (treeObject.StoredAsRTF)
			{
				if (treeObject.Rtf != richTextBox.Rtf)
				{
					//currentFileSaved = false;
					treeObject.Saved = false;
				}
				treeObject.Rtf = richTextBox.Rtf;
			}
			else
			{
				if (treeObject.Text != richTextBox.Text)
				{
					//currentFileSaved = false;
					treeObject.Saved = false;
				}
			}*/
			treeObject.Text = richTextBox.Text; //Always need to store text for version compatibility.
			treeObject.SelectionStart = richTextBox.getCornerIndex();
		}

		public void loadText(TreeObject treeObject)
		{
			currentlySelectedObject = treeObject;
			RichTextBoxEx richTextBox = (RichTextBoxEx)Controls[0];
			//richTextBox.SuspendDrawing();

			SetTextBoxCount(treeObject.entries.Count);
			for(int i = 0; i < treeObject.entries.Count; i++)
			{
				((RichTextBox)Controls[i]).Rtf = treeObject.entries[i].EntryText;
			}

			//try
			//{
			//    //richTextBox.Select(treeObject.SelectionStart, 0);
			//    //richTextBox.ScrollToCaret();
			//}
			//catch (Exception ex)
			//{
			//    //Some sort of asyncronous exception happens here if this has been called inside a RTB event handler.
			//    //We'll just ignore it for now.
			//}
			//richTextBox.ResumeDrawing();
		}

		public void FormatTextBoxMultiMode(RichTextBoxEx textBox)
		{
			textBox.Margin = new Padding(0, 0, 3, 3);
			textBox.Top = 0;
			textBox.Left = 0;
			textBox.Width = 10;
			textBox.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
			textBox.ContentsResized += new ContentsResizedEventHandler(RichTextBoxEx_ContentsResized);
			textBox.Height = textBox.SizeOfContents.Height + 20;
			textBox.Leave += new EventHandler(textBox_Leave);
			textBox.Enter += new EventHandler(textBox_Enter);
		}

		public void textBox_Leave(object sender, EventArgs e)
		{
			currentlySelectedObject.entries[Controls.IndexOf((Control)sender)].EntryText = ((RichTextBoxEx)sender).Rtf;
		}

		public void textBox_Enter(object sender, EventArgs e)
		{
			currentlySelectedTextBox = (RichTextBoxEx)sender;
		}

		public void SetTextBoxCount(int num)
		{
			if (num > 1)
			{
				FormatTextBoxMultiMode((RichTextBoxEx)Controls[0]);
				this.AutoScroll = true;
				Controls[0].Dock = DockStyle.None;
			}
			while (Controls.Count > num && Controls.Count >= 1)
			{
				Controls.RemoveAt(Controls.Count - 1);
			}
			while (Controls.Count < num && Controls.Count <= 999)
			{
				AddTextBox(new RichTextBoxEx());
			}
			if (num == 1)
			{
				this.AutoScroll = false;
				Controls[0].Dock = DockStyle.Fill;
			}
		}

		public void AddTextBox()
		{
			FormatTextBoxMultiMode((RichTextBoxEx)Controls[0]);
			this.AutoScroll = true;
			Controls[0].Dock = DockStyle.None;
			AddTextBox(new RichTextBoxEx());
			Entry entry = new Entry();
			entry.relatedObjects.Add(currentlySelectedObject);
			currentlySelectedObject.entries.Add(entry);
		}

		public void AddTextBox(RichTextBoxEx textBox)
		{
			ColumnCount = 1;
			RowCount += 1;
			FormatTextBoxMultiMode(textBox);
			Controls.Add(textBox, 0, RowCount - 1);
		}

		public void RichTextBoxEx_ContentsResized(object sender, ContentsResizedEventArgs e)
		{
			if(((RichTextBoxEx)sender).Dock == DockStyle.None)
				((RichTextBoxEx)sender).Height = e.NewRectangle.Height + 20;
		}

		public void doMouseWheel(MouseEventArgs e)
		{
			OnMouseWheel(e);
		}
	}
}
