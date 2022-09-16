using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsSrpGroupVerifier
{
	bool Accept(Srp6GroupParameters group);
}
