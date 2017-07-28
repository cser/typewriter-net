using System;

namespace MulticaretEditor
{
	public class CommandType
	{
		public static readonly CommandType InsertText = new CommandType(false);
		public static readonly CommandType ChangeCase = new CommandType(false);
		public static readonly CommandType EraseSelection = new CommandType(false);
		public static readonly CommandType EraseLines = new CommandType(false);
		public static readonly CommandType Delete = new CommandType(false);
		public static readonly CommandType Backspace = new CommandType(false);
		public static readonly CommandType Paste = new CommandType(false);
		public static readonly CommandType ShiftLeft = new CommandType(false);
		public static readonly CommandType ShiftRight = new CommandType(false);
		public static readonly CommandType RemoveWordLeft = new CommandType(false);
		public static readonly CommandType RemoveWordRight = new CommandType(false);
		public static readonly CommandType MoveLineUp = new CommandType(false);
		public static readonly CommandType MoveLineDown = new CommandType(false);
		public static readonly CommandType FixLineBreaks = new CommandType(false);
		public static readonly CommandType ViSavePositions = new CommandType(true);
		public static readonly CommandType ReplaceText = new CommandType(false);
		public static readonly CommandType InsertIndentedCket = new CommandType(false);
		public static readonly CommandType InsertIndentedBefore = new CommandType(false);
		
		public readonly bool helped;
		
		public CommandType(bool helped)
		{
			this.helped = helped;
		}
	}
}
