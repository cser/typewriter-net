using System;
using MulticaretEditor;

namespace TypewriterNET
{
	public class ConsoleInfo
	{
		public ConsoleInfo(string name)
		{
			this.name = name;
		}
		
		private string name;
		public string Name { get { return name; } }
		
		protected Controller controller;
		public Controller Controller { get { return controller; } }
		
		public static string StringOf(ConsoleInfo info)
		{
			return info.Name;
		}
	}
}
