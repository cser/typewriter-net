using System;
using System.Drawing;

namespace MulticaretEditor.Highlighting
{
	public class TextStyle
	{
		public TextStyle()
		{
		}
		
		public SolidBrush brush = new SolidBrush(Color.Black);
		public int fontStyle;
		
		public const int NoneMask = 0;
		public const int ItalicMask = 1;
		public const int BoldMask = 2;
		public const int UnderlineMask = 4;
		public const int StrikeoutMask = 8;
		
		public bool Italic
		{
			get { return (fontStyle & ItalicMask) != 0; }
			set { fontStyle = (fontStyle & (~ItalicMask)) | (value ? ItalicMask : 0); }
		}
		
		public bool Bold
		{
			get { return (fontStyle & BoldMask) != 0; }
			set { fontStyle = (fontStyle & (~BoldMask)) | (value ? BoldMask : 0); }
		}
		
		public bool Underline
		{
			get { return (fontStyle & UnderlineMask) != 0; }
			set { fontStyle = (fontStyle & (~UnderlineMask)) | (value ? UnderlineMask : 0); }
		}
		
		public bool Strikeout
		{
			get { return (fontStyle & StrikeoutMask) != 0; }
			set { fontStyle = (fontStyle & (~StrikeoutMask)) | (value ? StrikeoutMask : 0); }
		}
		
		public TextStyle Clone()
		{
			TextStyle style = new TextStyle();
			style.brush = new SolidBrush(brush.Color);
			style.fontStyle = fontStyle;
			return style;
		}
	}
}
