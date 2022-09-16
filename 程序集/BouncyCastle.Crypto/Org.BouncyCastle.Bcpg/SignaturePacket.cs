using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg;

public class SignaturePacket : ContainedPacket
{
	private int version;

	private int signatureType;

	private long creationTime;

	private long keyId;

	private PublicKeyAlgorithmTag keyAlgorithm;

	private HashAlgorithmTag hashAlgorithm;

	private MPInteger[] signature;

	private byte[] fingerprint;

	private SignatureSubpacket[] hashedData;

	private SignatureSubpacket[] unhashedData;

	private byte[] signatureEncoding;

	public int Version => version;

	public int SignatureType => signatureType;

	public long KeyId => keyId;

	public PublicKeyAlgorithmTag KeyAlgorithm => keyAlgorithm;

	public HashAlgorithmTag HashAlgorithm => hashAlgorithm;

	public long CreationTime => creationTime;

	internal SignaturePacket(BcpgInputStream bcpgIn)
	{
		version = bcpgIn.ReadByte();
		if (version == 3 || version == 2)
		{
			bcpgIn.ReadByte();
			signatureType = bcpgIn.ReadByte();
			creationTime = (((long)bcpgIn.ReadByte() << 24) | ((long)bcpgIn.ReadByte() << 16) | ((long)bcpgIn.ReadByte() << 8) | (uint)bcpgIn.ReadByte()) * 1000;
			keyId |= (long)bcpgIn.ReadByte() << 56;
			keyId |= (long)bcpgIn.ReadByte() << 48;
			keyId |= (long)bcpgIn.ReadByte() << 40;
			keyId |= (long)bcpgIn.ReadByte() << 32;
			keyId |= (long)bcpgIn.ReadByte() << 24;
			keyId |= (long)bcpgIn.ReadByte() << 16;
			keyId |= (long)bcpgIn.ReadByte() << 8;
			keyId |= (uint)bcpgIn.ReadByte();
			keyAlgorithm = (PublicKeyAlgorithmTag)bcpgIn.ReadByte();
			hashAlgorithm = (HashAlgorithmTag)bcpgIn.ReadByte();
		}
		else
		{
			if (version != 4)
			{
				throw new Exception("unsupported version: " + version);
			}
			signatureType = bcpgIn.ReadByte();
			keyAlgorithm = (PublicKeyAlgorithmTag)bcpgIn.ReadByte();
			hashAlgorithm = (HashAlgorithmTag)bcpgIn.ReadByte();
			int num = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
			byte[] buffer = new byte[num];
			bcpgIn.ReadFully(buffer);
			SignatureSubpacketsParser signatureSubpacketsParser = new SignatureSubpacketsParser(new MemoryStream(buffer, writable: false));
			IList list = Platform.CreateArrayList();
			SignatureSubpacket value;
			while ((value = signatureSubpacketsParser.ReadPacket()) != null)
			{
				list.Add(value);
			}
			hashedData = new SignatureSubpacket[list.Count];
			for (int i = 0; i != hashedData.Length; i++)
			{
				SignatureSubpacket signatureSubpacket = (SignatureSubpacket)list[i];
				if (signatureSubpacket is IssuerKeyId)
				{
					keyId = ((IssuerKeyId)signatureSubpacket).KeyId;
				}
				else if (signatureSubpacket is SignatureCreationTime)
				{
					creationTime = DateTimeUtilities.DateTimeToUnixMs(((SignatureCreationTime)signatureSubpacket).GetTime());
				}
				hashedData[i] = signatureSubpacket;
			}
			int num2 = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
			byte[] buffer2 = new byte[num2];
			bcpgIn.ReadFully(buffer2);
			signatureSubpacketsParser = new SignatureSubpacketsParser(new MemoryStream(buffer2, writable: false));
			list.Clear();
			while ((value = signatureSubpacketsParser.ReadPacket()) != null)
			{
				list.Add(value);
			}
			unhashedData = new SignatureSubpacket[list.Count];
			for (int j = 0; j != unhashedData.Length; j++)
			{
				SignatureSubpacket signatureSubpacket2 = (SignatureSubpacket)list[j];
				if (signatureSubpacket2 is IssuerKeyId)
				{
					keyId = ((IssuerKeyId)signatureSubpacket2).KeyId;
				}
				unhashedData[j] = signatureSubpacket2;
			}
		}
		fingerprint = new byte[2];
		bcpgIn.ReadFully(fingerprint);
		switch (keyAlgorithm)
		{
		case PublicKeyAlgorithmTag.RsaGeneral:
		case PublicKeyAlgorithmTag.RsaSign:
		{
			MPInteger mPInteger8 = new MPInteger(bcpgIn);
			signature = new MPInteger[1] { mPInteger8 };
			return;
		}
		case PublicKeyAlgorithmTag.Dsa:
		{
			MPInteger mPInteger6 = new MPInteger(bcpgIn);
			MPInteger mPInteger7 = new MPInteger(bcpgIn);
			signature = new MPInteger[2] { mPInteger6, mPInteger7 };
			return;
		}
		case PublicKeyAlgorithmTag.ElGamalEncrypt:
		case PublicKeyAlgorithmTag.ElGamalGeneral:
		{
			MPInteger mPInteger3 = new MPInteger(bcpgIn);
			MPInteger mPInteger4 = new MPInteger(bcpgIn);
			MPInteger mPInteger5 = new MPInteger(bcpgIn);
			signature = new MPInteger[3] { mPInteger3, mPInteger4, mPInteger5 };
			return;
		}
		case PublicKeyAlgorithmTag.ECDsa:
		{
			MPInteger mPInteger = new MPInteger(bcpgIn);
			MPInteger mPInteger2 = new MPInteger(bcpgIn);
			signature = new MPInteger[2] { mPInteger, mPInteger2 };
			return;
		}
		}
		if (keyAlgorithm >= PublicKeyAlgorithmTag.Experimental_1 && keyAlgorithm <= PublicKeyAlgorithmTag.Experimental_11)
		{
			signature = null;
			MemoryStream memoryStream = new MemoryStream();
			int num3;
			while ((num3 = bcpgIn.ReadByte()) >= 0)
			{
				memoryStream.WriteByte((byte)num3);
			}
			signatureEncoding = memoryStream.ToArray();
			return;
		}
		throw new IOException("unknown signature key algorithm: " + keyAlgorithm);
	}

	public SignaturePacket(int signatureType, long keyId, PublicKeyAlgorithmTag keyAlgorithm, HashAlgorithmTag hashAlgorithm, SignatureSubpacket[] hashedData, SignatureSubpacket[] unhashedData, byte[] fingerprint, MPInteger[] signature)
		: this(4, signatureType, keyId, keyAlgorithm, hashAlgorithm, hashedData, unhashedData, fingerprint, signature)
	{
	}

	public SignaturePacket(int version, int signatureType, long keyId, PublicKeyAlgorithmTag keyAlgorithm, HashAlgorithmTag hashAlgorithm, long creationTime, byte[] fingerprint, MPInteger[] signature)
		: this(version, signatureType, keyId, keyAlgorithm, hashAlgorithm, null, null, fingerprint, signature)
	{
		this.creationTime = creationTime;
	}

	public SignaturePacket(int version, int signatureType, long keyId, PublicKeyAlgorithmTag keyAlgorithm, HashAlgorithmTag hashAlgorithm, SignatureSubpacket[] hashedData, SignatureSubpacket[] unhashedData, byte[] fingerprint, MPInteger[] signature)
	{
		this.version = version;
		this.signatureType = signatureType;
		this.keyId = keyId;
		this.keyAlgorithm = keyAlgorithm;
		this.hashAlgorithm = hashAlgorithm;
		this.hashedData = hashedData;
		this.unhashedData = unhashedData;
		this.fingerprint = fingerprint;
		this.signature = signature;
		if (hashedData != null)
		{
			setCreationTime();
		}
	}

	public byte[] GetSignatureTrailer()
	{
		byte[] array = null;
		if (version == 3)
		{
			array = new byte[5];
			long num = creationTime / 1000;
			array[0] = (byte)signatureType;
			array[1] = (byte)(num >> 24);
			array[2] = (byte)(num >> 16);
			array[3] = (byte)(num >> 8);
			array[4] = (byte)num;
		}
		else
		{
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.WriteByte((byte)Version);
			memoryStream.WriteByte((byte)SignatureType);
			memoryStream.WriteByte((byte)KeyAlgorithm);
			memoryStream.WriteByte((byte)HashAlgorithm);
			MemoryStream memoryStream2 = new MemoryStream();
			SignatureSubpacket[] hashedSubPackets = GetHashedSubPackets();
			for (int i = 0; i != hashedSubPackets.Length; i++)
			{
				hashedSubPackets[i].Encode(memoryStream2);
			}
			byte[] array2 = memoryStream2.ToArray();
			memoryStream.WriteByte((byte)(array2.Length >> 8));
			memoryStream.WriteByte((byte)array2.Length);
			memoryStream.Write(array2, 0, array2.Length);
			byte[] array3 = memoryStream.ToArray();
			memoryStream.WriteByte((byte)Version);
			memoryStream.WriteByte(byte.MaxValue);
			memoryStream.WriteByte((byte)(array3.Length >> 24));
			memoryStream.WriteByte((byte)(array3.Length >> 16));
			memoryStream.WriteByte((byte)(array3.Length >> 8));
			memoryStream.WriteByte((byte)array3.Length);
			array = memoryStream.ToArray();
		}
		return array;
	}

	public MPInteger[] GetSignature()
	{
		return signature;
	}

	public byte[] GetSignatureBytes()
	{
		if (signatureEncoding != null)
		{
			return (byte[])signatureEncoding.Clone();
		}
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		MPInteger[] array = signature;
		foreach (MPInteger bcpgObject in array)
		{
			try
			{
				bcpgOutputStream.WriteObject(bcpgObject);
			}
			catch (IOException ex)
			{
				throw new Exception("internal error: " + ex);
			}
		}
		return memoryStream.ToArray();
	}

	public SignatureSubpacket[] GetHashedSubPackets()
	{
		return hashedData;
	}

	public SignatureSubpacket[] GetUnhashedSubPackets()
	{
		return unhashedData;
	}

	public override void Encode(BcpgOutputStream bcpgOut)
	{
		MemoryStream memoryStream = new MemoryStream();
		BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(memoryStream);
		bcpgOutputStream.WriteByte((byte)version);
		if (version == 3 || version == 2)
		{
			bcpgOutputStream.Write(5, (byte)signatureType);
			bcpgOutputStream.WriteInt((int)(creationTime / 1000));
			bcpgOutputStream.WriteLong(keyId);
			bcpgOutputStream.Write((byte)keyAlgorithm, (byte)hashAlgorithm);
		}
		else
		{
			if (version != 4)
			{
				throw new IOException("unknown version: " + version);
			}
			bcpgOutputStream.Write((byte)signatureType, (byte)keyAlgorithm, (byte)hashAlgorithm);
			EncodeLengthAndData(bcpgOutputStream, GetEncodedSubpackets(hashedData));
			EncodeLengthAndData(bcpgOutputStream, GetEncodedSubpackets(unhashedData));
		}
		bcpgOutputStream.Write(fingerprint);
		if (signature != null)
		{
			bcpgOutputStream.WriteObjects(signature);
		}
		else
		{
			bcpgOutputStream.Write(signatureEncoding);
		}
		bcpgOut.WritePacket(PacketTag.Signature, memoryStream.ToArray(), oldFormat: true);
	}

	private static void EncodeLengthAndData(BcpgOutputStream pOut, byte[] data)
	{
		pOut.WriteShort((short)data.Length);
		pOut.Write(data);
	}

	private static byte[] GetEncodedSubpackets(SignatureSubpacket[] ps)
	{
		MemoryStream memoryStream = new MemoryStream();
		foreach (SignatureSubpacket signatureSubpacket in ps)
		{
			signatureSubpacket.Encode(memoryStream);
		}
		return memoryStream.ToArray();
	}

	private void setCreationTime()
	{
		SignatureSubpacket[] array = hashedData;
		foreach (SignatureSubpacket signatureSubpacket in array)
		{
			if (signatureSubpacket is SignatureCreationTime)
			{
				creationTime = DateTimeUtilities.DateTimeToUnixMs(((SignatureCreationTime)signatureSubpacket).GetTime());
				break;
			}
		}
	}
}
