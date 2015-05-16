using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
	public class TextParser : ItemParser
	{
		protected int braceLevel = 0;
		public int textPosition = 0;
		public bool textPositionEarliest = true;

		public TextParser(string text)
			: base(text)
		{
		}

		public override void GoToBeginning()
		{
			base.GoToBeginning();
			braceLevel = 0;
			textPosition = 0;
			withinGroupHeader = false;
			withinTextSection = false;
			atEndOfTextSection = false;
			hideGroupLevel = 0;
			itemTextLength = 0;
			indexWithinItem = 0;
		}

		public bool GoToText()
		{
			if (textPosition > 0)
				GoToBeginning();
			while (!withinTextSection)
			{
				if (!GoToNextItem())
				{
					return false;
				}
			}
			return true;
		}

		public bool GoToNextTextTag()
		{
			if (!withinTextSection)
			{
				GoToText();
				IgnoreNextSeek();
			}
			return GoToNextTag();
		}

		public bool GoToNextTag()
		{
			while (GoToNextItem())
			{
				if (itemType == ItemType.Tag)
				{
					return true;
				}
			}
			return false;
		}

		/*public GroupParser(GroupParser mirror)
			: base(mirror)
		{
			SetCurrentGroup(mirror.groupType, mirror.braceLevel);
		}

		public void SetCurrentGroup(GroupType groupType, int braceLevel)
		{
			this.groupType = groupType;
			this.braceLevel = braceLevel;
		}

		public new GroupParser CreateCopy()
		{
			GroupParser copy = new GroupParser(this);
			return copy;
		}*/

		//Returns the equivalent length of the item in simple text.
		private int GetItemTextLength()
		{
			if (withinTextSection && braceLevel >= 1 && hideGroupLevel == 0)
			{
				if (itemType == ItemType.EscapeSequence ||
					itemType == ItemType.HexCharacter)
					return 1;
				else if (itemType == ItemType.Text)
					return itemLength;
				else if (braceLevel == 1)
				{
					string tg = GetTag();
					if (tg.Equals("ldblquote") || tg.Equals("rdblquote") ||
					tg.Equals("lquote") || tg.Equals("rquote") ||
					tg.Equals("par") || tg.Equals("tab") || tg.Equals("line"))
					{
						return 1;
					}
				}
			}
			return 0;
		}

		public int itemTextLength = 0;

		public override void IdentifyNextItem()
		{
			indexWithinItem = 0;
			base.IdentifyNextItem();
			itemTextLength = GetItemTextLength();
			if (lastItemType == ItemType.LeftBrace && itemType == ItemType.Tag)
				withinGroupHeader = true;
			if (itemType == ItemType.RightBrace && braceLevel == 1)
				atEndOfTextSection = true;
			if (braceLevel == 1 && !withinGroupHeader && !withinTextSection &&
				itemType != ItemType.LeftBrace && itemType != ItemType.RightBrace && itemType != ItemType.NewlineCharacter)
			{
				withinTextSection = true;
			}
		}

		public void InsertTag(string tag)
		{
			InsertTagAt(indexWithinItem, tag); 
		}

		public void ReplaceTag(string tag)
		{
			if (itemType != ItemType.Tag)
				throw new Exception("Item is not a tag. Cannot replace.");
			InsertTag(tag); //Identifies next item at end of function.
			int tagToBeDelPos = itemStartIndex + tag.Length;
			while(itemStartIndex < tagToBeDelPos)
			{
				GoToNextItem();
			}
			RemoveTag();
		}

		public int indexWithinItem;

		public bool withinGroupHeader = false;
		public bool withinTextSection = false;
		public bool atEndOfTextSection = false;
		public int hideGroupLevel = 0; //0 doesn't hide group, 1 or greater hides it. (like in "\*")

		// getEarliest should be true if you want to get the earliest possible position.
		// getEarliest should be false if you want the latest possible position.
		public bool GoToTextPosition(int position, bool getEarliest)
		{
			if (position < textPosition || (position == textPosition && (!textPositionEarliest && getEarliest)))
			{
				GoToBeginning();
			}

			//Make sure we have reached the text.
			GoToText();

			while (true)
			{
				if (textPosition == position && (itemTextLength > 0 || getEarliest))
				{
					indexWithinItem = 0;
					return true;
				}
				else if (textPosition + itemTextLength > position)
				{
					indexWithinItem = position - textPosition;
					return true;
				}

				if (!GoToNextItem())
				{
					return false;
				}
			}
		}

		public override void RecordItem()
		{
			if (itemType != ItemType.Tag && withinGroupHeader)
			{
				withinGroupHeader = false;
			}
			base.RecordItem();
			textPosition += itemTextLength;
			if(itemTextLength > 0)
			{
				textPositionEarliest = true;
			}
			else if (withinTextSection) // && itemTextLength == 0
			{
				textPositionEarliest = false;
			}
		}

		public override void RecordBrace()
		{
			if (itemType == ItemType.LeftBrace)
			{
				braceLevel++;
			}
			else if (itemType == ItemType.RightBrace)
			{
				braceLevel--;
				if (braceLevel < hideGroupLevel)
				{
					hideGroupLevel = 0;
				}
			}
			if (braceLevel == 0)
			{
				withinTextSection = false;
			}
		}

		/*public void RecordNewPar()
		{
			//There is always a /par at the end that shouldn't be counted
			if (withinTextSection && (text.Length - itemEndIndex) > 5)
			{
				textPosition += 1;
			}
		}*/

		public override void RecordTag()
		{
			if (withinGroupHeader)
			{
				RecordGroupHeader();
			}
			if (hideGroupLevel == 0 && GetItemChar(1) == '*')
			{
				hideGroupLevel = braceLevel;
			}
		}

		public virtual void RecordGroupHeader()
		{
		}

		public string GetGroupHeader()
		{
			return GetTag();
		}
	}
}
