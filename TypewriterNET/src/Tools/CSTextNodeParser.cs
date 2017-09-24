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
	
	public struct Token
	{
		public bool isString;
		public string text;
		public char c;
		public Place place;
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
	
	private const int Normal = 0;
	private const int MultilineString = 1;
	private const int MultilineComment = 2;
	
	public static List<Token> ParseTokens(LineArray lines)
	{
		List<Token> tokens = new List<Token>();
		StringBuilder builder = new StringBuilder();
		int state = Normal;
		for (int iBlock = 0; iBlock < lines.blocksCount; ++iBlock)
		{
			LineBlock block = lines.blocks[iBlock];
			for (int iLine = 0; iLine < block.count; ++iLine)
			{
				Line line = block.array[iLine];
				for (int i = 0; i < line.charsCount;)
				{
					char c = line.chars[iBlock].c;
					if (state == Normal)
					{
						if (char.IsWhiteSpace(c))
						{
							++i;
							continue;
						}
						if (char.IsLetterOrDigit(c) || c == '_')
						{
							Token token = new Token();
							builder.Length = 0;
							builder.Append(c);
							for (++i; i < line.charsCount; ++i)
							{
								c = line.chars[iBlock].c;
								if (!char.IsLetterOrDigit(c) && c != '_')
								{
									break;
								}
								builder.Append(c);
							}
							token.text = builder.ToString();
							token.place = new Place(i, block.offset + iLine);
							tokens.Add(token);
							continue;
						}
						if (c == '/')
						{
							++i;
							if (i < line.charsCount)
							{
								c = line.chars[iBlock].c;
								if (c == '/')
								{
									i = line.charsCount;
								}
								else if (c == '*')
								{
									++i;
									state = MultilineComment;
								}
								else
								{
									Token token = new Token();
									token.c = '/';
									token.place = new Place(i, block.offset + iLine);
									tokens.Add(token);
								}
							}
						}
						else
						{
							if (char.IsPunctuation(c))
							{
								Token token = new Token();
								token.c = c;
								token.place = new Place(i, block.offset + iLine);
								tokens.Add(token);
							}
							++i;
						}
					}
					else if (state == MultilineComment)
					{
						if (c == '*' && i + 1 < line.charsCount && line.chars[i + 1].c == '/')
						{
							i += 2;
							state = Normal;
						}
					}
					else if (state == MultilineString)
					{
						// TODO
					}
				}
			}
		}
		return tokens;
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
		while (!iterator.IsEnd)
		{
			if (position == iterator.Position)
			{
				throw new System.Exception("NOT MOVED");// TODO remove
				break;
			}
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
			Place place = iterator.Place;
			string ident;
			string type;
			if (iterator.FirstUnemptyAfterIdent() == '(')
			{
				builder.Length = 0;
				iterator.MoveIdent(builder);
				ident = builder.ToString();
				iterator.MoveSpacesAndRN();
				type = "";
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				builder.Length = 0;
				ParseParameters(iterator, builder);
				string parameters = builder.ToString();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + ident + parameters;
				nodes.Add(node);
				MoveBrackets(iterator);
				iterator.MoveSpacesAndRN();
				continue;
			}
			builder.Length = 0;
			ParseType(iterator, builder);
			type = builder.ToString();
			iterator.MoveSpacesAndRN();
			builder.Length = 0;
			iterator.MoveIdent(builder);
			ident = builder.ToString();
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
			if (iterator.RightChar == '[')
			{
				Node node = (Node)(new Dictionary<string, Node>());
				node["line"] = place.iLine + 1;
				node["childs"] = new List<Node>();
				builder.Length = 0;
				ParseQuadParameters(iterator, builder);
				string parameters = builder.ToString();
				node["name"] = (modifiers.Length > 0 ? modifiers + " " : "~ ") + type + " " + ident + parameters;
				nodes.Add(node);
				MoveBrackets(iterator);
				iterator.MoveSpacesAndRN();
				continue;
			}
			iterator.MoveSpacesAndRN();
			MoveComment(iterator);
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
			MoveComment(iterator);
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
	
	private void ParseQuadParameters(ParserIterator iterator, StringBuilder builder)
	{
		if (iterator.RightChar == '[')
		{
			while (!iterator.IsEnd)
			{
				char c = iterator.RightChar;
				if (c == ']')
				{
					builder.Append(']');
					iterator.MoveRight();
					break;
				}
				builder.Append(c);
				iterator.MoveRight();
			}
		}
	}
	
	private void MoveComment(ParserIterator iterator)
	{
		char c = iterator.RightChar;
		if (c == '/')
		{
			if (iterator.IsRightOnLine("//"))
			{
				while (!iterator.IsEnd)
				{
					c = iterator.RightChar;
					if (c == '\r')
					{
						iterator.MoveRight();
						if (iterator.RightChar == '\n')
						{
							iterator.MoveRight();
						}
						break;
					}
					if (c == '\n')
					{
						iterator.MoveRight();
						break;
					}
					iterator.MoveRight();
				}
			}
			else if (iterator.IsRightOnLine("/*"))
			{
				while (!iterator.IsEnd)
				{
					if (iterator.IsRightWord("*/"))
					{
						iterator.MoveRightOnLine(2);
						break;
					}
					iterator.MoveRight();
				}
			}
		}
	}
}