using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
	public class ItemParser
	{
		public string text;
		public int itemStartIndex = 0;
		public int itemEndIndex = 0;
		bool ignoreNextSeek = false;
		public int itemLength
		{
			get { return itemEndIndex - itemStartIndex; }
			set { itemEndIndex = itemStartIndex + value; }
		}
		public ItemType lastItemType = ItemType.NotYetAssigned;
		public ItemType itemType = ItemType.NotYetAssigned;
		public bool treatSemicolonAsDelimiter = true;
		public enum ItemType { NotYetAssigned, LeftBrace, RightBrace, Tag,
		NewlineCharacter, EscapeSequence, HexCharacter, TagStop, Text, Delimiter}

		public ItemParser(string text)
		{
			this.text = text;
		}

		public ItemParser(ItemParser mirror)
			: this(mirror.text)
		{
			SetCurrentPosition(mirror.itemStartIndex, mirror.itemEndIndex, mirror.itemType);
		}

		public virtual ItemParser CreateCopy()
		{
			ItemParser copy = new ItemParser(text);
			copy.SetCurrentPosition(itemStartIndex, itemEndIndex, itemType);
			return copy;
		}

		public void SetCurrentPosition(int itemStartIndex, int itemEndIndex, ItemType itemType)
		{
			this.itemStartIndex = itemStartIndex;
			this.itemEndIndex = itemEndIndex;
			this.itemType = itemType;
		}

		public virtual void ParseCompletely()
		{
			int i = 0;
			while (GoToNextItem())
			{
				string itt = GetItem();
				i++;
			}
		}

		public virtual void GoToBeginning()
		{
			itemStartIndex = 0;
			itemEndIndex = 0;
			lastItemType = ItemType.NotYetAssigned;
			itemType = ItemType.NotYetAssigned;
		}

		public void IgnoreNextSeek()
		{
			ignoreNextSeek = true;
		}

		public bool GoToNextItem()
		{
			if (!ignoreNextSeek)
			{
				if (itemEndIndex > 0)
				{
					RecordItem();
				}
				if (itemEndIndex == text.Length)
				{
					return false;
				}
				itemStartIndex = itemEndIndex;
			}
			else
			{
				ignoreNextSeek = false;
			}
			IdentifyNextItem();
			return true;
		}

		public string GetItem()
		{
			return text.Substring(itemStartIndex, itemLength);
		}

		public string GetTag()
		{
			return text.Substring(itemStartIndex + 1, itemLength - 1);
		}

		public char GetItemChar(int relativePosition)
		{
			return text[itemStartIndex + relativePosition];
		}

		public int GetItemIndexOfAny(char[] anyOf)
		{
			int res = text.IndexOfAny(anyOf, itemStartIndex + 1) - itemStartIndex;
			if (res == 0)
				throw new Exception("Character not found");
			return res;
		}

		public virtual void RecordItem()
		{
			lastItemType = itemType;
			switch (itemType)
			{
				case ItemType.LeftBrace:
				case ItemType.RightBrace:
					RecordBrace();
					break;
				case ItemType.Tag:
					RecordTag();
					break;
				case ItemType.EscapeSequence:
					RecordEscapeSequence();
					break;
				case ItemType.HexCharacter:
					RecordHexCharacter();
					break;
				case ItemType.Text:
					RecordText();
					break;
				case ItemType.Delimiter:
					RecordDelimiter();
					break;
			}
		}

		public virtual void RecordText()
		{
		}

		public virtual void RecordBrace()
		{
		}

		public virtual void RecordDelimiter()
		{
		}

		public virtual void RecordTag()
		{
		}

		public virtual void RecordEscapeSequence()
		{
		}

		public virtual void RecordHexCharacter()
		{
		}

		//Should not be used for inserting tag.
		protected void InsertItem(string item)
		{
			text = text.Insert(itemStartIndex, item);
			IdentifyNextItem();
		}

		//itemIndex must be 0 unless it is actual text.
		protected void InsertTagAt(int insertIndex, string tag)
		{
			if (insertIndex > 0 && itemType != ItemType.Text)
			{
				throw new Exception("Cannot insert a tag inside of any item");
			}
			if (insertIndex >= itemLength)
			{
				throw new Exception("Must insert the tag at an index less than the item length");
			}
			// Will we need insert a tagstop after this item?
			bool appendTagStopAtEnd = (itemType == ItemType.Text);
			text = text.Insert(itemStartIndex + insertIndex, "\\" + tag + (appendTagStopAtEnd ? " " : ""));
			IdentifyNextItem();
		}

		//itemIndex must be 0 unless it is actual text.
		// Removes the tag. Next time GoToNextItem is called, will identify the next item, but won't change positions.
		public void RemoveTag()
		{
			if (itemType != ItemType.Tag)
			{
				throw new Exception("Cannot remove anything but tags in the removeTag method");
			}
			text = text.Remove(itemStartIndex, itemLength);
			if (lastItemType != ItemType.Tag && (GetItemChar(0) == ' ' || GetItemChar(0) == '-'))
			{
				// Oh. I guess we have to remove the tag stop too. That better be it!
				text = text.Remove(itemStartIndex, 1);
			}
			IgnoreNextSeek();
		}

		//Identify
		public virtual void IdentifyNextItem()
		{
			// Tag stop
			if (lastItemType == ItemType.Tag && (GetItemChar(0) == ' ' || GetItemChar(0) == '-'))
			{
				itemType = ItemType.TagStop;
				itemLength = 1;
				return;
			}

			// Normal scenario
			switch (GetItemChar(0))
			{
				case '{':
					itemType = ItemType.LeftBrace;
					itemLength = 1;
					break;
				case '}':
					itemType = ItemType.RightBrace;
					itemLength = 1;
					break;
				case ';':
					if (treatSemicolonAsDelimiter)
					{
						itemType = ItemType.Delimiter;
						itemLength = 1;
						break;
					}
					else
					{
						goto default; // Fall throughs aren't allowed in C#
					}
				case '\r':
					if (GetItemChar(1) == '\n')
					{
						itemType = ItemType.NewlineCharacter;
						itemLength = 2;
					}
					break;
				case '\n': //Added as a bug fix.
					itemType = ItemType.NewlineCharacter;
					itemLength = 1;
					break;
				case '\\':
					switch (GetItemChar(1))
					{
						case '{':
						case '}':
						case '\\':
							itemType = ItemType.EscapeSequence;
							itemLength = 2;
							break;
						case '\'':
							itemType = ItemType.HexCharacter;
							itemLength = 4;
							break;
						default:
							itemType = ItemType.Tag;
							itemLength = GetItemIndexOfAny(new char[] { '{', '}', ';', ' ', '-', '\r', '\n', '\\' });
							break;
					}
					break;
				default:
					itemType = ItemType.Text;
					itemLength = GetItemIndexOfAny(new char[] { '{', '}', '\r', '\n', '\\' });
					break;
			}
		}
	}
}
