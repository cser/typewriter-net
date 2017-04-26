using System;
using System.Text;

namespace MulticaretEditor.Commands
{
	public class CopyCommand : Command
	{
		public CopyCommand() : base(CommandType.Copy)
		{
		}
		
		override public bool Init()
		{
			lines.JoinSelections();
			if (lines.AllSelectionsEmpty)
			{
				return false;
			}
			StringBuilder text = new StringBuilder();
			SelectionMemento[] mementos = GetSelectionMementos();
			bool first = true;
			foreach (SelectionMemento memento  in mementos)
			{
				if (!first)
					text.Append('\n');
				first = false;
				text.Append(lines.GetText(memento.Left, memento.Count));
			}
			ClipboardExecuter.PutToClipboard(text.ToString());
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
