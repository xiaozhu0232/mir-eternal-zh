using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls;

internal class TlsServerContextImpl : AbstractTlsContext, TlsServerContext, TlsContext
{
	public override bool IsServer => true;

	internal TlsServerContextImpl(SecureRandom secureRandom, SecurityParameters securityParameters)
		: base(secureRandom, securityParameters)
	{
	}
}
