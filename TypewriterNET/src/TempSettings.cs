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
	private FileQualitiesStorage storage = new FileQualitiesStorage();
	private RecentlyStorage recently = new RecentlyStorage();
	private RecentlyStorage recentlyDirs = new RecentlyStorage();
	private const string Scheme = "scheme";
	
	public readonly Dictionary<string, SValue> settingsData = new Dictionary<string, SValue>();
	
	public int helpPosition;
	public int viHelpPosition;
	public string NullableCurrentDir;
	
	public List<string> GetRecentlyFiles()
	{
		return recently.GetFiles();
	}
	
	public List<string> GetRecentlyDirs()
	{
		return recentlyDirs.GetFiles();
	}

	public TempSettings(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	public void Load(string postfix, bool rememberOpenedFiles)
	{
		SValue state = SValue.None;
		string file = GetTempSettingsPath(postfix, AppPath.StartupDir);
		if (File.Exists(file))
			state = SValue.Unserialize(File.ReadAllBytes(file));

		NullableCurrentDir = state["currentDir"].String;
		int width = Math.Max(50, state["width"].GetInt(700));
		int height = Math.Max(30, state["height"].GetInt(480));
		int x = Math.Max(0, state["x"].Int);
		int y = Math.Max(0, state["y"].Int);
		mainForm.Size = new Size(width, height);
		mainForm.Location = new Point(x, y);
		mainForm.WindowState = state["maximized"].GetBool(false) ? FormWindowState.Maximized : FormWindowState.Normal;
		storage.Unserialize(state["storage"]);
		recently.Unserialize(state["recently"]);
		recentlyDirs.Unserialize(state["recentlyDirs"]);
		DecodeGlobalBookmarks(state["bm"]);
		if (rememberOpenedFiles)
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
		findInFilesTempFilter.Unserialize(state["findInFilesTempFilter"]);
		findInFilesTempCurrentFilter.value = state["findInFilesTempCurrentFilter"].String;
		moveHistory.Unserialize(state["moveHistory"]);
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
		helpPosition = state["helpPosition"].Int;
		viHelpPosition = state["viHelpPosition"].Int;
		UnserializeSettings(ref state);
	}

	public void MarkLoaded(Buffer buffer)
	{
		if (buffer.FullPath != null)
			recently.Add(buffer.FullPath);
	}
	
	public void AddDirectory(string directory)
	{
		if (!string.IsNullOrEmpty(directory))
		{
			recentlyDirs.Add(directory);
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
		if (buffer.Controller.bookmarks.Count > 0)
		{
			value.With("bm", SValue.NewBytes(EncodeBookmarks(buffer.Controller)));
		}
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
        byte[] bookmarks = value["bm"].Bytes;
        if (bookmarks != null)
        {
	        DecodeBookmarks(buffer.Controller, bookmarks);
	    }
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

	public void Save(string postfix, bool rememberOpenedFiles)
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
		if (rememberOpenedFiles)
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
		state["recently"] = recently.Serialize();
		state["recentlyDirs"] = recentlyDirs.Serialize();
		state["bm"] = EncodeGlobalBookmakrs();
		ValuesSerialize(state);
		state["commandHistory"] = commandHistory.Serialize();
		state["findHistory"] = findHistory.Serialize();
		state["findInFilesHistory"] = findInFilesHistory.Serialize();
		state["findInFilesTempFilter"] = findInFilesTempFilter.Serialize();
		state["findInFilesTempCurrentFilter"] = SValue.NewString(findInFilesTempCurrentFilter.value);
		state["moveHistory"] = moveHistory.Serialize();
		state["replacePatternHistory"] = replacePatternHistory.Serialize();
		state["replaceHistory"] = replaceHistory.Serialize();
		state["goToLineHistory"] = goToLineHistory.Serialize();
		state["findParams"] = findParams.Serialize();
		if (!string.IsNullOrEmpty(NullableCurrentDir))
			state["currentDir"] = SValue.NewString(NullableCurrentDir);
		state["showFileTree"] = SValue.NewBool(mainForm.FileTreeOpened);
		state["fileTreeExpanded"] = mainForm.FileTree.GetExpandedTemp();
		state["helpPosition"] = SValue.NewInt(helpPosition);
		state["viHelpPosition"] = SValue.NewInt(viHelpPosition);
		SerializeSettings(ref state);
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
	
	private StringList findInFilesTempFilter = new StringList();
	public StringList FindInFilesTempFilter { get { return findInFilesTempFilter; } }
	
	private StringValue findInFilesTempCurrentFilter = new StringValue();
	public StringValue FindInFilesTempCurrentFilter { get { return findInFilesTempCurrentFilter; } }
	
	private StringList moveHistory = new StringList();
	public StringList MoveHistory { get { return moveHistory; } }

	private StringList replacePatternHistory = new StringList();
	public StringList ReplacePatternHistory { get { return replacePatternHistory; } }

	private StringList replaceHistory = new StringList();
	public StringList ReplaceHistory { get { return replaceHistory; } }

	private StringList goToLineHistory = new StringList();
	public StringList GoToLineHistory { get { return goToLineHistory; } }

	private FindParams findParams = new FindParams();
	public FindParams FindParams { get { return findParams; } }
	
	private byte[] EncodeBookmarks(Controller controller)
	{
		int count = Math.Min(controller.bookmarks.Count, controller.bookmarkNames.Count);
		byte[] bytes = new byte[count * 5];
		for (int i = 0; i < count; ++i)
		{
			int position = controller.bookmarks[i];
			char c = controller.bookmarkNames[i];
			bytes[i * 5] = (byte)(position & 0xff);
			bytes[i * 5 + 1] = (byte)((position >> 8) & 0xff);
			bytes[i * 5 + 2] = (byte)((position >> 16) & 0xff);
			bytes[i * 5 + 3] = (byte)((position >> 24) & 0xff);
			bytes[i * 5 + 4] = (byte)c;
		}
		return bytes;
	}
	
	private void DecodeBookmarks(Controller controller, byte[] bytes)
	{
		controller.bookmarks.Clear();
		controller.bookmarkNames.Clear();
		int count = bytes.Length / 5;
		for (int i = 0; i < count; ++i)
		{
			int position = bytes[i * 5] |
				(bytes[i * 5 + 1] << 8) |
				(bytes[i * 5 + 2] << 16) |
				(bytes[i * 5 + 3] << 24);
			controller.bookmarks.Add(position);
			controller.bookmarkNames.Add((char)bytes[i * 5 + 4]);
		}
	}
	
	private void DecodeGlobalBookmarks(SValue data)
	{
		if (MulticaretTextBox.initMacrosExecutor != null)
		{
			IRList<SValue> list = data.List;
			if (list != null)
			{
				int count = ('Z' - 'A' + 1) * 2;
				for (int i = 0; i + 1 < list.Count && i < count; i += 2)
				{
					string path = list[i].String;
					int position = list[i + 1].Int;
					if (!string.IsNullOrEmpty(path))
					{
						MulticaretTextBox.initMacrosExecutor.SetBookmark((char)(i / 2 + 'A'), path, position);
					}
				}
			}
		}
	}
	
	private SValue EncodeGlobalBookmakrs()
	{
		SValue data = SValue.NewList();
		if (MulticaretTextBox.initMacrosExecutor != null)
		{
			for (char c = 'A'; c <= 'Z'; ++c)
			{
				string path;
				int position;
				MulticaretTextBox.initMacrosExecutor.GetBookmark(c, out path, out position);
				data.Add(SValue.NewString(path));
				data.Add(SValue.NewInt(position));
			}
		}
		return data;
	}
	
	private void UnserializeSettings(ref SValue state)
	{
		settingsData.Clear();
		Dictionary<string, SValue> dict = state["settings"].AsDictionary;
		if (dict != null)
		{
			foreach (KeyValuePair<string, SValue> pair in dict)
			{
				if (pair.Key == Scheme)
				{
					continue;
				}
				settingsData[pair.Key] = pair.Value;
			}
			string scheme = state[Scheme].String;
			settingsData[Scheme] = SValue.NewString(!string.IsNullOrEmpty(scheme) ? scheme : Settings.DefaultScheme);
		}
	}
	
	private void SerializeSettings(ref SValue state)
	{
		SValue hash = state.SetNewHash("settings");
		foreach (KeyValuePair<string, SValue> pair in settingsData)
		{
			if (pair.Key == Scheme)
			{
				continue;
			}
			hash[pair.Key] = pair.Value;
		}
		string scheme = settingsData.ContainsKey(Scheme) ? settingsData[Scheme].String : null;
		state[Scheme] = !string.IsNullOrEmpty(scheme) ? SValue.NewString(scheme) : SValue.None;
	}
}
