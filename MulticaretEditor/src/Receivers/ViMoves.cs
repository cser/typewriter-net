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
			void Move(Controller controller, bool shift);
		}
		
		public class MoveStep : IMove
		{
			private Direction direction;
			
			public MoveStep(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift)
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
			
			public override string ToString()
			{
				return "MoveStep:" + direction;
			}
		}
		
		public class MoveWord : IMove
		{
			private Direction direction;
			
			public MoveWord(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Move(Controller controller, bool shift)
			{
				switch (direction)
				{
					case Direction.Left:
						controller.ViMoveWordLeft(shift);
						break;
					case Direction.Right:
						controller.ViMoveWordRight(shift);
						break;
				}
			}
			
			public override string ToString()
			{
				return "MoveWord:" + direction;
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
			
			public void Move(Controller controller, bool shift)
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
			
			public override string ToString()
			{
				return count + ":" + type + ":" + charToFind;
			}
		}
		
		public class Home : IMove
		{
			private bool indented;
			
			public Home(bool indented)
			{
				this.indented = indented;
			}
			
			public void Move(Controller controller, bool shift)
			{
				controller.MoveHome(shift);
				controller.ViMoveHome(shift, indented);
			}
			
			public override string ToString()
			{
				return "Home:" + indented;
			}
		}
		
		public class End : IMove
		{
			private int count;
			
			public End(int count)
			{
				this.count = count;
			}
			
			public void Move(Controller controller, bool shift)
			{
				controller.ViMoveEnd(shift, count);
			}
			
			public override string ToString()
			{
				return "End:" + count;
			}
		}
		
		public class DocumentStart : IMove
		{
			public void Move(Controller controller, bool shift)
			{
				controller.DocumentStart(shift);
			}
		}
		
		public class DocumentEnd : IMove
		{
			public void Move(Controller controller, bool shift)
			{
				controller.ViDocumentEnd(shift);
			}
		}
		
		public class PageUpDown : IMove
		{
			private bool isUp;
			
			public PageUpDown(bool isUp)
			{
				this.isUp = isUp;
			}
			
			public void Move(Controller controller, bool shift)
			{
				controller.ScrollPage(isUp, shift);
			}
		}
	}
}