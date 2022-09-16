using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Core;

public class FileSystemScanner
{
	public ProcessDirectoryDelegate ProcessDirectory;

	public ProcessFileDelegate ProcessFile;

	public DirectoryFailureDelegate DirectoryFailure;

	public FileFailureDelegate FileFailure;

	private IScanFilter fileFilter;

	private IScanFilter directoryFilter;

	private bool alive;

	public FileSystemScanner(string filter)
	{
		fileFilter = new PathFilter(filter);
	}

	public FileSystemScanner(string fileFilter, string directoryFilter)
	{
		this.fileFilter = new PathFilter(fileFilter);
		this.directoryFilter = new PathFilter(directoryFilter);
	}

	public FileSystemScanner(IScanFilter fileFilter)
	{
		this.fileFilter = fileFilter;
	}

	public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
	{
		this.fileFilter = fileFilter;
		this.directoryFilter = directoryFilter;
	}

	public void OnDirectoryFailure(string directory, Exception e)
	{
		if (DirectoryFailure == null)
		{
			alive = false;
			return;
		}
		ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(directory, e);
		DirectoryFailure(this, scanFailureEventArgs);
		alive = scanFailureEventArgs.ContinueRunning;
	}

	public void OnFileFailure(string file, Exception e)
	{
		if (FileFailure == null)
		{
			alive = false;
			return;
		}
		ScanFailureEventArgs scanFailureEventArgs = new ScanFailureEventArgs(file, e);
		FileFailure(this, scanFailureEventArgs);
		alive = scanFailureEventArgs.ContinueRunning;
	}

	public void OnProcessFile(string file)
	{
		if (ProcessFile != null)
		{
			ScanEventArgs scanEventArgs = new ScanEventArgs(file);
			ProcessFile(this, scanEventArgs);
			alive = scanEventArgs.ContinueRunning;
		}
	}

	public void OnProcessDirectory(string directory, bool hasMatchingFiles)
	{
		if (ProcessDirectory != null)
		{
			DirectoryEventArgs directoryEventArgs = new DirectoryEventArgs(directory, hasMatchingFiles);
			ProcessDirectory(this, directoryEventArgs);
			alive = directoryEventArgs.ContinueRunning;
		}
	}

	public void Scan(string directory, bool recurse)
	{
		alive = true;
		ScanDir(directory, recurse);
	}

	private void ScanDir(string directory, bool recurse)
	{
		try
		{
			string[] files = Directory.GetFiles(directory);
			bool flag = false;
			for (int i = 0; i < files.Length; i++)
			{
				if (!fileFilter.IsMatch(files[i]))
				{
					files[i] = null;
				}
				else
				{
					flag = true;
				}
			}
			OnProcessDirectory(directory, flag);
			if (alive && flag)
			{
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						if (text != null)
						{
							OnProcessFile(text);
							if (!alive)
							{
								break;
							}
						}
					}
					catch (Exception e)
					{
						OnFileFailure(text, e);
					}
				}
			}
		}
		catch (Exception e)
		{
			OnDirectoryFailure(directory, e);
		}
		if (!alive || !recurse)
		{
			return;
		}
		try
		{
			string[] files = Directory.GetDirectories(directory);
			string[] array = files;
			foreach (string text2 in array)
			{
				if (directoryFilter == null || directoryFilter.IsMatch(text2))
				{
					ScanDir(text2, recurse: true);
					if (!alive)
					{
						break;
					}
				}
			}
		}
		catch (Exception e)
		{
			OnDirectoryFailure(directory, e);
		}
	}
}
