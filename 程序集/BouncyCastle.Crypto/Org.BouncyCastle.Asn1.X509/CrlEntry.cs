using System;

namespace Org.BouncyCastle.Asn1.X509;

public class CrlEntry : Asn1Encodable
{
	internal Asn1Sequence seq;

	internal DerInteger userCertificate;

	internal Time revocationDate;

	internal X509Extensions crlEntryExtensions;

	public DerInteger UserCertificate => userCertificate;

	public Time RevocationDate => revocationDate;

	public X509Extensions Extensions
	{
		get
		{
			if (crlEntryExtensions == null && seq.Count == 3)
			{
				crlEntryExtensions = X509Extensions.GetInstance(seq[2]);
			}
			return crlEntryExtensions;
		}
	}

	public CrlEntry(Asn1Sequence seq)
	{
		if (seq.Count < 2 || seq.Count > 3)
		{
			throw new ArgumentException("Bad sequence size: " + seq.Count);
		}
		this.seq = seq;
		userCertificate = DerInteger.GetInstance(seq[0]);
		revocationDate = Time.GetInstance(seq[1]);
	}

	public override Asn1Object ToAsn1Object()
	{
		return seq;
	}
}
