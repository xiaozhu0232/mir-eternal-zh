using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Crypto.Operators;

internal class RsaOaepWrapperProvider : WrapperProvider
{
	private readonly DerObjectIdentifier digestOid;

	internal RsaOaepWrapperProvider(DerObjectIdentifier digestOid)
	{
		this.digestOid = digestOid;
	}

	object WrapperProvider.CreateWrapper(bool forWrapping, ICipherParameters parameters)
	{
		return new RsaOaepWrapper(forWrapping, parameters, digestOid);
	}
}
