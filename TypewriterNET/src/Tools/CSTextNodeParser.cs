using System;
using System.Collections.Generic;
using System.Text;
using TinyJSON;
using MulticaretEditor;

public class CSTextNodeParser : TextNodeParser
{
	public CSTextNodeParser(string name) : base(name)
	{
	}
	
	public override Node Parse(LineArray lines)
	{
		CSTokenIterator iterator = new CSTokenIterator(lines);
		List<Node> nodes = new List<Node>();
		Node root = (Node)(new Dictionary<string, Node>());
		root["name"] = "FILE";
		root["line"] = -1;
		root["childs"] = nodes;
		ParseRoot(iterator, nodes);
		return nodes.Count == 1 ? nodes[0] : root;
	}
	
	private void ParseRoot(CSTokenIterator iterator, List<Node> nodes)
	{
		while (!iterator.isEnd)
		{
			if (iterator.current.text == "class")
			{
				nodes.Add(ParseClass(iterator));
			}
			if (iterator.current.text == "namespace")
			{
				nodes.Add(ParseNamespace(iterator));
			}
			if (iterator.current.text == "struct")
			{
				nodes.Add(ParseStruct(iterator));
			}
			if (iterator.current.text == "enum")
			{
				nodes.Add(ParseEnum(iterator));
			}
			iterator.MoveNext();
		}
	}
	
	private void ParseContent(CSTokenIterator iterator, List<Node> nodes)
	{
		if (iterator.current.c != '{')
		{
			return;
		}
		iterator.MoveNext();
		while (!iterator.isEnd)
		{
			if (iterator.current.c == '}')
			{
				iterator.MoveNext();
				return;
			}
			string modifiers = "";
			while (true)
			{
				if (iterator.current.text == "private")
				{
					iterator.MoveNext();
					modifiers += "-";
					continue;
				}
				if (iterator.current.text == "public")
				{
					iterator.MoveNext();
					modifiers += "+";
					continue;
				}
				if (iterator.current.text == "protected")
				{
					iterator.MoveNext();
					modifiers += "#";
					continue;
				}
				if (iterator.current.text == "internal")
				{
					iterator.MoveNext();
					modifiers += "~";
					continue;
				}
				if (iterator.current.text == "static")
				{
					iterator.MoveNext();
					modifiers = "|" + modifiers;
					continue;
				}
				if (iterator.current.text == "virtual")
				{
					iterator.MoveNext();
					continue;
				}
				if (iterator.current.text == "override")
				{
					iterator.MoveNext();
					continue;
				}
				break;
			}
			if (iterator.current.text == "class")
			{
				nodes.Add(ParseClass(iterator));
				continue;
			}
			if (iterator.current.text == "struct")
			{
				nodes.Add(ParseEnum(iterator));
				iterator.MoveNext();
				continue;
			}
			if (iterator.current.text == "enum")
			{
				nodes.Add(ParseEnum(iterator));
				iterator.MoveNext();
				continue;
			}
			Place place = iterator.current.place;
			string ident;
			string type;
			if (iterator.Next.c == '(')
			{
				ident = iterator.current.text;
				iterator.MoveNext();
				if (iterator.current.IsIdent)
				{
					iterator.MoveNext();
				}
				type = "";
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				iterator.builder.Length = 0;
				ParseParameters(iterator, iterator.builder);
				string parameters = iterator.builder.ToString();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + ident + parameters;
				nodes.Add(node);
				MoveBrackets(iterator);
				continue;
			}
			iterator.builder.Length = 0;
			ParseType(iterator, iterator.builder);
			type = iterator.builder.ToString();
			ident = iterator.current.text;
			if (iterator.current.IsIdent)
			{
				iterator.MoveNext();
			}
			if (iterator.current.c == '(')
			{
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				iterator.builder.Length = 0;
				ParseParameters(iterator, iterator.builder);
				string parameters = iterator.builder.ToString();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + type + " " + ident + parameters;
				nodes.Add(node);
				MoveBrackets(iterator);
				continue;
			}
			if (iterator.current.c == '{')
			{
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + type + " " + ident;
				nodes.Add(node);
				MoveBrackets(iterator);
				continue;
			}
			if (iterator.current.c == '[')
			{
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				iterator.builder.Length = 0;
				ParseQuadParameters(iterator, iterator.builder);
				string parameters = iterator.builder.ToString();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + type + " " + ident + parameters;
				nodes.Add(node);
				MoveBrackets(iterator);
				continue;
			}
			while (!iterator.isEnd)
			{
				if (iterator.current.c == ';')
				{
					iterator.MoveNext();
					break;
				}
				iterator.MoveNext();
			}
		}
	}
	
	private Node ParseClass(CSTokenIterator iterator)
	{
		iterator.MoveNext();
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "class " + iterator.current.text;
		node["line"] = iterator.current.place.iLine + 1;
		List<Node> nodes = new List<Node>();
		node["childs"] = nodes;
		if (iterator.current.IsIdent)
		{
			iterator.MoveNext();
			while (!iterator.isEnd)
			{
				if (iterator.current.c == '{')
				{
					break;
				}
				iterator.MoveNext();
			}
		}
		ParseContent(iterator, nodes);
		return node;
	}
	
	private Node ParseNamespace(CSTokenIterator iterator)
	{
		iterator.MoveNext();
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "namespace " + iterator.current.text;
		node["line"] = iterator.current.place.iLine + 1;
		node["childs"] = new List<Node>();
		iterator.MoveNext();
		MoveBrackets(iterator);
		return node;
	}
	
	private Node ParseStruct(CSTokenIterator iterator)
	{
		iterator.MoveNext();
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "struct " + iterator.current.text;
		node["line"] = iterator.current.place.iLine + 1;
		List<Node> nodes = new List<Node>();
		node["childs"] = nodes;
		iterator.MoveNext();
		ParseContent(iterator, nodes);
		return node;
	}
	
	private Node ParseEnum(CSTokenIterator iterator)
	{
		iterator.MoveNext();
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "enum " + iterator.current.text;
		node["line"] = iterator.current.place.iLine + 1;
		node["childs"] = new List<Node>();
		MoveBrackets(iterator);
		return node;
	}
	
	private void MoveBrackets(CSTokenIterator iterator)
	{
		if (iterator.current.c == '{')
		{
			int depth = 0;
			while (!iterator.isEnd)
			{
				char c = iterator.current.c;
				if (c == '{')
				{
					++depth;
				}
				else if (c == '}')
				{
					--depth;
					if (depth <= 0)
					{
						iterator.MoveNext();
						break;
					}
				}
				iterator.MoveNext();
			}
		}
	}
	
	private void ParseType(CSTokenIterator iterator, StringBuilder builder)
	{
		if (iterator.current.IsIdent)
		{
			builder.Append(iterator.current.text);
			iterator.MoveNext();
			if (iterator.current.c == '<')
			{
				int depth = 0;
				while (!iterator.isEnd)
				{
					if (iterator.current.c == '<')
					{
						iterator.builder.Append('<');
						++depth;
					}
					else if (iterator.current.c == '>')
					{
						iterator.builder.Append('>');
						--depth;
						if (depth <= 0)
						{
							iterator.MoveNext();
							break;
						}
					}
					else if (iterator.current.c == ',')
					{
						iterator.builder.Append(", ");
					}
					else if (iterator.current.text != null)
					{
						iterator.builder.Append(iterator.current.text);
					}
					else
					{
						iterator.builder.Append(iterator.current.c);
					}
					iterator.MoveNext();
				}
			}
			if (iterator.current.c == '[')
			{
				builder.Append('[');
				iterator.MoveNext();
				if (iterator.current.c == ']')
				{
					builder.Append(']');
					iterator.MoveNext();
				}
			}
		}
	}
	
	private void ParseParameters(CSTokenIterator iterator, StringBuilder builder)
	{
		if (iterator.current.c == '(')
		{
			builder.Append('(');
			iterator.MoveNext();
			bool needSpace = false;
			while (!iterator.isEnd)
			{
				if (iterator.current.c == ')')
				{
					iterator.builder.Append(')');
					iterator.MoveNext();
					break;
				}
				if (iterator.current.c == ',' || iterator.current.c == ']')
				{
					iterator.builder.Append(iterator.current.c);
					needSpace = true;
				}
				else if (iterator.current.c == '=')
				{
					iterator.builder.Append(" = ");
					needSpace = false;
				}
				else if (iterator.current.text != null)
				{
					if (needSpace)
					{
						iterator.builder.Append(' ');
					}
					iterator.builder.Append(iterator.current.text);
					needSpace = true;
				}
				else
				{
					iterator.builder.Append(iterator.current.c);
					needSpace = false;
				}
				iterator.MoveNext();
			}
		}
	}
	
	private void ParseQuadParameters(CSTokenIterator iterator, StringBuilder builder)
	{
		if (iterator.current.c == '[')
		{
			int depth = 0;
			bool needSpace = false;
			while (!iterator.isEnd)
			{
				if (iterator.current.c == ']')
				{
					--depth;
					if (depth <= 0)
					{
						iterator.builder.Append(']');
						iterator.MoveNext();
						break;
					}
					else
					{
						iterator.builder.Append(']');
					}
				}
				else if (iterator.current.c == '[')
				{
					++depth;
					iterator.builder.Append('[');
				}
				else if (iterator.current.c == ',')
				{
					iterator.builder.Append(iterator.current.c);
					needSpace = true;
				}
				else if (iterator.current.text != null)
				{
					if (needSpace)
					{
						iterator.builder.Append(' ');
					}
					iterator.builder.Append(iterator.current.text);
					needSpace = true;
				}
				else
				{
					iterator.builder.Append(iterator.current.c);
					needSpace = false;
				}
				iterator.MoveNext();
			}
		}
	}
}