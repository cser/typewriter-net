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
			private char register;
			
			public Delete(ViMoves.IMove move, int count, bool change, char register)
			{
				this.move = move;
				this.count = count;
				this.change = change;
				this.register = register;
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
				controller.ViCut(register);
			}
		}
		
		public class DeleteLine : ICommand
		{
			private int count;
			private char register;
			
			public DeleteLine(int count, char register)
			{
				this.count = count;
				this.register = register;
			}
			
			public void Execute(Controller controller)
			{
				controller.ViSelectLine(count);
				controller.ViCut(register);
				controller.ViMoveHome(false, true);
			}
		}
		
		public class Paste : ICommand
		{
			private Direction direction;
			private char register;
			private int count;
			
			public Paste(Direction direction, char register, int count)
			{
				this.direction = direction;
				this.register = register;
				this.count = count;
			}
			
			public void Execute(Controller controller)
			{
				controller.ViPaste(register, direction, count);
			}
		}
		
		public class J : ICommand
		{	
			public void Execute(Controller controller)
			{
				controller.ViJ();
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