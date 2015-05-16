using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Organizer
{
	public partial class EditableTreeView : TreeView
	{
		public EditableTreeView()
		{
			InitializeComponent();
			//SetStyle(ControlStyles.UserPaint, true);
			//Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.ResizeRedraw, True)
			g = CreateGraphics();
		}

		Graphics g;

		#region [TREEVIEW DRAG AND DROP FUNCTIONALITY]

		private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
			//e.Effect = ... (We aren't setting the effect here because we want more specific drag information)
        }

		DateTime dragDestinationTimeSelected;
		long hoverTimeToExpand = 800; //(In milliseconds)
		int placeHolderHotspotWidth = 4;

		private void treeView1_DragOver(object sender, DragEventArgs e)
		{
			Point pt = PointToClient(new Point(e.X, e.Y));

			//Set to move or copy, based on keys pressed.
			if (e.Data.GetDataPresent(DataFormats.Text))
			{
				//1L 2R 4Sh 8Ctrl 16Middle 32Alt
				if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
				{
					e.Effect = DragDropEffects.Copy;
				}
				else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
				{
					e.Effect = DragDropEffects.Move;
				}
			}

			TreeNode nodeAt = GetNodeAt(pt);
			TreeNode nodeAbove = GetNodeAt(pt.X, pt.Y - placeHolderHotspotWidth);
			TreeNode nodeBelow = GetNodeAt(pt.X, pt.Y + placeHolderHotspotWidth);
			if (nodeBelow == null) //At end of list
			{
				selectDragDestinationNode(null);
				selectDragDestinationAboveNode(null);
				selectDragDestinationBelowNode(getLastVisibleNodeLessThanX(pt.X));
			}
			else if (nodeAbove == null) //At beggining of list
			{
				selectDragDestinationNode(null);
				selectDragDestinationBelowNode(null);
				selectDragDestinationAboveNode(getFirstNode());
			}
			else if (nodeAbove != nodeBelow) //Within placeholder hotspot
			{
				selectDragDestinationNode(null);

				//Do we have a choice about which level to insert the node at?
				if (nodeAbove.Level > nodeBelow.Level)
				{
					int threshold = nodeAbove.Bounds.Left;
					if (pt.X > threshold)
					{
						selectDragDestinationAboveNode(null);
						selectDragDestinationBelowNode(nodeAbove);
					}
					else
					{
						selectDragDestinationBelowNode(null);
						selectDragDestinationAboveNode(nodeBelow);
					}
				}
				else
				{
					selectDragDestinationAboveNode(nodeBelow);
				}
			}
			else
			{
				TreeNode newDragDestination = nodeAt;
				// Is mouse over a different node than last iteration? If so, update.
				if (newDragDestination != dragDestinationNode)
				{
					if (isDescendant(dragSourceNode, newDragDestination) && e.Effect == DragDropEffects.Move)
					{
						e.Effect = DragDropEffects.None;
						newDragDestination = null; //Make sure we don't drag anything to this location! (though we could do a swap)
					}
					selectDragDestinationNode(newDragDestination);
					selectDragDestinationAboveNode(null);
					selectDragDestinationBelowNode(null);

					// if we are on a new object, reset our timer
					dragDestinationTimeSelected = DateTime.Now;
				}
				else if (dragDestinationNode != null)
				{
					// otherwise check to see if enough time has passed and expand the destination node
					TimeSpan hoverTime = DateTime.Now.Subtract(dragDestinationTimeSelected);
					if (hoverTime.Ticks > hoverTimeToExpand * 10000)
					{
						dragDestinationNode.Expand();
					}
				}
			}
		}

		TreeNode dragDestinationNode;

		private void selectDragDestinationNode(TreeNode node)
		{
			if (dragDestinationNode != null)
			{
				dragDestinationNode.NodeFont = Font;
				dragDestinationNode.ForeColor = Color.Black;
			}
			dragDestinationNode = node;
			if (dragDestinationNode != null)
			{
				dragDestinationNode.NodeFont = new Font(Font, FontStyle.Bold);
				dragDestinationNode.Text = dragDestinationNode.Text; //Refresh text so it doesn't get cut off.
			}
		}

		TreeNode dragDestinationAboveNode;

		private void selectDragDestinationAboveNode(TreeNode node)
		{
			if (node != dragDestinationAboveNode)
			{
				if (dragDestinationAboveNode != null)
				{
					Invalidate(); //Redraw the treeview to erase the last-drawn placeholder.
				}
				dragDestinationAboveNode = node;
				if (dragDestinationAboveNode != null)
				{
					drawPlaceHolderAbove(dragDestinationAboveNode);
				}
			}
		}

		TreeNode dragDestinationBelowNode;

		private void selectDragDestinationBelowNode(TreeNode node)
		{
			if (node != dragDestinationBelowNode)
			{
				if (dragDestinationBelowNode != null)
				{
					Invalidate();
				}
				dragDestinationBelowNode = node;
				if (dragDestinationBelowNode != null)
				{
					drawPlaceHolderBelow(dragDestinationBelowNode);
				}
			}
		}

		private void drawPlaceHolderBelow(TreeNode node)
		{
			drawPlaceHolder(node.Bounds.Left, node.Bounds.Right, getLastVisibleNode(node).Bounds.Bottom);
		}

		private void drawPlaceHolderAbove(TreeNode node)
		{
			drawPlaceHolder(node.Bounds.Left, node.Bounds.Right, node.Bounds.Top);
		}

		private void drawPlaceHolder(int left, int right, int y)
		{
			//This will let the application repaint the form first so it doesn't erase what we're doing.
			Application.DoEvents(); 

			g.DrawLine(Pens.Black, left, y, right, y);
			drawTriangle(Pens.Black, left, y, -2, 2);
			drawTriangle(Pens.Black, right, y, 2, 2);
		}

		private void drawTriangle(Pen pen, int x, int y, int dx, int dy)
		{
			g.DrawLines(pen, new Point[] { new Point(x + dx, y - dy), new Point(x, y), new Point(x + dx, y + dy) });
		}

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
			//if(dragDestinationNode == dragSourceNode)
			//{
			//	return;
			//	//do nothing
			//}
			/*if (isDescendant(dragSourceNode, dragDestinationNode) && e.Effect == DragDropEffects.Move)
			{
				//Obviously we can't drag the parent node to its child, but we could swap them.
				int destIndex = dragDestinationNode.Index;
				int srcIndex = dragSourceNode.Index;
				TreeNodeCollection parentCollection = getSiblings(dragSourceNode);

				dragDestinationNode.Remove();
				while(dragSourceNode.Nodes.Count > 0)
				{
					TreeNode node = dragSourceNode.Nodes[0];
					node.Remove();
					dragDestinationNode.Nodes.Add(node);
				}
				dragSourceNode.Remove();

				dragDestinationNode.Nodes.Insert(destIndex, dragSourceNode);
				parentCollection.Insert(srcIndex, dragDestinationNode);
			}*/


			TreeNode destination = dragDestinationNode;
			selectDragDestinationNode(null);
			if (dragSourceNode == null)
			{
				if (destination != null)
				{
					if (onDragTextToNode != null)
						onDragTextToNode(destination, e.Data.GetData(DataFormats.Text).ToString());
				}
				else
				{
					dragSourceNode = new TreeNode(e.Data.GetData(DataFormats.Text).ToString());
				}
			}
			if (dragSourceNode != null)
			{
				TreeNodeCollection aboveSiblings = getSiblings(dragDestinationAboveNode);
				TreeNodeCollection belowSiblings = getSiblings(dragDestinationBelowNode);
				if (e.Effect == DragDropEffects.Copy)
				{
					dragSourceNode = (TreeNode)dragSourceNode.Clone();
				}
				else if (e.Effect == DragDropEffects.Move)
				{
					dragSourceNode.Remove();
				}

				if (dragDestinationAboveNode != null)
				{
					aboveSiblings.Insert(dragDestinationAboveNode.Index, dragSourceNode);
				}
				else if (dragDestinationBelowNode != null)
				{
					belowSiblings.Insert(dragDestinationBelowNode.Index + 1, dragSourceNode);
				}
				else if (destination != null)
				{
					destination.Nodes.Add(dragSourceNode);
					destination.Expand();
				}
				else
				{
					Nodes.Add(dragSourceNode);
				}
			}

		}

		private TreeNodeCollection getSiblings(TreeNode node)
		{
			if (node == null)
			{
				return null;
			}
			else if (node.Parent == null)
			{
				return Nodes;
			}
			else
			{
				return node.Parent.Nodes;
			}
		}

		private TreeNode getFirstNode()
		{
			return Nodes[0];
		}

		private TreeNode getLastVisibleNode()
		{
			return getLastVisibleNode(Nodes);
		}

		private TreeNode getLastVisibleNode(TreeNode node)
		{
			if (node.IsExpanded && node.Nodes.Count > 0)
			{
				return getLastVisibleNode(node.Nodes);
			}
			else
			{
				return node;
			}
		}

		private TreeNode getLastVisibleNode(TreeNodeCollection nodes)
		{
			return getLastVisibleNode(nodes[nodes.Count - 1]);
		}

		private TreeNode getLastVisibleNodeLessThanX(int x)
		{
			return getLastVisibleNodeLessThanX(Nodes, x);
		}

		private TreeNode getLastVisibleNodeLessThanX(TreeNode node, int x)
		{
			if (node.IsExpanded && node.Nodes.Count > 0 && x > node.Nodes[node.Nodes.Count - 1].Bounds.Left)
			{
				return getLastVisibleNodeLessThanX(node.Nodes, x);
			}
			else
			{
				return node;
			}
		}

		private TreeNode getLastVisibleNodeLessThanX(TreeNodeCollection nodes, int x)
		{
			return getLastVisibleNodeLessThanX(nodes[nodes.Count - 1], x);
		}

		public int getChildCount(TreeNode node)
		{
			return getChildCount(node.Nodes);
		}

		public int getChildCount(TreeNodeCollection nodes)
		{
			int count = nodes.Count;
			foreach(TreeNode node in nodes)
			{
				count += getChildCount(node);
			}
			return count;
		}


		public TreeNode getNodeAtIndex(int index)
		{
			return getNodeAtIndex(Nodes, index);
		}

		public TreeNode getNodeAtIndex(TreeNodeCollection nodes, int index)
		{
			foreach (TreeNode node in nodes)
			{
				if (index == 0)
					return node;

				index--;
				int count = getChildCount(node);
				if (index < count)
				{
					return getNodeAtIndex(node.Nodes, index);
				}
				else
				{
					index -= count;
				}
			}
			throw new IndexOutOfRangeException("Attempted to access element " + index + ". There are only " + getChildCount(nodes) + " elements.");
		}

		public int getIndexOfNode(TreeNode node)
		{
			return getIndexOfNode(Nodes, node);
		}

		public int getIndexOfNode(TreeNodeCollection nodes, TreeNode nodeToFind)
		{
			int index = 0;
			foreach (TreeNode node in nodes)
			{
				if (node == nodeToFind)
				{
					return index;
				}
				else
				{
					index++;
					int result = getIndexOfNode(node.Nodes, nodeToFind);
					if (result != -1)
					{
						return result + index;
					}
					else
					{
						index += getChildCount(node.Nodes);
					}
				}
			}
			return -1;
		}

		public TreeNode findChildByName(string name)
		{
			return findChildByName(Nodes, name);
		}

		public TreeNode findChildByName(TreeNodeCollection nodes, string name)
		{
			TreeNode nodeFound;
			foreach (TreeNode node in nodes)
			{
				if (node.Text.Equals(name))
				{
					return node;
				}
				else
				{
					nodeFound = findChildByName(node.Nodes, name);
					if (nodeFound != null)
						return nodeFound;
				}
			}
			return null;
		}

		public TreeNode findChildById(int id)
		{
			return findChildByName(Nodes, id);
		}

		public TreeNode findChildByName(TreeNodeCollection nodes, int id)
		{
			TreeNode nodeFound;
			foreach (TreeNode node in nodes)
			{
				if (Form1.GetTreeObject(node).Id == id)
				{
					return node;
				}
				else
				{
					nodeFound = findChildByName(node.Nodes, id);
					if (nodeFound != null)
						return nodeFound;
				}
			}
			return null;
		}

		public bool isDescendant(TreeNode parent, TreeNode child)
		{
			if (parent == null)
				return false;
			// Keep finding the child's parents until we either reach the 
			// top of the tree, or the parent we are searching for.
			while(child != parent && child != null)
			{
				child = child.Parent;
			}
			return (child == parent);
		}

		public TreeNode GetNextTreeNode(TreeNode currentNode)
		{
			// Has children?
			if (currentNode.Nodes.Count > 0)
				return currentNode.Nodes[0];

			// Get next sybling, or sybling of parent.
			while (true)
			{
				int position = currentNode.Index;
				//Top of tree?
				if(currentNode.Parent == null)
				{
					if (position + 1 < Nodes.Count)
					{
						return Nodes[position + 1];
					}
					else
					{
						return Nodes[0]; //Start over at beginning.
					}
				}
				//Are there more children?
				if(position + 1 < currentNode.Parent.Nodes.Count)
				{
					return currentNode.Parent.Nodes[position + 1];
				}
				else
				{
					// I guess this is the last child. Climb up the tree
					// and ask for the parent's sybling.
					currentNode = currentNode.Parent;
				}
			}
		}

		private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
		{
			TreeNode newDragSource = (TreeNode)e.Item;
			dragSourceNode = newDragSource;
			DoDragDrop(newDragSource.Name, DragDropEffects.Copy | DragDropEffects.Move);
		}

		TreeNode dragSourceNode;

		private void treeView1_MouseUp(object sender, MouseEventArgs e)
		{
			selectDragDestinationNode(null);
			dragSourceNode = null;
			if (e.Button == MouseButtons.Right)
			{
				rightClickNode = GetNodeAt(new Point(e.X, e.Y));
				SelectedNode = rightClickNode;
				if (rightClickNode != null)
				{
					ShowNodeActions(true);
					contextMenuStrip1.Show(this, e.Location);
				}
				else
				{
					ShowNodeActions(false);
					contextMenuStrip1.Show(this, e.Location);
				}
			}
		}

		private void ShowNodeActions(bool visible)
		{
			renameToolStripMenuItem.Visible = visible;
			deleteToolStripMenuItem.Visible = visible;
			expandAllToolStripMenuItem.Visible = visible;
		}

		private void EditableTreeView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if (e.EscapePressed)
			{
				selectDragDestinationNode(null);
				selectDragDestinationAboveNode(null);
				selectDragDestinationBelowNode(null);
			}
		}


		#endregion

		#region [NODE RIGHT-CLICK CONTEXT MENU]

		TreeNode rightClickNode;

		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SelectedNode = rightClickNode;
			if (!rightClickNode.IsEditing)
			{
				rightClickNode.BeginEdit();
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			deleteSelectedNodeAfterConfirmation();
		}

		private void alphabetizeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNodeCollection nodes = (rightClickNode != null) ? rightClickNode.Nodes : Nodes;
			List<TreeNode> list = new List<TreeNode>();
			foreach (TreeNode n in nodes)
			{
				list.Add(n);
			}
			list.Sort(delegate(TreeNode n1, TreeNode n2){return n1.Text.CompareTo(n2.Text);});
			nodes.Clear();
			foreach (TreeNode n in list)
			{
				nodes.Add(n);
			}
		}

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			rightClickNode.ExpandAll();
		}
		
		private void treeView1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				deleteSelectedNodeAfterConfirmation();
			}
		}

		private void deleteSelectedNodeAfterConfirmation()
		{
			if (MessageBox.Show("Are you sure you want to delete this node?",
				"Confirm Node Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Nodes.Remove(SelectedNode);
			}
		}

		private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label != null) // For some reason this is null sometimes, if the user doesn't succeed in renaming a node.
			{
				if (e.Label.Length > 0)
				{
					if (e.Label.IndexOfAny(invalidCharacters) == -1)
					{
						// Stop editing without canceling the label change.
						e.Node.EndEdit(false);
					}
					else
					{
						/* Cancel the label edit action, inform the user, and 
						   place the node in edit mode again. */
						e.CancelEdit = true;
						MessageBox.Show("Invalid tree node label.\n" +
						   "The invalid characters are: '@','.', ',', '!'",
						   "Node Label Edit");
						e.Node.BeginEdit();
					}
				}
				else
				{
					/* Cancel the label edit action, inform the user, and 
					   place the node in edit mode again. */
					e.CancelEdit = true;
					MessageBox.Show("Invalid tree node label.\nThe label cannot be blank",
					   "Node Label Edit");
					e.Node.BeginEdit();
				}
			}
		}

		private void addNodeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode newNode = new TreeNode("New Node");
			TreeNodeCollection nodes = (rightClickNode != null) ? rightClickNode.Nodes : Nodes;
			nodes.Add(newNode);
			if (afterAddNodeEvent != null)
				afterAddNodeEvent(newNode);
			SelectedNode = newNode;
			if(rightClickNode != null)
				rightClickNode.Expand();
			newNode.BeginEdit();
		}

		#endregion

		char[] invalidCharacters = new char[] { '@', '.', ',', '!' };
		public char[] InvalidCharacters
		{
			get { return invalidCharacters; }
			set { invalidCharacters = value; }
		}

		public TreeNode RightClickNode
		{
			get { return rightClickNode; }
		}

		public ContextMenuStrip RightClickNodeContextMenu
		{
			get { return contextMenuStrip1; }
			set { contextMenuStrip1 = value; }
		}

		public delegate void AfterAddNodeDelegate(TreeNode nodeAdded);
		event AfterAddNodeDelegate afterAddNodeEvent;
		public event AfterAddNodeDelegate AfterAddNode
		{
			add 
			{
				afterAddNodeEvent = (AfterAddNodeDelegate)Delegate.Combine(afterAddNodeEvent, value);
			}
			remove 
			{
				afterAddNodeEvent = (AfterAddNodeDelegate)Delegate.Remove(afterAddNodeEvent, value);
			}
		}

		public delegate void OnDragTextToNodeDelegate(TreeNode destinationNode, string text);
		event OnDragTextToNodeDelegate onDragTextToNode;
		public event OnDragTextToNodeDelegate OnDragTextToNode
		{
			add
			{
				onDragTextToNode = (OnDragTextToNodeDelegate)Delegate.Combine(onDragTextToNode, value);
			}
			remove
			{
				onDragTextToNode = (OnDragTextToNodeDelegate)Delegate.Remove(onDragTextToNode, value);
			}
		}

		StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
		int x = 0;
		protected override void OnPaint(PaintEventArgs e)
		{
			//if(x++ < 1000)
			//	base.OnPaint(e);
			

			//base.OnPaint(e);
			//e.Graphics.DrawRectangle(Pens.Black, new Rectangle(10, 10, 10, 10));
			//drawNodes(e, Nodes);
		}

		private void drawNodes(PaintEventArgs e, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				drawNode(e, node);
			}
		}
		private void drawNode(PaintEventArgs e, TreeNode node)
		{
			Rectangle textRect = new Rectangle(node.Bounds.X, node.Bounds.Y, node.Bounds.Width, node.Bounds.Height);
			if (node.IsSelected)
			{
				e.Graphics.FillRectangle(SystemBrushes.Highlight, textRect);
				e.Graphics.DrawString(node.Text, SystemFonts.DefaultFont, SystemBrushes.HighlightText, textRect, sf);
			}
			else
			{
				e.Graphics.DrawString(node.Text, SystemFonts.DefaultFont, SystemBrushes.WindowText, textRect, sf);
			}
			drawNodes(e, node.Nodes);
		}

		private void EditableTreeView_Resize(object sender, EventArgs e)
		{
			g = CreateGraphics(); //If it resizes, we won't be able to draw on the expanded portions of the control.
		}
	}
}