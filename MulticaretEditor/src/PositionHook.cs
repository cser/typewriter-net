using System;

namespace MulticaretEditor
{
	public class PositionHook : TextChangeHook
	{
		private readonly Controller _controller;
		
        public PositionHook(Controller controller)
        {
	        _controller = controller;
        }
        
		public override void InsertText(int index, string text)
		{
			MacrosExecutor executor = _controller.macrosExecutor;
			for (int i = 0; i < executor.positionHistory.Length; ++i)
			{
				PositionNode node = executor.positionHistory[i];
				if (node != null && node.file == executor.currentFile && node.position > index)
				{
					node.position += text.Length;
				}
			}
			for (int i = _controller.markbooks.Count; i-- > 0;)
			{
				int position = _controller.markbooks[i];
				if (position != -1 && position > index)
				{
					_controller.markbooks[i] = position + text.Length;
				}
			}
		}

		public override void RemoveText(int index, int count)
		{
			MacrosExecutor executor = _controller.macrosExecutor;
			for (int i = 0; i < executor.positionHistory.Length; ++i)
			{
				PositionNode node = executor.positionHistory[i];
				if (node != null && node.file == executor.currentFile && node.position > index)
				{
					node.position -= count;
					if (node.position < index)
					{
						node.position = index;
					}
				}
			}
			for (int i = _controller.markbooks.Count; i-- > 0;)
			{
				int position = _controller.markbooks[i];
				if (position != -1 && position > index)
				{
					position -= count;
					if (position < index)
					{
						_controller.markbookNames.RemoveAt(i);
						_controller.markbooks.RemoveAt(i);
					}
					else
					{
						_controller.markbooks[i] = position;
					}
				}
			}
		}
	}
}
