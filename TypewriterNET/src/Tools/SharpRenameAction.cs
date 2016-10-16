using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MulticaretEditor;
using TinyJSON;

public class SharpRenameAction
{
	private MainForm mainForm;
	private Buffer lastBuffer;
	private TempSettings tempSettings;
	private Place place;
	private string editorText;
	private string name;
	
	public void Execute(MainForm mainForm, TempSettings tempSettings, Buffer lastBuffer)
	{
		this.mainForm = mainForm;
		this.lastBuffer = lastBuffer;
		this.tempSettings = tempSettings;
		
		place = lastBuffer.Controller.Lines.PlaceOf(lastBuffer.Controller.LastSelection.Left);
		editorText = lastBuffer.Controller.Lines.GetText();
		name = lastBuffer.Controller.GetWord(place);
		if (string.IsNullOrEmpty(name) || name.Trim() == "" || !Regex.IsMatch(name, @"[\w_][\w\d_]*"))
		{
			mainForm.Dialogs.ShowInfo("Error", "Incorrect name: " + name);
			return;
		}
		
		mainForm.Dialogs.OpenInput("Rename identificator", name, DoInputNewName);
	}
	
	public class Change
	{
		public string FileName;
		public string Buffer;
	}
	
	private bool DoInputNewName(string newName)
	{
		if (newName == name)
			return true;
		mainForm.Dialogs.CloseInput();
		if (!Regex.IsMatch(newName, @"[\w_][\w\d_]*"))
		{
			mainForm.Dialogs.ShowInfo("Error", "Incorrect new name: " + newName);
			return true;
		}
		Node node = new SharpRequest(mainForm)
			.Add("FileName", lastBuffer.FullPath)
			.Add("Buffer", editorText)
			.Add("Line", (place.iLine + 1) + "")
			.Add("Column", (place.iChar + 1) + "")
			.Add("RenameTo", newName)
			.Send(mainForm.SharpManager.Url + "/rename", false);
		if (node == null || !node.IsTable())
		{
			mainForm.Dialogs.ShowInfo("OmniSharp",
				"Response parsing error: Table expected, but was:" + (node != null ? node.TypeOf() + "" : "null"));
			return true;
		}
		node = node["Changes"];
		if (node == null || !node.IsArray())
		{
			mainForm.Dialogs.ShowInfo("OmniSharp",
				"Response parsing error: \"Changes\" array expected, but was:" + (node != null ? node.TypeOf() + "" : "null"));
			return true;
		}
		List<Change> changes = new List<Change>();
		string error = null;
		for (int i = 0; i < node.Count; i++)
		{
			try
			{
				Change change = new Change();
				change.FileName = (string)node[i]["FileName"];
				change.Buffer = (string)node[i]["Buffer"];
				changes.Add(change);
			}
			catch (Exception e)
			{
				error = e.Message;
			}
		}
		if (error != null)
		{
			mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: " + error);
			return true;
		}
		List<string> errors = new List<string>();
		foreach (Change change in changes)
		{
			try
			{
				EncodingPair encodingPair = tempSettings.GetEncoding(change.FileName, mainForm.Settings.defaultEncoding.Value);
				if (!encodingPair.bom && encodingPair.encoding == Encoding.UTF8)
					File.WriteAllText(change.FileName, change.Buffer);
				else
					File.WriteAllText(change.FileName, change.Buffer, encodingPair.encoding);
			}
			catch (Exception e)
			{
				errors.Add(e.Message);
				return true;
			}
		}
		if (errors.Count > 0)
		{
			mainForm.Dialogs.ShowInfo("OmniSharp", "Errors:\n" + string.Join("\n", errors.ToArray()));
		}
		return true;
	}
}