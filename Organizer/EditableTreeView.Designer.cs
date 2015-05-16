namespace Organizer
{
	partial class EditableTreeView
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
			this.components = new System.ComponentModel.Container();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.alphabetizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNodeToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.expandAllToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.alphabetizeToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(142, 114);
			// 
			// addNodeToolStripMenuItem
			// 
			this.addNodeToolStripMenuItem.Name = "addNodeToolStripMenuItem";
			this.addNodeToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.addNodeToolStripMenuItem.Text = "Add Node";
			this.addNodeToolStripMenuItem.Click += new System.EventHandler(this.addNodeToolStripMenuItem_Click);
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.renameToolStripMenuItem.Text = "Rename";
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
			// 
			// expandAllToolStripMenuItem
			// 
			this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
			this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.expandAllToolStripMenuItem.Text = "Expand All";
			this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// alphabetizeToolStripMenuItem
			// 
			this.alphabetizeToolStripMenuItem.Name = "alphabetizeToolStripMenuItem";
			this.alphabetizeToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.alphabetizeToolStripMenuItem.Text = "Alphabetize";
			this.alphabetizeToolStripMenuItem.Click += new System.EventHandler(this.alphabetizeToolStripMenuItem_Click);
			// 
			// EditableTreeView
			// 
			this.AllowDrop = true;
			this.LabelEdit = true;
			this.LineColor = System.Drawing.Color.Black;
			this.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.EditableTreeView_QueryContinueDrag);
			this.Resize += new System.EventHandler(this.EditableTreeView_Resize);
			this.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView1_AfterLabelEdit);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseUp);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView1_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView1_DragEnter);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
			this.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView1_ItemDrag);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView1_DragOver);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem addNodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem alphabetizeToolStripMenuItem;
	}
}
