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
		public static bool fakeLayout = false;
		public static bool fakeEnLayout = true;
		
		private static CharsRegularExpressions.Regex _viRegex;
		public static CharsRegularExpressions.Regex ViRegex { get { return _viRegex; } }
		
		private static CharsRegularExpressions.Regex _viBackwardRegex;
		public static CharsRegularExpressions.Regex ViBackwardRegex { get { return _viBackwardRegex; } }
		
		public static void Reset(bool useFake)
		{
			ClipboardExecuter.useFake = useFake;
			fakeText = "";
			registers = new string[RegistersCount];
			viLastCommand = "";
			viLastInsertText = "";
			viFileName = "";
			fakeLayout = false;
			fakeEnLayout = true;
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
				if (string.IsNullOrEmpty(text))
				{
					_viRegex = null;
					_viBackwardRegex = null;
				}
				else
				{
					for (int i = 0; i < 2; i++)
					{
						if (i == 1)
						{
							text = ViReceiverVisual.Escape(text);
						}
						try
						{
							System.TimeSpan span = new System.TimeSpan(0, 0, 0, 0, 200);
							CharsRegularExpressions.RegexOptions options = CharsRegularExpressions.RegexOptions.None;
							if (text.Length < 50)
							{
								options |= CharsRegularExpressions.RegexOptions.Compiled;
							}
							_viRegex = new CharsRegularExpressions.Regex(text, options, span);
							_viBackwardRegex = new CharsRegularExpressions.Regex(
								text, CharsRegularExpressions.RegexOptions.RightToLeft | options, span);
							break;
						}
						catch
						{
							_viRegex = null;
							_viBackwardRegex = null;
						}
					}
				}
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
		
		public static bool IsEnLayout()
		{
			string name = InputLanguage.CurrentInputLanguage.Culture.Name;
			bool result = name[0] == 'e' && name[1] == 'n';
			if (fakeLayout)
			{
				result = fakeEnLayout;
			}
			return result;
		}
	}
}
