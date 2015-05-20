using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
	public class Entry
	{
		private int id;
		public int Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
				if (id > IdCounter)
					IdCounter = id;
			}
		}
		public static int IdCounter;

		public static List<Entry> masterList = new List<Entry>();
		public string EntryText
		{
			get
			{
				return entryText;
			}
			set
			{
				if (entryText != value)
				{
					entryText = value;
					saved = false;
				}
			}
		}
		private string entryText = "";
		public List<TreeObject> relatedObjects = new List<TreeObject>();
		public bool saved;

		public Entry()
		{
			masterList.Add(this);
		}

		public Entry(string text)
			: this()
		{
			entryText = text;
			id = IdCounter++;
		}

		public void Disconnect()
		{
			foreach(TreeObject treeObject in relatedObjects)
			{
				treeObject.entries.Remove(this);
			}
			masterList.Remove(this);
		}

		public static Entry FindOrCreateById(int entryId)
		{
			foreach(Entry entry in masterList)
			{
				if (entry.Id == entryId)
					return entry;
			}
			Entry newEntry = new Entry();
			newEntry.Id = entryId;
			return newEntry;
		}
	}
}
