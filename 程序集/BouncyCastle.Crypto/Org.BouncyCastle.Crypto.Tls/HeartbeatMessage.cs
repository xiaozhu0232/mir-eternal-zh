using System;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class HeartbeatMessage
{
	internal class PayloadBuffer : MemoryStream
	{
		internal byte[] ToTruncatedByteArray(int payloadLength)
		{
			int num = payloadLength + 16;
			if (Length < num)
			{
				return null;
			}
			byte[] buffer = GetBuffer();
			return Arrays.CopyOf(buffer, payloadLength);
		}
	}

	protected readonly byte mType;

	protected readonly byte[] mPayload;

	protected readonly int mPaddingLength;

	public HeartbeatMessage(byte type, byte[] payload, int paddingLength)
	{
		if (!HeartbeatMessageType.IsValid(type))
		{
			throw new ArgumentException("not a valid HeartbeatMessageType value", "type");
		}
		if (payload == null || payload.Length >= 65536)
		{
			throw new ArgumentException("must have length < 2^16", "payload");
		}
		if (paddingLength < 16)
		{
			throw new ArgumentException("must be at least 16", "paddingLength");
		}
		mType = type;
		mPayload = payload;
		mPaddingLength = paddingLength;
	}

	public virtual void Encode(TlsContext context, Stream output)
	{
		TlsUtilities.WriteUint8(mType, output);
		TlsUtilities.CheckUint16(mPayload.Length);
		TlsUtilities.WriteUint16(mPayload.Length, output);
		output.Write(mPayload, 0, mPayload.Length);
		byte[] array = new byte[mPaddingLength];
		context.NonceRandomGenerator.NextBytes(array);
		output.Write(array, 0, array.Length);
	}

	public static HeartbeatMessage Parse(Stream input)
	{
		byte b = TlsUtilities.ReadUint8(input);
		if (!HeartbeatMessageType.IsValid(b))
		{
			throw new TlsFatalAlert(47);
		}
		int payloadLength = TlsUtilities.ReadUint16(input);
		PayloadBuffer payloadBuffer = new PayloadBuffer();
		Streams.PipeAll(input, payloadBuffer);
		byte[] array = payloadBuffer.ToTruncatedByteArray(payloadLength);
		if (array == null)
		{
			return null;
		}
		TlsUtilities.CheckUint16(payloadBuffer.Length);
		int paddingLength = (int)payloadBuffer.Length - array.Length;
		return new HeartbeatMessage(b, array, paddingLength);
	}
}
