using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using TinyJSON;

public class TextNodesList : Buffer
{
	public static IRList<TextNodeParser> buildinParsers = new RWList<TextNodeParser>(new TextNodeParser[] {
		new CSTextNodeParser("buildin-cs")
	});
	
	private readonly Buffer buffer;
	private readonly MainForm mainForm;
	private LineArray lines;
	private List<Place> places;
	private int tabSize;
	
	public TextNodesList(Buffer buffer, MainForm mainForm) : base(null, "Nodes list", SettingsMode.TabList)
	{
		this.buffer = buffer;
		this.mainForm = mainForm;
	}
	
	public void Build(Properties.CommandInfo commandInfo, Encoding encoding, out string error, out string shellError)
	{
		TextNodeParser buildinParser = null;
		string command = commandInfo.command;
		int index = command.IndexOf(':');
		if (index != -1)
		{
			customSyntax = command.Substring(index + 1).Trim();
			command = command.Substring(0, index).Trim();
		}
		foreach (TextNodeParser parser in buildinParsers)
		{
			if (parser.name == command)
			{
				buildinParser = parser;
				break;
			}
		}
		
		error = null;
		shellError = null;
		Node node = null;
		if (buildinParser != null)
		{
			node = buildinParser.Parse(buffer.Controller.Lines);
		}
		else
		{
			Process p = new Process();
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.StandardOutputEncoding = encoding;
			p.StartInfo.StandardErrorEncoding = encoding;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = "cmd.exe";
			p.StartInfo.Arguments = "/C " + command;
			p.Start();
			p.StandardInput.Write(buffer.Controller.Lines);
			p.StandardInput.Close();
			string output = p.StandardOutput.ReadToEnd();
			string errors = p.StandardError.ReadToEnd();
			p.WaitForExit();
			
			if (!string.IsNullOrEmpty(errors))
			{
				shellError = errors;
			}
			else
			{
				try
				{
					node = new Parser().Load(output);
				}
				catch (System.Exception e)
				{
					error = "Parsing error: " + e.Message +
						"\nSee \"" + mainForm.Settings.getTextNodes.name + "\" for more info";
				}
			}
		}
		if (node == null)
		{
			if (error == null)
			{
				error = "Empty output\nSee \"" + mainForm.Settings.getTextNodes.name + "\" for more info";
			}
			return;
		}
		tabSize = mainForm.Settings.tabSize.GetValue(null);
		lines = Controller.Lines;
		lines.ClearAllUnsafely();
		places = new List<Place>();
		AddLine(buffer.Name, new Place(-1, -1), true);
		AppendNode(node, "");
		if (lines.LinesCount == 0)
		{
			lines.AddLineUnsafely(new Line(32));
		}
		else
		{
			lines.CutLastLineBreakUnsafely();
		}
		Place target = buffer.Controller.Lines.SoftNormalizedPlaceOf(buffer.Controller.LastSelection.caret);
		for (int i = places.Count; i-- > 0;)
		{
			if (places[i].iLine <= target.iLine)
			{
				Place place = new Place(0, i);
				if (place.iLine < 0)
				{
					place.iLine = 0;
				}
				else if (place.iLine >= buffer.Controller.Lines.LinesCount)
				{
					place.iLine = buffer.Controller.Lines.LinesCount;
				}
				Controller.PutCursor(place, false);
				break;
			}
		}
		
		showEncoding = false;
		Controller.isReadonly = true;
		additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Close nodes list", DoCloseBuffer, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Jump to node", DoJumpTo, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		}
	}
	
	private void AddText(Line line, string text, Ds ds)
	{
		short style = ds.index;
		for (int i = 0; i < text.Length; i++)
		{
			line.Chars_Add(new Char(text[i], style));
		}
	}
	
	private void AddLine(string text, Place place, bool title)
	{
		Line line = new Line(text.Length + 1);
		line.tabSize = tabSize;
		AddText(line, text, title ? Ds.Comment : Ds.Normal);
		if (!title)
		{
			AddText(line, " (", Ds.Operator);
			AddText(line, (place.iLine + 1) + "", Ds.DecVal);
			if (place.iChar >= 0)
			{
				AddText(line, ", ", Ds.Operator);
				AddText(line, (place.iChar + 1) + "", Ds.DecVal);
			}
			AddText(line, ")", Ds.Operator);
		}
		line.Chars_Add(new Char('\n', 0));
		lines.AddLineUnsafely(line);
		places.Add(place);
	}
	
	private void AppendNode(Node node, string indent)
	{
		if (node == null)
		{
			AddLine(indent + "[NO NODE]", new Place(-1, -1), false);
			return;
		}
		Node name = node["name"];
		string nameText = name != null ? name.ToString().Trim() : "";
		Place place = new Place(-1, -1);
		if (node["line"] != null && node["line"].IsInt())
		{
			place.iLine = (int)node["line"] - 1;
		}
		if (node["col"] != null && node["col"].IsInt())
		{
			place.iChar = (int)node["col"] - 1;
		}
		AddLine(indent + (!string.IsNullOrEmpty(nameText) ? nameText : "[NO NAME]"), place, false);
		Node childs = node["childs"];
		if (childs != null && childs.IsArray())
		{
			List<Node> nodes = (List<Node>)childs;
			if (nodes != null && nodes.Count > 0)
			{
				foreach (Node nodeI in nodes)
				{
					AppendNode(nodeI, "\t" + indent);
				}
			}
		}
	}
	
	private bool DoCloseBuffer(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoJumpTo(Controller controller)
	{
		Place place = controller.Lines.SoftNormalizedPlaceOf(controller.LastSelection.caret);
		if (place.iLine >= 0 && place.iLine < places.Count)
		{
			Place target = places[place.iLine];
			if (target.iLine < 0)
			{
				return false;
			}
			if (buffer.FullPath != null)
			{
				buffer.Controller.ViAddHistoryPosition(false);
			}
			Close();
			if (target.iLine >= buffer.Controller.Lines.LinesCount)
			{
				target.iLine = buffer.Controller.Lines.LinesCount;
			}
			if (target.iChar < 0)
			{
				Line line = buffer.Controller.Lines[target.iLine];
				target.iChar = line.GetFirstSpaces();
			}
			buffer.Controller.PutCursor(target, false);
			if (buffer.Frame != null)
			{
				buffer.Frame.TextBox.MoveToCaret();
				if (buffer.FullPath != null)
				{
					buffer.Controller.ViAddHistoryPosition(true);
					return true;
				}
			}
		}
		return false;
	}
	
	public void Close()
	{
		Frame frame = Frame;
		if (frame != null)
		{
			if (frame.ContainsBuffer(buffer))
			{
				frame.SelectedBuffer = buffer;
			}
			frame.RemoveBuffer(this);
		}
	}
	
	public void CloseSilent()
	{
		if (Frame != null)
		{
			Frame.RemoveBuffer(this);
		}
	}
}