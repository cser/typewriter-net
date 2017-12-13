using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MulticaretEditor;

public class RenameDialog : ADialog
{
	private readonly Getter<string, bool> doInput;
	private readonly List<bool> isDirectory;
	private readonly string text;
	
	private TabBar<string> tabBar;
	private MulticaretTextBox textBox;
	private bool startViMode;

	public RenameDialog(Getter<string, bool> doInput, string name, string text, List<bool> isDirectory)
	{
		this.doInput = doInput;
		this.isDirectory = isDirectory;
		Name = name;
		this.text = text;
	}

	override protected void DoCreate()
	{
		tabBar = new TabBar<string>(new SwitchList<string>(), TabBar<string>.DefaultStringOf);
		tabBar.Text = Name;
		tabBar.CloseClick += OnCloseClick;
		Controls.Add(tabBar);

		KeyMap frameKeyMap = new KeyMap();
		KeyItem escape = new KeyItem(Keys.Escape, null,
			new KeyAction("&View\\File tree\\Cancel renaming", DoCancel, null, false));
		frameKeyMap.AddItem(escape);
		frameKeyMap.AddItem(new KeyItem(Keys.Enter, null,
			new KeyAction("&View\\File tree\\Complete renaming", DoComplete, null, false)));
		
		KeyMap beforeKeyMap = new KeyMap();
		beforeKeyMap.AddItem(escape);

		textBox = new MulticaretTextBox();
		textBox.KeyMap.AddBefore(beforeKeyMap);
		textBox.KeyMap.AddAfter(KeyMap);
		textBox.KeyMap.AddAfter(frameKeyMap, 1);
		textBox.KeyMap.AddAfter(DoNothingKeyMap, -1);
		textBox.FocusedChange += OnTextBoxFocusedChange;
		Controls.Add(textBox);

		tabBar.MouseDown += OnTabBarMouseDown;
		InitResizing(tabBar, null);
	}

	override public bool Focused { get { return textBox.Focused; } }

	private void OnCloseClick()
	{
		if (startViMode)
		{
			textBox.SetViMode(true);
		}
		DispatchNeedClose();
	}

	override protected void DoDestroy()
	{
	}

	override public Size MinSize { get { return new Size(tabBar.Height * 3, tabBar.Height + textBox.CharHeight); } }

	override public void Focus()
	{
		textBox.Focus();
		
		startViMode = MulticaretTextBox.initMacrosExecutor != null &&
			MulticaretTextBox.initMacrosExecutor.viMode != ViMode.Insert;
		textBox.Text = text;
		textBox.Controller.ClearMinorSelections();
		for (int i = 0; i < textBox.Controller.Lines.LinesCount; i++)
		{
			Line line = textBox.Controller.Lines[i];
			if (i == 0)
			{
				textBox.Controller.PutCursor(new Place(0, i), false);
			}
			else
			{
				textBox.Controller.PutNewCursor(new Place(0, i));
			}
			int right = line.NormalCount;
			if (isDirectory != null && i < isDirectory.Count && !isDirectory[i])
			{
				for (int j = right; j-- > 1;)
				{
					if (line.chars[j].c == '.')
					{
						right = j;
						break;
					}
				}
			}
			if (!startViMode)
			{
				textBox.Controller.PutCursor(new Place(right, i), true);
			}
		}
		textBox.Invalidate();
		Nest.size = tabBar.Height + textBox.CharHeight * (
			!string.IsNullOrEmpty(text) && textBox.Controller != null ? textBox.GetScrollSizeY() : 1
		) + 4;
		SetNeedResize();
	}

	private void OnTabBarMouseDown(object sender, EventArgs e)
	{
		textBox.Focus();
	}

	private void OnTextBoxFocusedChange()
	{
		if (Destroyed)
			return;
		tabBar.Selected = textBox.Focused;
		if (textBox.Focused)
			Nest.MainForm.SetFocus(textBox, textBox.KeyMap, null);
	}

	override protected void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int tabBarHeight = tabBar.Height;
		tabBar.Size = new Size(Width, tabBarHeight);
		textBox.Location = new Point(0, tabBarHeight);
		textBox.Size = new Size(Width, Height - tabBarHeight + 1);
	}

	override protected void DoUpdateSettings(Settings settings, UpdatePhase phase)
	{
		if (phase == UpdatePhase.Raw)
		{
			settings.ApplySimpleParameters(textBox, null);
			textBox.SetViMap(settings.viMapSource.Value, settings.viMapResult.Value);
		}
		else if (phase == UpdatePhase.Parsed)
		{
			textBox.Scheme = settings.ParsedScheme;
			tabBar.Scheme = settings.ParsedScheme;
		}
	}

	private bool DoCancel(Controller controller)
	{
		if (startViMode)
		{
			textBox.SetViMode(true);
		}
		DispatchNeedClose();
		return true;
	}

	private bool DoComplete(Controller controller)
	{
		if (startViMode)
		{
			textBox.SetViMode(true);
		}
		return doInput(textBox.Text);
	}
}
