using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;

namespace LumiSoft.Net.DNS.Client;

public class DNS_ClientTransaction
{
	private object m_pLock = new object();

	private DNS_ClientTransactionState m_State;

	private DateTime m_CreateTime;

	private Dns_Client m_pOwner;

	private IPAddress[] m_pDnsServers;

	private int m_ID = 1;

	private string m_QName = "";

	private DNS_QType m_QType;

	private TimerEx m_pTimeoutTimer;

	private DnsServerResponse m_pResponse;

	private int m_ResponseCount;

	public DNS_ClientTransactionState State => m_State;

	public DateTime CreateTime => m_CreateTime;

	public int ID => m_ID;

	public string QName => m_QName;

	public DNS_QType QType => m_QType;

	public DnsServerResponse Response => m_pResponse;

	public event EventHandler<EventArgs<DNS_ClientTransaction>> StateChanged;

	public event EventHandler Timeout;

	internal DNS_ClientTransaction(Dns_Client owner, IPAddress[] dnsServers, int id, DNS_QType qtype, string qname, int timeout)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (dnsServers == null)
		{
			throw new ArgumentNullException("dnsServers");
		}
		if (qname == null)
		{
			throw new ArgumentNullException("qname");
		}
		m_pOwner = owner;
		m_pDnsServers = dnsServers;
		m_ID = id;
		m_QName = qname;
		m_QType = qtype;
		m_CreateTime = DateTime.Now;
		m_pTimeoutTimer = new TimerEx(timeout);
		m_pTimeoutTimer.Elapsed += m_pTimeoutTimer_Elapsed;
	}

	public void Dispose()
	{
		lock (m_pLock)
		{
			if (State != DNS_ClientTransactionState.Disposed)
			{
				SetState(DNS_ClientTransactionState.Disposed);
				if (m_pTimeoutTimer != null)
				{
					m_pTimeoutTimer.Dispose();
					m_pTimeoutTimer = null;
				}
				m_pOwner = null;
				m_pResponse = null;
				this.StateChanged = null;
				this.Timeout = null;
			}
		}
	}

	private void m_pTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			OnTimeout();
		}
		catch
		{
		}
		finally
		{
			Dispose();
		}
	}

	public void Start()
	{
		if (State != 0)
		{
			throw new InvalidOperationException("DNS_ClientTransaction.Start may be called only in 'WaitingForStart' transaction state.");
		}
		SetState(DNS_ClientTransactionState.Active);
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				if (Dns_Client.UseDnsCache)
				{
					DnsServerResponse fromCache = m_pOwner.Cache.GetFromCache(m_QName, (int)m_QType);
					if (fromCache != null)
					{
						m_pResponse = fromCache;
						SetState(DNS_ClientTransactionState.Completed);
						Dispose();
						return;
					}
				}
				byte[] array = new byte[1400];
				int count = CreateQuery(array, m_ID, m_QName, m_QType, 1);
				IPAddress[] pDnsServers = m_pDnsServers;
				foreach (IPAddress target in pDnsServers)
				{
					m_pOwner.Send(target, array, count);
				}
				m_pTimeoutTimer.Start();
			}
			catch
			{
				try
				{
					new IdnMapping().GetAscii(m_QName);
				}
				catch
				{
					m_pResponse = new DnsServerResponse(connectionOk: true, m_ID, DNS_RCode.NAME_ERROR, new List<DNS_rr>(), new List<DNS_rr>(), new List<DNS_rr>());
				}
				SetState(DNS_ClientTransactionState.Completed);
			}
		});
	}

	internal void ProcessResponse(DnsServerResponse response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		try
		{
			lock (m_pLock)
			{
				if (State == DNS_ClientTransactionState.Active)
				{
					m_ResponseCount++;
					if (m_pResponse == null && (response.ResponseCode != DNS_RCode.REFUSED || m_ResponseCount >= Dns_Client.DnsServers.Length))
					{
						m_pResponse = response;
						SetState(DNS_ClientTransactionState.Completed);
					}
				}
			}
		}
		finally
		{
			if (State == DNS_ClientTransactionState.Completed)
			{
				Dispose();
			}
		}
	}

	private void SetState(DNS_ClientTransactionState state)
	{
		if (State != DNS_ClientTransactionState.Disposed)
		{
			m_State = state;
			OnStateChanged();
		}
	}

	private int CreateQuery(byte[] buffer, int ID, string qname, DNS_QType qtype, int qclass)
	{
		buffer[0] = (byte)(ID >> 8);
		buffer[1] = (byte)((uint)ID & 0xFFu);
		buffer[2] = 1;
		buffer[3] = 0;
		buffer[4] = 0;
		buffer[5] = 1;
		buffer[6] = 0;
		buffer[7] = 0;
		buffer[8] = 0;
		buffer[9] = 0;
		buffer[10] = 0;
		buffer[11] = 0;
		qname = new IdnMapping().GetAscii(qname);
		string[] array = qname.Split('.');
		int num = 12;
		string[] array2 = array;
		foreach (string s in array2)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			buffer[num++] = (byte)bytes.Length;
			bytes.CopyTo(buffer, num);
			num += bytes.Length;
		}
		buffer[num++] = 0;
		buffer[num++] = 0;
		buffer[num++] = (byte)qtype;
		buffer[num++] = 0;
		buffer[num++] = (byte)qclass;
		return num;
	}

	private void OnStateChanged()
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, new EventArgs<DNS_ClientTransaction>(this));
		}
	}

	private void OnTimeout()
	{
		if (this.Timeout != null)
		{
			this.Timeout(this, new EventArgs());
		}
	}
}
