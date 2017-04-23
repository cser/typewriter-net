using System;

namespace MulticaretEditor
{
	public class CommandType
	{
		public static readonly CommandType None = new CommandType(false, false);
		public static readonly CommandType InsertText = new CommandType(true, false);
		public static readonly CommandType ChangeCase = new CommandType(true, false);
		public static readonly CommandType EraseSelection = new CommandType(true, false);
		public static readonly CommandType Delete = new CommandType(true, false);
		public static readonly CommandType Backspace = new CommandType(true, false);
		public static readonly CommandType Copy = new CommandType(false, false);
		public static readonly CommandType Paste = new CommandType(true, false);
		public static readonly CommandType ShiftLeft = new CommandType(true, false);
		public static readonly CommandType ShiftRight = new CommandType(true, false);
		public static readonly CommandType RemoveWordLeft = new CommandType(true, false);
		public static readonly CommandType RemoveWordRight = new CommandType(true, false);
		public static readonly CommandType MoveLineUp = new CommandType(true, false);
		public static readonly CommandType MoveLineDown = new CommandType(true, false);
		public static readonly CommandType FixLineBreaks = new CommandType(true, false);
		public static readonly CommandType ViSavePositions = new CommandType(true, true);
		public static readonly CommandType ReplaceText = new CommandType(true, false);
		
		public readonly bool changesText;
		public readonly bool helped;
		
		public CommandType(bool changesText, bool helped)
		{
			this.changesText = changesText;
			this.helped = helped;
		}
	}
}
