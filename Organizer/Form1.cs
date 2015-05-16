using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Organizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
			richTextBoxEx1.AutoWordSelection = false;
			richTextBoxEx1.startingUp = false;
        }

		List<FontFamily> fonts;

        private void Form1_Load(object sender, EventArgs e)
        {
			this.BeginInvoke(new EventHandler(delegate
			{
				richTextBoxEx1.AutoWordSelection = false;
			}));

			preferredFont = richTextBoxEx1.Font;
			preferredFontColor = richTextBoxEx1.ForeColor;
			
            richTextBoxEx1.AllowDrop = true;
            editableTreeView1.AllowDrop = true;
            richTextBoxEx1.DragEnter += new DragEventHandler(richTextBoxEx1_DragEnter);
            richTextBoxEx1.DragDrop += new DragEventHandler(richTextBoxEx1_DragDrop);
			richTextBoxEx1.DragOver += new DragEventHandler(richTextBoxEx1_DragOver);

			editableTreeView1.AfterAddNode += new EditableTreeView.AfterAddNodeDelegate(editableTreeView1_AfterAddNode);
			
			fonts = new List<FontFamily>();
			foreach (FontFamily font in FontFamily.Families)
			{
				fonts.Add(font);
				toolStripComboBox1.Items.Add(font.Name);
			}

			for(int i = 8; i <= 72; i++)
			{
				toolStripComboBox2.Items.Add(i);
			}

			String[] args = Environment.GetCommandLineArgs();
			//setDebugValue("CmdArgsz", printArray(args));
			if (args.Length > 1) //first argument is the excecutable path
			{
				if (!openFile(args[1]))
					newFile();
			}
			else
			{
				newFile();
			}
			//setDebugValue("Recent Files", Organizer.Properties.Settings.Default.RecentFiles);


			//this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			//this.newToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
			//this.newToolStripMenuItem.Text = "New";
			//this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);


			//Organizer.Properties.Settings.Default.Speed

			//Properties.Settings.Default["SomeProperty"] = "Some Value";
			//Properties.Settings.Default.Save(); // Saves settings in application configuration file
			string[] paths = Organizer.Properties.Settings.Default.RecentFiles.Split(',');
			recentFilePaths = new List<string>(paths);
			recentFileMenuItems = new List<ToolStripMenuItem>();
			UpdateRecentFileDropdown();

			string[] entries = Organizer.Properties.Settings.Default.SpecialEntries.Split(',');
			specialEntries = new List<string>(entries);
			if (Organizer.Properties.Settings.Default.ShowSpecialEntriesToolbar)
			{
				ShowCharacterToolStrip();
				UpdateCharacterToolStrip();
			}
		}

		List<string> recentFilePaths;
		List<ToolStripMenuItem> recentFileMenuItems;

		private void AddRecentFile(string filePath)
		{
			if (recentFilePaths.Contains(filePath))
			{
				recentFilePaths.Remove(filePath);
			}
			recentFilePaths.Insert(0, filePath);
			UpdateRecentFileDropdown();
		}

		private void UpdateRecentFileDropdown()
		{
			while (recentFilePaths.Count > 4)
			{
				recentFilePaths.RemoveAt(4);
			}
			while (recentFilePaths.Count > recentFileMenuItems.Count)
			{
				ToolStripMenuItem recentFileMenuItem = new ToolStripMenuItem();
				fileToolStripMenuItem.DropDownItems.Add(recentFileMenuItem);
				recentFileMenuItems.Add(recentFileMenuItem);
				recentFileMenuItem.Click += new EventHandler(recentFileMenuItem_Click);
			}
			string paths = "";
			for (int i = 0; i < recentFilePaths.Count; i++)
			{
				recentFileMenuItems[i].Text = recentFilePaths[i];
				paths += ((paths.Length > 0) ? "," : "") + recentFilePaths[i];
			}
			Organizer.Properties.Settings.Default.RecentFiles = paths;
		}

		private void recentFileMenuItem_Click(object sender, EventArgs e)
		{
			openFile(recentFilePaths[recentFileMenuItems.IndexOf((ToolStripMenuItem)sender)]);
		}


		ToolStrip characterToolStrip;
		List<string> specialEntries;

		private void showSpecialCharactersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (characterToolStrip == null)
			{
				ShowCharacterToolStrip();
			}
			else
			{
				toolStripContainer2.TopToolStripPanel.Controls.Remove(characterToolStrip);
				characterToolStrip = null;
				Organizer.Properties.Settings.Default.ShowSpecialEntriesToolbar = false;
			}
		}

		public void ShowCharacterToolStrip()
		{
			characterToolStrip = new ToolStrip();
			toolStripContainer2.TopToolStripPanel.Controls.Add(characterToolStrip);
			if (characterContextMenu == null)
			{
				characterContextMenu = new ContextMenuStrip(components);
				ToolStripMenuItem removeButton = new ToolStripMenuItem();
				removeButton.Size = new Size(40, 22);
				removeButton.Text = "Remove";
				removeButton.Click += new System.EventHandler(removeButton_Click);
				characterContextMenu.Items.Add(removeButton);
			}
			Organizer.Properties.Settings.Default.ShowSpecialEntriesToolbar = true;
		}

		ContextMenuStrip characterContextMenu;

		public void UpdateCharacterToolStrip()
		{
			while (specialEntries.Count < characterToolStrip.Items.Count)
			{
				characterToolStrip.Items.RemoveAt(0);
			}
			while (specialEntries.Count > characterToolStrip.Items.Count)
			{
				ToolStripButton button = new ToolStripButton();
				button.DisplayStyle = ToolStripItemDisplayStyle.Text;
				button.Click += new EventHandler(characterButtonMenuItem_Click);
				button.MouseUp += new MouseEventHandler(button_MouseUp);
				characterToolStrip.Items.Add(button);
			}
			string entries = "";
			for (int i = 0; i < specialEntries.Count; i++)
			{
				characterToolStrip.Items[i].Text = specialEntries[i];
				characterToolStrip.Items[i].ToolTipText = "Add character " + specialEntries[i];
				entries += ((entries.Length > 0) ? "," : "") + specialEntries[i];
			}
			Organizer.Properties.Settings.Default.SpecialEntries = entries;
			Properties.Settings.Default.Save();
		}

		ToolStripButton rightClickCharacterButton;

		private void button_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				Point buttonLocation = ((ToolStripButton)sender).Bounds.Location;
				characterContextMenu.Show(this, buttonLocation.X + e.X, buttonLocation.Y + e.Y);
				rightClickCharacterButton = ((ToolStripButton)sender);
			}
		}

		private void characterButtonMenuItem_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectedText = ((ToolStripButton)sender).Text;
		}

		private void removeButton_Click(object sender, EventArgs e)
		{
			if (rightClickCharacterButton != null)
			{
				specialEntries.Remove(rightClickCharacterButton.Text);
				characterToolStrip.Items.Remove(rightClickCharacterButton);
				UpdateCharacterToolStrip();
			}
		}

		private FontFamily getFont(String fontName)
		{
			foreach (FontFamily font in FontFamily.Families)
			{
				if(font.Name.Equals(fontName))
				{
					return font;
				}
			}
			return null;
		}

		int num = 0;

		public void editableTreeView1_AfterAddNode(TreeNode nodeAdded)
		{
			bool v2dot1 = nodeAdded.Parent != null ? GetTreeObject(nodeAdded.Parent).StoredAsRTF : currentFileVersion.Equals("2.1");
			//string text = v2dot1 ? @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs17\par\r\n}" : "abc";
			TreeObject treeObject = new TreeObject("abc");
			treeObject.StoredAsRTF = false;
			nodeAdded.Tag = treeObject;
		}		

		#region [richTextBoxEx1 DRAG FUNCTIONALITY]

		private void richTextBoxEx1_DragEnter(object sender, DragEventArgs e)
        {
        }

		int eventComplete = 0;

        private void richTextBoxEx1_DragDrop(object sender, DragEventArgs e)
        {
			setDebugValue("Num of times", "" + ++eventComplete);

			if (e.Data.GetDataPresent(DataFormats.Text))
			{
				if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
				{
					//e.Effect = DragDropEffects.Copy;
				}
				else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
				{
					//e.Effect = DragDropEffects.Move;
				}
				else
				{
					//e.Effect = DragDropEffects.None;
				}
			}
		}

		private void richTextBoxEx1_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.Text))
			{
				if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
				{
					e.Effect = DragDropEffects.Copy;
				}
				else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
				{
					e.Effect = DragDropEffects.Move;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
		}

		bool richTextBoxEx1IsDragSource = false;

		private void richTextBoxEx1_MouseUp(object sender, MouseEventArgs e)
		{
			richTextBoxEx1IsDragSource = false;
			//setDebugValue("IsDragSource", "" + richTextBoxEx1IsDragSource);
		}

		#endregion

		#region [DEBUG VALUE LIST]

		//KeyValuePair<string, string> myKeyValuePair = new KeyValuePair<string, string>("defaultkey", "defaultvalue");

		Dictionary<string, string> debugList = new Dictionary<string, string>();

		public String printObject(Object o)
		{
			if (o == null)
			{
				return "null";
			}
			else
			{
				return o.ToString();
			}
		}

		public String printArray(Object[] o)
		{
			if (o == null)
			{
				return "null";
			}
			else if (o.Length == 0)
			{
				return o.ToString() + ". Empty.";
			}
			else
			{
				string s = "";
				for (int i = 0; i < o.Length; i++)
				{
					s += (i == 0 ? "" : ", ") + printObject(o[i]);
				}
				return s;
			}
		}

		public static Form1 runningForm;

		public static RichTextBoxEx GetRichTextBoxEx()
		{
			return runningForm.richTextBoxEx1;
		}

		public static EditableTreeView GetTreeView()
		{
			return runningForm.editableTreeView1;
		}

		public static void SetDebugValue(String key, String value)
		{
			runningForm.setDebugValue(key, value);
		}

		public void setDebugValue(String key, String value)
		{
			debugList[key] = value;
			updateDebugList();
		}

		public void updateDebugList()
		{
			if (debugRtb != null)
			{
				String debugString = "";
				foreach (KeyValuePair<string, string> kvp in debugList)
				{
					debugString = debugString + kvp.Key + ": " + kvp.Value + "\n";
				}
				debugRtb.Text = debugString;
			}
		}

		#endregion DEBUG LIST

		#region TreeNode Navigation

		List<TreeNode> lastSelectedNodes;
		TreeNode lastSelectedNode;

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			//Save last
			if (lastSelectedNode != null)
				storeTreeObject();

			lastSelectedNode = e.Node;
			if (!navigationEvent)
			{
				selectedNodeStackIndex++;
				if (selectedNodeStackIndex > lastSelectedNodes.Count)
					selectedNodeStackIndex = lastSelectedNodes.Count;
				while (selectedNodeStackIndex < lastSelectedNodes.Count)
					lastSelectedNodes.RemoveAt(selectedNodeStackIndex);
				lastSelectedNodes.Add(e.Node);
				if (lastSelectedNodes.Count > 100)
					lastSelectedNodes.RemoveAt(0);
				UpdateNodeList();
			}

			//Open new
			loadTreeObject();

			setDebugValue("LastSelectedNodeIndex", "" + editableTreeView1.getIndexOfNode(editableTreeView1.SelectedNode));
			setDebugValue("ChildCount", "" + editableTreeView1.getChildCount(editableTreeView1.SelectedNode));
			updateSepIcon();
		}

		TreeObject treeObject;

		public void storeTreeObject()
		{
			treeObject = GetTreeObject(lastSelectedNode);
			if (treeObject != null)
			{
				storeText(richTextBoxEx1, treeObject);
			}
			else
			{
				setDebugValue("Error saving text", "Error saving text.");
			}
		}

		public void loadTreeObject()
		{
			treeObject = GetTreeObject(lastSelectedNode);
			setDebugValue("Loaded Node", lastSelectedNode.Text);
			if (treeObject != null)
			{
				loadText(richTextBoxEx1, treeObject);
			}
			else
			{
				richTextBoxEx1.Text = "Error loading text.";
			}
		}

		public void storeText(RichTextBoxEx richTextBox, TreeObject treeObject)
		{
			bool containsRtf = richTextBox.ContainsRtf(preferredFont, preferredFontColor);
			if (containsRtf != treeObject.StoredAsRTF && ! treeObject.StoredSeparate)
			{
				currentFileSaved = false;
				treeObject.StoredAsRTF = containsRtf;
			}
			if (treeObject.StoredAsRTF)
			{
				if (treeObject.Rtf != richTextBox.Rtf)
				{
					currentFileSaved = false;
					treeObject.Saved = false;
				}
				treeObject.Rtf = richTextBox.Rtf;
			}
			else
			{
				if (treeObject.Text != richTextBox.Text)
				{
					currentFileSaved = false;
					treeObject.Saved = false;
				}
			}
			treeObject.Text = richTextBox.Text; //Always need to store text for version compatibility.
			treeObject.SelectionStart = richTextBox.getCornerIndex();
		}

		public void loadText(RichTextBoxEx richTextBox, TreeObject treeObject)
		{
			richTextBox.SuspendDrawing();
			if (treeObject.StoredSeparate && treeObject.Rtf == null)
			{
				ReadTreeObjectRtf(treeObject);
			}
			if (treeObject.StoredAsRTF && treeObject.Rtf != null)
			{
				richTextBox.Rtf = treeObject.Rtf;
			}
			else
			{
				richTextBox.Text = treeObject.Text;
				richTextBox.SelectAll();
				richTextBox.SelectionFont = preferredFont;
				richTextBox.SelectionColor = preferredFontColor;
			}
			try
			{
				richTextBox.Select(treeObject.SelectionStart, 0);
				richTextBox.ScrollToCaret();
			}
			catch(Exception ex)
			{
				//Some sort of asyncronous exception happens here if this has been called inside a RTB event handler.
				//We'll just ignore it for now.
			}
			richTextBox.ResumeDrawing();
		}

		public static TreeObject GetTreeObject(TreeNode node)
		{
			if (node.Tag == null)
			{
				SetDebugValue("Error", "Node " + node.Text + " tag is null");
				return null;
			}
			else if (node.Tag is TreeObject)
			{
				return (TreeObject)node.Tag;
			}
			else
			{
				SetDebugValue("Error", "Tag on node " + node.Text + " cannot be cast to TreeObject");
				return null;
			}
		}

		#endregion

		#region [TIMER]

		long ticks;

		public static TreeNode FindChildById(int id)
		{
			return runningForm.editableTreeView1.findChildById(id);
		}

		public static void StartTimer()
		{
			runningForm.startTimer();
		}

		public static void ShowTime(String task)
		{
			runningForm.showTime(task);
		}

		public void startTimer()
		{
			ticks = DateTime.Now.Ticks;
		}

		public void showTime(String task)
		{
			long stopTime = DateTime.Now.Ticks;
			long spanTime = stopTime - ticks;
			setDebugValue(task, "" + (spanTime / 10000) + " ms");
			ticks = stopTime;
		}

		#endregion

		#region FileOperations

		public bool openFile(string filePath)
		{
			string version;
			if (filePath.EndsWith(".txt"))
			{
				version = "1.0";
			}
			else if (filePath.EndsWith(".hrc"))
			{
				version = "2.0";
			}
			else
			{
				MessageBox.Show("File does not have a valid extension.");
				return false;
			}

			if (File.Exists(filePath))
			{
				TreeNode nodeToSelect;
				startTimer();
				if (version.Equals("1.0"))
				{
					nodeToSelect = openFileVersion1(filePath);
				}
				else //if (version.Equals("2.0"))
				{
					nodeToSelect = openFileVersion2(filePath);
					if (nodeToSelect == null)
						return false;
				}
				showTime("Total time to read file");
				currentFileSaved = true;
				setCurrentFile(filePath, currentFileVersion);
				AddRecentFile(filePath);
				lastSelectedNodes = new List<TreeNode>();
				selectedNodeStackIndex = 0;
				editableTreeView1.SelectedNode = nodeToSelect;
				return true;
			}
			else
			{
				MessageBox.Show("File not found.");
				return false;
			}
		}

		public TreeNode openFileVersion1(string filename)
		{
			startTimer();
			String openText = System.IO.File.ReadAllText(filename);
			string[] topics = openText.Split('#');
			String nodeName;
			String path;
			String text;
			int titleDelimiterLocation;
			TreeNode cursor = null;
			TreeNode newCursor;
			for (int i = 1; i < topics.Length; i++)
			{
				String topic = topics[i];
				titleDelimiterLocation = topic.IndexOf("\n");
				path = topic.Substring(0, titleDelimiterLocation);
				text = topic.Substring(titleDelimiterLocation + 1, topic.Length - titleDelimiterLocation - 1);
				if (i == 1)
				{
					editableTreeView1.Nodes.Clear();
					cursor = new TreeNode(path);
					editableTreeView1.Nodes.Add(cursor);
					cursor.Tag = new TreeObject(text);
				}
				else
				{
					//If not a child node, move cursor to parent node.
					while (!path.StartsWith(cursor.FullPath))
					{
						cursor = cursor.Parent;
					}
					nodeName = path.Substring(path.LastIndexOf("\\") + 1, path.Length - path.LastIndexOf("\\") - 1);
					newCursor = new TreeNode(nodeName);
					cursor.Nodes.Add(newCursor);
					cursor = newCursor;
					cursor.Tag = new TreeObject(text);
				}
			}
			return editableTreeView1.getNodeAtIndex(0);
		}

		public bool nodesStoredSeparatelyOrHaveId(TreeNodeCollection nodes)
		{
			if (TreeObject.idUsed)
				return true;
			foreach (TreeNode node in nodes)
			{
				if (nodeStoredSeparately(node))
					return true;
			}
			return false;
		}

		public bool nodeStoredSeparately(TreeNode node)
		{
			return GetTreeObject(node).StoredSeparate || nodesContainRTF(node.Nodes);
		}

		public bool nodesContainRTF(TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (nodeContainsRTF(node))
					return true;
			}
			return false;
		}

		public bool nodeContainsRTF(TreeNode node)
		{
			return GetTreeObject(node).StoredAsRTF || nodesContainRTF(node.Nodes);
		}

		public void SaveUnsavedSeparatelyStoredNodes(TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				SaveUnsavedSeparatelyStoredNodes(node);
			}
		}

		public void SaveUnsavedSeparatelyStoredNodes(TreeNode node)
		{
			TreeObject treeObject = GetTreeObject(node);
			if (treeObject.StoredSeparate && !treeObject.Saved)
			{
				WriteTreeObjectRtf(treeObject);
			}
			SaveUnsavedSeparatelyStoredNodes(node.Nodes);
		}

		public void saveFile(string filePath, string version, bool fileSavedPreviously)
		{
			if (fileSavedPreviously && !File.Exists(filePath))
			{
				MessageBox.Show("File not found.");
				return;
			}
			storeTreeObject();
			if (nodesStoredSeparatelyOrHaveId(editableTreeView1.Nodes) && ! VersionAtLeast(version, "2.2"))
			{
				if (MessageBox.Show("Only organizer files version 2.2 and higher support text stored separately or links. All text will be saved internally and links will be mixed up. Continue?",
					"Organizer", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
					return;
			}
			if (nodesContainRTF(editableTreeView1.Nodes) && ! VersionAtLeast(version, "2.1"))
			{
				if (MessageBox.Show("Only organizer files version 2.1 and higher support formatted text. Formatting will be lost when saved. Continue?",
					"Organizer", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
					return;
			}
			startTimer();
			if (currentFileVersion.Equals("1.0"))
			{
				saveFileVersion1(filePath);
			}
			else if (currentFileVersion.StartsWith("2"))
			{
				saveFileVersion2(filePath, version);
			}
			showTime("Total time to write file");
			setCurrentFile(filePath, version);
			currentFileSaved = true;
			AddRecentFile(filePath);
		}

		public bool VersionAtLeast(string version, string versionToCompare)
		{
			return Convert.ToSingle(version) >= Convert.ToSingle(versionToCompare);
		}

		//Save File Version 1.0
		public void saveFileVersion1(string filePath)
		{
			if (editableTreeView1.Nodes.Count > 1)
				MessageBox.Show("The old version only supports one root node. Only the first root node will be saved.");
			sb = new StringBuilder();
			setDebugValue("Version", "with sb");
			//saveText = "";
			addNodeToSaveText(editableTreeView1.Nodes[0]);
			//System.IO.File.WriteAllText(filePath, saveText);
			System.IO.File.WriteAllText(filePath, sb.ToString());
		}
		
		//String saveText;
		StringBuilder sb;

		public void addNodeToSaveText(TreeNode root)
		{
			//saveText += "#" + root.FullPath + "\n";
			//saveText += getTreeObject(root).Text;
			sb.Append("#" + root.FullPath + "\n" + GetTreeObject(root).Text);
			foreach (TreeNode node in root.Nodes)
			{
				addNodeToSaveText(node);
			}
		}

		//Save File Version 2.0
		public void saveFileVersion2(string filePath, string version)
		{
			if(VersionAtLeast(version, "2.2"))
			{
				SaveUnsavedSeparatelyStoredNodes(editableTreeView1.Nodes);
			}
			setDebugValue("Buf stream", "true");
			//using (BufferedStream stream = new BufferedStream(File.Open(currentFilePath, FileMode.Create)))
			//{
			using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
				//using (BinaryWriter writer = new BinaryWriter(stream))
				{
					if (version.Equals("2.2"))
						writeNodes2dot2(writer);
					else if (version.Equals("2.1"))
						writeNodes2dot1(writer);
					else
						writeNodes2(writer);
				}
			//}
		}

		public void writeNodes2(BinaryWriter writer)
		{
			writeNodes2(writer, editableTreeView1.Nodes);
		}

		public void writeNodes2(BinaryWriter writer, TreeNodeCollection nodes)
		{
			writer.Write(nodes.Count);
			foreach (TreeNode node in nodes)
			{
				writeNode2(writer, node);
			}
		}

		public void writeNode2(BinaryWriter writer, TreeNode node)
		{
			writer.Write(node.Text);
			TreeObject treeObject = GetTreeObject(node);
			writer.Write(treeObject.Text);
			writer.Write(treeObject.SelectionStart);
			writeNodes2(writer, node.Nodes);
		}

		public void writeNodes2dot1(BinaryWriter writer)
		{
			writer.Write(-1);
			writeNodes2dot1(writer, editableTreeView1.Nodes);
			writer.Write(editableTreeView1.getIndexOfNode(editableTreeView1.SelectedNode));
		}

		public void writeNodes2dot1(BinaryWriter writer, TreeNodeCollection nodes)
		{
			writer.Write(nodes.Count);
			foreach (TreeNode node in nodes)
			{
				writeNode2dot1(writer, node);
			}
		}

		public void writeNode2dot1(BinaryWriter writer, TreeNode node)
		{
			writer.Write(node.Text);
			TreeObject treeObject = GetTreeObject(node);
			writer.Write(treeObject.StoredAsRTF && treeObject.Rtf != null);
			if (treeObject.StoredAsRTF && treeObject.Rtf != null)
				writer.Write(treeObject.Rtf);
			else
				writer.Write(treeObject.Text);
			writer.Write(treeObject.SelectionStart);
			writeNodes2dot1(writer, node.Nodes);
		}

		public void writeNodes2dot2(BinaryWriter writer)
		{
			writer.Write(-2);
			writeNodes2dot2(writer, editableTreeView1.Nodes);
			writer.Write(editableTreeView1.getIndexOfNode(editableTreeView1.SelectedNode));
		}

		public void writeNodes2dot2(BinaryWriter writer, TreeNodeCollection nodes)
		{
			writer.Write(nodes.Count);
			foreach (TreeNode node in nodes)
			{
				writeNode2dot2(writer, node);
			}
		}

		public void writeNode2dot2(BinaryWriter writer, TreeNode node)
		{
			writer.Write(node.Text);
			TreeObject treeObject = GetTreeObject(node);
			writer.Write(treeObject.Id);
			writer.Write(treeObject.StoredSeparate && treeObject.SeparateFilename != null);
			writer.Write(treeObject.StoredAsRTF && treeObject.Rtf != null);
			if (treeObject.StoredSeparate)
				writer.Write(treeObject.SeparateFilename); //Write filename instead.
			else if (treeObject.StoredAsRTF && treeObject.Rtf != null)
				writer.Write(treeObject.Rtf);
			else
				writer.Write(treeObject.Text);
			writer.Write(treeObject.SelectionStart);
			writeNodes2dot2(writer, node.Nodes);
		}

		public TreeNode openFileVersion2(string filePath)
		{
			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
			{
				editableTreeView1.Nodes.Clear();
				if (!startReadNodes(reader, editableTreeView1.Nodes))
					return null;
				if (VersionAtLeast(currentFileVersion, "2.1"))
				{
					return editableTreeView1.getNodeAtIndex(reader.ReadInt32());
				}
				else
				{
					return editableTreeView1.getNodeAtIndex(0);
				}
			}
		}

		public void readNodes(BinaryReader reader, TreeNodeCollection nodes)
		{
			int n = reader.ReadInt32();
			for(int i = 0; i < n; i++)
			{
				nodes.Add(readNode(reader));
			}
		}

		public bool startReadNodes(BinaryReader reader, TreeNodeCollection nodes)
		{
			int n = reader.ReadInt32();
			if (n > 0)
			{
				for (int i = 0; i < n; i++)
				{
					nodes.Add(readNode(reader));
				}
			}
			else if (n == -1) //text is saved as rtf in version 2.1
			{
				currentFileVersion = "2.1";
				setDebugValue("currentFileVersion", currentFileVersion);
				readNodes(reader, nodes);
			}
			else if (n == -2) //text is saved as rtf in version 2.2
			{
				currentFileVersion = "2.2";
				setDebugValue("currentFileVersion", currentFileVersion);
				readNodes(reader, nodes);
			}
			else
			{
				MessageBox.Show("Please download a newer application. This old version cannot open files newer than version 2.2");
				return false;
			}
			return true;
		}

		public TreeNode readNode(BinaryReader reader)
		{
			TreeNode node = new TreeNode(reader.ReadString());
			bool storedSeparately = false;
			bool storedAsRTF = false;
			int id = 0;
			if (VersionAtLeast(currentFileVersion, "2.2"))
			{
				id = reader.ReadInt32();
				storedSeparately = reader.ReadBoolean();
			}
			if (VersionAtLeast(currentFileVersion, "2.1"))
				storedAsRTF = reader.ReadBoolean();
			if (storedSeparately)
			{
				node.Tag = new TreeObject(reader.ReadString(), reader.ReadInt32(), storedAsRTF, storedSeparately, id);
			}
			else
			{
				node.Tag = new TreeObject(reader.ReadString(), reader.ReadInt32(), storedAsRTF, id);
			}
			readNodes(reader, node.Nodes);
			return node;
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (filePathChosen)
			{
				saveFile(currentFilePath, currentFileVersion, true);
			}
			else
			{
				saveAs();
			}
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (confirmCloseOrNew())
				open();
		}

		SaveFileDialog saveFileDialog1 = new SaveFileDialog();

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(confirmCloseOrNew())
				newFile();
		}

		private void newFile()
		{
			currentFileSaved = true;
			filePathChosen = false;
			currentFileName = "Tree1";
			currentFileVersion = "2.1";
			setHeader();
			editableTreeView1.Nodes.Clear();
			TreeNode newNode = new TreeNode("Node1");
			TreeObject treeObject = new TreeObject("Text");
			treeObject.StoredAsRTF = false;
			newNode.Tag = treeObject;
			editableTreeView1.Nodes.Add(newNode);
			lastSelectedNodes = new List<TreeNode>();
			selectedNodeStackIndex = 0;
			editableTreeView1.SelectedNode = newNode;
		}

		private void setHeader()
		{
			this.Text = currentFileName + " - Organizer";
		}

		OpenFileDialog openFileDialog1 = new OpenFileDialog();

		private void openToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			open();
		}

		private void open()
		{
			openFileDialog1.InitialDirectory = "c:\\";
			openFileDialog1.Filter = "Organizer v1.0 (*.txt)|*.txt|Organizer v2.0 (*.hrc)|*.hrc|All files (*.*)|*.*";
			openFileDialog1.FilterIndex = 2;
			openFileDialog1.InitialDirectory = @"E:\Organizer\"; //Application.ExecutablePath;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				openFile(openFileDialog1.FileName);
			}
		}

		private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			if (filePathChosen)
			{
				saveFile(currentFilePath, currentFileVersion, true);
			}
			else
			{
				saveAs();
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveAs();
		}

		private String ExtractFileName(string path)
		{
			int x0 = path.LastIndexOf("\\");
			int x1 = path.LastIndexOf(".");
			return substr(path, x0 + 1, x1 - 1); 
		}

		private string ExtractFilePath(string path)
		{
			int x0 = 0;
			int x1 = path.LastIndexOf("\\");
			return substr(path, x0, x1 - 1);
		}

		private string GetOrgDirectory()
		{
			return ExtractFilePath(currentFilePath);
		}

		private string GetNodeDirectory()
		{
			string orgDirectory = GetOrgDirectory();
			return CombinePath(orgDirectory, currentFileName);
		}

		private string GetNodeFilePath()
		{
			TreeObject ob = GetTreeObject(editableTreeView1.SelectedNode);
			string directory = GetNodeDirectory();
			return CombinePath(directory, ob.SeparateFilename);
		}

		private string GetNodeFilePath(TreeObject ob)
		{
			string directory = GetNodeDirectory();
			return CombinePath(directory, ob.SeparateFilename);
		}

		private string CreateNodeFileName()
		{
			return editableTreeView1.SelectedNode.Text + ".rtf";
		}

		private string CombinePath(string directory, string filename)
		{
			return directory + @"\" + filename;
		}

		private String substr(string s, int x0, int x1)
		{
			return s.Substring(x0, x1 - x0 + 1);
		}

		private void setCurrentFile(string path)
		{
			if (path.EndsWith(".txt"))
			{
				setCurrentFile(path, "1.0");
			}
			if (path.EndsWith(".hrc"))
			{
				setCurrentFile(path, "2.0");
			}
		}

		private void setCurrentFile(string path, string version)
		{
			filePathChosen = true;
			currentFileName = ExtractFileName(path);
			setHeader();
			currentFilePath = path;
			currentFileVersion = version;
			setDebugValue("currentFileVersion", currentFileVersion);
		}

		string currentFileName = null;
		string currentFilePath = null;
		string currentFileVersion = null;
		bool currentFileSaved;
		bool filePathChosen; //True if user has chosen a filepath for the file.

		private void saveAs()
		{
			saveFileDialog1.Filter = "Organizer v1.0 (*.txt)|*.txt|Organizer v2.0 (*.hrc)|*.hrc|Organizer v2.1 (*.hrc)|*.hrc|Organizer v2.2 (*.hrc)|*.hrc|All files (*.*)|*.*";
			saveFileDialog1.FilterIndex = 4;
			saveFileDialog1.InitialDirectory = @"E:\Organizer\"; //Application.ExecutablePath;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string filter = saveFileDialog1.Filter.Split('|')[(saveFileDialog1.FilterIndex - 1) * 2];
				string version = filter.Substring(filter.IndexOf('v') + 1, 3);
				saveFile(saveFileDialog1.FileName, version, false);
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			storeTreeObject();
			if (! confirmCloseOrNew())
			{
				e.Cancel = true;
			}
		}

		//returns false if cancel operation.
		private bool confirmCloseOrNew()
		{
			if (!currentFileSaved)
			{
				// Display a MsgBox asking the user to save changes or abort. 
				DialogResult result = MessageBox.Show("Do you want to save changes to " + currentFileName + "?",
					"Organizer", MessageBoxButtons.YesNoCancel);
				if (result == DialogResult.Yes)
				{
					if (currentFilePath != null)
					{
						saveFile(currentFilePath, currentFileVersion, true);
					}
					else
					{
						saveAs();
					}
				}
				else if (result == DialogResult.Cancel)
				{
					return false;
				}
			}
			Properties.Settings.Default.Save();
			return true;
		}

		private void toolStripButton8_Click(object sender, EventArgs e)
		{
			TreeObject ob = GetTreeObject(editableTreeView1.SelectedNode);
			if (!filePathChosen)
			{
				MessageBox.Show("File must be saved first.");
				return;
			}
			if (ob.StoredSeparate)
			{
				string treeNodeFileLocation = GetNodeFilePath();
				if (MessageBox.Show("Keep " + treeNodeFileLocation + "?", "Keep file", MessageBoxButtons.YesNo)
					== DialogResult.No)
				{
					if (!File.Exists(treeNodeFileLocation))
					{
						MessageBox.Show("File not found.");
						return;
					}
					else
					{
						File.Delete(treeNodeFileLocation);
					}
				}
				ob.StoredSeparate = false;
			}
			else
			{
				if (!Directory.Exists(GetOrgDirectory()))
				{
					MessageBox.Show("Directory not found.");
					return;
				}
				else
				{
					string treeNodeFileName = CreateNodeFileName();
					string treeNodeFileLocation = CombinePath(GetNodeDirectory(), treeNodeFileName);
					if (!Directory.Exists(GetNodeDirectory()))
					{
						Directory.CreateDirectory(GetNodeDirectory());
					}
					else if (File.Exists(treeNodeFileLocation) &&
						MessageBox.Show(treeNodeFileLocation + " already exists. Overwrite?", "Overwrite file", MessageBoxButtons.YesNo)
						== DialogResult.No)
					{
						return;
					}
					ob.SeparateFilename = treeNodeFileName;
					ob.StoredSeparate = true;
					if (!ob.StoredAsRTF)
					{
						ob.Rtf = richTextBoxEx1.Rtf;
						ob.StoredAsRTF = true;
					}
					WriteTreeObjectRtf(ob);
				}
			}
			updateSepIcon();
			//richTextBoxEx1.DoubleAction();
			//if (richTextBoxEx1.SelectionLength > 0)
			//	richTextBoxEx1.SelectionColor = Color.Black;
			//else
			//	richTextBoxEx1.ForeColor = Color.Blue;
		}

		public void WriteTreeObjectRtf(TreeObject ob)
		{
			if (!ob.StoredAsRTF || !ob.StoredSeparate)
				throw new Exception("Not stored as rtf or not stored separately");
			ob.Saved = true;
			string treeNodeFileLocation = GetNodeFilePath(ob);
			System.IO.File.WriteAllText(treeNodeFileLocation, ob.Rtf);
		}

		public void ReadTreeObjectRtf(TreeObject ob)
		{
			string treeNodeFileLocation = GetNodeFilePath(ob);
			try
			{
				ob.Rtf = System.IO.File.ReadAllText(treeNodeFileLocation);
			}
			catch(Exception ex)
			{
				ob.Rtf = @"{\rtf1\ansi\ Error reading file}";
			}
		}

		private void toolStripButton6_Click(object sender, EventArgs e)
		{
			if (confirmCloseOrNew())
				newFile();
		}

		private void toolStripButton7_Click(object sender, EventArgs e)
		{
			if (confirmCloseOrNew())
				open();
		}

		private void toolStripButton5_Click(object sender, EventArgs e)
		{
			if (filePathChosen)
			{
				saveFile(currentFilePath, currentFileVersion, true);
			}
			else
			{
				saveAs();
			}
		}

		#endregion

		#region Toolbar Formatting Shortcuts
		
		private void pictureBox1_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionFontBold = !richTextBoxEx1.SelectionFontBold;
		}

		private void pictureBox2_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionFontItalic = !richTextBoxEx1.SelectionFontItalic;
		}

		private void pictureBox3_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionFontUnderline = !richTextBoxEx1.SelectionFontUnderline;
		}

		private void pictureBox4_Click(object sender, EventArgs e)
		{
			if (richTextBoxEx1.SelectionLength > 0)
			{
				richTextBoxEx1.SelectionFont = preferredFont;
				richTextBoxEx1.SelectionColor = preferredFontColor;
			}
			else
			{
				richTextBoxEx1.setFont(preferredFont, preferredFontColor);
			}
		}

		private void toolStripButton12_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionUnderlineColor = RichTextBoxEx.UnderlineColor.Red;
		}

		private void toolStripButton13_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionUnderlineColor = RichTextBoxEx.UnderlineColor.Magenta;
		}

		private void toolStripButton14_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionUnderlineColor = RichTextBoxEx.UnderlineColor.Yellow;
		}

		private void toolStripButton15_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionUnderlineColor = RichTextBoxEx.UnderlineColor.Green;
		}

		private void toolStripButton16_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionUnderlineColor = RichTextBoxEx.UnderlineColor.Blue;
		}

		private void toolStripButton17_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectionUnderlineColor = RichTextBoxEx.UnderlineColor.Cyan;
		}

		#endregion
		
		private void updateSepIcon()
		{
			bool sep = GetTreeObject(editableTreeView1.SelectedNode).StoredSeparate;
			toolStripButton8.Checked = sep;
			toolStripButton8.ToolTipText = sep ? "Stored separately" : "Save separately";
		}

		private void pasteSpecialToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBoxEx1.SelectedText = Clipboard.GetText(TextDataFormat.Text);
		}

		int linkStart;
		int linkLength;
		HyperlinkDialog dialog;

		private void linkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dialog = new HyperlinkDialog();
			dialog.TextToDisplay = richTextBoxEx1.SelectedText;
			linkStart = richTextBoxEx1.SelectionStart;
			linkLength = richTextBoxEx1.SelectionLength;
			dialog.Show();
			dialog.FormClosing += new FormClosingEventHandler(dialog_FormClosing);
		}

		private void dialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (dialog.Result == DialogResult.OK)
			{
				richTextBoxEx1.InsertLink(dialog.TextToDisplay, dialog.LinkDestination);
			}
		}

		private void viewToolbarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			toolStrip1.Visible = !toolStrip1.Visible;
			if (toolStrip1.Visible)
			{
				splitContainer1.Top += 25;
				splitContainer1.Height -= 25;
				viewToolbarToolStripMenuItem.Text = "Hide Toolbar";
			}
			else
			{
				splitContainer1.Top -= 25;
				splitContainer1.Height += 25;
				viewToolbarToolStripMenuItem.Text = "Show Toolbar";
			}
		}

		private void richTextBoxEx1_OnSubLinkClicked(TreeNode nodeToLinkTo, string text)
		{
			if (nodeToLinkTo != null)
			{
				Application.DoEvents();
				richTextBoxEx1.SuspendDrawing();
				editableTreeView1.SelectedNode = nodeToLinkTo;
				if (text.Contains(":"))
				{
					int verse = Convert.ToInt32(text.Substring(text.LastIndexOf(':') + 1));
					string txt = richTextBoxEx1.Text;
					int position = 0;
					for (int i = 0; i < txt.Length; i++)
					{
						if (txt[i] == '\n')
						{
							verse--;
							if(verse == 0)
							{
								position = i;
								break;
							}
						}
					}
					try
					{
						richTextBoxEx1.Select(position, 0);
						richTextBoxEx1.ScrollToCaret();
					}
					catch (Exception ex)
					{
						//Some sort of asyncronous exception happens here if this has been called inside a RTB event handler.
						//We'll just ignore it for now.
					}
				}
				richTextBoxEx1.ResumeDrawing();
			}
		}

		private void richTextBoxEx1_LinkClicked(object sender, LinkClickedEventArgs e)
		{

		}

		private void copyRtfTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (richTextBoxEx1.SelectionLength > 0)
				Clipboard.SetText(richTextBoxEx1.SelectedRtf);
			else
				Clipboard.SetText(richTextBoxEx1.Rtf);

		}

		private void editableTreeView1_OnDragTextToNode(TreeNode destinationNode, string text)
		{
			richTextBoxEx1.SelectionFontLang = GetTreeObject(destinationNode).Id;
			//richTextBoxEx1.InsertLink(richTextBoxEx1.SelectedText, destinationNode.Text);
			treeObject = GetTreeObject(destinationNode);
			if (treeObject.StoredAsRTF)
			{
				
				string textToAppend = @"\lang" + (GetTreeObject(editableTreeView1.SelectedNode).Id + 5000) + " " + 
					GetReference() + @"\lang1033 " + "\t" + text;
				string newrtf = appendTextToRtf(treeObject.Rtf, textToAppend);
				treeObject.Rtf = newrtf;
			}
			else
			{
				treeObject.Text = treeObject.Text + "\n" + text;
			}
		}

		private string GetReference()
		{
			string title = editableTreeView1.SelectedNode.Text;
			TreeNode ss = editableTreeView1.findChildByName("Scriptures");
			if(editableTreeView1.isDescendant(ss, editableTreeView1.SelectedNode))
			{
				string substr = richTextBoxEx1.Text.Substring(0, richTextBoxEx1.SelectionStart);
				int verse = 1;
				for(int i = 0; i < substr.Length; i++)
				{
					if(substr[i] == '\n')
						verse++;
				}
				return editableTreeView1.SelectedNode.Text + ":" + verse;
			}
			else
			{
				return editableTreeView1.SelectedNode.Text;
			}
		}

		private string appendTextToRtf(string rtf, string text)
		{
			return rtf.Insert(rtf.LastIndexOf("}"), text + @"\par"); 
		}

		private void isRTFToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (richTextBoxEx1.ContainsRtf(preferredFont, preferredFontColor))
				MessageBox.Show("Contains rtf");
		}






		int selectedNodeStackIndex;
		bool navigationEvent;

		private void toolStripButton18_Click(object sender, EventArgs e)
		{
			selectedNodeStackIndex--;
			if (selectedNodeStackIndex >= 0)
			{
				navigationEvent = true;
				editableTreeView1.SelectedNode = lastSelectedNodes[selectedNodeStackIndex];
				navigationEvent = false;
				UpdateNodeList();
			}
			else
			{
				selectedNodeStackIndex = 0;
			}

			/*for (int i = 0; i < 16; i++)
			{
				//Color c = ((Bitmap)toolStripButton17.Image).GetPixel(0, i);
				//richTextBoxEx1.SelectedText = "\r\nColor.FromArgb(" + c.R + ", " + c.G + ", " + c.B + "),";
			}

			for (int i = 0; i < 16; i++)
			{
				richTextBoxEx1.Select(i, 1);
				richTextBoxEx1.SelectionUnderlineColorAndStyle = (byte)((byte)RichTextBoxEx.UnderlineStyle.Thick | (i * 16));
				//richTextBoxEx1.SelectionUnderlineColorAndStyle = (byte)((i) | (byte)RichTextBoxEx.UnderlineColor.Red);
			}*/
		}

		private void toolStripButton19_Click(object sender, EventArgs e)
		{
			selectedNodeStackIndex++;
			if (selectedNodeStackIndex < lastSelectedNodes.Count)
			{
				navigationEvent = true;
				editableTreeView1.SelectedNode = lastSelectedNodes[selectedNodeStackIndex];
				navigationEvent = false;
				UpdateNodeList();
			}
			else
			{
				selectedNodeStackIndex = lastSelectedNodes.Count - 1;
			}
			//richTextBoxEx1.SelectionFontVisible = !richTextBoxEx1.SelectionFontVisible;
		}

		private void richTextBoxEx1_SelectionChanged(object sender, EventArgs e)
		{
			if(charRtb != null)
			{
				updateCharFormatRtb();
			}
		}

		private void UpdateNodeList()
		{
			string text = "";
			for (int i = 0; i < lastSelectedNodes.Count; i++)
			{
				text += ((i == selectedNodeStackIndex) ? " +" : " ") + GetTreeObject(lastSelectedNodes[i]).Id;
			}
			setDebugValue("Node Stack", text);
		}


		private void richTextBoxEx1_TextChanged(object sender, EventArgs e)
		{
			if (specialRtb != null && !modifyingText)
			{
				modifyingText = true;
				specialRtb.Text = richTextBoxEx1.Rtf;
				modifyingText = false;
			}
			if (undoRtb != null)
			{
				refreshUndoRtb();
			}
		}

		private void richTextBoxEx1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control)
			{
				if (e.KeyCode == Keys.B)
				{
					richTextBoxEx1.SelectionFontBold = !richTextBoxEx1.SelectionFontBold;
				}
				if (e.KeyCode == Keys.I)
				{
					richTextBoxEx1.SelectionFontItalic = !richTextBoxEx1.SelectionFontItalic;
					e.SuppressKeyPress = true; //Normally a tab is inserted on Ctrl-I
				}
				if (e.KeyCode == Keys.U)
				{
					richTextBoxEx1.SelectionFontUnderline = !richTextBoxEx1.SelectionFontUnderline;
				}
				if (e.KeyCode == Keys.C)
				{
					//Clipboard.Clear();
					DataObject data = new DataObject();
					data.SetData(DataFormats.Text, richTextBoxEx1.SelectedText);
					data.SetData(DataFormats.Rtf, richTextBoxEx1.SelectedRtf);
					Clipboard.SetDataObject(data);
					//Clipboard.SetText(richTextBoxEx1.SelectedRtf, TextDataFormat.Rtf);
					e.SuppressKeyPress = true;
				}
				if (e.KeyCode == Keys.V)
				{
					if (Clipboard.GetText(TextDataFormat.Rtf) != "")
						richTextBoxEx1.SelectedRtf = Clipboard.GetText(TextDataFormat.Rtf);
					else
						richTextBoxEx1.SelectedText = Clipboard.GetText();
					e.SuppressKeyPress = true;
				}
				if (e.KeyCode == Keys.Z)
				{
					//Undo all "unknown" actions together, as a single undo action.
					//while (richTextBoxEx1.CanUndo && richTextBoxEx1.UndoActionName.Equals("Unknown"))
					//{
					//	richTextBoxEx1.UndoActionName
					//	richTextBoxEx1.Undo();
					//	e.SuppressKeyPress = true; //Don't undo more than this group.
					//}

					richTextBoxEx1.CustomUndo();
					e.SuppressKeyPress = true;
					if (undoRtb != null)
					{
						refreshUndoRtb();
					}
				}
				if (e.KeyCode == Keys.Y)
				{
					//Redo all "unknown" actions together, as a single undo action.
					//while (richTextBoxEx1.CanRedo && richTextBoxEx1.RedoActionName.Equals("Unknown"))
					//{
					//	richTextBoxEx1.Redo();
					//	e.SuppressKeyPress = true; //Don't undo more than this group.
					//}

					richTextBoxEx1.CustomRedo();
					e.SuppressKeyPress = true;
					if (undoRtb != null)
					{
						refreshUndoRtb();
					}
				}
			}
		}

		private void toolStripButton21_Click(object sender, EventArgs e)
		{
			if (specialRtb.SelectionLength > 0)
			{
				Clipboard.SetText(specialRtb.Text, TextDataFormat.Rtf);
			}
			else
			{
				specialRtb.Text = Clipboard.GetText(TextDataFormat.Rtf);
			}
		}

		private void toolStripButton22_Click(object sender, EventArgs e)
		{
			int selStart = specialRtb.SelectionStart;
			TextParser parser = new TextParser(specialRtb.Text);
			parser.GoToTextPosition(richTextBoxEx1.SelectionStart, true);
			while (parser.itemType == ItemParser.ItemType.Tag)
			{
				parser.RemoveTag();
			}
			specialRtb.Text = parser.text;
			specialRtb.Select(selStart, 0);
		}

		private void toolStripButton23_Click(object sender, EventArgs e)
		{
			startTimer();

			Random r = new Random();
			for (int i = 0; i < 1000; i++) //2 ms / action
			{
				//getSelectionUnderlineColorAndStyle
			}
			return;

			for (int i = 0; i < 1000; i++) //2 ms / action
			{
				int st = r.Next(0, richTextBoxEx1.TextLength);
				int ln = r.Next(1, richTextBoxEx1.TextLength - st);
				int fn = r.Next(0, 3);
				bool bl = (r.Next(0, 2) == 1);
				richTextBoxEx1.Select(st, ln);
				switch(fn)
				{
					case 0:
						richTextBoxEx1.SelectionFontBold = bl;
						break;
					case 1:
						richTextBoxEx1.SelectionFontItalic = bl;
						break;
					case 2:
						richTextBoxEx1.SelectionFontUnderline = bl;
						break;
				}
			}

			//Selection: 2109 ms
			//Behind scenes: 734 ms (3 times faster)
			//Parser: 171 ms (20 times faster)


			showTime("Selection");
			richTextBoxEx1.SuspendDrawing();
			for (int i = 0; i < 1000; i++) //2 ms / action
			{
				int st = r.Next(0, richTextBoxEx1.TextLength);
				int ln = r.Next(1, richTextBoxEx1.TextLength - st);
				int fn = r.Next(0, 3);
				bool bl = (r.Next(0, 2) == 1);
				richTextBoxEx1.Select(st, ln);
				switch (fn)
				{
					case 0:
						richTextBoxEx1.SelectionFontBold = bl;
						break;
					case 1:
						richTextBoxEx1.SelectionFontItalic = bl;
						break;
					case 2:
						richTextBoxEx1.SelectionFontUnderline = bl;
						break;
				}
			}
			richTextBoxEx1.ResumeDrawing();
			showTime("Behind scenes");
			string rtf = richTextBoxEx1.Rtf;
			for (int i = 0; i < 1000; i++) //2 ms / action
			{
				int st = r.Next(0, richTextBoxEx1.TextLength);
				int ln = r.Next(1, richTextBoxEx1.TextLength - st);
				int fn = r.Next(0, 3);
				bool bl = (r.Next(0, 2) == 1);
				rtf = FontParser.SetProperty(rtf, (fn == 0) ? "b" : ((fn == 1) ? "i" : "ul"), bl);
			}
			richTextBoxEx1.Rtf = rtf;
			showTime("Parser");
		}

		RichTextBoxEx specialRtb;

		private void viewRTFInSeparateTextboxToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (specialRtb == null)
			{
				specialRtb = new RichTextBoxEx();
				AddControl(specialRtb);
				specialRtb.TextChanged += new System.EventHandler(this.specialRtb_TextChanged);
			}
			else
			{
				RemoveControl(specialRtb);
				specialRtb = null;
			}
		}

		#region PanelOperations

		private void AddControl(Control newControl)
		{
			SplitPanel((Panel)richTextBoxEx1.Parent).Controls.Add(newControl);
			newControl.Dock = DockStyle.Fill;
		}

		private void RemoveControl(Control oldControl)
		{
			RemovePanel((Panel)oldControl.Parent);
		}

		private Panel SplitPanel(Panel panel)
		{
			Control originalControl = panel.Controls[0];
			SplitContainer splitContainer = new SplitContainer();
			panel.Controls.Clear();
			panel.Controls.Add(splitContainer);
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Panel1.Controls.Add(originalControl);
			return splitContainer.Panel2;
		}

		private void RemovePanel(Panel panel)
		{
			if (panel.Parent is SplitContainer)
			{
				SplitContainer splitContainer = (SplitContainer)panel.Parent;
				Panel oppositePanel = (splitContainer.Panel1 == panel) ? splitContainer.Panel2 : splitContainer.Panel1;
				Control parentControl = splitContainer.Parent;
				parentControl.Controls.Clear();
				parentControl.Controls.Add(oppositePanel.Controls[0]);
			}
			else
			{
				throw new Exception("Parent is not a SplitContainer");
			}
		}

		#endregion

		private void toolStripButton24_Click(object sender, EventArgs e)
		{
			string[] list = richTextBoxEx1.Text.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
			TreeNode parent = editableTreeView1.SelectedNode;
			foreach (string item in list)
			{
				int newLineLoc = item.IndexOf('\n');
				string firstLine = (newLineLoc > 0) ? item.Substring(0, newLineLoc) : item;
				TreeNode child = new TreeNode();
				child.Text = firstLine;
				child.Tag = new TreeObject(item);
				parent.Nodes.Add(child);
			}
		}

		private void toolStripButton24_c_Click(object sender, EventArgs e)
		{
			TreeNode parent = editableTreeView1.SelectedNode;
			string text = "";
			List<string> list = new List<string>();
			foreach (TreeNode child in parent.Nodes)
			{
				TreeObject obj = GetTreeObject(child);

				string item = (obj.Text.Length == 0) ? obj.Rtf : obj.Text;
				int newLineLoc = item.IndexOf('\n');
				string firstLine = (newLineLoc > 0) ? item.Substring(0, newLineLoc) : item;
				if(firstLine.StartsWith(child.Text))
					text += ((text.Length > 0) ? "\n\n" : "") + item;
				else
					text += ((text.Length > 0) ? "\n\n" : "") + firstLine + "\n" + item;
			}
			richTextBoxEx1.AppendText(text);
		}

		Font preferredFont;
		Color preferredFontColor;

		#region ValidateFontSize

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateFontName();
		}

		bool ComboBox1TextChanged;

		private void toolStripComboBox1_Leave(object sender, EventArgs e)
		{
			if(ComboBox1TextChanged)
				ValidateFontName();
		}

		private void toolStripComboBox1_TextChanged(object sender, EventArgs e)
		{
			ComboBox1TextChanged = true;
		}

		private void toolStripComboBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.SuppressKeyPress = true;
				richTextBoxEx1.Focus();
			}
		}

		private void ValidateFontName()
		{
			ComboBox1TextChanged = false;
			try
			{
				string fontName = toolStripComboBox1.Text;
				if (!toolStripComboBox1.Items.Contains(fontName))
				{
					MessageBox.Show("The font does not exist.");
					return;
				}
				if (richTextBoxEx1.SelectionLength > 0)
				{
					richTextBoxEx1.SelectionFontName = fontName;
					//richTextBoxEx1.SelectionFont = new Font(getFont(toolStripComboBox1.Text), float.Parse(toolStripComboBox2.Text));
				}
				else
				{
					richTextBoxEx1.FontName = fontName;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateFontSize();
		}

		bool ComboBox2TextChanged;

		private void toolStripComboBox2_Leave(object sender, EventArgs e)
		{
			if (ComboBox2TextChanged)
				ValidateFontSize();
		}

		private void toolStripComboBox2_TextChanged(object sender, EventArgs e)
		{
			ComboBox2TextChanged = true;
		}

		private void toolStripComboBox2_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.SuppressKeyPress = true;
				richTextBoxEx1.Focus();
			}
		}

		private void ValidateFontSize()
		{
			ComboBox2TextChanged = false;
			try
			{
				float fontSize = Convert.ToSingle(toolStripComboBox2.Text);
				if (fontSize < 1 || fontSize > 1000)
				{
					MessageBox.Show("Invalid entry. Number must be between 1 and 1000");
					return;
				}
				if (richTextBoxEx1.SelectionLength > 0)
				{
					richTextBoxEx1.SelectionFontSize = fontSize;
					//richTextBoxEx1.SelectionFont = new Font(getFont(toolStripComboBox1.Text), float.Parse(toolStripComboBox2.Text));
				}
				else
				{
					preferredFont = new Font(getFont(toolStripComboBox1.Text), float.Parse(toolStripComboBox2.Text));
					richTextBoxEx1.Font = preferredFont;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Invalid entry. Not a number.");
			}
		}

		#endregion

		#region DebugRtbs

		//	{NotYetAssigned,                             Brace,                        Tag,    NewlineCharacter, 
		Color[] fontColors = new Color[] {Color.Black, Color.Magenta, Color.Magenta, Color.Blue, Color.Black, 
		//   EscapeSequence, HexCharacter, TagStop,        Text}
			Color.Red, Color.Red, Color.Black, Color.Black, Color.LightGreen, Color.DarkGreen};

		bool modifyingText = false;
		private void specialRtb_TextChanged(object sender, EventArgs e)
		{
			int selStart = specialRtb.SelectionStart;
			TextParser parser = new TextParser(specialRtb.Text);
			int i = 0;
			while (parser.GoToNextItem())
			{
				i++;
				specialRtb.Select(parser.itemStartIndex, parser.itemLength);
				int colorIndex = ((int)parser.itemType);
				if (parser.itemTextLength > 0)
					colorIndex = fontColors.Length - 2;
				specialRtb.SelectionColor = fontColors[colorIndex];
			}

			specialRtb.Select(selStart, 0);
			if (!modifyingText)
			{
				modifyingText = true;
				try
				{
					richTextBoxEx1.Rtf = specialRtb.Text;
					specialRtb.BackColor = Color.White;
				}
				catch (Exception ex)
				{
					specialRtb.BackColor = Color.Red;
				}
				modifyingText = false;
			}
		}

		RichTextBox debugRtb;

		private void debugWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (debugRtb == null)
			{
				debugRtb = new RichTextBoxEx();
				AddControl(debugRtb);
				viewDebugWindowToolStripMenuItem.Text = "Hide Debug Window";
			}
			else
			{
				RemoveControl(debugRtb);
				debugRtb = null;
				viewDebugWindowToolStripMenuItem.Text = "Show Debug Window";
			}
		}

		public void refreshUndoRtb()
		{
			undoRtb.Text = "Undo Stack\n" + PrintStack(richTextBoxEx1.CustomUndoStack)
				+ "\n\nRedo Stack\n" + PrintStack(richTextBoxEx1.CustomRedoStack);
		}

		public string PrintStack(Stack<RichTextBoxEx.CustomAction> stack)
		{
			string s = "st end ct";
			foreach (RichTextBoxEx.CustomAction action in stack)
			{
				s = s + "\n" + action.selStart + " " + action.selEnd + " " + action.count;
			}
			return s;
		}

		RichTextBoxEx undoRtb;

		private void viewUndoStackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (undoRtb == null)
			{
				undoRtb = new RichTextBoxEx();
				AddControl(undoRtb);
			}
			else
			{
				RemoveControl(undoRtb);
				undoRtb = null;
			}
		}

		private void addSpecialEntryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			specialEntries.Add(richTextBoxEx1.SelectedText);
			if (characterToolStrip == null)
			{
				ShowCharacterToolStrip();
			}
			UpdateCharacterToolStrip();
		}

		RichTextBoxEx sizeRtb;

		private void viewSizeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (sizeRtb == null)
			{
				sizeRtb = new RichTextBoxEx();
				AddControl(sizeRtb);
				updateSizes();
			}
			else
			{
				RemoveControl(sizeRtb);
				sizeRtb = null;
			}
		}

		string sizeResult;

		public void updateSizes()
		{
			sizeResult = "";
			updateSizes(editableTreeView1.Nodes);
			sizeRtb.Text = sizeResult;
		}

		public void updateSizes(TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				updateSizes(node);
			}
		}

		public void updateSizes(TreeNode node)
		{
			sizeResult += "\r\n" + node.Text + "\t" + getSize(GetTreeObject(node));
			updateSizes(node.Nodes);
		}

		public int getSize(TreeObject ob)
		{
			return ob.StoredAsRTF ? ob.Rtf.Length : ob.Text.Length;
		}

		RichTextBoxEx charRtb;

		private void viewCharFormatToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (charRtb == null)
			{
				charRtb = new RichTextBoxEx();
				AddControl(charRtb);
				updateCharFormatRtb();
			}
			else
			{
				RemoveControl(charRtb);
				charRtb = null;
			}
		}

		private void updateCharFormatRtb()
		{
			charRtb.Text = richTextBoxEx1.getRepOfCharFmt();
		}

		#endregion

		FindDialog findDialog;

		private void findToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (findDialog == null || findDialog.IsDisposed)
				findDialog = new FindDialog();
			findDialog.Show();
		}
	}
}