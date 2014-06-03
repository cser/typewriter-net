using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;

public class FileTreeProcessor
{
	private Buffer buffer;
	public Buffer Buffer { get { return buffer; } }

	public FileTreeProcessor()
	{
		buffer = new Buffer(null, "File tree");
		buffer.Controller.isReadonly = true;
		buffer.additionKeyMap = new KeyMap();
		buffer.additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, new KeyAction("F&ind\\Navigate to finded", DoOnEnter, null, false)));
	}

	private bool DoOnEnter(Controller controller)
	{
		return true;
	}

	public void Reload()
	{
		Rebuild();
	}

	private void Rebuild()
	{
		StringBuilder builder = new StringBuilder();
		foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*"))
		{
			builder.AppendLine(file);
		}
		buffer.Controller.InitText(builder.ToString());
	}
}
