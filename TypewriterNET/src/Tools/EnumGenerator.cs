using MulticaretEditor;
using System;
using System.Collections.Generic;
using System.Text;

public class EnumGenerator
{
	public enum Mode
	{
		Number,
		ZeroBeforeNumber,
		Roman
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
		char numberC = '\0';
		bool numberExists = false;
		bool numberCorrect = false;
		int step = 1;
		bool stepExists = false;
		bool stepCorrect = false;
		int count = 1;
		bool countExists = false;
		bool countCorrect = false;
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
			if (trimmed.Count > 0)
			{
				numberExists = true;
				if (int.TryParse(trimmed[0], out number))
				{
					numberCorrect = true;
				}
				else if (mode == Mode.Roman)
				{
					number = CommonHelper.OfRoman(trimmed[0]);
					numberCorrect = number != 0;
				}
				else if (trimmed[0].Length == 1)
				{
					numberCorrect = true;
					numberC = trimmed[0][0];
				}
			}
			if (trimmed.Count > 1)
			{
				stepExists = true;
				if (int.TryParse(trimmed[1], out step))
				{
					stepCorrect = true;
				}
				else if (mode == Mode.Roman)
				{
					step = CommonHelper.OfRoman(trimmed[1]);
					stepCorrect = step != 0;
				}
			}
			if (trimmed.Count > 2)
			{
				countExists = true;
				if (int.TryParse(trimmed[2], out count))
				{
					countCorrect = true;
				}
				else if (mode == Mode.Roman)
				{
					count = CommonHelper.OfRoman(trimmed[2]);
					countCorrect = count != 0;
				}
			}
			if (numberExists && !numberCorrect)
			{
				error = mode != Mode.Roman ? "Expected number or one char" : "Expected number or roman";
				return;
			}
			if (stepExists && !stepCorrect)
			{
				error = "Step must be number";
				return;
			}
			if (countExists && !countCorrect)
			{
				error = "Count must be number";
				return;
			}
			if (numberC != '\0')
			{
				AddChars(numberC, step, count, selectionsCount);
				return;
			}
		}
		if (mode == Mode.Roman)
		{
			AddRomans(number, step, count, selectionsCount);
			return;
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
				int nextC = c + step;
				if (nextC < 32)
				{
					nextC = 32;
				}
				else if (nextC > 0xffff)
				{
					nextC = 0xffff;
				}
				c = (char)nextC;
			}
			texts.Add(builder.ToString());
		}
	}
	
	private void AddRomans(int n0, int step, int count, int selectionsCount)
	{
		int number = n0;
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
				builder.Append(CommonHelper.RomanOf(number));
				number += step;
			}
			texts.Add(builder.ToString());
		}
	}
}