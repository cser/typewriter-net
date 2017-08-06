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
		builder.AppendLine("# " + Application.ProductName + ", Build " + Application.ProductVersion +
			" - input mode help");
		builder.AppendLine();
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
		builder.AppendLine("Vi-mode enable by Ctrl+]");
		builder.AppendLine("Other info reed in vi-mode help by Shift+F1");
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
		builder.AppendLine("# " + Application.ProductName + ", Build " + Application.ProductVersion +
			" - vi-mode help");
		builder.AppendLine();
		builder.AppendLine("# Common");
		builder.AppendLine();
		builder.AppendLine("- Vi mode can be enabled by `Ctrl+[`");
		builder.AppendLine("- For default Normal mode use `" + settings.startWithViMode.name + "` config parameter");
		builder.AppendLine("- Parameters `" + settings.viMapSource.name + "` and `" + settings.viMapResult.name + "`");
		builder.AppendLine("  uses for mapping another keyboard layout to vi commands");
		builder.AppendLine("  For expample see base config by `Shift+F12`");
		builder.AppendLine("- There are no vi macroses implemented,");
		builder.AppendLine("  use simple `Ctrl-Q`(begin), `Ctrl-Q`(end), `Ctrl-Shift-Q`(execute)");
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
		TextTable table = new TextTable().SetMaxColWidth(40);
		table.Add("Sequence").Add("Move");
		table.AddLine();
		table.Add("w").Add("Move word rigth").NewRow();
		table.Add("e").Add("Move word rigth without space").NewRow();
		table.Add("b").Add("Move word left").NewRow();
		table.Add("gg").Add("Move to document start").NewRow();
		table.Add("G").Add("Move to document end").NewRow();
		table.Add("<number>G").Add("Move to line number").NewRow();
		table.Add("%").Add("Move to bracket under cursor pair");
		table.AddLine();
		table.Add("Sequence").Add("Action");
		table.AddLine();
		table.Add("i").Add("Switch to Input with cursor at left").NewRow();
		table.Add("a").Add("Switch to Input with cursor at right").NewRow();
		table.Add("c<move>").Add("Remove and switch to Input").NewRow();
		table.Add("d<move>").Add("Remove").NewRow();
		table.Add("x").Add("Remove char").NewRow();
		table.Add("y<move>").Add("Copy").NewRow();
		table.Add("p").Add("Past").NewRow();
		table.Add("P").Add("Past before").NewRow();
		table.Add(".").Add("Repeat last action").NewRow();
		table.Add("u").Add("Undo").NewRow();
		table.Add("Ctrl+r").Add("Redo");
		table.AddLine();
		table.Add("Text object").Add("Region");
		table.AddLine();
		table.Add("<action>i<object>").Add("Apply action to text object").NewRow();
		table.Add("w").Add("Word").NewRow();
		table.Add("{, }, (, ), [, ]").Add("Text inside brackets").NewRow();
		table.Add("\", '").Add("Text inside quotes");
		builder.Append(table);
		Buffer buffer = new Buffer(null, "Vi-help.twh", SettingsMode.Help);
		buffer.tags = BufferTag.Other;
		buffer.Controller.isReadonly = true;
		buffer.Controller.InitText(builder.ToString());
		buffer.Controller.Lines.ranges = ranges;
		return buffer;
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