using System.Collections;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class DefaultTlsSrpGroupVerifier : TlsSrpGroupVerifier
{
	protected static readonly IList DefaultGroups;

	protected readonly IList mGroups;

	static DefaultTlsSrpGroupVerifier()
	{
		DefaultGroups = Platform.CreateArrayList();
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_1024);
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_1536);
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_2048);
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_3072);
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_4096);
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_6144);
		DefaultGroups.Add(Srp6StandardGroups.rfc5054_8192);
	}

	public DefaultTlsSrpGroupVerifier()
		: this(DefaultGroups)
	{
	}

	public DefaultTlsSrpGroupVerifier(IList groups)
	{
		mGroups = groups;
	}

	public virtual bool Accept(Srp6GroupParameters group)
	{
		foreach (Srp6GroupParameters mGroup in mGroups)
		{
			if (AreGroupsEqual(group, mGroup))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool AreGroupsEqual(Srp6GroupParameters a, Srp6GroupParameters b)
	{
		if (a != b)
		{
			if (AreParametersEqual(a.N, b.N))
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
}
