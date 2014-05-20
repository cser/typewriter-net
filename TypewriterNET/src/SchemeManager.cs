using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

public class SchemeManager
{
	private MainForm mainForm;
	private Settings settings;

	public SchemeManager(MainForm mainForm, Settings settings)
	{
		this.mainForm = mainForm;
		this.settings = settings;
	}

	private AppPath GetSchemePath(string schemeName)
	{
		return new AppPath(Path.Combine(AppPath.Schemes, schemeName + ".xml"));
	}
	
	private List<AppPath> GetSchemePaths()
	{
		List<AppPath> paths = new List<AppPath>();
		foreach (string schemeName in ParseSchemeName(settings.scheme.Value))
		{
			paths.Add(GetSchemePath(schemeName));
		}
		return paths;
	}

	private static List<string> ParseSchemeName(string schemeName)
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(schemeName))
		{
			int startIndex = 0;
			while (true)
			{
				startIndex = schemeName.IndexOf('-', startIndex);
				if (startIndex == -1)
				{
					list.Add(schemeName);
					break;
				}
				list.Add(schemeName.Substring(0, startIndex));
				startIndex++;
			}
		}
		return list;
	}
	    
	public void Reload()
	{
		Scheme scheme = new Scheme();
		List<XmlDocument> xmls = new List<XmlDocument>();
		foreach (AppPath schemePath in GetSchemePaths())
		{
			XmlDocument xml = mainForm.XmlLoader.Load(schemePath.GetExisted(), true);
			if (xml != null)
				xmls.Add(xml);
		}
		scheme.ParseXml(xmls);

		settings.ParsedScheme = scheme;
		settings.DispatchParsedSchemeChange();
	}
}
