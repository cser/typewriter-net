using System;
using System.Collections.Generic;
using System.Text;

namespace MulticaretEditor.Commands
{
	public class History
	{
		public event Setter ChangedChange;

		private int _nextTagIndex = 0;

		private SwitchList<CommandTag> _tags;
		public readonly IRList<CommandTag> tags;

		private CommandTag head;
		public CommandTag Head { get { return head; } }

		private int indexOffset = 0;
		public int UndosCount { get { return head.Prev.index + indexOffset; } }

		private int maxUndosCount = 10000;
		public int MaxUndosCount
		{
			get { return maxUndosCount; }
			set { maxUndosCount = value; }
		}

		private Node root;

		public History()
		{
			_tags = new SwitchList<CommandTag>();
			tags = _tags;

			Reset();

			savedNode = head.Prev;
		}

		public void Reset()
		{
			_tags.Clear();
			root = new Node(null, 0);
			head = new CommandTag(_nextTagIndex++);
			head.Prev = root;
			_tags.Add(head);
		}

		public void ExecuteInited(Command command)
		{
			Node node0 = head.Prev;
			while (node0.index + indexOffset >= maxUndosCount)
			{
				Node oldRoot = root;
				root = oldRoot.nexts[0];
				Dictionary<CommandTag, bool> tagsToRemove = new Dictionary<CommandTag, bool>();
				foreach (Node nodeI in oldRoot.nexts)
				{
					nodeI.prev = null;
					if (nodeI.main)
					{
						root = nodeI;
					}
					else
					{
						FindAllTagsOf(nodeI, tagsToRemove);
					}
				}
				oldRoot.nexts.Clear();
				root.command = null;
				indexOffset--;
				foreach (KeyValuePair<CommandTag, bool> pair in tagsToRemove)
				{
					_tags.Remove(pair.Key);
				}
			}
			Node node = new Node(command, node0.index + 1);
			if (node0.nexts.Count > 0)
			{
				CommandTag tag = new CommandTag(_nextTagIndex++);
				tag.Prev = node;
				_tags.Add(tag);
				head = tag;
			}
			node0.nexts.Add(node);
			node.prev = node0;
			head.Prev = node;
			node.main = true;
			node.command.Redo();

			SetChanged(savedNode != head.Prev);
		}

		public bool CanUndo { get { return head.Prev != root; } }
		public Command LastCommand { get { return head.Prev != root ? head.Prev.command : null; } }

		public bool Undo()
		{
			if (head.Prev != root)
			{
				Node node = head.Prev;
				head.Prev = node.prev;
				head.redos.Add(node);
				node.main = false;
				node.command.Undo();

				SetChanged(savedNode != head.Prev);
				return true;
			}
			return false;
		}

		public bool CanRedo { get { return head.redos.Count > 0; } }
		public Command NextCommand { get { return head.redos.Count > 0 ? head.redos[head.redos.Count - 1].command : null; } }

		public void Redo()
		{
			if (head.redos.Count > 0)
			{
				int index = head.redos.Count - 1;
				Node node = head.redos[index];
				head.redos.RemoveAt(index);
				head.Prev = node;
				node.main = true;
				node.command.Redo();

				SetChanged(savedNode != head.Prev);
			}
		}

		public void Checkout(CommandTag tag)
		{
			Node common = null;
			{
				Dictionary<Node, bool> visited = new Dictionary<Node, bool>();
				Node nodeI = head.Prev;
				Node nodeJ = tag.Prev;
				while (nodeI != null || nodeJ != null)
				{
					if (nodeI != null)
					{
						if (visited.ContainsKey(nodeI))
						{
							common = nodeI;
							break;
						}
						visited.Add(nodeI, true);
						nodeI = nodeI.prev;
					}
					if (nodeJ != null)
					{
						if (visited.ContainsKey(nodeJ))
						{
							common = nodeJ;
							break;
						}
						visited.Add(nodeJ, true);
						nodeJ = nodeJ.prev;
					}
				}
			}
			if (common != null)
			{
				for (Node nodeI = head.Prev; nodeI != common; nodeI = nodeI.prev)
				{
					nodeI.main = false;
					nodeI.command.Undo();
				}

				List<Node> redos = new List<Node>();
				for (Node nodeI = tag.Prev; nodeI != common; nodeI = nodeI.prev)
				{
					redos.Add(nodeI);
				}
				redos.Reverse();
				foreach (Node nodeI in redos)
				{
					nodeI.main = true;
					nodeI.command.Redo();
				}

				head = tag;
			}
		}

		private void FindAllTagsOf(Node root, Dictionary<CommandTag, bool> tags)
		{
			Stack<Node> stack = new Stack<Node>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				Node nodeI = stack.Pop();
				foreach (CommandTag tag in nodeI.tags)
				{
					tags.Add(tag, true);
				}
				foreach (Node child in nodeI.nexts)
				{
					stack.Push(child);
				}
			}
		}

		public string ToDebugString()
		{
			StringBuilder text = new StringBuilder();
			ToDebugString(text, 0, root);
			return text.ToString();
		}

		private void ToDebugString(StringBuilder text, int indent, Node node)
		{
			text.Append(new string(' ', indent * 2));
			text.Append(node.command);
			foreach (CommandTag tag in node.tags)
			{
				text.Append(" " + tag);
			}
			text.Append('\n');
			for (int i = 0; i < node.nexts.Count; i++)
			{
				ToDebugString(text, indent + 1, node.nexts[i]);
			}
		}

		public void TagsModeOn()
		{
			_tags.ModeOn();
		}

		public void TagsModeOff()
		{
			_tags.ModeOff();
		}

		public void TagsDown()
		{
			_tags.Down();
			Checkout(_tags.Selected);
		}

		private bool changed = false;
		public bool Changed { get { return changed; } }

		private Node savedNode;

		public void MarkAsSaved()
		{
			savedNode = head.Prev;
			SetChanged(false);
		}

		private void SetChanged(bool value)
		{
			if (changed != value)
			{
				changed = value;
				if (ChangedChange != null)
					ChangedChange();
			}
		}
	}
}
