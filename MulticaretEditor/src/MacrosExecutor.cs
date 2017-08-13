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
		
		public ViMode viMode;

		private readonly Getter<MulticaretTextBox> getTextBox;
		
		public MacrosExecutor(Getter<MulticaretTextBox> getTextBox) : this(getTextBox, 20)
		{
		}

		public MacrosExecutor(Getter<MulticaretTextBox> getTextBox, int maxViPositions)
		{
			this.getTextBox = getTextBox;
			_maxViPositions = maxViPositions;
			positionHistory = new PositionNode[_maxViPositions];
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
		
		private int _maxViPositions;
		
		public PositionFile currentFile;
		public readonly PositionNode[] positionHistory;
		
		private PositionFile _lastFile;
		private int _lastPosition = -1;
		
		private PositionFile[] _files;
		private int _filesIndex;
		
		private PositionFile GetFile(string path)
		{
			if (currentFile != null && currentFile.path == path)
			{
				return currentFile;
			}
			if (_files != null)
			{
				for (int i = 0; i < _files.Length; ++i)
				{
					PositionFile file = _files[i];
					if (file != null && file.path == path)
					{
						return _files[i];
					}
				}
			}
			for (int i = bookmarkFiles.Count; i-- > 0;)
			{
				PositionFile file = bookmarkFiles[i];
				if (file.path == path)
				{
					return file;
				}
			}
			return null;
		}
		
		public void ViSetCurrentFile(string path)
		{
			if (currentFile != null && currentFile.path == path)
			{
				return;
			}
			currentFile = GetFile(path);
			if (currentFile == null)
			{
				currentFile = new PositionFile(path);
				if (_files == null)
				{
					_files = new PositionFile[_maxViPositions];
				}
				_files[_filesIndex] = currentFile;
				_filesIndex = (_filesIndex + 1) % _maxViPositions;
			}
		}
		
		public void ViRenameFile(string oldFile, string newFile)
		{
			if (_files != null)
			{
				for (int i = 0; i < _files.Length; ++i)
				{
					PositionFile file = _files[i];
					if (file != null && file.path == oldFile)
					{
						file.path = newFile;
					}
				}
			}
		}
		
		private int _offset;
		private int _prevCount;
		private int _nextCount;
		
		public void ViPositionAdd(int position)
		{
			if (currentFile != null && (_lastFile != currentFile || _lastPosition != position))
			{
				_lastFile = currentFile;
				_lastPosition = position;
				if (_nextCount > 0)
				{
					++_prevCount;
					for (int i = 0; i < _nextCount - 1; ++i)
					{
						positionHistory[(_offset + _prevCount + i) % _maxViPositions] = null;
					}
					_nextCount = 0;
				}
				positionHistory[(_offset + _prevCount) % _maxViPositions] = new PositionNode(currentFile, position);
				++_nextCount;
				if (_prevCount + _nextCount > _maxViPositions)
				{
					_offset = (_offset + 1) % _maxViPositions;
					--_prevCount;
				}
			}
		}
		
		public void ViPositionSet(int position)
		{
			if (_nextCount > 0)
			{
				PositionNode node = positionHistory[(_offset + _prevCount) % _maxViPositions];
				if (node.file == currentFile && _lastFile == currentFile)
				{
					_lastPosition = position;
					node.position = position;
					if (_nextCount > 0)
					{
						for (int i = 1; i < _nextCount; ++i)
						{
							positionHistory[(_offset + _prevCount + i) % _maxViPositions] = null;
						}
						_nextCount = 1;
					}
					return;
				}
			}
			ViPositionAdd(position);
		}
		
		public PositionNode ViPositionPrev()
		{
			PositionNode node = null;
			if (_prevCount > 0)
			{
				node = positionHistory[(_offset + _prevCount - 1) % _maxViPositions];
				--_prevCount;
				++_nextCount;
			}
			return node;
		}
		
		public PositionNode ViPositionNext()
		{
			PositionNode node = null;
			if (_nextCount > 1)
			{
				++_prevCount;
				--_nextCount;
				node = positionHistory[(_offset + _prevCount) % _maxViPositions];
			}
			return node;
		}
		
		public string GetDebugText()
		{
			string text = "[";
			for (int i = 0; i < _prevCount; ++i)
			{
				text += positionHistory[(_offset + i) % _maxViPositions];
			}
			text += "][";
			for (int i = 0; i < _nextCount; ++i)
			{
				text += positionHistory[(_offset + _prevCount + i) % _maxViPositions];
			}
			text += "]";
			if (_prevCount + _nextCount > _maxViPositions)
			{
				text += ":OVERFLOW";
			}
			int nullsCount = 0;
			for (int i = 0; i < positionHistory.Length; ++i)
			{
				if (positionHistory[i] == null)
				{
					++nullsCount;
				}
			}
			if (_prevCount + _nextCount + nullsCount != _maxViPositions)
			{
				text += ":UNNULLED";
			}
			return text;
		}
		
		public readonly List<PositionFile> bookmarkFiles = new List<PositionFile>(8);
		public readonly List<List<PositionChar>> bookmarks = new List<List<PositionChar>>();
		
		public void SetBookmark(char c, string path, int position)
		{
			if (c >= 'A' && c <= 'Z')
			{
				PositionFile file = GetFile(path) ?? new PositionFile(path);
				for (int i = bookmarks.Count; i-- > 0;)
				{
					List<PositionChar> pcs = bookmarks[i];
					for (int j = pcs.Count; j-- > 0;)
					{
						PositionChar pc = pcs[j];
						if (pc.c == c)
						{
							pcs.RemoveAt(j);
							break;
						}
					}
					if (pcs.Count == 0)
					{
						bookmarks.RemoveAt(i);
					}
				}
				int index = bookmarkFiles.IndexOf(file);
				if (index == -1)
				{
					index = bookmarkFiles.Count;
					bookmarkFiles.Add(file);
					bookmarks.Add(new List<PositionChar>(4));
				}
				bookmarks[index].Add(new PositionChar(c, position));
			}
		}
		
		public void GetBookmark(char c, out string path, out int position)
		{
			path = null;
			position = -1;
			if (c >= 'A' && c <= 'Z')
			{
				for (int i = bookmarks.Count; i-- > 0;)
				{
					List<PositionChar> pcs = bookmarks[i];
					for (int j = pcs.Count; j-- > 0;)
					{
						PositionChar pc = pcs[j];
						if (pc.c == c)
						{
							path = bookmarkFiles[i].path;
							position = pc.position;
							return;
						}
					}
					if (pcs.Count == 0)
					{
						bookmarks.RemoveAt(i);
					}
				}
			}
			return;
		}
	}
}