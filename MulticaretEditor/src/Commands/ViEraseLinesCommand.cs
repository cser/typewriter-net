using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class ViEraseLinesCommand : Command
	{
		private readonly Controller controller;
		private readonly List<SimpleRange> ranges;
		
		public ViEraseLinesCommand(Controller controller, List<SimpleRange> ranges) : base(CommandType.EraseLines)
		{
			this.controller = controller;
			this.ranges = ranges;
		}
		
		private SelectionMemento[] mementos;
		private EraseSelectionCommand eraseCommand;
		
		override public bool Init()
		{
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			lines.ResizeSelections(ranges.Count);
			for (int i = ranges.Count; i-- > 0;)
			{
				SimpleRange range = ranges[i];
				Line endLine = lines[range.index + range.count - 1];
				Selection selection = selections[i];
				if (range.index > 0 && range.index + range.count == lines.LinesCount)
				{
					Line startLine = lines[range.index - 1];
					selection.anchor = lines.IndexOf(new Place(startLine.NormalCount, range.index - 1));
					selection.caret = lines.IndexOf(new Place(endLine.charsCount, range.index + range.count - 1));
				}
				else
				{
					selection.anchor = lines.IndexOf(new Place(0, range.index));
					selection.caret = lines.IndexOf(new Place(endLine.charsCount, range.index + range.count - 1));
				}
			}
			
			EraseSelectionCommand eraseCommand = new EraseSelectionCommand();
			eraseCommand.lines = lines;
			eraseCommand.selections = lines.selections;
			if (eraseCommand.Init())
			{
				eraseCommand.Redo();
				this.eraseCommand = eraseCommand;
			}
			else
			{
				this.eraseCommand = null;
			}
			controller.ViMoveHome(false, true);
		}
		
		override public void Undo()
		{
			if (eraseCommand != null)
			{
				eraseCommand.Undo();
				eraseCommand = null;
			}
			SetSelectionMementos(mementos);
			lines.viStoreSelector.ViStoreMementos(mementos);
		}
	}
}
