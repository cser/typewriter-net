using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class ViEraseLinesForChangeCommand : Command
	{
		private readonly Controller controller;
		private readonly List<SimpleRange> ranges;
		
		public ViEraseLinesForChangeCommand(Controller controller, List<SimpleRange> ranges) : base(CommandType.EraseLines)
		{
			this.controller = controller;
			this.ranges = ranges;
		}
		
		private SelectionMemento[] mementos;
		private InsertTextCommand eraseCommand;
		
		override public bool Init()
		{
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			lines.ResizeSelections(ranges.Count);
			bool[] isLast = new bool[selections.Count];
			for (int i = ranges.Count; i-- > 0;)
			{
				SimpleRange range = ranges[i];
				Line endLine = lines[range.index + range.count - 1];
				Selection selection = selections[i];
				if (range.index > 0 && range.index + range.count == lines.LinesCount)
				{
					isLast[i] = true;
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
			
			InsertTextCommand eraseCommand = new InsertTextCommand(lines.lineBreak, null, true);
			eraseCommand.lines = lines;
			eraseCommand.selections = selections;
			if (eraseCommand.Init())
			{
				eraseCommand.Redo();
				for (int i = 0; i < selections.Count; i++)
				{
					Selection selection = selections[i];
					if (!isLast[i])
					{
						selection.anchor = selection.caret = selection.caret - lines.lineBreak.Length;
					}
				}
				this.eraseCommand = eraseCommand;
			}
			else
			{
				this.eraseCommand = null;
			}
			//controller.ViMoveHome(false, true);
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
