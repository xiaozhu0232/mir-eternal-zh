using System;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkix;

public class TrustAnchor
{
	private readonly AsymmetricKeyParameter pubKey;

	private readonly string caName;

	private readonly X509Name caPrincipal;

	private readonly X509Certificate trustedCert;

	private byte[] ncBytes;

	private NameConstraints nc;

	public X509Certificate TrustedCert => trustedCert;

	public X509Name CA => caPrincipal;

	public string CAName => caName;

	public AsymmetricKeyParameter CAPublicKey => pubKey;

	public byte[] GetNameConstraints => Arrays.Clone(ncBytes);

	public TrustAnchor(X509Certificate trustedCert, byte[] nameConstraints)
	{
		if (trustedCert == null)
		{
			throw new ArgumentNullException("trustedCert");
		}
		this.trustedCert = trustedCert;
		pubKey = null;
		caName = null;
		caPrincipal = null;
		setNameConstraints(nameConstraints);
	}

	public TrustAnchor(X509Name caPrincipal, AsymmetricKeyParameter pubKey, byte[] nameConstraints)
	{
		if (caPrincipal == null)
		{
			throw new ArgumentNullException("caPrincipal");
		}
		if (pubKey == null)
		{
			throw new ArgumentNullException("pubKey");
		}
		trustedCert = null;
		this.caPrincipal = caPrincipal;
		caName = caPrincipal.ToString();
		this.pubKey = pubKey;
		setNameConstraints(nameConstraints);
	}

	public TrustAnchor(string caName, AsymmetricKeyParameter pubKey, byte[] nameConstraints)
	{
		if (caName == null)
		{
			throw new ArgumentNullException("caName");
		}
		if (pubKey == null)
		{
			throw new ArgumentNullException("pubKey");
		}
		if (caName.Length == 0)
		{
			throw new ArgumentException("caName can not be an empty string");
		}
		caPrincipal = new X509Name(caName);
		this.pubKey = pubKey;
		this.caName = caName;
		trustedCert = null;
		setNameConstraints(nameConstraints);
	}

	private void setNameConstraints(byte[] bytes)
	{
		if (bytes == null)
		{
			ncBytes = null;
			nc = null;
		}
		else
		{
			ncBytes = (byte[])bytes.Clone();
			nc = NameConstraints.GetInstance(Asn1Object.FromByteArray(bytes));
		}
	}

	public override string ToString()
	{
		string newLine = Platform.NewLine;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append(newLine);
		if (pubKey != null)
		{
			stringBuilder.Append("  Trusted CA Public Key: ").Append(pubKey).Append(newLine);
			stringBuilder.Append("  Trusted CA Issuer Name: ").Append(caName).Append(newLine);
		}
		else
		{
			stringBuilder.Append("  Trusted CA cert: ").Append(TrustedCert).Append(newLine);
		}
		if (nc != null)
		{
			stringBuilder.Append("  Name Constraints: ").Append(nc).Append(newLine);
		}
		return stringBuilder.ToString();
	}
}
