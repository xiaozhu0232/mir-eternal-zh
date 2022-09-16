namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsSrpIdentityManager
{
	TlsSrpLoginParameters GetLoginParameters(byte[] identity);
}
