using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class ShiftCommand : Command
	{
		private readonly bool isLeft;

		public ShiftCommand(bool isLeft) : base(isLeft ? CommandType.ShiftLeft : CommandType.ShiftRight)
		{
			this.isLeft = isLeft;
		}

		private Memento[] deleted;
		private Range[] ranges;
		private SelectionMemento[] mementos;

		override public bool Init()
		{
			bool allow = false;
			foreach (Selection selection in selections)
			{
				if (!selection.Empty)
				{
					Place place0 = lines.PlaceOf(selection.Left);
					Place place1 = lines.PlaceOf(selection.Right);
					if (place1.iLine - place0.iLine > 0)
					{
						allow = true;
						break;
					}
				}
			}
			if (!allow)
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
				if (iLine1 > iLine0 && place1.iChar == 0)
					iLine1--;
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
			int deletedLength = 0;
			for (int i = 0; i < ranges.Length; i ++)
			{
				Range range = ranges[i];
				deletedLength += range.iLine1 - range.iLine0 + 1;
			}
			deleted = new Memento[deletedLength];
			SelectionPart[] parts = new SelectionPart[mementos.Length * 2];
			for (int i = 0; i < mementos.Length; i++)
			{
				SelectionMemento memento = mementos[i];
				if (memento.anchor <= memento.caret)
				{
					parts[i * 2] = new SelectionPart(false, memento.index);
					parts[i * 2 + 1] = new SelectionPart(true, memento.index);
				}
				else
				{
					parts[i * 2] = new SelectionPart(true, memento.index);
					parts[i * 2 + 1] = new SelectionPart(false, memento.index);
				}
			}

			int k = 0;
			int iPart = 0;
			int start = 0;
			int offset = 0;
			for (int i = 0; i < ranges.Length; i++)
			{
				Range range = ranges[i];
				int iLine0 = range.iLine0;
				int iLine1 = range.iLine1;
				LineIterator iterator = lines.GetLineRange(iLine0, iLine1 - iLine0 + 1);

				start = range.start;
				while (iterator.MoveNext())
				{
					Line line = iterator.current;
					int oldCount = line.charsCount;
					string deletedI;
					int tabsCount;
					line.GetFirstIntegerTabs(out deletedI, out tabsCount);
					List<char> chars = new List<char>();
					if (isLeft)
					{
						if (lines.spacesInsteadTabs)
						{
							for (int j = 0; j < (tabsCount - 1) * lines.tabSize; j++)
							{
								chars.Add(' ');
							}
						}
						else
						{
							for (int j = 0; j < tabsCount - 1; j++)
							{
								chars.Add('\t');
							}
						}
						if (tabsCount == 0)
						{
							string spaces = "";
							for (int j = deletedI.Length; j < line.charsCount && line.chars[j].c == ' '; j++)
							{
								spaces += ' ';
							}
							deletedI = spaces + deletedI;
						}
					}
					else
					{
						if (lines.spacesInsteadTabs)
						{
							for (int j = 0; j < (tabsCount + 1) * lines.tabSize; j++)
							{
								chars.Add(' ');
							}
						}
						else
						{
							for (int j = 0; j < tabsCount + 1; j++)
							{
								chars.Add('\t');
							}
						}
					}
					line.Chars_RemoveRange(0, deletedI.Length);
					line.Chars_InsertRange(0, chars);
					int delta = chars.Count - deletedI.Length;
					iterator.InvalidateCurrentText(delta);
					deleted[k++] = new Memento(deletedI, chars.Count);
					while (iPart < parts.Length)
					{
						SelectionPart part = parts[iPart];
						Selection selection = selections[part.index];
						int selectionValue = part.isCaret ? selection.caret : selection.anchor;
						if (delta < 0 && selectionValue - start >= 0 && selectionValue - start < -delta)
						{
							selectionValue = start;
						}
						else if (selectionValue >= start)
						{
							break;
						}
						if (part.isCaret)
						{
							selection.caret = selectionValue + offset;
						}
						else
						{
							selection.anchor = selectionValue + offset;
						}
						iPart++;
					}
					offset += delta;
					start += oldCount;
				}
			}
			while (iPart < parts.Length)
			{
				SelectionPart part = parts[iPart++];
				if (part.isCaret)
				{
					selections[part.index].caret += offset;
				}
				else
				{
					selections[part.index].anchor += offset;
				}
			}
			lines.ResetTextCache();
		}

		override public void Undo()
		{
			int k = 0;
			for (int i = 0; i < ranges.Length; i++)
			{
				Range range = ranges[i];
				int iLine0 = range.iLine0;
				int iLine1 = range.iLine1;
				LineIterator iterator = lines.GetLineRange(iLine0, iLine1 - iLine0 + 1);

				while (iterator.MoveNext())
				{
					Memento deletedI = deleted[k++];
					Line line = iterator.current;
					line.Chars_RemoveRange(0, deletedI.count);
					line.Chars_InsertRange(0, deletedI.text);
					iterator.InvalidateCurrentText(deletedI.text.Length - deletedI.count);
				}
			}
			deleted = null;
			SetSelectionMementos(mementos);
			lines.ResetTextCache();
			lines.viStoreSelector.ViStoreMementos(mementos);
		}
	}
}
