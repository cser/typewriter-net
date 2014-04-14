using System;
using System.Drawing;
using MulticaretEditor;

public class Settings
{
	public event Setter Changed;

	public Settings()
	{
	}

	private void DispatchChange()
	{
		if (Changed != null)
			Changed();
	}

	public class Field<T>
	{
		private readonly Setter dispatchChange;

		public Field(T value, Setter dispatchChange)
		{
			this.value = value;
			this.dispatchChange = dispatchChange;
		}

		private T value;
		public T Value
		{
			get { return value; }
			set
			{
				if (!this.value.Equals(value))
				{
					this.value = value;
					changed = true;
					if (dispatchChange != null)
						dispatchChange();
				}
			}
		}

		private bool changed = true;
		public bool Changed { get { return changed; } }

		public void MarkReaded()
		{
			changed = false;
		}
	}
}
