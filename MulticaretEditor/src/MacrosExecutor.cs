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
		public string currentFile;
		
		private PositionNode[] _nodes;
		private int _offset;
		private int _prevCount;
		private int _nextCount;
		
		public PositionNode[] PositionHistory { get { return _nodes; } }
		
		public void ViPositionAdd(int position)
		{
			if (currentFile != null)
			{
				if (_nodes == null)
				{
					_nodes = new PositionNode[maxViPositions];
				}
				if (_nextCount > 0)
				{
					++_prevCount;
					for (int i = 0; i < _nextCount - 1; i++)
					{
						_nodes[(_offset + _prevCount + i) % maxViPositions] = null;
					}
					_nextCount = 0;
				}
				_nodes[(_offset + _prevCount) % maxViPositions] = new PositionNode(currentFile, position);
				++_nextCount;
				if (_prevCount + _nextCount > maxViPositions)
				{
					_offset = (_offset + 1) % maxViPositions;
					--_prevCount;
				}
			}
		}
		
		public PositionNode ViPositionPrev()
		{
			PositionNode node = null;
			if (_prevCount > 0)
			{
				node = _nodes[(_offset + _prevCount - 1) % maxViPositions];
				--_prevCount;
				++_nextCount;
			}
			return node;
		}
		
		public PositionNode ViPositionNext()
		{
			if (_nextCount <= 0)
			{
				return null;
			}
			++_prevCount;
			--_nextCount;
			return _nextCount > 0 ? _nodes[(_offset + _prevCount) % maxViPositions] : null;
		}
	}
}
