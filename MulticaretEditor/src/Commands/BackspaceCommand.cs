using System;

namespace MulticaretEditor
{
	public class BackspaceCommand : Command
	{
		public BackspaceCommand() : base(CommandType.Backspace)
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
				if (memento.caret > 0)
				{
					deletedI = lines.GetText(memento.caret - 1, 1);
					if (deletedI == "\n" && memento.caret > 1 && lines.GetText(memento.caret - 2, 1) == "\r")
					{
						deletedI = "\r\n";
						lines.RemoveText(memento.caret - 2, 2);
						memento.caret -= 2;
						offset -= 2;
					}
					else
					{
						lines.RemoveText(memento.caret - 1, 1);
						memento.caret--;
						offset--;
					}
				}
				else
				{
					deletedI = "";
				}
				deleted[i] = deletedI;
				memento.anchor = memento.caret;
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
				lines.RemoveText(memento.Left, memento.Count);
				
				string deletedI = deleted[i];
				lines.InsertText(memento.Left, deletedI);
				offset += deletedI.Length;
				memento.anchor += deletedI.Length;
				memento.caret = memento.anchor;
				Place place = lines.PlaceOf(memento.caret);
				memento.preferredPos = lines[place.iLine].PosOfIndex(place.iChar);
				mementos[i] = memento;
			}
			deleted = null;
			SetSelectionMementos(mementos);
		}
	}
}
