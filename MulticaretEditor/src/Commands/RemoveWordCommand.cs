using System;

namespace MulticaretEditor.Commands
{
	public class RemoveWordCommand : Command
	{
		private bool isLeft;
		
		public RemoveWordCommand(bool isLeft) : base(isLeft ? CommandType.RemoveWordLeft : CommandType.RemoveWordRight)
		{
			this.isLeft = isLeft;
		}
		
		private EraseSelectionCommand eraseSelection;
		private SelectionMemento[] mementos;
		
		override public bool Init()
		{
			if (!lines.AllSelectionsEmpty)
				return false;
			mementos = GetSelectionMementos();
			if (isLeft)
			{
				Controller.MoveWordLeft(lines, true);
			}
			else
			{
				Controller.MoveWordRight(lines, true);
			}
			eraseSelection = new EraseSelectionCommand();
			eraseSelection.lines = lines;
			eraseSelection.selections = selections;
			return eraseSelection.Init();
		}
		
		override public void Redo()
		{
			eraseSelection.Redo();
		}
		
		override public void Undo()
		{
			eraseSelection.Undo();
			SetSelectionMementos(mementos);
		}
	}
}
