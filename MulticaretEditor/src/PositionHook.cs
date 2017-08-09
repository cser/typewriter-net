using System;

namespace MulticaretEditor
{
	public class PositionHook : TextChangeHook
	{
		private PositionNode[] _nodes;
		private PositionFile _file;
		
        public PositionHook(PositionNode[] nodes, PositionFile file)
        {
	        _nodes = nodes;
	        _file = file;
        }
        
		public override void InsertText(int index, string text)
		{
			for (int i = 0; i < _nodes.Length; ++i)
			{
				PositionNode node = _nodes[i];
				if (node.file == _file && node.position > index)
				{
					node.position += text.Length;
				}
			}
		}

		public override void RemoveText(int index, int count)
		{
			for (int i = 0; i < _nodes.Length; ++i)
			{
				PositionNode node = _nodes[i];
				if (node.file == _file && node.position > index)
				{
					node.position -= count;
					if (node.position < index)
					{
						node.position = index;
					}
				}
			}
		}
	}
}
