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
			void Move(Controller controller, bool shift, bool change);
		}
		
		public class MoveStep : IMove
		{
			private Direction direction;
			
			public MoveStep(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift, bool change)
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
		
		public class MoveWord : IMove
		{
			private Direction direction;
			
			public MoveWord(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift, bool change)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.ViMoveWordLeft(shift, change);
						break;
					case Direction.Right:
						controller.ViMoveWordRight(shift, change);
						break;
				}
			}
		}
		
		public class MoveObject : IMove
		{
			private char o;
			private bool inside;
			
			public MoveObject(char o, bool inside)
			{
				this.o = o;
				this.inside = inside;
			}
			
			public void Move(Controller controller, bool shift, bool change)
			{
				switch (o)
				{
					case 'w':
						controller.ViMoveInWord(shift, inside);
						break;
				}
			}
		}
		
		public class MoveWordE : IMove
		{	
			public void Move(Controller controller, bool shift, bool change)
			{
				if (change)
				{
					controller.ViMoveWordRight(shift, change);
				}
				else
				{
					controller.ViMoveWordE(shift);
				}
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
			
			public void Move(Controller controller, bool shift, bool change)
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
		
		public class FindForwardPattern : IMove
		{
			public void Move(Controller controller, bool shift, bool change)
			{
				controller.ViFindForward(ClipboardExecuter.ViRegex);
			}
		}
		
		public class FindBackwardPattern : IMove
		{
			public void Move(Controller controller, bool shift, bool change)
			{
				controller.ViFindBackward(ClipboardExecuter.ViBackwardRegex);
			}
		}
		
		public class Home : IMove
		{
			private bool indented;
			
			public Home(bool indented)
			{
				this.indented = indented;
			}
			
			public void Move(Controller controller, bool shift, bool change)
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
			
			public void Move(Controller controller, bool shift, bool change)
			{
				controller.ViMoveEnd(shift, count);
			}
		}
		
		public class DocumentStart : IMove
		{
			public void Move(Controller controller, bool shift, bool change)
			{
				controller.DocumentStart(shift);
			}
		}
		
		public class DocumentEnd : IMove
		{
			public void Move(Controller controller, bool shift, bool change)
			{
				controller.ViDocumentEnd(shift);
			}
		}
		
		public class GoToLine : IMove
		{
			private int count;
			
			public GoToLine(int count)
			{
				this.count = count;
			}
			
			public void Move(Controller controller, bool shift, bool change)
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
			
			public void Move(Controller controller, bool shift, bool change)
			{
				controller.ScrollPage(isUp, shift);
			}
		}
	}
}