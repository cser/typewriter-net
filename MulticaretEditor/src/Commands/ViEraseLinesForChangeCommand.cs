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
		private InsertTextCommand indentCommand;
		
		override public bool Init()
		{
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			lines.ResizeSelections(ranges.Count);
			string[] lineBreaks = new string[selections.Count];
			for (int i = ranges.Count; i-- > 0;)
			{
				SimpleRange range = ranges[i];
				Line endLine = lines[range.index + range.count - 1];
				Selection selection = selections[i];
				lineBreaks[i] = range.index + range.count == lines.LinesCount ? "" : lines.lineBreak;
				selection.anchor = lines.IndexOf(new Place(0, range.index));
				selection.caret = lines.IndexOf(new Place(endLine.charsCount, range.index + range.count - 1));
			}
			
			this.eraseCommand = null;
			this.indentCommand = null;
			InsertTextCommand eraseCommand = new InsertTextCommand(null, lineBreaks, true);
			eraseCommand.lines = lines;
			eraseCommand.selections = selections;
			if (eraseCommand.Init())
			{
				string[] indents = new string[selections.Count];
				for (int i = 0; i < selections.Count; i++)
				{
					Selection selection = selections[i];
					Place place = lines.PlaceOf(Math.Min(selection.anchor, selection.caret));
					Line line = lines[place.iLine];
					string text;
					int count;
					line.GetFirstIntegerTabs(out text, out count);
					indents[i] = text ?? "";
				}
				
				eraseCommand.Redo();
				for (int i = 0; i < selections.Count; i++)
				{
					Selection selection = selections[i];
					selection.anchor = selection.caret = selection.caret - lineBreaks[i].Length;
				}
				this.eraseCommand = eraseCommand;
				
				InsertTextCommand indentCommand = new InsertTextCommand(null, indents, true);
				indentCommand.lines = lines;
				indentCommand.selections = selections;
				if (indentCommand.Init())
				{
					indentCommand.Redo();
					this.indentCommand = indentCommand;
				}
			}
		}
		
		override public void Undo()
		{
			if (indentCommand != null)
			{
				indentCommand.Undo();
				indentCommand = null;
			}
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
