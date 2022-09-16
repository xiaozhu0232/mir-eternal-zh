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

public class CmsAuthenticatedDataStreamGenerator : CmsAuthenticatedGenerator
{
	private class CmsAuthenticatedDataOutputStream : BaseOutputStream
	{
		private readonly Stream macStream;

		private readonly IMac mac;

		private readonly BerSequenceGenerator cGen;

		private readonly BerSequenceGenerator authGen;

		private readonly BerSequenceGenerator eiGen;

		public CmsAuthenticatedDataOutputStream(Stream macStream, IMac mac, BerSequenceGenerator cGen, BerSequenceGenerator authGen, BerSequenceGenerator eiGen)
		{
			this.macStream = macStream;
			this.mac = mac;
			this.cGen = cGen;
			this.authGen = authGen;
			this.eiGen = eiGen;
		}

		public override void WriteByte(byte b)
		{
			macStream.WriteByte(b);
		}

		public override void Write(byte[] bytes, int off, int len)
		{
			macStream.Write(bytes, off, len);
		}

		public override void Close()
		{
			Platform.Dispose(macStream);
			eiGen.Close();
			byte[] str = MacUtilities.DoFinal(mac);
			authGen.AddObject(new DerOctetString(str));
			authGen.Close();
			cGen.Close();
			base.Close();
		}
	}

	private int _bufferSize;

	private bool _berEncodeRecipientSet;

	public CmsAuthenticatedDataStreamGenerator()
	{
	}

	public CmsAuthenticatedDataStreamGenerator(SecureRandom rand)
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

	private Stream Open(Stream outStr, string macOid, CipherKeyGenerator keyGen)
	{
		byte[] array = keyGen.GenerateKey();
		KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter(macOid, array);
		Asn1Encodable asn1Params = GenerateAsn1Parameters(macOid, array);
		ICipherParameters cipherParameters;
		AlgorithmIdentifier algorithmIdentifier = GetAlgorithmIdentifier(macOid, keyParameter, asn1Params, out cipherParameters);
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
		return Open(outStr, algorithmIdentifier, keyParameter, asn1EncodableVector);
	}

	protected Stream Open(Stream outStr, AlgorithmIdentifier macAlgId, ICipherParameters cipherParameters, Asn1EncodableVector recipientInfos)
	{
		try
		{
			BerSequenceGenerator berSequenceGenerator = new BerSequenceGenerator(outStr);
			berSequenceGenerator.AddObject(CmsObjectIdentifiers.AuthenticatedData);
			BerSequenceGenerator berSequenceGenerator2 = new BerSequenceGenerator(berSequenceGenerator.GetRawOutputStream(), 0, isExplicit: true);
			berSequenceGenerator2.AddObject(new DerInteger(AuthenticatedData.CalculateVersion(null)));
			Stream rawOutputStream = berSequenceGenerator2.GetRawOutputStream();
			Asn1Generator asn1Generator = (_berEncodeRecipientSet ? ((Asn1Generator)new BerSetGenerator(rawOutputStream)) : ((Asn1Generator)new DerSetGenerator(rawOutputStream)));
			foreach (Asn1Encodable recipientInfo in recipientInfos)
			{
				asn1Generator.AddObject(recipientInfo);
			}
			asn1Generator.Close();
			berSequenceGenerator2.AddObject(macAlgId);
			BerSequenceGenerator berSequenceGenerator3 = new BerSequenceGenerator(rawOutputStream);
			berSequenceGenerator3.AddObject(CmsObjectIdentifiers.Data);
			Stream output = CmsUtilities.CreateBerOctetOutputStream(berSequenceGenerator3.GetRawOutputStream(), 0, isExplicit: false, _bufferSize);
			IMac mac = MacUtilities.GetMac(macAlgId.Algorithm);
			mac.Init(cipherParameters);
			Stream macStream = new TeeOutputStream(output, new MacSink(mac));
			return new CmsAuthenticatedDataOutputStream(macStream, mac, berSequenceGenerator, berSequenceGenerator2, berSequenceGenerator3);
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

	public Stream Open(Stream outStr, string encryptionOid)
	{
		CipherKeyGenerator keyGenerator = GeneratorUtilities.GetKeyGenerator(encryptionOid);
		keyGenerator.Init(new KeyGenerationParameters(rand, keyGenerator.DefaultStrength));
		return Open(outStr, encryptionOid, keyGenerator);
	}

	public Stream Open(Stream outStr, string encryptionOid, int keySize)
	{
		CipherKeyGenerator keyGenerator = GeneratorUtilities.GetKeyGenerator(encryptionOid);
		keyGenerator.Init(new KeyGenerationParameters(rand, keySize));
		return Open(outStr, encryptionOid, keyGenerator);
	}
}
