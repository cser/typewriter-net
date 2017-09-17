using System;
using System.IO;
using System.Text;
using MulticaretEditor;
using KlerksSoft;

public class Buffer
{
	public readonly SettingsMode settingsMode;
	
	public Buffer(string fullPath, string name, SettingsMode settingsMode)
	{
		this.settingsMode = settingsMode;
		controller = new Controller(new LineArray());
		controller.Lines.viFullPath = fullPath;
		this.fullPath = fullPath;
		this.name = name;
		controller.Lines.hook2 = !string.IsNullOrEmpty(fullPath) ? new PositionHook(controller) : null;
	}
	
	public BufferList owner;
	public bool softRemove;

	public Frame Frame { get { return owner != null ? owner.frame : null; } }

	private readonly Controller controller;
	public Controller Controller { get { return controller; } }

	public bool HasHistory { get { return controller.processor.CanUndo || controller.processor.CanRedo; } }
	public bool Changed { get { return controller.processor.Changed; } }
	public bool IsEmpty { get { return controller.Lines.IsEmpty; } }
	
	public string ShowingName
	{
		get
		{
			string name = Name;
			if (Changed)
			{
				name += "*";
			}
			else if (unsaved)
			{
				name += "\"";
			}
			return name;
		}
	}
	
	public bool unsaved;

	private string fullPath;
	public string FullPath { get { return fullPath; } }

	private string name;
	public string Name { get { return name; } }

	public string httpServer;
	public EncodingPair settedEncodingPair;
	public EncodingPair encodingPair = new EncodingPair(Encoding.UTF8, false);
	public bool showEncoding = true;
	public string customSyntax;
	
	public void MarkAsSaved()
	{
		controller.processor.MarkAsSaved();
		unsaved = false;
	}
	
	public void MarkAsFullyUnsaved()
	{
		controller.processor.MarkAsFullyUnsaved();
	}

	public void SetFile(string fullPath, string name)
	{
		controller.Lines.viFullPath = fullPath;
		this.fullPath = fullPath;
		this.name = name;
		if (controller.Lines.hook2 == null)
		{
			controller.Lines.hook2 = !string.IsNullOrEmpty(fullPath) ? new PositionHook(controller) : null;
		}
		else if (string.IsNullOrEmpty(fullPath))
		{
			controller.Lines.hook2 = null;
		}
	}

	public bool needSaveAs;
	public FileInfo fileInfo;
	public DateTime lastWriteTimeUtc;
	public BufferTag tags = BufferTag.None;
	public Getter<Buffer, bool> onRemove;
	public Setter<Buffer> onAdd;
	public Setter<Buffer> onSelected;
	public Setter<Buffer, UpdatePhase> onUpdateSettings;

	public KeyMap additionKeyMap;
	public KeyMap additionBeforeKeyMap;

	public static string StringOf(Buffer buffer)
	{
		return buffer.ShowingName;
	}

	public static string EncodeOf(Buffer buffer)
	{
		return buffer.showEncoding ? buffer.encodingPair.ToString() : null;
	}

	//--------------------------------------------------------------------------
	// Helped
	//--------------------------------------------------------------------------

	public void Write(string text)
	{
		Write(text, null);
	}

	public void Write(string text, Ds ds)
	{
		int index = controller.Lines.charsCount;
		controller.ClearMinorSelections();
		controller.PutCursor(controller.Lines.PlaceOf(controller.Lines.charsCount), false);
		controller.Lines.InsertText(index, text);
		if (ds != null)
			controller.SetStyleRange(new StyleRange(index, text.Length, ds.index));
		controller.NeedScrollToCaret();
	}

	public void WriteLine(string text)
	{
		WriteLine(text, null);
	}

	public void WriteLine(string text, Ds ds)
	{
		Write(text + "\n", ds);
	}

	public void InitText(string text)
	{
		controller.InitText(text);
	}

	public void InitBytes(byte[] bytes, EncodingPair defaultEncoding, out string error)
	{
		error = null;
		string text = "";
		encodingPair = defaultEncoding;
		if (bytes != null)
		{
			try
			{
				if (!settedEncodingPair.IsNull)
				{
					encodingPair = settedEncodingPair;
				}
				else
				{
					bool bom;
					Encoding encoding = TextFileEncodingDetector.DetectTextByteArrayEncoding(bytes, out bom);
					if (encoding != null)
						encodingPair = new EncodingPair(encoding, bom);
				}
				int bomLength = encodingPair.CorrectBomLength(bytes);
				if (encodingPair.bom && encodingPair.encoding == Encoding.UTF8 && bomLength == 0)
				{
					encodingPair = new EncodingPair(Encoding.UTF8, false);
					settedEncodingPair = encodingPair;
					if (error == null)
					{
						error = "Missing bom, loaded as without it";
					}
				}
				text = encodingPair.GetString(bytes, bomLength);
			}
			catch (Exception e)
			{
				error = e.Message;
			}
		}
		if (encodingPair.IsNull)
			encodingPair = new EncodingPair(Encoding.UTF8, false);
		Controller.InitText(text);
	}
}
