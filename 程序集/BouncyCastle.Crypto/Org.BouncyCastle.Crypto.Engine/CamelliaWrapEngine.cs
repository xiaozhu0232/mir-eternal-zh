namespace Org.BouncyCastle.Crypto.Engines;

public class CamelliaWrapEngine : Rfc3394WrapEngine
{
	public CamelliaWrapEngine()
		: base(new CamelliaEngine())
	{
	}
}
