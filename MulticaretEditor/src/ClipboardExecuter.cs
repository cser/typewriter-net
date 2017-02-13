using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MulticaretEditor
{
	public class ClipboardExecuter
	{
		public string text;
		
		private void PutTo()
		{
			try
			{
				if (text != null && text != "")
					Clipboard.SetText(text);
			}
			catch (ExternalException)
			{
			}
		}
		
		private void GetFrom()
		{
			if (Clipboard.ContainsText())
				text = Clipboard.GetText();
		}
		
		private const int RegistersCount = 28;
		private static bool useFake = false;
		private static string fakeText = "";
		private static string[] registers = new string[RegistersCount];
		
		public static string viLastCommand = "";
		public static string viLastInsertText = "";
		public static string viFileName = "";
		
		public static void Reset(bool useFake)
		{
			ClipboardExecuter.useFake = useFake;
			fakeText = "";
			registers = new string[RegistersCount];
			viLastCommand = "";
			viLastInsertText = "";
			viFileName = "";
		}
		
		public static void PutToClipboard(string text)
		{
			if (useFake)
			{
				fakeText = text;
				return;
			}
			ClipboardExecuter executer = new ClipboardExecuter();
			executer.text = text;
			Thread thread = new Thread(executer.PutTo);
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}
		
		public static string GetFromClipboard()
		{
			if (useFake)
			{
				return fakeText;
			}
			ClipboardExecuter executer = new ClipboardExecuter();
			Thread thread = new Thread(executer.GetFrom);
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			return executer.text;
		}
		
		public static void PutToRegister(char c, string text)
		{
			if (c == '*' || c == '-')
			{
				PutToClipboard(text);
			}
			else if (c == '0' || c == '\0')
			{
				registers[27] = text;
			}
			else if (c >= 'a' && c <= 'z')
			{
				registers[c - 'a'] = text;
			}
			else if (c >= 'A' && c <= 'Z')
			{
				registers[c - 'A'] += text;
			}
			else if (c == '/')
			{
				registers[26] = text;
			}
		}
		
		public static string GetFromRegister(char c)
		{
			string result = null;
			if (c == '*' || c == '-')
			{
				result = GetFromClipboard();
			}
			else if (c == '0' || c == '\0')
			{
				result = registers[27];
			}
			else if (c >= 'a' && c <= 'z')
			{
				result = registers[c - 'a'];
			}
			else if (c >= 'A' && c <= 'Z')
			{
				result = registers[c - 'A'];
			}
			else if (c == '/')
			{
				result = registers[26];
			}
			else if (c == ':')
			{
				result = viLastCommand;
			}
			else if (c == '.')
			{
				result = viLastInsertText;
			}
			else if (c == '%')
			{
				result = viFileName;
			}
			return result ?? "";
		}
	}
}
