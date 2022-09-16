using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarEntry : ICloneable
{
	private string file;

	private TarHeader header;

	public TarHeader TarHeader => header;

	public string Name
	{
		get
		{
			return header.Name;
		}
		set
		{
			header.Name = value;
		}
	}

	public int UserId
	{
		get
		{
			return header.UserId;
		}
		set
		{
			header.UserId = value;
		}
	}

	public int GroupId
	{
		get
		{
			return header.GroupId;
		}
		set
		{
			header.GroupId = value;
		}
	}

	public string UserName
	{
		get
		{
			return header.UserName;
		}
		set
		{
			header.UserName = value;
		}
	}

	public string GroupName
	{
		get
		{
			return header.GroupName;
		}
		set
		{
			header.GroupName = value;
		}
	}

	public DateTime ModTime
	{
		get
		{
			return header.ModTime;
		}
		set
		{
			header.ModTime = value;
		}
	}

	public string File => file;

	public long Size
	{
		get
		{
			return header.Size;
		}
		set
		{
			header.Size = value;
		}
	}

	public bool IsDirectory
	{
		get
		{
			if (file != null)
			{
				return Directory.Exists(file);
			}
			if (header != null && (header.TypeFlag == 53 || Name.EndsWith("/")))
			{
				return true;
			}
			return false;
		}
	}

	private TarEntry()
	{
	}

	public TarEntry(byte[] headerBuf)
	{
		Initialize();
		header.ParseBuffer(headerBuf);
	}

	public TarEntry(TarHeader header)
	{
		file = null;
		this.header = header;
	}

	public object Clone()
	{
		TarEntry tarEntry = new TarEntry();
		tarEntry.file = file;
		tarEntry.header = (TarHeader)header.Clone();
		tarEntry.Name = Name;
		return tarEntry;
	}

	public static TarEntry CreateTarEntry(string name)
	{
		TarEntry tarEntry = new TarEntry();
		tarEntry.Initialize();
		tarEntry.NameTarHeader(tarEntry.header, name);
		return tarEntry;
	}

	public static TarEntry CreateEntryFromFile(string fileName)
	{
		TarEntry tarEntry = new TarEntry();
		tarEntry.Initialize();
		tarEntry.GetFileTarHeader(tarEntry.header, fileName);
		return tarEntry;
	}

	private void Initialize()
	{
		file = null;
		header = new TarHeader();
	}

	public override bool Equals(object it)
	{
		if (!(it is TarEntry))
		{
			return false;
		}
		return Name.Equals(((TarEntry)it).Name);
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public bool IsDescendent(TarEntry desc)
	{
		return desc.Name.StartsWith(Name);
	}

	public void SetIds(int userId, int groupId)
	{
		UserId = userId;
		GroupId = groupId;
	}

	public void SetNames(string userName, string groupName)
	{
		UserName = userName;
		GroupName = groupName;
	}

	public void AdjustEntryName(byte[] outbuf, string newName)
	{
		int offset = 0;
		TarHeader.GetNameBytes(newName, outbuf, offset, TarHeader.NAMELEN);
	}

	public void GetFileTarHeader(TarHeader hdr, string file)
	{
		this.file = file;
		string text = file;
		if (text.IndexOf(Environment.CurrentDirectory) == 0)
		{
			text = text.Substring(Environment.CurrentDirectory.Length);
		}
		text = text.Replace(Path.DirectorySeparatorChar, '/');
		while (text.StartsWith("/"))
		{
			text = text.Substring(1);
		}
		hdr.LinkName = string.Empty;
		hdr.Name = text;
		if (Directory.Exists(file))
		{
			hdr.Mode = 1003;
			hdr.TypeFlag = 53;
			if (hdr.Name.Length == 0 || hdr.Name[hdr.Name.Length - 1] != '/')
			{
				hdr.Name += "/";
			}
			hdr.Size = 0L;
		}
		else
		{
			hdr.Mode = 33216;
			hdr.TypeFlag = 48;
			hdr.Size = new FileInfo(file.Replace('/', Path.DirectorySeparatorChar)).Length;
		}
		hdr.ModTime = System.IO.File.GetLastWriteTime(file.Replace('/', Path.DirectorySeparatorChar)).ToUniversalTime();
		hdr.DevMajor = 0;
		hdr.DevMinor = 0;
	}

	public TarEntry[] GetDirectoryEntries()
	{
		if (file == null || !Directory.Exists(file))
		{
			return new TarEntry[0];
		}
		string[] fileSystemEntries = Directory.GetFileSystemEntries(file);
		TarEntry[] array = new TarEntry[fileSystemEntries.Length];
		for (int i = 0; i < fileSystemEntries.Length; i++)
		{
			array[i] = CreateEntryFromFile(fileSystemEntries[i]);
		}
		return array;
	}

	public void WriteEntryHeader(byte[] outbuf)
	{
		header.WriteHeader(outbuf);
	}

	public void NameTarHeader(TarHeader hdr, string name)
	{
		bool flag = name.EndsWith("/");
		hdr.Name = name;
		hdr.Mode = (flag ? 1003 : 33216);
		hdr.UserId = 0;
		hdr.GroupId = 0;
		hdr.Size = 0L;
		hdr.ModTime = DateTime.UtcNow;
		hdr.TypeFlag = (byte)(flag ? 53 : 48);
		hdr.LinkName = string.Empty;
		hdr.UserName = string.Empty;
		hdr.GroupName = string.Empty;
		hdr.DevMajor = 0;
		hdr.DevMinor = 0;
	}
}
