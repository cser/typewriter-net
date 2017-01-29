using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViCommandParser
	{
		private bool _ready;
		private bool _complete;
		
		public ViCommandParser()
		{
			Reset();
		}
		
		private const int INIT = 0;
		private const int COUNT = 1;
		private const int MOVE = 2;
		private const int ACTION = 3;
		
		private int _state;
		private string _stateText;
		
		public void AddKey(char code)
		{
			if (_complete)
			{
				Reset();
			}
			Parse(code);
		}
		
		private bool IsMove(char code)
		{
			switch (code)
			{
				case 'j':
				case 'k':
				case 'h':
				case 'l':
					return true;
			}
			return false;
		}
		
		private bool IsNeedMove(char code)
		{
			switch (code)
			{
				case 'i':
				case 'a':
					return false;
			}
			return true;
		}
		
		private void Parse(char code)
		{
			switch (_state)
			{
				case INIT:
					if (char.IsNumber(code))
					{
						_state = COUNT;
						Parse(code);
						return;
					}
					_state = ACTION;
					Parse(code);
					return;
				case COUNT:
					if (char.IsNumber(code))
					{
						_stateText += code;
						return;
					}
					if (_stateText != "")
					{
						count = int.Parse(_stateText);
						_stateText = "";
					}
					_state = ACTION;
					Parse(code);
					return;
				case MOVE:
					move = code;
					_ready = true;
					return;
				case ACTION:
					if (IsMove(code))
					{
						_state = MOVE;
						Parse(code);
						return;
					}
					action = code;
					if (!IsNeedMove(code))
					{
						_ready = true;
					}
					return;
			}
		}
		
		public bool TryComplete()
		{
			if (_ready)
			{
				_ready = false;
				_complete = true;
				return true;
			}
			return false;
		}
		
		private void Reset()
		{
			_ready = false;
			_complete = false;
			_state = INIT;
			_stateText = "";
			count = 1;
			move = '\0';
			action = '\0';
		}
		
		public int count;
		public char move;
		public char action;
	}
}