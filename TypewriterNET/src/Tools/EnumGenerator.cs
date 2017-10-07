using System;
using System.Collections.Generic;
using System.Text;

public class EnumGenerator
{
	public enum Mode
	{
		Number,
		ZeroBeforeNumber
	}
	
	public readonly List<string> texts = new List<string>();
	public string error;
	
	private readonly Mode mode;
	
	public EnumGenerator(string text, int count, Mode mode)
	{
		this.mode = mode;
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
			if (trimmed.Count > 0 && !int.TryParse(trimmed[0], out number))
			{
				if (trimmed[0].Length != 1)
				{
					error = "Number mast be number";
					return;
				}
				AddChars(trimmed[0][0], step, count, selectionsCount);
				return;
			}
		}
		AddNumbers(number, step, count, selectionsCount);
	}
	
	private void AddNumbers(int number, int step, int count, int selectionsCount)
	{
		int number0 = number;
		int number1 = number + selectionsCount * count * step - 1;
		int maxNumberLength = (Math.Abs(number1) + "").Length;
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
				if (mode == Mode.ZeroBeforeNumber)
				{
					if ((number0 < 0 || number1 < 0) && number >= 0)
					{
						builder.Append(' ');
					}
				}
				string numberText = Math.Abs(number) + "";
				if (number < 0)
				{
					builder.Append('-');
				}
				if (mode == Mode.ZeroBeforeNumber)
				{
					for (int k = maxNumberLength - numberText.Length; k-- > 0;)
					{
						builder.Append('0');
					}
				}
				builder.Append(numberText);
				number += step;
			}
			texts.Add(builder.ToString());
		}
	}
	
	private void AddChars(char c0, int step, int count, int selectionsCount)
	{
		char c = c0;
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
				builder.Append(c);
				c += (char)step;
			}
			texts.Add(builder.ToString());
		}
	}
}