using System;
using System.Collections.Generic;
using System.IO;

public class PasteFromClipboardAction
{
	public readonly PathSet NewFullPaths = new PathSet();
	public readonly List<string> Errors = new List<string>();
	
	public class FileMoveInfo
	{
		public string prevNorm;
		public string next;
		public bool hasPostfixed;
	}
	
	private readonly string renamePostfixed;
	private readonly bool pastePostfixedAfterCopy;
	
	public PasteFromClipboardAction(string renamePostfixed, bool pastePostfixedAfterCopy)
	{
		this.renamePostfixed = renamePostfixed;
		this.pastePostfixedAfterCopy = pastePostfixedAfterCopy;
	}
	
	public void Execute(string[] files, string targetDir, bool cutMode)
	{
		List<FileMoveInfo> moves = new List<FileMoveInfo>();
		{
			Dictionary<string, FileMoveInfo> prevNormPaths = new Dictionary<string, FileMoveInfo>();
			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
				FileMoveInfo move = new FileMoveInfo();
				move.prevNorm = PathSet.GetNorm(file);
				move.next = Path.Combine(targetDir, Path.GetFileName(file));
				moves.Add(move);
				prevNormPaths[move.prevNorm] = move;
			}
			if (!string.IsNullOrEmpty(renamePostfixed))
			{
				string lowerPostfix = renamePostfixed.ToLowerInvariant();
				for (int i = moves.Count; i-- > 0;)
				{
					FileMoveInfo move = moves[i];
					if (move.prevNorm.EndsWith(lowerPostfix))
					{
						string prev = move.prevNorm.Substring(0, move.prevNorm.Length - lowerPostfix.Length);
						if (!prev.EndsWith(lowerPostfix) && prevNormPaths.ContainsKey(prev))
						{
							prevNormPaths[PathSet.GetNorm(prev)].hasPostfixed = true;
							moves.RemoveAt(i);
						}
					}
				}
			}
			if (cutMode)
			{
				for (int i = moves.Count; i-- > 0;)
				{
					FileMoveInfo move = moves[i];
					if (move.prevNorm == PathSet.GetNorm(move.next))
					{
						moves.RemoveAt(i);
					}
				}
			}
		}
		bool hasErrors = false;
		foreach (FileMoveInfo info in moves)
		{
			bool isDir = Directory.Exists(info.prevNorm);
			if (!isDir && !File.Exists(info.prevNorm))
			{
				continue;
			}
			int index = 1;
			string next = info.next;
			string nextPostfixed = info.hasPostfixed ? info.next + renamePostfixed : null;
			{
				string nextExtension;
				string nextBase;
				if (Path.GetFileNameWithoutExtension(info.next) == "")
				{
					nextExtension = "";
					nextBase = info.next;
				}
				else
				{
					nextExtension = Path.GetExtension(info.next);
					nextBase = info.next.Substring(0, info.next.Length - nextExtension.Length);
				}
				while (Directory.Exists(next) || File.Exists(next) ||
					nextPostfixed != null && (Directory.Exists(nextPostfixed) || File.Exists(nextPostfixed)))
				{
					string suffix = index == 1 ? "-copy" : "-copy" + index;
					next = nextBase + suffix + nextExtension;
					if (nextPostfixed != null)
					{
						nextPostfixed = nextBase + suffix + nextExtension + renamePostfixed;
					}
					++index;
				}
			}
			try
			{
				if (isDir)
				{
					if (cutMode)
					{
						Directory.Move(info.prevNorm, next);
					}
					else
					{
						CopyDirectoryRecursive(info.prevNorm, next);
					}
				}
				else
				{
					if (cutMode)
					{
						File.Move(info.prevNorm, next);
					}
					else
					{
						File.Copy(info.prevNorm, next);
					}
				}
				if (nextPostfixed != null)
				{
					if (cutMode)
					{
						File.Move(info.prevNorm + renamePostfixed, nextPostfixed);
					}
					else if (pastePostfixedAfterCopy)
					{
						File.Copy(info.prevNorm + renamePostfixed, nextPostfixed);
					}
				}
			}
			catch (Exception e)
			{
				hasErrors = true;
				Errors.Add(e.Message + "\n" +
					"  " + info.prevNorm + " ->\n" +
					"  " + next + (nextPostfixed != null ? "(" + renamePostfixed + ")" : ""));
			}
			NewFullPaths.Add(next);
		}
	}
	
	private void CopyDirectoryRecursive(string prev, string targetFolder)
	{
		if (!Directory.Exists(targetFolder))
		{
			Directory.CreateDirectory(targetFolder);
		}
		foreach (string dir in Directory.GetDirectories(prev))
		{
			CopyDirectoryRecursive(dir, Path.Combine(targetFolder, Path.GetFileName(dir)));
		}
		foreach (string file in Directory.GetFiles(prev))
		{
			if (!pastePostfixedAfterCopy && !string.IsNullOrEmpty(renamePostfixed) &&
				file.ToLowerInvariant().EndsWith(renamePostfixed))
			{
				continue;
			}
			File.Copy(file, Path.Combine(targetFolder, Path.GetFileName(file)));
		}
	}
}