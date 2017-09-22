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
		ParserIterator iterator = lines.GetParserIterator(0);
		List<Node> stack = new List<Node>();
		StringBuilder builder = new StringBuilder();
		int NORMAL = 0;
		int WAIT_OPEN = 1;
		int WAIT_COMMENT_CLOSE = 2;
		int WAIT_TEXT_CLOSE = 3;
		int WAIT_CHAR_CLOSE = 4;
		int state = NORMAL;
		Node root = (Node)(new Dictionary<string, Node>());
		root["name"] = "FILE";
		root["line"] = -1;
		root["childs"] = new List<Node>();
		stack.Add(root);
		while (!iterator.IsEnd)
		{
			char c = iterator.RightChar;
			if (state == NORMAL)
			{
				if (iterator.IsRightWord("class"))
				{
					iterator.MoveRightOnLine(5);
					for (; char.IsWhiteSpace(iterator.RightChar) && !iterator.IsEnd; iterator.MoveRight())
					{
					}
					if (iterator.IsEnd)
					{
						break;
					}
					builder.Length = 0;
					while (!iterator.IsEnd)
					{
						c = iterator.RightChar;
						if (char.IsLetterOrDigit(c) || c == '_')
						{
							builder.Append(iterator.RightChar);
						}
						else
						{
							break;
						}
						iterator.MoveRight();
					}
					string className = builder.ToString();
					builder.Length = 0;
					Node node = (Node)(new Dictionary<string, Node>());
					node["name"] = "class " + className;
					node["line"] = iterator.Place.iLine + 1;
					node["childs"] = new List<Node>();
					if (stack.Count > 0)
					{
						((List<Node>)stack[stack.Count - 1]["childs"]).Add(node);
					}
					stack.Add(node);
					state = WAIT_OPEN;
				}
				if (iterator.IsRightWord("namespace"))
				{
					iterator.MoveRightOnLine(9);
					for (; char.IsWhiteSpace(iterator.RightChar) && !iterator.IsEnd; iterator.MoveRight())
					{
					}
					if (iterator.IsEnd)
					{
						break;
					}
					builder.Length = 0;
					while (!iterator.IsEnd)
					{
						c = iterator.RightChar;
						if (char.IsLetterOrDigit(c) || c == '_')
						{
							builder.Append(iterator.RightChar);
						}
						else
						{
							break;
						}
						iterator.MoveRight();
					}
					string className = builder.ToString();
					builder.Length = 0;
					Node node = (Node)(new Dictionary<string, Node>());
					node["name"] = "namespace " + className;
					node["line"] = iterator.Place.iLine + 1;
					node["childs"] = new List<Node>();
					if (stack.Count > 0)
					{
						((List<Node>)stack[stack.Count - 1]["childs"]).Add(node);
					}
					stack.Add(node);
					state = WAIT_OPEN;
				}
				if (iterator.IsRightWord("struct"))
				{
					iterator.MoveRightOnLine(6);
					for (; char.IsWhiteSpace(iterator.RightChar) && !iterator.IsEnd; iterator.MoveRight())
					{
					}
					if (iterator.IsEnd)
					{
						break;
					}
					builder.Length = 0;
					while (!iterator.IsEnd)
					{
						c = iterator.RightChar;
						if (char.IsLetterOrDigit(c) || c == '_')
						{
							builder.Append(iterator.RightChar);
						}
						else
						{
							break;
						}
						iterator.MoveRight();
					}
					string className = builder.ToString();
					builder.Length = 0;
					Node node = (Node)(new Dictionary<string, Node>());
					node["name"] = "struct " + className;
					node["line"] = iterator.Place.iLine + 1;
					node["childs"] = new List<Node>();
					if (stack.Count > 0)
					{
						((List<Node>)stack[stack.Count - 1]["childs"]).Add(node);
					}
					stack.Add(node);
					state = WAIT_OPEN;
				}
				else if (iterator.IsRightOnLine("//"))
				{
					iterator.MoveRightOnLine(2);
					for (; !iterator.IsEnd; iterator.MoveRight())
					{
						c = iterator.RightChar;
						if (c == '\r' || c == '\n')
						{
							break;
						}
					}
				}
				else if (iterator.IsRightOnLine("/*"))
				{
					iterator.MoveRightOnLine(2);
					state = WAIT_COMMENT_CLOSE;
				}
				else if (c == '"')
				{
					state = WAIT_TEXT_CLOSE;
				}
				else if (c == '\'')
				{
					state = WAIT_CHAR_CLOSE;
				}
				else if (c == '}')
				{
					if (stack.Count > 0)
					{
						stack.RemoveAt(stack.Count - 1);
					}
				}
				else if (c == '{')
				{
					Node node = (Node)(new Dictionary<string, Node>());
					node["childs"] = new List<Node>();
					node["name"] = "";
					if (stack.Count > 0)
					{
						Node parent = stack[stack.Count - 1];
						if (((string)parent["name"]).StartsWith("class ") ||
							((string)parent["name"]).StartsWith("struct "))
						{
							string name = "AAAA";
							name = name.Replace("private ", "- ");
							name = name.Replace("protected ", "- ");
							name = name.Replace("public ", "+ ");
							name = name.Replace("internal ", "+ ");
							name = name.Replace("override ", "");
							name = name.Replace("virtual ", "");
							name = name.Replace("sealed ", "");
							name = name.Trim();
							if (name.Contains("static "))
							{
								name = name.Replace("static ", "");
								name = "|" + name;
							}
							node["name"] = name;
							((List<Node>)parent["childs"]).Add(node);
						}
					}
					stack.Add(node);
				}
			}
			else if (state == WAIT_OPEN)
			{
				for (; !iterator.IsEnd; iterator.MoveRight())
				{
					if (iterator.RightChar == '{')
					{
						state = NORMAL;
						break;
					}
				}
			}
			else if (state == WAIT_COMMENT_CLOSE)
			{
				if (iterator.IsRightOnLine("*/"))
				{
					iterator.MoveRightOnLine(2);
					state = NORMAL;
				}
			}
			else if (state == WAIT_TEXT_CLOSE)
			{
				if (c == '\\')
				{
					iterator.MoveRight();
				}
				else if (c == '"')
				{
					state = NORMAL;
				}
			}
			else if (state == WAIT_CHAR_CLOSE)
			{
				if (c == '\\')
				{
					iterator.MoveRight();
				}
				else if (c == '\'')
				{
					state = NORMAL;
				}
			}
			iterator.MoveRight();
		}
		return ((List<Node>)root["childs"]).Count == 1 ? ((List<Node>)root["childs"])[0] : root;
	}
}