using System;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class CommandProcessor
	{
		public event Setter ChangedChange
		{
			add { history.ChangedChange += value; }
			remove { history.ChangedChange -= value; }
		}
		
		private readonly LineArray lines;
		private readonly Controller controller;
		private readonly List<Selection> selections;
		
		public bool needDispatchChange;
		public int forceBatchLevel;
		public readonly History history = new History();
		
		public CommandProcessor(Controller controller, LineArray lines, List<Selection> selections)
		{
			this.controller = controller;
			this.lines = lines;
			this.selections = selections;
		}
		
		public void DoAfterInitText()
		{
			history.Reset();
			history.MarkAsSaved();
		}
		
		public void BeginBatch()
		{
			++forceBatchLevel;
		}
		
		public void EndBatch()
		{
			--forceBatchLevel;
		}
		
		private CommandType lastCommandType = CommandType.None;
		private long lastTime;

		public void ResetCommandsBatching()
		{
			if (forceBatchLevel <= 0)
			{
				lastCommandType = CommandType.None;
				lastTime = 0;
			}
		}
		
		public long debugNowMilliseconds = -1;
		
		public long GetNowMilliseconds()
		{
			return debugNowMilliseconds != -1 ?
				debugNowMilliseconds :
				(long)(new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds);
		}

		public bool Execute(Command command)
		{
			if (controller.isReadonly)
				return false;
			command.lines = lines;
			command.selections = selections;
			long time = GetNowMilliseconds();
			if (forceBatchLevel <= 0 && command.type != lastCommandType ||
				time - lastTime > 1000)
			{
				if (history.LastCommand != null)
					history.LastCommand.marked = true;
				lastCommandType = command.type;
				lastTime = time;
			}
			bool result = command.Init();
			if (result)
				history.ExecuteInited(command);
			needDispatchChange = true;
			return result;
		}

		public void Undo()
		{
			ResetCommandsBatching();
			bool changed = false;
			while (true)
			{
				if (history.Undo())
					changed = true;
				if (history.LastCommand == null || history.LastCommand.marked)
					break;
			}
			if (changed)
				needDispatchChange = true;
		}

		public void Redo()
		{
			ResetCommandsBatching();
			while (true)
			{
				if (history.NextCommand == null)
					break;
				if (history.NextCommand.marked)
				{
					history.Redo();
					break;
				}
				history.Redo();
			}
		}
		
		public void TagsModeOn()
		{
			history.TagsModeOn();
		}
		
		public void TagsModeOff()
		{
			history.TagsModeOff();
		}
		
		public void TagsDown()
		{
			history.TagsDown();
		}
		
		public void MarkAsSaved()
		{
			history.MarkAsSaved();
		}
		
		public void MarkAsFullyUnsaved()
		{
			history.MarkAsFullyUnsaved();
		}
		
		public bool CanUndo { get { return history.CanUndo; } }
		public bool CanRedo { get { return history.CanRedo; } }
		public bool Changed { get { return history.Changed; } }
	}
}
