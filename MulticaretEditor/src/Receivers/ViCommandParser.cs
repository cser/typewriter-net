using System;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

namespace MulticaretEditor
{
	public class ViCommandParser
	{
		private const int INIT = 0;
		private const int COUNT = 1;
		private const int ACTION = 2;
		private const int WAIT_CHAR = 3;
		
		private bool _ready;
		private bool _complete;
		private int _state;
		private string _stateText;
		
		public int count;
		public ViChar move;
		public ViChar moveChar;
		public ViChar action;
		
		public ViCommandParser()
		{
			Reset();
		}
		
		public void Reset()
		{
			Console.WriteLine("    Reset()");
			_ready = false;
			_complete = false;
			_state = INIT;
			_stateText = "";
			
			count = 1;
			move = new ViChar('\0', false);
			moveChar = new ViChar('\0', false);
			action = new ViChar('\0', false);
		}
		
		public void AddKey(ViChar code)
		{
			Console.WriteLine("    AddKey(" + code + ")");
			if (_complete)
			{
				Reset();
			}
			Parse(code);
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
		
		private void Parse(ViChar code)
		{
			switch (_state)
			{
				case INIT:
					if (char.IsNumber(code.c))
					{
						_state = COUNT;
						Parse(code);
						return;
					}
					_state = ACTION;
					Parse(code);
					return;
				case COUNT:
					if (char.IsNumber(code.c))
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
				case ACTION:
					switch (code.c)
					{
						case 'j':
						case 'k':
						case 'h':
						case 'l':
						case 'w':
						case 'b':
							move = code;
							_ready = true;
							return;
						case 'f':
							_state = WAIT_CHAR;
							move = code;
							return;
					}
					switch (code.c)
					{
						case 'i':
						case 'a':
						case 'u':
							action = code;
							Console.WriteLine("    action=" + code);
							_ready = true;
							break;
						default:
							action = code;
							Console.WriteLine("    action=" + code);
							break;
					}
					return;
				case WAIT_CHAR:
					moveChar = code;
					_ready = true;
					return;
			}
		}
	}
}