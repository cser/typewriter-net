using System;
using System.Collections.Generic;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class BufferList
{
	public BufferList()
	{
	}

	public readonly SwitchList<Buffer> list = new SwitchList<Buffer>();

	public Frame frame;

	public Buffer GetBuffer(string fullPath, string name)
	{
		for (int i = list.Count; i-- > 0;)
		{
			Buffer buffer = list[i];
			if (buffer.FullPath == fullPath && buffer.Name == name)
				return buffer;
		}
		return null;
	}

	public Buffer GetByFullPath(BufferTag tags, string fullPath)
	{
		foreach (Buffer buffer in list)
		{
			if (buffer.FullPath == fullPath && (buffer.tags & tags) == tags)
				return buffer;
		}
		return null;
	}
}
