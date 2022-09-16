using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms;

public class CmsEnvelopedDataStreamGenerator : CmsEnvelopedGenerator
{
	private class CmsEnvelopedDataOutputStream : BaseOutputStream
	{
		private readonly CmsEnvelopedGenerator _outer;

		private readonly CipherStream _out;

		private readonly BerSequenceGenerator _cGen;

		private readonly BerSequenceGenerator _envGen;

		private readonly BerSequenceGenerator _eiGen;

		public CmsEnvelopedDataOutputStream(CmsEnvelopedGenerator outer, CipherStream outStream, BerSequenceGenerator cGen, BerSequenceGenerator envGen, BerSequenceGenerator eiGen)
		{
			_outer = outer;
			_out = outStream;
			_cGen = cGen;
			_envGen = envGen;
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
			Platform.Dispose(_out);
			_eiGen.Close();
			if (_outer.unprotectedAttributeGenerator != null)
			{
				Org.BouncyCastle.Asn1.Cms.AttributeTable attributes = _outer.unprotectedAttributeGenerator.GetAttributes(Platform.CreateHashtable());
				Asn1Set obj = new BerSet(attributes.ToAsn1EncodableVector());
				_envGen.AddObject(new DerTaggedObject(explicitly: false, 1, obj));
			}
			_envGen.Close();
			_cGen.Close();
			base.Close();
		}
	}

	private object _originatorInfo = null;

	private object _unprotectedAttributes = null;

	private int _bufferSize;

	private bool _berEncodeRecipientSet;

	private DerInteger Version
	{
		get
		{
			int value = ((_originatorInfo != null || _unprotectedAttributes != null) ? 2 : 0);
			return new DerInteger(value);
		}
	}

	public CmsEnvelopedDataStreamGenerator()
	{
	}

	public CmsEnvelopedDataStreamGenerator(SecureRandom rand)
		: base(rand)
	{
	}

	public void SetBufferSize(int bufferSize)
	{
		_bufferSize = bufferSize;
	}

	public void SetBerEncodeRecipients(bool berEncodeRecipientSet)
	{
		_berEncodeRecipientSet = berEncodeRecipientSet;
	}

	private Stream Open(Stream outStream, string encryptionOid, CipherKeyGenerator keyGen)
	{
		byte[] array = keyGen.GenerateKey();
		KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter(encryptionOid, array);
		Asn1Encodable asn1Params = GenerateAsn1Parameters(encryptionOid, array);
		ICipherParameters cipherParameters;
		AlgorithmIdentifier algorithmIdentifier = GetAlgorithmIdentifier(encryptionOid, keyParameter, asn1Params, out cipherParameters);
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		foreach (RecipientInfoGenerator recipientInfoGenerator in recipientInfoGenerators)
		{
			try
			{
				asn1EncodableVector.Add(recipientInfoGenerator.Generate(keyParameter, rand));
			}
			catch (InvalidKeyException e)
			{
				throw new CmsException("key inappropriate for algorithm.", e);
			}
			catch (GeneralSecurityException e2)
			{
				throw new CmsException("error making encrypted content.", e2);
			}
		}
		return Open(outStream, algorithmIdentifier, cipherParameters, asn1EncodableVector);
	}

	private Stream Open(Stream outStream, AlgorithmIdentifier encAlgID, ICipherParameters cipherParameters, Asn1EncodableVector recipientInfos)
	{
		try
		{
			BerSequenceGenerator berSequenceGenerator = new BerSequenceGenerator(outStream);
			berSequenceGenerator.AddObject(CmsObjectIdentifiers.EnvelopedData);
			BerSequenceGenerator berSequenceGenerator2 = new BerSequenceGenerator(berSequenceGenerator.GetRawOutputStream(), 0, isExplicit: true);
			berSequenceGenerator2.AddObject(Version);
			Stream rawOutputStream = berSequenceGenerator2.GetRawOutputStream();
			Asn1Generator asn1Generator = (_berEncodeRecipientSet ? ((Asn1Generator)new BerSetGenerator(rawOutputStream)) : ((Asn1Generator)new DerSetGenerator(rawOutputStream)));
			foreach (Asn1Encodable recipientInfo in recipientInfos)
			{
				asn1Generator.AddObject(recipientInfo);
			}
			asn1Generator.Close();
			BerSequenceGenerator berSequenceGenerator3 = new BerSequenceGenerator(rawOutputStream);
			berSequenceGenerator3.AddObject(CmsObjectIdentifiers.Data);
			berSequenceGenerator3.AddObject(encAlgID);
			Stream stream = CmsUtilities.CreateBerOctetOutputStream(berSequenceGenerator3.GetRawOutputStream(), 0, isExplicit: false, _bufferSize);
			IBufferedCipher cipher = CipherUtilities.GetCipher(encAlgID.Algorithm);
			cipher.Init(forEncryption: true, new ParametersWithRandom(cipherParameters, rand));
			CipherStream outStream2 = new CipherStream(stream, null, cipher);
			return new CmsEnvelopedDataOutputStream(this, outStream2, berSequenceGenerator, berSequenceGenerator2, berSequenceGenerator3);
		}
		catch (SecurityUtilityException e)
		{
			throw new CmsException("couldn't create cipher.", e);
		}
		catch (InvalidKeyException e2)
		{
			throw new CmsException("key invalid in message.", e2);
		}
		catch (IOException e3)
		{
			throw new CmsException("exception decoding algorithm parameters.", e3);
		}
	}

	public Stream Open(Stream outStream, string encryptionOid)
	{
		CipherKeyGenerator keyGenerator = GeneratorUtilities.GetKeyGenerator(encryptionOid);
		keyGenerator.Init(new KeyGenerationParameters(rand, keyGenerator.DefaultStrength));
		return Open(outStream, encryptionOid, keyGenerator);
	}

	public Stream Open(Stream outStream, string encryptionOid, int keySize)
	{
		CipherKeyGenerator keyGenerator = GeneratorUtilities.GetKeyGenerator(encryptionOid);
		keyGenerator.Init(new KeyGenerationParameters(rand, keySize));
		return Open(outStream, encryptionOid, keyGenerator);
	}
}
