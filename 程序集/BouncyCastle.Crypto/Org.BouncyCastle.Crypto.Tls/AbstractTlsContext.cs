using System;
using System.Threading;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

internal abstract class AbstractTlsContext : TlsContext
{
	private static long counter = Times.NanoTime();

	private readonly IRandomGenerator mNonceRandom;

	private readonly SecureRandom mSecureRandom;

	private readonly SecurityParameters mSecurityParameters;

	private ProtocolVersion mClientVersion = null;

	private ProtocolVersion mServerVersion = null;

	private TlsSession mSession = null;

	private object mUserObject = null;

	public virtual IRandomGenerator NonceRandomGenerator => mNonceRandom;

	public virtual SecureRandom SecureRandom => mSecureRandom;

	public virtual SecurityParameters SecurityParameters => mSecurityParameters;

	public abstract bool IsServer { get; }

	public virtual ProtocolVersion ClientVersion => mClientVersion;

	public virtual ProtocolVersion ServerVersion => mServerVersion;

	public virtual TlsSession ResumableSession => mSession;

	public virtual object UserObject
	{
		get
		{
			return mUserObject;
		}
		set
		{
			mUserObject = value;
		}
	}

	private static long NextCounterValue()
	{
		return Interlocked.Increment(ref counter);
	}

	internal AbstractTlsContext(SecureRandom secureRandom, SecurityParameters securityParameters)
	{
		IDigest digest = TlsUtilities.CreateHash(4);
		byte[] array = new byte[digest.GetDigestSize()];
		secureRandom.NextBytes(array);
		mNonceRandom = new DigestRandomGenerator(digest);
		mNonceRandom.AddSeedMaterial(NextCounterValue());
		mNonceRandom.AddSeedMaterial(Times.NanoTime());
		mNonceRandom.AddSeedMaterial(array);
		mSecureRandom = secureRandom;
		mSecurityParameters = securityParameters;
	}

	internal virtual void SetClientVersion(ProtocolVersion clientVersion)
	{
		mClientVersion = clientVersion;
	}

	internal virtual void SetServerVersion(ProtocolVersion serverVersion)
	{
		mServerVersion = serverVersion;
	}

	internal virtual void SetResumableSession(TlsSession session)
	{
		mSession = session;
	}

	public virtual byte[] ExportKeyingMaterial(string asciiLabel, byte[] context_value, int length)
	{
		if (context_value != null && !TlsUtilities.IsValidUint16(context_value.Length))
		{
			throw new ArgumentException("must have length less than 2^16 (or be null)", "context_value");
		}
		SecurityParameters securityParameters = SecurityParameters;
		if (!securityParameters.IsExtendedMasterSecret)
		{
			throw new InvalidOperationException("cannot export keying material without extended_master_secret");
		}
		byte[] clientRandom = securityParameters.ClientRandom;
		byte[] serverRandom = securityParameters.ServerRandom;
		int num = clientRandom.Length + serverRandom.Length;
		if (context_value != null)
		{
			num += 2 + context_value.Length;
		}
		byte[] array = new byte[num];
		int num2 = 0;
		Array.Copy(clientRandom, 0, array, num2, clientRandom.Length);
		num2 += clientRandom.Length;
		Array.Copy(serverRandom, 0, array, num2, serverRandom.Length);
		num2 += serverRandom.Length;
		if (context_value != null)
		{
			TlsUtilities.WriteUint16(context_value.Length, array, num2);
			num2 += 2;
			Array.Copy(context_value, 0, array, num2, context_value.Length);
			num2 += context_value.Length;
		}
		if (num2 != num)
		{
			throw new InvalidOperationException("error in calculation of seed for export");
		}
		return TlsUtilities.PRF(this, securityParameters.MasterSecret, asciiLabel, array, length);
	}
}
