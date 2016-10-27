using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using MulticaretEditor.KeyMapping;
using Microsoft.Win32;
using CustomScrollBar;

public class AutocompleteMenu : ToolStripDropDown
{
	private readonly AutocompleteMode.Handler handler;
	private readonly StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
	private readonly Scheme scheme;
	private readonly int charWidth;
	private readonly int charHeight;
	private readonly Font[] fonts = new Font[16];
	private readonly Font font;
	private readonly ToolStripControlHost host;
	private readonly MenuControl control;
	private readonly TextStyle defaultStyle;
	private readonly TextStyle typeStyle;
	private readonly int scrollingIndent;
	
	public readonly int maxLinesCount;
	
	public AutocompleteMenu(AutocompleteMode.Handler handler)
	{
		this.handler = handler;
		this.scheme = handler.TextBox.Scheme;
		this.scrollingIndent = handler.TextBox.ScrollingIndent;
		FontFamily family = handler.TextBox.FontFamily;
		float emSize = handler.TextBox.FontSize;
		
		fonts[TextStyle.NoneMask] = new Font(family, emSize);

		fonts[TextStyle.ItalicMask] = new Font(family, emSize, FontStyle.Italic);
		fonts[TextStyle.BoldMask] = new Font(family, emSize, FontStyle.Bold);
		fonts[TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Underline);
		fonts[TextStyle.StrikeoutMask] = new Font(family, emSize, FontStyle.Strikeout);

		fonts[TextStyle.ItalicMask | TextStyle.BoldMask] = new Font(family, emSize, FontStyle.Italic | FontStyle.Bold);
		fonts[TextStyle.ItalicMask | TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Italic | FontStyle.Underline);
		fonts[TextStyle.ItalicMask | TextStyle.StrikeoutMask] = new Font(family, emSize, FontStyle.Italic | FontStyle.Strikeout);

		fonts[TextStyle.BoldMask | TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Bold | FontStyle.Underline);
		fonts[TextStyle.BoldMask | TextStyle.StrikeoutMask] = new Font(family, emSize, FontStyle.Bold | FontStyle.Strikeout);

		fonts[TextStyle.StrikeoutMask | TextStyle.UnderlineMask] = new Font(family, emSize, FontStyle.Strikeout | FontStyle.Underline);

		fonts[TextStyle.ItalicMask | TextStyle.BoldMask | TextStyle.UnderlineMask] =
			new Font(family, emSize, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline);
		fonts[TextStyle.ItalicMask | TextStyle.BoldMask | TextStyle.StrikeoutMask] =
			new Font(family, emSize, FontStyle.Italic | FontStyle.Bold | FontStyle.Strikeout);
		fonts[TextStyle.ItalicMask | TextStyle.UnderlineMask | TextStyle.StrikeoutMask] =
			new Font(family, emSize, FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
		fonts[TextStyle.BoldMask | TextStyle.UnderlineMask | TextStyle.StrikeoutMask] =
			new Font(family, emSize, FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);

		fonts[TextStyle.ItalicMask | TextStyle.BoldMask | TextStyle.UnderlineMask | TextStyle.StrikeoutMask] =
			new Font(family, emSize, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);

		font = fonts[TextStyle.NoneMask];
		defaultStyle = scheme[Ds.Normal];
		typeStyle = scheme[Ds.DataType];

		SizeF size = GetCharSize(fonts[0], 'M');
		charWidth = (int)Math.Round(size.Width * 1f) - 1;
		charHeight = (int)Math.Round(size.Height * 1f) + 1;
		
		maxLinesCount = Math.Max(10, Screen.PrimaryScreen.Bounds.Height / (2 * charHeight) - 2);
		
		AutoClose = false;
		AutoSize = false;
		DropShadowEnabled = false;
		
		Margin = Padding.Empty;
		Padding = Padding.Empty;
		
		control = new MenuControl(this);
		host = new ToolStripControlHost(control);
		host.Margin = Padding.Empty;
		host.Padding = Padding.Empty;
		host.AutoSize = false;
		host.AutoToolTip = false;
		Items.Add(host);
	}
	
	private Timer timer;
	
	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);
		timer = new Timer();
		timer.Tick += OnTimerTick;
		timer.Interval = 100;
		timer.Start();
	}
	
	protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
	{
		timer.Stop();
		timer.Dispose();
		base.OnClosed(e);
	}
	
	private void OnTimerTick(object source, EventArgs e)
	{
		handler.CheckPosition();
	}
	
	private Point screenPoint;
	
	public void SetScreenPosition(Point point)
	{
		screenPoint = point;
	}
	
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		int delta = (int)Math.Round((float)e.Delta / 120f) * GetControlPanelWheelScrollLinesValue();
		control.Scroll(delta);
	}
	
	private static int GetControlPanelWheelScrollLinesValue()
	{
		try
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
			{
				return Convert.ToInt32(key.GetValue("WheelScrollLines"));
			}
		}
		catch
		{
			return 3;
		}
	}
	
	private static SizeF GetCharSize(Font font, char c)
	{
		Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", font);
		Size sz3 = TextRenderer.MeasureText("<>", font);
		return new SizeF(sz2.Width - sz3.Width + 1, font.Height);
	}
	
	private readonly List<Variant> variants = new List<Variant>();
	private Variant selectedVariant;
	
	private int visibleLinesCount;
	private int maxLength;
	
	public void SetVariants(List<Variant> variants)
	{
		this.variants.Clear();
		this.variants.AddRange(variants);
		
		maxLength = 0;
		for (int i = this.variants.Count; i-- > 0;)
		{
			int length = (this.variants[i].DisplayText ?? "").Length;
			if (maxLength < length)
				maxLength = length;
		}
		visibleLinesCount = this.variants.Count;
		if (visibleLinesCount > maxLinesCount)
			visibleLinesCount = maxLinesCount;
		
		bool scrollBarVisible = visibleLinesCount < this.variants.Count;
		int width = maxLength * charWidth + (scrollBarVisible ? control.scrollBarWidth : 0);
		int height = visibleLinesCount * charHeight;
		Size = new Size(width, height);
		host.Size = new Size(width, height);
		Invalidate();
		control.SetLogicSize(maxLength, visibleLinesCount, width, height, scrollBarVisible);
		control.Invalidate();
		UpdateScreenPosition();
	}
	
	public void UpdateScreenPosition()
	{
		int height = visibleLinesCount * charHeight;
		Left = screenPoint.X;
		if (height < Screen.PrimaryScreen.Bounds.Height - screenPoint.Y)
		{
			Top = screenPoint.Y;
		}
		else
		{
			Top = screenPoint.Y - height - charHeight;
		}
	}
	
	public void SetSelectedVariant(Variant variant)
	{
		selectedVariant = variant;
		Invalidate();
		control.ScrollIfNeed();
	}
	
	protected override void OnPaint(PaintEventArgs e)
	{
	}
	
	public class MenuControl : Control
	{
		private readonly AutocompleteMenu menu;
		private readonly ScrollBarEx vScrollBar;
		
		public int scrollBarWidth;
		
		public MenuControl(AutocompleteMenu menu)
		{
			this.menu = menu;
			
			Margin = Padding.Empty;
			Padding = Padding.Empty;
			
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			ImeMode = ImeMode.Off;
			
			SuspendLayout();
			
			vScrollBar = new ScrollBarEx(true, menu.scheme);
			vScrollBar.SmallChange = 1;
			vScrollBar.Cursor = Cursors.Default;
			vScrollBar.SmallChange = menu.charHeight;
			vScrollBar.Scroll += OnVScroll;
			vScrollBar.Visible = false;
			scrollBarWidth = vScrollBar.Width;
			Controls.Add(vScrollBar);

			ResumeLayout(false);
		}
		
		private int width;
		private int height;
		
		public void SetLogicSize(int columns, int lines, int width, int height, bool scrollBarVisible)
		{
			this.width = width;
			this.height = height;
			Size = new Size(width, height);
			vScrollBar.Visible = scrollBarVisible;
			vScrollBar.Left = width - scrollBarWidth;
			vScrollBar.Height = height;			
			vScrollBar.Minimum = 0;
			vScrollBar.Maximum = menu.variants.Count;
			vScrollBar.LargeChange = menu.visibleLinesCount;
		}
		
		public void Scroll(int delta)
		{
			vScrollBar.Value = CommonHelper.Clamp(
				vScrollBar.Value - delta,
				0,
				menu.variants.Count - menu.visibleLinesCount);
			Invalidate();
		}
		
		public void ScrollIfNeed()
		{
			if (menu.selectedVariant == null)
				return;
			int offset = vScrollBar.Value;
			int index = menu.variants.IndexOf(menu.selectedVariant);
			if (index == -1)
				return;
			int indent = Math.Max(0, Math.Min(menu.visibleLinesCount / 2 - 1, menu.scrollingIndent));
			if (index - indent < offset)
			{
				offset = index - indent;
			}
			else if (index > offset + menu.visibleLinesCount - 1 - indent)
			{
				offset = index - menu.visibleLinesCount + 1 + indent;
			}
			if (offset < 0)
			{
				offset = 0;
			}
			else if (offset > menu.variants.Count - menu.visibleLinesCount)
			{
				offset = menu.variants.Count - menu.visibleLinesCount;
			}
			vScrollBar.Value = offset;
			Invalidate();
		}
		
		private void OnVScroll(object target, ScrollEventArgs args)
		{
			Invalidate();
		}
		
		public int GetLine(Point point)
		{
			int line = vScrollBar.Value + point.Y / menu.charHeight;
			if (line < 0)
				line = 0;
			if (line >= menu.variants.Count)
				line = menu.variants.Count - 1;
			return line;
		}
		
		private Point lastMouseDownLocation;
	
		protected override void OnMouseDown(MouseEventArgs e)
		{
			lastMouseDownLocation = e.Location;
			int index = GetLine(lastMouseDownLocation);
			if (index != -1)
			{
				menu.handler.ProcessSelect(menu.variants[index]);
				Invalidate();
			}
		}
		
		protected override void OnDoubleClick(EventArgs e)
		{
			int index = GetLine(lastMouseDownLocation);
			if (index != -1)
			{
				menu.handler.ProcessSelect(menu.variants[index]);
				menu.handler.ProcessComplete();
			}
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			List<Variant> variants = menu.variants;
			
			Graphics g = e.Graphics;
			g.FillRectangle(menu.scheme.lineBgBrush, new Rectangle(0, 0, width, height));
			int offset = vScrollBar.Value;
			for (int i = variants.Count; i-- > 0;)
			{
				if (i >= offset && i < offset + menu.visibleLinesCount)
				{
					if (variants[i] == menu.selectedVariant)
					{
						g.FillRectangle(
							menu.scheme.selectionBrush,
							new Rectangle(0, (i - offset) * menu.charHeight, width, menu.charHeight));
					}
					string text = variants[i].DisplayText;
					string text0 = "";
					string text1 = text;
					int index = -1;
                    int deep = 0;
                    for (int j = 0; j < text.Length; j++)
                    {
                        char c = text[j];
                        if (c == '<')
                        {
                            ++deep;
                        }
                        else if (c == '>')
                        {
                            --deep;
                        }
                        else if (c == ' ')
                        {
                        	if (deep == 0)
                        	{
                            	index = j;
                            	break;
                            }
                        }
                        else if (c == '(')
                        {
                        	index = -1;
                        	break;
                        }
                    }
                    if (index != -1)
                    {
                    	string newText0 = text.Substring(0, index);
                    	if (newText0 != "protected" && newText0 != "public" && newText0 != "private" && newText0 != "override")
                    	{
							text0 = newText0;
							text1 = text.Substring(index + 1);
						}
                    }
					DrawLineChars(g, new Point(0, (i - offset) * menu.charHeight), menu.defaultStyle, text1);
					DrawLineChars(g, new Point((menu.maxLength - text0.Length) * menu.charWidth, (i - offset) * menu.charHeight), menu.typeStyle, text0);
				}
			}
		}
		
		private void DrawLineChars(Graphics g, Point position, TextStyle style, string text)
		{
			int count = text.Length;
			float y = position.Y;
			float x = position.X - menu.charWidth / 3;
			for (int i = 0; i < count; i++)
			{
				g.DrawString(
					text[i].ToString(),
					menu.fonts[style.fontStyle],
					style.brush,
					x + menu.charWidth * i,
					y,
					menu.stringFormat);
			}
		}
	}
}