using System;
using System.Collections.Generic;
using System.Text;

public class TextTable
{
	private List<List<string>> rows = new List<List<string>>();

	public TextTable()
	{
		rows.Add(new List<string>());
	}

	private int maxColWidth = 100;

	public TextTable SetMaxColWidth(int maxColWidth)
	{
		this.maxColWidth = maxColWidth;
		return this;
	}

	public TextTable Add(string text)
	{
		List<string> list = rows[rows.Count - 1];
		list.Add(text);
		return this;
	}

	public TextTable AddLine()
	{
		rows.Add(null);
		rows.Add(new List<string>());
		return this;
	}

	public TextTable NewRow()
	{
		rows.Add(new List<string>());
		return this;
	}

	override public string ToString()
	{
		int colsCount = 0;
		int rowsCount = rows.Count;
		for (int i = 0; i < rowsCount; i++)
		{
			List<string> list = rows[i];
			if (list != null)
			{
				if (colsCount < list.Count)
					colsCount = list.Count;
			}
		}
		int[] colSizes = new int[colsCount];
		for (int i = 0; i < colsCount; i++)
		{
			for (int j = 0; j < rowsCount; j++)
			{
				List<string> list = rows[j];
				if (list != null)
				{
					string text = i < list.Count ? list[i] : "";
					int length = Math.Min(maxColWidth, text.Length);
					if (colSizes[i] < length)
						colSizes[i] = length;
				}
			}
		}
		int width = (colsCount - 1) * 3;
		for (int i = 0; i < colsCount; i++)
		{
			width += colSizes[i];
		}
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < rowsCount; i++)
		{
			List<string> list = rows[i];
			if (list != null)
			{
				int k = 0;
				while (true)
				{
					bool allCompleted = true;
					for (int j = 0; j < colsCount; j++)
					{
						if (j > 0)
							builder.Append(" | ");
						string text = j < list.Count ? list[j] : "";
						int colSize = colSizes[j];
						if (k * colSize < text.Length)
						{
							if ((k + 1) * colSize < text.Length)
							{
								allCompleted = false;
								text = text.Substring(k * colSize, colSize);
							}
							else
							{
								text = text.Substring(k * colSize);
							}
							builder.Append(text.PadRight(colSizes[j]));
						}
						else
						{
							builder.Append(new string(' ', colSizes[j]));
						}
					}
					k++;
					builder.AppendLine();
					if (allCompleted)
						break;
				}
			}
			else
			{
				builder.AppendLine(new string('-', width));
			}
		}
		return builder.ToString();
	}
}
