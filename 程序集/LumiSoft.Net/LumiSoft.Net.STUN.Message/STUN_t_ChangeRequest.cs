namespace LumiSoft.Net.STUN.Message;

public class STUN_t_ChangeRequest
{
	private bool m_ChangeIP = true;

	private bool m_ChangePort = true;

	public bool ChangeIP
	{
		get
		{
			return m_ChangeIP;
		}
		set
		{
			m_ChangeIP = value;
		}
	}

	public bool ChangePort
	{
		get
		{
			return m_ChangePort;
		}
		set
		{
			m_ChangePort = value;
		}
	}

	public STUN_t_ChangeRequest()
	{
	}

	public STUN_t_ChangeRequest(bool changeIP, bool changePort)
	{
		m_ChangeIP = changeIP;
		m_ChangePort = changePort;
	}
}
