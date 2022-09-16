using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class TlsSrpUtilities
{
	public static void AddSrpExtension(IDictionary extensions, byte[] identity)
	{
		extensions[12] = CreateSrpExtension(identity);
	}

	public static byte[] GetSrpExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 12);
		if (extensionData != null)
		{
			return ReadSrpExtension(extensionData);
		}
		return null;
	}

	public static byte[] CreateSrpExtension(byte[] identity)
	{
		if (identity == null)
		{
			throw new TlsFatalAlert(80);
		}
		return TlsUtilities.EncodeOpaque8(identity);
	}

	public static byte[] ReadSrpExtension(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		MemoryStream memoryStream = new MemoryStream(extensionData, writable: false);
		byte[] result = TlsUtilities.ReadOpaque8(memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		return result;
	}

	public static BigInteger ReadSrpParameter(Stream input)
	{
		return new BigInteger(1, TlsUtilities.ReadOpaque16(input));
	}

	public static void WriteSrpParameter(BigInteger x, Stream output)
	{
		TlsUtilities.WriteOpaque16(BigIntegers.AsUnsignedByteArray(x), output);
	}

	public static bool IsSrpCipherSuite(int cipherSuite)
	{
		switch (cipherSuite)
		{
		case 49178:
		case 49179:
		case 49180:
		case 49181:
		case 49182:
		case 49183:
		case 49184:
		case 49185:
		case 49186:
			return true;
		default:
			return false;
		}
	}
}
