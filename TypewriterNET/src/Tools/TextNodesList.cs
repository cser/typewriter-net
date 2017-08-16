using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using TinyJSON;

public class TextNodesList : Buffer
{
	private Buffer buffer;
	private MainForm mainForm;
	private List<string> lines;
	private List<Place> places;
	
	public TextNodesList(Buffer buffer, MainForm mainForm) : base(null, "Nodes list", SettingsMode.TabList)
	{
		this.buffer = buffer;
		this.mainForm = mainForm;
	}
	
	public void Build(Properties.CommandInfo commandInfo, Encoding encoding, out string error, out string shellError)
	{
		lines = new List<string>();
		places = new List<Place>();
		
		Process p = new Process();
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.StandardOutputEncoding = encoding;
		p.StartInfo.StandardErrorEncoding = encoding;
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.FileName = "cmd.exe";
		p.StartInfo.Arguments = "/C " + commandInfo.command;
		p.Start();
		p.StandardInput.Write(buffer.Controller.Lines.GetText());
		p.StandardInput.Close();
		string output = p.StandardOutput.ReadToEnd();
		string errors = p.StandardError.ReadToEnd();
		p.WaitForExit();
		
		error = null;
		shellError = null;
		Node node = null;
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
			catch (Exception e)
			{
				error = "Parsing error: " + e.Message;
			}
		}
		
		AddLine(buffer.Name, new Place(-1, -1));
		if (node != null)
		{
			AppendNode(lines, node, "");
		}
		Controller.InitText(string.Join("\n", lines.ToArray()));
		
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
	
	private void AddLine(string line, Place place)
	{
		lines.Add(line);
		places.Add(place);
	}
	
	private void AppendNode(List<string> lines, Node node, string indent)
	{
		if (node == null)
		{
			AddLine(indent + "- [NO NODE]", new Place(-1, -1));
			return;
		}
		Node name = node["name"];
		string nameText = name != null ? name.ToString().Trim() : "";
		Place place = new Place(-1, -1);
		if (node["line"] != null && node["line"].IsInt())
		{
			place.iLine = (int)node["line"];
		}
		if (node["col"] != null && node["col"].IsInt())
		{
			place.iChar = (int)node["col"];
		}
		AddLine(indent + "- " + (!string.IsNullOrEmpty(nameText) ? nameText : "[NO NAME]") +
			": (" + place.iLine + (place.iChar >= 0 ? ", " + place.iChar : "") + ")", place);
		Node childs = node["childs"];
		if (childs != null && childs.IsArray())
		{
			List<Node> nodes = (List<Node>)childs;
			if (nodes != null && nodes.Count > 0)
			{
				foreach (Node nodeI in nodes)
				{
					AppendNode(lines, nodeI, "    " + indent);
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