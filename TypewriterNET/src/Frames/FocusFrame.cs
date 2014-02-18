using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TypewriterNET.Frames
{
	public abstract class FocusFrame
	{
		private readonly bool hideOnLostFocus;

		public FocusFrame(bool hideOnLostFocus)
		{
			this.hideOnLostFocus = hideOnLostFocus;
		}

		private Control target;
		public Control Target { get { return target; } }
		
		private bool created = false;
		private FocusFrame parent;

		public void Show(FocusFrame parent)
		{
			if (!created)
			{
				created = true;
				this.parent = parent;
				target = DoCreateControl();
				target.GotFocus += OnGotFocus;
				target.LostFocus += OnLostFocus;
				if (target.Focused)
					DoOnGotFocus();
			}
		}

		public void Hide()
		{
			if (created)
			{
				created = false;
				bool needChangeFocus = target.Focused;
				target.GotFocus -= OnGotFocus;
				target.LostFocus -= OnLostFocus;
				DoDestroyControl();
				target = null;
				if (needChangeFocus)
				{
					DoOnLostFocus();
					parent.Focus();
				}
			}
		}

		public void Focus()
		{
			if (created && target != null)
				target.Focus();
		}

		private void OnGotFocus(object sender, EventArgs e)
		{
			DoOnGotFocus();
		}

		private void OnLostFocus(object sender, EventArgs e)
		{
			if (created && hideOnLostFocus)
				Hide();
		}

		virtual protected void DoOnGotFocus()
		{
		}

		virtual protected void DoOnLostFocus()
		{
		}

		abstract protected Control DoCreateControl();

		abstract protected void DoDestroyControl();
	}
}
