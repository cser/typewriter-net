using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class ViShiftCommand : Command
	{
		private readonly bool isLeft;
		private readonly List<SimpleRange> ranges;

		public ViShiftCommand(List<SimpleRange> ranges, bool isLeft) : base(isLeft ? CommandType.ShiftLeft : CommandType.ShiftRight)
		{
			this.ranges = ranges;
			this.isLeft = isLeft;
		}
		
		private SelectionMemento[] mementos;

		override public bool Init()
		{
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}

		override public void Redo()
		{
			for (int i = 0; i < ranges.Count; i++)
			{
				SimpleRange range = ranges[i];
				LineIterator iterator = lines.GetLineRange(range.index, range.count);
				while (iterator.MoveNext())
				{
					Line line = iterator.current;
				}
			}
		}

		override public void Undo()
		{
		}
	}
}
