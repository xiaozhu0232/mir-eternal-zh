using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class AbstractTlsServer : AbstractTlsPeer, TlsServer, TlsPeer
{
	protected TlsCipherFactory mCipherFactory;

	protected TlsServerContext mContext;

	protected ProtocolVersion mClientVersion;

	protected int[] mOfferedCipherSuites;

	protected byte[] mOfferedCompressionMethods;

	protected IDictionary mClientExtensions;

	protected bool mEncryptThenMacOffered;

	protected short mMaxFragmentLengthOffered;

	protected bool mTruncatedHMacOffered;

	protected IList mSupportedSignatureAlgorithms;

	protected bool mEccCipherSuitesOffered;

	protected int[] mNamedCurves;

	protected byte[] mClientECPointFormats;

	protected byte[] mServerECPointFormats;

	protected ProtocolVersion mServerVersion;

	protected int mSelectedCipherSuite;

	protected byte mSelectedCompressionMethod;

	protected IDictionary mServerExtensions;

	protected virtual bool AllowEncryptThenMac => true;

	protected virtual bool AllowTruncatedHMac => false;

	protected virtual ProtocolVersion MaximumVersion => ProtocolVersion.TLSv11;

	protected virtual ProtocolVersion MinimumVersion => ProtocolVersion.TLSv10;

	public AbstractTlsServer()
		: this(new DefaultTlsCipherFactory())
	{
	}

	public AbstractTlsServer(TlsCipherFactory cipherFactory)
	{
		mCipherFactory = cipherFactory;
	}

	protected virtual IDictionary CheckServerExtensions()
	{
		return mServerExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(mServerExtensions);
	}

	protected abstract int[] GetCipherSuites();

	protected byte[] GetCompressionMethods()
	{
		return new byte[1];
	}

	protected virtual bool SupportsClientEccCapabilities(int[] namedCurves, byte[] ecPointFormats)
	{
		if (namedCurves == null)
		{
			return TlsEccUtilities.HasAnySupportedNamedCurves();
		}
		foreach (int namedCurve in namedCurves)
		{
			if (NamedCurve.IsValid(namedCurve) && (!NamedCurve.RefersToASpecificNamedCurve(namedCurve) || TlsEccUtilities.IsSupportedNamedCurve(namedCurve)))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void Init(TlsServerContext context)
	{
		mContext = context;
	}

	public virtual void NotifyClientVersion(ProtocolVersion clientVersion)
	{
		mClientVersion = clientVersion;
	}

	public virtual void NotifyFallback(bool isFallback)
	{
		if (isFallback && MaximumVersion.IsLaterVersionOf(mClientVersion))
		{
			throw new TlsFatalAlert(86);
		}
	}

	public virtual void NotifyOfferedCipherSuites(int[] offeredCipherSuites)
	{
		mOfferedCipherSuites = offeredCipherSuites;
		mEccCipherSuitesOffered = TlsEccUtilities.ContainsEccCipherSuites(mOfferedCipherSuites);
	}

	public virtual void NotifyOfferedCompressionMethods(byte[] offeredCompressionMethods)
	{
		mOfferedCompressionMethods = offeredCompressionMethods;
	}

	public virtual void ProcessClientExtensions(IDictionary clientExtensions)
	{
		mClientExtensions = clientExtensions;
		if (clientExtensions != null)
		{
			mEncryptThenMacOffered = TlsExtensionsUtilities.HasEncryptThenMacExtension(clientExtensions);
			mMaxFragmentLengthOffered = TlsExtensionsUtilities.GetMaxFragmentLengthExtension(clientExtensions);
			if (mMaxFragmentLengthOffered >= 0 && !MaxFragmentLength.IsValid((byte)mMaxFragmentLengthOffered))
			{
				throw new TlsFatalAlert(47);
			}
			mTruncatedHMacOffered = TlsExtensionsUtilities.HasTruncatedHMacExtension(clientExtensions);
			mSupportedSignatureAlgorithms = TlsUtilities.GetSignatureAlgorithmsExtension(clientExtensions);
			if (mSupportedSignatureAlgorithms != null && !TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(mClientVersion))
			{
				throw new TlsFatalAlert(47);
			}
			mNamedCurves = TlsEccUtilities.GetSupportedEllipticCurvesExtension(clientExtensions);
			mClientECPointFormats = TlsEccUtilities.GetSupportedPointFormatsExtension(clientExtensions);
		}
	}

	public virtual ProtocolVersion GetServerVersion()
	{
		if (MinimumVersion.IsEqualOrEarlierVersionOf(mClientVersion))
		{
			ProtocolVersion maximumVersion = MaximumVersion;
			if (mClientVersion.IsEqualOrEarlierVersionOf(maximumVersion))
			{
				return mServerVersion = mClientVersion;
			}
			if (mClientVersion.IsLaterVersionOf(maximumVersion))
			{
				return mServerVersion = maximumVersion;
			}
		}
		throw new TlsFatalAlert(70);
	}

	public virtual int GetSelectedCipherSuite()
	{
		IList usableSignatureAlgorithms = TlsUtilities.GetUsableSignatureAlgorithms(mSupportedSignatureAlgorithms);
		bool flag = SupportsClientEccCapabilities(mNamedCurves, mClientECPointFormats);
		int[] cipherSuites = GetCipherSuites();
		foreach (int num in cipherSuites)
		{
			if (Arrays.Contains(mOfferedCipherSuites, num) && (flag || !TlsEccUtilities.IsEccCipherSuite(num)) && TlsUtilities.IsValidCipherSuiteForVersion(num, mServerVersion) && TlsUtilities.IsValidCipherSuiteForSignatureAlgorithms(num, usableSignatureAlgorithms))
			{
				return mSelectedCipherSuite = num;
			}
		}
		throw new TlsFatalAlert(40);
	}

	public virtual byte GetSelectedCompressionMethod()
	{
		byte[] compressionMethods = GetCompressionMethods();
		for (int i = 0; i < compressionMethods.Length; i++)
		{
			if (Arrays.Contains(mOfferedCompressionMethods, compressionMethods[i]))
			{
				return mSelectedCompressionMethod = compressionMethods[i];
			}
		}
		throw new TlsFatalAlert(40);
	}

	public virtual IDictionary GetServerExtensions()
	{
		if (mEncryptThenMacOffered && AllowEncryptThenMac && TlsUtilities.IsBlockCipherSuite(mSelectedCipherSuite))
		{
			TlsExtensionsUtilities.AddEncryptThenMacExtension(CheckServerExtensions());
		}
		if (mMaxFragmentLengthOffered >= 0 && TlsUtilities.IsValidUint8(mMaxFragmentLengthOffered) && MaxFragmentLength.IsValid((byte)mMaxFragmentLengthOffered))
		{
			TlsExtensionsUtilities.AddMaxFragmentLengthExtension(CheckServerExtensions(), (byte)mMaxFragmentLengthOffered);
		}
		if (mTruncatedHMacOffered && AllowTruncatedHMac)
		{
			TlsExtensionsUtilities.AddTruncatedHMacExtension(CheckServerExtensions());
		}
		if (mClientECPointFormats != null && TlsEccUtilities.IsEccCipherSuite(mSelectedCipherSuite))
		{
			mServerECPointFormats = new byte[3] { 0, 1, 2 };
			TlsEccUtilities.AddSupportedPointFormatsExtension(CheckServerExtensions(), mServerECPointFormats);
		}
		return mServerExtensions;
	}

	public virtual IList GetServerSupplementalData()
	{
		return null;
	}

	public abstract TlsCredentials GetCredentials();

	public virtual CertificateStatus GetCertificateStatus()
	{
		return null;
	}

	public abstract TlsKeyExchange GetKeyExchange();

	public virtual CertificateRequest GetCertificateRequest()
	{
		return null;
	}

	public virtual void ProcessClientSupplementalData(IList clientSupplementalData)
	{
		if (clientSupplementalData != null)
		{
			throw new TlsFatalAlert(10);
		}
	}

	public virtual void NotifyClientCertificate(Certificate clientCertificate)
	{
		throw new TlsFatalAlert(80);
	}

	public override TlsCompression GetCompression()
	{
		if (mSelectedCompressionMethod == 0)
		{
			return new TlsNullCompression();
		}
		throw new TlsFatalAlert(80);
	}

	public override TlsCipher GetCipher()
	{
		int encryptionAlgorithm = TlsUtilities.GetEncryptionAlgorithm(mSelectedCipherSuite);
		int macAlgorithm = TlsUtilities.GetMacAlgorithm(mSelectedCipherSuite);
		return mCipherFactory.CreateCipher(mContext, encryptionAlgorithm, macAlgorithm);
	}

	public virtual NewSessionTicket GetNewSessionTicket()
	{
		return new NewSessionTicket(0L, TlsUtilities.EmptyBytes);
	}
}
