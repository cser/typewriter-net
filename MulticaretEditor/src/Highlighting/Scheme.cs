using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;

namespace MulticaretEditor.Highlighting
{
	public class Scheme : Dictionary<Ds, TextStyle>
	{
		public Scheme()
		{
			Reset();
			Update();
		}
		
		private readonly TextStyle defaultTextStyle = new TextStyle();
		
		public Color bgColor;
		public Color fgColor;
		public Color lineBgColor;
		public Color lineNumberBgColor;
		public Color lineNumberFgColor;
		public Color selectionBrushColor;
		public Color selectionPenColor;
		public Color markPenColor;
		public Color mainCaretColor;
		public Color caretColor;
		public Color printMarginColor;
		
		public Color separatorColor;
		public Color selectedSeparatorColor;
		
		public Color scrollBgColor;
		public Color scrollThumbColor;
		public Color scrollThumbHoverColor;
		public Color scrollArrowColor;
		public Color scrollArrowHoverColor;
		
		public Color splitterLineColor;
		
		public int mainCaretWidth;
		public int caretWidth;
		
		public Brush bgBrush;
		public Pen bgPen;
		public Brush fgBrush;
		public Pen fgPen;
		public Brush lineBgBrush;
		public Brush selectionBrush;
		public Pen selectionPen;
		public Pen markPen;
		public Pen mainCaretPen;
		public Pen caretPen;
		public Brush lineNumberBackground;
		public Brush lineNumberForeground;
		public Pen lineNumberFgPen;
		public Pen printMarginPen;
		
		public Pen splitterLinePen;
		public Brush scrollBgBrush;
		public Brush scrollThumbBrush;
		public Brush scrollThumbHoverBrush;
		public Pen scrollArrowPen;
		public Pen scrollArrowHoverPen;
		
		public void ParseXml(IEnumerable<XmlDocument> xmls)
		{
			Reset();
			
			Dictionary<string, Color> defColors = new Dictionary<string, Color>();
			Dictionary<string, Color> colors = new Dictionary<string, Color>();
			Dictionary<string, int> widths = new Dictionary<string, int>();
			
			foreach (XmlDocument xml in xmls)
			{
				foreach (XmlNode node in xml.ChildNodes)
				{	
					XmlElement root = node as XmlElement;
					if (root == null || root.Name != "scheme")
						continue;
					foreach (XmlNode nodeI in root.ChildNodes)
					{
						XmlElement elementI = nodeI as XmlElement;
						if (elementI == null)
							continue;
						if (elementI.Name == "defColor")
						{
							string name = elementI.GetAttribute("name");
							string value = elementI.GetAttribute("value");
							if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
							{
								Color? color = HighlighterUtil.ParseColor(value);
								if (color != null)
									defColors[name] = color.Value;
							}
						}
						else if (elementI.Name == "style")
						{
							Ds ds = Ds.GetByName(elementI.GetAttribute("name"));
							TextStyle style = new TextStyle();
							Color? color = ParseColorWithDefs(elementI.GetAttribute("color"), defColors);
							if (color != null)
								style.brush = new SolidBrush(color.Value);
							style.Italic = elementI.GetAttribute("italic") == "true";
							style.Bold = elementI.GetAttribute("bold") == "true";
							style.Underline = elementI.GetAttribute("underline") == "true";
							style.Strikeout = elementI.GetAttribute("strikeout") == "true";
							this[ds] = style;
						}
						else if (elementI.Name == "color")
						{
							string name = elementI.GetAttribute("name");
							string value = elementI.GetAttribute("value");
							if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
							{
								Color? color = ParseColorWithDefs(value, defColors);
								if (color != null)
									colors[name] = color.Value;
							}
						}
						else if (elementI.Name == "width")
						{
							string name = elementI.GetAttribute("name");
							string value = elementI.GetAttribute("value");
							if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
							{
								int intValue;
								if (int.TryParse(value, out intValue))
									widths[name] = intValue;
							}
						}
					}
					break;
				}
			}
			
			SetColor(ref bgColor, "bg", colors);
			SetColor(ref fgColor, "fg", colors);
			SetColor(ref lineBgColor, "lineBg", colors);
			SetColor(ref lineNumberBgColor, "lineNumberBg", colors);
			SetColor(ref lineNumberFgColor, "lineNumberFg", colors);
			SetColor(ref selectionBrushColor, "selectionBrush", colors);
			SetColor(ref selectionPenColor, "selectionPen", colors);
			SetColor(ref markPenColor, "markPen", colors);
			SetColor(ref mainCaretColor, "mainCaret", colors);
			SetColor(ref caretColor, "caret", colors);
			SetColor(ref printMarginColor, "printMargin", colors);
			SetWidth(ref mainCaretWidth, "mainCaret", widths);
			SetWidth(ref caretWidth, "caret", widths);
			
			SetColor(ref separatorColor, "separator", colors);
			SetColor(ref selectedSeparatorColor, "selectedSeparator", colors);
			
			SetColor(ref splitterLineColor, "splitterLine", colors);
			SetColor(ref scrollBgColor, "scrollBg", colors);
			SetColor(ref scrollThumbColor, "scrollThumb", colors);
			SetColor(ref scrollThumbHoverColor, "scrollThumbHover", colors);
			SetColor(ref scrollArrowColor, "scrollArrow", colors);
			SetColor(ref scrollArrowHoverColor, "scrollArrowHover", colors);
			
			Tabs_ParseXml(colors, widths);
			
			Update();
		}
		
		private static void SetColor(ref Color color, string name, Dictionary<string, Color> colors)
		{
			Color value;
			if (colors.TryGetValue(name, out value))
				color = value;
		}
		
		private static void SetWidth(ref int width, string name, Dictionary<string, int> widths)
		{
			int value;
			if (widths.TryGetValue(name, out value))
				width = value;
		}
		
		private static Color? ParseColorWithDefs(string raw, Dictionary<string, Color> colorByName)
		{
			Color color;
			if (colorByName.TryGetValue(raw, out color))
				return color;
			return HighlighterUtil.ParseColor(raw);
		}
		
		private void Reset()
		{
			bgColor = Color.White;
			lineBgColor = Color.FromArgb(230, 230, 240);			
			lineNumberBgColor = Color.FromArgb(228, 228, 228);
			lineNumberFgColor = Color.Gray;
			fgColor = Color.Black;
			selectionBrushColor = Color.FromArgb(220, 220, 255);
			selectionPenColor = Color.FromArgb(150, 150, 200);
			markPenColor = Color.FromArgb(150, 150, 200);
			mainCaretColor = Color.Black;
			caretColor = Color.Gray;
			printMarginColor = Color.Gray;
			mainCaretWidth = 1;
			caretWidth = 1;
			
			separatorColor = Color.Gray;
			selectedSeparatorColor = Color.White;
			
			splitterLineColor = Color.Gray;
			scrollBgColor = Color.WhiteSmoke;
			scrollThumbColor = Color.FromArgb(180, 180, 180);
			scrollThumbHoverColor = Color.FromArgb(100, 100, 200);
			scrollArrowColor = Color.Black;
			scrollArrowHoverColor = Color.FromArgb(50, 50, 255);
			
			Tabs_Reset();
			
			Clear();
			defaultTextStyle.brush = new SolidBrush(fgColor);
			foreach (Ds ds in Ds.all)
			{
				this[ds] = defaultTextStyle;
			}
		}
		
		public void Update()
		{
			bgBrush = new SolidBrush(bgColor);
			bgPen = new Pen(bgColor);
			fgPen = new Pen(fgColor);
			fgBrush = new SolidBrush(fgColor);
			lineBgBrush = new SolidBrush(lineBgColor);
			lineNumberBackground = new SolidBrush(lineNumberBgColor);
			lineNumberForeground = new SolidBrush(lineNumberFgColor);
			lineNumberFgPen = new Pen(lineNumberFgColor);
			selectionBrush = new SolidBrush(selectionBrushColor);
			selectionPen = new Pen(selectionPenColor, 2);
			markPen = new Pen(markPenColor, 2);
			mainCaretPen = new Pen(mainCaretColor, mainCaretWidth);
			caretPen = new Pen(caretColor, caretWidth);
			printMarginPen = new Pen(printMarginColor);
			
			splitterLinePen = new Pen(splitterLineColor, 1);
			scrollBgBrush = new SolidBrush(scrollBgColor);
			scrollThumbBrush = new SolidBrush(scrollThumbColor);
			scrollThumbHoverBrush = new SolidBrush(scrollThumbHoverColor);
			scrollArrowPen = new Pen(scrollArrowColor, 1);
			scrollArrowHoverPen = new Pen(scrollArrowHoverColor, 1);
			
			defaultTextStyle.brush = new SolidBrush(fgColor);
			
			Tabs_Update();
		}
		
		public Color tabsBgColor;
		public Color tabsFgColor;
		public Color tabsSelectedBgColor;
		public Color tabsSelectedFgColor;
		public Color tabsLineColor;
		public Color tabsLineFgColor;
		public Color lineSeparatorColor;
		public int tabsLineWidth;
		
		public Brush tabsBgBrush;
		public Brush tabsFgBrush;
		public Pen tabsFgPen;		
		public Brush tabsSelectedBgBrush;
		public Brush tabsSelectedFgBrush;
		public Pen tabsSelectedFgPen;
		public Brush tabsLineBrush;
		public Brush tabsLineFgBrush;
		public Pen lineSeparatorPen;
		
		private void Tabs_Reset()
		{
			tabsBgColor = Color.WhiteSmoke;
			tabsFgColor = Color.Black;
			tabsSelectedBgColor = Color.Gray;
			tabsSelectedFgColor = Color.White;
			tabsLineColor = Color.Black;
			tabsLineFgColor = Color.White;
			lineSeparatorColor = Color.Gray;
			tabsLineWidth = 0;
		}
		
		private void Tabs_ParseXml(Dictionary<string, Color> colors, Dictionary<string, int> widths)
		{
			SetColor(ref tabsBgColor, "tabsBg", colors);
			SetColor(ref tabsFgColor, "tabsFg", colors);
			SetColor(ref tabsSelectedBgColor, "tabsSelectedBg", colors);
			SetColor(ref tabsSelectedFgColor, "tabsSelectedFg", colors);
			SetColor(ref tabsLineColor, "tabsLine", colors);
			SetColor(ref tabsLineFgColor, "tabsLineFg", colors);
			SetColor(ref lineSeparatorColor, "lineSeparator", colors);
			SetWidth(ref tabsLineWidth, "tabsLine", widths);
		}
		
		private void Tabs_Update()
		{
			tabsBgBrush = new SolidBrush(tabsBgColor);
			tabsFgBrush = new SolidBrush(tabsFgColor);
			tabsFgPen = new Pen(tabsFgColor);
			tabsSelectedBgBrush = new SolidBrush(tabsSelectedBgColor);
			tabsSelectedFgPen = new Pen(tabsSelectedFgColor);
			tabsSelectedFgBrush = new SolidBrush(tabsSelectedFgColor);
			tabsLineBrush = new SolidBrush(tabsLineColor);
			tabsLineFgBrush = new SolidBrush(tabsLineFgColor);
			lineSeparatorPen = new Pen(lineSeparatorColor);
		}
	}
}
