using System.Collections.Generic;
using System.Drawing;
using System;
using MulticaretEditor;

public class Nest
{
	public Nest child;
	public Nest Child { get { return child; } }

	public Nest parent;
	public Nest Parent { get { return parent; } }

	private NestList owner;
	public NestList Owner { get { return owner; } }

	public bool hDivided;
	public bool left;
	public bool isPercents = true;
	public int size = 50;
	public TempSettingsInt settingsSize;

	public Size minSize;
	public Size selfMinSize;
	public int FullWidth;
	public int FullHeight;

	public BufferList buffers;

	public Size frameSize;
	public Size FrameSize { get { return frameSize; } }

	private MainForm mainForm;
	public MainForm MainForm { get { return mainForm; } }

	private Setter<Nest> removeNest;

	public Nest(NestList owner, MainForm mainForm, Setter<Nest> removeNest)
	{
		this.owner = owner;
		this.mainForm = mainForm;
		this.removeNest = removeNest;
	}

	public void Destroy()
	{
		if (owner == null)
			return;
		if (AFrame != null)
			AFrame.Destroy();
		removeNest(this);

		mainForm = null;
		owner = null;
	}

	private AFrame frame;
	public AFrame AFrame
	{
		get { return frame; }
		set { frame = value; }
	}

	public Frame Frame { get { return frame as Frame; } }

	public int GetSize(int nestSize)
	{
		return isPercents ? nestSize * size / 100 : size;
	}

	public void SetFrameSize(Size size)
	{
		this.frameSize = size;
		frame.Size = size;
	}

	public void Update()
	{
		selfMinSize = frame != null ? frame.MinSize : new Size();
		if (child != null)
		{
			child.Update();
			minSize = hDivided ?
				new Size(selfMinSize.Width + child.minSize.Width, Math.Max(selfMinSize.Height, child.minSize.Height)) :
				new Size(Math.Max(selfMinSize.Width, child.minSize.Width), selfMinSize.Height + child.minSize.Height);
		}
		else
		{
			minSize = selfMinSize;
		}
	}

	public void Resize(int x, int y, int width, int height)
	{
		if (width < minSize.Width)
			width = minSize.Width;
		if (height < minSize.Height)
			height = minSize.Height;
		Nest nest = GetFilledNest(this);
		if (nest != null)
			nest.PrivateResize(x, y, width, height);
	}

	private Nest GetFilledNest(Nest nest)
	{
		for (Nest nestI = nest; nestI != null; nestI = nestI.Child)
		{
			if (nestI.AFrame != null)
				return nestI;
		}
		return null;
	}

	private void PrivateResize(int x, int y, int width, int height)
	{
		FullWidth = width;
		FullHeight = height;
		Nest child = GetFilledNest(this.child);
		if (child != null)
		{
			if (hDivided)
			{
				int size = GetSize(width);
				if (size < selfMinSize.Width)
					size = selfMinSize.Width;
				else if (width - size < child.minSize.Width)
					size = width - child.minSize.Width;
				SetFrameSize(new Size(size, height));
				if (left)
				{
					frame.Location = new Point(x, y);
					child.PrivateResize(x + size, y, width - size, height);
				}
				else
				{
					frame.Location = new Point(x + (width - size), y);
					child.PrivateResize(x, y, width - size, height);
				}
			}
			else
			{
				int size = GetSize(height);
				if (size < selfMinSize.Height)
					size = selfMinSize.Height;
				else if (height - size < child.minSize.Height)
					size = height - child.minSize.Height;
				SetFrameSize(new Size(width, size));
				if (left)
				{
					frame.Location = new Point(x, y);
					child.PrivateResize(x, y + size, width, height - size);
				}
				else
				{
					frame.Location = new Point(x, y + (height - size));
					child.PrivateResize(x, y, width, height - size);
				}
			}
		}
		else
		{
			frame.Location = new Point(x, y);
			SetFrameSize(new Size(width, height));
		}
	}

	public void MarkSizeAsChanged()
	{
		if (settingsSize != null)
		{
			settingsSize.value = size;
			settingsSize.changed = true;
		}
	}

	public void UpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.TempSettingsLoaded)
		{
			if (settingsSize != null && !settingsSize.changed)
				size = settingsSize.value;
		}
		if (AFrame != null)
			AFrame.UpdateSettings(settings, phase);
	}
	
	public bool HasRight()
	{
		Nest child = GetFilledNest(this.child);
		if (child != null && left && hDivided)
		{
			return true;
		}
		Nest parent = this.parent;
		for (; parent != null; parent = parent.parent)
		{
			if (parent.AFrame != null && parent.hDivided && !parent.left)
				return true;
		}
		return false;
	}
}
