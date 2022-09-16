using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms;

public interface RecipientInfoGenerator
{
	RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random);
}
