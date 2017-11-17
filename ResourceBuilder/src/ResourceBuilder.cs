using System;
using System.Resources;
using System.Drawing;
using System.IO;

public class ResourceBuilder
{
	public static void Main(string[] args)
	{
		ResourceWriter writer = new ResourceWriter("TypewriterNET.resources");
		writer.AddResource("icon", new Icon("TypewriterNET\\TypewriterNET.ico"));
		writer.Close();
	}
}
