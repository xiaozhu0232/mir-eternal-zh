using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms;

public class CmsSignedDataGenerator : CmsSignedGenerator
{
	private class SignerInf
	{
		private readonly CmsSignedGenerator outer;

		private readonly ISignatureFactory sigCalc;

		private readonly SignerIdentifier signerIdentifier;

		private readonly string digestOID;

		private readonly string encOID;

		private readonly CmsAttributeTableGenerator sAttr;

		private readonly CmsAttributeTableGenerator unsAttr;

		private readonly Org.BouncyCastle.Asn1.Cms.AttributeTable baseSignedTable;

		internal AlgorithmIdentifier DigestAlgorithmID => new AlgorithmIdentifier(new DerObjectIdentifier(digestOID), DerNull.Instance);

		internal CmsAttributeTableGenerator SignedAttributes => sAttr;

		internal CmsAttributeTableGenerator UnsignedAttributes => unsAttr;

		internal SignerInf(CmsSignedGenerator outer, AsymmetricKeyParameter key, SignerIdentifier signerIdentifier, string digestOID, string encOID, CmsAttributeTableGenerator sAttr, CmsAttributeTableGenerator unsAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable baseSignedTable)
		{
			string digestAlgName = Helper.GetDigestAlgName(digestOID);
			string algorithm = digestAlgName + "with" + Helper.GetEncryptionAlgName(encOID);
			this.outer = outer;
			sigCalc = new Asn1SignatureFactory(algorithm, key);
			this.signerIdentifier = signerIdentifier;
			this.digestOID = digestOID;
			this.encOID = encOID;
			this.sAttr = sAttr;
			this.unsAttr = unsAttr;
			this.baseSignedTable = baseSignedTable;
		}

		internal SignerInf(CmsSignedGenerator outer, ISignatureFactory sigCalc, SignerIdentifier signerIdentifier, CmsAttributeTableGenerator sAttr, CmsAttributeTableGenerator unsAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable baseSignedTable)
		{
			this.outer = outer;
			this.sigCalc = sigCalc;
			this.signerIdentifier = signerIdentifier;
			digestOID = new DefaultDigestAlgorithmIdentifierFinder().find((AlgorithmIdentifier)sigCalc.AlgorithmDetails).Algorithm.Id;
			encOID = ((AlgorithmIdentifier)sigCalc.AlgorithmDetails).Algorithm.Id;
			this.sAttr = sAttr;
			this.unsAttr = unsAttr;
			this.baseSignedTable = baseSignedTable;
		}

		internal SignerInfo ToSignerInfo(DerObjectIdentifier contentType, CmsProcessable content, SecureRandom random)
		{
			AlgorithmIdentifier digestAlgorithmID = DigestAlgorithmID;
			string digestAlgName = Helper.GetDigestAlgName(digestOID);
			string algorithm = digestAlgName + "with" + Helper.GetEncryptionAlgName(encOID);
			byte[] array;
			if (outer._digests.Contains(digestOID))
			{
				array = (byte[])outer._digests[digestOID];
			}
			else
			{
				IDigest digestInstance = Helper.GetDigestInstance(digestAlgName);
				content?.Write(new DigestSink(digestInstance));
				array = DigestUtilities.DoFinal(digestInstance);
				outer._digests.Add(digestOID, array.Clone());
			}
			IStreamCalculator streamCalculator = sigCalc.CreateCalculator();
			Stream stream = new BufferedStream(streamCalculator.Stream);
			Asn1Set asn1Set = null;
			if (sAttr != null)
			{
				IDictionary baseParameters = outer.GetBaseParameters(contentType, digestAlgorithmID, array);
				Org.BouncyCastle.Asn1.Cms.AttributeTable attributeTable = sAttr.GetAttributes(baseParameters);
				if (contentType == null && attributeTable != null && attributeTable[CmsAttributes.ContentType] != null)
				{
					IDictionary dictionary = attributeTable.ToDictionary();
					dictionary.Remove(CmsAttributes.ContentType);
					attributeTable = new Org.BouncyCastle.Asn1.Cms.AttributeTable(dictionary);
				}
				asn1Set = outer.GetAttributeSet(attributeTable);
				new DerOutputStream(stream).WriteObject(asn1Set);
			}
			else
			{
				content?.Write(stream);
			}
			Platform.Dispose(stream);
			byte[] array2 = ((IBlockResult)streamCalculator.GetResult()).Collect();
			Asn1Set unauthenticatedAttributes = null;
			if (unsAttr != null)
			{
				IDictionary baseParameters2 = outer.GetBaseParameters(contentType, digestAlgorithmID, array);
				baseParameters2[CmsAttributeTableParameter.Signature] = array2.Clone();
				Org.BouncyCastle.Asn1.Cms.AttributeTable attributes = unsAttr.GetAttributes(baseParameters2);
				unauthenticatedAttributes = outer.GetAttributeSet(attributes);
			}
			Asn1Encodable defaultX509Parameters = SignerUtilities.GetDefaultX509Parameters(algorithm);
			AlgorithmIdentifier encAlgorithmIdentifier = Helper.GetEncAlgorithmIdentifier(new DerObjectIdentifier(encOID), defaultX509Parameters);
			return new SignerInfo(signerIdentifier, digestAlgorithmID, asn1Set, encAlgorithmIdentifier, new DerOctetString(array2), unauthenticatedAttributes);
		}
	}

	private static readonly CmsSignedHelper Helper = CmsSignedHelper.Instance;

	private readonly IList signerInfs = Platform.CreateArrayList();

	public CmsSignedDataGenerator()
	{
	}

	public CmsSignedDataGenerator(SecureRandom rand)
		: base(rand)
	{
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string digestOID)
	{
		AddSigner(privateKey, cert, Helper.GetEncOid(privateKey, digestOID), digestOID);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string encryptionOID, string digestOID)
	{
		doAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(cert), encryptionOID, digestOID, new DefaultSignedAttributeTableGenerator(), null, null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string digestOID)
	{
		AddSigner(privateKey, subjectKeyID, Helper.GetEncOid(privateKey, digestOID), digestOID);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string encryptionOID, string digestOID)
	{
		doAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID, new DefaultSignedAttributeTableGenerator(), null, null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string digestOID, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		AddSigner(privateKey, cert, Helper.GetEncOid(privateKey, digestOID), digestOID, signedAttr, unsignedAttr);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string encryptionOID, string digestOID, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		doAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(cert), encryptionOID, digestOID, new DefaultSignedAttributeTableGenerator(signedAttr), new SimpleAttributeTableGenerator(unsignedAttr), signedAttr);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string digestOID, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		AddSigner(privateKey, subjectKeyID, Helper.GetEncOid(privateKey, digestOID), digestOID, signedAttr, unsignedAttr);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string encryptionOID, string digestOID, Org.BouncyCastle.Asn1.Cms.AttributeTable signedAttr, Org.BouncyCastle.Asn1.Cms.AttributeTable unsignedAttr)
	{
		doAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID, new DefaultSignedAttributeTableGenerator(signedAttr), new SimpleAttributeTableGenerator(unsignedAttr), signedAttr);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string digestOID, CmsAttributeTableGenerator signedAttrGen, CmsAttributeTableGenerator unsignedAttrGen)
	{
		AddSigner(privateKey, cert, Helper.GetEncOid(privateKey, digestOID), digestOID, signedAttrGen, unsignedAttrGen);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, X509Certificate cert, string encryptionOID, string digestOID, CmsAttributeTableGenerator signedAttrGen, CmsAttributeTableGenerator unsignedAttrGen)
	{
		doAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(cert), encryptionOID, digestOID, signedAttrGen, unsignedAttrGen, null);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string digestOID, CmsAttributeTableGenerator signedAttrGen, CmsAttributeTableGenerator unsignedAttrGen)
	{
		AddSigner(privateKey, subjectKeyID, Helper.GetEncOid(privateKey, digestOID), digestOID, signedAttrGen, unsignedAttrGen);
	}

	public void AddSigner(AsymmetricKeyParameter privateKey, byte[] subjectKeyID, string encryptionOID, string digestOID, CmsAttributeTableGenerator signedAttrGen, CmsAttributeTableGenerator unsignedAttrGen)
	{
		doAddSigner(privateKey, CmsSignedGenerator.GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID, signedAttrGen, unsignedAttrGen, null);
	}

	public void AddSignerInfoGenerator(SignerInfoGenerator signerInfoGenerator)
	{
		signerInfs.Add(new SignerInf(this, signerInfoGenerator.contentSigner, signerInfoGenerator.sigId, signerInfoGenerator.signedGen, signerInfoGenerator.unsignedGen, null));
	}

	private void doAddSigner(AsymmetricKeyParameter privateKey, SignerIdentifier signerIdentifier, string encryptionOID, string digestOID, CmsAttributeTableGenerator signedAttrGen, CmsAttributeTableGenerator unsignedAttrGen, Org.BouncyCastle.Asn1.Cms.AttributeTable baseSignedTable)
	{
		signerInfs.Add(new SignerInf(this, privateKey, signerIdentifier, digestOID, encryptionOID, signedAttrGen, unsignedAttrGen, baseSignedTable));
	}

	public CmsSignedData Generate(CmsProcessable content)
	{
		return Generate(content, encapsulate: false);
	}

	public CmsSignedData Generate(string signedContentType, CmsProcessable content, bool encapsulate)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
		_digests.Clear();
		foreach (SignerInformation signer in _signers)
		{
			asn1EncodableVector.Add(Helper.FixAlgID(signer.DigestAlgorithmID));
			asn1EncodableVector2.Add(signer.ToSignerInfo());
		}
		DerObjectIdentifier contentType = ((signedContentType == null) ? null : new DerObjectIdentifier(signedContentType));
		foreach (SignerInf signerInf in signerInfs)
		{
			try
			{
				asn1EncodableVector.Add(signerInf.DigestAlgorithmID);
				asn1EncodableVector2.Add(signerInf.ToSignerInfo(contentType, content, rand));
			}
			catch (IOException e)
			{
				throw new CmsException("encoding error.", e);
			}
			catch (InvalidKeyException e2)
			{
				throw new CmsException("key inappropriate for signature.", e2);
			}
			catch (SignatureException e3)
			{
				throw new CmsException("error creating signature.", e3);
			}
			catch (CertificateEncodingException e4)
			{
				throw new CmsException("error creating sid.", e4);
			}
		}
		Asn1Set certificates = null;
		if (_certs.Count != 0)
		{
			certificates = (base.UseDerForCerts ? CmsUtilities.CreateDerSetFromList(_certs) : CmsUtilities.CreateBerSetFromList(_certs));
		}
		Asn1Set crls = null;
		if (_crls.Count != 0)
		{
			crls = (base.UseDerForCrls ? CmsUtilities.CreateDerSetFromList(_crls) : CmsUtilities.CreateBerSetFromList(_crls));
		}
		Asn1OctetString content2 = null;
		if (encapsulate)
		{
			MemoryStream memoryStream = new MemoryStream();
			if (content != null)
			{
				try
				{
					content.Write(memoryStream);
				}
				catch (IOException e5)
				{
					throw new CmsException("encapsulation error.", e5);
				}
			}
			content2 = new BerOctetString(memoryStream.ToArray());
		}
		ContentInfo contentInfo = new ContentInfo(contentType, content2);
		SignedData content3 = new SignedData(new DerSet(asn1EncodableVector), contentInfo, certificates, crls, new DerSet(asn1EncodableVector2));
		ContentInfo sigData = new ContentInfo(CmsObjectIdentifiers.SignedData, content3);
		return new CmsSignedData(content, sigData);
	}

	public CmsSignedData Generate(CmsProcessable content, bool encapsulate)
	{
		return Generate(CmsSignedGenerator.Data, content, encapsulate);
	}

	public SignerInformationStore GenerateCounterSigners(SignerInformation signer)
	{
		return Generate(null, new CmsProcessableByteArray(signer.GetSignature()), encapsulate: false).GetSignerInfos();
	}
}
