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
			PositionFile currentFile = _controller.macrosExecutor.currentFile;
			PositionNode[] positionHistory = _controller.macrosExecutor.positionHistory;
			for (int i = 0; i < positionHistory.Length; ++i)
			{
				PositionNode node = positionHistory[i];
				if (node != null && node.file == currentFile && node.position > index)
				{
					node.position += text.Length;
				}
			}
			PositionNode[] bookmarks = _controller.macrosExecutor.bookmarks;
			for (int i = 0; i < bookmarks.Length; ++i)
			{
				PositionNode node = bookmarks[i];
				if (node != null && node.file == currentFile && node.position > index)
				{
					node.position += text.Length;
				}
			}
			for (int i = _controller.bookmarks.Count; i-- > 0;)
			{
				int position = _controller.bookmarks[i];
				if (position != -1 && position > index)
				{
					_controller.bookmarks[i] = position + text.Length;
				}
			}
		}

		public override void RemoveText(int index, int count)
		{
			PositionFile currentFile = _controller.macrosExecutor.currentFile;
			PositionNode[] positionHistory = _controller.macrosExecutor.positionHistory;
			for (int i = 0; i < positionHistory.Length; ++i)
			{
				PositionNode node = positionHistory[i];
				if (node != null && node.file == currentFile && node.position > index)
				{
					node.position -= count;
					if (node.position < index)
					{
						node.position = index;
					}
				}
			}
			PositionNode[] bookmarks = _controller.macrosExecutor.bookmarks;
			for (int i = 0; i < bookmarks.Length; ++i)
			{
				PositionNode node = bookmarks[i];
				if (node != null && node.file == currentFile && node.position > index)
				{
					node.position -= count;
					if (node.position < index)
					{
						bookmarks[i] = null;
					}
				}
			}
			for (int i = _controller.bookmarks.Count; i-- > 0;)
			{
				int position = _controller.bookmarks[i];
				if (position != -1 && position > index)
				{
					position -= count;
					if (position < index)
					{
						_controller.bookmarkNames.RemoveAt(i);
						_controller.bookmarks.RemoveAt(i);
					}
					else
					{
						_controller.bookmarks[i] = position;
					}
				}
			}
		}
	}
}
