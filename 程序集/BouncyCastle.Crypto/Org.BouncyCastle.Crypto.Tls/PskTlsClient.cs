namespace Org.BouncyCastle.Crypto.Tls;

public class PskTlsClient : AbstractTlsClient
{
	protected TlsDHVerifier mDHVerifier;

	protected TlsPskIdentity mPskIdentity;

	public PskTlsClient(TlsPskIdentity pskIdentity)
		: this(new DefaultTlsCipherFactory(), pskIdentity)
	{
	}

	public PskTlsClient(TlsCipherFactory cipherFactory, TlsPskIdentity pskIdentity)
		: this(cipherFactory, new DefaultTlsDHVerifier(), pskIdentity)
	{
	}

	public PskTlsClient(TlsCipherFactory cipherFactory, TlsDHVerifier dhVerifier, TlsPskIdentity pskIdentity)
		: base(cipherFactory)
	{
		mDHVerifier = dhVerifier;
		mPskIdentity = pskIdentity;
	}

	public override int[] GetCipherSuites()
	{
		return new int[2] { 49207, 49205 };
	}

	public override TlsKeyExchange GetKeyExchange()
	{
		int keyExchangeAlgorithm = TlsUtilities.GetKeyExchangeAlgorithm(mSelectedCipherSuite);
		switch (keyExchangeAlgorithm)
		{
		case 13:
		case 14:
		case 15:
		case 24:
			return CreatePskKeyExchange(keyExchangeAlgorithm);
		default:
			throw new TlsFatalAlert(80);
		}
	}

	public override TlsAuthentication GetAuthentication()
	{
		throw new TlsFatalAlert(80);
	}

	protected virtual TlsKeyExchange CreatePskKeyExchange(int keyExchange)
	{
		return new TlsPskKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mPskIdentity, null, mDHVerifier, null, mNamedCurves, mClientECPointFormats, mServerECPointFormats);
	}
}
