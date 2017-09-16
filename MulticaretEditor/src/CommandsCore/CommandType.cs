using System;

namespace MulticaretEditor
{
	public enum CommandType
	{
		None,
		InsertText,
		ChangeCase,
		EraseSelection,
		EraseLines,
		Delete,
		Backspace,
		Paste,
		ShiftLeft,
		ShiftRight,
		RemoveWordLeft,
		RemoveWordRight,
		MoveLineUp,
		MoveLineDown,
		FixLineBreaks,
		ViSavePositions,
		ReplaceText,
		InsertIndentedCket,
		InsertIndentedBefore
	}
}
