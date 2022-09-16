using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack;

public class SIP_UA_Registration
{
	private bool m_IsDisposed;

	private SIP_UA_RegistrationState m_State = SIP_UA_RegistrationState.Unregistered;

	private SIP_Stack m_pStack;

	private SIP_Uri m_pServer;

	private string m_AOR = "";

	private AbsoluteUri m_pContact;

	private List<AbsoluteUri> m_pContacts;

	private int m_RefreshInterval = 300;

	private TimerEx m_pTimer;

	private SIP_RequestSender m_pRegisterSender;

	private SIP_RequestSender m_pUnregisterSender;

	private bool m_AutoRefresh = true;

	private bool m_AutoDispose;

	private SIP_Flow m_pFlow;

	public bool IsDisposed => m_IsDisposed;

	public SIP_UA_RegistrationState State => m_State;

	public int Expires
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return 3600;
		}
	}

	public string AOR
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_AOR;
		}
	}

	public AbsoluteUri Contact
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pContact;
		}
	}

	public AbsoluteUri[] Contacts
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pContacts.ToArray();
		}
	}

	public bool AutoFixContact
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return false;
		}
	}

	public event EventHandler StateChanged;

	public event EventHandler Registered;

	public event EventHandler Unregistered;

	public event EventHandler<SIP_ResponseReceivedEventArgs> Error;

	public event EventHandler Disposed;

	internal SIP_UA_Registration(SIP_Stack stack, SIP_Uri server, string aor, AbsoluteUri contact, int expires)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (aor == null)
		{
			throw new ArgumentNullException("aor");
		}
		if (aor == string.Empty)
		{
			throw new ArgumentException("Argument 'aor' value must be specified.");
		}
		if (contact == null)
		{
			throw new ArgumentNullException("contact");
		}
		m_pStack = stack;
		m_pServer = server;
		m_AOR = aor;
		m_pContact = contact;
		m_RefreshInterval = expires;
		m_pContacts = new List<AbsoluteUri>();
		m_pTimer = new TimerEx((m_RefreshInterval - 15) * 1000);
		m_pTimer.AutoReset = false;
		m_pTimer.Elapsed += m_pTimer_Elapsed;
		m_pTimer.Enabled = false;
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_pStack = null;
			m_pTimer.Dispose();
			m_pTimer = null;
			SetState(SIP_UA_RegistrationState.Disposed);
			OnDisposed();
			this.Registered = null;
			this.Unregistered = null;
			this.Error = null;
			this.Disposed = null;
		}
	}

	private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (m_pStack.State == SIP_StackState.Started)
		{
			BeginRegister(m_AutoRefresh);
		}
	}

	private void m_pRegisterSender_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		m_pFlow = e.ClientTransaction.Flow;
		if (e.Response.StatusCodeType == SIP_StatusCodeType.Provisional)
		{
			return;
		}
		if (e.Response.StatusCodeType == SIP_StatusCodeType.Success)
		{
			m_pContacts.Clear();
			SIP_t_ContactParam[] allValues = e.Response.Contact.GetAllValues();
			foreach (SIP_t_ContactParam sIP_t_ContactParam in allValues)
			{
				m_pContacts.Add(sIP_t_ContactParam.Address.Uri);
			}
			SetState(SIP_UA_RegistrationState.Registered);
			OnRegistered();
			m_pFlow.SendKeepAlives = true;
		}
		else
		{
			SetState(SIP_UA_RegistrationState.Error);
			OnError(e);
		}
		if (AutoFixContact && m_pContact is SIP_Uri)
		{
			SIP_Uri sIP_Uri = (SIP_Uri)m_pContact;
			IPAddress iPAddress = (Net_Utils.IsIPAddress(sIP_Uri.Host) ? IPAddress.Parse(sIP_Uri.Host) : null);
			SIP_t_ViaParm topMostValue = e.Response.Via.GetTopMostValue();
			if (topMostValue != null && iPAddress != null)
			{
				IPEndPoint iPEndPoint = new IPEndPoint((topMostValue.Received != null) ? topMostValue.Received : iPAddress, (topMostValue.RPort > 0) ? topMostValue.RPort : sIP_Uri.Port);
				if (!iPAddress.Equals(iPEndPoint.Address) || sIP_Uri.Port != topMostValue.RPort)
				{
					BeginUnregister(dispose: false);
					sIP_Uri.Host = iPEndPoint.Address.ToString();
					sIP_Uri.Port = iPEndPoint.Port;
					m_pRegisterSender.Dispose();
					m_pRegisterSender = null;
					BeginRegister(m_AutoRefresh);
					return;
				}
			}
		}
		if (m_AutoRefresh)
		{
			m_pTimer.Enabled = true;
		}
		m_pRegisterSender.Dispose();
		m_pRegisterSender = null;
	}

	private void m_pUnregisterSender_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
	{
		SetState(SIP_UA_RegistrationState.Unregistered);
		OnUnregistered();
		if (m_AutoDispose)
		{
			Dispose();
		}
		m_pUnregisterSender = null;
	}

	public void BeginRegister(bool autoRefresh)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_pStack.State != 0)
		{
			m_pTimer.Enabled = true;
			return;
		}
		m_AutoRefresh = autoRefresh;
		SetState(SIP_UA_RegistrationState.Registering);
		SIP_Request sIP_Request = m_pStack.CreateRequest("REGISTER", new SIP_t_NameAddress(m_pServer.Scheme + ":" + m_AOR), new SIP_t_NameAddress(m_pServer.Scheme + ":" + m_AOR));
		sIP_Request.RequestLine.Uri = SIP_Uri.Parse(m_pServer.Scheme + ":" + m_AOR.Substring(m_AOR.IndexOf('@') + 1));
		sIP_Request.Route.Add(m_pServer.ToString());
		sIP_Request.Contact.Add("<" + Contact?.ToString() + ">;expires=" + m_RefreshInterval);
		m_pRegisterSender = m_pStack.CreateRequestSender(sIP_Request, m_pFlow);
		m_pRegisterSender.ResponseReceived += m_pRegisterSender_ResponseReceived;
		m_pRegisterSender.Start();
	}

	public void BeginUnregister(bool dispose)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		m_AutoDispose = dispose;
		m_pTimer.Enabled = false;
		if (m_State == SIP_UA_RegistrationState.Registered)
		{
			SIP_Request sIP_Request = m_pStack.CreateRequest("REGISTER", new SIP_t_NameAddress(m_pServer.Scheme + ":" + m_AOR), new SIP_t_NameAddress(m_pServer.Scheme + ":" + m_AOR));
			sIP_Request.RequestLine.Uri = SIP_Uri.Parse(m_pServer.Scheme + ":" + m_AOR.Substring(m_AOR.IndexOf('@') + 1));
			sIP_Request.Route.Add(m_pServer.ToString());
			sIP_Request.Contact.Add("<" + Contact?.ToString() + ">;expires=0");
			m_pUnregisterSender = m_pStack.CreateRequestSender(sIP_Request, m_pFlow);
			m_pUnregisterSender.ResponseReceived += m_pUnregisterSender_ResponseReceived;
			m_pUnregisterSender.Start();
		}
		else
		{
			SetState(SIP_UA_RegistrationState.Unregistered);
			OnUnregistered();
			if (m_AutoDispose)
			{
				Dispose();
			}
			m_pUnregisterSender = null;
		}
	}

	private void SetState(SIP_UA_RegistrationState newState)
	{
		m_State = newState;
		OnStateChanged();
	}

	private void OnStateChanged()
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, new EventArgs());
		}
	}

	private void OnRegistered()
	{
		if (this.Registered != null)
		{
			this.Registered(this, new EventArgs());
		}
	}

	private void OnUnregistered()
	{
		if (this.Unregistered != null)
		{
			this.Unregistered(this, new EventArgs());
		}
	}

	private void OnError(SIP_ResponseReceivedEventArgs e)
	{
		if (this.Error != null)
		{
			this.Error(this, e);
		}
	}

	private void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}
}
