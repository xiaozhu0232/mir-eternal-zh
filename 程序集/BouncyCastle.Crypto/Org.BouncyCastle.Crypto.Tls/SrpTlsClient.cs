using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class SrpTlsClient : AbstractTlsClient
{
	protected TlsSrpGroupVerifier mGroupVerifier;

	protected byte[] mIdentity;

	protected byte[] mPassword;

	protected virtual bool RequireSrpServerExtension => false;

	public SrpTlsClient(byte[] identity, byte[] password)
		: this(new DefaultTlsCipherFactory(), new DefaultTlsSrpGroupVerifier(), identity, password)
	{
	}

	public SrpTlsClient(TlsCipherFactory cipherFactory, byte[] identity, byte[] password)
		: this(cipherFactory, new DefaultTlsSrpGroupVerifier(), identity, password)
	{
	}

	public SrpTlsClient(TlsCipherFactory cipherFactory, TlsSrpGroupVerifier groupVerifier, byte[] identity, byte[] password)
		: base(cipherFactory)
	{
		mGroupVerifier = groupVerifier;
		mIdentity = Arrays.Clone(identity);
		mPassword = Arrays.Clone(password);
	}

	public override int[] GetCipherSuites()
	{
		return new int[1] { 49182 };
	}

	public override IDictionary GetClientExtensions()
	{
		IDictionary dictionary = TlsExtensionsUtilities.EnsureExtensionsInitialised(base.GetClientExtensions());
		TlsSrpUtilities.AddSrpExtension(dictionary, mIdentity);
		return dictionary;
	}

	public override void ProcessServerExtensions(IDictionary serverExtensions)
	{
		if (!TlsUtilities.HasExpectedEmptyExtensionData(serverExtensions, 12, 47) && RequireSrpServerExtension)
		{
			throw new TlsFatalAlert(47);
		}
		base.ProcessServerExtensions(serverExtensions);
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

	public override TlsAuthentication GetAuthentication()
	{
		throw new TlsFatalAlert(80);
	}

	protected virtual TlsKeyExchange CreateSrpKeyExchange(int keyExchange)
	{
		return new TlsSrpKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mGroupVerifier, mIdentity, mPassword);
	}
}
