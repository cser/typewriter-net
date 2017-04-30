using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor.Commands
{
	public class EraseLinesCommand : Command
	{
		public EraseLinesCommand() : base(CommandType.EraseLines)
		{
		}
		
		private List<string> deleted;
		private SelectionMemento[] mementos;
		private EraseSelectionCommand eraseCommand;
		
		override public bool Init()
		{
			lines.JoinSelections();
			if (!lines.AllSelectionsEmpty)
			{
				return false;
			}
			deleted = new List<string>();
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			lines.JoinSelections();
			List<SimpleRange> ranges = GetLineRangesByCarets(this.mementos);
			lines.ResizeSelections(ranges.Count);
			for (int i = ranges.Count; i-- > 0;)
			{
				SimpleRange range = ranges[i];
				Selection selection = selections[i];
				selection.anchor = lines.IndexOf(new Place(0, range.index));
				Line endLine = lines[range.index + range.count - 1];
				selection.caret = lines.IndexOf(new Place(endLine.charsCount, range.index + range.count - 1));
			}
			
			EraseSelectionCommand eraseCommand = new EraseSelectionCommand();
			eraseCommand.lines = lines;
			eraseCommand.selections = lines.selections;
			if (eraseCommand.Init())
			{
				eraseCommand.Redo();
				this.eraseCommand = eraseCommand;
			}
		}
		
		override public void Undo()
		{
			if (eraseCommand != null)
			{
				eraseCommand.Undo();
				eraseCommand = null;
			}
			SetSelectionMementos(mementos);
		}
	}
}
