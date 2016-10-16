using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MulticaretEditor;

public class TempSettings
{
	public static string GetTempSettingsPath(string postfix, string startupPath)
	{
		return Path.Combine(
			Path.GetTempPath(),
			"typewriter-state" + (!string.IsNullOrEmpty(postfix) ? "-" + postfix : startupPath.GetHashCode() + "") +
			".bin"
		);
	}

	private MainForm mainForm;
	private Settings settings;
	private FileQualitiesStorage storage = new FileQualitiesStorage();

	public TempSettings(MainForm mainForm, Settings settings)
	{
		this.mainForm = mainForm;
		this.settings = settings;
	}

	public void Load(string postfix)
	{
		SValue state = SValue.None;
		string file = GetTempSettingsPath(postfix, AppPath.StartupDir);
		if (File.Exists(file))
			state = SValue.Unserialize(File.ReadAllBytes(file));

		if (settings.rememberCurrentDir.Value && !string.IsNullOrEmpty(state["currentDir"].String))
		{
			string error;
			mainForm.SetCurrentDirectory(state["currentDir"].String, out error);
		}
		int width = Math.Max(50, state["width"].GetInt(700));
		int height = Math.Max(30, state["height"].GetInt(480));
		int x = Math.Max(0, state["x"].Int);
		int y = Math.Max(0, state["y"].Int);
		mainForm.Size = new Size(width, height);
		mainForm.Location = new Point(x, y);
		mainForm.WindowState = state["maximized"].GetBool(false) ? FormWindowState.Maximized : FormWindowState.Normal;
		storage.Unserialize(state["storage"]);
		if (settings.rememberOpenedFiles.Value)
		{
			{
				foreach (SValue valueI in state["openedTabs"].List)
				{
					string fullPath = valueI["fullPath"].String;
					if (fullPath != "" && File.Exists(fullPath))
						mainForm.LoadFile(fullPath);
				}
				Buffer selectedTab = mainForm.MainNest.buffers.GetByFullPath(BufferTag.File, state["selectedTab"]["fullPath"].String);
				if (selectedTab != null)
					mainForm.MainNest.buffers.list.Selected = selectedTab;
			}
			{
				foreach (SValue valueI in state["openedTabs2"].List)
				{
					string fullPath = valueI["fullPath"].String;
					if (fullPath != "" && File.Exists(fullPath))
						mainForm.LoadFile(fullPath, null, mainForm.MainNest2);
				}
				if (mainForm.MainNest2 != null)
				{
					Buffer selectedTab = mainForm.MainNest.buffers.GetByFullPath(BufferTag.File, state["selectedTab2"]["fullPath"].String);
					if (selectedTab != null)
						mainForm.MainNest.buffers.list.Selected = selectedTab;
				}
			}
		}
		ValuesUnserialize(state);
		commandHistory.Unserialize(state["commandHistory"]);
		findHistory.Unserialize(state["findHistory"]);
		findInFilesHistory.Unserialize(state["findInFilesHistory"]);
		replacePatternHistory.Unserialize(state["replacePatternHistory"]);
		replaceHistory.Unserialize(state["replaceHistory"]);
		goToLineHistory.Unserialize(state["goToLineHistory"]);
		findParams.Unserialize(state["findParams"]);
		mainForm.FileTree.SetExpandedTemp(state["fileTreeExpanded"]);
		if (state["showFileTree"].Bool)
		{
			mainForm.OpenFileTree();
			if (mainForm.MainNest.Frame != null)
				mainForm.MainNest.Frame.Focus();
		}
	}

	public void StorageQualities(Buffer buffer)
	{
		SValue value = storage.Set(buffer.FullPath).With("cursor", SValue.NewInt(buffer.Controller.Lines.LastSelection.caret));
		if (!buffer.settedEncodingPair.IsNull)
			value.With("encoding", SValue.NewString(buffer.settedEncodingPair.ToString()));
		else
			value.With("encoding", SValue.None);
		if (!string.IsNullOrEmpty(buffer.customSyntax))
			value.With("syntax", SValue.NewString(buffer.customSyntax.ToString()));
		else
			value.With("syntax", SValue.None);
	}

	public void ResetQualitiesEncoding(Buffer buffer)
	{
		SValue value = storage.Get(buffer.FullPath);
		value["encoding"] = SValue.None;
	}

	public void ApplyQualities(Buffer buffer, int lineNumber)
	{
		int caret = storage.Get(buffer.FullPath)["cursor"].Int;
		if (lineNumber == 0)
		{
            buffer.Controller.PutCursor(buffer.Controller.SoftNormalizedPlaceOf(caret), false);
            buffer.Controller.NeedScrollToCaret();
        }
        else
        {
            Place place = new Place(0, lineNumber - 1);
            SValue value = storage.Get(buffer.FullPath);
            value["cursor"] = SValue.NewInt(buffer.Controller.Lines.IndexOf(place));
            buffer.Controller.PutCursor(place, false);
            buffer.Controller.NeedScrollToCaret();
        }
	}

    public void ApplyQualitiesBeforeLoading(Buffer buffer)
    {
        SValue value = storage.Get(buffer.FullPath);
        string rawEncoding = value["encoding"].String;
        if (!string.IsNullOrEmpty(rawEncoding))
        {
            string error;
            buffer.settedEncodingPair = EncodingPair.ParseEncoding(rawEncoding, out error);
        }
        buffer.customSyntax = value["syntax"].String;
    }
    
    public EncodingPair GetEncoding(string fullPath, EncodingPair defaultPair)
    {
    	SValue value = storage.Get(fullPath);
        string rawEncoding = value["encoding"].String;
        if (!string.IsNullOrEmpty(rawEncoding))
        {
            string error;
            EncodingPair pair = EncodingPair.ParseEncoding(rawEncoding, out error);
            if (string.IsNullOrEmpty(error))
            	return pair;
        }
        return defaultPair;
    }

	public void Save(string postfix)
	{
		SValue state = SValue.NewHash();
		if (mainForm.WindowState == FormWindowState.Maximized)
		{
			state["width"] = SValue.NewInt(mainForm.RestoreBounds.Width);
			state["height"] = SValue.NewInt(mainForm.RestoreBounds.Height);
			state["x"] = SValue.NewInt(mainForm.RestoreBounds.X);
			state["y"] = SValue.NewInt(mainForm.RestoreBounds.Y);
		}
		else
		{
			state["width"] = SValue.NewInt(mainForm.Width);
			state["height"] = SValue.NewInt(mainForm.Height);
			state["x"] = SValue.NewInt(mainForm.Location.X);
			state["y"] = SValue.NewInt(mainForm.Location.Y);
		}
		state["maximized"] = SValue.NewBool(mainForm.WindowState == FormWindowState.Maximized);
		if (settings.rememberOpenedFiles.Value)
		{
			{
				SValue openedTabs = state.SetNewList("openedTabs");
				foreach (Buffer buffer in mainForm.MainNest.buffers.list)
				{
					SValue valueI = SValue.NewHash().With("fullPath", SValue.NewString(buffer.FullPath));
					openedTabs.Add(valueI);
					if (buffer == mainForm.MainNest.buffers.list.Selected)
						state["selectedTab"] = valueI;
				}
			}
			if (mainForm.MainNest2 != null)
			{
				SValue openedTabs = state.SetNewList("openedTabs2");
				foreach (Buffer buffer in mainForm.MainNest2.buffers.list)
				{
					SValue valueI = SValue.NewHash().With("fullPath", SValue.NewString(buffer.FullPath));
					openedTabs.Add(valueI);
					if (buffer == mainForm.MainNest2.buffers.list.Selected)
						state["selectedTab2"] = valueI;
				}
			}
		}
		state["storage"] = storage.Serialize();
		ValuesSerialize(state);
		state["commandHistory"] = commandHistory.Serialize();
		state["findHistory"] = findHistory.Serialize();
		state["findInFilesHistory"] = findInFilesHistory.Serialize();
		state["replacePatternHistory"] = replacePatternHistory.Serialize();
		state["replaceHistory"] = replaceHistory.Serialize();
		state["goToLineHistory"] = goToLineHistory.Serialize();
		state["findParams"] = findParams.Serialize();
		if (settings.rememberCurrentDir.Value)
			state["currentDir"] = SValue.NewString(Directory.GetCurrentDirectory());
		state["showFileTree"] = SValue.NewBool(mainForm.FileTreeOpened);
		state["fileTreeExpanded"] = mainForm.FileTree.GetExpandedTemp();
		File.WriteAllBytes(GetTempSettingsPath(postfix, AppPath.StartupDir), SValue.Serialize(state));
	}

	private const int MaxSettingsInts = 20;
	private Dictionary<string, TempSettingsInt> settingsInts = new Dictionary<string, TempSettingsInt>();

	private void ValuesSerialize(SValue state)
	{
		List<TempSettingsInt> list = new List<TempSettingsInt>();
		foreach (KeyValuePair<string, TempSettingsInt> pair in settingsInts)
		{
			list.Add(pair.Value);
		}
		list.Sort(CompareSettingsInts);
		if (list.Count > MaxSettingsInts)
			list.RemoveRange(MaxSettingsInts, list.Count - MaxSettingsInts);
		SValue sList = SValue.NewList();
		foreach (TempSettingsInt settingsInt in list)
		{
			SValue hash = SValue.NewHash();
			hash["id"] = SValue.NewString(settingsInt.id);
			hash["priority"] = SValue.NewInt(settingsInt.priority);
			hash["value"] = SValue.NewInt(settingsInt.value);
			sList.Add(hash);
		}
		state["values"] = sList;
	}

	private static int CompareSettingsInts(TempSettingsInt value0, TempSettingsInt value1)
	{
		return value1.priority - value0.priority;
	}

	private void ValuesUnserialize(SValue state)
	{
		SValue sList = state["values"];
		foreach (SValue hash in sList.List)
		{
			string id = hash["id"].String;
			TempSettingsInt settingsInt;
			settingsInts.TryGetValue(id, out settingsInt);
			if (settingsInt == null)
			{
				settingsInt = new TempSettingsInt(id);
				settingsInts[id] = settingsInt;
			}
			settingsInt.priority = hash["priority"].Int;
			settingsInt.value = hash["value"].Int;
			settingsInts[id] = settingsInt;
		}
	}

	public TempSettingsInt GetInt(string id, int defaultValue)
	{
		TempSettingsInt settingsInt;
		settingsInts.TryGetValue(id, out settingsInt);
		if (settingsInt == null)
		{
			settingsInt = new TempSettingsInt(id);
			settingsInt.value = defaultValue;
			settingsInts[id] = settingsInt;
		}
		return settingsInt;
	}

	private StringList commandHistory = new StringList();
	public StringList CommandHistory { get { return commandHistory; } }

	private StringList findHistory = new StringList();
	public StringList FindHistory { get { return findHistory; } }

	private StringList findInFilesHistory = new StringList();
	public StringList FindInFilesHistory { get { return findInFilesHistory; } }

	private StringList replacePatternHistory = new StringList();
	public StringList ReplacePatternHistory { get { return replacePatternHistory; } }

	private StringList replaceHistory = new StringList();
	public StringList ReplaceHistory { get { return replaceHistory; } }

	private StringList goToLineHistory = new StringList();
	public StringList GoToLineHistory { get { return goToLineHistory; } }

	private FindParams findParams = new FindParams();
	public FindParams FindParams { get { return findParams; } }
}
