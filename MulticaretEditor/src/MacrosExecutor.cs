using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MulticaretEditor
{
	public class MacrosExecutor
	{
		public struct Action
		{
			public char code;
			public Keys keys;
			public bool? mode;

			public Action(char code)
			{
				this.code = code;
				this.keys = Keys.None;
				this.mode = null;
			}

			public Action(Keys keys)
			{
				this.code = '\0';
				this.keys = keys;
				this.mode = null;
			}

			public Action(Keys keys, bool mode)
			{
				this.code = '\0';
				this.keys = keys;
				this.mode = mode;
			}
		}
		
		public bool viMode;

		private readonly Getter<MulticaretTextBox> getTextBox;

		public MacrosExecutor(Getter<MulticaretTextBox> getTextBox)
		{
			this.getTextBox = getTextBox;
		}

		public List<Action> current;

		private List<Action> recorded;

		public void RecordOnOff()
		{
			if (current == null)
			{
				current = new List<Action>();
			}
			else
			{
				recorded = current;
				current = null;
			}
		}

		public void Execute()
		{
			if (recorded == null || getTextBox == null)
				return;
			for (int i = 0, count = recorded.Count; i < recorded.Count; i++)
			{
				MulticaretTextBox tb = getTextBox();
				if (tb == null)
					return;
				Action action = recorded[i];
				tb.ProcessMacrosAction(action);
			}
		}
		
		public int maxViPositions = 20;
		
		private List<PositionNode> _prev = new List<PositionNode>();
		private List<PositionNode> _next = new List<PositionNode>();
		
		public void ViPositionAdd(string file, int position)
		{
			if (_next.Count > 0)
			{
				_prev.Add(_next[_next.Count - 1]);
				_next.Clear();
			}
			_next.Add(new PositionNode(file, position));
			if (_prev.Count + _next.Count > maxViPositions)
			{
				_prev.RemoveAt(0);
			}
		}
		
		public PositionNode ViPositionPrev()
		{
			PositionNode node = null;
			if (_prev.Count > 0)
			{
				node = _prev[_prev.Count - 1];
				_prev.RemoveAt(_prev.Count - 1);
				_next.Add(node);
			}
			return node;
		}
		
		public PositionNode ViPositionNext()
		{
			if (_next.Count == 0)
			{
				return null;
			}
			PositionNode node = _next[_next.Count - 1];
			_next.RemoveAt(_next.Count - 1);
			_prev.Add(node);
			return _next.Count > 0 ? _next[_next.Count - 1] : null;
		}
	}
}
