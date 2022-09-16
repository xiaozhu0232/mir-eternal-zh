namespace LumiSoft.Net.UPnP.NAT;

public class UPnP_NAT_Map
{
	private bool m_Enabled;

	private string m_Protocol = "";

	private string m_RemoteHost = "";

	private string m_ExternalPort = "";

	private string m_InternalHost = "";

	private int m_InternalPort;

	private string m_Description = "";

	private int m_LeaseDuration;

	public bool Enabled => m_Enabled;

	public string Protocol => m_Protocol;

	public string RemoteHost => m_RemoteHost;

	public string ExternalPort => m_ExternalPort;

	public string InternalHost => m_InternalHost;

	public int InternalPort => m_InternalPort;

	public string Description => m_Description;

	public int LeaseDuration => m_LeaseDuration;

	public UPnP_NAT_Map(bool enabled, string protocol, string remoteHost, string externalPort, string internalHost, int internalPort, string description, int leaseDuration)
	{
		m_Enabled = enabled;
		m_Protocol = protocol;
		m_RemoteHost = remoteHost;
		m_ExternalPort = externalPort;
		m_InternalHost = internalHost;
		m_InternalPort = internalPort;
		m_Description = description;
		m_LeaseDuration = leaseDuration;
	}
}
