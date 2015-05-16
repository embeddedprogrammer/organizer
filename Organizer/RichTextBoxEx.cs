using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Organizer
{
	public partial class RichTextBoxEx : RichTextBox
	{
		#region Interop-Defines
		[StructLayout(LayoutKind.Sequential)]
		public struct CHARFORMAT2_STRUCT
		{
			public UInt32 cbSize;
			public UInt32 dwMask;
			public UInt32 dwEffects;
			public Int32 yHeight;
			public Int32 yOffset;
			public Int32 crTextColor;
			public byte bCharSet;
			public byte bPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public char[] szFaceName;
			public UInt16 wWeight;
			public UInt16 sSpacing;
			public int crBackColor; // Color.ToArgb() -> int
			public int lcid;
			public int dwReserved;
			public Int16 sStyle;
			public Int16 wKerning;
			public byte bUnderlineType;
			public byte bAnimation;
			public byte bRevAuthor;
			public byte bReserved1;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int WM_USER = 0x0400;
		private const int EM_GETCHARFORMAT = WM_USER + 58;
		private const int EM_SETCHARFORMAT = WM_USER + 68;

		private const int SCF_SELECTION = 0x0001;
		private const int SCF_WORD = 0x0002;
		private const int SCF_ALL = 0x0004;

		#region CHARFORMAT2 Flags
		private const UInt32 CFE_BOLD = 0x0001;
		private const UInt32 CFE_ITALIC = 0x0002;
		private const UInt32 CFE_UNDERLINE = 0x0004;
		private const UInt32 CFE_STRIKEOUT = 0x0008;
		private const UInt32 CFE_PROTECTED = 0x0010;
		private const UInt32 CFE_LINK = 0x0020;
		private const UInt32 CFE_AUTOCOLOR = 0x40000000;
		private const UInt32 CFE_SUBSCRIPT = 0x00010000;		/* Superscript and subscript are */
		private const UInt32 CFE_SUPERSCRIPT = 0x00020000;		/*  mutually exclusive			 */

		private const int CFM_SMALLCAPS = 0x0040;			/* (*)	*/
		private const int CFM_ALLCAPS = 0x0080;			/* Displayed by 3.0	*/
		private const int CFM_HIDDEN = 0x0100;			/* Hidden by 3.0 */
		private const int CFM_OUTLINE = 0x0200;			/* (*)	*/
		private const int CFM_SHADOW = 0x0400;			/* (*)	*/
		private const int CFM_EMBOSS = 0x0800;			/* (*)	*/
		private const int CFM_IMPRINT = 0x1000;			/* (*)	*/
		private const int CFM_DISABLED = 0x2000;
		private const int CFM_REVISED = 0x4000;

		private const int CFM_BACKCOLOR = 0x04000000;
		private const int CFM_LCID = 0x02000000;
		private const int CFM_UNDERLINETYPE = 0x00800000;		/* Many displayed by 3.0 */
		private const int CFM_WEIGHT = 0x00400000;
		private const int CFM_SPACING = 0x00200000;		/* Displayed by 3.0	*/
		private const int CFM_KERNING = 0x00100000;		/* (*)	*/
		private const int CFM_STYLE = 0x00080000;		/* (*)	*/
		private const int CFM_ANIMATION = 0x00040000;		/* (*)	*/
		private const int CFM_REVAUTHOR = 0x00008000;


		private const UInt32 CFM_BOLD = 0x00000001;
		private const UInt32 CFM_ITALIC = 0x00000002;
		private const UInt32 CFM_UNDERLINE = 0x00000004;
		private const UInt32 CFM_STRIKEOUT = 0x00000008;
		private const UInt32 CFM_PROTECTED = 0x00000010;
		private const UInt32 CFM_LINK = 0x00000020;
		private const UInt32 CFM_SIZE = 0x80000000;
		private const UInt32 CFM_COLOR = 0x40000000;
		private const UInt32 CFM_FACE = 0x20000000;
		private const UInt32 CFM_OFFSET = 0x10000000;
		private const UInt32 CFM_CHARSET = 0x08000000;
		private const UInt32 CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
		private const UInt32 CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

		private const byte CFU_UNDERLINENONE = 0x00000000;
		private const byte CFU_UNDERLINE = 0x00000001;
		private const byte CFU_UNDERLINEWORD = 0x00000002; /* (*) displayed as ordinary underline	*/
		private const byte CFU_UNDERLINEDOUBLE = 0x00000003; /* (*) displayed as ordinary underline	*/
		private const byte CFU_UNDERLINEDOTTED = 0x00000004;
		private const byte CFU_UNDERLINEDASH = 0x00000005;
		private const byte CFU_UNDERLINEDASHDOT = 0x00000006;
		private const byte CFU_UNDERLINEDASHDOTDOT = 0x00000007;
		private const byte CFU_UNDERLINEWAVE = 0x00000008;
		private const byte CFU_UNDERLINETHICK = 0x00000009;
		private const byte CFU_UNDERLINEHAIRLINE = 0x0000000A; /* (*) displayed as ordinary underline	*/

		#endregion

		#region Underline styles and colors

		public enum UnderlineStyle
		{
			None = 0,
			Normal = 1,
			Word = 2,
			Double = 3,
			Dotted = 4,
			Dash = 5,
			DashDot = 6,
			DashDotDot = 7,
			Wave = 8,
			Thick = 9,
			HairLine = 10,
			DoubleWave = 11,
			HeavyWave = 12,
			LongDash = 13
		}

		public enum UnderlineColor
		{
			Black = 0x00,
			Blue = 0x10,
			Cyan = 0x20,
			LimeGreen = 0x30,
			Magenta = 0x40,
			Red = 0x50,
			Yellow = 0x60,
			White = 0x70,
			DarkBlue = 0x80,
			DarkCyan = 0x90,
			Green = 0xA0,
			DarkMagenta = 0xB0,
			Brown = 0xC0,
			OliveGreen = 0xD0,
			DarkGray = 0xE0,
			Gray = 0xF0
		}

		Color[] underlineColors = new Color[] {
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

		#endregion

		#endregion

		public RichTextBoxEx()
		{
			InitializeComponent();
			// Otherwise, non-standard links get lost when user starts typing
			// next to a non-standard link
			this.DetectUrls = false;
		}

		[DefaultValue(false)]
		public new bool DetectUrls
		{
			get { return base.DetectUrls; }
			set { base.DetectUrls = value; }
		}

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
		private const int WM_SETREDRAW = 11;

		private int numOfTimesSuspended = 0;

		public void SuspendDrawing()
		{
			if(numOfTimesSuspended == 0)
			{
				SendMessage(Handle, WM_SETREDRAW, false, 0);
			}
			numOfTimesSuspended++;
		}

		public void ResumeDrawing()
		{
			numOfTimesSuspended--;
			if (numOfTimesSuspended == 0)
			{
				SendMessage(Handle, WM_SETREDRAW, true, 0);
				Refresh();
			}
			else if (numOfTimesSuspended < 0)
			{
				throw new Exception("Cannot resume drawing multiple times");
				numOfTimesSuspended = 0;
			}
		}

		private byte convertTwoHexDigitsToByte(string a)
		{
			if (a.Length != 2)
				return 0;
			return convertTwoHexDigitsToByte(a[0], a[1]);
		}

		private byte convertTwoHexDigitsToByte(char a, char b)
		{
			return (byte)((convertHexToInt(a) << 4) | convertHexToInt(b));
		}

		private int convertHexToInt(char c)
		{
			if (c >= 97 && c < 103)     // a to f
				return (byte)(c - 97 + 10);
			else if (c >= 65 && c < 71) // A to F
				return (byte)(c - 65 + 10);
			else if (c >= 48 && c < 57) // 0 to 9
				return (byte)(c - 48);
			else
				return (byte)0;
		}

		private string convertByteToHex(byte b)
		{
			int firstDigit = (b & 0xF0) >> 4;
			int secondDigit = b & 0x0F;
			return "" + convertDigitToHex(firstDigit) + convertDigitToHex(secondDigit);
		}

		private char convertDigitToHex(int n)
		{
			if (n >= 0 && n < 10)
				return (char)('0' + n);
			else if (n >= 11 && n < 16)
				return (char)('a' + n - 10);
			else
				return '0';
		}

		public bool startingUp = true;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string SelectedRtf
		{
			get
			{
				return startingUp ? base.SelectedRtf : FontParser.ReplaceAnimWithClf(base.SelectedRtf);
			}
			set
			{
				if (!startingUp)
				{
					int offset = SelectionStart;
					base.SelectedRtf = value;
					int offset2 = SelectionStart;
					FontParser.ParseUnderlineColors(this, value, offset);
					Select(offset2, 0);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string Rtf
		{
			get
			{
				return startingUp ? base.Rtf : FontParser.ReplaceAnimWithClf(base.Rtf);
			}
			set
			{
				if (!startingUp)
				{
					base.Rtf = value;
					int offset2 = SelectionStart;
					FontParser.ParseUnderlineColors(this, value, 0);
					Select(offset2, 0);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionFontColor
		{
			get
			{
				int color = GetSelectionCharFormat().crTextColor;
				return Color.FromArgb(color & 0xff, (color >> 8) & 0xff, (color >> 16) & 0xff);
				//return FontParser.GetFontColor(SelectedRtf);
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.crTextColor = value.R | (value.G << 8) | (value.B << 16);
				fmt.dwMask = CFM_COLOR;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetFontColor(SelectedRtf, value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionFontBackColor
		{
			get
			{
				int color = GetSelectionCharFormat().crBackColor;
				return Color.FromArgb(color & 0xff, (color >> 8) & 0xff, (color >> 16) & 0xff);
				//return FontParser.GetFontBackColor(SelectedRtf);
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.crBackColor = value.R | (value.G << 8) | (value.B << 16);
				fmt.dwMask = CFM_BACKCOLOR;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetFontBackColor(SelectedRtf, value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectionFontName
		{
			get
			{
				return new string(GetSelectionCharFormat().szFaceName).Trim('\0');
				//return FontParser.GetFontName(SelectedRtf);
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.szFaceName = (value).PadRight(32, '\0').ToCharArray();
				fmt.dwMask = CFM_FACE;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetFontName(SelectedRtf, value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float SelectionFontSize
		{
			get
			{
				return ((float)GetSelectionCharFormat().yHeight) / 20;
				//return startingUp ? 8.25f : FontParser.GetFontSize(SelectedRtf);
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.yHeight = (int)(value * 20);
				fmt.dwMask = CFM_SIZE;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetFontSize(SelectedRtf, value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SelectionFontVisible
		{
			get
			{
				return !FontParser.GetProperty(SelectedRtf, "v");
			}
			set
			{
				SelectedRtf = FontParser.SetProperty(SelectedRtf, "v", !value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SelectionFontBold
		{
			get
			{
				return (GetSelectionCharFormat().dwEffects & CFE_BOLD) == CFE_BOLD;
				//return (!startingUp) && FontParser.GetProperty(SelectedRtf, "b");
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.dwEffects = value ? CFE_BOLD : 0;
				fmt.dwMask = CFM_BOLD;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetProperty(SelectedRtf, "b", value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SelectionFontItalic
		{
			get
			{
				return (GetSelectionCharFormat().dwEffects & CFE_ITALIC) == CFE_ITALIC;
				//return FontParser.GetProperty(SelectedRtf, "i");
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.dwEffects = value ? CFE_ITALIC : 0;
				fmt.dwMask = CFM_ITALIC;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetProperty(SelectedRtf, "i", value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SelectionFontUnderline
		{
			get
			{
				return (GetSelectionCharFormat().dwEffects & CFE_UNDERLINE) == CFE_UNDERLINE;
				//return FontParser.GetProperty(SelectedRtf, "ul");
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.dwEffects = value ? CFE_UNDERLINE : 0;
				fmt.dwMask = CFM_UNDERLINE;
				SetSelectionCharFormat(fmt);
				//SelectedRtf = FontParser.SetProperty(SelectedRtf, "ul", value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string FontName
		{
			get
			{
				return new string(GetAllCharFormat().szFaceName).Trim('\0');
				//return FontParser.GetFontName(Rtf);
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.szFaceName = (value).PadRight(32, '\0').ToCharArray();
				fmt.dwMask = CFM_FACE;
				SetAllCharFormat(fmt);
				//SelectedRtf = FontParser.SetFontName(SelectedRtf, value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float FontSize
		{
			get
			{
				return ((float)GetAllCharFormat().yHeight) / 20;
				//return startingUp ? 8.25f : FontParser.GetFontSize(SelectedRtf);
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.yHeight = (int)(value * 20);
				fmt.dwMask = CFM_SIZE;
				SetAllCharFormat(fmt);
				//SelectedRtf = FontParser.SetFontSize(Rtf, value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FontVisible
		{
			get
			{
				return startingUp || !FontParser.GetProperty(Rtf, "v");
				//return getSelectionVisible();
			}
			set
			{
				if (!startingUp)
				{
					RememberSelectionPosition();
					Rtf = FontParser.SetProperty(Rtf, "v", !value);
					ReturnToSelectionPosition();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FontBold
		{
			get
			{
				return (GetAllCharFormat().dwEffects & CFE_BOLD) == CFE_BOLD;
				//return (!startingUp) && FontParser.GetProperty(SelectedRtf, "b");
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.dwEffects = value ? CFE_BOLD : 0;
				fmt.dwMask = CFM_BOLD;
				SetAllCharFormat(fmt);
				//SelectedRtf = FontParser.SetProperty(SelectedRtf, "b", value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FontItalic
		{
			get
			{
				return (GetAllCharFormat().dwEffects & CFE_ITALIC) == CFE_ITALIC;
				//return FontParser.GetProperty(SelectedRtf, "i");
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.dwEffects = value ? CFE_ITALIC : 0;
				fmt.dwMask = CFM_ITALIC;
				SetAllCharFormat(fmt);
				//SelectedRtf = FontParser.SetProperty(SelectedRtf, "i", value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FontUnderline
		{
			get
			{
				return (GetAllCharFormat().dwEffects & CFE_UNDERLINE) == CFE_UNDERLINE;
				//return FontParser.GetProperty(SelectedRtf, "ul");
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.dwEffects = value ? CFE_UNDERLINE : 0;
				fmt.dwMask = CFM_UNDERLINE;
				SetAllCharFormat(fmt);
				//SelectedRtf = FontParser.SetProperty(SelectedRtf, "ul", value);
			}
		}

		[System.ComponentModel.Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public byte SelectionUnderlineColorAndStyle
		{
			set
			{
				SetSelectionUnderlineColorAndStyle(value);
			}
		}

		[System.ComponentModel.Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public UnderlineColor SelectionUnderlineColor
		{
			set
			{
				SetSelectionUnderlineColorAndStyle((byte)((byte)RichTextBoxEx.UnderlineStyle.Normal | (byte)value));
			}
		}

		[System.ComponentModel.Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionUnderlineColorIndex
		{
			set
			{
				SetSelectionUnderlineColorAndStyle((byte)((byte)RichTextBoxEx.UnderlineStyle.Normal | (byte)(value << 4)));
			}
		}

		[System.ComponentModel.Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionFontLang
		{
			get
			{
				return GetSelectionCharFormat().lcid;
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.lcid = value;
				fmt.dwMask = CFM_LCID;
				SetSelectionCharFormat(fmt);
			}
		}

		[System.ComponentModel.Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int FontLang
		{
			get
			{
				return GetAllCharFormat().lcid;
			}
			set
			{
				CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
				fmt.lcid = value;
				fmt.dwMask = CFM_LCID;
				SetAllCharFormat(fmt);
			}
		}



		//Includes the IDs of undo action (ie. group actions that should be undone as a group)
		public Stack<CustomAction> CustomUndoStack = new Stack<CustomAction>();
		public Stack<CustomAction> CustomRedoStack = new Stack<CustomAction>();

		public void CustomUndo()
		{
			if (CanUndo)
			{
				string undoAction = UndoActionName;
				int se = SelectionStart + SelectionLength;
				Undo();
				if (undoAction.Equals("Unknown") && CustomUndoStack.Count > 0 && SelectionStart == CustomUndoStack.Peek().selStart)
				{
					CustomAction customUndo = CustomUndoStack.Pop();
					CustomRedoStack.Push(customUndo);
					if (customUndo.count == 1)
					{
						customUndo.selEnd = se;
					}
					for (int i = 1; i < customUndo.count; i++)
					{
						if (CanUndo && UndoActionName.Equals("Unknown"))
						{
							if (i == customUndo.count - 1)
							{
								customUndo.selEnd = SelectionStart + SelectionLength;
							}
							Undo();
						}
					}
				}
			}
		}

		public void CustomRedo()
		{
			if (CanRedo)
			{
				string redoAction = RedoActionName;
				Redo();
				if (redoAction.Equals("Unknown") && CustomRedoStack.Count > 0 && SelectionStart == CustomRedoStack.Peek().selEnd)
				{
					CustomAction customRedo = CustomRedoStack.Pop();
					CustomUndoStack.Push(customRedo);
					for (int i = 0; i < customRedo.count - 1; i++)
					{
						if (CanRedo && RedoActionName.Equals("Unknown"))
						{
							Redo();
						}
					}
				}
			}
		}

		public void CombineCustomUndoActions(int count)
		{
			CustomUndoStack.Push(new CustomAction(SelectionStart, count));
		}

		private void EmptyUndoStack()
		{
			CustomUndoStack.Clear();
		}

		private void EmptyRedoStack()
		{
			CustomRedoStack.Clear();
		}

		public void DoubleAction()
		{
			int selStart = SelectionStart;
			SelectedText = "aaaa";
			Select(selStart, 4);
			//SelectionFontSize = 12;
			SelectionUnderlineColorAndStyle = (byte)((byte)RichTextBoxEx.UnderlineStyle.Normal | (byte)RichTextBoxEx.UnderlineColor.Red);
			CombineCustomUndoActions(2);
		}

		public class CustomAction
		{
			public int selStart;
			public int selEnd;
			public int count;

			public CustomAction(int selStart, int count)
			{
				this.selStart = selStart;
				this.count = count;
			}
		}

		#region High-level: Set and store underline and link styles

		public void InsertLink(string text, string hyperlink)
		{
			InsertLink(text, hyperlink, this.SelectionStart);
		}

		public void InsertLink(string text, string hyperlink, int position)
		{
			if (position < 0 || position > this.Text.Length)
				throw new ArgumentOutOfRangeException("position");

			this.SelectionStart = position;
			this.SelectedRtf = @"{\rtf1\ansi " + text + @"\v `" + hyperlink + @"\v0}";
			this.Select(position, text.Length + hyperlink.Length + 1);
			this.SetSelectionLink(true);
			this.Select(position + text.Length + hyperlink.Length + 1, 0);
		}

		// Insert a given text as a link into the RichTextBox at the current insert position.
		public void InsertLink(string text)
		{
			InsertLink(text, this.SelectionStart);
		}

		// Insert a given text at a given position as a link. 
		public void InsertLink(string text, int position)
		{
			if (position < 0 || position > this.Text.Length)
				throw new ArgumentOutOfRangeException("position");

			this.SelectionStart = position;
			this.SelectedText = text;
			this.Select(position, text.Length);
			this.SetSelectionLink(true);
			this.Select(position + text.Length, 0);
		}



		public string GetClippedRtf(int pos, int endPos)
		{
			return LangParser.GetClippedRtf(Rtf, pos, endPos);// + 1
		}

		public int getLangAt(int pos)
		{
			return LangParser.GetLangAt(Rtf, pos);
		}

		//public byte getUnderlineStyle(int pos)
		//{
		//	return convertTwoHexDigitsToByte(getTagIfExists('u', pos).text);
		//}

		#endregion

		#region Med-level: Set underline and link styles on display

		private UnderlineStyle getSelectionUnderlineStyle()
		{
			CHARFORMAT2_STRUCT fmt = GetSelectionCharFormat();

			// Default to no underline.
			if ((fmt.dwMask & CFM_UNDERLINETYPE) == 0)
				return UnderlineStyle.None;

			byte style = (byte)(fmt.bUnderlineType & 0x0F);

			return (UnderlineStyle)style;
		}

		private void setSelectionUnderlineStyle(UnderlineStyle value)
		{
			// Ensure we don't alter the color by accident.
			UnderlineColor color = getSelectionUnderlineColor();

			// Ensure we don't show it if it shouldn't be shown.
			if (value == UnderlineStyle.None)
				color = UnderlineColor.Black;

			CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
			fmt.dwMask = CFM_UNDERLINETYPE;
			fmt.bUnderlineType = (byte)((byte)value | (byte)color);

			SetSelectionCharFormat(fmt);
		}

		private UnderlineColor getSelectionUnderlineColor()
		{
			CHARFORMAT2_STRUCT fmt = GetSelectionCharFormat();

			// Default to black.
			if ((fmt.dwMask & CFM_UNDERLINETYPE) == 0)
				return UnderlineColor.Black;

			byte style = (byte)(fmt.bUnderlineType & 0xF0);

			return (UnderlineColor)style;
		}

		public void SetSelectionUnderlineColor(int value)
		{
			// Ensure we don't alter the style.
			UnderlineStyle style = UnderlineStyle.Normal; // getSelectionUnderlineStyle();

			// Ensure we don't show it if it shouldn't be shown.
			//if (style == UnderlineStyle.None)
			//	value = UnderlineColor.Black;

			CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();

			fmt.dwMask = CFM_UNDERLINETYPE;
			fmt.bUnderlineType = (byte)((byte)style | (byte)(value << 4));

			SetSelectionCharFormat(fmt);
		}

		private byte GetSelectionUnderlineColorAndStyle()
		{
			return GetSelectionCharFormat().bUnderlineType;
		}

		private void SetSelectionUnderlineColorAndStyle(byte value)
		{
			CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();
			fmt.bUnderlineType = value;
			fmt.bAnimation = (byte)((value & 0xF0) >> 4);
			fmt.dwMask = CFM_UNDERLINETYPE | CFM_ANIMATION; //Animation simply stores it in rtf.
			SetSelectionCharFormat(fmt);
		}

		public void SetSelectionLink(bool link)
		{
			setSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
		}

		public void SetAllLink(bool link)
		{
			setAllStyle(CFM_LINK, link ? CFE_LINK : 0);
		}

		public int GetSelectionLink()
		{
			return getSelectionStyle(CFM_LINK, CFE_LINK);
		}

		#endregion


		public void testFunction(byte value)
		{
			CHARFORMAT2_STRUCT fmt = GetSelectionCharFormat();
			//fmt.yHeight = 300;
			//fmt.dwMask = CFM_SIZE;
			//fmt.sSpacing = 5000;
			//fmt.dwMask = CFM_WEIGHT;
			//fmt.dwMask = CFM_FACE;
			//char[] c = ("Times New Roman\0                ").ToCharArray();
			//fmt.szFaceName = c;

			fmt.wWeight = 1000;
			fmt.dwMask = CFM_WEIGHT;

			SetSelectionCharFormat(fmt);
		}


		#region Low-level: Get and set selection Char format

		private CHARFORMAT2_STRUCT GetNewFormatStruct()
		{
			CHARFORMAT2_STRUCT fmt = new CHARFORMAT2_STRUCT();
			fmt.cbSize = (UInt32)Marshal.SizeOf(fmt);
			fmt.szFaceName = new char[32];
			return fmt;
		}

		public string getRepOfCharFmt()
		{
			return ConvertCharFmtToString(GetSelectionCharFormat());

		}

		private string ConvertCharFmtToString(CHARFORMAT2_STRUCT fmt)
		{
			return ConvertUInt32ToHex(fmt.cbSize) + "\r\n" +
				ConvertUInt32ToHex(fmt.dwMask) + "\r\n" +
				ConvertUInt32ToHex(fmt.dwEffects) + "\r\n" +
				ConvertUInt32ToHex((UInt32)fmt.yHeight) + "\r\n" +
				ConvertUInt32ToHex((UInt32)fmt.yOffset) + "\r\n" +
				ConvertUInt32ToHex((UInt32)fmt.crTextColor) + "\r\n" +
				convertByteToHex(fmt.bCharSet) + "\r\n" +
				convertByteToHex(fmt.bPitchAndFamily) + "\r\n" +
				fromCharArray(fmt.szFaceName) + "\r\n" +
				ConvertUInt16ToHex(fmt.wWeight) + "\r\n" +
				ConvertUInt16ToHex(fmt.sSpacing) + "\r\n" +
				ConvertUInt32ToHex((UInt32)fmt.crBackColor) + "\r\n" +
				ConvertUInt32ToHex((UInt32)fmt.lcid) + "\r\n" +
				ConvertUInt32ToHex((UInt32)fmt.dwReserved) + "\r\n" +
				ConvertUInt16ToHex((UInt16)fmt.sStyle) + "\r\n" +
				ConvertUInt16ToHex((UInt16)fmt.wKerning) + "\r\n" +
				convertByteToHex(fmt.bUnderlineType) + "\r\n" +
				convertByteToHex(fmt.bAnimation) + "\r\n" +
				convertByteToHex(fmt.bRevAuthor) + "\r\n" +
				convertByteToHex(fmt.bReserved1);
		}

		public string fromCharArray(char[] array)
		{
			return new string(array).Trim('\0');
		}

		public string ConvertUInt32ToHex(UInt32 value)
		{
			return ConvertUInt16ToHex((UInt16)(value >> 16)) + ConvertUInt16ToHex((UInt16)(value & 0xFFFF));
		}

		public string ConvertUInt16ToHex(UInt16 value)
		{
			return convertByteToHex((byte)(value >> 8)) + convertByteToHex((byte)(value & 0xFF));
		}

		private CHARFORMAT2_STRUCT GetSelectionCharFormat()
		{
			return GetCharFormat(SCF_SELECTION);
		}

		private CHARFORMAT2_STRUCT GetAllCharFormat()
		{
			return GetCharFormat(SCF_ALL);
		}

		private CHARFORMAT2_STRUCT GetCharFormat(int scfType)
		{
			CHARFORMAT2_STRUCT fmt = GetNewFormatStruct();

			IntPtr wpar = new IntPtr(scfType);
			IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmt));
			Marshal.StructureToPtr(fmt, lpar, false);

			IntPtr res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);
			fmt = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

			Marshal.FreeCoTaskMem(lpar);

			return fmt;
		}

		private void SetAllCharFormat(CHARFORMAT2_STRUCT fmt)
		{
			SetCharFormat(fmt, SCF_ALL);
		}

		private void SetSelectionCharFormat(CHARFORMAT2_STRUCT fmt)
		{
			SetCharFormat(fmt, SCF_SELECTION);
		}

		private void SetCharFormat(CHARFORMAT2_STRUCT fmt, int scfType)
		{
			IntPtr wpar = new IntPtr(scfType);
			IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmt));
			Marshal.StructureToPtr(fmt, lpar, false);

			IntPtr res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

			Marshal.FreeCoTaskMem(lpar);
		}

		private int getSelectionStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = GetSelectionCharFormat();

			int state;
			// dwMask holds the information which properties are consistent throughout the selection:
			if ((cf.dwMask & mask) == mask)
			{
				if ((cf.dwEffects & effect) == effect)
					state = 1;
				else
					state = 0;
			}
			else
			{
				state = -1;
			}

			return state;
		}

		private void setSelectionStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = GetNewFormatStruct();
			cf.dwMask = mask;
			cf.dwEffects = effect;

			SetSelectionCharFormat(cf);
		}

		private void setAllStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = GetNewFormatStruct();
			cf.dwMask = mask;
			cf.dwEffects = effect;

			SetAllCharFormat(cf);
		}

		#endregion

		public void setFont(Font font, Color fontColor)
		{
			/*int i = getCornerIndex();
			this.SelectAll();
			this.SelectionFont = font;
			this.SelectionColor = fontColor;
			this.Select(i, 0);
			this.ScrollToCaret();*/
		}

		public int getCornerIndex()
		{
			return this.GetCharIndexFromPosition(new Point(0, 1));
		}

		public int selStart;
		public int selLength;

		public void RememberSelectionPosition()
		{
			selStart = SelectionStart;
			selLength = SelectionLength;
		}

		public void ReturnToSelectionPosition()
		{
			Select(selStart, selLength);
		}

		public bool ContainsRtf(Font preferredFont, Color preferredFontColor)
		{
			return ContainsRtfParser.ContainsRtf(Rtf, preferredFont, preferredFontColor);
		}


		// Footnotes and hyperlinks

		Panel tooltipPanel;
		RichTextBoxEx richTextBoxEx2;

		TreeNode nodeToLinkTo;
		Graphics tooltipGraphics;
		int minWidth = 40;
		int minHeight = 30;
		private void showTooltip(Point location, string text, string rtf, int approxTextHeightInPixels)
		{
			if (tooltipPanel == null)
			{
				richTextBoxEx2 = new RichTextBoxEx();
				richTextBoxEx2.BorderStyle = BorderStyle.None;
				richTextBoxEx2.Location = new Point(1, 1);
				richTextBoxEx2.MouseDown += new MouseEventHandler(richTextBoxEx2_MouseDown);
				richTextBoxEx2.TextChanged += new EventHandler(richTextBoxEx2_TextChanged);
				richTextBoxEx2.LinkClicked += new LinkClickedEventHandler(richTextBoxEx2_LinkClicked);

				tooltipPanel = new Panel();
				tooltipPanel.BackColor = Color.Black;
				tooltipPanel.Controls.Add(richTextBoxEx2);
				tooltipPanel.MouseLeave += new EventHandler(tooltipPanel_MouseLeave);
				tooltipPanel.MouseEnter += new EventHandler(tooltipPanel_MouseEnter);
				tooltipPanel.Visible = false;
				tooltipGraphics = tooltipPanel.CreateGraphics();

				Controls.Add(tooltipPanel);
			}
			int spacing = 5;

			tooltipPanel.Location = new Point(location.X - tooltipPanel.Width / 4, location.Y + approxTextHeightInPixels + spacing);
			if (tooltipPanel.Left < 0)
				tooltipPanel.Left = 0;
			if (tooltipPanel.Right > Width)
				tooltipPanel.Left = Width - tooltipPanel.Width;
			if (tooltipPanel.Bottom > Bottom)
				tooltipPanel.Top = location.Y - approxTextHeightInPixels - spacing - tooltipPanel.Height;

			MeasureTooltip(text);
			//if (rtf.Length > 0)
			//	richTextBoxEx2.Rtf = rtf;
			//else
			richTextBoxEx2.Text = text + " ";
			richTextBoxEx2.Select(0, text.Length);
			richTextBoxEx2.SetSelectionLink(true);
			richTextBoxEx2.Select(0, 0);
			tooltipPanel.Visible = true;
			tooltipAppearTime = DateTime.Now;
			leftTooltip = false;
			clickedOnTooltip = false;
		}

		private void RichTextBoxEx_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (onLinkClicked != null)
				onLinkClicked(nodeToLinkTo, e.LinkText);
		}


		private void richTextBoxEx2_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (onLinkClicked != null)
				onLinkClicked(nodeToLinkTo, e.LinkText);
		}

		public delegate void OnLinkClickedDelegate(TreeNode nodeToLinkTo, string linkText);
		event OnLinkClickedDelegate onLinkClicked;
		public event OnLinkClickedDelegate OnSubLinkClicked
		{
			add
			{
				onLinkClicked = (OnLinkClickedDelegate)Delegate.Combine(onLinkClicked, value);
			}
			remove
			{
				onLinkClicked = (OnLinkClickedDelegate)Delegate.Remove(onLinkClicked, value);
			}
		}

		private void richTextBoxEx2_TextChanged(object sender, EventArgs e)
		{
			MeasureTooltip(richTextBoxEx2.Text);
		}

		private void MeasureTooltip(string text)
		{
			Size minTextSize = tooltipGraphics.MeasureString(text, richTextBoxEx2.Font, 150).ToSize();
			Size size = new Size(Math.Max(minTextSize.Width + 2, minWidth), Math.Max(minTextSize.Height + 2, minHeight));
			richTextBoxEx2.Size = new Size(size.Width - 2, size.Height - 2);
			tooltipPanel.Size = size;
		}

		private void hideAndSaveTooltip()
		{
			int selStart = SelectionStart;
			int selLength = SelectionLength;
			tooltipPanel.Visible = false;
			//string rtf = Rtf;
			//rtf = richTextBoxEx1.removeText(rtf, activeFootNote.location, activeFootNote.text.Length);
			//rtf = richTextBoxEx1.insertText(rtf, richTextBoxEx2.Text, activeFootNote.location);
			//Rtf = rtf;

			//int diff = richTextBoxEx2.Text.Length - activeFootNote.text.Length;
			//richTextBoxEx1.SelectionStart = selStart + ((selStart > activeFootNote.location) ? diff : 0);
			SelectionLength = selLength;
		}

		DateTime tooltipLeaveTime;
		bool leftTooltip;
		bool clickedOnTooltip;
		DateTime tooltipAppearTime;
		long toolTipTimeToDisappear = 800; //in ms.
		long minAppearTime = 200; //in ms.
		Point mouseLocation;

		private void RichTextBoxEx_MouseMove(object sender, MouseEventArgs e)
		{
			if (scroll)
			{
				RefreshClippedRtf();
			}
			int pos2 = GetCharIndexFromPosition(e.Location);
			int j = GetCharIndexFromPosition(new Point(Width - 100, Height - 4)) + 1;
			if (clipStartIndex != getCornerIndex()
				|| clipEndIndex != GetCharIndexFromPosition(new Point(Width, Height - 4)) + 1)
			{
				RefreshClippedRtf();
			}

			mouseLocation = e.Location;
			if (tooltipPanel == null || !tooltipPanel.Visible)
			{
				int pos = GetCharIndexFromPosition(e.Location);
				Form1.StartTimer();

				bool useClipVersion = (clippedRtf != null && pos >= clipStartIndex && pos < clipEndIndex);
				if (clippedRtf != null && !useClipVersion)
				{
					int aax = 0;
				}
				string test = LangParser.GetNextChars(
					useClipVersion ? clippedRtf : Rtf,
					useClipVersion ? pos - clipStartIndex : pos, 10);
				if (pos == 5)
				{
					int zzz = 0;
				}
				setDebugValue("Nearby Rtf", test);
				if (clippedRtf != null && clippedRtf.Length > 50)
					setDebugValue("Clip rtf", clippedRtf.Substring(0, 50));
				int lang = LangParser.GetLangAt(
					useClipVersion ? clippedRtf : Rtf,
					useClipVersion ? pos - clipStartIndex : pos);
				setDebugValue("Lang", "" + lang);
				Form1.ShowTime("GetLangAt");
				if (lang != 0 && lang != 1033)
				{
					//Text`f'abc'def`f0
					int approxTextHeightInPixels = (int)(Font.Size * 1.5);
					Point charLocation = GetPositionFromCharIndex(pos);
					Point toolTipLocation = new Point(charLocation.X, charLocation.Y);
					setDebugValue("Show Tooltip", "true");
					nodeToLinkTo = Form1.FindChildById(lang % 5000);
					if (nodeToLinkTo != null && lang < 5000)
					{
						showTooltip(toolTipLocation, nodeToLinkTo.Text, "", approxTextHeightInPixels);
					}
				}
				else
				{
					setDebugValue("Show Tooltip", "false");
				}
			}
			else if (leftTooltip && tooltipPanel.Visible && !clickedOnTooltip)
			{
				TimeSpan leftTime = DateTime.Now.Subtract(tooltipLeaveTime);
				if (leftTime.Ticks > toolTipTimeToDisappear * 10000)
				{
					hideAndSaveTooltip();
					setDebugValue("Show Tooltip", "false");
				}
			}
		}

		private void setDebugValue(String key, String value)
		{
			Form1.SetDebugValue(key, value);
		}

		public void SelChange()
		{
			/*int pos = richTextBoxEx1.SelectionStart;
			bool useClipVersion = (clippedRtf != null && pos >= clipStartIndex && pos < clipEndIndex);
			if (useClipVersion)
			{
				int aax = 0;
			}
			setDebugValue("clipStartIndex", "" + (clipStartIndex));
			setDebugValue("posn", "" + (pos));
			setDebugValue("relative position", "" + (pos - clipStartIndex));
			setDebugValue("used clipped version", useClipVersion ? "true" : "false");
			string test = LangParser.GetNextChars(
				useClipVersion ? clippedRtf : richTextBoxEx1.Rtf,
				useClipVersion ? pos - clipStartIndex : pos, 10);
			setDebugValue("Nearby Rtf", test);
			setDebugValue("Clip rtf", clippedRtf);*/
		}

		string clippedRtf;
		int clipStartIndex;
		int clipEndIndex;
		bool scroll;

		private void richTextBoxEx1_VScroll(object sender, EventArgs e)
		{
			scroll = true;
		}

		private void tooltipPanel_MouseLeave(object sender, EventArgs e)
		{
			leftTooltip = true;
			tooltipLeaveTime = DateTime.Now;
		}

		private void tooltipPanel_MouseEnter(object sender, EventArgs e)
		{
			leftTooltip = false;
		}

		private void richTextBoxEx1_MouseDown(object sender, MouseEventArgs e)
		{
			//mouseDownEvent = true;
			if (tooltipPanel != null && tooltipPanel.Visible && DateTime.Now.Subtract(tooltipAppearTime).Ticks > minAppearTime * 10000)
			{
				hideAndSaveTooltip();
			}
		}

		private void richTextBoxEx2_MouseDown(object sender, MouseEventArgs e)
		{
			clickedOnTooltip = true;
		}

		private void RichTextBoxEx_TextChanged(object sender, EventArgs e)
		{
			if(tooltipPanel != null)
				tooltipPanel.Visible = false;
			RefreshClippedRtf();
		}

		private void RefreshClippedRtf()
		{
			Form1.StartTimer();
			clipStartIndex = getCornerIndex();
			clipEndIndex = GetCharIndexFromPosition(new Point(Width, Height - 4)) + 1;
			clippedRtf = GetClippedRtf(clipStartIndex, clipEndIndex);
			scroll = false;
			Form1.ShowTime("Time to clip rtf");
		}

		private void RichTextBoxEx_KeyDown(object sender, KeyEventArgs e)
		{
			if (tooltipPanel != null && tooltipPanel.Visible)
			{
				tooltipPanel.Visible = false;
			}
		}
	}
}
