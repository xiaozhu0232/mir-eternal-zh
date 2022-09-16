using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSecretKeyRingBundle
{
	private readonly IDictionary secretRings;

	private readonly IList order;

	[Obsolete("Use 'Count' property instead")]
	public int Size => order.Count;

	public int Count => order.Count;

	private PgpSecretKeyRingBundle(IDictionary secretRings, IList order)
	{
		this.secretRings = secretRings;
		this.order = order;
	}

	public PgpSecretKeyRingBundle(byte[] encoding)
		: this(new MemoryStream(encoding, writable: false))
	{
	}

	public PgpSecretKeyRingBundle(Stream inputStream)
		: this(new PgpObjectFactory(inputStream).AllPgpObjects())
	{
	}

	public PgpSecretKeyRingBundle(IEnumerable e)
	{
		secretRings = Platform.CreateHashtable();
		order = Platform.CreateArrayList();
		foreach (object item in e)
		{
			if (!(item is PgpSecretKeyRing pgpSecretKeyRing))
			{
				throw new PgpException(Platform.GetTypeName(item) + " found where PgpSecretKeyRing expected");
			}
			long keyId = pgpSecretKeyRing.GetPublicKey().KeyId;
			secretRings.Add(keyId, pgpSecretKeyRing);
			order.Add(keyId);
		}
	}

	public IEnumerable GetKeyRings()
	{
		return new EnumerableProxy(secretRings.Values);
	}

	public IEnumerable GetKeyRings(string userId)
	{
		return GetKeyRings(userId, matchPartial: false, ignoreCase: false);
	}

	public IEnumerable GetKeyRings(string userId, bool matchPartial)
	{
		return GetKeyRings(userId, matchPartial, ignoreCase: false);
	}

	public IEnumerable GetKeyRings(string userId, bool matchPartial, bool ignoreCase)
	{
		IList list = Platform.CreateArrayList();
		if (ignoreCase)
		{
			userId = Platform.ToUpperInvariant(userId);
		}
		foreach (PgpSecretKeyRing keyRing in GetKeyRings())
		{
			foreach (string userId2 in keyRing.GetSecretKey().UserIds)
			{
				string text2 = userId2;
				if (ignoreCase)
				{
					text2 = Platform.ToUpperInvariant(text2);
				}
				if (matchPartial)
				{
					if (Platform.IndexOf(text2, userId) > -1)
					{
						list.Add(keyRing);
					}
				}
				else if (text2.Equals(userId))
				{
					list.Add(keyRing);
				}
			}
		}
		return new EnumerableProxy(list);
	}

	public PgpSecretKey GetSecretKey(long keyId)
	{
		foreach (PgpSecretKeyRing keyRing in GetKeyRings())
		{
			PgpSecretKey secretKey = keyRing.GetSecretKey(keyId);
			if (secretKey != null)
			{
				return secretKey;
			}
		}
		return null;
	}

	public PgpSecretKeyRing GetSecretKeyRing(long keyId)
	{
		if (secretRings.Contains(keyId))
		{
			return (PgpSecretKeyRing)secretRings[keyId];
		}
		foreach (PgpSecretKeyRing keyRing in GetKeyRings())
		{
			PgpSecretKey secretKey = keyRing.GetSecretKey(keyId);
			if (secretKey != null)
			{
				return keyRing;
			}
		}
		return null;
	}

	public bool Contains(long keyID)
	{
		return GetSecretKey(keyID) != null;
	}

	public byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public void Encode(Stream outStr)
	{
		BcpgOutputStream outStr2 = BcpgOutputStream.Wrap(outStr);
		foreach (object item in order)
		{
			long num = (long)item;
			PgpSecretKeyRing pgpSecretKeyRing = (PgpSecretKeyRing)secretRings[num];
			pgpSecretKeyRing.Encode(outStr2);
		}
	}

	public static PgpSecretKeyRingBundle AddSecretKeyRing(PgpSecretKeyRingBundle bundle, PgpSecretKeyRing secretKeyRing)
	{
		long keyId = secretKeyRing.GetPublicKey().KeyId;
		if (bundle.secretRings.Contains(keyId))
		{
			throw new ArgumentException("Collection already contains a key with a keyId for the passed in ring.");
		}
		IDictionary dictionary = Platform.CreateHashtable(bundle.secretRings);
		IList list = Platform.CreateArrayList(bundle.order);
		dictionary[keyId] = secretKeyRing;
		list.Add(keyId);
		return new PgpSecretKeyRingBundle(dictionary, list);
	}

	public static PgpSecretKeyRingBundle RemoveSecretKeyRing(PgpSecretKeyRingBundle bundle, PgpSecretKeyRing secretKeyRing)
	{
		long keyId = secretKeyRing.GetPublicKey().KeyId;
		if (!bundle.secretRings.Contains(keyId))
		{
			throw new ArgumentException("Collection does not contain a key with a keyId for the passed in ring.");
		}
		IDictionary dictionary = Platform.CreateHashtable(bundle.secretRings);
		IList list = Platform.CreateArrayList(bundle.order);
		dictionary.Remove(keyId);
		list.Remove(keyId);
		return new PgpSecretKeyRingBundle(dictionary, list);
	}
}
