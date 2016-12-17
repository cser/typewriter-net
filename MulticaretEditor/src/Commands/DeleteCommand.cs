using System;

namespace MulticaretEditor
{
	public class DeleteCommand : Command
	{
		public DeleteCommand() : base(CommandType.Delete)
		{
		}
		
		private string[] deleted;
		private SelectionMemento[] mementos;
		
		override public bool Init()
		{
			if (!lines.AllSelectionsEmpty)
				return false;
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			deleted = new string[mementos.Length];
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				string deletedI;
				if (memento.caret < lines.charsCount)
				{
					deletedI = lines.GetText(memento.caret, 1);
					if (deletedI == "\r" && memento.caret < lines.charsCount - 1 && lines.GetText(memento.caret + 1, 1) == "\n")
					{
						deletedI = "\r\n";
						lines.RemoveText(memento.caret, 2);
						offset -= 2;
					}
					else
					{
						lines.RemoveText(memento.caret, 1);
						offset--;
					}
				}
				else
				{
					deletedI = "";
				}
				deleted[i] = deletedI;
				Place place = lines.PlaceOf(memento.caret);
				memento.preferredPos = lines[place.iLine].PosOfIndex(place.iChar);
				mementos[i] = memento;
			}
			SetSelectionMementos(mementos);
			lines.JoinSelections();
		}
		
		override public void Undo()
		{
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				int anchor = memento.anchor;
				lines.RemoveText(memento.Left, memento.Count);
				
				string text = deleted[i];
				lines.InsertText(memento.Left, text);
				offset += text.Length;
				memento.anchor = anchor;
				memento.caret = anchor;
				Place place = lines.PlaceOf(memento.caret);
				memento.preferredPos = lines[place.iLine].PosOfIndex(place.iChar);
				mementos[i] = memento;
			}
			deleted = null;
			SetSelectionMementos(mementos);
		}
	}
}
