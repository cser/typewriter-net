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
		private string text = "";
		
		public ViJumpReceiver(char firstChar)
		{
			this.firstChar = firstChar;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override bool IsIdle { get { return false; } }
		
		public override bool DoFind(Pattern pattern)
		{
			ClipboardExecutor.PutToSearch(pattern);
			if (ClipboardExecutor.ViRegex != null)
			{
				controller.ViFindForward(ClipboardExecutor.ViRegex);
			}
			return true;
		}
		
		public int scrollX;
		public int scrollY;
		public int leftIndent;
		public int charWidth;
		public int charHeight;
		public char[,] map;
		
		public struct Position
		{
			public int x;
			public int y;
			public string text;
			
			public Position(int x, int y)
			{
				this.x = x;
				this.y = y;
				this.text = "";
			}
		}
		
		private List<Position> positions;
		
		public override void DoOn()
		{
		}
		
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
			if (!code.control)
			{
				text += code.c;
				bool hasStartsWith = false;
				foreach (Position position in positions)
				{
					if (position.text == text)
					{
						controller.PutCursor(GetPlace(position), false);
						context.SetState(new ViReceiver(null, false));
						hasStartsWith = true;
						break;
					}
					else if (position.text.StartsWith(text))
					{
						hasStartsWith = true;
					}
				}
				if (!hasStartsWith)
				{
					context.SetState(new ViReceiver(null, false));
				}
			}
		}
		
		private Place GetPlace(Position position)
		{
			Pos pos = new Pos(position.x + scrollX / charWidth, position.y + scrollY / charHeight);
			return lines.UniversalPlaceOf(pos);
		}
		
		public void FillChar(char c, float x, float y)
		{
			int xi = (int)((x - leftIndent) / charWidth);
			int yi = (int)(y / charHeight);
			if (xi >= 0 && xi < map.GetLength(0) &&
				yi >= 0 && yi < map.GetLength(1))
			{
				map[xi, yi] = c;
			}
		}
		
		public void DoPaint(Graphics g, Font font, StringFormat stringFormat, Scheme scheme)
		{
			if (positions == null)
			{
				positions = new List<Position>();
			}
			positions.Clear();
			int sizeX = map.GetLength(0);
			int sizeY = map.GetLength(1);
			for (int j = 0; j < sizeY; ++j)
			{
				for (int i = 0; i < sizeX; ++i)
				{
					char c = map[i, j];
					if (c == firstChar)
					{
						positions.Add(new Position(i, j));
					}
				}
			}
			int positionIndex = 0;
			for (int j = 0; j < sizeY; ++j)
			{
				for (int i = 0; i < sizeX; ++i)
				{
					char c = map[i, j];
					int x = leftIndent + i * charWidth;
					int y = j * charHeight;
					g.DrawString(c + "", font, scheme.fgBrush, x - charWidth / 3, y, stringFormat);
					if (c == firstChar)
					{
						Position position = positions[positionIndex];
						position.text = GetKey(positionIndex, positions.Count);
						positions[positionIndex] = position;
						++positionIndex;
						if (position.text.StartsWith(text))
						{
							g.FillRectangle(scheme.fgBrush, x, y, charWidth * position.text.Length, charHeight);
							g.DrawString(position.text, font, scheme.bgBrush, x - charWidth / 3, y, stringFormat);
						}
					}
				}
			}
		}
		
		private char[] keyCache = new char[1];
		
		private string GetKey(int index, int count)
		{
			string symbols = ";abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			int length = 1;
			while (count >= symbols.Length)
			{
				++length;
				count /= symbols.Length;
			}
			if (keyCache.Length != length)
			{
				keyCache = new char[length];
			}
			for (int i = 0; i < length; ++i)
			{
				keyCache[i] = symbols[index % symbols.Length];
				index /= symbols.Length;
			}
			return new string(keyCache);
		}
	}
}
