using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public sealed class ProtocolVersion
{
	public static readonly ProtocolVersion SSLv3 = new ProtocolVersion(768, "SSL 3.0");

	public static readonly ProtocolVersion TLSv10 = new ProtocolVersion(769, "TLS 1.0");

	public static readonly ProtocolVersion TLSv11 = new ProtocolVersion(770, "TLS 1.1");

	public static readonly ProtocolVersion TLSv12 = new ProtocolVersion(771, "TLS 1.2");

	public static readonly ProtocolVersion DTLSv10 = new ProtocolVersion(65279, "DTLS 1.0");

	public static readonly ProtocolVersion DTLSv12 = new ProtocolVersion(65277, "DTLS 1.2");

	private readonly int version;

	private readonly string name;

	public int FullVersion => version;

	public int MajorVersion => version >> 8;

	public int MinorVersion => version & 0xFF;

	public bool IsDtls => MajorVersion == 254;

	public bool IsSsl => this == SSLv3;

	public bool IsTls => MajorVersion == 3;

	private ProtocolVersion(int v, string name)
	{
		version = v & 0xFFFF;
		this.name = name;
	}

	public ProtocolVersion GetEquivalentTLSVersion()
	{
		if (!IsDtls)
		{
			return this;
		}
		if (this == DTLSv10)
		{
			return TLSv11;
		}
		return TLSv12;
	}

	public bool IsEqualOrEarlierVersionOf(ProtocolVersion version)
	{
		if (MajorVersion != version.MajorVersion)
		{
			return false;
		}
		int num = version.MinorVersion - MinorVersion;
		if (!IsDtls)
		{
			return num >= 0;
		}
		return num <= 0;
	}

	public bool IsLaterVersionOf(ProtocolVersion version)
	{
		if (MajorVersion != version.MajorVersion)
		{
			return false;
		}
		int num = version.MinorVersion - MinorVersion;
		if (!IsDtls)
		{
			return num < 0;
		}
		return num > 0;
	}

	public override bool Equals(object other)
	{
		if (this != other)
		{
			if (other is ProtocolVersion)
			{
				return Equals((ProtocolVersion)other);
			}
			return false;
		}
		return true;
	}

	public bool Equals(ProtocolVersion other)
	{
		if (other != null)
		{
			return version == other.version;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return version;
	}

	public static ProtocolVersion Get(int major, int minor)
	{
		return major switch
		{
			3 => minor switch
			{
				0 => SSLv3, 
				1 => TLSv10, 
				2 => TLSv11, 
				3 => TLSv12, 
				_ => GetUnknownVersion(major, minor, "TLS"), 
			}, 
			254 => minor switch
			{
				255 => DTLSv10, 
				254 => throw new TlsFatalAlert(47), 
				253 => DTLSv12, 
				_ => GetUnknownVersion(major, minor, "DTLS"), 
			}, 
			_ => throw new TlsFatalAlert(47), 
		};
	}

	public override string ToString()
	{
		return name;
	}

	private static ProtocolVersion GetUnknownVersion(int major, int minor, string prefix)
	{
		TlsUtilities.CheckUint8(major);
		TlsUtilities.CheckUint8(minor);
		int num = (major << 8) | minor;
		string text = Platform.ToUpperInvariant(Convert.ToString(0x10000 | num, 16).Substring(1));
		return new ProtocolVersion(num, prefix + " 0x" + text);
	}
}
