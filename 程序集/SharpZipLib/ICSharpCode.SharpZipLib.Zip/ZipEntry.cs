using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Zip;

public class ZipEntry : ICloneable
{
	private static int KNOWN_SIZE = 1;

	private static int KNOWN_CSIZE = 2;

	private static int KNOWN_CRC = 4;

	private static int KNOWN_TIME = 8;

	private static int KNOWN_EXTERN_ATTRIBUTES = 16;

	private ushort known = 0;

	private int externalFileAttributes = -1;

	private ushort versionMadeBy;

	private string name;

	private ulong size;

	private ulong compressedSize;

	private ushort versionToExtract;

	private uint crc;

	private uint dosTime;

	private CompressionMethod method = CompressionMethod.Deflated;

	private byte[] extra = null;

	private string comment = null;

	private int flags;

	private int zipFileIndex = -1;

	private int offset;

	public bool IsCrypted
	{
		get
		{
			return (flags & 1) != 0;
		}
		set
		{
			if (value)
			{
				flags |= 1;
			}
			else
			{
				flags &= -2;
			}
		}
	}

	public int Flags
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	public int ZipFileIndex
	{
		get
		{
			return zipFileIndex;
		}
		set
		{
			zipFileIndex = value;
		}
	}

	public int Offset
	{
		get
		{
			return offset;
		}
		set
		{
			if ((value & -4294967296L) != 0)
			{
				throw new ArgumentOutOfRangeException("Offset");
			}
			offset = value;
		}
	}

	public int ExternalFileAttributes
	{
		get
		{
			if ((known & KNOWN_EXTERN_ATTRIBUTES) == 0)
			{
				return -1;
			}
			return externalFileAttributes;
		}
		set
		{
			externalFileAttributes = value;
			known |= (ushort)KNOWN_EXTERN_ATTRIBUTES;
		}
	}

	public int VersionMadeBy => versionMadeBy & 0xFF;

	public int HostSystem => (versionMadeBy >> 8) & 0xFF;

	public int Version
	{
		get
		{
			if (versionToExtract != 0)
			{
				return versionToExtract;
			}
			int result = 10;
			if (CompressionMethod.Deflated == method)
			{
				result = 20;
			}
			else if (IsDirectory)
			{
				result = 20;
			}
			else if (IsCrypted)
			{
				result = 20;
			}
			else if ((known & KNOWN_EXTERN_ATTRIBUTES) != 0 && ((uint)externalFileAttributes & 8u) != 0)
			{
				result = 11;
			}
			return result;
		}
	}

	public bool RequiresZip64 => size > uint.MaxValue || compressedSize > uint.MaxValue;

	public long DosTime
	{
		get
		{
			if ((known & KNOWN_TIME) == 0)
			{
				return 0L;
			}
			return dosTime;
		}
		set
		{
			dosTime = (uint)value;
			known |= (ushort)KNOWN_TIME;
		}
	}

	public DateTime DateTime
	{
		get
		{
			if (dosTime == 0)
			{
				return DateTime.Now;
			}
			uint second = 2 * (dosTime & 0x1F);
			uint minute = (dosTime >> 5) & 0x3Fu;
			uint hour = (dosTime >> 11) & 0x1Fu;
			uint day = (dosTime >> 16) & 0x1Fu;
			uint month = (dosTime >> 21) & 0xFu;
			uint year = ((dosTime >> 25) & 0x7F) + 1980;
			return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
		}
		set
		{
			DosTime = (uint)((((value.Year - 1980) & 0x7F) << 25) | (value.Month << 21) | (value.Day << 16) | (value.Hour << 11) | (value.Minute << 5) | (int)((uint)value.Second >> 1));
		}
	}

	public string Name => name;

	public long Size
	{
		get
		{
			return ((known & KNOWN_SIZE) != 0) ? ((long)size) : (-1L);
		}
		set
		{
			if ((value & -4294967296L) != 0)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			size = (ulong)value;
			known |= (ushort)KNOWN_SIZE;
		}
	}

	public long CompressedSize
	{
		get
		{
			return ((known & KNOWN_CSIZE) != 0) ? ((long)compressedSize) : (-1L);
		}
		set
		{
			if ((value & -4294967296L) != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			compressedSize = (ulong)value;
			known |= (ushort)KNOWN_CSIZE;
		}
	}

	public long Crc
	{
		get
		{
			return ((known & KNOWN_CRC) != 0) ? ((long)crc & 0xFFFFFFFFL) : (-1);
		}
		set
		{
			if ((crc & -4294967296L) != 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			crc = (uint)value;
			known |= (ushort)KNOWN_CRC;
		}
	}

	public CompressionMethod CompressionMethod
	{
		get
		{
			return method;
		}
		set
		{
			method = value;
		}
	}

	public byte[] ExtraData
	{
		get
		{
			return extra;
		}
		set
		{
			if (value == null)
			{
				extra = null;
				return;
			}
			if (value.Length > 65535)
			{
				throw new ArgumentOutOfRangeException();
			}
			extra = new byte[value.Length];
			Array.Copy(value, 0, extra, 0, value.Length);
			try
			{
				int num2;
				for (int i = 0; i < extra.Length; i += num2)
				{
					int num = (extra[i++] & 0xFF) | ((extra[i++] & 0xFF) << 8);
					num2 = (extra[i++] & 0xFF) | ((extra[i++] & 0xFF) << 8);
					if (num2 < 0 || i + num2 > extra.Length)
					{
						break;
					}
					switch (num)
					{
					case 21589:
					{
						int num3 = extra[i];
						if (((uint)num3 & (true ? 1u : 0u)) != 0 && num2 >= 5)
						{
							int seconds = (extra[i + 1] & 0xFF) | ((extra[i + 2] & 0xFF) << 8) | ((extra[i + 3] & 0xFF) << 16) | ((extra[i + 4] & 0xFF) << 24);
							DateTime = (new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
							known |= (ushort)KNOWN_TIME;
						}
						break;
					}
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	public string Comment
	{
		get
		{
			return comment;
		}
		set
		{
			if (value != null && value.Length > 65535)
			{
				throw new ArgumentOutOfRangeException();
			}
			comment = value;
		}
	}

	public bool IsDirectory
	{
		get
		{
			int length = name.Length;
			bool flag = length > 0 && name[length - 1] == '/';
			if (!flag && (known & KNOWN_EXTERN_ATTRIBUTES) != 0 && HostSystem == 0 && ((uint)ExternalFileAttributes & 0x10u) != 0)
			{
				flag = true;
			}
			return flag;
		}
	}

	public bool IsFile
	{
		get
		{
			bool flag = !IsDirectory;
			if (flag && (known & KNOWN_EXTERN_ATTRIBUTES) != 0 && HostSystem == 0 && ((uint)ExternalFileAttributes & 8u) != 0)
			{
				flag = false;
			}
			return flag;
		}
	}

	public ZipEntry(string name)
		: this(name, 0, 20)
	{
	}

	internal ZipEntry(string name, int versionRequiredToExtract)
		: this(name, versionRequiredToExtract, 20)
	{
	}

	internal ZipEntry(string name, int versionRequiredToExtract, int madeByInfo)
	{
		if (name == null)
		{
			throw new ArgumentNullException("ZipEntry name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("ZipEntry name is empty");
		}
		if (versionRequiredToExtract != 0 && versionRequiredToExtract < 10)
		{
			throw new ArgumentOutOfRangeException("versionRequiredToExtract");
		}
		DateTime = DateTime.Now;
		this.name = name;
		versionMadeBy = (ushort)madeByInfo;
		versionToExtract = (ushort)versionRequiredToExtract;
	}

	public ZipEntry(ZipEntry e)
	{
		known = e.known;
		name = e.name;
		size = e.size;
		compressedSize = e.compressedSize;
		crc = e.crc;
		dosTime = e.dosTime;
		method = e.method;
		ExtraData = e.ExtraData;
		comment = e.comment;
		versionToExtract = e.versionToExtract;
		versionMadeBy = e.versionMadeBy;
		externalFileAttributes = e.externalFileAttributes;
		flags = e.flags;
		zipFileIndex = -1;
		offset = 0;
	}

	public static string CleanName(string name, bool relativePath)
	{
		if (name == null)
		{
			return "";
		}
		if (Path.IsPathRooted(name))
		{
			name = name.Substring(Path.GetPathRoot(name).Length);
		}
		name = name.Replace("\\", "/");
		if (relativePath)
		{
			if (name.Length > 0 && (name[0] == Path.AltDirectorySeparatorChar || name[0] == Path.DirectorySeparatorChar))
			{
				name = name.Remove(0, 1);
			}
		}
		else if (name.Length > 0 && name[0] != Path.AltDirectorySeparatorChar && name[0] != Path.DirectorySeparatorChar)
		{
			name = name.Insert(0, "/");
		}
		return name;
	}

	public static string CleanName(string name)
	{
		return CleanName(name, relativePath: true);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		return name;
	}
}
