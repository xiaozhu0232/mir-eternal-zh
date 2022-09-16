using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Ocsp;

public class BasicOcspResp : X509ExtensionBase
{
	private readonly BasicOcspResponse resp;

	private readonly ResponseData data;

	public int Version => data.Version.IntValueExact + 1;

	public RespID ResponderId => new RespID(data.ResponderID);

	public DateTime ProducedAt => data.ProducedAt.ToDateTime();

	public SingleResp[] Responses
	{
		get
		{
			Asn1Sequence responses = data.Responses;
			SingleResp[] array = new SingleResp[responses.Count];
			for (int i = 0; i != array.Length; i++)
			{
				array[i] = new SingleResp(SingleResponse.GetInstance(responses[i]));
			}
			return array;
		}
	}

	public X509Extensions ResponseExtensions => data.ResponseExtensions;

	public string SignatureAlgName => OcspUtilities.GetAlgorithmName(resp.SignatureAlgorithm.Algorithm);

	public string SignatureAlgOid => resp.SignatureAlgorithm.Algorithm.Id;

	public BasicOcspResp(BasicOcspResponse resp)
	{
		this.resp = resp;
		data = resp.TbsResponseData;
	}

	public byte[] GetTbsResponseData()
	{
		try
		{
			return data.GetDerEncoded();
		}
		catch (IOException e)
		{
			throw new OcspException("problem encoding tbsResponseData", e);
		}
	}

	protected override X509Extensions GetX509Extensions()
	{
		return ResponseExtensions;
	}

	[Obsolete("RespData class is no longer required as all functionality is available on this class")]
	public RespData GetResponseData()
	{
		return new RespData(data);
	}

	public byte[] GetSignature()
	{
		return resp.GetSignatureOctets();
	}

	private IList GetCertList()
	{
		IList list = Platform.CreateArrayList();
		Asn1Sequence certs = resp.Certs;
		if (certs != null)
		{
			foreach (Asn1Encodable item in certs)
			{
				try
				{
					list.Add(new X509CertificateParser().ReadCertificate(item.GetEncoded()));
				}
				catch (IOException e)
				{
					throw new OcspException("can't re-encode certificate!", e);
				}
				catch (CertificateException e2)
				{
					throw new OcspException("can't re-encode certificate!", e2);
				}
			}
			return list;
		}
		return list;
	}

	public X509Certificate[] GetCerts()
	{
		IList certList = GetCertList();
		X509Certificate[] array = new X509Certificate[certList.Count];
		for (int i = 0; i < certList.Count; i++)
		{
			array[i] = (X509Certificate)certList[i];
		}
		return array;
	}

	public IX509Store GetCertificates(string type)
	{
		try
		{
			return X509StoreFactory.Create("Certificate/" + type, new X509CollectionStoreParameters(GetCertList()));
		}
		catch (Exception e)
		{
			throw new OcspException("can't setup the CertStore", e);
		}
	}

	public bool Verify(AsymmetricKeyParameter publicKey)
	{
		try
		{
			ISigner signer = SignerUtilities.GetSigner(SignatureAlgName);
			signer.Init(forSigning: false, publicKey);
			byte[] derEncoded = data.GetDerEncoded();
			signer.BlockUpdate(derEncoded, 0, derEncoded.Length);
			return signer.VerifySignature(GetSignature());
		}
		catch (Exception ex)
		{
			throw new OcspException("exception processing sig: " + ex, ex);
		}
	}

	public byte[] GetEncoded()
	{
		return resp.GetEncoded();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is BasicOcspResp basicOcspResp))
		{
			return false;
		}
		return resp.Equals(basicOcspResp.resp);
	}

	public override int GetHashCode()
	{
		return resp.GetHashCode();
	}
}
