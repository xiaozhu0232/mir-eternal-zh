using System;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarHeader : ICloneable
{
	public const int CHKSUMOFS = 148;

	public const byte LF_OLDNORM = 0;

	public const byte LF_NORMAL = 48;

	public const byte LF_LINK = 49;

	public const byte LF_SYMLINK = 50;

	public const byte LF_CHR = 51;

	public const byte LF_BLK = 52;

	public const byte LF_DIR = 53;

	public const byte LF_FIFO = 54;

	public const byte LF_CONTIG = 55;

	public const byte LF_GHDR = 103;

	public const byte LF_ACL = 65;

	public const byte LF_GNU_DUMPDIR = 68;

	public const byte LF_EXTATTR = 69;

	public const byte LF_META = 73;

	public const byte LF_GNU_LONGLINK = 75;

	public const byte LF_GNU_LONGNAME = 76;

	public const byte LF_GNU_MULTIVOL = 77;

	public const byte LF_GNU_NAMES = 78;

	public const byte LF_GNU_SPARSE = 83;

	public const byte LF_GNU_VOLHDR = 86;

	public static readonly int NAMELEN = 100;

	public static readonly int MODELEN = 8;

	public static readonly int UIDLEN = 8;

	public static readonly int GIDLEN = 8;

	public static readonly int CHKSUMLEN = 8;

	public static readonly int SIZELEN = 12;

	public static readonly int MAGICLEN = 6;

	public static readonly int VERSIONLEN = 2;

	public static readonly int MODTIMELEN = 12;

	public static readonly int UNAMELEN = 32;

	public static readonly int GNAMELEN = 32;

	public static readonly int DEVLEN = 8;

	public static readonly byte LF_XHDR = 120;

	public static readonly string TMAGIC = "ustar ";

	public static readonly string GNU_TMAGIC = "ustar  ";

	private string name;

	private int mode;

	private int userId;

	private int groupId;

	private long size;

	private DateTime modTime;

	private int checksum;

	private bool isChecksumValid;

	private byte typeFlag;

	private string linkName;

	private string magic;

	private string version;

	private string userName;

	private string groupName;

	private int devMajor;

	private int devMinor;

	internal static int userIdAsSet = 0;

	internal static int groupIdAsSet = 0;

	internal static string userNameAsSet = null;

	internal static string groupNameAsSet = "None";

	internal static int defaultUserId = 0;

	internal static int defaultGroupId = 0;

	internal static string defaultGroupName = "None";

	internal static string defaultUser = null;

	private static readonly long timeConversionFactor = 10000000L;

	private static readonly DateTime dateTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			name = value;
		}
	}

	public int Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
		}
	}

	public int UserId
	{
		get
		{
			return userId;
		}
		set
		{
			userId = value;
		}
	}

	public int GroupId
	{
		get
		{
			return groupId;
		}
		set
		{
			groupId = value;
		}
	}

	public long Size
	{
		get
		{
			return size;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			size = value;
		}
	}

	public DateTime ModTime
	{
		get
		{
			return modTime;
		}
		set
		{
			if (value < dateTime1970)
			{
				throw new ArgumentOutOfRangeException();
			}
			modTime = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
		}
	}

	public int Checksum => checksum;

	public bool IsChecksumValid => isChecksumValid;

	public byte TypeFlag
	{
		get
		{
			return typeFlag;
		}
		set
		{
			typeFlag = value;
		}
	}

	public string LinkName
	{
		get
		{
			return linkName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			linkName = value;
		}
	}

	public string Magic
	{
		get
		{
			return magic;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			magic = value;
		}
	}

	public string Version
	{
		get
		{
			return version;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			version = value;
		}
	}

	public string UserName
	{
		get
		{
			return userName;
		}
		set
		{
			if (value != null)
			{
				userName = value.Substring(0, Math.Min(UNAMELEN, value.Length));
				return;
			}
			string text = Environment.UserName;
			if (text.Length > UNAMELEN)
			{
				text = text.Substring(0, UNAMELEN);
			}
			userName = text;
		}
	}

	public string GroupName
	{
		get
		{
			return groupName;
		}
		set
		{
			if (value == null)
			{
				groupName = "None";
			}
			else
			{
				groupName = value;
			}
		}
	}

	public int DevMajor
	{
		get
		{
			return devMajor;
		}
		set
		{
			devMajor = value;
		}
	}

	public int DevMinor
	{
		get
		{
			return devMinor;
		}
		set
		{
			devMinor = value;
		}
	}

	public TarHeader()
	{
		Magic = TMAGIC;
		Version = " ";
		Name = "";
		LinkName = "";
		UserId = defaultUserId;
		GroupId = defaultGroupId;
		UserName = defaultUser;
		GroupName = defaultGroupName;
		Size = 0L;
	}

	internal static void RestoreSetValues()
	{
		defaultUserId = userIdAsSet;
		defaultUser = userNameAsSet;
		defaultGroupId = groupIdAsSet;
		defaultGroupName = groupNameAsSet;
	}

	public static void SetValueDefaults(int userId, string userName, int groupId, string groupName)
	{
		defaultUserId = (userIdAsSet = userId);
		defaultUser = (userNameAsSet = userName);
		defaultGroupId = (groupIdAsSet = groupId);
		defaultGroupName = (groupNameAsSet = groupName);
	}

	internal static void SetActiveDefaults(int userId, string userName, int groupId, string groupName)
	{
		defaultUserId = userId;
		defaultUser = userName;
		defaultGroupId = groupId;
		defaultGroupName = groupName;
	}

	public static void ResetValueDefaults()
	{
		defaultUserId = 0;
		defaultGroupId = 0;
		defaultGroupName = "None";
		defaultUser = null;
	}

	public object Clone()
	{
		TarHeader tarHeader = new TarHeader();
		tarHeader.Name = Name;
		tarHeader.Mode = Mode;
		tarHeader.UserId = UserId;
		tarHeader.GroupId = GroupId;
		tarHeader.Size = Size;
		tarHeader.ModTime = ModTime;
		tarHeader.TypeFlag = TypeFlag;
		tarHeader.LinkName = LinkName;
		tarHeader.Magic = Magic;
		tarHeader.Version = Version;
		tarHeader.UserName = UserName;
		tarHeader.GroupName = GroupName;
		tarHeader.DevMajor = DevMajor;
		tarHeader.DevMinor = DevMinor;
		return tarHeader;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TarHeader)
		{
			TarHeader tarHeader = obj as TarHeader;
			return name == tarHeader.name && mode == tarHeader.mode && UserId == tarHeader.UserId && GroupId == tarHeader.GroupId && Size == tarHeader.Size && ModTime == tarHeader.ModTime && Checksum == tarHeader.Checksum && TypeFlag == tarHeader.TypeFlag && LinkName == tarHeader.LinkName && Magic == tarHeader.Magic && Version == tarHeader.Version && UserName == tarHeader.UserName && GroupName == tarHeader.GroupName && DevMajor == tarHeader.DevMajor && DevMinor == tarHeader.DevMinor;
		}
		return false;
	}

	[Obsolete]
	public string GetName()
	{
		return name.ToString();
	}

	public static long ParseOctal(byte[] header, int offset, int length)
	{
		long num = 0L;
		bool flag = true;
		int num2 = offset + length;
		for (int i = offset; i < num2 && header[i] != 0; i++)
		{
			if (header[i] == 32 || header[i] == 48)
			{
				if (flag)
				{
					continue;
				}
				if (header[i] == 32)
				{
					break;
				}
			}
			flag = false;
			num = (num << 3) + (header[i] - 48);
		}
		return num;
	}

	public static StringBuilder ParseName(byte[] header, int offset, int length)
	{
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = offset; i < offset + length && header[i] != 0; i++)
		{
			stringBuilder.Append((char)header[i]);
		}
		return stringBuilder;
	}

	public static int GetNameBytes(StringBuilder name, int nameOffset, byte[] buf, int bufferOffset, int length)
	{
		return GetNameBytes(name.ToString(), nameOffset, buf, bufferOffset, length);
	}

	public static int GetNameBytes(string name, int nameOffset, byte[] buf, int bufferOffset, int length)
	{
		int i;
		for (i = 0; i < length - 1 && nameOffset + i < name.Length; i++)
		{
			buf[bufferOffset + i] = (byte)name[nameOffset + i];
		}
		for (; i < length; i++)
		{
			buf[bufferOffset + i] = 0;
		}
		return bufferOffset + length;
	}

	public static int GetNameBytes(StringBuilder name, byte[] buf, int offset, int length)
	{
		return GetNameBytes(name.ToString(), 0, buf, offset, length);
	}

	public static int GetNameBytes(string name, byte[] buf, int offset, int length)
	{
		return GetNameBytes(name, 0, buf, offset, length);
	}

	public static int GetAsciiBytes(string toAdd, int nameOffset, byte[] buffer, int bufferOffset, int length)
	{
		for (int i = 0; i < length && nameOffset + i < toAdd.Length; i++)
		{
			buffer[bufferOffset + i] = (byte)toAdd[nameOffset + i];
		}
		return bufferOffset + length;
	}

	public static int GetOctalBytes(long val, byte[] buf, int offset, int length)
	{
		int num = length - 1;
		buf[offset + num] = 0;
		num--;
		if (val > 0)
		{
			long num2 = val;
			while (num >= 0 && num2 > 0)
			{
				buf[offset + num] = (byte)(48 + (byte)(num2 & 7));
				num2 >>= 3;
				num--;
			}
		}
		while (num >= 0)
		{
			buf[offset + num] = 48;
			num--;
		}
		return offset + length;
	}

	public static int GetLongOctalBytes(long val, byte[] buf, int offset, int length)
	{
		return GetOctalBytes(val, buf, offset, length);
	}

	private static int GetCheckSumOctalBytes(long val, byte[] buf, int offset, int length)
	{
		GetOctalBytes(val, buf, offset, length - 1);
		return offset + length;
	}

	private static int ComputeCheckSum(byte[] buf)
	{
		int num = 0;
		for (int i = 0; i < buf.Length; i++)
		{
			num += buf[i];
		}
		return num;
	}

	private static int MakeCheckSum(byte[] buf)
	{
		int num = 0;
		for (int i = 0; i < 148; i++)
		{
			num += buf[i];
		}
		for (int i = 0; i < CHKSUMLEN; i++)
		{
			num += 32;
		}
		for (int i = 148 + CHKSUMLEN; i < buf.Length; i++)
		{
			num += buf[i];
		}
		return num;
	}

	private static int GetCTime(DateTime dateTime)
	{
		return (int)((dateTime.Ticks - dateTime1970.Ticks) / timeConversionFactor);
	}

	private static DateTime GetDateTimeFromCTime(long ticks)
	{
		try
		{
			return new DateTime(dateTime1970.Ticks + ticks * timeConversionFactor);
		}
		catch
		{
			return dateTime1970;
		}
	}

	public void ParseBuffer(byte[] header)
	{
		int num = 0;
		name = ParseName(header, num, NAMELEN).ToString();
		num += NAMELEN;
		mode = (int)ParseOctal(header, num, MODELEN);
		num += MODELEN;
		UserId = (int)ParseOctal(header, num, UIDLEN);
		num += UIDLEN;
		GroupId = (int)ParseOctal(header, num, GIDLEN);
		num += GIDLEN;
		Size = ParseOctal(header, num, SIZELEN);
		num += SIZELEN;
		ModTime = GetDateTimeFromCTime(ParseOctal(header, num, MODTIMELEN));
		num += MODTIMELEN;
		checksum = (int)ParseOctal(header, num, CHKSUMLEN);
		num += CHKSUMLEN;
		TypeFlag = header[num++];
		LinkName = ParseName(header, num, NAMELEN).ToString();
		num += NAMELEN;
		Magic = ParseName(header, num, MAGICLEN).ToString();
		num += MAGICLEN;
		Version = ParseName(header, num, VERSIONLEN).ToString();
		num += VERSIONLEN;
		UserName = ParseName(header, num, UNAMELEN).ToString();
		num += UNAMELEN;
		GroupName = ParseName(header, num, GNAMELEN).ToString();
		num += GNAMELEN;
		DevMajor = (int)ParseOctal(header, num, DEVLEN);
		num += DEVLEN;
		DevMinor = (int)ParseOctal(header, num, DEVLEN);
		isChecksumValid = Checksum == MakeCheckSum(header);
	}

	public void WriteHeader(byte[] outbuf)
	{
		int offset = 0;
		offset = GetNameBytes(Name, outbuf, offset, NAMELEN);
		offset = GetOctalBytes(mode, outbuf, offset, MODELEN);
		offset = GetOctalBytes(UserId, outbuf, offset, UIDLEN);
		offset = GetOctalBytes(GroupId, outbuf, offset, GIDLEN);
		long val = Size;
		offset = GetLongOctalBytes(val, outbuf, offset, SIZELEN);
		offset = GetLongOctalBytes(GetCTime(ModTime), outbuf, offset, MODTIMELEN);
		int offset2 = offset;
		for (int i = 0; i < CHKSUMLEN; i++)
		{
			outbuf[offset++] = 32;
		}
		outbuf[offset++] = TypeFlag;
		offset = GetNameBytes(LinkName, outbuf, offset, NAMELEN);
		offset = GetAsciiBytes(Magic, 0, outbuf, offset, MAGICLEN);
		offset = GetNameBytes(Version, outbuf, offset, VERSIONLEN);
		offset = GetNameBytes(UserName, outbuf, offset, UNAMELEN);
		offset = GetNameBytes(GroupName, outbuf, offset, GNAMELEN);
		if (TypeFlag == 51 || TypeFlag == 52)
		{
			offset = GetOctalBytes(DevMajor, outbuf, offset, DEVLEN);
			offset = GetOctalBytes(DevMinor, outbuf, offset, DEVLEN);
		}
		while (offset < outbuf.Length)
		{
			outbuf[offset++] = 0;
		}
		checksum = ComputeCheckSum(outbuf);
		GetCheckSumOctalBytes(checksum, outbuf, offset2, CHKSUMLEN);
		isChecksumValid = true;
	}
}
