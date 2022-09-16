using System.Net;

namespace LumiSoft.Net.STUN.Client;

public class STUN_Result
{
	private STUN_NetType m_NetType = STUN_NetType.OpenInternet;

	private IPEndPoint m_pPublicEndPoint;

	public STUN_NetType NetType => m_NetType;

	public IPEndPoint PublicEndPoint => m_pPublicEndPoint;

	public STUN_Result(STUN_NetType netType, IPEndPoint publicEndPoint)
	{
		m_NetType = netType;
		m_pPublicEndPoint = publicEndPoint;
	}
}
