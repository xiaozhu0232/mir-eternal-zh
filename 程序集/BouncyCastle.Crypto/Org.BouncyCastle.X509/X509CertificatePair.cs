using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;

namespace Org.BouncyCastle.X509;

public class X509CertificatePair
{
	private readonly X509Certificate forward;

	private readonly X509Certificate reverse;

	public X509Certificate Forward => forward;

	public X509Certificate Reverse => reverse;

	public X509CertificatePair(X509Certificate forward, X509Certificate reverse)
	{
		this.forward = forward;
		this.reverse = reverse;
	}

	public X509CertificatePair(CertificatePair pair)
	{
		if (pair.Forward != null)
		{
			forward = new X509Certificate(pair.Forward);
		}
		if (pair.Reverse != null)
		{
			reverse = new X509Certificate(pair.Reverse);
		}
	}

	public byte[] GetEncoded()
	{
		try
		{
			X509CertificateStructure x509CertificateStructure = null;
			X509CertificateStructure x509CertificateStructure2 = null;
			if (forward != null)
			{
				x509CertificateStructure = X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(forward.GetEncoded()));
				if (x509CertificateStructure == null)
				{
					throw new CertificateEncodingException("unable to get encoding for forward");
				}
			}
			if (reverse != null)
			{
				x509CertificateStructure2 = X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(reverse.GetEncoded()));
				if (x509CertificateStructure2 == null)
				{
					throw new CertificateEncodingException("unable to get encoding for reverse");
				}
			}
			return new CertificatePair(x509CertificateStructure, x509CertificateStructure2).GetDerEncoded();
		}
		catch (Exception ex)
		{
			throw new CertificateEncodingException(ex.Message, ex);
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is X509CertificatePair x509CertificatePair))
		{
			return false;
		}
		if (object.Equals(forward, x509CertificatePair.forward))
		{
			return object.Equals(reverse, x509CertificatePair.reverse);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = -1;
		if (forward != null)
		{
			num ^= forward.GetHashCode();
		}
		if (reverse != null)
		{
			num *= 17;
			num ^= reverse.GetHashCode();
		}
		return num;
	}
}
