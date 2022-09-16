using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip;

public class FastZip
{
	public enum Overwrite
	{
		Prompt,
		Never,
		Always
	}

	public delegate bool ConfirmOverwriteDelegate(string fileName);

	private byte[] buffer;

	private ZipOutputStream outputStream;

	private ZipInputStream inputStream;

	private string password = null;

	private string targetDirectory;

	private string sourceDirectory;

	private NameFilter fileFilter;

	private NameFilter directoryFilter;

	private Overwrite overwrite;

	private ConfirmOverwriteDelegate confirmDelegate;

	private bool restoreDateTime = false;

	private bool createEmptyDirectories = false;

	private FastZipEvents events;

	private ZipNameTransform nameTransform;

	public bool CreateEmptyDirectories
	{
		get
		{
			return createEmptyDirectories;
		}
		set
		{
			createEmptyDirectories = value;
		}
	}

	public ZipNameTransform NameTransform
	{
		get
		{
			return nameTransform;
		}
		set
		{
			if (value == null)
			{
				nameTransform = new ZipNameTransform();
			}
			else
			{
				nameTransform = value;
			}
		}
	}

	public FastZip()
	{
		events = null;
	}

	public FastZip(FastZipEvents events)
	{
		this.events = events;
	}

	public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
	{
		NameTransform = new ZipNameTransform(useRelativePaths: true, sourceDirectory);
		this.sourceDirectory = sourceDirectory;
		outputStream = new ZipOutputStream(File.Create(zipFileName));
		try
		{
			FileSystemScanner fileSystemScanner = new FileSystemScanner(fileFilter, directoryFilter);
			fileSystemScanner.ProcessFile = (ProcessFileDelegate)Delegate.Combine(fileSystemScanner.ProcessFile, new ProcessFileDelegate(ProcessFile));
			if (CreateEmptyDirectories)
			{
				fileSystemScanner.ProcessDirectory = (ProcessDirectoryDelegate)Delegate.Combine(fileSystemScanner.ProcessDirectory, new ProcessDirectoryDelegate(ProcessDirectory));
			}
			fileSystemScanner.Scan(sourceDirectory, recurse);
		}
		finally
		{
			outputStream.Close();
		}
	}

	public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter)
	{
		CreateZip(zipFileName, sourceDirectory, recurse, fileFilter, null);
	}

	public void ExtractZip(string zipFileName, string targetDirectory, string fileFilter)
	{
		ExtractZip(zipFileName, targetDirectory, Overwrite.Always, null, fileFilter, null);
	}

	public void ExtractZip(string zipFileName, string targetDirectory, Overwrite overwrite, ConfirmOverwriteDelegate confirmDelegate, string fileFilter, string directoryFilter)
	{
		if (overwrite == Overwrite.Prompt && confirmDelegate == null)
		{
			throw new ArgumentNullException("confirmDelegate");
		}
		this.overwrite = overwrite;
		this.confirmDelegate = confirmDelegate;
		this.targetDirectory = targetDirectory;
		this.fileFilter = new NameFilter(fileFilter);
		this.directoryFilter = new NameFilter(directoryFilter);
		inputStream = new ZipInputStream(File.OpenRead(zipFileName));
		try
		{
			if (password != null)
			{
				inputStream.Password = password;
			}
			ZipEntry nextEntry;
			while ((nextEntry = inputStream.GetNextEntry()) != null)
			{
				if (this.directoryFilter.IsMatch(Path.GetDirectoryName(nextEntry.Name)) && this.fileFilter.IsMatch(nextEntry.Name))
				{
					ExtractEntry(nextEntry);
				}
			}
		}
		finally
		{
			inputStream.Close();
		}
	}

	private void ProcessDirectory(object sender, DirectoryEventArgs e)
	{
		if (!e.HasMatchingFiles && createEmptyDirectories)
		{
			if (events != null)
			{
				events.OnProcessDirectory(e.Name, e.HasMatchingFiles);
			}
			if (e.Name != sourceDirectory)
			{
				string name = nameTransform.TransformDirectory(e.Name);
				ZipEntry entry = new ZipEntry(name);
				outputStream.PutNextEntry(entry);
			}
		}
	}

	private void ProcessFile(object sender, ScanEventArgs e)
	{
		if (events != null)
		{
			events.OnProcessFile(e.Name);
		}
		string name = nameTransform.TransformFile(e.Name);
		ZipEntry entry = new ZipEntry(name);
		outputStream.PutNextEntry(entry);
		AddFileContents(e.Name);
	}

	private void AddFileContents(string name)
	{
		if (buffer == null)
		{
			buffer = new byte[4096];
		}
		FileStream fileStream = File.OpenRead(name);
		try
		{
			int num;
			do
			{
				num = fileStream.Read(buffer, 0, buffer.Length);
				outputStream.Write(buffer, 0, num);
			}
			while (num > 0);
		}
		finally
		{
			fileStream.Close();
		}
	}

	private void ExtractFileEntry(ZipEntry entry, string targetName)
	{
		bool flag = true;
		if (overwrite == Overwrite.Prompt && confirmDelegate != null && File.Exists(targetName))
		{
			flag = confirmDelegate(targetName);
		}
		if (!flag)
		{
			return;
		}
		if (events != null)
		{
			events.OnProcessFile(entry.Name);
		}
		FileStream fileStream = File.Create(targetName);
		try
		{
			if (buffer == null)
			{
				buffer = new byte[4096];
			}
			int num;
			do
			{
				num = inputStream.Read(buffer, 0, buffer.Length);
				fileStream.Write(buffer, 0, num);
			}
			while (num > 0);
		}
		finally
		{
			fileStream.Close();
		}
		if (restoreDateTime)
		{
			File.SetLastWriteTime(targetName, entry.DateTime);
		}
	}

	private bool NameIsValid(string name)
	{
		return name != null && name.Length > 0 && name.IndexOfAny(Path.InvalidPathChars) < 0;
	}

	private void ExtractEntry(ZipEntry entry)
	{
		bool flag = NameIsValid(entry.Name);
		string path = null;
		string text = null;
		if (flag)
		{
			string text2;
			if (Path.IsPathRooted(entry.Name))
			{
				string pathRoot = Path.GetPathRoot(entry.Name);
				pathRoot = entry.Name.Substring(pathRoot.Length);
				text2 = Path.Combine(Path.GetDirectoryName(pathRoot), Path.GetFileName(entry.Name));
			}
			else
			{
				text2 = entry.Name;
			}
			text = Path.Combine(targetDirectory, text2);
			path = Path.GetDirectoryName(Path.GetFullPath(text));
			flag = flag && text2.Length > 0;
		}
		if (flag && !Directory.Exists(path) && (!entry.IsDirectory || CreateEmptyDirectories))
		{
			try
			{
				Directory.CreateDirectory(path);
			}
			catch
			{
				flag = false;
			}
		}
		if (flag && entry.IsFile)
		{
			ExtractFileEntry(entry, text);
		}
	}
}
