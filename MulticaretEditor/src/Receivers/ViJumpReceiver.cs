using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViJumpReceiver : AReceiver
	{
		public enum Mode
		{
			Single,
			Selection,
			LinesSelection,
			New
		}
		
		public override ViMode ViMode { get { return ViMode.Normal; } }
		
		public override ViJumpReceiver AsJump { get { return this; } }
		
		private readonly char firstChar;
		private readonly Mode mode;
		private string text = "";
		
		public ViJumpReceiver(char firstChar, Mode mode)
		{
			this.firstChar = firstChar;
			this.mode = mode;
		}
		
		public override bool AltMode { get { return true; } }
		
		public override bool IsIdle { get { return false; } }
		
		public override bool DoFind(Pattern pattern, bool isBackward)
		{
			ClipboardExecutor.PutToSearch(pattern);
			if (ClipboardExecutor.ViRegex != null)
			{
				if (isBackward)
				{
					controller.ViFindBackward(ClipboardExecutor.ViBackwardRegex);
				}
				else
				{
					controller.ViFindForward(ClipboardExecutor.ViRegex);
				}
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
		
		public override bool DoKeyDown(Keys keysData, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
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
						if (mode == Mode.Single)
						{
							controller.PutCursor(GetPlace(position), false);
							controller.ViAddHistoryPosition(true);
							context.SetState(new ViReceiver(null, false, false));
							scrollToCursor = true;
						}
						else if (mode == Mode.Selection)
						{
							controller.PutCursor(GetPlace(position), true);
							controller.ViAddHistoryPosition(true);
							context.SetState(new ViReceiverVisual(false));
							scrollToCursor = true;
						}
						else if (mode == Mode.LinesSelection)
						{
							controller.PutCursor(GetPlace(position), true);
							controller.ViAddHistoryPosition(true);
							context.SetState(new ViReceiverVisual(true));
							scrollToCursor = true;
						}
						else if (mode == Mode.New)
						{
							controller.PutNewCursor(GetPlace(position));
							controller.ViAddHistoryPosition(true);
							context.SetState(new ViReceiver(null, false, false));
							scrollToCursor = true;
						}
						hasStartsWith = true;
						break;
					}
					if (position.text.StartsWith(text))
					{
						hasStartsWith = true;
					}
				}
				if (!hasStartsWith)
				{
					context.SetState(new ViReceiver(null, false, false));
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
					if (c == firstChar)
					{
						Position position = positions[positionIndex];
						position.text = GetKey(Symbols, positionIndex, positions.Count);
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
		
		private const string Symbols =
			";abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		
		private static char[] keyCache = new char[1];
		
		public static string GetKey(string symbols, int index, int count)
		{
			int length = 1;
			while (count > symbols.Length)
			{
				++length;
				int oldCount = count;
				count /= symbols.Length;
				if (oldCount % symbols.Length > 0)
				{
					++count;
				}
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
