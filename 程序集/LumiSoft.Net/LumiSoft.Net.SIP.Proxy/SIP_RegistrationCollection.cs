using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_RegistrationCollection : IEnumerable
{
	private List<SIP_Registration> m_pRegistrations;

	public int Count => m_pRegistrations.Count;

	public SIP_Registration this[string addressOfRecord]
	{
		get
		{
			lock (m_pRegistrations)
			{
				foreach (SIP_Registration pRegistration in m_pRegistrations)
				{
					if (pRegistration.AOR.ToLower() == addressOfRecord.ToLower())
					{
						return pRegistration;
					}
				}
				return null;
			}
		}
	}

	public SIP_Registration[] Values => m_pRegistrations.ToArray();

	public SIP_RegistrationCollection()
	{
		m_pRegistrations = new List<SIP_Registration>();
	}

	public void Add(SIP_Registration registration)
	{
		lock (m_pRegistrations)
		{
			if (Contains(registration.AOR))
			{
				throw new ArgumentException("Registration with specified registration name already exists !");
			}
			m_pRegistrations.Add(registration);
		}
	}

	public void Remove(string addressOfRecord)
	{
		lock (m_pRegistrations)
		{
			foreach (SIP_Registration pRegistration in m_pRegistrations)
			{
				if (pRegistration.AOR.ToLower() == addressOfRecord.ToLower())
				{
					m_pRegistrations.Remove(pRegistration);
					break;
				}
			}
		}
	}

	public bool Contains(string addressOfRecord)
	{
		lock (m_pRegistrations)
		{
			foreach (SIP_Registration pRegistration in m_pRegistrations)
			{
				if (pRegistration.AOR.ToLower() == addressOfRecord.ToLower())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveExpired()
	{
		lock (m_pRegistrations)
		{
			for (int i = 0; i < m_pRegistrations.Count; i++)
			{
				SIP_Registration sIP_Registration = m_pRegistrations[i];
				sIP_Registration.RemoveExpiredBindings();
				if (sIP_Registration.Bindings.Length == 0)
				{
					m_pRegistrations.Remove(sIP_Registration);
					i--;
				}
			}
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_pRegistrations.GetEnumerator();
	}
}
