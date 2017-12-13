using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;

public static class Help
{
	public const string HomeUrl = "https://github.com/cser/typewriter-net";
	public const string HomeWikiUrl = "https://github.com/cser/typewriter-net/wiki";
	public const string BugreportUrl = "https://github.com/cser/typewriter-net/issues";
	public const string LastStableUrl  = "https://ci.appveyor.com/project/cser/typewriter-net/branch/master/artifacts";
	
	public static Buffer NewHelpBuffer(Settings settings, Commander commander)
	{
		List<StyleRange> ranges = new List<StyleRange>();
		StringBuilder builder = new StringBuilder();
		AddHead(builder, "Input mode");
		builder.AppendLine("# Command line options");
		builder.AppendLine();
		builder.AppendLine(GetExeHelp());
		builder.AppendLine();
		builder.AppendLine("# Actions");
		builder.AppendLine();
		builder.AppendLine("┌──────────────────────────────────────────────────────────────────────────────────────────┐");
		builder.AppendLine("│ IF YOU DON'T KNOW \"HOW TO DO IT\" PRESS `Ctrl+Shift+P` AT FIRST - IT'S SUPERIOR KNOWLEDGE │");
		builder.AppendLine("└──────────────────────────────────────────────────────────────────────────────────────────┘");
		builder.AppendLine("- All not-vi actions are represented in menu (use this `Ctrl+Shift+P` to search menu item)");
		builder.AppendLine("- Menu subitems are depended on frame with cursor");
		builder.AppendLine("- [] in menu item denotes complex shortcut, i.e. for `Ctrl+Tab`:");
		builder.AppendLine("    Ctrl↓, Tab↓↑, Ctrl↑ - switch back / forward between 2 tabs");
		builder.AppendLine("    Ctrl↓, Tab↓↑, Tab↓↑, Ctrl↑ - switch back / forward between 3 tabs");
		AddViMode(builder, settings);
		builder.AppendLine("- To leart more abot vi-mode use `Shift+F1` or :vh command");
		builder.AppendLine();
		builder.AppendLine(commander.GetHelpText());
		builder.AppendLine(settings.GetHelpText());
		builder.AppendLine("# Snippets");
		builder.AppendLine();
		builder.AppendLine("Executes if `Tab` pressed after key in input mode, next `Tab` put cursor to next entry.");
		builder.AppendLine("To see all accessible snippets press `Ctrl+Shift+Tab` in input mode");
		builder.AppendLine("Snippet file format:");
		builder.AppendLine();
		builder.AppendLine("	# <comment>");
		builder.AppendLine("	snippet <key> [action:]<description>");
		builder.AppendLine("		<body>");
		builder.AppendLine();
		builder.AppendLine("Body variables:");
		builder.AppendLine("	${1} - first cursor entry");
		builder.AppendLine("	${1:<default text>} - this text remains if cursor switched to next entry by `Tab`");
		builder.AppendLine("	${1:Regex#<c# regex>} - using (group) as default text");
		builder.AppendLine("	${0} - last cursor entry");
		builder.AppendLine("	$1 - first cursor input text");
		builder.AppendLine("	`g:snips_author` - snipsAuthor from config");
		builder.AppendLine("	`strftime(\"<format>\")` - onboard clock snippet execution time");
		builder.AppendLine("		%Y - full year, e.g. 2010");
		builder.AppendLine("		%m - month, e.g. 09");
		builder.AppendLine("		%d - day, e.g. 05");
		builder.AppendLine("		%H - hours, e.g. 14");
		builder.AppendLine("		%M - minutes, e.g. 48");
		builder.AppendLine("		%S - seconds, e.g. 34");
		builder.AppendLine("		%a - week day, e.g. Tue");
		builder.AppendLine("		%A - full week day, e.g. Tuesday");
		builder.AppendLine("		%b - month, e.g. Sep");
		builder.AppendLine("		%B - full month, e.g. September");
		builder.AppendLine("	%f%, %n%, … - command dialog variables, suffixes allowed");
		builder.AppendLine("If description starts with 'action:' then <body> will be parsed as command");
		builder.AppendLine();
		builder.AppendLine("# Syntax highlighting styles");
		builder.AppendLine();
		{
			int cols = 5;
			int rows = Math.Max(1, (Ds.all.Count + cols - 1) / cols);
			Ds[,] grid = new Ds[cols, rows];
			for (int i = 0; i < Ds.all.Count; ++i)
			{
				Ds ds = Ds.all[i];
				grid[i / rows, i % rows] = ds;
			}
			int[] maxSizes = new int[cols];
			for (int col = 0; col < cols; ++col)
			{
				for (int row = 0; row < rows; ++row)
				{
					Ds ds = grid[col, row];
					if (ds != null && maxSizes[col] < ds.name.Length)
					{
						maxSizes[col] = ds.name.Length;
					}
				}
			}
			for (int row = 0; row < rows; ++row)
			{
				for (int col = 0; col < cols; ++col)
				{
					Ds ds = grid[col, row];
					if (ds != null)
					{
						ranges.Add(new StyleRange(builder.Length, ds.name.Length, ds.index));
						builder.Append(ds.name);
						builder.Append(new string(' ', maxSizes[col] - ds.name.Length));
					}
					else
					{
						builder.Append(new string(' ', maxSizes[col]));
					}
					if (col != cols - 1)
					{
						builder.Append(" │ ");
					}
				}
				builder.AppendLine();
			}
		}
		builder.AppendLine();
		builder.AppendLine("Learn more about syntax.xml files in \"?\\Kate syntax highlighting help…\" menu item");
		builder.AppendLine();
		builder.AppendLine("# Using with Unity 3d");
		builder.AppendLine();
		builder.AppendLine("- add to config: xml: <item name=\"omnisharpSln\" value=\"Path\\To\\Unity-generated.sln\"/>");
		builder.AppendLine("    it's need for omnisharp commands working (see omnisharp- commands above)");
		builder.AppendLine("    if autocomplete stopped working properly try to run omnisharp-reloadsolution by `F9`");
		builder.AppendLine("- recommended to use local config (`Ctrl+F2` when current dir is project dir) and relative sln path");
		builder.AppendLine("- open Edit/Unity Preferences/External Tools in Unity 3d and then:");
		builder.AppendLine("    add TypewriterNET.exe to editor list");
		builder.AppendLine("    write in External Script Editor Args: -line=$(Line) \"$(File)\"");
		builder.AppendLine("    now you can jump to error by double click");
		builder.AppendLine("- if you want to navitate on Unity Console stack trace output:");
		builder.AppendLine("    copy it to clipboard");
		builder.AppendLine("    paste it in Typewriter.NET shell output by special shortcut `Ctrl+Shift+V`");
		builder.AppendLine("- when paste files in File tree by `Ctrl+Shift+V` after copy by `Ctrl+Shift+C`:");
		builder.AppendLine("    postfixed files (.meta by default) will not be inserted to prevent uids duplication");
		builder.AppendLine("    they will be insterted only after cut by `Ctrl+Shift+X`");
		builder.AppendLine("    if you want to insert this files change property pastePostfixedAfterCopy by Command dialog");
		builder.AppendLine("    or change line in config to: xml: <item name=\"pastePostfixedAfterCopy\" value=\"true\"/>");
		Buffer buffer = new Buffer(null, "Help.twh", SettingsMode.Help);
		buffer.tags = BufferTag.Other;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.Lines.ranges = ranges;
		return buffer;
	}
	
	public static Buffer NewViHelpBuffer(Settings settings)
	{
		List<StyleRange> ranges = new List<StyleRange>();
		StringBuilder builder = new StringBuilder();
		AddHead(builder, "Vi-mode");
		builder.AppendLine("# Common");
		builder.AppendLine();
		AddViMode(builder, settings);
		builder.AppendLine("- Parameters \"" + settings.viMapSource.name + "\" and \"" + settings.viMapResult.name + "\"");
		builder.AppendLine("  uses for mapping another keyboard layout to vi commands");
		builder.AppendLine("  For expample see base config by `Shift+F12`");
		builder.AppendLine("- There are no vi macroses implemented,");
		builder.AppendLine("  use simple `Ctrl+q`(begin), `Ctrl+q`(end), `Ctrl+Q`(execute)");
		builder.AppendLine("- Usualy you can use `\\` instead leader `,` (both works at the same time)");
		builder.AppendLine();
		builder.AppendLine("# Vi modes");
		builder.AppendLine();
		builder.AppendLine("- Input (equals to default editor mode)");
		builder.AppendLine("  Enter from Normal: `i`, `a`, `c`, `s`, `A`, `C`");
		builder.AppendLine("- Normal");
		builder.AppendLine("  Enter from Input, Visual, Lines visual: `Ctrl+[`");
		builder.AppendLine("- Visual");
		builder.AppendLine("  Enter from Normal, Lines visual: `v`");
		builder.AppendLine("- Lines visual");
		builder.AppendLine("  Enter from Normal, Lines visual: `V`");
		builder.AppendLine();
		builder.AppendLine("# Normal mode");
		builder.AppendLine();
		{
			TextTable table = new TextTable().SetMaxColWidth(40);
			table.Add(" Leader actions").NewRow();
			table.AddLine();
			table.Add(" ,b").Add("Show tab list, inside:\n  `Enter` - Select tab\n  `dd`- Close tab\n  `Ctrl+[` - Exit tab list").NewRow();
			table.Add(" ,g").Add("Show text nodes list, inside:\n  `Enter` - Jump to node\n  `Ctrl+[` - Exit tab list").NewRow();
			table.Add(" ,n").Add("Open/close file tree, inside:\n  `o` - open\n  `O` - open without switch\n  `dd` - delete file").NewRow();
			table.Add(" ,N").Add("Open/close file tree with current file").NewRow();
			table.Add(" ,s").Add("Save file").NewRow();
			table.Add(" ,r").Add("Reload file").NewRow();
			table.Add(" ,c").Add("Open/close shell command results").NewRow();
			table.Add(" ,f").Add("Open/close find results").NewRow();
			table.AddLine();
			table.Add(" Commands").NewRow();
			table.AddLine();
			table.Add(" :").Add("Open command dialog, inside:\n  `Ctrl+f` - normal mode inside dialog\n  `Ctrl+[` - close dialog\n  Full command list can be found in\n  input mode help - `F1` or `:help`").NewRow();
			table.AddLine();
			table.Add(" Moves").NewRow();
			table.AddLine();
			table.Add(" h").Add("Move left").NewRow();
			table.Add(" j").Add("Move down").NewRow();
			table.Add(" k").Add("Move up").NewRow();
			table.Add(" l").Add("Move right").NewRow();
			table.Add(" Ctrl+h").Add("Scroll left").NewRow();
			table.Add(" Ctrl+j").Add("Scroll down").NewRow();
			table.Add(" Ctrl+k").Add("Scroll up").NewRow();
			table.Add(" Ctrl+l").Add("Scroll right").NewRow();
			table.Add(" Ctrl+Shift+j").Add("Add new cursor bottom").NewRow();
			table.Add(" Ctrl+Shift+k").Add("Add new cursor up").NewRow();
			table.Add(" Ctrl+f").Add("Page down").NewRow();
			table.Add(" Ctrl+b").Add("Page up").NewRow();
			table.Add(" w").Add("Move next word start").NewRow();
			table.Add(" W").Add("Move next word start, with punctuation").NewRow();
			table.Add(" e").Add("Move word end").NewRow();
			table.Add(" E").Add("Move word end, with punctuation").NewRow();
			table.Add(" b").Add("Move word left").NewRow();
			table.Add(" B").Add("Move word left, ignore punctuation").NewRow();
			table.Add(" ^").Add("Move to line start").NewRow();
			table.Add(" $").Add("Move to line end").NewRow();
			table.Add(" 0").Add("Move to line start without indentation").NewRow();
			table.Add(" gg").Add("Move to document start").NewRow();
			table.Add(" G").Add("Move to document end").NewRow();
			table.Add(" <number>G").Add("Move to line number").NewRow();
			table.Add(" %").Add("Move to bracket under cursor pair").NewRow();
			table.Add(" Ctrl+]").Add("f12Command (navigate to)").NewRow();
			table.Add(" Ctrl+o").Add("Go to previous place").NewRow();
			table.Add(" Ctrl+i").Add("Return back").NewRow();
			table.Add(" g]").Add("shiftF12Command (all usages/definitions)").NewRow();
			table.AddLine();
			table.Add(" Actions").NewRow();
			table.AddLine();
			table.Add(" i").Add("Switch to input at left").NewRow();
			table.Add(" a").Add("Switch to input at right").NewRow();
			table.Add(" I").Add("Switch to input at line start").NewRow();
			table.Add(" A").Add("Switch to input at line end").NewRow();
			table.Add(" c<move>").Add("Remove and switch to input").NewRow();
			table.Add(" d<move>").Add("Remove").NewRow();
			table.Add(" x").Add("Remove char").NewRow();
			table.Add(" s").Add("Remove char and switch to input").NewRow();
			table.Add(" ~").Add("Switch char upper/lowercase").NewRow();
			table.Add(" y<move>").Add("Copy").NewRow();
			table.Add(" yy").Add("Copy line").NewRow();
			table.Add(" p").Add("Past").NewRow();
			table.Add(" P").Add("Past before").NewRow();
			table.Add(" .").Add("Repeat last action").NewRow();
			table.Add(" u").Add("Undo").NewRow();
			table.Add(" Ctrl+r").Add("Redo").NewRow();
			table.Add(" dd").Add("Remove line").NewRow();
			table.Add(" cc").Add("Remove line and switch to input").NewRow();
			table.Add(" o").Add("New line and switch to input").NewRow();
			table.Add(" O").Add("New line before and switch to input").NewRow();
			table.Add(" C").Add("Remove to line end and switch to input").NewRow();
			table.Add(" D").Add("Remove to line end").NewRow();
			table.Add(" <<").Add("Indent left").NewRow();
			table.Add(" >>").Add("Indent right").NewRow();
			table.Add(" /").Add("Open find dialog, inside:\n  `Ctrl+f` - normal mode inside dialog\n  `Ctrl+[` - close dialog").NewRow();
			table.Add(" ?").Add("Open find backward dialog").NewRow();
			table.Add(" Ctrl+/").Add("Open find dialog\n  with resetted options").NewRow();
			table.Add(" Ctrl+?").Add("Open find backward dialog\n  with resetted options").NewRow();
			table.Add(" *").Add("Put word or selection into\n  find register and find next").NewRow();
			table.Add(" #").Add("Put word or selection into\n  find register and find prev").NewRow();
			table.Add(" n").Add("Find next").NewRow();
			table.Add(" N").Add("Find previous").NewRow();
			table.Add(" <space><symbol><showed_symbols>").Add("Jump where you look:\n  <symbol> - symbol, what you look,\n  <showed_symbols> - symbols,  \nshowed after <symbol> entered").NewRow();
			table.Add(" ,<space><symbol><showed_symbols>").Add("Jump with new cursor").NewRow();
			table.Add(" Gv").Add("Recover last selection").NewRow();
			table.Add(" gK").Add("Remove prev selection (ersatz of Ctrl+K)").NewRow();
			table.AddLine();
			table.Add(" Text objects").NewRow();
			table.AddLine();
			table.Add(" <action>i<object>").Add("Apply action to text object").NewRow();
			table.Add(" w").Add("Word").NewRow();
			table.Add(" W").Add("Word with punctuation").NewRow();
			table.Add(" {, }, (, ), [, ]").Add("Text inside brackets").NewRow();
			table.Add(" \"").Add("Text inside quotes").NewRow();
			table.Add(" '").Add("Text inside single quotes").NewRow();
			table.AddLine();
			table.Add(" Regisers").NewRow();
			table.AddLine();
			table.Add(" \"<register><y|yy|p|P|<action>>").Add("Common form,\n  uses `\"0` if `\"<register>` unspecified").NewRow();
			table.Add(" \"0").Add("Default, don't need to be specified").NewRow();
			table.Add(" \"*, \"-").Add("System clipboard").NewRow();
			table.Add(" \"a - \"z").Add("Innder registers").NewRow();
			table.Add(" \"A - \"Z").Add("The same registers with accumulation").NewRow();
			table.Add(" \"/").Add("Find register (readonly)").NewRow();
			table.Add(" \":").Add("Last command (readonly)").NewRow();
			table.Add(" \".").Add("Last input text (readonly)").NewRow();
			table.Add(" \"%").Add("File path (readonly)").NewRow();
			table.AddLine();
			table.Add(" Bookmarks").NewRow();
			table.AddLine();
			table.Add(" m<a-z>").Add("Add bookmark inside current file").NewRow();
			table.Add(" m<A-Z>").Add("Add global bookmark").NewRow();
			table.Add(" `<a-zA-Z>").Add("Move to bookmark").NewRow();
			table.Add(" '<a-zA-Z>").Add("Move to bookmark line indented start").NewRow();
			builder.Append(table);
		}
		builder.AppendLine();
		builder.AppendLine("# Visual mode");
		builder.AppendLine();
		builder.AppendLine("(Only not equal to normal mode key sequences)");
		builder.AppendLine();
		{
			TextTable table = new TextTable().SetMaxColWidth(40);
			table.Add(" Actions").NewRow();
			table.AddLine();
			table.Add(" <").Add("Indent left").NewRow();
			table.Add(" >").Add("Indent right").NewRow();
			table.Add(" I").Add("Switch to input at selection start").NewRow();
			table.Add(" A").Add("Switch to input at selection end").NewRow();
			table.AddLine();
			table.Add(" Moves").NewRow();
			table.AddLine();
			table.Add(" o").Add("Switch anchor and caret of selection").NewRow();
			table.Add(" U").Add("Uppercase selection").NewRow();
			table.Add(" u").Add("Lowercase selection").NewRow();
			builder.Append(table);
		}
		builder.AppendLine();
		builder.AppendLine("Also don't foget for input mode shortcuts, that hasn't vi-mode analouges:");
		builder.AppendLine("  `Ctrl+E` - switch betwean frames");
		builder.AppendLine("  `Ctrl+D` - select next text");
		builder.AppendLine("  `Ctrl+Shift+O` - open previous files");
		builder.AppendLine("  `Ctrl+Shift+G` - open previous directoriesk");
		builder.AppendLine("  `Ctrl+P` - open file incremental search");
		builder.AppendLine("  e.t.c, use `Ctrl+Shift+P` or menu to learn more (context-depenent)");
		builder.AppendLine();
		builder.AppendLine("# Omnicomplition");
		builder.AppendLine();
		builder.AppendLine("  `Ctrl+n` - move selection down");
		builder.AppendLine("  `Ctrl+p` - move selection up");
		builder.AppendLine("  `Ctrl+j` - move selection down (works in incremental search dialogs)");
		builder.AppendLine("  `Ctrl+k` - move selection up (works in incremental search dialogs)");
		Buffer buffer = new Buffer(null, "Vi-help.twh", SettingsMode.Help);
		buffer.tags = BufferTag.Other;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.Lines.ranges = ranges;
		return buffer;
	}
	
	private static void AddHead(StringBuilder builder, string name)
	{
		builder.AppendLine("[[ " + name + " ]]");
		builder.AppendLine();
		builder.AppendLine(Application.ProductName + ", build " + Application.ProductVersion + ", official site:");
		builder.AppendLine(HomeUrl);
		builder.AppendLine("Last stable build page:");
		builder.AppendLine(LastStableUrl);
		builder.AppendLine();
	}
	
	private static void AddViMode(StringBuilder builder, Settings settings)
	{
		builder.AppendLine("- To enter vi-mode press `Ctrl+[` or alternative:");
		builder.AppendLine("    `Alt+[` (if \"" + settings.viAltOem.name + "\" is true in config)");
		builder.AppendLine("    `Esc` (if \"" + settings.viEsc.name + "\" is true in config)");
		builder.AppendLine("- For default Normal mode use \"" + settings.startWithViMode.name + "\" config parameter");
	}
	
	public static string GetExeHelp()
	{
	    return "<fileName>\n" +
            "-connect <fictiveFileName> <httpServer>\n" +
            "-temp <tempFilePostfix> - use different temp settings\n" +
            "-config <tempFilePostfix> - use different config\n" +
            "-help\n" + 
            "-line=<line>";
	}
}