using System;

namespace ICSharpCode.SharpZipLib.Core;

public class ScanEventArgs : EventArgs
{
	private string name;

	private bool continueRunning;

	public string Name => name;

	public bool ContinueRunning
	{
		get
		{
			return continueRunning;
		}
		set
		{
			continueRunning = value;
		}
	}

	public ScanEventArgs(string name)
	{
		this.name = name;
		ContinueRunning = true;
	}
}
