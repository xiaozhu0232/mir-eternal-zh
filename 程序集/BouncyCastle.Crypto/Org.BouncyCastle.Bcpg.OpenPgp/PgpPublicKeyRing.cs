using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpPublicKeyRing : PgpKeyRing
{
	private readonly IList keys;

	public PgpPublicKeyRing(byte[] encoding)
		: this(new MemoryStream(encoding, writable: false))
	{
	}

	internal PgpPublicKeyRing(IList pubKeys)
	{
		keys = pubKeys;
	}

	public PgpPublicKeyRing(Stream inputStream)
	{
		keys = Platform.CreateArrayList();
		BcpgInputStream bcpgInputStream = BcpgInputStream.Wrap(inputStream);
		PacketTag packetTag = bcpgInputStream.NextPacketTag();
		if (packetTag != PacketTag.PublicKey && packetTag != PacketTag.PublicSubkey)
		{
			int num = (int)packetTag;
			throw new IOException("public key ring doesn't start with public key tag: tag 0x" + num.ToString("X"));
		}
		PublicKeyPacket publicPk = ReadPublicKeyPacket(bcpgInputStream);
		TrustPacket trustPk = PgpKeyRing.ReadOptionalTrustPacket(bcpgInputStream);
		IList keySigs = PgpKeyRing.ReadSignaturesAndTrust(bcpgInputStream);
		PgpKeyRing.ReadUserIDs(bcpgInputStream, out var ids, out var idTrusts, out var idSigs);
		keys.Add(new PgpPublicKey(publicPk, trustPk, keySigs, ids, idTrusts, idSigs));
		while (bcpgInputStream.NextPacketTag() == PacketTag.PublicSubkey)
		{
			keys.Add(ReadSubkey(bcpgInputStream));
		}
	}

	public virtual PgpPublicKey GetPublicKey()
	{
		return (PgpPublicKey)keys[0];
	}

	public virtual PgpPublicKey GetPublicKey(long keyId)
	{
		foreach (PgpPublicKey key in keys)
		{
			if (keyId == key.KeyId)
			{
				return key;
			}
		}
		return null;
	}

	public virtual IEnumerable GetPublicKeys()
	{
		return new EnumerableProxy(keys);
	}

	public virtual byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public virtual void Encode(Stream outStr)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		foreach (PgpPublicKey key in keys)
		{
			key.Encode(outStr);
		}
	}

	public static PgpPublicKeyRing InsertPublicKey(PgpPublicKeyRing pubRing, PgpPublicKey pubKey)
	{
		IList list = Platform.CreateArrayList(pubRing.keys);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i != list.Count; i++)
		{
			PgpPublicKey pgpPublicKey = (PgpPublicKey)list[i];
			if (pgpPublicKey.KeyId == pubKey.KeyId)
			{
				flag = true;
				list[i] = pubKey;
			}
			if (pgpPublicKey.IsMasterKey)
			{
				flag2 = true;
			}
		}
		if (!flag)
		{
			if (pubKey.IsMasterKey)
			{
				if (flag2)
				{
					throw new ArgumentException("cannot add a master key to a ring that already has one");
				}
				list.Insert(0, pubKey);
			}
			else
			{
				list.Add(pubKey);
			}
		}
		return new PgpPublicKeyRing(list);
	}

	public static PgpPublicKeyRing RemovePublicKey(PgpPublicKeyRing pubRing, PgpPublicKey pubKey)
	{
		IList list = Platform.CreateArrayList(pubRing.keys);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			PgpPublicKey pgpPublicKey = (PgpPublicKey)list[i];
			if (pgpPublicKey.KeyId == pubKey.KeyId)
			{
				flag = true;
				list.RemoveAt(i);
			}
		}
		if (!flag)
		{
			return null;
		}
		return new PgpPublicKeyRing(list);
	}

	internal static PublicKeyPacket ReadPublicKeyPacket(BcpgInputStream bcpgInput)
	{
		Packet packet = bcpgInput.ReadPacket();
		if (!(packet is PublicKeyPacket))
		{
			throw new IOException("unexpected packet in stream: " + packet);
		}
		return (PublicKeyPacket)packet;
	}

	internal static PgpPublicKey ReadSubkey(BcpgInputStream bcpgInput)
	{
		PublicKeyPacket publicPk = ReadPublicKeyPacket(bcpgInput);
		TrustPacket trustPk = PgpKeyRing.ReadOptionalTrustPacket(bcpgInput);
		IList sigs = PgpKeyRing.ReadSignaturesAndTrust(bcpgInput);
		return new PgpPublicKey(publicPk, trustPk, sigs);
	}
}
