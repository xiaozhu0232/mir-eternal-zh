using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsDHVerifier
{
	bool Accept(DHParameters dhParameters);
}
