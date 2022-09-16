using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cmp;

public class ProtectedPkiMessageBuilder
{
	private PkiHeaderBuilder hdrBuilBuilder;

	private PkiBody body;

	private IList generalInfos = Platform.CreateArrayList();

	private IList extraCerts = Platform.CreateArrayList();

	public ProtectedPkiMessageBuilder(GeneralName sender, GeneralName recipient)
		: this(PkiHeader.CMP_2000, sender, recipient)
	{
	}

	public ProtectedPkiMessageBuilder(int pvno, GeneralName sender, GeneralName recipient)
	{
		hdrBuilBuilder = new PkiHeaderBuilder(pvno, sender, recipient);
	}

	public ProtectedPkiMessageBuilder SetTransactionId(byte[] tid)
	{
		hdrBuilBuilder.SetTransactionID(tid);
		return this;
	}

	public ProtectedPkiMessageBuilder SetFreeText(PkiFreeText freeText)
	{
		hdrBuilBuilder.SetFreeText(freeText);
		return this;
	}

	public ProtectedPkiMessageBuilder AddGeneralInfo(InfoTypeAndValue genInfo)
	{
		generalInfos.Add(genInfo);
		return this;
	}

	public ProtectedPkiMessageBuilder SetMessageTime(DerGeneralizedTime generalizedTime)
	{
		hdrBuilBuilder.SetMessageTime(generalizedTime);
		return this;
	}

	public ProtectedPkiMessageBuilder SetRecipKID(byte[] id)
	{
		hdrBuilBuilder.SetRecipKID(id);
		return this;
	}

	public ProtectedPkiMessageBuilder SetRecipNonce(byte[] nonce)
	{
		hdrBuilBuilder.SetRecipNonce(nonce);
		return this;
	}

	public ProtectedPkiMessageBuilder SetSenderKID(byte[] id)
	{
		hdrBuilBuilder.SetSenderKID(id);
		return this;
	}

	public ProtectedPkiMessageBuilder SetSenderNonce(byte[] nonce)
	{
		hdrBuilBuilder.SetSenderNonce(nonce);
		return this;
	}

	public ProtectedPkiMessageBuilder SetBody(PkiBody body)
	{
		this.body = body;
		return this;
	}

	public ProtectedPkiMessageBuilder AddCmpCertificate(X509Certificate certificate)
	{
		extraCerts.Add(certificate);
		return this;
	}

	public ProtectedPkiMessage Build(ISignatureFactory signatureFactory)
	{
		if (body == null)
		{
			throw new InvalidOperationException("body must be set before building");
		}
		IStreamCalculator signer = signatureFactory.CreateCalculator();
		if (!(signatureFactory.AlgorithmDetails is AlgorithmIdentifier))
		{
			throw new ArgumentException("AlgorithmDetails is not AlgorithmIdentifier");
		}
		FinalizeHeader((AlgorithmIdentifier)signatureFactory.AlgorithmDetails);
		PkiHeader header = hdrBuilBuilder.Build();
		DerBitString protection = new DerBitString(CalculateSignature(signer, header, body));
		return FinalizeMessage(header, protection);
	}

	public ProtectedPkiMessage Build(IMacFactory factory)
	{
		if (body == null)
		{
			throw new InvalidOperationException("body must be set before building");
		}
		IStreamCalculator signer = factory.CreateCalculator();
		FinalizeHeader((AlgorithmIdentifier)factory.AlgorithmDetails);
		PkiHeader header = hdrBuilBuilder.Build();
		DerBitString protection = new DerBitString(CalculateSignature(signer, header, body));
		return FinalizeMessage(header, protection);
	}

	private void FinalizeHeader(AlgorithmIdentifier algorithmIdentifier)
	{
		hdrBuilBuilder.SetProtectionAlg(algorithmIdentifier);
		if (generalInfos.Count > 0)
		{
			InfoTypeAndValue[] array = new InfoTypeAndValue[generalInfos.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (InfoTypeAndValue)generalInfos[i];
			}
			hdrBuilBuilder.SetGeneralInfo(array);
		}
	}

	private ProtectedPkiMessage FinalizeMessage(PkiHeader header, DerBitString protection)
	{
		if (extraCerts.Count > 0)
		{
			CmpCertificate[] array = new CmpCertificate[extraCerts.Count];
			for (int i = 0; i < array.Length; i++)
			{
				byte[] encoded = ((X509Certificate)extraCerts[i]).GetEncoded();
				array[i] = CmpCertificate.GetInstance(Asn1Object.FromByteArray(encoded));
			}
			return new ProtectedPkiMessage(new PkiMessage(header, body, protection, array));
		}
		return new ProtectedPkiMessage(new PkiMessage(header, body, protection));
	}

	private byte[] CalculateSignature(IStreamCalculator signer, PkiHeader header, PkiBody body)
	{
		Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
		asn1EncodableVector.Add(header);
		asn1EncodableVector.Add(body);
		byte[] encoded = new DerSequence(asn1EncodableVector).GetEncoded();
		signer.Stream.Write(encoded, 0, encoded.Length);
		object result = signer.GetResult();
		if (result is DefaultSignatureResult)
		{
			return ((DefaultSignatureResult)result).Collect();
		}
		if (result is IBlockResult)
		{
			return ((IBlockResult)result).Collect();
		}
		if (result is byte[])
		{
			return (byte[])result;
		}
		throw new InvalidOperationException("result is not byte[] or DefaultSignatureResult");
	}
}
