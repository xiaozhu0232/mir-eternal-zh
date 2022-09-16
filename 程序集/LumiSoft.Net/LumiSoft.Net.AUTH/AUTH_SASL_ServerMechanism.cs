using System.Collections.Generic;

namespace LumiSoft.Net.AUTH;

public abstract class AUTH_SASL_ServerMechanism
{
	private Dictionary<string, object> m_pTags;

	public abstract bool IsCompleted { get; }

	public abstract bool IsAuthenticated { get; }

	public abstract string Name { get; }

	public abstract bool RequireSSL { get; }

	public abstract string UserName { get; }

	public Dictionary<string, object> Tags => m_pTags;

	public AUTH_SASL_ServerMechanism()
	{
	}

	public abstract void Reset();

	public abstract byte[] Continue(byte[] clientResponse);
}
