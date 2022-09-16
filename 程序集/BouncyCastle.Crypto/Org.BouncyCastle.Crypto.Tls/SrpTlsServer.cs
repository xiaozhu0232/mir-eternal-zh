using System.Collections;

namespace Org.BouncyCastle.Crypto.Tls;

public class SrpTlsServer : AbstractTlsServer
{
	protected TlsSrpIdentityManager mSrpIdentityManager;

	protected byte[] mSrpIdentity = null;

	protected TlsSrpLoginParameters mLoginParameters = null;

	public SrpTlsServer(TlsSrpIdentityManager srpIdentityManager)
		: this(new DefaultTlsCipherFactory(), srpIdentityManager)
	{
	}

	public SrpTlsServer(TlsCipherFactory cipherFactory, TlsSrpIdentityManager srpIdentityManager)
		: base(cipherFactory)
	{
		mSrpIdentityManager = srpIdentityManager;
	}

	protected virtual TlsSignerCredentials GetDsaSignerCredentials()
	{
		throw new TlsFatalAlert(80);
	}

	protected virtual TlsSignerCredentials GetRsaSignerCredentials()
	{
		throw new TlsFatalAlert(80);
	}

	protected override int[] GetCipherSuites()
	{
		return new int[6] { 49186, 49183, 49185, 49182, 49184, 49181 };
	}

	public override void ProcessClientExtensions(IDictionary clientExtensions)
	{
		base.ProcessClientExtensions(clientExtensions);
		mSrpIdentity = TlsSrpUtilities.GetSrpExtension(clientExtensions);
	}

	public override int GetSelectedCipherSuite()
	{
		int selectedCipherSuite = base.GetSelectedCipherSuite();
		if (TlsSrpUtilities.IsSrpCipherSuite(selectedCipherSuite))
		{
			if (mSrpIdentity != null)
			{
				mLoginParameters = mSrpIdentityManager.GetLoginParameters(mSrpIdentity);
			}
			if (mLoginParameters == null)
			{
				throw new TlsFatalAlert(115);
			}
		}
		return selectedCipherSuite;
	}

	public override TlsCredentials GetCredentials()
	{
		return TlsUtilities.GetKeyExchangeAlgorithm(mSelectedCipherSuite) switch
		{
			21 => null, 
			22 => GetDsaSignerCredentials(), 
			23 => GetRsaSignerCredentials(), 
			_ => throw new TlsFatalAlert(80), 
		};
	}

	public override TlsKeyExchange GetKeyExchange()
	{
		int keyExchangeAlgorithm = TlsUtilities.GetKeyExchangeAlgorithm(mSelectedCipherSuite);
		switch (keyExchangeAlgorithm)
		{
		case 21:
		case 22:
		case 23:
			return CreateSrpKeyExchange(keyExchangeAlgorithm);
		default:
			throw new TlsFatalAlert(80);
		}
	}

	protected virtual TlsKeyExchange CreateSrpKeyExchange(int keyExchange)
	{
		return new TlsSrpKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mSrpIdentity, mLoginParameters);
	}
}
