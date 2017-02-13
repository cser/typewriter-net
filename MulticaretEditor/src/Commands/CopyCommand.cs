using System;
using System.Text;

namespace MulticaretEditor
{
	public class CopyCommand : Command
	{
		private readonly char register;
		
		public CopyCommand(char register) : base(CommandType.Copy)
		{
			this.register = register;
		}
		
		override public bool Init()
		{
			lines.JoinSelections();
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
			ClipboardExecuter.PutToRegister(register, text.ToString());
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
