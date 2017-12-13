using System;
using System.Xml;
using MulticaretEditor;

public class ConcreteHighlighterSet : HighlighterSet
{
	private readonly MainForm mainForm;
	private readonly XmlLoader xmlLoader;
	private Log log;
	private SyntaxFilesScanner scanner;
	
	public ConcreteHighlighterSet(XmlLoader xmlLoader, Log log, MainForm mainForm)
	{
		this.xmlLoader = xmlLoader;
		this.log = log;
		this.mainForm = mainForm;
	}
	
	public void UpdateParameters(SyntaxFilesScanner scanner)
	{
		Reset();
		this.scanner = scanner;
	}
	
	override protected Raw NewRaw(string type)
	{
		string file = scanner.GetSyntaxFileByName(type);
		if (string.IsNullOrEmpty(file))
		{
			if (mainForm.Dialogs != null)
			{
				mainForm.Dialogs.ShowInfo("Syntax highlighting", "Missing syntax: " + type);
			}
			return null;
		}
		XmlDocument xml = xmlLoader.Load(file, false);		
		Raw raw = Raw.Parse(xml);
		Raw.PrefixContexts(raw, type);
		Raw.InlineIncludeRules(raw, this);
		return raw;
	}
}
