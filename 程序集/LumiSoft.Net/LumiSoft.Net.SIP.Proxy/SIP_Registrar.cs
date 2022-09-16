using System;
using System.Timers;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy;

public class SIP_Registrar
{
	private bool m_IsDisposed;

	private SIP_Proxy m_pProxy;

	private SIP_Stack m_pStack;

	private SIP_RegistrationCollection m_pRegistrations;

	private Timer m_pTimer;

	public SIP_Proxy Proxy => m_pProxy;

	public SIP_Registration[] Registrations
	{
		get
		{
			lock (m_pRegistrations)
			{
				SIP_Registration[] array = new SIP_Registration[m_pRegistrations.Count];
				m_pRegistrations.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public event SIP_CanRegisterEventHandler CanRegister;

	public event EventHandler<SIP_RegistrationEventArgs> AorRegistered;

	public event EventHandler<SIP_RegistrationEventArgs> AorUnregistered;

	public event EventHandler<SIP_RegistrationEventArgs> AorUpdated;

	internal SIP_Registrar(SIP_Proxy proxy)
	{
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		m_pProxy = proxy;
		m_pStack = m_pProxy.Stack;
		m_pRegistrations = new SIP_RegistrationCollection();
		m_pTimer = new Timer(15000.0);
		m_pTimer.Elapsed += m_pTimer_Elapsed;
		m_pTimer.Enabled = true;
	}

	internal void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			this.CanRegister = null;
			this.AorRegistered = null;
			this.AorUnregistered = null;
			this.AorUpdated = null;
			m_pProxy = null;
			m_pStack = null;
			m_pRegistrations = null;
			if (m_pTimer != null)
			{
				m_pTimer.Dispose();
				m_pTimer = null;
			}
		}
	}

	private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		m_pRegistrations.RemoveExpired();
	}

	public SIP_Registration GetRegistration(string aor)
	{
		return m_pRegistrations[aor];
	}

	public void SetRegistration(string aor, SIP_t_ContactParam[] contacts)
	{
		SetRegistration(aor, contacts, null);
	}

	public void SetRegistration(string aor, SIP_t_ContactParam[] contacts, SIP_Flow flow)
	{
		lock (m_pRegistrations)
		{
			SIP_Registration sIP_Registration = m_pRegistrations[aor];
			if (sIP_Registration == null)
			{
				sIP_Registration = new SIP_Registration("system", aor);
				m_pRegistrations.Add(sIP_Registration);
				OnAorRegistered(sIP_Registration);
			}
			sIP_Registration.AddOrUpdateBindings(flow, "", 1, contacts);
		}
	}

	public void DeleteRegistration(string addressOfRecord)
	{
		m_pRegistrations.Remove(addressOfRecord);
	}

	internal void Register(SIP_RequestReceivedEventArgs e)
	{
		SIP_ServerTransaction serverTransaction = e.ServerTransaction;
		SIP_Request request = e.Request;
		SIP_Uri sIP_Uri = null;
		string userName = "";
		if (SIP_Utils.IsSipOrSipsUri(request.To.Address.Uri.ToString()))
		{
			sIP_Uri = (SIP_Uri)request.To.Address.Uri;
			if (!m_pProxy.AuthenticateRequest(e, out userName))
			{
				return;
			}
			if (!m_pProxy.OnAddressExists(sIP_Uri.Address))
			{
				serverTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x404_Not_Found, request));
				return;
			}
			if (!OnCanRegister(userName, sIP_Uri.Address))
			{
				serverTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x403_Forbidden, request));
				return;
			}
			SIP_t_ContactParam sIP_t_ContactParam = null;
			SIP_t_ContactParam[] allValues = request.Contact.GetAllValues();
			foreach (SIP_t_ContactParam sIP_t_ContactParam2 in allValues)
			{
				if (sIP_t_ContactParam2.IsStarContact)
				{
					sIP_t_ContactParam = sIP_t_ContactParam2;
					break;
				}
			}
			if (sIP_t_ContactParam != null)
			{
				if (request.Contact.GetAllValues().Length > 1)
				{
					serverTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ": RFC 3261 10.3.6 -> If star(*) present, only 1 contact allowed.", request));
					return;
				}
				if (sIP_t_ContactParam.Expires != 0)
				{
					serverTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ": RFC 3261 10.3.6 -> star(*) contact parameter 'expires' value must be always '0'.", request));
					return;
				}
				SIP_Registration sIP_Registration = m_pRegistrations[sIP_Uri.Address];
				if (sIP_Registration != null)
				{
					SIP_RegistrationBinding[] bindings = sIP_Registration.Bindings;
					foreach (SIP_RegistrationBinding sIP_RegistrationBinding in bindings)
					{
						if (request.CallID != sIP_RegistrationBinding.CallID || request.CSeq.SequenceNumber > sIP_RegistrationBinding.CSeqNo)
						{
							sIP_RegistrationBinding.Remove();
						}
					}
				}
			}
			if (sIP_t_ContactParam == null)
			{
				bool flag = false;
				SIP_Registration sIP_Registration2 = m_pRegistrations[sIP_Uri.Address];
				if (sIP_Registration2 == null)
				{
					flag = true;
					sIP_Registration2 = new SIP_Registration(userName, sIP_Uri.Address);
					m_pRegistrations.Add(sIP_Registration2);
				}
				allValues = request.Contact.GetAllValues();
				foreach (SIP_t_ContactParam sIP_t_ContactParam3 in allValues)
				{
					if (sIP_t_ContactParam3.Expires == -1)
					{
						sIP_t_ContactParam3.Expires = request.Expires;
					}
					if (sIP_t_ContactParam3.Expires == -1)
					{
						sIP_t_ContactParam3.Expires = m_pProxy.Stack.MinimumExpireTime;
					}
					if (sIP_t_ContactParam3.Expires != 0 && sIP_t_ContactParam3.Expires < m_pProxy.Stack.MinimumExpireTime)
					{
						SIP_Response sIP_Response = m_pStack.CreateResponse(SIP_ResponseCodes.x423_Interval_Too_Brief, request);
						sIP_Response.MinExpires = m_pProxy.Stack.MinimumExpireTime;
						serverTransaction.SendResponse(sIP_Response);
						return;
					}
					SIP_RegistrationBinding binding = sIP_Registration2.GetBinding(sIP_t_ContactParam3.Address.Uri);
					if (binding != null && binding.CallID == request.CallID && request.CSeq.SequenceNumber < binding.CSeqNo)
					{
						serverTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ": CSeq value out of order.", request));
						return;
					}
				}
				sIP_Registration2.AddOrUpdateBindings(e.ServerTransaction.Flow, request.CallID, request.CSeq.SequenceNumber, request.Contact.GetAllValues());
				if (flag)
				{
					OnAorRegistered(sIP_Registration2);
				}
				else
				{
					OnAorUpdated(sIP_Registration2);
				}
			}
			SIP_Response sIP_Response2 = m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, request);
			sIP_Response2.Date = DateTime.Now;
			SIP_Registration sIP_Registration3 = m_pRegistrations[sIP_Uri.Address];
			if (sIP_Registration3 != null)
			{
				SIP_RegistrationBinding[] bindings = sIP_Registration3.Bindings;
				foreach (SIP_RegistrationBinding sIP_RegistrationBinding2 in bindings)
				{
					if (sIP_RegistrationBinding2.TTL > 1)
					{
						sIP_Response2.Header.Add("Contact:", sIP_RegistrationBinding2.ToContactValue());
					}
				}
			}
			sIP_Response2.AuthenticationInfo.Add("qop=\"auth\",nextnonce=\"" + m_pStack.DigestNonceManager.CreateNonce() + "\"");
			serverTransaction.SendResponse(sIP_Response2);
		}
		else
		{
			serverTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ": To: value must be SIP or SIPS URI.", request));
		}
	}

	internal bool OnCanRegister(string userName, string address)
	{
		if (this.CanRegister != null)
		{
			return this.CanRegister(userName, address);
		}
		return false;
	}

	private void OnAorRegistered(SIP_Registration registration)
	{
		if (this.AorRegistered != null)
		{
			this.AorRegistered(this, new SIP_RegistrationEventArgs(registration));
		}
	}

	private void OnAorUnregistered(SIP_Registration registration)
	{
		if (this.AorUnregistered != null)
		{
			this.AorUnregistered(this, new SIP_RegistrationEventArgs(registration));
		}
	}

	private void OnAorUpdated(SIP_Registration registration)
	{
		if (this.AorUpdated != null)
		{
			this.AorUpdated(this, new SIP_RegistrationEventArgs(registration));
		}
	}
}
