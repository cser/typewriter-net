using System;

namespace MulticaretEditor
{
	public class ReplaceTextCommand : Command
	{
		private SimpleRange[] orderedRanges;
		private string newText;

		public ReplaceTextCommand(SimpleRange[] orderedRanges, string newText) : base(CommandType.ReplaceText)
		{
			this.orderedRanges = orderedRanges;
			this.newText = newText;
		}

		private string[] deleted;
		private SelectionMemento[] mementos;

		override public bool Init()
		{
			mementos = GetSelectionMementos();
			return true;
		}

		override public void Redo()
		{
			deleted = new string[orderedRanges.Length];
			int offset = 0;
			for (int i = 0; i < orderedRanges.Length; i++)
			{
				SimpleRange range = orderedRanges[i];
				range.index += offset;
				string deletedI = lines.GetText(range.index, range.count);
				lines.RemoveText(range.index, range.count);
				lines.InsertText(range.index, newText);
				offset += newText.Length - range.count;
				orderedRanges[i] = range;
				deleted[i] = deletedI;
			}
			if (orderedRanges.Length > 0)
			{
				selections.RemoveRange(0, selections.Count - 1);
				Selection selection = selections[0];
				SimpleRange lastRange = orderedRanges[orderedRanges.Length - 1];
				selection.anchor = selection.caret = lastRange.index + newText.Length;
				Place place = lines.PlaceOf(selection.caret);
				lines.SetPreferredPos(selection, place);
			}
		}

		override public void Undo()
		{
			int offset = 0;
			for (int i = 0; i < orderedRanges.Length; i++)
			{
				SimpleRange range = orderedRanges[i];
				range.index += offset;
				string deletedI = deleted[i];
				lines.RemoveText(range.index, newText.Length);
				lines.InsertText(range.index, deletedI);
				offset += deletedI.Length - newText.Length;
				orderedRanges[i] = range;
			}
			deleted = null;
			SetSelectionMementos(mementos);
			lines.viStoreSelector.ViStoreMementos(mementos);
		}
	}
}
