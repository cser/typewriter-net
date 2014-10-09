using System;
using System.Collections.Generic;

namespace MulticaretEditor.Commands
{
	public class MoveLineCommand : Command
	{
		private bool isUp;

		public MoveLineCommand(bool isUp) : base(isUp ? CommandType.MoveLineUp : CommandType.MoveLineDown)
		{
			this.isUp = isUp;
		}

		private SelectionMemento[] mementos;
		private Range[] ranges;

		override public bool Init()
		{
			int minIndex = 0;
			int maxIndex = 0;
			if (selections.Count > 0)
			{
				minIndex = selections[0].anchor;
				maxIndex = selections[0].anchor;
			}
			for (int i = 0; i < selections.Count; i++)
			{
				Selection selection = selections[i];
				if (minIndex > selection.anchor)
					minIndex = selection.anchor;
				if (maxIndex < selection.anchor)
					maxIndex = selection.anchor;
				if (minIndex > selection.caret)
					minIndex = selection.caret;
				if (maxIndex < selection.caret)
					maxIndex = selection.caret;
			}
			if (isUp && lines.PlaceOf(minIndex).iLine <= 0 ||
				!isUp && lines.PlaceOf(maxIndex).iLine >= lines.LinesCount - 1)
				return false;

			lines.JoinSelections();
			mementos = GetSelectionMementos();
			List<Range> ranges = new List<Range>();
			int iRange = 0;
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				Place place0 = lines.PlaceOf(memento.Left);
				Place place1 = lines.PlaceOf(memento.Right);
				int iLine0 = place0.iLine;
				int iLine1 = place1.iLine;
				int start = memento.Left - place0.iChar;
				if (iRange > 0)
				{
					Range prevRange = ranges[iRange - 1];
					if (prevRange.iLine1 >= iLine0 - 1)
					{
						prevRange.iLine1 = iLine1;
						ranges[iRange - 1] = prevRange;
					}
					else
					{
						ranges.Add(new Range(iLine0, iLine1, start));
						iRange++;
					}
				}
				else
				{
					ranges.Add(new Range(iLine0, iLine1, start));
					iRange++;
				}
			}
			this.ranges = ranges.ToArray();
			return true;
		}

		override public void Redo()
		{
			SetSelectionMementos(mementos);
			int move = isUp ? -1 : 1;
			List<Place> places = new List<Place>();
			for (int i = 0; i < selections.Count; i++)
			{
				Selection selection = selections[i];
				Place anchor = lines.PlaceOf(selection.anchor);
				Place caret = lines.PlaceOf(selection.caret);
				places.Add(anchor);
				places.Add(caret);
				anchor.iLine += move;
				caret.iLine += move;
				selection.anchor = lines.IndexOf(anchor);
				selection.caret = lines.IndexOf(caret);
			}
			for (int i = 0; i < ranges.Length; i++)
			{
				Range range = ranges[i];
				LineIterator iterator = isUp ?
					lines.GetLineRange(range.iLine0, range.iLine1 - range.iLine0 + 1) :
					lines.GetLineRange(range.iLine1, range.iLine0 - range.iLine1 - 1);
				while (iterator.MoveNext())
				{
					iterator.SwapCurrent(isUp);
				}
			}
			for (int i = 0; i < selections.Count; i++)
			{
				Selection selection = selections[i];
				Place anchor = places[i * 2];
				Place caret = places[i * 2 + 1];
				anchor.iLine += move;
				caret.iLine += move;
				selection.anchor = lines.IndexOf(anchor);
				selection.caret = lines.IndexOf(caret);
			}
		}

		override public void Undo()
		{
			for (int i = 0; i < ranges.Length; i++)
			{
				Range range = ranges[i];
				LineIterator iterator = isUp ?
					lines.GetLineRange(range.iLine1, range.iLine0 - range.iLine1 - 1) :
					lines.GetLineRange(range.iLine0, range.iLine1 - range.iLine0 + 1);
				while (iterator.MoveNext())
				{
					iterator.SwapCurrent(isUp);
				}
			}
			SetSelectionMementos(mementos);
		}
	}
}
