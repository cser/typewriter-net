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
		
		public Color splitterBgColor;
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
		
		public Brush splitterBgBrush;
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
			Dictionary<string, Color> colors2 = new Dictionary<string, Color>();
			Dictionary<string, int> widths = new Dictionary<string, int>();
			Dictionary<string, int> widths2 = new Dictionary<string, int>();
			
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
							string value2 = elementI.GetAttribute("value2");
							if (!string.IsNullOrEmpty(name))
							{
								if (!string.IsNullOrEmpty(value))
								{
									Color? color = ParseColorWithDefs(value, defColors);
									if (color != null)
										colors[name] = color.Value;
								}
								if (!string.IsNullOrEmpty(value2))
								{
									Color? color = ParseColorWithDefs(value2, defColors);
									if (color != null)
										colors2[name] = color.Value;
								}
							}
						}
						else if (elementI.Name == "width")
						{
							string name = elementI.GetAttribute("name");
							string value = elementI.GetAttribute("value");
							string value2 = elementI.GetAttribute("value2");
							if (!string.IsNullOrEmpty(name))
							{
								if (!string.IsNullOrEmpty(value))
								{
									int intValue;
									if (int.TryParse(value, out intValue))
										widths[name] = intValue;
								}
								if (!string.IsNullOrEmpty(value2))
								{
									int intValue;
									if (int.TryParse(value2, out intValue))
										widths[name] = intValue;
								}
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
			
			SetColor(ref splitterBgColor, "splitterBg", colors);
			SetColor(ref splitterLineColor, "splitterLine", colors);
			SetColor(ref scrollBgColor, "scrollBg", colors);
			SetColor(ref scrollThumbColor, "scrollThumb", colors);
			SetColor(ref scrollThumbHoverColor, "scrollThumbHover", colors);
			SetColor(ref scrollArrowColor, "scrollArrow", colors);
			SetColor(ref scrollArrowHoverColor, "scrollArrowHover", colors);
			
			Tabs_ParseXml(colors, colors2, widths, widths2);
			
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
			
			splitterBgColor = Color.WhiteSmoke;
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
			
			splitterBgBrush = new SolidBrush(splitterBgColor);
			splitterLinePen = new Pen(splitterLineColor, 1);
			scrollBgBrush = new SolidBrush(scrollBgColor);
			scrollThumbBrush = new SolidBrush(scrollThumbColor);
			scrollThumbHoverBrush = new SolidBrush(scrollThumbHoverColor);
			scrollArrowPen = new Pen(scrollArrowColor, 1);
			scrollArrowHoverPen = new Pen(scrollArrowHoverColor, 1);
			
			defaultTextStyle.brush = new SolidBrush(fgColor);
			
			Tabs_Update();
		}
		
		public class ColorItem
		{
			public readonly string name;
			
			public Color color;
			public Color color2;
			public Brush brush;
			public Brush brush2;
			public Pen pen;
			public Pen pen2;
			
			public ColorItem(string name)
			{
				this.name = name;
			}
			
			public void SetColor(Color color, Color color2)
			{
				this.color = color;
				this.color2 = color;
			}
			
			public Color GetColor(bool selected)
			{
				return selected ? color : color2;
			}
			
			public Brush GetBrush(bool selected)
			{
				return selected ? brush : brush2;
			}
			
			public Pen GetPen(bool selected)
			{
				return selected ? pen : pen2;
			}
			
			public void Update()
			{
				brush = new SolidBrush(color);
				brush2 = new SolidBrush(color2);
				pen = new Pen(color);
				pen2 = new Pen(color2);
			}
		}
		
		private static void SetColor(ColorItem item,
			Dictionary<string, Color> colors, Dictionary<string, Color> colors2)
		{
			Color value;
			if (colors.TryGetValue(item.name, out value))
				item.color = value;
			if (colors.TryGetValue(item.name, out value))
				item.color2 = value;
		}
		
		public readonly ColorItem tabsBg = new ColorItem("tabsBg");
		public readonly ColorItem tabsFg = new ColorItem("tabsFg");
		public readonly ColorItem tabsLine = new ColorItem("tabsLine");
		public readonly ColorItem tabsSeparator = new ColorItem("tabsSeparator");
		public int tabsLineWidth;
		
		private void Tabs_Reset()
		{
			tabsBg.SetColor(Color.WhiteSmoke, Color.Gray);
			tabsFg.SetColor(Color.Black, Color.White);
			tabsLine.SetColor(Color.Black, Color.Black);
			tabsSeparator.SetColor(Color.Gray, Color.Gray);
			tabsLineWidth = 0;
		}
		
		private void Tabs_ParseXml(Dictionary<string, Color> colors, Dictionary<string, Color> colors2,
			Dictionary<string, int> widths, Dictionary<string, int> widths2)
		{
			SetColor(tabsBg, colors, colors2);
			SetColor(tabsFg, colors, colors2);
			SetColor(tabsLine, colors, colors2);
			SetColor(tabsSeparator, colors, colors2);
			SetWidth(ref tabsLineWidth, "tabsLine", widths);
		}
		
		private void Tabs_Update()
		{
			tabsBg.Update();
			tabsFg.Update();
			tabsLine.Update();
			tabsSeparator.Update();
		}
	}
}
