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
		
		public void ViSetCurrentFile(string path)
		{
			if (_files == null)
			{
				_files = new PositionFile[_maxViPositions];
			}
			for (int i = 0; i < _files.Length; ++i)
			{
				PositionFile file = _files[i];
				if (file != null && file.path == path)
				{
					currentFile = file;
					return;
				}
			}
			currentFile = new PositionFile(path);
			_files[_filesIndex] = currentFile;
			_filesIndex = (_filesIndex + 1) % _maxViPositions;
		}
		
		public void ViRenameFile(string oldFile, string newFile)
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
				++_prevCount;
				if (_prevCount + _nextCount > _maxViPositions)
				{
					_offset = (_offset + 1) % _maxViPositions;
					--_prevCount;
				}
			}
		}
		
		public string GetDebugText()
		{
			string text = "[";
			for (int i = 0; i < _prevCount; ++i)
			{
				text += positionHistory[(_offset + i) % _maxViPositions] + ";";
			}
			text += "][";
			for (int i = 0; i < _nextCount; ++i)
			{
				text += positionHistory[(_offset + _prevCount + i) % _maxViPositions] + ";";
			}
			text += "]";
			return text;
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
			if (_nextCount > 0)
			{
				++_prevCount;
				--_nextCount;
				node = positionHistory[(_offset + _prevCount - 1) % _maxViPositions];
			}
			return node;
		}
	}
}
