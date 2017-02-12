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
		
		private static bool useFake = false;
		private static string fakeText;
		private static string[] registers;
		
		public static void Reset(bool useFake)
		{
			ClipboardExecuter.useFake = useFake;
			registers = new string[26];
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
			if (c >= 'a' && c <= 'z')
			{
				registers[c - 'a'] = text;
			}
		}
		
		public static string GetFromRegister(char c)
		{
			if (c >= 'a' && c <= 'z')
			{
				return registers[c - 'a'];
			}
			return "";
		}
	}
}
