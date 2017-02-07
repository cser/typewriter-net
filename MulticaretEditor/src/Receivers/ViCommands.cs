using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public static class ViCommands
	{
		public interface ICommand
		{
			void Execute(Controller controller);
		}
		
		public class Repeat : ICommand
		{
			private ICommand command;
			private int count;
			
			public Repeat(ICommand command, int count)
			{
				this.command = command;
				this.count = count;
			}
			
			public void Execute(Controller controller)
			{
				for (int i = 0; i < count; i++)
				{
					command.Execute(controller);
				}
			}
		}
		
		public class Empty : ICommand
		{
			private ViMoves.IMove move;
			private int count;
			
			public Empty(ViMoves.IMove move, int count)
			{
				this.move = move;
				this.count = count;
			}
			
			public void Execute(Controller controller)
			{
				move.Move(controller, false, false);
			}
		}
		
		public class Delete : ICommand
		{
			private ViMoves.IMove move;
			private int count;
			private bool change;
			
			public Delete(ViMoves.IMove move, int count, bool change)
			{
				this.move = move;
				this.count = count;
				this.change = change;
			}
			
			public void Execute(Controller controller)
			{
				for (int i = 0; i < count - 1; i++)
				{
					move.Move(controller, true, false);
				}
				if (count > 0)
				{
					move.Move(controller, true, change);
				}
				controller.ViCut();
			}
		}
		
		public class Copy : ICommand
		{
			private ViMoves.IMove move;
			private int count;
			
			public Copy(ViMoves.IMove move, int count)
			{
				this.move = move;
				this.count = count;
			}
			
			public void Execute(Controller controller)
			{
				for (int i = 0; i < count; i++)
				{
					move.Move(controller, true, false);
				}
				controller.ViCopy();
				controller.ViCollapseSelections();
			}
		}
		
		public class Paste : ICommand
		{
			private Direction direction;
			
			public Paste(Direction direction)
			{
				this.direction = direction;
			}
			
			public void Execute(Controller controller)
			{
				if (direction == Direction.Right)
				{
					controller.ViSavePositions();
					controller.ViMoveRightFromCursor();
				}
				controller.ViPaste();
				controller.ViSavePositions();
			}
		}
		
		public class Undo : ICommand
		{
			public void Execute(Controller controller)
			{
				controller.Undo();
				controller.ViCollapseSelections();
			}
		}
		
		public class Redo : ICommand
		{
			public void Execute(Controller controller)
			{
				controller.Redo();
				controller.ViCollapseSelections();
			}
		}
		
		public class ReplaceChar : ICommand
		{
			private char c;
			private int count;
			
			public ReplaceChar(char c, int count)
			{
				this.c = c;
				this.count = count;
			}
			
			public void Execute(Controller controller)
			{
				controller.ViReplaceChar(c, count);
			}
		}
	}
}