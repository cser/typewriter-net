using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using MulticaretEditor.KeyMapping;

public class AutocompleteMenu : ToolStripDropDown
{
	private readonly StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
	private readonly Scheme scheme;
	private readonly int charWidth;
	private readonly int charHeight;
	private readonly Font[] fonts = new Font[16];
	private readonly Font font;
	private readonly ToolStripControlHost host;
	private readonly MenuControl control;
	private readonly TextStyle defaultStyle;
	private readonly int maxLinesCount;
	
	public AutocompleteMenu(Scheme scheme, FontFamily family, float emSize)
	{
		this.scheme = scheme;
		
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

		SizeF size = GetCharSize(fonts[0], 'M');
		charWidth = (int)Math.Round(size.Width * 1f) - 1;
		charHeight = (int)Math.Round(size.Height * 1f) + 1;
		
		maxLinesCount = Math.Max(10, Screen.PrimaryScreen.Bounds.Height / (2 * charHeight));
		
		AutoClose = false;
		AutoSize = false;
		DropShadowEnabled = false;
		
		control = new MenuControl(this);
		host = new ToolStripControlHost(control);
		host.Margin = Padding.Empty;
		host.Padding = Padding.Empty;
		host.AutoSize = false;
		host.AutoToolTip = false;
		Items.Add(host);
	}
	
	private static SizeF GetCharSize(Font font, char c)
	{
		Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", font);
		Size sz3 = TextRenderer.MeasureText("<>", font);
		return new SizeF(sz2.Width - sz3.Width + 1, font.Height);
	}
	
	private readonly List<Variant> variants = new List<Variant>();
	
	private int visibleLinesCount;
	
	public void SetVariants(List<Variant> variants)
	{
		this.variants.Clear();
		this.variants.AddRange(variants);
		
		int maxLength = 0;
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
	}
	
	protected override void OnPaint(PaintEventArgs e)
	{
	}
	
	public class MenuControl : Control
	{
		private readonly AutocompleteMenu menu;
		private readonly ScrollBar vScrollBar;
		private bool needVScrollFix;
		
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
			
			vScrollBar = new VScrollBar();
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
		}
		
		private void OnVScroll(object target, ScrollEventArgs args)
		{
			if (args.Type == ScrollEventType.EndScroll)
				needVScrollFix = true;
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			List<Variant> variants = menu.variants;
			
			Graphics g = e.Graphics;
			g.FillRectangle(menu.scheme.lineNumberBackground, new Rectangle(0, 0, width, height));
			for (int i = menu.visibleLinesCount; i-- > 0;)
			{
				//if (i < 0 || i >= variants.Count)
				//	continue;
				DrawLineChars(g, new Point(0, i * menu.charHeight), variants[i].DisplayText);
			}
		}
		
		private void DrawLineChars(Graphics g, Point position, string text)
		{
			int count = text.Length;
			float y = position.Y;
			float x = position.X - menu.charWidth / 3;
			for (int i = 0; i < count; i++)
			{
				g.DrawString(
					text[i].ToString(),
					menu.fonts[menu.defaultStyle.fontStyle],
					menu.defaultStyle.brush,
					x + menu.charWidth * i,
					y,
					menu.stringFormat);
			}
		}
	}
}