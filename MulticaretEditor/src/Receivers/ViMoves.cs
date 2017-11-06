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
			bool IsDCLines { get; }
			void Move(Controller controller, bool shift, MoveMode mode);
		}
		
		public class MoveStep : IMove
		{
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
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
			private bool isLines;
			public bool IsDCLines { get { return isLines; } }
			
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
					case 'W':
						controller.ViMoveInBigWord(shift, inside);
						break;
					case '{':
					case '}':
						controller.ViMoveInBrackets(shift, inside, '{', '}', count);
						isLines |= controller.ViTryConvertToLines('{', '}', inside);
						break;
					case '(':
					case ')':
						controller.ViMoveInBrackets(shift, inside, '(', ')', count);
						isLines |= controller.ViTryConvertToLines('(', ')', inside);
						break;
					case '[':
					case ']':
						controller.ViMoveInBrackets(shift, inside, '[', ']', count);
						isLines |= controller.ViTryConvertToLines('[', ']', inside);
						break;
					case '<':
					case '>':
						controller.ViMoveInBrackets(shift, inside, '<', '>', count);
						isLines |= controller.ViTryConvertToLines('<', '>', inside);
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
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViPairBracket(shift);
			}
		}
		
		public class Find : IMove
		{
			public bool IsDCLines { get { return false; } }
			
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
			private bool isDCLines;
			public bool IsDCLines { get { return isDCLines; } }
			
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
							isDCLines = true;
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
								controller.LastSelection.anchor = position0;
								controller.LastSelection.caret = position1;
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
			public bool IsDCLines { get { return false; } }
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViFindForward(ClipboardExecutor.ViRegex);
			}
		}
		
		public class FindBackwardPattern : IMove
		{
			public bool IsDCLines { get { return false; } }
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				controller.ViFindBackward(ClipboardExecutor.ViBackwardRegex);
			}
		}
		
		public class Home : IMove
		{
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return true; } }
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				if (!controller.lastSelectionFree)
				{
					controller.ClearMinorSelections();
				}
				controller.LastSelection.caret = 0;
				controller.ViMoveHome(shift, true);
			}
		}
		
		public class DocumentEnd : IMove
		{
			public bool IsDCLines { get { return true; } }
			
			public void Move(Controller controller, bool shift, MoveMode mode)
			{
				if (!controller.lastSelectionFree)
				{
					controller.ClearMinorSelections();
				}
				controller.LastSelection.caret = controller.Lines.charsCount;
				controller.ViMoveHome(shift, true);
			}
		}
		
		public class GoToLine : IMove
		{
			public bool IsDCLines { get { return false; } }
			
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
			public bool IsDCLines { get { return false; } }
			
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