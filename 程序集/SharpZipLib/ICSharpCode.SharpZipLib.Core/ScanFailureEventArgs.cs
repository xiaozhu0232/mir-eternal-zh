using System;

namespace ICSharpCode.SharpZipLib.Core;

public class ScanFailureEventArgs
{
	private string name;

	private Exception exception;

	private bool continueRunning;

	public string Name => name;

	public Exception Exception => exception;

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

	public ScanFailureEventArgs(string name, Exception e)
	{
		this.name = name;
		exception = e;
		continueRunning = true;
	}
}
