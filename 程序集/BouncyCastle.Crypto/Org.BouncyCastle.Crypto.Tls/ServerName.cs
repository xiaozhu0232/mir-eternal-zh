using System;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class ServerName
{
	protected readonly byte mNameType;

	protected readonly object mName;

	public virtual byte NameType => mNameType;

	public virtual object Name => mName;

	public ServerName(byte nameType, object name)
	{
		if (!IsCorrectType(nameType, name))
		{
			throw new ArgumentException("not an instance of the correct type", "name");
		}
		mNameType = nameType;
		mName = name;
	}

	public virtual string GetHostName()
	{
		if (!IsCorrectType(0, mName))
		{
			throw new InvalidOperationException("'name' is not a HostName string");
		}
		return (string)mName;
	}

	public virtual void Encode(Stream output)
	{
		TlsUtilities.WriteUint8(mNameType, output);
		if (mNameType == 0)
		{
			byte[] array = Strings.ToAsciiByteArray((string)mName);
			if (array.Length < 1)
			{
				throw new TlsFatalAlert(80);
			}
			TlsUtilities.WriteOpaque16(array, output);
			return;
		}
		throw new TlsFatalAlert(80);
	}

	public static ServerName Parse(Stream input)
	{
		byte b = TlsUtilities.ReadUint8(input);
		if (b == 0)
		{
			byte[] array = TlsUtilities.ReadOpaque16(input);
			if (array.Length < 1)
			{
				throw new TlsFatalAlert(50);
			}
			object name = Strings.FromAsciiByteArray(array);
			return new ServerName(b, name);
		}
		throw new TlsFatalAlert(50);
	}

	protected static bool IsCorrectType(byte nameType, object name)
	{
		if (nameType == 0)
		{
			return name is string;
		}
		throw new ArgumentException("unsupported NameType", "nameType");
	}
}
