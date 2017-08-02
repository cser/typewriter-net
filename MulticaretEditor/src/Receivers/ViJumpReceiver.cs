using System;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViJumpReceiver : AReceiver
	{
		public override ViMode ViMode { get { return ViMode.Normal; } }
		
		public override ViJumpReceiver AsJump { get { return this; } }
		
		private readonly char firstChar;
		
		public ViJumpReceiver(char firstChar)
		{
			this.firstChar = firstChar;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override void DoOn()
		{
		}
		
		public override bool IsIdle { get { return false; } }
		
		public override void DoKeyPress(char code, out string viShortcut, out bool scrollToCursor)
		{
			ViChar viChar = new ViChar(code, false);
			viChar.c = context.GetMapped(code);
			ProcessKey(viChar, out viShortcut, out scrollToCursor);
		}
		
		public override bool DoKeyDown(Keys keysData, out bool scrollToCursor)
		{
			scrollToCursor = false;
			return true;
		}
		
		private void ProcessKey(ViChar code, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			scrollToCursor = false;
		}
		
		public override bool DoFind(Pattern pattern)
		{
			ClipboardExecutor.PutToSearch(pattern);
			if (ClipboardExecutor.ViRegex != null)
			{
				controller.ViFindForward(ClipboardExecutor.ViRegex);
			}
			return true;
		}
		
		public void DoPaint(Graphics g, Font font, StringFormat stringFormat, Scheme scheme, int lineInterval,
			char[,] map, int charWidth, int charHeight)
		{
			int sizeX = map.GetLength(0);
			int sizeY = map.GetLength(1);
			for (int y = 0; y < sizeY; ++y)
			{
				for (int x = 0; x < sizeX; ++x)
				{
					char c = map[x, y];
					g.DrawString("X", font, scheme.fgBrush,
						x * charWidth - charWidth / 3, y * charHeight + lineInterval / 2, stringFormat);
				}
			}
		}
	}
}
