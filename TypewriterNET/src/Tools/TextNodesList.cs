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
	
	public TextNodesList(Buffer buffer) : base(null, "Nodes list", SettingsMode.TabList)
	{
		this.buffer = buffer;
	}
	
	public void Build(Properties.CommandInfo commandInfo, Encoding encoding, out string error, out string shellError)
	{
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
		
		StringBuilder builder = new StringBuilder();
		builder.Append(buffer.Name);
		builder.AppendLine();
		if (node != null)
		{
			AppendNode(builder, node, "");
		}
		if (builder.Length > 0 && builder[builder.Length - 1] == '\n')
		{
			--builder.Length;
		}
		Controller.InitText(builder.ToString());
		
		showEncoding = false;
		Controller.isReadonly = true;
		additionKeyMap = new KeyMap();
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Close nodes list", DoCloseBuffer, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Escape, null, action));
			additionKeyMap.AddItem(new KeyItem(Keys.Control | Keys.OemOpenBrackets, null, action));
		}
		{
			KeyAction action = new KeyAction("&View\\Nodes list\\Jump to node", DoOpenTab, null, false);
			additionKeyMap.AddItem(new KeyItem(Keys.Enter, null, action));
		}
	}
	
	private void AppendNode(StringBuilder builder, Node node, string indent)
	{
		builder.Append(indent + "-");
		if (node == null)
		{
			builder.Append("[NO NODE]");
			return;
		}
		Node name = node["name"];
		string nameText = name != null ? name.ToString().Trim() : "";
		builder.Append(string.IsNullOrEmpty(nameText) ? nameText : "[NO NAME]");
		Node childs = node["childs"];
		if (childs != null && childs.IsArray())
		{
			List<Node> nodes = (List<Node>)childs;
			if (nodes != null)
			{
				foreach (Node nodeI in nodes)
				{
					AppendNode(builder, nodeI, "  " + indent);
				}
			}
		}
		builder.Append("\n");
	}
	
	private bool DoCloseBuffer(Controller controller)
	{
		Close();
		return true;
	}
	
	private bool DoOpenTab(Controller controller)
	{
		return true;
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