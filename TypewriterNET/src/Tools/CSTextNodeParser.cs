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
	
	private StringBuilder builder = new StringBuilder();
	
	public override Node Parse(LineArray lines)
	{
		builder.Length = 0;
		ParserIterator iterator = lines.GetParserIterator(0);
		List<Node> nodes = new List<Node>();
		Node root = (Node)(new Dictionary<string, Node>());
		root["name"] = "FILE";
		root["line"] = -1;
		root["childs"] = nodes;
		ParseRoot(iterator, nodes);
		builder.Length = 0;
		return nodes.Count == 1 ? nodes[0] : root;
	}
	
	private void ParseRoot(ParserIterator iterator, List<Node> nodes)
	{
		while (!iterator.IsEnd)
		{
			char c = iterator.RightChar;
			if (iterator.IsRightWord("class"))
			{
				nodes.Add(ParseClass(iterator));
			}
			if (iterator.IsRightWord("namespace"))
			{
				nodes.Add(ParseNamespace(iterator));
			}
			if (iterator.IsRightWord("struct"))
			{
				nodes.Add(ParseStruct(iterator));
			}
			if (iterator.IsRightWord("enum"))
			{
				nodes.Add(ParseEnum(iterator));
			}
			iterator.MoveRight();
		}
	}
	
	private void ParseContent(ParserIterator iterator, List<Node> nodes)
	{
		iterator.MoveSpacesAndRN();
		while (!iterator.IsEnd)
		{
			char c = iterator.RightChar;
			if (c == '{')
			{
				break;
			}
		}
		if (iterator.RightChar != '{')
		{
			return;
		}
		iterator.MoveRight();
		iterator.MoveSpacesAndRN();
		int position = -1;
		while (!iterator.IsEnd && position == iterator.Position)
		{
			position = iterator.Position;
			char c = iterator.RightChar;
			if (c == '}')
			{
				iterator.MoveRight();
				return;
			}
			string modifiers = "";
			while (true)
			{
				if (iterator.IsRightWord("private"))
				{
					iterator.MoveRightOnLine(7);
					iterator.MoveSpacesAndRN();
					modifiers += "-";
					continue;
				}
				if (iterator.IsRightWord("public"))
				{
					iterator.MoveRightOnLine(6);
					iterator.MoveSpacesAndRN();
					modifiers += "+";
					continue;
				}
				if (iterator.IsRightWord("protected"))
				{
					iterator.MoveRightOnLine(9);
					iterator.MoveSpacesAndRN();
					modifiers += "#";
					continue;
				}
				if (iterator.IsRightWord("internal"))
				{
					iterator.MoveRightOnLine(8);
					iterator.MoveSpacesAndRN();
					modifiers += "~";
					continue;
				}
				if (iterator.IsRightWord("static"))
				{
					iterator.MoveRightOnLine(6);
					iterator.MoveSpacesAndRN();
					modifiers = "|" + modifiers;
					continue;
				}
				if (iterator.IsRightWord("virtual"))
				{
					iterator.MoveRightOnLine(7);
					iterator.MoveSpacesAndRN();
					continue;
				}
				if (iterator.IsRightWord("override"))
				{
					iterator.MoveRightOnLine(8);
					iterator.MoveSpacesAndRN();
					continue;
				}
				break;
			}
			if (iterator.IsRightWord("class"))
			{
				nodes.Add(ParseClass(iterator));
				iterator.MoveSpacesAndRN();
				continue;
			}
			if (iterator.IsRightWord("struct"))
			{
				nodes.Add(ParseEnum(iterator));
				iterator.MoveSpacesAndRN();
				continue;
			}
			if (iterator.IsRightWord("enum"))
			{
				nodes.Add(ParseEnum(iterator));
				iterator.MoveSpacesAndRN();
				continue;
			}
			builder.Length = 0;
			ParseType(iterator, builder);
			string type = builder.ToString();
			iterator.MoveSpacesAndRN();
			Place place = iterator.Place;
			builder.Length = 0;
			iterator.MoveIdent(builder);
			string ident = builder.ToString();
			iterator.MoveSpacesAndRN();
			if (iterator.RightChar == '(')
			{
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				builder.Length = 0;
				ParseParameters(iterator, builder);
				string parameters = builder.ToString();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + type + " " + ident + parameters;
				nodes.Add(node);
				MoveBrackets(iterator);
				iterator.MoveSpacesAndRN();
				continue;
			}
			if (iterator.RightChar == '{')
			{
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + type + " " + ident;
				nodes.Add(node);
				MoveBrackets(iterator);
				iterator.MoveSpacesAndRN();
				continue;
			}
			while (!iterator.IsEnd)
			{
				if (iterator.RightChar == ';')
				{
					iterator.MoveRight();
					break;
				}
				iterator.MoveRight();
			}
			iterator.MoveSpacesAndRN();
		}
	}
	
	private Node ParseClass(ParserIterator iterator)
	{
		iterator.MoveRightOnLine(5);
		iterator.MoveSpacesAndRN();
		builder.Length = 0;
		iterator.MoveIdent(builder);
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "class " + builder.ToString();
		node["line"] = iterator.Place.iLine + 1;
		List<Node> nodes = new List<Node>();
		node["childs"] = nodes;
		ParseContent(iterator, nodes);
		return node;
	}
	
	private Node ParseNamespace(ParserIterator iterator)
	{
		iterator.MoveRightOnLine(9);
		iterator.MoveSpacesAndRN();
		builder.Length = 0;
		iterator.MoveIdent(builder);
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "namespace " + builder.ToString();
		node["line"] = iterator.Place.iLine + 1;
		node["childs"] = new List<Node>();
		MoveBrackets(iterator);
		return node;
	}
	
	private Node ParseStruct(ParserIterator iterator)
	{
		iterator.MoveRightOnLine(6);
		iterator.MoveSpacesAndRN();
		builder.Length = 0;
		iterator.MoveIdent(builder);
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "struct " + builder.ToString();
		node["line"] = iterator.Place.iLine + 1;
		node["childs"] = new List<Node>();
		MoveBrackets(iterator);
		return node;
	}
	
	private Node ParseEnum(ParserIterator iterator)
	{
		iterator.MoveRightOnLine(4);
		iterator.MoveSpacesAndRN();
		builder.Length = 0;
		iterator.MoveIdent(builder);
		Node node = (Node)(new Dictionary<string, Node>());
		node["name"] = "enum " + builder.ToString();
		node["line"] = iterator.Place.iLine + 1;
		node["childs"] = new List<Node>();
		MoveBrackets(iterator);
		return node;
	}
	
	private void MoveBrackets(ParserIterator iterator)
	{
		iterator.MoveSpacesAndRN();
		if (iterator.RightChar != '{')
		{
			return;
		}
		int depth = 0;
		while (!iterator.IsEnd)
		{
			char c = iterator.RightChar;
			if (c == '{')
			{
				++depth;
			}
			else if (c == '}')
			{
				--depth;
				if (depth <= 0)
				{
					iterator.MoveRight();
					break;
				}
			}
			iterator.MoveRight();
		}
	}
	
	private void ParseType(ParserIterator iterator, StringBuilder builder)
	{
		while (!iterator.IsEnd)
		{
			char c = iterator.RightChar;
			if (char.IsWhiteSpace(c))
			{
				break;
			}
			builder.Append(c);
			iterator.MoveRight();
		}
	}
	
	private void ParseParameters(ParserIterator iterator, StringBuilder builder)
	{
		if (iterator.RightChar == '(')
		{
			while (!iterator.IsEnd)
			{
				char c = iterator.RightChar;
				if (c == ')')
				{
					builder.Append(')');
					iterator.MoveRight();
					break;
				}
				builder.Append(c);
				iterator.MoveRight();
			}
		}
	}
}