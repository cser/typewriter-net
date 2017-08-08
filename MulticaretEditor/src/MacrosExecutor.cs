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
		
		private PositionNode[] viPositions;
		private int viStart = 0;
		private int viIndex = -1;
		private int viCount = 0;
		
		public void ViPositionAdd(string file, int position, bool asNew)
		{
			if (viPositions == null)
			{
				viPositions = new PositionNode[maxViPositions];
			}
			viIndex = (viIndex + 1) % maxViPositions;
			viPositions[viIndex] = new PositionNode(file, position);
			++viCount;
			if (viCount > maxViPositions)
			{
				viCount = maxViPositions;
				viStart = (viStart + 1) % maxViPositions;
			}
		}
		
		public PositionNode ViPositionPrev()
		{
			Console.Write("!" + viCount);
			--viCount;
			if (viCount < 0)
			{
				viCount = 0;
				return null;
			}
			viIndex = (viIndex + maxViPositions - 1) % maxViPositions;
			return viPositions[viIndex];
		}
		
		public PositionNode ViPositionNext()
		{
			viIndex = (viIndex + 1) % maxViPositions;
			PositionNode node = viPositions[viIndex];
			return node;
		}
	}
}
