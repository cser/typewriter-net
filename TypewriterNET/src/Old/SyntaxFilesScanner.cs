using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MulticaretEditor;
using MulticaretEditor.Highlighting;

namespace TypewriterNET
{
	public class SyntaxFilesScanner
	{
		private string[] dirs;

		public SyntaxFilesScanner(string[] dirs)
		{
			this.dirs = dirs;
		}

		public void Rescan()
		{
			string tempFile = Path.Combine(Path.GetTempPath(), "typewriter-syntax.bin");
			SValue temp = File.Exists(tempFile) ? SValue.Unserialize(File.ReadAllBytes(tempFile)) : SValue.None;
			Dictionary<string, bool> scanned = new Dictionary<string, bool>();
			List<string> files = new List<string>();
			foreach (string dir in dirs)
			{
				if (!Directory.Exists(dir))
					continue;
				foreach (string fileI in Directory.GetFiles(dir, "*.xml"))
				{
					if (!scanned.ContainsKey(fileI))
					{
						scanned[fileI] = true;
						files.Add(fileI);
					}
				}
			}
			scanned.Clear();

			SValue newTemp = SValue.NewHash();
			infos.Clear();
			syntaxFileByName.Clear();
			foreach (string fileI in files)
			{
				SValue tempI = temp[fileI];
				long newTicks = File.GetLastWriteTime(fileI).Ticks;
				long ticks = tempI["ticks"].Long;
				if (newTicks == ticks)
				{
					LanguageInfo info = new LanguageInfo();
					info.syntax = tempI["syntax"].String;
					info.patterns = ParsePatterns(tempI["patterns"].String);
					info.priority = tempI["priority"].Int;
					syntaxFileByName[info.syntax] = fileI;
					infos.Add(info);

					newTemp[fileI] = tempI;
				}
				else
				{
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.ProhibitDtd = false;
					using (XmlReader reader = XmlReader.Create(fileI, settings))
					{
						while (reader.Read())
						{
							if (reader.NodeType == XmlNodeType.Element && reader.Name == "language")
							{
								string syntax = "";
								string patterns = "";
								int priority = 0;
								for (int i = 0; i < reader.AttributeCount; i++)
								{
									reader.MoveToAttribute(i);
									if (reader.Name == "name")
									{
										syntax = reader.Value.ToLowerInvariant();
									}
									else if (reader.Name == "extensions")
									{
										patterns = reader.Value;
									}
									else if (reader.Name == "priority")
									{
										int.TryParse(reader.Value, out priority);
									}
								}
								if (!string.IsNullOrEmpty(syntax))
								{
									LanguageInfo info = new LanguageInfo();
									info.syntax = syntax;
									info.patterns = ParsePatterns(patterns);
									info.priority = priority;
									syntaxFileByName[info.syntax] = fileI;
									infos.Add(info);

									SValue newTempI = SValue.NewHash();
									newTempI["syntax"] = SValue.NewString(info.syntax);
									newTempI["patterns"] = SValue.NewString(patterns);
									newTempI["priority"] = SValue.NewInt(priority);
									newTempI["ticks"] = SValue.NewLong(newTicks);
									newTemp[fileI] = newTempI;
								}
								break;
							}
						}
					}
				}
			}
			File.WriteAllBytes(tempFile, SValue.Serialize(newTemp));
		}

		private Regex[] ParsePatterns(string text)
		{
			string[] splitted = text.Split(';');
			List<Regex> patterns = new List<Regex>();
			for (int i = 0; i < splitted.Length; i++)
			{
				string splittedI = splitted[i].Trim();
				if (!string.IsNullOrEmpty(splittedI))
					patterns.Add(HighlighterUtil.GetFilenamePatternRegex(splittedI));
			}
			return patterns.ToArray();
		}

		//----------------------------------------------------------------------
		// Data
		//----------------------------------------------------------------------

		public class LanguageInfo
		{
			public string syntax;
			public int priority;
			public Regex[] patterns;
		}
		
		private List<LanguageInfo> infos = new List<LanguageInfo>();
		private Dictionary<string, string> syntaxFileByName = new Dictionary<string, string>();
		
		public void Reset()
		{
			infos.Clear();
			syntaxFileByName.Clear();
		}
		
		public string GetSyntaxByFile(string file)
		{
			string syntax = null;
			int priority = int.MinValue;
			int count = infos.Count;
			for (int i = 0; i < count; i++)
			{
				LanguageInfo info = infos[i];
				if (priority < info.priority)
				{
					Regex[] patterns = info.patterns;
					for (int j = 0; j < patterns.Length; j++)
					{
						if (patterns[j].IsMatch(file))
						{
							syntax = info.syntax;
							priority = info.priority;
							break;
						}
					}
				}
			}
			return syntax;
		}
		
		public string GetSyntaxFileByName(string name)
		{
			string fileName;
			syntaxFileByName.TryGetValue(name, out fileName);
			return fileName;
		}
	}
}
