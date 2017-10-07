using System.Collections.Generic;
using System.Text;

public class EnumGenerator
{
	public readonly List<string> texts = new List<string>();
	public string error;
	
	public EnumGenerator(string text, int count)
	{
		Execute(text, count);
	}
	
	private void Execute(string text, int selectionsCount)
	{
		int number = 1;
		int step = 1;
		int count = 1;
		if (!string.IsNullOrEmpty(text) && text.Trim() != "")
		{
			string[] raw = text.Split(new char[] { ' ', '\t' });
			List<string> trimmed = new List<string>();
			for (int i = 0; i < raw.Length; i++)
			{
				string trimmedI = raw[i].Trim();
				if (trimmedI != "")
				{
					trimmed.Add(trimmedI);
				}
			}
			if (trimmed.Count > 0 && !int.TryParse(trimmed[0], out number))
			{
				error = "Number mast be number";
				return;
			}
			if (trimmed.Count > 1 && !int.TryParse(trimmed[1], out step))
			{
				error = "Step mast be number";
				return;
			}
			if (trimmed.Count > 2 && !int.TryParse(trimmed[2], out count))
			{
				error = "Count mast be number";
				return;
			}
		}
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < selectionsCount; i++)
		{
			builder.Length = 0;
			for (int j = 0; j < count; j++)
			{
				if (j > 0)
				{
					builder.Append(' ');
				}
				builder.Append(number + "");
				number += step;
			}
			texts.Add(builder.ToString());
		}
	}
}