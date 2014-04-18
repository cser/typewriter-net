using System;
using System.Drawing;

public class FrameList
{
	private readonly MainForm mainForm;
	private readonly NestList list;

	public FrameList(MainForm mainForm)
	{
		this.mainForm = mainForm;
		list = new NestList();
	}

	public void Resize(int x, int y, Size size)
	{
		if (list.Head != null)
		{
			list.Head.Update();
			list.Head.Resize(x, y, size.Width, size.Height);
		}
	}

	public Nest AddParentNode()
	{
		Nest nest = new Nest(mainForm);
		list.AddParent(nest);
		return nest;
	}

	public Frame GetFocusedFrame()
	{
		for (Nest nestI = list.Head; nestI != null; nestI = nestI.Child)
		{
			if (nestI.Frame != null && nestI.Frame.Focused)
				return nestI.Frame;
		}
		return null;
	}

	public Buffer GetSelectedBuffer()
	{
		Frame frame = GetFocusedFrame();
		return frame != null ? frame.SelectedBuffer : null;
	}

	public Frame GetFirstFrame()
	{
		for (Nest nestI = list.Head; nestI != null; nestI = nestI.Child)
		{
			if (nestI.Frame != null)
				return nestI.Frame;
		}
		return null;
	}

	public Frame GetChildFrame(Frame frame)
	{
		Nest nest = frame != null ? frame.Nest : null;
		if (nest != null)
		{
			for (Nest nestI = nest.Child; nestI != null; nestI = nestI.Child)
			{
				if (nestI.Frame != null)
					return nestI.Frame;
			}
		}
		return null;
	}

	public void Remove(Nest nest)
	{
		list.Remove(nest);
	}
}
