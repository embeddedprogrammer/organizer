using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Organizer
{
	public class TreeObject
	{
		public TreeNode Node;
		public int SelectionStart;
		public string Text;
		public string Rtf;
		public bool StoredAsRTF;
		public bool StoredSeparate;
		public string SeparateFilename;
		public bool Saved;
		private int id;
		public static bool idUsed;
		public int Id
		{
			get
			{
				idUsed = true;
				return id;
			}
			set
			{
				id = value;
			}
		}
		public static int IdCounter;

		public TreeObject(string text)
		{
			Text = text;
			SelectionStart = 0;
			StoredAsRTF = false;
			Id = ++IdCounter;
		}

		public TreeObject(string text, int selectionStart)
		{
			Text = text;
			SelectionStart = selectionStart;
			StoredAsRTF = false;
			Id = ++IdCounter;
		}

		public TreeObject(string text, bool storedAsRTF)
		{
			Text = text;
			SelectionStart = 0;
			StoredAsRTF = storedAsRTF;
			Id = ++IdCounter;
		}

		public TreeObject(string text, int selectionStart, bool storedAsRTF)
		{
			SelectionStart = selectionStart;
			StoredAsRTF = storedAsRTF;
			if (StoredAsRTF)
			{
				Text = "";
				Rtf = text;
			}
			else
			{
				Text = text;
			}
			Saved = true;
			Id = ++IdCounter;
		}

		public TreeObject(string separateFilename, int selectionStart, bool storedAsRTF, bool storedSeparate)
		{
			if (!storedSeparate)
				throw new Exception("Must be stored separate to use this constructor");
			SelectionStart = selectionStart;
			StoredAsRTF = storedAsRTF;
			StoredSeparate = storedSeparate;
			SeparateFilename = separateFilename;
			Saved = true;
			Id = ++IdCounter;
		}

		public TreeObject(string text, int selectionStart, bool storedAsRTF, int id)
		{
			SelectionStart = selectionStart;
			StoredAsRTF = storedAsRTF;
			if (StoredAsRTF)
			{
				Text = "";
				Rtf = text;
			}
			else
			{
				Text = text;
			}
			Saved = true;
			if (id == 0)
			{
				Id = ++IdCounter;
			}
			else
			{
				Id = id;
				if (Id > IdCounter)
					IdCounter = Id;
			}
		}

		public TreeObject(string separateFilename, int selectionStart, bool storedAsRTF, bool storedSeparate, int id)
		{
			if (!storedSeparate)
				throw new Exception("Must be stored separate to use this constructor");
			SelectionStart = selectionStart;
			StoredAsRTF = storedAsRTF;
			StoredSeparate = storedSeparate;
			SeparateFilename = separateFilename;
			Saved = true;
			Id = id;
			if (id == 0)
			{
				Id = ++IdCounter;
			}
			else
			{
				Id = id;
				if (Id > IdCounter)
					IdCounter = Id;
			}
		}

		public String toString()
		{
			return Text;
		}
	}
}
