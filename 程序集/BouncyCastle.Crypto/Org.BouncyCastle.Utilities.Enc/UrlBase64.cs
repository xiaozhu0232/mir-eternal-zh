using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders;

public class UrlBase64
{
	private static readonly IEncoder encoder = new UrlBase64Encoder();

	public static byte[] Encode(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream();
		try
		{
			encoder.Encode(data, 0, data.Length, memoryStream);
		}
		catch (IOException ex)
		{
			throw new Exception("exception encoding URL safe base64 string: " + ex.Message, ex);
		}
		return memoryStream.ToArray();
	}

	public static int Encode(byte[] data, Stream outStr)
	{
		return encoder.Encode(data, 0, data.Length, outStr);
	}

	public static byte[] Decode(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream();
		try
		{
			encoder.Decode(data, 0, data.Length, memoryStream);
		}
		catch (IOException ex)
		{
			throw new Exception("exception decoding URL safe base64 string: " + ex.Message, ex);
		}
		return memoryStream.ToArray();
	}

	public static int Decode(byte[] data, Stream outStr)
	{
		return encoder.Decode(data, 0, data.Length, outStr);
	}

	public static byte[] Decode(string data)
	{
		MemoryStream memoryStream = new MemoryStream();
		try
		{
			encoder.DecodeString(data, memoryStream);
		}
		catch (IOException ex)
		{
			throw new Exception("exception decoding URL safe base64 string: " + ex.Message, ex);
		}
		return memoryStream.ToArray();
	}

	public static int Decode(string data, Stream outStr)
	{
		return encoder.DecodeString(data, outStr);
	}
}
