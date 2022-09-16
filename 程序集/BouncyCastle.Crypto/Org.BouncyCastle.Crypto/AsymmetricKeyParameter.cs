namespace Org.BouncyCastle.Crypto;

public abstract class AsymmetricKeyParameter : ICipherParameters
{
	private readonly bool privateKey;

	public bool IsPrivate => privateKey;

	protected AsymmetricKeyParameter(bool privateKey)
	{
		this.privateKey = privateKey;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AsymmetricKeyParameter other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(AsymmetricKeyParameter other)
	{
		return privateKey == other.privateKey;
	}

	public override int GetHashCode()
	{
		return privateKey.GetHashCode();
	}
}
