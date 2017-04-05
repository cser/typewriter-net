using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor.Commands
{
	public class FixLineBreaksCommand : Command
	{
		public struct SelectionPlace
		{
			public Place anchor;
			public Place caret;
			
			public SelectionPlace(Place anchor, Place caret)
			{
				this.anchor = anchor;
				this.caret = caret;
			}
		}
		
		public FixLineBreaksCommand() : base(CommandType.FixLineBreaks)
		{
		}

		private SelectionMemento[] mementos;
		private string[] lineBreaks;
		private string linesLineBreak;

		override public bool Init()
		{
			bool allow = false;
			for (int i = 0; i < lines.blocksCount && !allow; i++)
			{
				LineBlock block = lines.blocks[i];
				for (int j = 0; j < block.count; j++)
				{
					if (block.array[j].GetRN() != lines.lineBreak)
					{
						allow = true;
						break;
					}
				}
			}
			if (!allow)
				return false;

			linesLineBreak = lines.lineBreak;
			mementos = GetSelectionMementos();
			return true;
		}

		override public void Redo()
		{
			SetSelectionMementos(mementos);
			List<SelectionPlace> places = new List<SelectionPlace>();
			foreach (Selection selection in lines.selections)
			{
				places.Add(new SelectionPlace(lines.PlaceOf(selection.anchor), lines.PlaceOf(selection.caret)));
			}
			
			lineBreaks = new string[lines.LinesCount];
			LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
			while (iterator.MoveNext())
			{
				Line line = iterator.current;
				string lineBreak = line.GetRN();
				lineBreaks[iterator.Index] = lineBreak;
				if (lineBreak != linesLineBreak && lineBreak != "")
				{
					line.Chars_RemoveRange(line.charsCount - lineBreak.Length, lineBreak.Length);
					line.Chars_Add(new Char(linesLineBreak[0]));
					if (linesLineBreak.Length > 1)
						line.Chars_Add(new Char(linesLineBreak[1]));
					iterator.InvalidateCurrentText(linesLineBreak.Length - lineBreak.Length);
				}
			}
			
			for (int i = places.Count; i-- > 0;)
			{
				SelectionPlace place = places[i];
				Selection selection = lines.selections[i];
				selection.anchor = lines.IndexOf(place.anchor);
				selection.caret = lines.IndexOf(place.caret);
			}
			
			lines.cachedText = null;
		}

		override public void Undo()
		{
			LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
			while (iterator.MoveNext())
			{
				Line line = iterator.current;
				string lineBreak = lineBreaks[iterator.Index];
				if (lineBreak != linesLineBreak && lineBreak != "")
				{
					line.Chars_RemoveRange(line.charsCount - linesLineBreak.Length, linesLineBreak.Length);
					line.Chars_Add(new Char(lineBreak[0]));
					if (lineBreak.Length > 1)
						line.Chars_Add(new Char(lineBreak[1]));
					iterator.InvalidateCurrentText(lineBreak.Length - linesLineBreak.Length);
				}
			}
			SetSelectionMementos(mementos);
			lines.cachedText = null;
		}
	}
}
