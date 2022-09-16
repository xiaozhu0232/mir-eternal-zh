using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Cmp;

public class RevocationDetails
{
	private readonly RevDetails revDetails;

	public X509Name Subject => revDetails.CertDetails.Subject;

	public X509Name Issuer => revDetails.CertDetails.Issuer;

	public BigInteger SerialNumber => revDetails.CertDetails.SerialNumber.Value;

	public RevocationDetails(RevDetails revDetails)
	{
		this.revDetails = revDetails;
	}

	public RevDetails ToASN1Structure()
	{
		return revDetails;
	}
}
