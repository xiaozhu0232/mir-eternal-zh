using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpPublicKeyRingBundle
{
	private readonly IDictionary pubRings;

	private readonly IList order;

	[Obsolete("Use 'Count' property instead")]
	public int Size => order.Count;

	public int Count => order.Count;

	private PgpPublicKeyRingBundle(IDictionary pubRings, IList order)
	{
		this.pubRings = pubRings;
		this.order = order;
	}

	public PgpPublicKeyRingBundle(byte[] encoding)
		: this(new MemoryStream(encoding, writable: false))
	{
	}

	public PgpPublicKeyRingBundle(Stream inputStream)
		: this(new PgpObjectFactory(inputStream).AllPgpObjects())
	{
	}

	public PgpPublicKeyRingBundle(IEnumerable e)
	{
		pubRings = Platform.CreateHashtable();
		order = Platform.CreateArrayList();
		foreach (object item in e)
		{
			if (!(item is PgpPublicKeyRing pgpPublicKeyRing))
			{
				throw new PgpException(Platform.GetTypeName(item) + " found where PgpPublicKeyRing expected");
			}
			long keyId = pgpPublicKeyRing.GetPublicKey().KeyId;
			pubRings.Add(keyId, pgpPublicKeyRing);
			order.Add(keyId);
		}
	}

	public IEnumerable GetKeyRings()
	{
		return new EnumerableProxy(pubRings.Values);
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
		foreach (PgpPublicKeyRing keyRing in GetKeyRings())
		{
			foreach (string userId2 in keyRing.GetPublicKey().GetUserIds())
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

	public PgpPublicKey GetPublicKey(long keyId)
	{
		foreach (PgpPublicKeyRing keyRing in GetKeyRings())
		{
			PgpPublicKey publicKey = keyRing.GetPublicKey(keyId);
			if (publicKey != null)
			{
				return publicKey;
			}
		}
		return null;
	}

	public PgpPublicKeyRing GetPublicKeyRing(long keyId)
	{
		if (pubRings.Contains(keyId))
		{
			return (PgpPublicKeyRing)pubRings[keyId];
		}
		foreach (PgpPublicKeyRing keyRing in GetKeyRings())
		{
			PgpPublicKey publicKey = keyRing.GetPublicKey(keyId);
			if (publicKey != null)
			{
				return keyRing;
			}
		}
		return null;
	}

	public bool Contains(long keyID)
	{
		return GetPublicKey(keyID) != null;
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
			PgpPublicKeyRing pgpPublicKeyRing = (PgpPublicKeyRing)pubRings[num];
			pgpPublicKeyRing.Encode(outStr2);
		}
	}

	public static PgpPublicKeyRingBundle AddPublicKeyRing(PgpPublicKeyRingBundle bundle, PgpPublicKeyRing publicKeyRing)
	{
		long keyId = publicKeyRing.GetPublicKey().KeyId;
		if (bundle.pubRings.Contains(keyId))
		{
			throw new ArgumentException("Bundle already contains a key with a keyId for the passed in ring.");
		}
		IDictionary dictionary = Platform.CreateHashtable(bundle.pubRings);
		IList list = Platform.CreateArrayList(bundle.order);
		dictionary[keyId] = publicKeyRing;
		list.Add(keyId);
		return new PgpPublicKeyRingBundle(dictionary, list);
	}

	public static PgpPublicKeyRingBundle RemovePublicKeyRing(PgpPublicKeyRingBundle bundle, PgpPublicKeyRing publicKeyRing)
	{
		long keyId = publicKeyRing.GetPublicKey().KeyId;
		if (!bundle.pubRings.Contains(keyId))
		{
			throw new ArgumentException("Bundle does not contain a key with a keyId for the passed in ring.");
		}
		IDictionary dictionary = Platform.CreateHashtable(bundle.pubRings);
		IList list = Platform.CreateArrayList(bundle.order);
		dictionary.Remove(keyId);
		list.Remove(keyId);
		return new PgpPublicKeyRingBundle(dictionary, list);
	}
}
