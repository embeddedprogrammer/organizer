using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Organizer
{
	public class LangParser : FontParser
	{
		public static string ReplaceLangWithHyperlink(string rtf)
		{
			LangParser parser = new LangParser(rtf);

			while (parser.GoToNextTextTag())
			{
				//\ul\ulc2 
				string tg = parser.GetTag();
				if (tg.StartsWith("lang") && char.IsDigit(tg, 4))
				{
					int underlineNum = ExtractNum("lang", tg);
					if (underlineNum == 0)
					{
						tg = @"ul0\ulc0";
					}
					else
					{
						int fontIndex = 0;//fontColors.IndexOf(underlineColors[underlineNum]);
						tg = @"ul\ulc" + fontIndex;
					}
					parser.ReplaceTag(tg);
				}
			}
			return parser.text;
		}

		public static string GetClippedRtf(string rtf, int pos, int endPos)
		{
			LangParser parser = new LangParser(rtf);
			parser.GoToTextPosition(pos, false);
			parser.InsertTag("lang" + parser.lang);
			if(parser.itemType == ItemType.Text)
				parser.GoToNextItem();
			int beginCut = parser.itemStartIndex + ((parser.itemType == ItemType.Text) ? parser.indexWithinItem : 0);
			parser.GoToTextPosition(endPos, true);
			int endCut = parser.itemStartIndex + ((parser.itemType == ItemType.Text) ? parser.indexWithinItem : 0);
			return @"{\rtf1\ansi{}" + parser.text.Substring(beginCut, endCut - beginCut) + "}";
		}

		public LangParser(string rtf)
			: base(rtf)
		{
		}

		public static int GetLangAt(string rtf, int pos)
		{
			LangParser parser = new LangParser(rtf);
			parser.GoToTextPosition(pos, false);
			return parser.lang;
		}

		public static string GetNextChars(string rtf, int pos, int len)
		{
			LangParser parser = new LangParser(rtf);
			parser.GoToTextPosition(pos, false);
			string s = "";
			do
			{
				if (parser.itemType == ItemType.Text)
				{
					s += parser.GetItem().Substring(parser.indexWithinItem);
				}
				else if (parser.itemType == ItemType.EscapeSequence)
				{
					s += parser.GetItemChar(1);
				}
				else
				{
					for (int i = 0; i < parser.itemTextLength; i++)
					{
						s += "-";
					}
				}
			}
			while (parser.GoToNextItem() && s.Length < len);
			return (s.Length > 10) ? s.Substring(0, 10) : s;
		}

		public int lang;

		public override void RecordBrace()
		{
			base.RecordBrace();
			if (itemType == ItemType.RightBrace)
			{
				lang = 0;
			}
		}

		public override void RecordTag()
		{
			base.RecordTag();
			if (withinTextSection && hideGroupLevel == 0)
			{
				string tg = GetTag();
				if (tg.StartsWith("lang") && char.IsDigit(tg, 4))
				{
					lang = ExtractNum("lang", tg);
				}
			}
		}
	}

	public class FontParser : GroupParser
	{
		public static void ParseUnderlineColors(RichTextBoxEx rtb, string rtf, int offset)
		{
			rtb.SuspendDrawing();
			LangParser parser = new LangParser(rtf);
			List<Color> fontColors = parser.GetFontColors();

			bool ul = false;
			int underlineColorIndex = 0;
			int zstartIndx = 0;
			int zendIndx = 0;
			int startIndx = 0;
			int endIndx = 0;
			parser.GoToText();
			while (parser.GoToNextItem())
			{
				if (parser.itemType == ItemType.RightBrace)
				{
					if (underlineColorIndex != 0) //any brace resets the underline color.
					{
						endIndx = parser.textPosition + offset;
						rtb.Select(startIndx, endIndx - startIndx);
						rtb.SelectionUnderlineColorIndex = underlineColorIndex;

						ul = false;
						underlineColorIndex = 0;
					}
				}
				else if (parser.itemType == ItemType.Tag && parser.withinTextSection)
				{
					//\ul\ulc2 
					string tg = parser.GetTag();
					if (tg.StartsWith("ul"))
					{
						if (tg.Equals("ul0") || tg.Equals("ulnone"))
						{
							if (ul && underlineColorIndex != 0)
							{
								endIndx = parser.textPosition + offset;
								rtb.Select(startIndx, endIndx - startIndx);
								rtb.SelectionUnderlineColorIndex = underlineColorIndex;

								ul = false;
								underlineColorIndex = 0;
							}
						}
						else if (tg.StartsWith("ulc") && ul) //for now must do ul and then ulc.
						{
							if (underlineColorIndex != 0)
							{
								endIndx = parser.textPosition + offset;
								rtb.Select(startIndx, endIndx - startIndx);
								rtb.SelectionUnderlineColorIndex = underlineColorIndex;

								underlineColorIndex = 0;
							}
							Color underlineColor = fontColors[Convert.ToInt32(ExtractNum("ulc", tg))];
							underlineColorIndex = FindNearestUnderlineColor(underlineColor);
							startIndx = parser.textPosition + offset;
						}
						else
						{
							ul = true;
						}
					}
					if (tg.StartsWith("lang") && char.IsDigit(tg, 4))
					{
						if (parser.lang > 5000) //last parser thing.
						{
							zendIndx = parser.textPosition + offset;

							rtb.Select(zstartIndx, zendIndx - zstartIndx);
							rtb.SetSelectionLink(true);
						}
						else
						{
							zstartIndx = parser.textPosition + offset;
						}
					}
				}
			}
			if (ul && underlineColorIndex != 0)
			{
				endIndx = parser.textPosition + offset;
				rtb.Select(startIndx, endIndx - startIndx);
				rtb.SelectionUnderlineColorIndex = underlineColorIndex;

				ul = false;
				underlineColorIndex = 0;
			}
			rtb.ResumeDrawing();
		}

		private static Color[] underlineColors = new Color[] {
				Color.FromArgb(0, 0, 0),
				Color.FromArgb(0, 0, 255),
				Color.FromArgb(0, 255, 255),
				Color.FromArgb(0, 255, 0),
				Color.FromArgb(255, 0, 255),
				Color.FromArgb(255, 0, 0),
				Color.FromArgb(255, 255, 0),
				Color.FromArgb(255, 255, 255),
				Color.FromArgb(0, 0, 128),
				Color.FromArgb(0, 128, 128),
				Color.FromArgb(0, 128, 0),
				Color.FromArgb(128, 0, 128),
				Color.FromArgb(128, 0, 0),
				Color.FromArgb(128, 128, 0),
				Color.FromArgb(128, 128, 128),
				Color.FromArgb(192, 192, 192)};

		//anim really stores from 1 to 18.
		private static int FindNearestUnderlineColor(Color givenColor)
		{
			int closestColorIndex = 0;
			int closestDistance = 256 * 3;
			for (int i = 0; i < underlineColors.Length; i++)
			{
				Color underlineColor = underlineColors[i];
				int distance = Math.Abs(givenColor.R - underlineColor.R) +
							   Math.Abs(givenColor.G - underlineColor.G) +
							   Math.Abs(givenColor.B - underlineColor.B);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestColorIndex = i;
				}
			}
			return closestColorIndex;
		}

		public static string ReplaceClfWithAnim(string rtf)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();

			bool ul = false;
			int underlineColorIndex = 0;
			parser.GoToText();
			while (parser.GoToNextTag())
			{
				if (parser.itemType == ItemType.RightBrace)
				{
					if (underlineColorIndex != 0)
					{
						ul = false;
						underlineColorIndex = 0;
						parser.InsertTag("animtext0");
					}
				}
				else if (parser.itemType == ItemType.Tag && parser.withinTextSection)
				{
					//\ul\ulc2 
					string tg = parser.GetTag();
					if (tg.StartsWith("ul"))
					{
						if (tg.Equals("ul0") || tg.Equals("ulnone"))
						{
							ul = false;
							if (underlineColorIndex != 0)
							{
								parser.ReplaceTag("animtext0");
							}
						}
						else if (tg.StartsWith("ulc") && ul) //for now must do ul and then ulc.
						{
							Color underlineColor = fontColors[Convert.ToInt32(ExtractNum("ulc", tg))];
							int newUnderlineColorIndex = FindNearestUnderlineColor(underlineColor);
							if (newUnderlineColorIndex != underlineColorIndex)
							{
								underlineColorIndex = newUnderlineColorIndex;
								parser.ReplaceTag("animtext" + underlineColorIndex);
							}
							else
							{
								parser.RemoveTag();
							}
						}
						else
						{
							ul = true;
						}
					}
				}
			}
			return parser.text;
		}

		public static string ReplaceAnimWithClf(string rtf)
		{
			FontParser parser = new FontParser(rtf);
			FontParser parser2 = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();

			//No color table!
			if (fontColors.Count == 0)
			{
				parser.GoToBeginning();
				while (parser.braceLevel < 1 || parser.itemType == ItemType.Tag)
				{
					if (!parser.GoToNextItem())
					{
						throw new Exception("Only tags!");
					}
				}
				parser.InsertItem(@"{\colortbl ;\red0\green0\blue0;}");
				parser.GoToBeginning();
				fontColors = parser.GetFontColors();
			}

			while (parser2.GoToNextTextTag())
			{
				string tg = parser2.GetTag();
				if (tg.StartsWith("animtext"))
				{
					int underlineNum = ExtractNum("animtext", tg);
					Color underlineColor = underlineColors[underlineNum];
					if (!fontColors.Contains(underlineColor))
					{
						parser.GoToEndOfGroup(2);
						parser.AddFontColor(fontColors.Count, underlineColor);
						fontColors.Add(underlineColor);
					}
				}
			}

			while (parser.GoToNextTextTag())
			{
				//\ul\ulc2 
				string tg = parser.GetTag();
				if (tg.StartsWith("animtext"))
				{
					int underlineNum = ExtractNum("animtext", tg);
					if (underlineNum == 0)
					{
						tg = @"ul0\ulc0";
					}
					else
					{
						int fontIndex = fontColors.IndexOf(underlineColors[underlineNum]);
						tg = @"ul\ulc" + fontIndex;
					}
					parser.ReplaceTag(tg);
				}
			}
			return parser.text;
		}

		public static Color GetFontUnderlineColor(string rtf)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();

			while (parser.GoToNextTextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("ul"))
				{
					return fontColors[Convert.ToInt32(ExtractNum("ul", tg))];
				}
			}
			return Color.FromArgb(0, 0, 0);
		}

		public static string SetFontUnderlineColor(string rtf, Color fontColor)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();
			int fontIndex = fontColors.IndexOf(fontColor);
			if (fontIndex == -1)
			{
				fontIndex = fontColors.Count;
				parser.AddFontColor(fontIndex, fontColor);
			}
			parser.GoToTextPosition(0, true);
			parser.InsertTag("ulc" + fontIndex);
			while (parser.GoToNextTextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("ulc"))
				{
					parser.RemoveTag();
				}
			}
			return parser.text;
		}

		public static Color GetFontBackColor(string rtf)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();

			while (parser.GoToNextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("highlight"))
				{
					return fontColors[Convert.ToInt32(ExtractNum("highlight", tg))];
				}
			}
			return Color.FromArgb(0, 0, 0);
		}

		public static string SetFontBackColor(string rtf, Color fontColor)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();
			int fontIndex = fontColors.IndexOf(fontColor);
			if (fontIndex == -1)
			{
				fontIndex = fontColors.Count;
				parser.AddFontColor(fontIndex, fontColor);
			}
			parser.GoToTextPosition(0, true);
			parser.InsertTag("highlight" + fontIndex);
			while (parser.GoToNextTextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("highlight"))
				{
					parser.RemoveTag();
				}
			}
			return parser.text;
		}

		public static Color GetFontColor(string rtf)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();

			while (parser.GoToNextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("cf"))
				{
					return fontColors[Convert.ToInt32(ExtractNum("cf", tg))];
				}
			}
			return Color.FromArgb(0, 0, 0);
		}

		public static string SetFontColor(string rtf, Color fontColor)
		{
			FontParser parser = new FontParser(rtf);
			List<Color> fontColors = parser.GetFontColors();
			int fontIndex = fontColors.IndexOf(fontColor);
			if (fontIndex == -1)
			{
				fontIndex = fontColors.Count;
				parser.AddFontColor(fontIndex, fontColor);
			}
			parser.GoToTextPosition(0, true);
			parser.InsertTag("cf" + fontIndex);
			while (parser.GoToNextTextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("cf"))
				{
					parser.RemoveTag();
				}
			}
			return parser.text;
		}

		public static string GetFontName(string rtf)
		{
			FontParser parser = new FontParser(rtf);
			List<string> fontNames = parser.GetFontNames();

			while (parser.GoToNextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("f") && !tg.StartsWith("fs"))
				{
					return fontNames[Convert.ToInt32(ExtractNum("fs", tg))];
				}
			}
			return "Times New Roman";
		}

		public static string SetFontName(string rtf, string fontName)
		{
			FontParser parser = new FontParser(rtf);
			List<string> fontNames = parser.GetFontNames();
			int fontIndex = fontNames.IndexOf(fontName);
			if (fontIndex == -1)
			{
				fontIndex = fontNames.Count;
				parser.AddFontName(fontIndex, fontName);
			}
			parser.GoToTextPosition(0, true);
			parser.InsertTag("f" + fontIndex);
			while (parser.GoToNextTextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("f") && !tg.StartsWith("fs"))
				{
					parser.RemoveTag();
				}
			}
			return parser.text;
		}

		public static float GetFontSize(string rtf)
		{
			TextParser parser = new TextParser(rtf);
			while (parser.GoToNextTag())
			{
				string tg = parser.GetTag();
				if (tg.StartsWith("fs"))
				{
					return ((float)Convert.ToInt32(tg.Substring(2, tg.Length - 2))) / 2;
				}
			}
			return 8.25f;
		}

		public static string SetFontSize(string rtf, float fontSize)
		{
			int fs = (int)(Math.Ceiling(fontSize * 2));
			TextParser parser = new TextParser(rtf);
			parser.GoToTextPosition(0, true);
			parser.InsertTag("fs" + fs);
			while (parser.GoToNextTextTag())
			{
				if (parser.GetTag().StartsWith("fs"))
				{
					parser.RemoveTag();
				}
			}
			return parser.text;
		}

		public static bool GetProperty(string rtf, string tag)
		{
			TextParser parser = new TextParser(rtf);
			while (parser.GoToNextTextTag())
			{
				if (parser.GetTag().Equals(tag))
				{
					return true;
				}
			}
			return false;
		}

		public static string SetProperty(string rtf, string tag, bool option)
		{
			TextParser parser = new TextParser(rtf);
			if (option)
			{
				parser.GoToTextPosition(0, true);
				parser.InsertTag(tag);
				//parser.GoToTextPosition(SelectionLength, true);
				//parser.InsertTag(tag + "0");
			}
			while (parser.GoToNextTextTag())
			{
				string tg = parser.GetTag();
				if (tg.Equals(tag) || tg.Equals(tag + "0") || tg.Equals(tag + "none"))
				{
					parser.RemoveTag();
				}
			}
			return parser.text;
		}

		public FontParser(string text)
			: base(text)
		{
		}

		//{\colortbl ;\red255\green0\blue0;\green255;\blue255;}
		public List<Color> GetFontColors()
		{
			int red = 0;
			int green = 0;
			int blue = 0;
			List<Color> fontColors = new List<Color>();
			GoToBeginning();
			while (groupType != GroupType.ColorTable)
			{
				if (!GoToNextItem())
					return fontColors;
			}
			while (!IsAtEndOfGroup(2))
			{
				if (itemType == ItemType.Tag)
				{
					string tg = GetTag();
					if (tg.StartsWith("red"))
						red = ExtractNum("red", tg);
					if (tg.StartsWith("green"))
						green = ExtractNum("green", tg);
					if (tg.StartsWith("blue"))
						blue = ExtractNum("blue", tg);
				}
				if (itemType == ItemType.Delimiter)
				{
					fontColors.Add(Color.FromArgb(red, green, blue));
					red = 0;
					green = 0;
					blue = 0;
				}
				GoToNextItem();
			}
			return fontColors;
		}

		public static int ExtractNum(string suffix, string tag)
		{
			return Convert.ToInt32(tag.Substring(suffix.Length, tag.Length - suffix.Length));
		}

		List<string> SpecialFonts = new List<string>(new string[] 
			{"Bookshelf Symbol 7", "Marlett", "Monotype Sorts", "MS Outlook", "MS Reference Specialty",
				"MT Extra", "Symbol", "Technic", "Webdings", "Wingdings", "Wingdings 2", "Wingdings 3"});

		public List<String> GetFontNames()
		{
			List<String> fontNames = new List<string>();
			GoToBeginning();
			while (groupType != GroupType.FontTable)
			{
				if (!GoToNextItem())
					return fontNames;
			}
			while (! IsAtEndOfGroup(2))
			{
				if (itemType == ItemType.Text)
				{
					fontNames.Add(GetItem().TrimEnd(';'));
				}
				GoToNextItem();
			}
			return fontNames;
		}

		//Must be at end of font table.
		public void AddFontName(int fontIndex, string fontName)
		{
			if (groupType != GroupType.FontTable || !IsAtEndOfGroup(2))
				throw new Exception("Must be at end of font table");
			int charset = SpecialFonts.Contains(fontName) ? 2 : 0;
			InsertItem(@"{\f" + fontIndex + @"\fnil\fcharset" + charset + " " + fontName + ";}");
		}

		//{\colortbl ;\red255\green0\blue0;\green255;\blue255;}
		//Must be at end of color table.
		public void AddFontColor(int fontIndex, Color fontColor)
		{
			if (groupType != GroupType.ColorTable || !IsAtEndOfGroup(2))
				throw new Exception("Must be at end of color table");
			InsertItem(@"\red" + fontColor.R + @"\green" + fontColor.G + @"\blue" + fontColor.B + ";");
		}
	}
}