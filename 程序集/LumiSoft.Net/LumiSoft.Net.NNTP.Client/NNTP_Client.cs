using System;
using System.Collections.Generic;
using System.IO;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.NNTP.Client;

public class NNTP_Client : TCP_Client
{
	public override void Disconnect()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("NNTP client is not connected.");
		}
		try
		{
			WriteLine("QUIT");
		}
		catch
		{
		}
		try
		{
			base.Disconnect();
		}
		catch
		{
		}
	}

	public string[] GetNewsGroups()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("NNTP client is not connected.");
		}
		WriteLine("LIST");
		string text = ReadLine();
		if (!text.StartsWith("215"))
		{
			throw new Exception(text);
		}
		List<string> list = new List<string>();
		text = ReadLine();
		while (text != ".")
		{
			list.Add(text.Split(' ')[0]);
			text = ReadLine();
		}
		return list.ToArray();
	}

	public void PostMessage(string newsgroup, Stream message)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("NNTP client is not connected.");
		}
		WriteLine("POST");
		string text = ReadLine();
		if (!text.StartsWith("340"))
		{
			throw new Exception(text);
		}
		TcpStream.WritePeriodTerminated(message);
		text = ReadLine();
		if (!text.StartsWith("240"))
		{
			throw new Exception(text);
		}
	}

	protected override void OnConnected()
	{
		string text = ReadLine();
		if (!text.StartsWith("200"))
		{
			throw new Exception(text);
		}
	}
}
