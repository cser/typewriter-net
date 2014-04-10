using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;
using MulticaretEditor;

public class Frame : Control
{
	private TabBar<string> tabBar;
	private MulticaretTextBox textBox;

	public Frame(string name)
	{
		Name = name;
		TabStop = false;
		SwitchList<string> list = new SwitchList<string>();
		tabBar = new TabBar<string>(list, TabBar<string>.DefaultStringOf);
		tabBar.Text = name;
		Controls.Add(tabBar);

		textBox = new MulticaretTextBox();
		textBox.Location = new Point(0, 20);
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		tabBar.MouseUp += OnTabBarMouseUp;
	}

	public string Title
	{
		get { return tabBar.Text; }
		set { tabBar.Text = value; }
	}

	private Nest nest;
	public Nest Nest { get { return nest; } }

	public void SetNest(Nest nest)
	{
		this.nest = nest;
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		tabBar.Size = new Size(Width, 20);
		textBox.Size = new Size(Width, Height - 20);
	}

	private int startY;
	private int startSizeY;
	private int startHeight;

	private void OnTabBarMouseDown(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindHeightTarget();
		if (target != null)
		{
			startY = Control.MousePosition.Y;
			startSizeY = target.size;
			startHeight = target.frame.Height;
			tabBar.MouseMove += OnTabBarMouseMove;
		}
	}

	private void OnTabBarMouseUp(object sender, MouseEventArgs e)
	{
		tabBar.MouseMove -= OnTabBarMouseMove;
	}

	private void OnTabBarMouseMove(object sender, MouseEventArgs e)
	{
		if (nest == null)
			return;
		Nest target = FindHeightTarget();
		if (target != null)
		{
			int k = target.left ? -1 : 1;
			target.size = startSizeY * (startHeight + k * (startY - Control.MousePosition.Y)) / startHeight;
			if (target.size < 1)
				target.size = 1;
			else if (target.isPercents && target.size > 99)
				target.size = 99;
			target.MainForm.DoResize();
		}
	}

	private Nest FindHeightTarget()
	{
		if (nest == null)
			return null;
		if (!nest.hDivided && !nest.left)
			return nest;
		for (Nest nestI = nest.parent; nestI != null; nestI = nestI.parent)
		{
			if (!nestI.hDivided && nestI.left)
				return nestI;
		}
		return null;
	}
}
