using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using TinyJSON;

public class SharpRequest
{
	private readonly MainForm mainForm;
	private readonly NameValueCollection parameters = new NameValueCollection();
	
	public SharpRequest(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}
	
	public SharpRequest Add(string name, string value)
	{
		parameters[name] = value;
		return this;
	}
	
	public string SendWithRawOutput(string httpServer, out string error)
	{
		error = null;
		string output = null;
		using (WebClient client = new WebClient())
		{
			try
			{
				byte[] bytes = client.UploadValues(httpServer, "POST", parameters);
				output = Encoding.UTF8.GetString(bytes);
			}
			catch (Exception e)
			{
				error = "HTTP error: " + e.ToString();
			}
		}
		return output;
	}
	
	public Node Send(string httpServer, bool showOutput)
	{
		string output = null;
		using (WebClient client = new WebClient())
		{
			try
			{
				byte[] bytes = client.UploadValues(httpServer, "POST", parameters);
				output = Encoding.UTF8.GetString(bytes);
			}
			catch (Exception e)
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "HTTP error: " + e.ToString());
			}
		}
		if (output != null)
		{
			if (showOutput)
				mainForm.Log.WriteInfo("OmniSharp", "OUTPUT: " + output);
			Node node = null;
			try
			{
				node = new Parser().Load(output);
				return node;
			}
			catch (Exception e)
			{
				mainForm.Dialogs.ShowInfo("OmniSharp", "Response parsing error: " + e.Message + "\n" + output);
			}
		}
		return null;
	}
	
	public Node Send(string httpServer, out string error)
	{
		error = null;
		string output = null;
		using (WebClient client = new WebClient())
		{
			try
			{
				byte[] bytes = client.UploadValues(httpServer, "POST", parameters);
				output = Encoding.UTF8.GetString(bytes);
			}
			catch (Exception e)
			{
				error = "HTTP error: " + e.ToString();
			}
		}
		if (output != null)
		{
			Node node = null;
			try
			{
				node = new Parser().Load(output);
				return node;
			}
			catch (Exception e)
			{
				error = "Response parsing error: " + e.Message + "\n" + output;
			}
		}
		return null;
	}
}