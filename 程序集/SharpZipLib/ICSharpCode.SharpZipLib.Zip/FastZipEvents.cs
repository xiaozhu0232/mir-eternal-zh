using System;
using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip;

public class FastZipEvents
{
	public ProcessDirectoryDelegate ProcessDirectory;

	public ProcessFileDelegate ProcessFile;

	public DirectoryFailureDelegate DirectoryFailure;

	public FileFailureDelegate FileFailure;

	public void OnDirectoryFailure(string directory, Exception e)
	{
		if (DirectoryFailure != null)
		{
			ScanFailureEventArgs e2 = new ScanFailureEventArgs(directory, e);
			DirectoryFailure(this, e2);
		}
	}

	public void OnFileFailure(string file, Exception e)
	{
		if (FileFailure != null)
		{
			ScanFailureEventArgs e2 = new ScanFailureEventArgs(file, e);
			FileFailure(this, e2);
		}
	}

	public void OnProcessFile(string file)
	{
		if (ProcessFile != null)
		{
			ScanEventArgs e = new ScanEventArgs(file);
			ProcessFile(this, e);
		}
	}

	public void OnProcessDirectory(string directory, bool hasMatchingFiles)
	{
		if (ProcessDirectory != null)
		{
			DirectoryEventArgs e = new DirectoryEventArgs(directory, hasMatchingFiles);
			ProcessDirectory(this, e);
		}
	}
}
