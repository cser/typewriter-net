using System;

namespace MulticaretEditor.Commands
{
	public class EraseSelectionCommand : Command
	{
		public EraseSelectionCommand() : base(CommandType.EraseSelection)
		{
		}
		
		private string[] deleted;
		private SelectionMemento[] mementos;
		
		override public bool Init()
		{
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}
		
		override public void Redo()
		{
			Debug.Begin("Redo()");
			deleted = new string[mementos.Length];
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				Debug.Begin("[" + i + "]");
				SelectionMemento memento = mementos[i];
				memento.caret += offset;
				memento.anchor += offset;
				{
				/*/*/Place place0 = lines.PlaceOf(memento.Left);
				/*/*/Place place1 = lines.PlaceOf(memento.Right);
				/*/*/Debug.Log("[" + i + "]: (" + memento.caret + ", " + memento.anchor + ": " + memento.Count + ") - " + place0 + ", " + place1);}
				string deletedI;
				if (!memento.Empty)
				{
					deletedI = lines.GetText(memento.Left, memento.Count);
					Debug.Log("deletedI=<<" + deletedI + ">>");
					lines.RemoveText(memento.Left, memento.Count);
					offset -= memento.Count;
				}
				else
				{
					deletedI = "";
				}
				deleted[i] = deletedI;
				mementos[i] = memento;
				Debug.End();
			}
			SetSelectionMementos(mementos);
			foreach (Selection selection in selections)
			{
				selection.anchor = selection.Left;
				selection.caret = selection.anchor;
				lines.SetPreferredPos(selection, lines.PlaceOf(selection.caret));
			}
			lines.JoinSelections();
			Debug.End();
		}
		
		override public void Undo()
		{
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				string deletedI = deleted[i];
				lines.InsertText(memento.Left, deletedI);
				offset += deletedI.Length;
				Place place = lines.PlaceOf(memento.caret);
				memento.preferredPos = lines[place.iLine].PosOfIndex(place.iChar);
				mementos[i] = memento;
			}
			deleted = null;
			SetSelectionMementos(mementos);
		}
	}
}
