using System;
using System.Collections.Generic;

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
			int pcIndex = _controller.macrosExecutor.bookmarkFiles.IndexOf(currentFile);
			if (pcIndex != -1)
			{
				List<PositionChar> pcs = _controller.macrosExecutor.bookmarks[pcIndex];
				for (int i = pcs.Count; i-- > 0;)
				{
					PositionChar pc = pcs[i];
					if (pc.position > pcIndex)
					{
						pc.position += text.Length;
						pcs[i] = pc;
					}
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
			int pcIndex = _controller.macrosExecutor.bookmarkFiles.IndexOf(currentFile);
			if (pcIndex != -1)
			{
				List<PositionChar> pcs = _controller.macrosExecutor.bookmarks[pcIndex];
				for (int i = pcs.Count; i-- > 0;)
				{
					PositionChar pc = pcs[i];
					if (pc.position > pcIndex)
					{
						pc.position -= count;
						if (pc.position < pcIndex)
						{
							pcs.RemoveAt(i);
							if (pcs.Count == 0)
							{
								_controller.bookmarks.RemoveAt(pcIndex);
								break;
							}
						}
						else
						{
							pcs[i] = pc;
						}
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
