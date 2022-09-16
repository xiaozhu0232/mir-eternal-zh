using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms;

public class CmsAuthenticatedGenerator : CmsEnvelopedGenerator
{
	public CmsAuthenticatedGenerator()
	{
	}

	public CmsAuthenticatedGenerator(SecureRandom rand)
		: base(rand)
	{
	}
}
