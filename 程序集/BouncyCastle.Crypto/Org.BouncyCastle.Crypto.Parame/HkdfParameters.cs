using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class HkdfParameters : IDerivationParameters
{
	private readonly byte[] ikm;

	private readonly bool skipExpand;

	private readonly byte[] salt;

	private readonly byte[] info;

	public virtual bool SkipExtract => skipExpand;

	private HkdfParameters(byte[] ikm, bool skip, byte[] salt, byte[] info)
	{
		if (ikm == null)
		{
			throw new ArgumentNullException("ikm");
		}
		this.ikm = Arrays.Clone(ikm);
		skipExpand = skip;
		if (salt == null || salt.Length == 0)
		{
			this.salt = null;
		}
		else
		{
			this.salt = Arrays.Clone(salt);
		}
		if (info == null)
		{
			this.info = new byte[0];
		}
		else
		{
			this.info = Arrays.Clone(info);
		}
	}

	public HkdfParameters(byte[] ikm, byte[] salt, byte[] info)
		: this(ikm, skip: false, salt, info)
	{
	}

	public static HkdfParameters SkipExtractParameters(byte[] ikm, byte[] info)
	{
		return new HkdfParameters(ikm, skip: true, null, info);
	}

	public static HkdfParameters DefaultParameters(byte[] ikm)
	{
		return new HkdfParameters(ikm, skip: false, null, null);
	}

	public virtual byte[] GetIkm()
	{
		return Arrays.Clone(ikm);
	}

	public virtual byte[] GetSalt()
	{
		return Arrays.Clone(salt);
	}

	public virtual byte[] GetInfo()
	{
		return Arrays.Clone(info);
	}
}
