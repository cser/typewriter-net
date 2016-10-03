using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using MulticaretEditor;
using MulticaretEditor.KeyMapping;
using MulticaretEditor.Highlighting;

public class SharpManager
{
	private readonly MainForm mainForm;
	
	public SharpManager(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}
	
	private string omnisharpSln;
	private int omnisharpPort;
	private int realPort;
	private bool omnisharpConsole;
	
	public bool Started { get { return server != null; } }
	public string Url { get { return "http://localhost:" + realPort; } }
	public string AutocompleteUrl { get { return Url + "/autocomplete"; } }
	
	public void UpdateSettings(Settings settings)
	{
		if ((omnisharpSln + "") != (settings.omnisharpSln.Value + "") ||
			omnisharpPort != settings.omnisharpPort.Value ||
			omnisharpConsole != settings.omnisharpConsole.Value)
		{
			omnisharpSln = settings.omnisharpSln.Value;
			omnisharpPort = settings.omnisharpPort.Value;
			realPort = omnisharpPort;
			omnisharpConsole = settings.omnisharpConsole.Value;
			ApplySettings();
		}
	}
	
	public void Close()
	{
		KillServer();
	}
	
	private void KillServer()
	{
		if (server != null)
		{
			try
			{
				server.Kill();
				server = null;
			}
			catch (Exception e)
			{
				mainForm.Log.WriteError("Omnisharp", "Server close error: " + e.Message);
				mainForm.Log.Open();
			}
		}
	}
	
	private static bool AreEquals(IEnumerable<string> a, IEnumerable<string> b)
	{
		int aCount = 0;
		foreach (string s in a)
		{
			aCount++;
		}
		int bCount = 0;
		foreach (string s in b)
		{
			bCount++;
		}
		if (aCount != bCount)
			return false;
		IEnumerator<string> bEnumerator = b.GetEnumerator();
		foreach (string s in a)
		{
			bEnumerator.MoveNext();
			if (s != bEnumerator.Current)
				return false;
		}		
		return true;
	}
	
	private Process server;
	
	private void ApplySettings()
	{
		if (server != null)
		{
			KillServer();
		}
		if (!string.IsNullOrEmpty(omnisharpSln))
		{
			realPort = FindFreePort(omnisharpPort);
			mainForm.Log.WriteInfo("Omnisharp", omnisharpSln + " - connecting to: " + realPort);
			server = new Process();
			server.StartInfo.UseShellExecute = omnisharpConsole;
			server.StartInfo.CreateNoWindow = !omnisharpConsole;
			server.StartInfo.FileName = Path.Combine(AppPath.StartupDir, "omnisharp_server/OmniSharp.exe");
			server.StartInfo.Arguments = "-s " + omnisharpSln + " -p=" + realPort;
			server.Start();
		}
	}
	
	public int FindFreePort(int port)
	{
		IPAddress address = Dns.GetHostAddresses("localhost")[0];
		for (int offset = 0; offset < 10; offset++)
		{
			TcpListener listener = new TcpListener(address, port);
			try
			{
				listener.Start();
				listener.Stop();
				return port;
			}
			catch (Exception)
			{
				mainForm.Log.WriteInfo("Omnisharp", "Consumed port: " + port);
				port++;
			}
		}
		return port;
	}
}
