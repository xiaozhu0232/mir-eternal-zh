using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Operators;

public class Asn1DigestFactory : IDigestFactory
{
	private readonly IDigest mDigest;

	private readonly DerObjectIdentifier mOid;

	public virtual object AlgorithmDetails => new AlgorithmIdentifier(mOid);

	public virtual int DigestLength => mDigest.GetDigestSize();

	public static Asn1DigestFactory Get(DerObjectIdentifier oid)
	{
		return new Asn1DigestFactory(DigestUtilities.GetDigest(oid), oid);
	}

	public static Asn1DigestFactory Get(string mechanism)
	{
		DerObjectIdentifier objectIdentifier = DigestUtilities.GetObjectIdentifier(mechanism);
		return new Asn1DigestFactory(DigestUtilities.GetDigest(objectIdentifier), objectIdentifier);
	}

	public Asn1DigestFactory(IDigest digest, DerObjectIdentifier oid)
	{
		mDigest = digest;
		mOid = oid;
	}

	public virtual IStreamCalculator CreateCalculator()
	{
		return new DfDigestStream(mDigest);
	}
}
