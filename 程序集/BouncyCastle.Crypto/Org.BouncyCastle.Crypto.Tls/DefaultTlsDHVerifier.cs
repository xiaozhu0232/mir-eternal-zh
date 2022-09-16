using System.Collections;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class DefaultTlsDHVerifier : TlsDHVerifier
{
	public static readonly int DefaultMinimumPrimeBits;

	protected static readonly IList DefaultGroups;

	protected readonly IList mGroups;

	protected readonly int mMinimumPrimeBits;

	public virtual int MinimumPrimeBits => mMinimumPrimeBits;

	private static void AddDefaultGroup(DHParameters dhParameters)
	{
		DefaultGroups.Add(dhParameters);
	}

	static DefaultTlsDHVerifier()
	{
		DefaultMinimumPrimeBits = 2048;
		DefaultGroups = Platform.CreateArrayList();
		AddDefaultGroup(DHStandardGroups.rfc7919_ffdhe2048);
		AddDefaultGroup(DHStandardGroups.rfc7919_ffdhe3072);
		AddDefaultGroup(DHStandardGroups.rfc7919_ffdhe4096);
		AddDefaultGroup(DHStandardGroups.rfc7919_ffdhe6144);
		AddDefaultGroup(DHStandardGroups.rfc7919_ffdhe8192);
		AddDefaultGroup(DHStandardGroups.rfc3526_1536);
		AddDefaultGroup(DHStandardGroups.rfc3526_2048);
		AddDefaultGroup(DHStandardGroups.rfc3526_3072);
		AddDefaultGroup(DHStandardGroups.rfc3526_4096);
		AddDefaultGroup(DHStandardGroups.rfc3526_6144);
		AddDefaultGroup(DHStandardGroups.rfc3526_8192);
	}

	public DefaultTlsDHVerifier()
		: this(DefaultMinimumPrimeBits)
	{
	}

	public DefaultTlsDHVerifier(int minimumPrimeBits)
		: this(DefaultGroups, minimumPrimeBits)
	{
	}

	public DefaultTlsDHVerifier(IList groups, int minimumPrimeBits)
	{
		mGroups = groups;
		mMinimumPrimeBits = minimumPrimeBits;
	}

	public virtual bool Accept(DHParameters dhParameters)
	{
		if (CheckMinimumPrimeBits(dhParameters))
		{
			return CheckGroup(dhParameters);
		}
		return false;
	}

	protected virtual bool AreGroupsEqual(DHParameters a, DHParameters b)
	{
		if (a != b)
		{
			if (AreParametersEqual(a.P, b.P))
			{
				return AreParametersEqual(a.G, b.G);
			}
			return false;
		}
		return true;
	}

	protected virtual bool AreParametersEqual(BigInteger a, BigInteger b)
	{
		if (a != b)
		{
			return a.Equals(b);
		}
		return true;
	}

	protected virtual bool CheckGroup(DHParameters dhParameters)
	{
		foreach (DHParameters mGroup in mGroups)
		{
			if (AreGroupsEqual(dhParameters, mGroup))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool CheckMinimumPrimeBits(DHParameters dhParameters)
	{
		return dhParameters.P.BitLength >= MinimumPrimeBits;
	}
}
