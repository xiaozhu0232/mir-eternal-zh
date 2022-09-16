using System;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_RegistrationBinding : IComparable
{
	private SIP_Registration m_pRegistration;

	private DateTime m_LastUpdate;

	private SIP_Flow m_pFlow;

	private AbsoluteUri m_ContactURI;

	private int m_Expires = 3600;

	private double m_QValue = 1.0;

	private string m_CallID = "";

	private int m_CSeqNo = 1;

	public DateTime LastUpdate => m_LastUpdate;

	public bool IsExpired => TTL <= 0;

	public int TTL
	{
		get
		{
			if (DateTime.Now > m_LastUpdate.AddSeconds(m_Expires))
			{
				return 0;
			}
			return (int)(m_LastUpdate.AddSeconds(m_Expires) - DateTime.Now).TotalSeconds;
		}
	}

	public SIP_Flow Flow => m_pFlow;

	public AbsoluteUri ContactURI => m_ContactURI;

	public double QValue => m_QValue;

	public string CallID => m_CallID;

	public int CSeqNo => m_CSeqNo;

	internal SIP_RegistrationBinding(SIP_Registration owner, AbsoluteUri contactUri)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (contactUri == null)
		{
			throw new ArgumentNullException("contactUri");
		}
		m_pRegistration = owner;
		m_ContactURI = contactUri;
	}

	public void Update(SIP_Flow flow, int expires, double qvalue, string callID, int cseqNo)
	{
		if (expires < 0)
		{
			throw new ArgumentException("Argument 'expires' value must be >= 0.");
		}
		if (qvalue < 0.0 || qvalue > 1.0)
		{
			throw new ArgumentException("Argument 'qvalue' value must be >= 0.000 and <= 1.000");
		}
		if (callID == null)
		{
			throw new ArgumentNullException("callID");
		}
		if (cseqNo < 0)
		{
			throw new ArgumentException("Argument 'cseqNo' value must be >= 0.");
		}
		m_pFlow = flow;
		m_Expires = expires;
		m_QValue = qvalue;
		m_CallID = callID;
		m_CSeqNo = cseqNo;
		m_LastUpdate = DateTime.Now;
	}

	public void Remove()
	{
		m_pRegistration.RemoveBinding(this);
	}

	public string ToContactValue()
	{
		SIP_t_ContactParam sIP_t_ContactParam = new SIP_t_ContactParam();
		sIP_t_ContactParam.Parse(new StringReader(m_ContactURI.ToString()));
		sIP_t_ContactParam.Expires = m_Expires;
		return sIP_t_ContactParam.ToStringValue();
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return -1;
		}
		if (!(obj is SIP_RegistrationBinding))
		{
			return -1;
		}
		SIP_RegistrationBinding sIP_RegistrationBinding = (SIP_RegistrationBinding)obj;
		if (sIP_RegistrationBinding.QValue == QValue)
		{
			return 0;
		}
		if (sIP_RegistrationBinding.QValue > QValue)
		{
			return 1;
		}
		_ = sIP_RegistrationBinding.QValue;
		_ = QValue;
		return -1;
	}
}
