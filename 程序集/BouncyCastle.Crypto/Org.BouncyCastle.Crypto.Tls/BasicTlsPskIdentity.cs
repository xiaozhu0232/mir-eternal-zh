using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class BasicTlsPskIdentity : TlsPskIdentity
{
	protected byte[] mIdentity;

	protected byte[] mPsk;

	public BasicTlsPskIdentity(byte[] identity, byte[] psk)
	{
		mIdentity = Arrays.Clone(identity);
		mPsk = Arrays.Clone(psk);
	}

	public BasicTlsPskIdentity(string identity, byte[] psk)
	{
		mIdentity = Strings.ToUtf8ByteArray(identity);
		mPsk = Arrays.Clone(psk);
	}

	public virtual void SkipIdentityHint()
	{
	}

	public virtual void NotifyIdentityHint(byte[] psk_identity_hint)
	{
	}

	public virtual byte[] GetPskIdentity()
	{
		return mIdentity;
	}

	public virtual byte[] GetPsk()
	{
		return Arrays.Clone(mPsk);
	}
}
