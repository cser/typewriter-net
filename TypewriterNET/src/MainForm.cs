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

		{
			FindDialog dialog = new FindDialog("Find");
			_nest = new Nest(dialog, _nest);
			_nest.hDivided = false;
			_nest.left = false;
			_nest.isPercents = false;
			_nest.size = dialog.Height;
			_nest.Init(this);
			Controls.Add(dialog);
		}
		{
			ReplaceDialog dialog = new ReplaceDialog("Replace");
			_nest = new Nest(dialog, _nest);
			_nest.hDivided = false;
			_nest.left = false;
			_nest.isPercents = false;
			_nest.size = dialog.Height;
			_nest.Init(this);
			Controls.Add(dialog);
		}

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
		Size size = ClientSize;
		if (_nest != null)
		{
			_nest.Update();
			_nest.Resize(0, 0, size.Width, size.Height);
		}
	}
}
