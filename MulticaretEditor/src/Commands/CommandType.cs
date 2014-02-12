using System;

namespace MulticaretEditor.Commands
{
	public class CommandType
	{
		public static readonly CommandType None = new CommandType(false);
		public static readonly CommandType InsertText = new CommandType(true);
		public static readonly CommandType EraseSelection = new CommandType(true);
		public static readonly CommandType Delete = new CommandType(true);
		public static readonly CommandType Backspace = new CommandType(true);
		public static readonly CommandType Copy = new CommandType(false);
		public static readonly CommandType Paste = new CommandType(true);
		public static readonly CommandType ShiftLeft = new CommandType(true);
		public static readonly CommandType ShiftRight = new CommandType(true);
		public static readonly CommandType RemoveWordLeft = new CommandType(true);
		public static readonly CommandType RemoveWordRight = new CommandType(true);
		public static readonly CommandType MoveLineUp = new CommandType(true);
		public static readonly CommandType MoveLineDown = new CommandType(true);
		
		public readonly bool changesText;
		
		public CommandType(bool changesText)
		{
			this.changesText = changesText;
		}
	}
}
