using System.Collections.Generic;

public class EnumGenerator
{
	public readonly List<string> texts = new List<string>();
	public string error;
	
	public EnumGenerator(string text, int count)
	{
		Execute(text, count);
	}
	
	private void Execute(string text, int count)
	{
		int number = 1;
		int step = 1;
		if (!string.IsNullOrEmpty(text) && text.Trim() != "")
		{
			int index = text.IndexOf(' ');
			string rawNumber = null;
			string rawStep = null;
			if (index != -1)
			{
				rawNumber = text.Substring(0, index).Trim();
				rawStep = text.Substring(index + 1).Trim();
			}
			else
			{
				rawNumber = text.Trim();
			}
			if (rawNumber != null && !int.TryParse(rawNumber, out number))
			{
				error = "Number mast be number";
				return;
			}
			if (rawStep != null && !int.TryParse(rawStep, out step))
			{
				error = "Step mast be number";
				return;
			}
		}
		for (int i = 0; i < count; i++)
		{
			texts.Add(number + "");
			number += step;
		}
	}
}