namespace LumiSoft.Net.STUN.Message;

public class STUN_t_ErrorCode
{
	private int m_Code;

	private string m_ReasonText = "";

	public int Code
	{
		get
		{
			return m_Code;
		}
		set
		{
			m_Code = value;
		}
	}

	public string ReasonText
	{
		get
		{
			return m_ReasonText;
		}
		set
		{
			m_ReasonText = value;
		}
	}

	public STUN_t_ErrorCode(int code, string reasonText)
	{
		m_Code = code;
		m_ReasonText = reasonText;
	}
}
