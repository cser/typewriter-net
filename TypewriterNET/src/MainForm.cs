using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Resources;
using System.Xml;
using MulticaretEditor;
using MulticaretEditor.Highlighting;
using MulticaretEditor.KeyMapping;
using TypewriterNET;
using TypewriterNET.Frames;

public class MainForm : Form
{
	private readonly string[] args;
	private readonly Timer settingsTimer;
	private readonly Settings settings;

	public MainForm(string[] args)
	{
		this.args = args;

		ResourceManager manager = new ResourceManager("TypewriterNET", typeof(Program).Assembly);
		Icon = (Icon)manager.GetObject("icon");
		Name = Application.ProductName;
		Text = Name;

		settingsTimer = new Timer();
		settingsTimer.Interval = 10;
		settingsTimer.Tick += OnSettingsTimer;

		settings = new Settings();
		settings.Changed += OnSettingsChanged;

		Load += OnLoad;
	}

	private void OnSettingsChanged()
	{
		settingsTimer.Start();
	}

	private void OnSettingsTimer(object sender, EventArgs e)
	{
		settingsTimer.Stop();
		ValidateSettings(false);
	}

	private void OnLoad(object sender, EventArgs e)
	{
		AddFrame(new Frame("main"), true, false, true, 50);
		AddFrame(new Frame("left"), true, true, true, 50);
		AddFrame(new Frame("left"), true, true, true, 50);
		AddFrame(new Frame("bottom"), false, false, true, 30);
		AddFrame(new Frame("bottom"), false, false, true, 30);
		AddFrame(new Frame("bottom"), true, false, true, 50);
		AddFrame(new Frame("top"), false, true, false, 50);
		ValidateSettings(true);
	}

	private void ValidateSettings(bool forced)
	{
		bool needResize = forced;
		if (settings.frameMinSize.Changed)
		{
			settings.frameMinSize.MarkReaded();
			needResize = true;
		}
		if (needResize)
			OnResize(null);
	}

	public void DoResize()
	{
		OnResize(null);
	}

	private Nest _nest;

	private void AddFrame(Frame frame, bool hDivided, bool left, bool isPercents, int percents)
	{
		_nest = new Nest(frame, _nest);
		_nest.hDivided = hDivided;
		_nest.left = left;
		_nest.isPercents = isPercents;
		_nest.size = percents;
		_nest.Init(this);
		Controls.Add(frame);
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		UpdateNest(_nest);
		Size size = ClientSize;
		if (_nest != null)
		{
			if (size.Width < _nest.minSize.Width)
				size.Width = _nest.minSize.Width;
			if (size.Height < _nest.minSize.Height)
				size.Height = _nest.minSize.Height;
		}
		ResizeNest(_nest, 0, 0, size.Width, size.Height);
	}

	private void UpdateNest(Nest nest)
	{
		if (nest == null)
			return;
		Size minSize = settings.frameMinSize.Value;
		nest.selfMinSize = minSize;
		if (nest.child != null)
		{
			nest.child.parent = nest;
			UpdateNest(nest.child);
			nest.minSize = nest.hDivided ?
				new Size(minSize.Width + nest.child.minSize.Width, Math.Max(minSize.Height, nest.child.minSize.Height)) :
				new Size(Math.Max(minSize.Width, nest.child.minSize.Width), minSize.Height + nest.child.minSize.Height);
		}
		else
		{
			nest.minSize = settings.frameMinSize.Value;
		}
	}

	private void ResizeNest(Nest nest, int x, int y, int width, int height)
	{
		if (nest == null)
			return;
		nest.FullWidth = width;
		nest.FullHeight = height;
		if (nest.child != null)
		{
			if (nest.hDivided)
			{
				int size = nest.GetSize(width);
				if (size < nest.selfMinSize.Width)
					size = nest.selfMinSize.Width;
				else if (width - size < nest.child.minSize.Width)
					size = width - nest.child.minSize.Width;
				nest.SetFrameSize(new Size(size, height));
				if (nest.left)
				{
					nest.frame.Location = new Point(x, y);
					ResizeNest(nest.child, x + size, y, width - size, height);
				}
				else
				{
					nest.frame.Location = new Point(x + (width - size), y);
					ResizeNest(nest.child, x, y, width - size, height);
				}
			}
			else
			{
				int size = nest.GetSize(height);
				if (size < nest.selfMinSize.Height)
					size = nest.selfMinSize.Height;
				else if (height - size < nest.child.minSize.Height)
					size = height - nest.child.minSize.Height;
				nest.SetFrameSize(new Size(width, size));
				if (nest.left)
				{
					nest.frame.Location = new Point(x, y);
					ResizeNest(nest.child, x, y + size, width, height - size);
				}
				else
				{
					nest.frame.Location = new Point(x, y + (height - size));
					ResizeNest(nest.child, x, y, width, height - size);
				}
			}
		}
		else
		{
			nest.frame.Location = new Point(x, y);
			nest.SetFrameSize(new Size(width, height));
		}
	}
}
