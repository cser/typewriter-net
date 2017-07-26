using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor
{
	public class CopyLinesCommand : Command
	{
		private readonly char register;
		private readonly List<SimpleRange> ranges;
		
		public CopyLinesCommand(char register, List<SimpleRange> ranges) : base(CommandType.CopyLines)
		{
			this.register = register;
			this.ranges = ranges;
		}
		
		override public bool Init()
		{
			lines.JoinSelections();
			StringBuilder text = new StringBuilder();
			foreach (SimpleRange range  in ranges)
			{
				if (range.count > 0)
				{
					LineIterator iterator = lines.GetLineRange(range.index, range.count);
					while (iterator.MoveNext())
					{
						Line line = iterator.current;
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
				}
			}
			ClipboardExecutor.PutToRegister(register, text.ToString());
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
