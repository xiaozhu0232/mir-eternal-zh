using System;
using System.Collections.Generic;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_Registration
{
	private DateTime m_CreateTime;

	private string m_UserName = "";

	private string m_AOR = "";

	private List<SIP_RegistrationBinding> m_pBindings;

	private object m_pLock = new object();

	public DateTime CreateTime => m_CreateTime;

	public string UserName => m_UserName;

	public string AOR => m_AOR;

	public SIP_RegistrationBinding[] Bindings
	{
		get
		{
			SIP_RegistrationBinding[] array = m_pBindings.ToArray();
			Array.Sort(array);
			return array;
		}
	}

	public SIP_Registration(string userName, string aor)
	{
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (aor == null)
		{
			throw new ArgumentNullException("aor");
		}
		if (aor == "")
		{
			throw new ArgumentException("Argument 'aor' value must be specified.");
		}
		m_UserName = userName;
		m_AOR = aor;
		m_CreateTime = DateTime.Now;
		m_pBindings = new List<SIP_RegistrationBinding>();
	}

	public SIP_RegistrationBinding GetBinding(AbsoluteUri contactUri)
	{
		if (contactUri == null)
		{
			throw new ArgumentNullException("contactUri");
		}
		lock (m_pLock)
		{
			foreach (SIP_RegistrationBinding pBinding in m_pBindings)
			{
				if (contactUri.Equals(pBinding.ContactURI))
				{
					return pBinding;
				}
			}
			return null;
		}
	}

	public void AddOrUpdateBindings(SIP_Flow flow, string callID, int cseqNo, SIP_t_ContactParam[] contacts)
	{
		if (callID == null)
		{
			throw new ArgumentNullException("callID");
		}
		if (cseqNo < 0)
		{
			throw new ArgumentException("Argument 'cseqNo' value must be >= 0.");
		}
		if (contacts == null)
		{
			throw new ArgumentNullException("contacts");
		}
		lock (m_pLock)
		{
			foreach (SIP_t_ContactParam sIP_t_ContactParam in contacts)
			{
				SIP_RegistrationBinding sIP_RegistrationBinding = GetBinding(sIP_t_ContactParam.Address.Uri);
				if (sIP_RegistrationBinding == null)
				{
					sIP_RegistrationBinding = new SIP_RegistrationBinding(this, sIP_t_ContactParam.Address.Uri);
					m_pBindings.Add(sIP_RegistrationBinding);
				}
				sIP_RegistrationBinding.Update(flow, (sIP_t_ContactParam.Expires == -1) ? 3600 : sIP_t_ContactParam.Expires, (sIP_t_ContactParam.QValue == -1.0) ? 1.0 : sIP_t_ContactParam.QValue, callID, cseqNo);
			}
		}
	}

	public void RemoveBinding(SIP_RegistrationBinding binding)
	{
		if (binding == null)
		{
			throw new ArgumentNullException("binding");
		}
		lock (m_pLock)
		{
			m_pBindings.Remove(binding);
		}
	}

	public void RemoveAllBindings()
	{
		lock (m_pLock)
		{
			m_pBindings.Clear();
		}
	}

	public void RemoveExpiredBindings()
	{
		lock (m_pLock)
		{
			for (int i = 0; i < m_pBindings.Count; i++)
			{
				if (m_pBindings[i].IsExpired)
				{
					m_pBindings.RemoveAt(i);
					i--;
				}
			}
		}
	}
}
