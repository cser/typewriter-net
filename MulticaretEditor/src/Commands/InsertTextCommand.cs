using System;

namespace MulticaretEditor
{
	public class InsertTextCommand : Command
	{
		private string text;
		private string[] texts;
		private bool changeSelection;

		public InsertTextCommand(string text, string[] texts, bool changeSelection) : base(CommandType.InsertText)
		{
			this.text = text;
			this.texts = texts;
			this.changeSelection = changeSelection;
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
			deleted = new string[mementos.Length];
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				string text = GetText(i);
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				string deletedI = lines.GetText(memento.Left, memento.Count);
				lines.RemoveText(memento.Left, memento.Count);
				lines.InsertText(memento.Left, text);
				memento.caret += text.Length;
				memento.anchor += text.Length;
				offset += -memento.Count + text.Length;
				mementos[i] = memento;
				deleted[i] = deletedI;
			}
			SetSelectionMementos(mementos);
			if (changeSelection)
			{
				foreach (Selection selection in selections)
				{
					selection.anchor = selection.Left;
					selection.caret = selection.anchor;
					Place place = lines.PlaceOf(selection.caret);
					lines.SetPreferredPos(selection, place);
				}
			}
			else
			{
				for (int i = 0; i < selections.Count; i++)
				{
					Selection selection = selections[i];
					int length = GetText(i).Length;
					selection.anchor -= length;
					selection.caret -= length;
					Place place = lines.PlaceOf(selection.caret);
					lines.SetPreferredPos(selection, place);
				}
			}
			lines.JoinSelections();
		}

		override public void Undo()
		{
			int offset = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				string text = GetText(i);
				SelectionMemento memento = mementos[i];
				memento.anchor += offset;
				memento.caret += offset;
				string deletedI = deleted[i];
				memento.anchor -= text.Length;
				memento.caret -= text.Length;
				lines.RemoveText(memento.Left, text.Length);
				lines.InsertText(memento.Left, deletedI);
				offset += deletedI.Length - text.Length;
				Place place = lines.PlaceOf(memento.caret);
				memento.preferredPos = lines[place.iLine].PosOfIndex(place.iChar);
				mementos[i] = memento;
			}
			deleted = null;
			SetSelectionMementos(mementos);
			lines.viStoreSelector.ViStoreMementos(mementos);
		}

		private string GetText(int index)
		{
			string result = text;
			if (result == null)
				result = texts != null && index < texts.Length ? texts[index] : "";
			return result;
		}
	}
}
