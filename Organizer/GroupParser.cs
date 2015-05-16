using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Organizer
{
	public class GroupParser : TextParser
	{
		public GroupParser(string text)
			: base(text)
		{
		}

		public enum GroupType { None, FontTable, ColorTable, Text, Picture, Unknown };
		public GroupType groupType = GroupType.None;

		public override void GoToBeginning()
		{
			base.GoToBeginning();
			groupType = GroupType.None;
		}

		/*public bool GoToStartOfNextGroup(int level)
		{
			while (GoToNextItem())
			{
				if (braceLevel == level)
				{
					if (itemType == ItemType.Brace && GetItemChar(0) == '{')
					{
						groupType = GetGroupType();
						return true;
					}
					//else if(itemType != ItemType.NewlineCharacter)
					//{
					//	return false;
					//}
				}
			}
			return false;
		}*/

		//Places the cursor just before the ending brace of the group level desired.
		//Must already be within group.
		public void GoToEndOfGroup(int level)
		{
			if (level > braceLevel)
			{
				throw new Exception("Trying to go to end of group that parser is not yet in.");
			}
			while (!IsAtEndOfGroup(level))
			{
				GoToNextItem();
			}
		}

		public bool IsAtEndOfGroup(int level)
		{
			return (braceLevel == level && itemType == ItemType.RightBrace);
		}

		protected string groupHeader;
		public override void RecordGroupHeader()
		{
			base.RecordGroupHeader();
			groupHeader = GetGroupHeader();
			if (groupHeader.Equals("colortbl"))
				groupType = GroupType.ColorTable;
			else if (groupHeader.Equals("fonttbl"))
				groupType = GroupType.FontTable;
			delimiterCounter = 0;
		}

		protected int delimiterCounter = 0;

		public override void RecordDelimiter()
		{
			delimiterCounter++;
		}
	}

	public class GroupEditor
	{
		public List<TextParser> groupBeginnings; //stores brace locations
		public List<TextParser> groupEndings;
		public int level; //Level zero means you are outside of all braces

		public GroupEditor(string text)
		{
			groupBeginnings[0] = new TextParser(text);
		}

		public void GoToLevel()
		{

		}

		public void GoToNextLevel()
		{

		}

		// Navigates to the next sub group within a group
		// If first exits the group, returns false.
		public bool GoToNextGroup()
		{
			groupBeginnings[level] = groupEndings[level];
			//if (! groupBeginnings[level].GoToStartOfNextGroup(level))
			//	return false;
			//groupEndings[level] = groupBeginnings[level].CreateCopy();
			//if(! groupEndings[level].GoToEndOfGroup(level))
			//	throw new Exception("Could not find end of braces on level " + level);
			return true;
		}

		public void GetGroupText()
		{
			//GroupParser temp = groupParser.CreateCopy();
		}
	}
}