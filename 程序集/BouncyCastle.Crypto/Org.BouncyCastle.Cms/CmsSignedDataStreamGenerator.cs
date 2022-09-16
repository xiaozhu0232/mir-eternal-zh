using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms;

public class CmsSignedDataStreamGenerator : CmsSignedGenerator
{
	private class DigestAndSignerInfoGeneratorHolder
	{
		internal readonly ISignerInfoGenerator signerInf;

		internal readonly string digestOID;

		internal AlgorithmIdentifier DigestAlgorithm => new AlgorithmIdentifier(new DerObjectIdentifier(digestOID), DerNull.Instance);

		internal DigestAndSignerInfoGeneratorHolder(ISignerInfoGenerator signerInf, string digestOID)
		{
			this.signerInf = signerInf;
			this.digestOID = digestOID;
		}
	}

	private class SignerInfoGeneratorImpl : ISignerInfoGenerator
	{
		private readonly CmsSignedDataStreamGenerator outer;

		private readonly SignerIdentifier _signerIdentifier;

		private readonly string _digestOID;

		private readonly string _encOID;

		private readonly CmsAttributeTableGenerator _sAttr;

		private readonly CmsAttributeTableGenerator _unsAttr;

		private readonly string _encName;

		private readonly ISigner _sig;

		internal SignerInfoGeneratorImpl(CmsSignedDataStreamGenerator outer, AsymmetricKeyParameter key, SignerIdentifier signerIdentifier, string digestOID, string encOID, CmsAttributeTableGenerator sAttr, CmsAttributeTableGenerator unsAttr)
		{
			this.outer = outer;
			_signerIdentifier = signerIdentifier;
			_digestOID = digestOID;
			_encOID = encOID;
			_sAttr = sAttr;
			_unsAttr = unsAttr;
			_encName = Helper.GetEncryptionAlgName(_encOID);
			string digestAlgName = Helper.GetDigestAlgName(_digestOID);
			string algorithm = digestAlgName + "with" + _encName;
			if (_sAttr != null)
			{
				_sig = Helper.GetSignatureInstance(algorithm);
			}
			else if (_encName.Equals("RSA"))
			{
				_sig = Helper.GetSignatureInstance("RSA");
			}
			else
			{
				if (!_encName.Equals("DSA"))
				{
					throw new SignatureException("algorithm: " + _encName + " not supported in base signatures.");
				}
				_sig = Helper.GetSignatureInstance("NONEwithDSA");
			}
			_sig.Init(forSigning: true, new ParametersWithRandom(key, outer.rand));
		}

		public SignerInfo Generate(DerObjectIdentifier contentType, AlgorithmIdentifier digestAlgorithm, byte[] calculatedDigest)
		{
			try
			{
				string digestAlgName = Helper.GetDigestAlgName(_digestOID);
				string algorithm = digestAlgName + "with" + _encName;
				byte[] array = calculatedDigest;
				Asn1Set asn1Set = null;
				if (_sAttr != null)
				{
					IDictionary baseParameters = outer.GetBaseParameters(contentType, digestAlgorithm, calculatedDigest);
					Org.BouncyCastle.Asn1.Cms.AttributeTable attributeTable = _sAttr.GetAttributes(baseParameters);
					if (contentType == null && attributeTable != null && attributeTable[CmsAttributes.ContentType] != null)
					{
						IDictionary dictionary = attributeTable.ToDictionary();
						dictionary.Remove(CmsAttributes.ContentType);
						attributeTable = new Org.BouncyCastle.Asn1.Cms.AttributeTable(dictionary);
					}
					asn1Set = outer.GetAttributeSet(attributeTable);
					array = asn1Set.GetEncoded("DER");
				}
				else if (_encName.Equals("RSA"))
				{
					DigestInfo digestInfo = new DigestInfo(digestAlgorithm, calculatedDigest);
					array = digestInfo.GetEncoded("DER");
				}
				_sig.BlockUpdate(array, 0, array.Length);
				byte[] array2 = _sig.GenerateSignature();
				Asn1Set unauthenticatedAttributes = null;
				if (_unsAttr != null)
				{
					IDictionary baseParameters2 = outer.GetBaseParameters(contentType, digestAlgorithm, calculatedDigest);
					baseParameters2[CmsAttributeTableParameter.Signature] = array2.Clone();
					Org.BouncyCastle.Asn1.Cms.AttributeTable attributes = _unsAttr.GetAttributes(baseParameters2);
					unauthenticatedAttributes = outer.GetAttributeSet(attributes);
				}
				Asn1Encodable defaultX509Parameters = SignerUtilities.GetDefaultX509Parameters(algorithm);
				AlgorithmIdentifier encAlgorithmIdentifier = Helper.GetEncAlgorithmIdentifier(new DerObjectIdentifier(_encOID), defaultX509Parameters);
				return new SignerInfo(_signerIdentifier, digestAlgorithm, asn1Set, encAlgorithmIdentifier, new DerOctetString(array2), unauthenticatedAttributes);
			}
			catch (IOException e)
			{
				throw new CmsStreamException("encoding error.", e);
			}
			catch (SignatureException e2)
			{
				throw new CmsStreamException("error creating signature.", e2);
			}
		}
	}

	private class CmsSignedDataOutputStream : BaseOutputStream
	{
		private readonly CmsSignedDataStreamGenerator outer;

		private Stream _out;

		private DerObjectIdentifier _contentOID;

		private BerSequenceGenerator _sGen;

		private BerSequenceGenerator _sigGen;

		private BerSequenceGenerator _eiGen;

		public CmsSignedDataOutputStream(CmsSignedDataStreamGenerator outer, Stream outStream, string contentOID, BerSequenceGenerator sGen, BerSequenceGenerator sigGen, BerSequenceGenerator eiGen)
		{
			this.outer = outer;
			_out = outStream;
			_contentOID = new DerObjectIdentifier(contentOID);
			_sGen = sGen;
			_sigGen = sigGen;
			_eiGen = eiGen;
		}

		public override void WriteByte(byte b)
		{
			_out.WriteByte(b);
		}

		public override void Write(byte[] bytes, int off, int len)
		{
			_out.Write(bytes, off, len);
		}

		public override void Close()
		{
			DoClose();
			base.Close();
		}

		private void DoClose()
		{
			Platform.Dispose(_out);
			_eiGen.Close();
			outer._digests.Clear();
			if (outer._certs.Count > 0)
			{
				Asn1Set obj = (outer.UseDerForCerts ? CmsUtilities.CreateDerSetFromList(outer._certs) : CmsUtilities.CreateBerSetFromList(outer._certs));
				WriteToGenerator(_sigGen, new BerTaggedObject(explicitly: false, 0, obj));
			}
			if (outer._crls.Count > 0)
			{
				Asn1Set obj2 = (outer.UseDerForCrls ? CmsUtilities.CreateDerSetFromList(outer._crls) : CmsUtilities.CreateBerSetFromList(outer._crls));
				WriteToGenerator(_sigGen, new BerTaggedObject(explicitly: false, 1, obj2));
			}
			foreach (object messageDigest in outer._messageDigests)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)messageDigest;
				outer._messageHashes.Add(dictionaryEntry.Key, DigestUtilities.DoFinal((IDigest)dictionaryEntry.Value));
			}
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
			foreach (DigestAndSignerInfoGeneratorHolder signerInf in outer._signerInfs)
			{
				AlgorithmIdentifier digestAlgorithm = signerInf.DigestAlgorithm;
				byte[] array = (byte[])outer._messageHashes[Helper.GetDigestAlgName(signerInf.digestOID)];
				outer._digests[signerInf.digestOID] = array.Clone();
				asn1EncodableVector.Add(signerInf.signerInf.Generate(_contentOID, digestAlgorithm, array));
			}
			foreach (SignerInformation signer in outer._signers)
			{
				asn1EncodableVector.Add(signer.ToSignerInfo());
			}
			WriteToGenerator(_sigGen, new DerSet(asn1EncodableVector));
			_sigGen.Close();
			_sGen.Close();
		}

		private static void WriteToGenerator(Asn1Generator ag, Asn1Encodable ae)
		{
			byte[] encoded = ae.GetEncoded();
			ag.GetRawOutputStream().Write(encoded, 0, encoded.Length);
		}
	}

	private static readonly CmsSignedHelper Helper = CmsSignedHelper.Instance;

	private readonly IList _signerInfs = Platform.CreateArrayList();

	private readonly ISet _messageDigestOids = new HashSet();

	private readonly IDictionary _messageDigests = Platform.CreateHashtable();

	private readonly IDictionary _messageHashes = Platform.CreateHashtable();

	private bool _messageDigestsLocked;

	private int _bufferSize;

	public CmsSignedDataStreamGenerator()
	{
	}

	public CmsSignedDataStreamGenerator(SecureRandom rand)
		: base(rand)
	{
	}

	public void SetBufferSize(int bufferSize)
	{
		_bufferSize = bufferSize;
	}

	public void AddDigests(params string[] digestOids)
	{
		AddDigests((IEnumerable)digestOids);
	}

	public void AddDigests(IEnumerable digestOids)
	{
		foreach (string digestOid in digestOids)
		{
			ConfigureDigest(digestOid);
		}
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string digestOid)
	{
		AddSigner(privateKey, cert, digestOid, new DefaultSignedAttributeTableGenerator(), null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string encryptionOid, string digestOid)
	{
		AddSigner(privateKey, cert, encryptionOid, digestOid, new DefaultSignedAttributeTableGenerator(), null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string digestOid, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		AddSigner(privateKey, cert, digestOid, new DefaultSignedAttributeTableGenerator(signedAttr), new SimpleAttributeTableGenerator(unsignedAttr));
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string encryptionOid, string digestOid, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		AddSigner(privateKey, cert, encryptionOid, digestOid, new DefaultSignedAttributeTableGenerator(signedAttr), new SimpleAttributeTableGenerator(unsignedAttr));
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string digestOid, CmsAttributeTableGenerator signedAttrGenerator, CmsAttributeTableGenerator unsignedAttrGenerator)
	{
		AddSigner(privateKey, cert, Helper.GetEncOid(privateKey, digestOid), digestOid, signedAttrGenerator, unsignedAttrGenerator);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string encryptionOid, string digestOid, CmsAttributeTableGenerator signedAttrGenerator, CmsAttributeTableGenerator unsignedAttrGenerator)
	{
		DoAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(cert), encryptionOid, digestOid, signedAttrGenerator, unsignedAttrGenerator);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string digestOid)
	{
		AddSigner(privateKey, subjectKeyID, digestOid, new DefaultSignedAttributeTableGenerator(), null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string encryptionOid, string digestOid)
	{
		AddSigner(privateKey, subjectKeyID, encryptionOid, digestOid, new DefaultSignedAttributeTableGenerator(), null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string digestOid, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		AddSigner(privateKey, subjectKeyID, digestOid, new DefaultSignedAttributeTableGenerator(signedAttr), new SimpleAttributeTableGenerator(unsignedAttr));
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string digestOid, CmsAttributeTableGenerator signedAttrGenerator, CmsAttributeTableGenerator unsignedAttrGenerator)
	{
		AddSigner(privateKey, subjectKeyID, Helper.GetEncOid(privateKey, digestOid), digestOid, signedAttrGenerator, unsignedAttrGenerator);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string encryptionOid, string digestOid, CmsAttributeTableGenerator signedAttrGenerator, CmsAttributeTableGenerator unsignedAttrGenerator)
	{
		DoAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(subjectKeyID), encryptionOid, digestOid, signedAttrGenerator, unsignedAttrGenerator);
	}

	private void DoAddSigner(AsymmetricKeyParameter privateKey, SignerIdentifier signerIdentifier, string encryptionOid, string digestOid, CmsAttributeTableGenerator signedAttrGenerator, CmsAttributeTableGenerator unsignedAttrGenerator)
	{
		ConfigureDigest(digestOid);
		SignerInfoGeneratorImpl signerInf = new SignerInfoGeneratorImpl(this, privateKey, signerIdentifier, digestOid, encryptionOid, signedAttrGenerator, unsignedAttrGenerator);
		_signerInfs.Add(new DigestAndSignerInfoGeneratorHolder(signerInf, digestOid));
	}

	internal override void AddSignerCallback(SignerInformation si)
	{
		RegisterDigestOid(si.DigestAlgorithmID.Algorithm.Id);
	}

	public Stream Open(Stream outStream)
	{
		return Open(outStream, encapsulate: false);
	}

	public Stream Open(Stream outStream, bool encapsulate)
	{
		return Open(outStream, CmsSignedGenerator.Data, encapsulate);
	}

	public Stream Open(Stream outStream, bool encapsulate, Stream dataOutputStream)
	{
		return Open(outStream, CmsSignedGenerator.Data, encapsulate, dataOutputStream);
	}

	public Stream Open(Stream outStream, string signedContentType, bool encapsulate)
	{
		return Open(outStream, signedContentType, encapsulate, null);
	}

	public Stream Open(Stream outStream, string signedContentType, bool encapsulate, Stream dataOutputStream)
	{
		if (outStream == null)
		{
			throw new ArgumentNullException("outStream");
		}
		if (!outStream.CanWrite)
		{
			throw new ArgumentException("Expected writeable stream", "outStream");
		}
		if (dataOutputStream != null && !dataOutputStream.CanWrite)
		{
			throw new ArgumentException("Expected writeable stream", "dataOutputStream");
		}
		_messageDigestsLocked = true;
		BerSequenceGenerator berSequenceGenerator = new BerSequenceGenerator(outStream);
		berSequenceGenerator.AddObject(CmsObjectIdentifiers.SignedData);
		BerSequenceGenerator berSequenceGenerator2 = new BerSequenceGenerator(berSequenceGenerator.GetRawOutputStream(), 0, isExplicit: true);
		DerObjectIdentifier derObjectIdentifier = ((signedContentType == null) ? null : new DerObjectIdentifier(signedContentType));
		berSequenceGenerator2.AddObject(CalculateVersion(derObjectIdentifier));
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (string messageDigestOid in _messageDigestOids)
		{
			asn1EncodableVector.Add(new AlgorithmIdentifier(new DerObjectIdentifier(messageDigestOid), DerNull.Instance));
		}
		byte[] encoded = new DerSet(asn1EncodableVector).GetEncoded();
		berSequenceGenerator2.GetRawOutputStream().Write(encoded, 0, encoded.Length);
		BerSequenceGenerator berSequenceGenerator3 = new BerSequenceGenerator(berSequenceGenerator2.GetRawOutputStream());
		berSequenceGenerator3.AddObject(derObjectIdentifier);
		Stream s = (encapsulate ? CmsUtilities.CreateBerOctetOutputStream(berSequenceGenerator3.GetRawOutputStream(), 0, isExplicit: true, _bufferSize) : null);
		Stream safeTeeOutputStream = GetSafeTeeOutputStream(dataOutputStream, s);
		Stream outStream2 = AttachDigestsToOutputStream(_messageDigests.Values, safeTeeOutputStream);
		return new CmsSignedDataOutputStream(this, outStream2, signedContentType, berSequenceGenerator, berSequenceGenerator2, berSequenceGenerator3);
	}

	private void RegisterDigestOid(string digestOid)
	{
		if (_messageDigestsLocked)
		{
			if (!_messageDigestOids.Contains(digestOid))
			{
				throw new InvalidOperationException("Cannot register new digest OIDs after the data stream is opened");
			}
		}
		else
		{
			_messageDigestOids.Add(digestOid);
		}
	}

	private void ConfigureDigest(string digestOid)
	{
		RegisterDigestOid(digestOid);
		string digestAlgName = Helper.GetDigestAlgName(digestOid);
		IDigest digest = (IDigest)_messageDigests[digestAlgName];
		if (digest == null)
		{
			if (_messageDigestsLocked)
			{
				throw new InvalidOperationException("Cannot configure new digests after the data stream is opened");
			}
			digest = Helper.GetDigestInstance(digestAlgName);
			_messageDigests[digestAlgName] = digest;
		}
	}

	internal void Generate(Stream outStream, string eContentType, bool encapsulate, Stream dataOutputStream, CmsProcessable content)
	{
		Stream stream = Open(outStream, eContentType, encapsulate, dataOutputStream);
		content?.Write(stream);
		Platform.Dispose(stream);
	}

	private DerInteger CalculateVersion(DerObjectIdentifier contentOid)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (_certs != null)
		{
			foreach (object cert in _certs)
			{
				if (cert is Asn1TaggedObject)
				{
					Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)cert;
					if (asn1TaggedObject.TagNo == 1)
					{
						flag3 = true;
					}
					else if (asn1TaggedObject.TagNo == 2)
					{
						flag4 = true;
					}
					else if (asn1TaggedObject.TagNo == 3)
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			return new DerInteger(5);
		}
		if (_crls != null)
		{
			foreach (object crl in _crls)
			{
				if (crl is Asn1TaggedObject)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			return new DerInteger(5);
		}
		if (flag4)
		{
			return new DerInteger(4);
		}
		if (flag3 || !CmsObjectIdentifiers.Data.Equals(contentOid) || CheckForVersion3(_signers))
		{
			return new DerInteger(3);
		}
		return new DerInteger(1);
	}

	private bool CheckForVersion3(IList signerInfos)
	{
		foreach (SignerInformation signerInfo in signerInfos)
		{
			SignerInfo instance = SignerInfo.GetInstance(signerInfo.ToSignerInfo());
			if (instance.Version.IntValueExact == 3)
			{
				return true;
			}
		}
		return false;
	}

	private static Stream AttachDigestsToOutputStream(ICollection digests, Stream s)
	{
		Stream stream = s;
		foreach (IDigest digest in digests)
		{
			stream = GetSafeTeeOutputStream(stream, new DigestSink(digest));
		}
		return stream;
	}

	private static Stream GetSafeOutputStream(Stream s)
	{
		if (s == null)
		{
			return new NullOutputStream();
		}
		return s;
	}

	private static Stream GetSafeTeeOutputStream(Stream s1, Stream s2)
	{
		if (s1 == null)
		{
			return GetSafeOutputStream(s2);
		}
		if (s2 == null)
		{
			return GetSafeOutputStream(s1);
		}
		return new TeeOutputStream(s1, s2);
	}
}
