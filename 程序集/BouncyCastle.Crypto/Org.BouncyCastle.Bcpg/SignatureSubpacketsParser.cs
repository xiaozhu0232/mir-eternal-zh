using System.IO;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg;

public class SignatureSubpacketsParser
{
	private readonly Stream input;

	public SignatureSubpacketsParser(Stream input)
	{
		this.input = input;
	}

	public SignatureSubpacket ReadPacket()
	{
		int num = input.ReadByte();
		if (num < 0)
		{
			return null;
		}
		int num2 = 0;
		bool isLongLength = false;
		if (num < 192)
		{
			num2 = num;
		}
		else if (num <= 223)
		{
			num2 = (num - 192 << 8) + input.ReadByte() + 192;
		}
		else
		{
			if (num != 255)
			{
				throw new IOException("unexpected length header");
			}
			isLongLength = true;
			num2 = (input.ReadByte() << 24) | (input.ReadByte() << 16) | (input.ReadByte() << 8) | input.ReadByte();
		}
		int num3 = input.ReadByte();
		if (num3 < 0)
		{
			throw new EndOfStreamException("unexpected EOF reading signature sub packet");
		}
		if (num2 <= 0)
		{
			throw new EndOfStreamException("out of range data found in signature sub packet");
		}
		byte[] array = new byte[num2 - 1];
		int num4 = Streams.ReadFully(input, array);
		bool critical = (num3 & 0x80) != 0;
		SignatureSubpacketTag signatureSubpacketTag = (SignatureSubpacketTag)(num3 & 0x7F);
		if (num4 != array.Length)
		{
			array = signatureSubpacketTag switch
			{
				SignatureSubpacketTag.CreationTime => CheckData(array, 4, num4, "Signature Creation Time"), 
				SignatureSubpacketTag.IssuerKeyId => CheckData(array, 8, num4, "Issuer"), 
				SignatureSubpacketTag.KeyExpireTime => CheckData(array, 4, num4, "Signature Key Expiration Time"), 
				SignatureSubpacketTag.ExpireTime => CheckData(array, 4, num4, "Signature Expiration Time"), 
				_ => throw new EndOfStreamException("truncated subpacket data."), 
			};
		}
		switch (signatureSubpacketTag)
		{
		case SignatureSubpacketTag.CreationTime:
			return new SignatureCreationTime(critical, isLongLength, array);
		case SignatureSubpacketTag.KeyExpireTime:
			return new KeyExpirationTime(critical, isLongLength, array);
		case SignatureSubpacketTag.ExpireTime:
			return new SignatureExpirationTime(critical, isLongLength, array);
		case SignatureSubpacketTag.Revocable:
			return new Revocable(critical, isLongLength, array);
		case SignatureSubpacketTag.Exportable:
			return new Exportable(critical, isLongLength, array);
		case SignatureSubpacketTag.IssuerKeyId:
			return new IssuerKeyId(critical, isLongLength, array);
		case SignatureSubpacketTag.TrustSig:
			return new TrustSignature(critical, isLongLength, array);
		case SignatureSubpacketTag.PreferredSymmetricAlgorithms:
		case SignatureSubpacketTag.PreferredHashAlgorithms:
		case SignatureSubpacketTag.PreferredCompressionAlgorithms:
			return new PreferredAlgorithms(signatureSubpacketTag, critical, isLongLength, array);
		case SignatureSubpacketTag.KeyFlags:
			return new KeyFlags(critical, isLongLength, array);
		case SignatureSubpacketTag.PrimaryUserId:
			return new PrimaryUserId(critical, isLongLength, array);
		case SignatureSubpacketTag.SignerUserId:
			return new SignerUserId(critical, isLongLength, array);
		case SignatureSubpacketTag.NotationData:
			return new NotationData(critical, isLongLength, array);
		default:
			return new SignatureSubpacket(signatureSubpacketTag, critical, isLongLength, array);
		}
	}

	private byte[] CheckData(byte[] data, int expected, int bytesRead, string name)
	{
		if (bytesRead != expected)
		{
			throw new EndOfStreamException("truncated " + name + " subpacket data.");
		}
		return Arrays.CopyOfRange(data, 0, expected);
	}
}
