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
		private Memento[] deleted;

		override public bool Init()
		{
			lines.JoinSelections();
			mementos = GetSelectionMementos();
			return true;
		}

		override public void Redo()
		{
			int deletedLength = 0;
			for (int i = 0; i < ranges.Count; i ++)
			{
				SimpleRange range = ranges[i];
				deletedLength += range.count;
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
			int offset = 0;
			for (int i = 0; i < ranges.Count; i++)
			{
				SimpleRange range = ranges[i];
				LineIterator iterator = lines.GetLineRange(range.index, range.count);

				int lineStart = lines.IndexOf(new Place(0, range.index));
				while (iterator.MoveNext())
				{
					Line line = iterator.current;
					int oldCount = line.charsCount;
					string deletedI;
					List<char> chars = new List<char>();
					int tabsCount;
					line.GetFirstIntegerTabs(out deletedI, out tabsCount);
					if (isLeft)
					{
						if (lines.spacesInsteadTabs)
						{
							AddChars(chars, ' ', (tabsCount - 1) * lines.tabSize);
						}
						else
						{
							AddChars(chars, '\t', tabsCount - 1);
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
							AddChars(chars, ' ', (tabsCount + 1) * lines.tabSize);
						}
						else
						{
							AddChars(chars, '\t', tabsCount + 1);
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
						if (delta < 0 && selectionValue - lineStart >= 0 && selectionValue - lineStart < -delta)
						{
							selectionValue = lineStart;
						}
						else if (selectionValue >= lineStart)
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
					lineStart += oldCount;
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
		
		private static void AddChars(List<char> chars, char c, int count)
		{
			for (int i = count; i-- > 0;)
			{
				chars.Add(c);
			}
		}

		override public void Undo()
		{
			int k = 0;
			for (int i = 0; i < ranges.Count; i++)
			{
				SimpleRange range = ranges[i];
				LineIterator iterator = lines.GetLineRange(range.index, range.count);

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
