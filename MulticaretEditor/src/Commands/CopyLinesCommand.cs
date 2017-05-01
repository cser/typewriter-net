using System;
using System.Text;

namespace MulticaretEditor
{
	public class CopyLinesCommand : Command
	{
		private readonly char register;
		
		public CopyLinesCommand(char register) : base(CommandType.CopyLines)
		{
			this.register = register;
		}
		
		override public bool Init()
		{
			lines.JoinSelections();
			if (!lines.AllSelectionsEmpty)
			{
				return false;
			}
			StringBuilder text = new StringBuilder();
			SelectionMemento[] mementos = GetSelectionMementos();
			bool first = true;
			int lastLineIndex = -1;
			foreach (SelectionMemento memento  in mementos)
			{
				Place place = lines.PlaceOf(memento.caret);
				if (first)
				{
					first = false;
				}
				else if (place.iLine == lastLineIndex)
				{
					continue;
				}
				lastLineIndex = place.iLine;
				Line line = lines[place.iLine];
				int normalCount = line.NormalCount;
				Char[] chars = line.chars;
				for (int i = 0; i < normalCount; ++i)
				{
					text.Append(chars[i].c);
				}
				if (normalCount < line.charsCount)
				{
					text.Append(line.GetRN());
				}
				else
				{
					text.Append(lines.lineBreak);
				}
			}
			ClipboardExecuter.PutToRegister('*', text.ToString());
			return false;
		}
		
		override public void Redo()
		{
		}
		
		override public void Undo()
		{
		}
	}
}
