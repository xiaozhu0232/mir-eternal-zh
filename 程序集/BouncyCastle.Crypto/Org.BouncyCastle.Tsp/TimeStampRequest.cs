using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Tsp;

public class TimeStampRequest : X509ExtensionBase
{
	private TimeStampReq req;

	private X509Extensions extensions;

	public int Version => req.Version.IntValueExact;

	public string MessageImprintAlgOid => req.MessageImprint.HashAlgorithm.Algorithm.Id;

	public string ReqPolicy
	{
		get
		{
			if (req.ReqPolicy != null)
			{
				return req.ReqPolicy.Id;
			}
			return null;
		}
	}

	public BigInteger Nonce
	{
		get
		{
			if (req.Nonce != null)
			{
				return req.Nonce.Value;
			}
			return null;
		}
	}

	public bool CertReq
	{
		get
		{
			if (req.CertReq != null)
			{
				return req.CertReq.IsTrue;
			}
			return false;
		}
	}

	internal X509Extensions Extensions => req.Extensions;

	public virtual bool HasExtensions => extensions != null;

	public TimeStampRequest(TimeStampReq req)
	{
		this.req = req;
		extensions = req.Extensions;
	}

	public TimeStampRequest(byte[] req)
		: this(new Asn1InputStream(req))
	{
	}

	public TimeStampRequest(Stream input)
		: this(new Asn1InputStream(input))
	{
	}

	private TimeStampRequest(Asn1InputStream str)
	{
		try
		{
			req = TimeStampReq.GetInstance(str.ReadObject());
		}
		catch (InvalidCastException ex)
		{
			throw new IOException("malformed request: " + ex);
		}
		catch (ArgumentException ex2)
		{
			throw new IOException("malformed request: " + ex2);
		}
	}

	public byte[] GetMessageImprintDigest()
	{
		return req.MessageImprint.GetHashedMessage();
	}

	public void Validate(IList algorithms, IList policies, IList extensions)
	{
		if (!algorithms.Contains(MessageImprintAlgOid))
		{
			throw new TspValidationException("request contains unknown algorithm", 128);
		}
		if (policies != null && ReqPolicy != null && !policies.Contains(ReqPolicy))
		{
			throw new TspValidationException("request contains unknown policy", 256);
		}
		if (Extensions != null && extensions != null)
		{
			foreach (DerObjectIdentifier extensionOid in Extensions.ExtensionOids)
			{
				if (!extensions.Contains(extensionOid.Id))
				{
					throw new TspValidationException("request contains unknown extension", 8388608);
				}
			}
		}
		int digestLength = TspUtil.GetDigestLength(MessageImprintAlgOid);
		if (digestLength != GetMessageImprintDigest().Length)
		{
			throw new TspValidationException("imprint digest the wrong length", 4);
		}
	}

	public byte[] GetEncoded()
	{
		return req.GetEncoded();
	}

	public virtual X509Extension GetExtension(DerObjectIdentifier oid)
	{
		if (extensions != null)
		{
			return extensions.GetExtension(oid);
		}
		return null;
	}

	public virtual IList GetExtensionOids()
	{
		return TspUtil.GetExtensionOids(extensions);
	}

	protected override X509Extensions GetX509Extensions()
	{
		return Extensions;
	}
}
