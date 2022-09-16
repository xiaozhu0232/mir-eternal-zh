using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkcs;

public class Pkcs12Store
{
	internal class CertId
	{
		private readonly byte[] id;

		internal byte[] Id => id;

		internal CertId(AsymmetricKeyParameter pubKey)
		{
			id = CreateSubjectKeyID(pubKey).GetKeyIdentifier();
		}

		internal CertId(byte[] id)
		{
			this.id = id;
		}

		public override int GetHashCode()
		{
			return Arrays.GetHashCode(id);
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (!(obj is CertId certId))
			{
				return false;
			}
			return Arrays.AreEqual(id, certId.id);
		}
	}

	private class IgnoresCaseHashtable : IEnumerable
	{
		private readonly IDictionary orig = Platform.CreateHashtable();

		private readonly IDictionary keys = Platform.CreateHashtable();

		public ICollection Keys => orig.Keys;

		public object this[string alias]
		{
			get
			{
				string key = Platform.ToUpperInvariant(alias);
				string text = (string)keys[key];
				if (text == null)
				{
					return null;
				}
				return orig[text];
			}
			set
			{
				string key = Platform.ToUpperInvariant(alias);
				string text = (string)keys[key];
				if (text != null)
				{
					orig.Remove(text);
				}
				keys[key] = alias;
				orig[alias] = value;
			}
		}

		public ICollection Values => orig.Values;

		public int Count => orig.Count;

		public void Clear()
		{
			orig.Clear();
			keys.Clear();
		}

		public IEnumerator GetEnumerator()
		{
			return orig.GetEnumerator();
		}

		public object Remove(string alias)
		{
			string key = Platform.ToUpperInvariant(alias);
			string text = (string)keys[key];
			if (text == null)
			{
				return null;
			}
			keys.Remove(key);
			object result = orig[text];
			orig.Remove(text);
			return result;
		}
	}

	public const string IgnoreUselessPasswordProperty = "Org.BouncyCastle.Pkcs12.IgnoreUselessPassword";

	private const int MinIterations = 1024;

	private const int SaltSize = 20;

	private readonly IgnoresCaseHashtable keys = new IgnoresCaseHashtable();

	private readonly IDictionary localIds = Platform.CreateHashtable();

	private readonly IgnoresCaseHashtable certs = new IgnoresCaseHashtable();

	private readonly IDictionary chainCerts = Platform.CreateHashtable();

	private readonly IDictionary keyCerts = Platform.CreateHashtable();

	private readonly DerObjectIdentifier keyAlgorithm;

	private readonly DerObjectIdentifier keyPrfAlgorithm;

	private readonly DerObjectIdentifier certAlgorithm;

	private readonly DerObjectIdentifier certPrfAlgorithm;

	private readonly bool useDerEncoding;

	private AsymmetricKeyEntry unmarkedKeyEntry = null;

	public IEnumerable Aliases => new EnumerableProxy(GetAliasesTable().Keys);

	public int Count => GetAliasesTable().Count;

	private static SubjectKeyIdentifier CreateSubjectKeyID(AsymmetricKeyParameter pubKey)
	{
		return new SubjectKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey));
	}

	internal Pkcs12Store(DerObjectIdentifier keyAlgorithm, DerObjectIdentifier certAlgorithm, bool useDerEncoding)
	{
		this.keyAlgorithm = keyAlgorithm;
		keyPrfAlgorithm = null;
		this.certAlgorithm = certAlgorithm;
		certPrfAlgorithm = null;
		this.useDerEncoding = useDerEncoding;
	}

	internal Pkcs12Store(DerObjectIdentifier keyAlgorithm, DerObjectIdentifier keyPrfAlgorithm, DerObjectIdentifier certAlgorithm, DerObjectIdentifier certPrfAlgorithm, bool useDerEncoding)
	{
		this.keyAlgorithm = keyAlgorithm;
		this.keyPrfAlgorithm = keyPrfAlgorithm;
		this.certAlgorithm = certAlgorithm;
		this.certPrfAlgorithm = certPrfAlgorithm;
		this.useDerEncoding = useDerEncoding;
	}

	public Pkcs12Store()
		: this(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc, PkcsObjectIdentifiers.PbewithShaAnd40BitRC2Cbc, useDerEncoding: false)
	{
	}

	public Pkcs12Store(Stream input, char[] password)
		: this()
	{
		Load(input, password);
	}

	protected virtual void LoadKeyBag(PrivateKeyInfo privKeyInfo, Asn1Set bagAttributes)
	{
		AsymmetricKeyParameter key = PrivateKeyFactory.CreateKey(privKeyInfo);
		IDictionary dictionary = Platform.CreateHashtable();
		AsymmetricKeyEntry value = new AsymmetricKeyEntry(key, dictionary);
		string text = null;
		Asn1OctetString asn1OctetString = null;
		if (bagAttributes != null)
		{
			foreach (Asn1Sequence bagAttribute in bagAttributes)
			{
				DerObjectIdentifier instance = DerObjectIdentifier.GetInstance(bagAttribute[0]);
				Asn1Set instance2 = Asn1Set.GetInstance(bagAttribute[1]);
				Asn1Encodable asn1Encodable = null;
				if (instance2.Count <= 0)
				{
					continue;
				}
				asn1Encodable = instance2[0];
				if (dictionary.Contains(instance.Id))
				{
					if (!dictionary[instance.Id].Equals(asn1Encodable))
					{
						throw new IOException("attempt to add existing attribute with different value");
					}
				}
				else
				{
					dictionary.Add(instance.Id, asn1Encodable);
				}
				if (instance.Equals(PkcsObjectIdentifiers.Pkcs9AtFriendlyName))
				{
					text = ((DerBmpString)asn1Encodable).GetString();
					keys[text] = value;
				}
				else if (instance.Equals(PkcsObjectIdentifiers.Pkcs9AtLocalKeyID))
				{
					asn1OctetString = (Asn1OctetString)asn1Encodable;
				}
			}
		}
		if (asn1OctetString != null)
		{
			string text2 = Hex.ToHexString(asn1OctetString.GetOctets());
			if (text == null)
			{
				keys[text2] = value;
			}
			else
			{
				localIds[text] = text2;
			}
		}
		else
		{
			unmarkedKeyEntry = value;
		}
	}

	protected virtual void LoadPkcs8ShroudedKeyBag(EncryptedPrivateKeyInfo encPrivKeyInfo, Asn1Set bagAttributes, char[] password, bool wrongPkcs12Zero)
	{
		if (password != null)
		{
			PrivateKeyInfo privKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(password, wrongPkcs12Zero, encPrivKeyInfo);
			LoadKeyBag(privKeyInfo, bagAttributes);
		}
	}

	public void Load(Stream input, char[] password)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		Pfx instance = Pfx.GetInstance(Asn1Object.FromStream(input));
		ContentInfo authSafe = instance.AuthSafe;
		bool wrongPkcs12Zero = false;
		if (instance.MacData != null)
		{
			if (password == null)
			{
				throw new ArgumentNullException("password", "no password supplied when one expected");
			}
			MacData macData = instance.MacData;
			DigestInfo mac = macData.Mac;
			AlgorithmIdentifier algorithmID = mac.AlgorithmID;
			byte[] salt = macData.GetSalt();
			int intValue = macData.IterationCount.IntValue;
			byte[] octets = Asn1OctetString.GetInstance(authSafe.Content).GetOctets();
			byte[] a = CalculatePbeMac(algorithmID.Algorithm, salt, intValue, password, wrongPkcs12Zero: false, octets);
			byte[] digest = mac.GetDigest();
			if (!Arrays.ConstantTimeAreEqual(a, digest))
			{
				if (password.Length > 0)
				{
					throw new IOException("PKCS12 key store MAC invalid - wrong password or corrupted file.");
				}
				a = CalculatePbeMac(algorithmID.Algorithm, salt, intValue, password, wrongPkcs12Zero: true, octets);
				if (!Arrays.ConstantTimeAreEqual(a, digest))
				{
					throw new IOException("PKCS12 key store MAC invalid - wrong password or corrupted file.");
				}
				wrongPkcs12Zero = true;
			}
		}
		else if (password != null)
		{
			string environmentVariable = Platform.GetEnvironmentVariable("Org.BouncyCastle.Pkcs12.IgnoreUselessPassword");
			if (environmentVariable == null || !Platform.EqualsIgnoreCase("true", environmentVariable))
			{
				throw new IOException("password supplied for keystore that does not require one");
			}
		}
		keys.Clear();
		localIds.Clear();
		unmarkedKeyEntry = null;
		IList list = Platform.CreateArrayList();
		if (authSafe.ContentType.Equals(PkcsObjectIdentifiers.Data))
		{
			Asn1OctetString instance2 = Asn1OctetString.GetInstance(authSafe.Content);
			AuthenticatedSafe instance3 = AuthenticatedSafe.GetInstance(instance2.GetOctets());
			ContentInfo[] contentInfo = instance3.GetContentInfo();
			ContentInfo[] array = contentInfo;
			foreach (ContentInfo contentInfo2 in array)
			{
				DerObjectIdentifier contentType = contentInfo2.ContentType;
				byte[] array2 = null;
				if (contentType.Equals(PkcsObjectIdentifiers.Data))
				{
					array2 = Asn1OctetString.GetInstance(contentInfo2.Content).GetOctets();
				}
				else if (contentType.Equals(PkcsObjectIdentifiers.EncryptedData) && password != null)
				{
					EncryptedData instance4 = EncryptedData.GetInstance(contentInfo2.Content);
					array2 = CryptPbeData(forEncryption: false, instance4.EncryptionAlgorithm, password, wrongPkcs12Zero, instance4.Content.GetOctets());
				}
				if (array2 == null)
				{
					continue;
				}
				Asn1Sequence instance5 = Asn1Sequence.GetInstance(array2);
				foreach (Asn1Sequence item in instance5)
				{
					SafeBag safeBag = new SafeBag(item);
					if (safeBag.BagID.Equals(PkcsObjectIdentifiers.CertBag))
					{
						list.Add(safeBag);
					}
					else if (safeBag.BagID.Equals(PkcsObjectIdentifiers.Pkcs8ShroudedKeyBag))
					{
						LoadPkcs8ShroudedKeyBag(EncryptedPrivateKeyInfo.GetInstance(safeBag.BagValue), safeBag.BagAttributes, password, wrongPkcs12Zero);
					}
					else if (safeBag.BagID.Equals(PkcsObjectIdentifiers.KeyBag))
					{
						LoadKeyBag(PrivateKeyInfo.GetInstance(safeBag.BagValue), safeBag.BagAttributes);
					}
				}
			}
		}
		certs.Clear();
		chainCerts.Clear();
		keyCerts.Clear();
		foreach (SafeBag item2 in list)
		{
			CertBag certBag = new CertBag((Asn1Sequence)item2.BagValue);
			byte[] octets2 = ((Asn1OctetString)certBag.CertValue).GetOctets();
			X509Certificate x509Certificate = new X509CertificateParser().ReadCertificate(octets2);
			IDictionary dictionary = Platform.CreateHashtable();
			Asn1OctetString asn1OctetString = null;
			string text = null;
			if (item2.BagAttributes != null)
			{
				foreach (Asn1Sequence bagAttribute in item2.BagAttributes)
				{
					DerObjectIdentifier instance6 = DerObjectIdentifier.GetInstance(bagAttribute[0]);
					Asn1Set instance7 = Asn1Set.GetInstance(bagAttribute[1]);
					if (instance7.Count <= 0)
					{
						continue;
					}
					Asn1Encodable asn1Encodable = instance7[0];
					if (dictionary.Contains(instance6.Id))
					{
						if (!dictionary[instance6.Id].Equals(asn1Encodable))
						{
							throw new IOException("attempt to add existing attribute with different value");
						}
					}
					else
					{
						dictionary.Add(instance6.Id, asn1Encodable);
					}
					if (instance6.Equals(PkcsObjectIdentifiers.Pkcs9AtFriendlyName))
					{
						text = ((DerBmpString)asn1Encodable).GetString();
					}
					else if (instance6.Equals(PkcsObjectIdentifiers.Pkcs9AtLocalKeyID))
					{
						asn1OctetString = (Asn1OctetString)asn1Encodable;
					}
				}
			}
			CertId certId = new CertId(x509Certificate.GetPublicKey());
			X509CertificateEntry value = new X509CertificateEntry(x509Certificate, dictionary);
			chainCerts[certId] = value;
			if (unmarkedKeyEntry != null)
			{
				if (keyCerts.Count == 0)
				{
					string text2 = Hex.ToHexString(certId.Id);
					keyCerts[text2] = value;
					keys[text2] = unmarkedKeyEntry;
				}
				else
				{
					keys["unmarked"] = unmarkedKeyEntry;
				}
				continue;
			}
			if (asn1OctetString != null)
			{
				string key = Hex.ToHexString(asn1OctetString.GetOctets());
				keyCerts[key] = value;
			}
			if (text != null)
			{
				certs[text] = value;
			}
		}
	}

	public AsymmetricKeyEntry GetKey(string alias)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		return (AsymmetricKeyEntry)keys[alias];
	}

	public bool IsCertificateEntry(string alias)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		if (certs[alias] != null)
		{
			return keys[alias] == null;
		}
		return false;
	}

	public bool IsKeyEntry(string alias)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		return keys[alias] != null;
	}

	private IDictionary GetAliasesTable()
	{
		IDictionary dictionary = Platform.CreateHashtable();
		foreach (string key3 in certs.Keys)
		{
			dictionary[key3] = "cert";
		}
		foreach (string key4 in keys.Keys)
		{
			if (dictionary[key4] == null)
			{
				dictionary[key4] = "key";
			}
		}
		return dictionary;
	}

	public bool ContainsAlias(string alias)
	{
		if (certs[alias] == null)
		{
			return keys[alias] != null;
		}
		return true;
	}

	public X509CertificateEntry GetCertificate(string alias)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		X509CertificateEntry x509CertificateEntry = (X509CertificateEntry)certs[alias];
		if (x509CertificateEntry == null)
		{
			string text = (string)localIds[alias];
			x509CertificateEntry = ((text == null) ? ((X509CertificateEntry)keyCerts[alias]) : ((X509CertificateEntry)keyCerts[text]));
		}
		return x509CertificateEntry;
	}

	public string GetCertificateAlias(X509Certificate cert)
	{
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		foreach (object cert2 in certs)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)cert2;
			X509CertificateEntry x509CertificateEntry = (X509CertificateEntry)dictionaryEntry.Value;
			if (x509CertificateEntry.Certificate.Equals(cert))
			{
				return (string)dictionaryEntry.Key;
			}
		}
		foreach (object keyCert in keyCerts)
		{
			DictionaryEntry dictionaryEntry2 = (DictionaryEntry)keyCert;
			X509CertificateEntry x509CertificateEntry2 = (X509CertificateEntry)dictionaryEntry2.Value;
			if (x509CertificateEntry2.Certificate.Equals(cert))
			{
				return (string)dictionaryEntry2.Key;
			}
		}
		return null;
	}

	public X509CertificateEntry[] GetCertificateChain(string alias)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		if (!IsKeyEntry(alias))
		{
			return null;
		}
		X509CertificateEntry x509CertificateEntry = GetCertificate(alias);
		if (x509CertificateEntry != null)
		{
			IList list = Platform.CreateArrayList();
			while (x509CertificateEntry != null)
			{
				X509Certificate certificate = x509CertificateEntry.Certificate;
				X509CertificateEntry x509CertificateEntry2 = null;
				Asn1OctetString extensionValue = certificate.GetExtensionValue(X509Extensions.AuthorityKeyIdentifier);
				if (extensionValue != null)
				{
					AuthorityKeyIdentifier instance = AuthorityKeyIdentifier.GetInstance(extensionValue.GetOctets());
					byte[] keyIdentifier = instance.GetKeyIdentifier();
					if (keyIdentifier != null)
					{
						x509CertificateEntry2 = (X509CertificateEntry)chainCerts[new CertId(keyIdentifier)];
					}
				}
				if (x509CertificateEntry2 == null)
				{
					X509Name issuerDN = certificate.IssuerDN;
					X509Name subjectDN = certificate.SubjectDN;
					if (!issuerDN.Equivalent(subjectDN))
					{
						foreach (CertId key in chainCerts.Keys)
						{
							X509CertificateEntry x509CertificateEntry3 = (X509CertificateEntry)chainCerts[key];
							X509Certificate certificate2 = x509CertificateEntry3.Certificate;
							X509Name subjectDN2 = certificate2.SubjectDN;
							if (subjectDN2.Equivalent(issuerDN))
							{
								try
								{
									certificate.Verify(certificate2.GetPublicKey());
									x509CertificateEntry2 = x509CertificateEntry3;
								}
								catch (InvalidKeyException)
								{
									continue;
								}
								break;
							}
						}
					}
				}
				list.Add(x509CertificateEntry);
				x509CertificateEntry = ((x509CertificateEntry2 == x509CertificateEntry) ? null : x509CertificateEntry2);
			}
			X509CertificateEntry[] array = new X509CertificateEntry[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = (X509CertificateEntry)list[i];
			}
			return array;
		}
		return null;
	}

	public void SetCertificateEntry(string alias, X509CertificateEntry certEntry)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		if (certEntry == null)
		{
			throw new ArgumentNullException("certEntry");
		}
		if (keys[alias] != null)
		{
			throw new ArgumentException("There is a key entry with the name " + alias + ".");
		}
		certs[alias] = certEntry;
		chainCerts[new CertId(certEntry.Certificate.GetPublicKey())] = certEntry;
	}

	public void SetKeyEntry(string alias, AsymmetricKeyEntry keyEntry, X509CertificateEntry[] chain)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		if (keyEntry == null)
		{
			throw new ArgumentNullException("keyEntry");
		}
		if (keyEntry.Key.IsPrivate && chain == null)
		{
			throw new ArgumentException("No certificate chain for private key");
		}
		if (keys[alias] != null)
		{
			DeleteEntry(alias);
		}
		keys[alias] = keyEntry;
		certs[alias] = chain[0];
		for (int i = 0; i != chain.Length; i++)
		{
			chainCerts[new CertId(chain[i].Certificate.GetPublicKey())] = chain[i];
		}
	}

	public void DeleteEntry(string alias)
	{
		if (alias == null)
		{
			throw new ArgumentNullException("alias");
		}
		AsymmetricKeyEntry asymmetricKeyEntry = (AsymmetricKeyEntry)keys[alias];
		if (asymmetricKeyEntry != null)
		{
			keys.Remove(alias);
		}
		X509CertificateEntry x509CertificateEntry = (X509CertificateEntry)certs[alias];
		if (x509CertificateEntry != null)
		{
			certs.Remove(alias);
			chainCerts.Remove(new CertId(x509CertificateEntry.Certificate.GetPublicKey()));
		}
		if (asymmetricKeyEntry != null)
		{
			string text = (string)localIds[alias];
			if (text != null)
			{
				localIds.Remove(alias);
				x509CertificateEntry = (X509CertificateEntry)keyCerts[text];
			}
			if (x509CertificateEntry != null)
			{
				keyCerts.Remove(text);
				chainCerts.Remove(new CertId(x509CertificateEntry.Certificate.GetPublicKey()));
			}
		}
		if (x509CertificateEntry == null && asymmetricKeyEntry == null)
		{
			throw new ArgumentException("no such entry as " + alias);
		}
	}

	public bool IsEntryOfType(string alias, Type entryType)
	{
		if ((object)entryType == typeof(X509CertificateEntry))
		{
			return IsCertificateEntry(alias);
		}
		if ((object)entryType == typeof(AsymmetricKeyEntry))
		{
			if (IsKeyEntry(alias))
			{
				return GetCertificate(alias) != null;
			}
			return false;
		}
		return false;
	}

	[Obsolete("Use 'Count' property instead")]
	public int Size()
	{
		return Count;
	}

	public void Save(Stream stream, char[] password, SecureRandom random)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (random == null)
		{
			throw new ArgumentNullException("random");
		}
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (string key2 in keys.Keys)
		{
			byte[] array = new byte[20];
			random.NextBytes(array);
			AsymmetricKeyEntry asymmetricKeyEntry = (AsymmetricKeyEntry)keys[key2];
			DerObjectIdentifier oid;
			Asn1Encodable asn1Encodable;
			if (password == null)
			{
				oid = PkcsObjectIdentifiers.KeyBag;
				asn1Encodable = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricKeyEntry.Key);
			}
			else
			{
				oid = PkcsObjectIdentifiers.Pkcs8ShroudedKeyBag;
				asn1Encodable = ((keyPrfAlgorithm == null) ? EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo(keyAlgorithm, password, array, 1024, asymmetricKeyEntry.Key) : EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo(keyAlgorithm, keyPrfAlgorithm, password, array, 1024, random, asymmetricKeyEntry.Key));
			}
			Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
			foreach (string bagAttributeKey in asymmetricKeyEntry.BagAttributeKeys)
			{
				Asn1Encodable element = asymmetricKeyEntry[bagAttributeKey];
				if (!bagAttributeKey.Equals(PkcsObjectIdentifiers.Pkcs9AtFriendlyName.Id))
				{
					asn1EncodableVector2.Add(new DerSequence(new DerObjectIdentifier(bagAttributeKey), new DerSet(element)));
				}
			}
			asn1EncodableVector2.Add(new DerSequence(PkcsObjectIdentifiers.Pkcs9AtFriendlyName, new DerSet(new DerBmpString(key2))));
			if (asymmetricKeyEntry[PkcsObjectIdentifiers.Pkcs9AtLocalKeyID] == null)
			{
				X509CertificateEntry certificate = GetCertificate(key2);
				AsymmetricKeyParameter publicKey = certificate.Certificate.GetPublicKey();
				SubjectKeyIdentifier element2 = CreateSubjectKeyID(publicKey);
				asn1EncodableVector2.Add(new DerSequence(PkcsObjectIdentifiers.Pkcs9AtLocalKeyID, new DerSet(element2)));
			}
			asn1EncodableVector.Add(new SafeBag(oid, asn1Encodable.ToAsn1Object(), new DerSet(asn1EncodableVector2)));
		}
		byte[] derEncoded = new DerSequence(asn1EncodableVector).GetDerEncoded();
		ContentInfo contentInfo = new ContentInfo(PkcsObjectIdentifiers.Data, new BerOctetString(derEncoded));
		byte[] array2 = new byte[20];
		random.NextBytes(array2);
		Asn1EncodableVector asn1EncodableVector3 = new Asn1EncodableVector();
		Pkcs12PbeParams pkcs12PbeParams = new Pkcs12PbeParams(array2, 1024);
		AlgorithmIdentifier algorithmIdentifier = new AlgorithmIdentifier(certAlgorithm, pkcs12PbeParams.ToAsn1Object());
		ISet set = new HashSet();
		foreach (string key3 in keys.Keys)
		{
			X509CertificateEntry certificate2 = GetCertificate(key3);
			CertBag certBag = new CertBag(PkcsObjectIdentifiers.X509Certificate, new DerOctetString(certificate2.Certificate.GetEncoded()));
			Asn1EncodableVector asn1EncodableVector4 = new Asn1EncodableVector();
			foreach (string bagAttributeKey2 in certificate2.BagAttributeKeys)
			{
				Asn1Encodable element3 = certificate2[bagAttributeKey2];
				if (!bagAttributeKey2.Equals(PkcsObjectIdentifiers.Pkcs9AtFriendlyName.Id))
				{
					asn1EncodableVector4.Add(new DerSequence(new DerObjectIdentifier(bagAttributeKey2), new DerSet(element3)));
				}
			}
			asn1EncodableVector4.Add(new DerSequence(PkcsObjectIdentifiers.Pkcs9AtFriendlyName, new DerSet(new DerBmpString(key3))));
			if (certificate2[PkcsObjectIdentifiers.Pkcs9AtLocalKeyID] == null)
			{
				AsymmetricKeyParameter publicKey2 = certificate2.Certificate.GetPublicKey();
				SubjectKeyIdentifier element4 = CreateSubjectKeyID(publicKey2);
				asn1EncodableVector4.Add(new DerSequence(PkcsObjectIdentifiers.Pkcs9AtLocalKeyID, new DerSet(element4)));
			}
			asn1EncodableVector3.Add(new SafeBag(PkcsObjectIdentifiers.CertBag, certBag.ToAsn1Object(), new DerSet(asn1EncodableVector4)));
			set.Add(certificate2.Certificate);
		}
		foreach (string key4 in certs.Keys)
		{
			X509CertificateEntry x509CertificateEntry = (X509CertificateEntry)certs[key4];
			if (keys[key4] != null)
			{
				continue;
			}
			CertBag certBag2 = new CertBag(PkcsObjectIdentifiers.X509Certificate, new DerOctetString(x509CertificateEntry.Certificate.GetEncoded()));
			Asn1EncodableVector asn1EncodableVector5 = new Asn1EncodableVector();
			foreach (string bagAttributeKey3 in x509CertificateEntry.BagAttributeKeys)
			{
				if (!bagAttributeKey3.Equals(PkcsObjectIdentifiers.Pkcs9AtLocalKeyID.Id))
				{
					Asn1Encodable element5 = x509CertificateEntry[bagAttributeKey3];
					if (!bagAttributeKey3.Equals(PkcsObjectIdentifiers.Pkcs9AtFriendlyName.Id))
					{
						asn1EncodableVector5.Add(new DerSequence(new DerObjectIdentifier(bagAttributeKey3), new DerSet(element5)));
					}
				}
			}
			asn1EncodableVector5.Add(new DerSequence(PkcsObjectIdentifiers.Pkcs9AtFriendlyName, new DerSet(new DerBmpString(key4))));
			asn1EncodableVector3.Add(new SafeBag(PkcsObjectIdentifiers.CertBag, certBag2.ToAsn1Object(), new DerSet(asn1EncodableVector5)));
			set.Add(x509CertificateEntry.Certificate);
		}
		foreach (CertId key5 in chainCerts.Keys)
		{
			X509CertificateEntry x509CertificateEntry2 = (X509CertificateEntry)chainCerts[key5];
			if (set.Contains(x509CertificateEntry2.Certificate))
			{
				continue;
			}
			CertBag certBag3 = new CertBag(PkcsObjectIdentifiers.X509Certificate, new DerOctetString(x509CertificateEntry2.Certificate.GetEncoded()));
			Asn1EncodableVector asn1EncodableVector6 = new Asn1EncodableVector();
			foreach (string bagAttributeKey4 in x509CertificateEntry2.BagAttributeKeys)
			{
				if (!bagAttributeKey4.Equals(PkcsObjectIdentifiers.Pkcs9AtLocalKeyID.Id))
				{
					asn1EncodableVector6.Add(new DerSequence(new DerObjectIdentifier(bagAttributeKey4), new DerSet(x509CertificateEntry2[bagAttributeKey4])));
				}
			}
			asn1EncodableVector3.Add(new SafeBag(PkcsObjectIdentifiers.CertBag, certBag3.ToAsn1Object(), new DerSet(asn1EncodableVector6)));
		}
		byte[] derEncoded2 = new DerSequence(asn1EncodableVector3).GetDerEncoded();
		ContentInfo contentInfo2;
		if (password == null || certAlgorithm == null)
		{
			contentInfo2 = new ContentInfo(PkcsObjectIdentifiers.Data, new BerOctetString(derEncoded2));
		}
		else
		{
			byte[] str = CryptPbeData(forEncryption: true, algorithmIdentifier, password, wrongPkcs12Zero: false, derEncoded2);
			EncryptedData encryptedData = new EncryptedData(PkcsObjectIdentifiers.Data, algorithmIdentifier, new BerOctetString(str));
			contentInfo2 = new ContentInfo(PkcsObjectIdentifiers.EncryptedData, encryptedData.ToAsn1Object());
		}
		ContentInfo[] info = new ContentInfo[2] { contentInfo, contentInfo2 };
		byte[] encoded = new AuthenticatedSafe(info).GetEncoded(useDerEncoding ? "DER" : "BER");
		ContentInfo contentInfo3 = new ContentInfo(PkcsObjectIdentifiers.Data, new BerOctetString(encoded));
		MacData macData = null;
		if (password != null)
		{
			byte[] array3 = new byte[20];
			random.NextBytes(array3);
			byte[] digest = CalculatePbeMac(OiwObjectIdentifiers.IdSha1, array3, 1024, password, wrongPkcs12Zero: false, encoded);
			AlgorithmIdentifier algID = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);
			DigestInfo digInfo = new DigestInfo(algID, digest);
			macData = new MacData(digInfo, array3, 1024);
		}
		Pfx obj = new Pfx(contentInfo3, macData);
		DerOutputStream derOutputStream = ((!useDerEncoding) ? new BerOutputStream(stream) : new DerOutputStream(stream));
		derOutputStream.WriteObject(obj);
	}

	internal static byte[] CalculatePbeMac(DerObjectIdentifier oid, byte[] salt, int itCount, char[] password, bool wrongPkcs12Zero, byte[] data)
	{
		Asn1Encodable pbeParameters = PbeUtilities.GenerateAlgorithmParameters(oid, salt, itCount);
		ICipherParameters parameters = PbeUtilities.GenerateCipherParameters(oid, password, wrongPkcs12Zero, pbeParameters);
		IMac mac = (IMac)PbeUtilities.CreateEngine(oid);
		mac.Init(parameters);
		return MacUtilities.DoFinal(mac, data);
	}

	private static byte[] CryptPbeData(bool forEncryption, AlgorithmIdentifier algId, char[] password, bool wrongPkcs12Zero, byte[] data)
	{
		if (!(PbeUtilities.CreateEngine(algId) is IBufferedCipher bufferedCipher))
		{
			throw new Exception("Unknown encryption algorithm: " + algId.Algorithm);
		}
		if (algId.Algorithm.Equals(PkcsObjectIdentifiers.IdPbeS2))
		{
			PbeS2Parameters instance = PbeS2Parameters.GetInstance(algId.Parameters);
			ICipherParameters parameters = PbeUtilities.GenerateCipherParameters(algId.Algorithm, password, instance);
			bufferedCipher.Init(forEncryption, parameters);
			return bufferedCipher.DoFinal(data);
		}
		Pkcs12PbeParams instance2 = Pkcs12PbeParams.GetInstance(algId.Parameters);
		ICipherParameters parameters2 = PbeUtilities.GenerateCipherParameters(algId.Algorithm, password, wrongPkcs12Zero, instance2);
		bufferedCipher.Init(forEncryption, parameters2);
		return bufferedCipher.DoFinal(data);
	}
}
