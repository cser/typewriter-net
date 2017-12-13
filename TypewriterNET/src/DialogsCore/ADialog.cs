using System;
using MulticaretEditor;

public class ADialog : AFrame
{
	public ADialog()
	{
	}

	public event Setter NeedClose;
	
	public bool preventOpen = false;
	public MulticaretTextBox textBoxToFocus;

	public void DispatchNeedClose()
	{
		if (NeedClose != null)
			NeedClose();
	}
}
