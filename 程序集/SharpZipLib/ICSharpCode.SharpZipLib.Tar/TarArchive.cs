using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarArchive
{
	private bool keepOldFiles;

	private bool asciiTranslate;

	private int userId;

	private string userName;

	private int groupId;

	private string groupName;

	private string rootPath;

	private string pathPrefix;

	private int recordSize;

	private byte[] recordBuf;

	private TarInputStream tarIn;

	private TarOutputStream tarOut;

	private bool applyUserInfoOverrides = false;

	public string PathPrefix
	{
		get
		{
			return pathPrefix;
		}
		set
		{
			pathPrefix = value;
		}
	}

	public string RootPath
	{
		get
		{
			return rootPath;
		}
		set
		{
			rootPath = value;
		}
	}

	public bool ApplyUserInfoOverrides
	{
		get
		{
			return applyUserInfoOverrides;
		}
		set
		{
			applyUserInfoOverrides = value;
		}
	}

	public int UserId => userId;

	public string UserName => userName;

	public int GroupId => groupId;

	public string GroupName => groupName;

	public int RecordSize
	{
		get
		{
			if (tarIn != null)
			{
				return tarIn.GetRecordSize();
			}
			if (tarOut != null)
			{
				return tarOut.GetRecordSize();
			}
			return 10240;
		}
	}

	public event ProgressMessageHandler ProgressMessageEvent;

	protected virtual void OnProgressMessageEvent(TarEntry entry, string message)
	{
		if (this.ProgressMessageEvent != null)
		{
			this.ProgressMessageEvent(this, entry, message);
		}
	}

	protected TarArchive()
	{
	}

	public static TarArchive CreateInputTarArchive(Stream inputStream)
	{
		return CreateInputTarArchive(inputStream, 20);
	}

	public static TarArchive CreateInputTarArchive(Stream inputStream, int blockFactor)
	{
		TarArchive tarArchive = new TarArchive();
		tarArchive.tarIn = new TarInputStream(inputStream, blockFactor);
		tarArchive.Initialize(blockFactor * 512);
		return tarArchive;
	}

	public static TarArchive CreateOutputTarArchive(Stream outputStream)
	{
		return CreateOutputTarArchive(outputStream, 20);
	}

	public static TarArchive CreateOutputTarArchive(Stream outputStream, int blockFactor)
	{
		TarArchive tarArchive = new TarArchive();
		tarArchive.tarOut = new TarOutputStream(outputStream, blockFactor);
		tarArchive.Initialize(blockFactor * 512);
		return tarArchive;
	}

	private void Initialize(int recordSize)
	{
		this.recordSize = recordSize;
		rootPath = null;
		pathPrefix = null;
		userId = 0;
		userName = string.Empty;
		groupId = 0;
		groupName = string.Empty;
		keepOldFiles = false;
		recordBuf = new byte[RecordSize];
	}

	public void SetKeepOldFiles(bool keepOldFiles)
	{
		this.keepOldFiles = keepOldFiles;
	}

	public void SetAsciiTranslation(bool asciiTranslate)
	{
		this.asciiTranslate = asciiTranslate;
	}

	public void SetUserInfo(int userId, string userName, int groupId, string groupName)
	{
		this.userId = userId;
		this.userName = userName;
		this.groupId = groupId;
		this.groupName = groupName;
		applyUserInfoOverrides = true;
	}

	public void CloseArchive()
	{
		if (tarIn != null)
		{
			tarIn.Close();
		}
		else if (tarOut != null)
		{
			tarOut.Flush();
			tarOut.Close();
		}
	}

	public void ListContents()
	{
		while (true)
		{
			bool flag = true;
			TarEntry nextEntry = tarIn.GetNextEntry();
			if (nextEntry == null)
			{
				break;
			}
			OnProgressMessageEvent(nextEntry, null);
		}
	}

	public void ExtractContents(string destDir)
	{
		while (true)
		{
			bool flag = true;
			TarEntry nextEntry = tarIn.GetNextEntry();
			if (nextEntry == null)
			{
				break;
			}
			ExtractEntry(destDir, nextEntry);
		}
	}

	private void EnsureDirectoryExists(string directoryName)
	{
		if (!Directory.Exists(directoryName))
		{
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch (Exception ex)
			{
				throw new TarException("Exception creating directory '" + directoryName + "', " + ex.Message);
			}
		}
	}

	private bool IsBinary(string filename)
	{
		using (FileStream fileStream = File.OpenRead(filename))
		{
			int num = Math.Min(4096, (int)fileStream.Length);
			byte[] array = new byte[num];
			int num2 = fileStream.Read(array, 0, num);
			for (int i = 0; i < num2; i++)
			{
				byte b = array[i];
				if (b < 8 || (b > 13 && b < 32) || b == byte.MaxValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ExtractEntry(string destDir, TarEntry entry)
	{
		OnProgressMessageEvent(entry, null);
		string text = entry.Name;
		if (Path.IsPathRooted(text))
		{
			text = text.Substring(Path.GetPathRoot(text).Length);
		}
		text = text.Replace('/', Path.DirectorySeparatorChar);
		string text2 = Path.Combine(destDir, text);
		if (entry.IsDirectory)
		{
			EnsureDirectoryExists(text2);
			return;
		}
		string directoryName = Path.GetDirectoryName(text2);
		EnsureDirectoryExists(directoryName);
		bool flag = true;
		FileInfo fileInfo = new FileInfo(text2);
		if (fileInfo.Exists)
		{
			if (keepOldFiles)
			{
				OnProgressMessageEvent(entry, "Destination file already exists");
				flag = false;
			}
			else if ((fileInfo.Attributes & FileAttributes.ReadOnly) != 0)
			{
				OnProgressMessageEvent(entry, "Destination file already exists, and is read-only");
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = false;
		Stream stream = File.Create(text2);
		if (asciiTranslate)
		{
			flag2 = !IsBinary(text2);
		}
		StreamWriter streamWriter = null;
		if (flag2)
		{
			streamWriter = new StreamWriter(stream);
		}
		byte[] array = new byte[32768];
		while (true)
		{
			bool flag3 = true;
			int num = tarIn.Read(array, 0, array.Length);
			if (num <= 0)
			{
				break;
			}
			if (flag2)
			{
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					if (array[i] == 10)
					{
						string @string = Encoding.ASCII.GetString(array, num2, i - num2);
						streamWriter.WriteLine(@string);
						num2 = i + 1;
					}
				}
			}
			else
			{
				stream.Write(array, 0, num);
			}
		}
		if (flag2)
		{
			streamWriter.Close();
		}
		else
		{
			stream.Close();
		}
	}

	public void WriteEntry(TarEntry sourceEntry, bool recurse)
	{
		try
		{
			if (recurse)
			{
				TarHeader.SetValueDefaults(sourceEntry.UserId, sourceEntry.UserName, sourceEntry.GroupId, sourceEntry.GroupName);
			}
			InternalWriteEntry(sourceEntry, recurse);
		}
		finally
		{
			if (recurse)
			{
				TarHeader.RestoreSetValues();
			}
		}
	}

	private void InternalWriteEntry(TarEntry sourceEntry, bool recurse)
	{
		bool flag = false;
		string text = null;
		string text2 = sourceEntry.File;
		TarEntry tarEntry = (TarEntry)sourceEntry.Clone();
		if (applyUserInfoOverrides)
		{
			tarEntry.GroupId = groupId;
			tarEntry.GroupName = groupName;
			tarEntry.UserId = userId;
			tarEntry.UserName = userName;
		}
		OnProgressMessageEvent(tarEntry, null);
		if (asciiTranslate && !tarEntry.IsDirectory && !IsBinary(text2))
		{
			text = Path.GetTempFileName();
			StreamReader streamReader = File.OpenText(text2);
			Stream stream = File.Create(text);
			while (true)
			{
				bool flag2 = true;
				string text3 = streamReader.ReadLine();
				if (text3 == null)
				{
					break;
				}
				byte[] bytes = Encoding.ASCII.GetBytes(text3);
				stream.Write(bytes, 0, bytes.Length);
				stream.WriteByte(10);
			}
			streamReader.Close();
			stream.Flush();
			stream.Close();
			tarEntry.Size = new FileInfo(text).Length;
			text2 = text;
		}
		string text4 = null;
		if (rootPath != null && tarEntry.Name.StartsWith(rootPath))
		{
			text4 = tarEntry.Name.Substring(rootPath.Length + 1);
		}
		if (pathPrefix != null)
		{
			text4 = ((text4 == null) ? (pathPrefix + "/" + tarEntry.Name) : (pathPrefix + "/" + text4));
		}
		if (text4 != null)
		{
			tarEntry.Name = text4;
		}
		tarOut.PutNextEntry(tarEntry);
		if (tarEntry.IsDirectory)
		{
			if (recurse)
			{
				TarEntry[] directoryEntries = tarEntry.GetDirectoryEntries();
				for (int i = 0; i < directoryEntries.Length; i++)
				{
					InternalWriteEntry(directoryEntries[i], recurse);
				}
			}
			return;
		}
		Stream stream2 = File.OpenRead(text2);
		int num = 0;
		byte[] array = new byte[32768];
		while (true)
		{
			bool flag2 = true;
			int num2 = stream2.Read(array, 0, array.Length);
			if (num2 <= 0)
			{
				break;
			}
			tarOut.Write(array, 0, num2);
			num += num2;
		}
		stream2.Close();
		if (text != null && text.Length > 0)
		{
			File.Delete(text);
		}
		tarOut.CloseEntry();
	}
}
