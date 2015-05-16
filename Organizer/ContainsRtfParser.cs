using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Organizer
{
	public class ContainsRtfParser : GroupParser
	{
		public bool containsRtf = false;
		Font prefFont;
		Color prefColor;
		int red;
		int green;
		int blue;
		string fontName;
		int fontSize;

		public static bool ContainsRtf(string rtf, Font preferredFont, Color preferredFontColor)
		{
			ContainsRtfParser parser = new ContainsRtfParser(rtf, preferredFont, preferredFontColor);
			parser.ParseCompletely();
			return parser.containsRtf;
		}

		public ContainsRtfParser(string text, Font prefFont, Color prefColor)
			: base(text)
		{
			this.prefFont = prefFont;
			this.prefColor = prefColor;
			this.red = prefColor.R;
			this.green = prefColor.G;
			this.blue = prefColor.B;
			this.fontName = prefFont.Name;
			this.fontSize = (int)(Math.Ceiling(prefFont.Size * 2));
		}

		public override void GoToBeginning()
		{
			base.GoToBeginning();
			containsRtf = false;
		}

		public override void RecordGroupHeader()
		{
			base.RecordGroupHeader();
			if (groupHeader.Equals("pict"))
			{
				groupType = GroupType.Picture; // We shouldn't really support pictures... but they're there.
				containsRtf = true;
			}
		}

		public override void RecordText()
		{
			base.RecordText();
			if (groupType == GroupType.FontTable)
			{
				string fontName = GetItem().TrimEnd(';');
				if (!fontName.Equals(this.fontName))
					containsRtf = true;
			}
		}

		public override void RecordTag()
		{
			base.RecordTag();
			string tg;
			if (withinTextSection)
			{
				tg = GetTag();
				if (tg.Equals("b") || tg.Equals("i") || tg.Equals("ul") ||
					tg.Equals("super") || tg.Equals("sub") || tg.Equals("v"))
					containsRtf = true;
				else if (tg.StartsWith("fs") && !tg.Equals("fs" + fontSize))
					containsRtf = true;
				else if (tg.StartsWith("highlight"))
					containsRtf = true;
				else if (tg.StartsWith("lang"))
					containsRtf = true;
			}
			else if (groupType == GroupType.ColorTable)
			{
				tg = GetTag();
				if (tg.StartsWith("red") && !tg.Equals("red" + red))
					containsRtf = true;
				else if (tg.StartsWith("green") && !tg.Equals("green" + green))
					containsRtf = true;
				else if (tg.StartsWith("blue") && !tg.Equals("blue" + blue))
					containsRtf = true;
			}
		}
	}
}