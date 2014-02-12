using System;
using System.Resources;
using System.Xml;
using System.Drawing;
using System.IO;

public class ResourceBuilder
{
	public static void Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("No file specified");
			return;
		}
		string file = args[0];
		string dir = Path.GetDirectoryName(file);
		if (!string.IsNullOrEmpty(dir))
			Directory.SetCurrentDirectory(dir);

		XmlDocument xml = new XmlDocument();
		try
		{
			xml.Load(file);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			return;
		}
		foreach (XmlNode nodeI in xml.ChildNodes)
		{
			XmlElement elementI = nodeI as XmlElement;
			if (elementI != null && elementI.Name == "resources")
			{
				BuildResources(elementI);
			}
		}
	}

	private static void BuildResources(XmlElement root)
	{
		string outFile = root.GetAttribute("out");
		if (string.IsNullOrEmpty(outFile))
		{
			Console.WriteLine("Out file is not specified");
			return;
		}
		ResourceWriter writer = new ResourceWriter(outFile);
		foreach (XmlNode nodeI in root.ChildNodes)
		{
			XmlElement elementI = nodeI as XmlElement;
			if (elementI != null)
			{
				if (elementI.Name == "icon")
				{
					string name = elementI.GetAttribute("name");
					if (string.IsNullOrEmpty(name))
					{
						Console.WriteLine("Missing name attribute");
						writer.Close();
						return;
					}
					string src = elementI.GetAttribute("src");
					if (string.IsNullOrEmpty(src))
					{
						Console.WriteLine("Missing src attribute");
						writer.Close();
						return;
					}
					writer.AddResource(name, new Icon(src));
				}
			}
		}
		writer.Close();
	}
}
