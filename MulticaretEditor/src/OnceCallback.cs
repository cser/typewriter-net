using System;

namespace MulticaretEditor
{
	public struct OnceCallback
	{
		private Setter action;
		private bool processed;
		
		public OnceCallback(Setter action)
		{
			this.action = action;
			this.processed = false;
		}
		
		public void Execute()
		{
			if (!processed)
			{
				processed = true;
				if (action != null)
				{
					action();
				}
			}
		}
	}
}
