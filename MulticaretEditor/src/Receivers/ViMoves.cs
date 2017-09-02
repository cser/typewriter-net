using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public static class ViMoves
	{
		public interface IMove
		{
			void Move(Controller controller, bool shift, MoveMode mode);
		}
		
		public class MoveStep : IMove
		{
			private Direction direction;
			
			public MoveStep(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.ViMoveLeft(shift);
						break;
					case Direction.Right:
						controller.ViMoveRight(shift);
						break;
					case Direction.Up:
						controller.ViMoveUp(shift);
						break;
					case Direction.Down:
						controller.ViMoveDown(shift);
						break;
				}
			}
		}
		
		public class SublineMoveStep : IMove
		{
			private Direction direction;
			
			public SublineMoveStep(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				switch (direction)
				{
					case Direction.Up:
						controller.MoveUp(shift);
						controller.ViFixPositions(false);
						break;
					case Direction.Down:
						controller.MoveDown(shift);
						controller.ViFixPositions(false);
						break;
				}
			}
		}
		
		public class MoveWord : IMove
		{
			private Direction direction;
			
			public MoveWord(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.ViMove_b(shift, mode == MoveMode.Change);
						if (mode != MoveMode.Change)
						{
							controller.ViFixPositions(true);
						}
						break;
					case Direction.Right:
						controller.ViMove_w(shift, mode == MoveMode.Change, mode == MoveMode.Move);
						if (mode != MoveMode.Change)
						{
							controller.ViFixPositions(true);
						}
						break;
				}
			}
		}
		
		public class BigMoveWord : IMove
		{
			private Direction direction;
			
			public BigMoveWord(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.ViMove_B(shift, mode == MoveMode.Change);
						break;
					case Direction.Right:
						controller.ViMove_W(shift, mode == MoveMode.Change);
						break;
				}
			}
		}
		
		public class MoveObject : IMove
		{
			private char o;
			private bool inside;
			private int count;
			
			public MoveObject(char o, bool inside, int count)
			{
				this.o = o;
				this.inside = inside;
				this.count = count;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				switch (o)
				{
					case 'w':
						controller.ViMoveInWord(shift, inside);
						break;
					case '{':
					case '}':
						controller.ViMoveInBrackets(shift, inside, '{', '}', count);
						break;
					case '(':
					case ')':
						controller.ViMoveInBrackets(shift, inside, '(', ')', count);
						break;
					case '[':
					case ']':
						controller.ViMoveInBrackets(shift, inside, '[', ']', count);
						break;
					case '<':
					case '>':
						controller.ViMoveInBrackets(shift, inside, '<', '>', count);
						break;
					case '"':
					case '\'':
						controller.ViMoveInQuotes(shift, inside, o);
						break;
				}
			}
		}
		
		public class MoveWordE : IMove
		{	
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				if (mode == MoveMode.Change)
				{
					controller.ViMove_w(shift, true, true);
				}
				else
				{
					controller.ViMove_e(shift);
				}
			}
		}
		
		public class BigMoveWordE : IMove
		{	
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				if (mode == MoveMode.Change)
				{
					controller.ViMove_W(shift, true);
				}
				else
				{
					controller.ViMove_E(shift);
				}
			}
		}
		
		public class FindPairBracket : IMove
		{
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViPairBracket(shift);
			}
		}
		
		public class Find : IMove
		{
			private char type;
			private char charToFind;
			private int count;
			
			public Find(char type, char charToFind, int count)
			{
				this.type = type;
				this.charToFind = charToFind;
				this.count = count;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				switch (type)
				{
					case 'f':
						controller.ViMoveToCharRight(charToFind, shift, count, false);
						break;
					case 't':
						controller.ViMoveToCharRight(charToFind, shift, count, true);
						break;
					case 'F':
						controller.ViMoveToCharLeft(charToFind, shift, count, false);
						break;
					case 'T':
						controller.ViMoveToCharLeft(charToFind, shift, count, true);
						break;
				}
			}
		}
		
		public class JumpBookmark : IMove
		{
			public static bool IsFileBased(char charToJump)
			{
				return charToJump >= 'A' && charToJump <= 'Z';
			}
			
			public static int GetLocalPosition(Controller controller, char charToJump)
			{
				if (charToJump >= 'A' && charToJump <= 'Z')
				{
					string path;
					int position;
					controller.macrosExecutor.GetBookmark(charToJump, out path, out position);
					if (controller.Lines.viFullPath != null &&
						controller.Lines.viFullPath == path &&
						position != -1)
					{
						return position;
					}
					return -1;
				}
				return controller.GetBookmark(charToJump);
			}
			
			private char type;
			private char charToJump;
			
			public JumpBookmark(char type, char charToJump)
			{
				this.type = type;
				this.charToJump = charToJump;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				int position = GetLocalPosition(controller, charToJump);
				if (position != -1)
				{
					switch (type)
					{
						case '`':
							controller.ViMoveTo(position, shift);
							break;
						case '\'':
							if (mode == MoveMode.Delete)
							{
								controller.ClearMinorSelections();
								int position0;
								int position1;
								if (position >= controller.LastSelection.Left && position <= controller.LastSelection.Right)
								{
									position0 = controller.LastSelection.Left;
									position1 = controller.LastSelection.Right;
								}
								else if (position < controller.LastSelection.Right)
								{
									position0 = position;
									position1 = controller.LastSelection.Right;
								}
								else
								{
									position0 = controller.LastSelection.Left;
									position1 = position;
								}
								Place start = controller.Lines.PlaceOf(position0);
								start.iChar = 0;
								Place end = controller.Lines.PlaceOf(position1);
								{
									Line line = controller.Lines[end.iLine];
									end.iChar = line.charsCount;
								}
								controller.LastSelection.anchor = controller.Lines.IndexOf(start);
								controller.LastSelection.caret = controller.Lines.IndexOf(end);
							}
							else
							{
								controller.ViMoveTo(position, shift);
								controller.ViMoveHome(shift, true);
							}
							break;
					}
				}
			}
		}
		
		public class FindForwardPattern : IMove
		{
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViFindForward(ClipboardExecutor.ViRegex);
			}
		}
		
		public class FindBackwardPattern : IMove
		{
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViFindBackward(ClipboardExecutor.ViBackwardRegex);
			}
		}
		
		public class Home : IMove
		{
			private bool indented;
			
			public Home(bool indented)
			{
				this.indented = indented;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.MoveHome(shift);
				controller.ViMoveHome(shift, indented);
			}
		}
		
		public class End : IMove
		{
			private int count;
			
			public End(int count)
			{
				this.count = count;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViMoveEnd(shift, count);
			}
		}
		
		public class DocumentStart : IMove
		{
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.DocumentStart(shift);
			}
		}
		
		public class DocumentEnd : IMove
		{
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				if (mode == MoveMode.Delete)
				{
					controller.ClearMinorSelections();
					Place start = controller.Lines.PlaceOf(controller.LastSelection.anchor);
					if (start.iLine > 0)
					{
						--start.iLine;
						start.iChar = controller.Lines[start.iLine].NormalCount;
					}
					else
					{
						start.iChar = 0;
					}
					controller.LastSelection.anchor = controller.Lines.IndexOf(start);
					controller.LastSelection.caret = controller.Lines.charsCount;
				}
				else
				{
					controller.ViDocumentEnd(shift, mode);
				}
			}
		}
		
		public class GoToLine : IMove
		{
			private int count;
			
			public GoToLine(int count)
			{
				this.count = count;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViGoToLine(count - 1, shift);
			}
		}
		
		public class PageUpDown : IMove
		{
			private bool isUp;
			
			public PageUpDown(bool isUp)
			{
				this.isUp = isUp;
			}
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ScrollPage(isUp, shift);
			}
		}
	}
}