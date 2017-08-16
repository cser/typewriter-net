using System.Collections.Generic;
using TinyJSON;
using MulticaretEditor;

public class CSTextNodeParser : TextNodeParser
{
	public CSTextNodeParser(string name) : base(name)
	{
	}
	
	public override Node Parse(LineArray lines)
	{
		LineIterator iterator = lines.GetLineRange(0, lines.LinesCount);
		Node data = null;
		while (iterator.MoveNext())
		{
			string line = iterator.current.Text.Trim();
			if (data == null)
			{
				if (line.Contains("class"))
				{
					data = (Node)(new Dictionary<string, Node>());
					data["name"] = line;
					data["line"] = iterator.Index + 1;
					data["childs"] = new List<Node>();
				}
			}
			else
			{
				if (line.Contains("private") || line.Contains("public"))
				{
					Node node = (Node)(new Dictionary<string, Node>());
					node["name"] = line;
					node["line"] = iterator.Index + 1;
					((List<Node>)data["childs"]).Add(node);
				}
			}
		}
		return data;
	}
}