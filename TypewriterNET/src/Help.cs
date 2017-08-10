using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;

public static class Help
{
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
		builder.AppendLine("All actions are represented in menu.");
		builder.AppendLine("Menu subitems are depended on frame with cursor");
		builder.AppendLine("[] in menu item denotes complex shortcut,");
		builder.AppendLine("i.e. for [Ctrl+Tab]:");
		builder.AppendLine("\tCtrl↓, Tab↓↑, Ctrl↑ - switch back / forward between 2 tabs");
		builder.AppendLine("\tCtrl↓, Tab↓↑, Tab↓↑, Ctrl↑ - switch back / forward between 3 tabs");
		builder.AppendLine();
		builder.AppendLine("To enter vi-mode press `Ctrl+]`");
		builder.AppendLine("See more info in vi-mode help - `Shift+F1` or `\\H` / `,H` in vi-mode");
		builder.AppendLine();
		builder.AppendLine(commander.GetHelpText());
		builder.AppendLine(settings.GetHelpText());
		builder.AppendLine("# Syntax highlighting styles");
		builder.AppendLine();
		foreach (Ds ds in Ds.all)
		{
			ranges.Add(new StyleRange(builder.Length, ds.name.Length, ds.index));
			builder.AppendLine(ds.name);
		}
		Buffer buffer = new Buffer(null, "Help.twh", SettingsMode.Help);
		buffer.tags = BufferTag.Other;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.Lines.ranges = ranges;
		return buffer;
	}
	
	public static Buffer NewViHelpBuffer(Settings settings, Commander commander)
	{
		List<StyleRange> ranges = new List<StyleRange>();
		StringBuilder builder = new StringBuilder();
		AddHead(builder, "Vi-mode");
		builder.AppendLine("# Common");
		builder.AppendLine();
		builder.AppendLine("- Vi mode can be enabled by `Ctrl+[`");
		builder.AppendLine("- For default Normal mode use `" + settings.startWithViMode.name + "` config parameter");
		builder.AppendLine("- Parameters `" + settings.viMapSource.name + "` and `" + settings.viMapResult.name + "`");
		builder.AppendLine("  uses for mapping another keyboard layout to vi commands");
		builder.AppendLine("  For expample see base config by `Shift+F12`");
		builder.AppendLine("- There are no vi macroses implemented,");
		builder.AppendLine("  use simple `Ctrl+q`(begin), `Ctrl+q`(end), `Ctrl+Q`(execute)");
		builder.AppendLine();
		builder.AppendLine("# Vi modes");
		builder.AppendLine();
		builder.AppendLine("- [Input](equals to default editor mode)");
		builder.AppendLine("  Enter from [Normal]: `i`, `a`, `c`, `s`, `A`, `C`");
		builder.AppendLine("- [Normal]");
		builder.AppendLine("  Enter from [Input], [Visual], [LinesVisual]: `Ctrl+[`");
		builder.AppendLine("- [Visual]");
		builder.AppendLine("  Enter from [Normal], [LinesVisual]: `v`");
		builder.AppendLine("- [LinesVisual]");
		builder.AppendLine("  Enter from [Normal], [LinesVisual]: `V`");
		builder.AppendLine();
		builder.AppendLine("# Normal mode");
		builder.AppendLine();
		{
			TextTable table = new TextTable().SetMaxColWidth(40);
			table.Add(" Sequence").Add("Help");
			table.AddLine();
			table.Add(" \\h").Add("Show/Hide common help").NewRow();
			table.Add(" ,h").Add("Show/Hide common help").NewRow();
			table.Add(" \\H").Add("Show/Hide vi-mode help").NewRow();
			table.Add(" ,H").Add("Show/Hide vi-mode help");
			table.AddLine();
			table.Add(" Sequence").Add("Move");
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
			table.Add(" Ctrl+]").Add("OmniSharp navigate to").NewRow();
			table.Add(" Ctrl+o").Add("Go to previous place").NewRow();
			table.Add(" Ctrl+i").Add("Return back").NewRow();
			table.Add(" \\n").Add("Open/close file tree").NewRow();
			table.Add(" ,n").Add("Open/close file tree").NewRow();
			table.Add(" \\N").Add("Open/close file tree with current file").NewRow();
			table.Add(" ,N").Add("Open/close file tree with current file").NewRow();
			table.Add(" ,s").Add("Save file");
			table.AddLine();
			table.Add(" Sequence").Add("Action");
			table.AddLine();
			table.Add(" i").Add("Switch to Input at left").NewRow();
			table.Add(" a").Add("Switch to Input at right").NewRow();
			table.Add(" I").Add("Switch to Input at line start").NewRow();
			table.Add(" A").Add("Switch to Input at line end").NewRow();
			table.Add(" c<move>").Add("Remove and switch to Input").NewRow();
			table.Add(" d<move>").Add("Remove").NewRow();
			table.Add(" x").Add("Remove char").NewRow();
			table.Add(" s").Add("Remove char and switch to Input").NewRow();
			table.Add(" y<move>").Add("Copy").NewRow();
			table.Add(" yy").Add("Copy line").NewRow();
			table.Add(" p").Add("Past").NewRow();
			table.Add(" P").Add("Past before").NewRow();
			table.Add(" .").Add("Repeat last action").NewRow();
			table.Add(" u").Add("Undo").NewRow();
			table.Add(" Ctrl+r").Add("Redo").NewRow();
			table.Add(" dd").Add("Remove line").NewRow();
			table.Add(" cc").Add("Remove line and switch to Input").NewRow();
			table.Add(" o").Add("New line and switch to Input").NewRow();
			table.Add(" O").Add("New line before and switch to Input").NewRow();
			table.Add(" C").Add("Remove to line end and switch to Input").NewRow();
			table.Add(" D").Add("Remove to line end").NewRow();
			table.Add(" :").Add("Open command dialog, inside:\n`Ctrl+f` - normal mode inside dialog\n`Ctrl+]` - close dialog").NewRow();
			table.Add(" /").Add("Open find dialog, inside:\n`Ctrl+f` - normal mode inside dialog\n`Ctrl+]` - close dialog").NewRow();
			table.Add(" *").Add("Put word or selection into find register and find next").NewRow();
			table.Add(" n").Add("Find next").NewRow();
			table.Add(" N").Add("Find previous").NewRow();
			table.Add(" ,b").Add("Show tab list, inside:\n`Enter` - Select tab\n`dd`- Close tab\n`Ctrl+[` - Exit tab list").NewRow();
			table.Add(" <space><symbol><showed_symbols>").Add("Jump where you look:\n<symbol> - symbol, what you look,\n<showed_symbols> - symbols,\nshowed after <symbol> entered").NewRow();
			table.Add(" ,<space><symbol><showed_symbols>").Add("Jump with new cursor");
			table.AddLine();
			table.Add(" Text object").Add("Region");
			table.AddLine();
			table.Add(" <action>i<object>").Add("Apply action to text object").NewRow();
			table.Add(" w").Add("Word").NewRow();
			table.Add(" {, }, (, ), [, ]").Add("Text inside brackets").NewRow();
			table.Add(" \"").Add("Text inside quotes").NewRow();
			table.Add(" '").Add("Text inside single quotes");
			table.AddLine();
			table.Add(" Text object").Add("Region");
			table.AddLine();
			table.Add(" \"<register><y|yy|p|P|<action>>").Add("Common form, if register empty - uses default").NewRow();
			table.Add(" \"*, \"-").Add("System clipboard").NewRow();
			table.Add(" \"a - \"z").Add("Innder registers").NewRow();
			table.Add(" \"A - \"Z").Add("The same registers with accumulation").NewRow();
			table.Add(" \"/").Add("Find register");
			builder.Append(table);
		}
		builder.AppendLine();
		builder.AppendLine("Also don't foget for input mode shortcuts, that hasn't vi-mode analouges:");
		builder.AppendLine("  `Ctrl+E` - switch betwean frames");
		builder.AppendLine("  `Ctrl+Shift+O` - open previous files");
		builder.AppendLine("  `Ctrl+Shift+P` - open other shortcuts list (context-depenent)");
		Buffer buffer = new Buffer(null, "Vi-help.twh", SettingsMode.Help);
		buffer.tags = BufferTag.Other;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.Lines.ranges = ranges;
		return buffer;
	}
	
	public static void AddHead(StringBuilder builder, string name)
	{
		builder.AppendLine("[[ " + name + " ]]");
		builder.AppendLine();
		builder.AppendLine(Application.ProductName + ", build " + Application.ProductVersion + ", official site:");
		builder.AppendLine("https://github.com/cser/typewriter-net");
		builder.AppendLine();
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