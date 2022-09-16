using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Operators;

public class CmsContentEncryptorBuilder
{
	private static readonly IDictionary KeySizes;

	private readonly DerObjectIdentifier encryptionOID;

	private readonly int keySize;

	private readonly EnvelopedDataHelper helper = new EnvelopedDataHelper();

	static CmsContentEncryptorBuilder()
	{
		KeySizes = Platform.CreateHashtable();
		KeySizes[NistObjectIdentifiers.IdAes128Cbc] = 128;
		KeySizes[NistObjectIdentifiers.IdAes192Cbc] = 192;
		KeySizes[NistObjectIdentifiers.IdAes256Cbc] = 256;
		KeySizes[NttObjectIdentifiers.IdCamellia128Cbc] = 128;
		KeySizes[NttObjectIdentifiers.IdCamellia192Cbc] = 192;
		KeySizes[NttObjectIdentifiers.IdCamellia256Cbc] = 256;
	}

	private static int GetKeySize(DerObjectIdentifier oid)
	{
		if (KeySizes.Contains(oid))
		{
			return (int)KeySizes[oid];
		}
		return -1;
	}

	public CmsContentEncryptorBuilder(DerObjectIdentifier encryptionOID)
		: this(encryptionOID, GetKeySize(encryptionOID))
	{
	}

	public CmsContentEncryptorBuilder(DerObjectIdentifier encryptionOID, int keySize)
	{
		this.encryptionOID = encryptionOID;
		this.keySize = keySize;
	}

	public ICipherBuilderWithKey Build()
	{
		return new Asn1CipherBuilderWithKey(encryptionOID, keySize, null);
	}
}
