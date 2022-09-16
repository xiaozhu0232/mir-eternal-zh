using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSecretKeyRing : PgpKeyRing
{
	private readonly IList keys;

	private readonly IList extraPubKeys;

	internal PgpSecretKeyRing(IList keys)
		: this(keys, Platform.CreateArrayList())
	{
	}

	private PgpSecretKeyRing(IList keys, IList extraPubKeys)
	{
		this.keys = keys;
		this.extraPubKeys = extraPubKeys;
	}

	public PgpSecretKeyRing(byte[] encoding)
		: this(new MemoryStream(encoding))
	{
	}

	public PgpSecretKeyRing(Stream inputStream)
	{
		keys = Platform.CreateArrayList();
		extraPubKeys = Platform.CreateArrayList();
		BcpgInputStream bcpgInputStream = BcpgInputStream.Wrap(inputStream);
		PacketTag packetTag = bcpgInputStream.NextPacketTag();
		if (packetTag != PacketTag.SecretKey && packetTag != PacketTag.SecretSubkey)
		{
			int num = (int)packetTag;
			throw new IOException("secret key ring doesn't start with secret key tag: tag 0x" + num.ToString("X"));
		}
		SecretKeyPacket secretKeyPacket = (SecretKeyPacket)bcpgInputStream.ReadPacket();
		while (bcpgInputStream.NextPacketTag() == PacketTag.Experimental2)
		{
			bcpgInputStream.ReadPacket();
		}
		TrustPacket trustPk = PgpKeyRing.ReadOptionalTrustPacket(bcpgInputStream);
		IList keySigs = PgpKeyRing.ReadSignaturesAndTrust(bcpgInputStream);
		PgpKeyRing.ReadUserIDs(bcpgInputStream, out var ids, out var idTrusts, out var idSigs);
		keys.Add(new PgpSecretKey(secretKeyPacket, new PgpPublicKey(secretKeyPacket.PublicKeyPacket, trustPk, keySigs, ids, idTrusts, idSigs)));
		while (bcpgInputStream.NextPacketTag() == PacketTag.SecretSubkey || bcpgInputStream.NextPacketTag() == PacketTag.PublicSubkey)
		{
			if (bcpgInputStream.NextPacketTag() == PacketTag.SecretSubkey)
			{
				SecretSubkeyPacket secretSubkeyPacket = (SecretSubkeyPacket)bcpgInputStream.ReadPacket();
				while (bcpgInputStream.NextPacketTag() == PacketTag.Experimental2)
				{
					bcpgInputStream.ReadPacket();
				}
				TrustPacket trustPk2 = PgpKeyRing.ReadOptionalTrustPacket(bcpgInputStream);
				IList sigs = PgpKeyRing.ReadSignaturesAndTrust(bcpgInputStream);
				keys.Add(new PgpSecretKey(secretSubkeyPacket, new PgpPublicKey(secretSubkeyPacket.PublicKeyPacket, trustPk2, sigs)));
			}
			else
			{
				PublicSubkeyPacket publicPk = (PublicSubkeyPacket)bcpgInputStream.ReadPacket();
				TrustPacket trustPk3 = PgpKeyRing.ReadOptionalTrustPacket(bcpgInputStream);
				IList sigs2 = PgpKeyRing.ReadSignaturesAndTrust(bcpgInputStream);
				extraPubKeys.Add(new PgpPublicKey(publicPk, trustPk3, sigs2));
			}
		}
	}

	public PgpPublicKey GetPublicKey()
	{
		return ((PgpSecretKey)keys[0]).PublicKey;
	}

	public PgpSecretKey GetSecretKey()
	{
		return (PgpSecretKey)keys[0];
	}

	public IEnumerable GetSecretKeys()
	{
		return new EnumerableProxy(keys);
	}

	public PgpSecretKey GetSecretKey(long keyId)
	{
		foreach (PgpSecretKey key in keys)
		{
			if (keyId == key.KeyId)
			{
				return key;
			}
		}
		return null;
	}

	public IEnumerable GetExtraPublicKeys()
	{
		return new EnumerableProxy(extraPubKeys);
	}

	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public void Encode(Stream outStr)
	{
		if (outStr == null)
		{
			throw new ArgumentNullException("outStr");
		}
		foreach (PgpSecretKey key in keys)
		{
			key.Encode(outStr);
		}
		foreach (PgpPublicKey extraPubKey in extraPubKeys)
		{
			extraPubKey.Encode(outStr);
		}
	}

	public static PgpSecretKeyRing ReplacePublicKeys(PgpSecretKeyRing secretRing, PgpPublicKeyRing publicRing)
	{
		IList list = Platform.CreateArrayList(secretRing.keys.Count);
		foreach (PgpSecretKey key in secretRing.keys)
		{
			PgpPublicKey publicKey = publicRing.GetPublicKey(key.KeyId);
			list.Add(PgpSecretKey.ReplacePublicKey(key, publicKey));
		}
		return new PgpSecretKeyRing(list);
	}

	public static PgpSecretKeyRing CopyWithNewPassword(PgpSecretKeyRing ring, char[] oldPassPhrase, char[] newPassPhrase, SymmetricKeyAlgorithmTag newEncAlgorithm, SecureRandom rand)
	{
		IList list = Platform.CreateArrayList(ring.keys.Count);
		foreach (PgpSecretKey secretKey in ring.GetSecretKeys())
		{
			if (secretKey.IsPrivateKeyEmpty)
			{
				list.Add(secretKey);
			}
			else
			{
				list.Add(PgpSecretKey.CopyWithNewPassword(secretKey, oldPassPhrase, newPassPhrase, newEncAlgorithm, rand));
			}
		}
		return new PgpSecretKeyRing(list, ring.extraPubKeys);
	}

	public static PgpSecretKeyRing InsertSecretKey(PgpSecretKeyRing secRing, PgpSecretKey secKey)
	{
		IList list = Platform.CreateArrayList(secRing.keys);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i != list.Count; i++)
		{
			PgpSecretKey pgpSecretKey = (PgpSecretKey)list[i];
			if (pgpSecretKey.KeyId == secKey.KeyId)
			{
				flag = true;
				list[i] = secKey;
			}
			if (pgpSecretKey.IsMasterKey)
			{
				flag2 = true;
			}
		}
		if (!flag)
		{
			if (secKey.IsMasterKey)
			{
				if (flag2)
				{
					throw new ArgumentException("cannot add a master key to a ring that already has one");
				}
				list.Insert(0, secKey);
			}
			else
			{
				list.Add(secKey);
			}
		}
		return new PgpSecretKeyRing(list, secRing.extraPubKeys);
	}

	public static PgpSecretKeyRing RemoveSecretKey(PgpSecretKeyRing secRing, PgpSecretKey secKey)
	{
		IList list = Platform.CreateArrayList(secRing.keys);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			PgpSecretKey pgpSecretKey = (PgpSecretKey)list[i];
			if (pgpSecretKey.KeyId == secKey.KeyId)
			{
				flag = true;
				list.RemoveAt(i);
			}
		}
		if (!flag)
		{
			return null;
		}
		return new PgpSecretKeyRing(list, secRing.extraPubKeys);
	}
}
