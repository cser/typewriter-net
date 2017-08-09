using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MulticaretEditor
{
	public class AReceiver
	{
		public virtual bool AltMode { get { return false; } }
		public virtual ViMode ViMode { get { return ViMode.Normal; } }
		
		protected Controller controller;
		protected LineArray lines;
		protected Receiver.Context context;
		
		public virtual bool IsIdle { get { return true; } }
		public virtual ViJumpReceiver AsJump { get { return null; } }
		
		public void Init(Controller controller, Receiver.Context context)
		{
			this.controller = controller;
			this.lines = controller.Lines;
			this.context = context;
		}
		
		protected void ProcessInputChar(char code)
		{
			switch (code)
			{
				case '\b':
					if (lines.AllSelectionsEmpty)
					{
						controller.Backspace();
					}
					else
					{
						controller.EraseSelection();
					}
					break;
				case '\r':
					controller.InsertLineBreak();
					break;
				default:
					controller.InsertText(code + "");
					break;
			}
		}
		
		public virtual void DoOn()
		{
		}
		
		public virtual void DoKeyPress(char code, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			scrollToCursor = false;
		}
		
		public virtual bool DoKeyDown(Keys keysData, out string viShortcut, out bool scrollToCursor)
		{
			viShortcut = null;
			scrollToCursor = false;
			return false;
		}
		
		public virtual bool DoFind(Pattern pattern)
		{
			return false;
		}
		
		public virtual void ResetViInput()
		{
		}
	}
}