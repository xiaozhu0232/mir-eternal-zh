namespace Org.BouncyCastle.Crypto.Tls;

public abstract class DefaultTlsClient : AbstractTlsClient
{
	protected TlsDHVerifier mDHVerifier;

	public DefaultTlsClient()
		: this(new DefaultTlsCipherFactory())
	{
	}

	public DefaultTlsClient(TlsCipherFactory cipherFactory)
		: this(cipherFactory, new DefaultTlsDHVerifier())
	{
	}

	public DefaultTlsClient(TlsCipherFactory cipherFactory, TlsDHVerifier dhVerifier)
		: base(cipherFactory)
	{
		mDHVerifier = dhVerifier;
	}

	public override int[] GetCipherSuites()
	{
		return new int[9] { 49195, 49187, 49161, 49199, 49191, 49171, 156, 60, 47 };
	}

	public override TlsKeyExchange GetKeyExchange()
	{
		int keyExchangeAlgorithm = TlsUtilities.GetKeyExchangeAlgorithm(mSelectedCipherSuite);
		switch (keyExchangeAlgorithm)
		{
		case 7:
		case 9:
		case 11:
			return CreateDHKeyExchange(keyExchangeAlgorithm);
		case 3:
		case 5:
			return CreateDheKeyExchange(keyExchangeAlgorithm);
		case 16:
		case 18:
		case 20:
			return CreateECDHKeyExchange(keyExchangeAlgorithm);
		case 17:
		case 19:
			return CreateECDheKeyExchange(keyExchangeAlgorithm);
		case 1:
			return CreateRsaKeyExchange();
		default:
			throw new TlsFatalAlert(80);
		}
	}

	protected virtual TlsKeyExchange CreateDHKeyExchange(int keyExchange)
	{
		return new TlsDHKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mDHVerifier, null);
	}

	protected virtual TlsKeyExchange CreateDheKeyExchange(int keyExchange)
	{
		return new TlsDheKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mDHVerifier, null);
	}

	protected virtual TlsKeyExchange CreateECDHKeyExchange(int keyExchange)
	{
		return new TlsECDHKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mNamedCurves, mClientECPointFormats, mServerECPointFormats);
	}

	protected virtual TlsKeyExchange CreateECDheKeyExchange(int keyExchange)
	{
		return new TlsECDheKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mNamedCurves, mClientECPointFormats, mServerECPointFormats);
	}

	protected virtual TlsKeyExchange CreateRsaKeyExchange()
	{
		return new TlsRsaKeyExchange(mSupportedSignatureAlgorithms);
	}
}
