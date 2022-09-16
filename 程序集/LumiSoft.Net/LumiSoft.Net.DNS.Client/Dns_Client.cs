using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LumiSoft.Net.UDP;

namespace LumiSoft.Net.DNS.Client;

public class Dns_Client : IDisposable
{
	public class GetHostAddressesAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private string m_HostNameOrIP;

		private List<IPAddress> m_pIPv4Addresses;

		private List<IPAddress> m_pIPv6Addresses;

		private int m_Counter;

		private bool m_RiseCompleted;

		public AsyncOP_State State => m_State;

		public Exception Error
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pException;
			}
		}

		public string HostNameOrIP
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_HostNameOrIP;
			}
		}

		public IPAddress[] Addresses
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Addresses' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				List<IPAddress> list = new List<IPAddress>();
				list.AddRange(m_pIPv4Addresses);
				list.AddRange(m_pIPv6Addresses);
				return list.ToArray();
			}
		}

		public event EventHandler<EventArgs<GetHostAddressesAsyncOP>> CompletedAsync;

		public GetHostAddressesAsyncOP(string hostNameOrIP)
		{
			if (hostNameOrIP == null)
			{
				throw new ArgumentNullException("hostNameOrIP");
			}
			m_HostNameOrIP = hostNameOrIP;
			m_pIPv4Addresses = new List<IPAddress>();
			m_pIPv6Addresses = new List<IPAddress>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_HostNameOrIP = null;
				m_pIPv4Addresses = null;
				m_pIPv6Addresses = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(Dns_Client dnsClient)
		{
			if (dnsClient == null)
			{
				throw new ArgumentNullException("dnsClient");
			}
			SetState(AsyncOP_State.Active);
			if (Net_Utils.IsIPAddress(m_HostNameOrIP))
			{
				m_pIPv4Addresses.Add(IPAddress.Parse(m_HostNameOrIP));
				SetState(AsyncOP_State.Completed);
			}
			if (m_HostNameOrIP.IndexOf(".") == -1)
			{
				try
				{
					AsyncCallback requestCallback = delegate(IAsyncResult ar)
					{
						try
						{
							IPAddress[] array = Dns.EndGetHostAddresses(ar);
							foreach (IPAddress iPAddress in array)
							{
								if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
								{
									m_pIPv4Addresses.Add(iPAddress);
								}
								else
								{
									m_pIPv6Addresses.Add(iPAddress);
								}
							}
						}
						catch (Exception pException2)
						{
							Exception ex2 = (m_pException = pException2);
						}
						SetState(AsyncOP_State.Completed);
					};
					Dns.BeginGetHostAddresses(m_HostNameOrIP, requestCallback, null);
				}
				catch (Exception pException)
				{
					Exception ex = (m_pException = pException);
				}
			}
			else
			{
				DNS_ClientTransaction dNS_ClientTransaction = dnsClient.CreateTransaction(DNS_QType.A, m_HostNameOrIP, 2000);
				dNS_ClientTransaction.StateChanged += delegate(object s1, EventArgs<DNS_ClientTransaction> e1)
				{
					if (e1.Value.State == DNS_ClientTransactionState.Completed)
					{
						lock (m_pLock)
						{
							if (e1.Value.Response.ResponseCode != 0)
							{
								m_pException = new DNS_ClientException(e1.Value.Response.ResponseCode);
							}
							else
							{
								DNS_rr_A[] aRecords = e1.Value.Response.GetARecords();
								foreach (DNS_rr_A dNS_rr_A in aRecords)
								{
									m_pIPv4Addresses.Add(dNS_rr_A.IP);
								}
							}
							m_Counter++;
							if (m_Counter == 2)
							{
								SetState(AsyncOP_State.Completed);
							}
						}
					}
				};
				dNS_ClientTransaction.Timeout += delegate
				{
					lock (m_pLock)
					{
						m_pException = new IOException("DNS transaction timeout, no response from DNS server.");
						m_Counter++;
						if (m_Counter == 2)
						{
							SetState(AsyncOP_State.Completed);
						}
					}
				};
				dNS_ClientTransaction.Start();
				DNS_ClientTransaction dNS_ClientTransaction2 = dnsClient.CreateTransaction(DNS_QType.AAAA, m_HostNameOrIP, 2000);
				dNS_ClientTransaction2.StateChanged += delegate(object s1, EventArgs<DNS_ClientTransaction> e1)
				{
					if (e1.Value.State == DNS_ClientTransactionState.Completed)
					{
						lock (m_pLock)
						{
							if (e1.Value.Response.ResponseCode != 0)
							{
								m_pException = new DNS_ClientException(e1.Value.Response.ResponseCode);
							}
							else
							{
								DNS_rr_AAAA[] aAAARecords = e1.Value.Response.GetAAAARecords();
								foreach (DNS_rr_AAAA dNS_rr_AAAA in aAAARecords)
								{
									m_pIPv6Addresses.Add(dNS_rr_AAAA.IP);
								}
							}
							m_Counter++;
							if (m_Counter == 2)
							{
								SetState(AsyncOP_State.Completed);
							}
						}
					}
				};
				dNS_ClientTransaction2.Timeout += delegate
				{
					lock (m_pLock)
					{
						m_pException = new IOException("DNS transaction timeout, no response from DNS server.");
						m_Counter++;
						if (m_Counter == 2)
						{
							SetState(AsyncOP_State.Completed);
						}
					}
				};
				dNS_ClientTransaction2.Start();
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				return;
			}
			lock (m_pLock)
			{
				m_State = state;
				if (m_State == AsyncOP_State.Completed && m_RiseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<GetHostAddressesAsyncOP>(this));
			}
		}
	}

	public class GetHostsAddressesAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private string[] m_pHostNames;

		private bool m_ResolveAny;

		private Dictionary<int, GetHostAddressesAsyncOP> m_pIpLookupQueue;

		private HostEntry[] m_pHostEntries;

		private bool m_RiseCompleted;

		private int m_ResolvedCount;

		public AsyncOP_State State => m_State;

		public Exception Error
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pException;
			}
		}

		public string[] HostNames
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_pHostNames;
			}
		}

		public HostEntry[] HostEntries
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'HostEntries' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_pHostEntries;
			}
		}

		public event EventHandler<EventArgs<GetHostsAddressesAsyncOP>> CompletedAsync;

		public GetHostsAddressesAsyncOP(string[] hostNames)
			: this(hostNames, resolveAny: false)
		{
		}

		public GetHostsAddressesAsyncOP(string[] hostNames, bool resolveAny)
		{
			if (hostNames == null)
			{
				throw new ArgumentNullException("hostNames");
			}
			m_pHostNames = hostNames;
			m_ResolveAny = resolveAny;
			m_pIpLookupQueue = new Dictionary<int, GetHostAddressesAsyncOP>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pHostNames = null;
				m_pIpLookupQueue = null;
				m_pHostEntries = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(Dns_Client dnsClient)
		{
			if (dnsClient == null)
			{
				throw new ArgumentNullException("dnsClient");
			}
			SetState(AsyncOP_State.Active);
			m_pHostEntries = new HostEntry[m_pHostNames.Length];
			Dictionary<int, GetHostAddressesAsyncOP> dictionary = new Dictionary<int, GetHostAddressesAsyncOP>();
			for (int i = 0; i < m_pHostNames.Length; i++)
			{
				GetHostAddressesAsyncOP value = new GetHostAddressesAsyncOP(m_pHostNames[i]);
				m_pIpLookupQueue.Add(i, value);
				dictionary.Add(i, value);
			}
			foreach (KeyValuePair<int, GetHostAddressesAsyncOP> item in dictionary)
			{
				int index = item.Key;
				item.Value.CompletedAsync += delegate(object s1, EventArgs<GetHostAddressesAsyncOP> e1)
				{
					GetHostAddressesCompleted(e1.Value, index);
				};
				if (!dnsClient.GetHostAddressesAsync(item.Value))
				{
					GetHostAddressesCompleted(item.Value, index);
				}
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				return;
			}
			lock (m_pLock)
			{
				m_State = state;
				if (m_State == AsyncOP_State.Completed && m_RiseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void GetHostAddressesCompleted(GetHostAddressesAsyncOP op, int index)
		{
			lock (m_pLock)
			{
				try
				{
					if (op.Error != null)
					{
						if (!m_ResolveAny || (m_ResolvedCount <= 0 && m_pIpLookupQueue.Count <= 1))
						{
							m_pException = op.Error;
						}
					}
					else
					{
						m_pHostEntries[index] = new HostEntry(op.HostNameOrIP, op.Addresses, null);
						m_ResolvedCount++;
					}
					m_pIpLookupQueue.Remove(index);
					if (m_pIpLookupQueue.Count == 0)
					{
						if (m_ResolveAny)
						{
							List<HostEntry> list = new List<HostEntry>();
							HostEntry[] pHostEntries = m_pHostEntries;
							foreach (HostEntry hostEntry in pHostEntries)
							{
								if (hostEntry != null)
								{
									list.Add(hostEntry);
								}
							}
							m_pHostEntries = list.ToArray();
						}
						SetState(AsyncOP_State.Completed);
					}
				}
				catch (Exception pException)
				{
					Exception ex = (m_pException = pException);
					SetState(AsyncOP_State.Completed);
				}
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<GetHostsAddressesAsyncOP>(this));
			}
		}
	}

	public class GetEmailHostsAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private string m_Domain;

		private HostEntry[] m_pHosts;

		private bool m_RiseCompleted;

		public AsyncOP_State State => m_State;

		public Exception Error
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pException;
			}
		}

		public string EmailDomain
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return m_Domain;
			}
		}

		public HostEntry[] Hosts
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_pHosts;
			}
		}

		public event EventHandler<EventArgs<GetEmailHostsAsyncOP>> CompletedAsync;

		public GetEmailHostsAsyncOP(string domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (domain == string.Empty)
			{
				throw new ArgumentException("Argument 'domain' value must be specified.", "domain");
			}
			m_Domain = domain;
			if (domain.IndexOf("@") > -1)
			{
				m_Domain = domain.Split(new char[1] { '@' }, 2)[1];
			}
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_Domain = null;
				m_pHosts = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(Dns_Client dnsClient)
		{
			if (dnsClient == null)
			{
				throw new ArgumentNullException("dnsClient");
			}
			SetState(AsyncOP_State.Active);
			try
			{
				LookupMX(dnsClient, m_Domain, domainIsCName: false);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				return;
			}
			lock (m_pLock)
			{
				m_State = state;
				if (m_State == AsyncOP_State.Completed && m_RiseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void LookupMX(Dns_Client dnsClient, string domain, bool domainIsCName)
		{
			if (dnsClient == null)
			{
				throw new ArgumentNullException("dnsClient");
			}
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			DNS_ClientTransaction transaction_MX = dnsClient.CreateTransaction(DNS_QType.MX, domain, 2000);
			Dictionary<string, int> name_to_index_map2;
			GetHostsAddressesAsyncOP op2;
			Dictionary<string, int> name_to_index_map;
			GetHostsAddressesAsyncOP op;
			transaction_MX.StateChanged += delegate(object s1, EventArgs<DNS_ClientTransaction> e1)
			{
				try
				{
					if (e1.Value.State == DNS_ClientTransactionState.Completed)
					{
						if (e1.Value.Response.ResponseCode == DNS_RCode.NO_ERROR)
						{
							List<DNS_rr_MX> list = new List<DNS_rr_MX>();
							DNS_rr_MX[] mXRecords = e1.Value.Response.GetMXRecords();
							foreach (DNS_rr_MX dNS_rr_MX in mXRecords)
							{
								if (!string.IsNullOrEmpty(dNS_rr_MX.Host))
								{
									list.Add(dNS_rr_MX);
								}
							}
							if (list.Count > 0)
							{
								m_pHosts = new HostEntry[list.Count];
								name_to_index_map2 = new Dictionary<string, int>();
								List<string> list2 = new List<string>();
								for (int j = 0; j < m_pHosts.Length; j++)
								{
									DNS_rr_MX dNS_rr_MX2 = list[j];
									IPAddress[] array = Get_A_or_AAAA_FromResponse(dNS_rr_MX2.Host, e1.Value.Response);
									if (array.Length == 0)
									{
										name_to_index_map2[dNS_rr_MX2.Host] = j;
										list2.Add(dNS_rr_MX2.Host);
									}
									else
									{
										m_pHosts[j] = new HostEntry(dNS_rr_MX2.Host, array, null);
									}
								}
								if (list2.Count > 0)
								{
									op2 = new GetHostsAddressesAsyncOP(list2.ToArray(), resolveAny: true);
									op2.CompletedAsync += delegate
									{
										LookupCompleted(op2, name_to_index_map2);
									};
									if (!dnsClient.GetHostsAddressesAsync(op2))
									{
										LookupCompleted(op2, name_to_index_map2);
									}
								}
								else
								{
									SetState(AsyncOP_State.Completed);
								}
							}
							else if (e1.Value.Response.GetCNAMERecords().Length != 0)
							{
								if (domainIsCName)
								{
									m_pException = new Exception("CNAME to CNAME loop dedected.");
									SetState(AsyncOP_State.Completed);
								}
								else
								{
									LookupMX(dnsClient, e1.Value.Response.GetCNAMERecords()[0].Alias, domainIsCName: true);
								}
							}
							else
							{
								m_pHosts = new HostEntry[1];
								name_to_index_map = new Dictionary<string, int>();
								name_to_index_map.Add(domain, 0);
								op = new GetHostsAddressesAsyncOP(new string[1] { domain });
								op.CompletedAsync += delegate
								{
									LookupCompleted(op, name_to_index_map);
								};
								if (!dnsClient.GetHostsAddressesAsync(op))
								{
									LookupCompleted(op, name_to_index_map);
								}
							}
						}
						else
						{
							m_pException = new DNS_ClientException(e1.Value.Response.ResponseCode);
							SetState(AsyncOP_State.Completed);
						}
					}
					transaction_MX.Timeout += delegate
					{
						m_pException = new IOException("DNS transaction timeout, no response from DNS server.");
						SetState(AsyncOP_State.Completed);
					};
				}
				catch (Exception pException)
				{
					m_pException = pException;
					SetState(AsyncOP_State.Completed);
				}
			};
			transaction_MX.Start();
		}

		private IPAddress[] Get_A_or_AAAA_FromResponse(string name, DnsServerResponse response)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			List<IPAddress> list = new List<IPAddress>();
			List<IPAddress> list2 = new List<IPAddress>();
			DNS_rr[] additionalAnswers = response.AdditionalAnswers;
			foreach (DNS_rr dNS_rr in additionalAnswers)
			{
				if (string.Equals(name, dNS_rr.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					if (dNS_rr is DNS_rr_A)
					{
						list.Add(((DNS_rr_A)dNS_rr).IP);
					}
					else if (dNS_rr is DNS_rr_AAAA)
					{
						list2.Add(((DNS_rr_AAAA)dNS_rr).IP);
					}
				}
			}
			list.AddRange(list2);
			return list.ToArray();
		}

		private void LookupCompleted(GetHostsAddressesAsyncOP op, Dictionary<string, int> name_to_index)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}
			HostEntry[] pHosts;
			if (op.Error != null)
			{
				bool flag = false;
				pHosts = m_pHosts;
				for (int i = 0; i < pHosts.Length; i++)
				{
					if (pHosts[i] != null)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					m_pException = op.Error;
				}
			}
			else
			{
				pHosts = op.HostEntries;
				foreach (HostEntry hostEntry in pHosts)
				{
					m_pHosts[name_to_index[hostEntry.HostName]] = hostEntry;
				}
			}
			op.Dispose();
			List<HostEntry> list = new List<HostEntry>();
			pHosts = m_pHosts;
			foreach (HostEntry hostEntry2 in pHosts)
			{
				if (hostEntry2 != null)
				{
					list.Add(hostEntry2);
				}
			}
			m_pHosts = list.ToArray();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<GetEmailHostsAsyncOP>(this));
			}
		}
	}

	private static Dns_Client m_pDnsClient;

	private static IPAddress[] m_DnsServers;

	private static bool m_UseDnsCache;

	private bool m_IsDisposed;

	private Dictionary<int, DNS_ClientTransaction> m_pTransactions;

	private Socket m_pIPv4Socket;

	private Socket m_pIPv6Socket;

	private List<UDP_DataReceiver> m_pReceivers;

	private Random m_pRandom;

	private DNS_ClientCache m_pCache;

	public static Dns_Client Static
	{
		get
		{
			if (m_pDnsClient == null)
			{
				m_pDnsClient = new Dns_Client();
			}
			return m_pDnsClient;
		}
	}

	public static string[] DnsServers
	{
		get
		{
			string[] array = new string[m_DnsServers.Length];
			for (int i = 0; i < m_DnsServers.Length; i++)
			{
				array[i] = m_DnsServers[i].ToString();
			}
			return array;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			IPAddress[] array = new IPAddress[value.Length];
			for (int i = 0; i < value.Length; i++)
			{
				array[i] = IPAddress.Parse(value[i]);
			}
			m_DnsServers = array;
		}
	}

	public static bool UseDnsCache
	{
		get
		{
			return m_UseDnsCache;
		}
		set
		{
			m_UseDnsCache = value;
		}
	}

	public DNS_ClientCache Cache => m_pCache;

	static Dns_Client()
	{
		m_pDnsClient = null;
		m_DnsServers = null;
		m_UseDnsCache = true;
		try
		{
			List<IPAddress> list = new List<IPAddress>();
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}
				foreach (IPAddress dnsAddress in networkInterface.GetIPProperties().DnsAddresses)
				{
					if (dnsAddress.AddressFamily == AddressFamily.InterNetwork && !list.Contains(dnsAddress))
					{
						list.Add(dnsAddress);
					}
				}
			}
			m_DnsServers = list.ToArray();
		}
		catch
		{
		}
	}

	public Dns_Client()
	{
		m_pTransactions = new Dictionary<int, DNS_ClientTransaction>();
		m_pIPv4Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		m_pIPv4Socket.Bind(new IPEndPoint(IPAddress.Any, 0));
		if (Socket.OSSupportsIPv6)
		{
			m_pIPv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
			m_pIPv6Socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
		}
		m_pReceivers = new List<UDP_DataReceiver>();
		m_pRandom = new Random();
		m_pCache = new DNS_ClientCache();
		for (int i = 0; i < 5; i++)
		{
			UDP_DataReceiver uDP_DataReceiver = new UDP_DataReceiver(m_pIPv4Socket);
			uDP_DataReceiver.PacketReceived += delegate(object s1, UDP_e_PacketReceived e1)
			{
				ProcessUdpPacket(e1);
			};
			m_pReceivers.Add(uDP_DataReceiver);
			uDP_DataReceiver.Start();
			if (m_pIPv6Socket != null)
			{
				UDP_DataReceiver uDP_DataReceiver2 = new UDP_DataReceiver(m_pIPv6Socket);
				uDP_DataReceiver2.PacketReceived += delegate(object s1, UDP_e_PacketReceived e1)
				{
					ProcessUdpPacket(e1);
				};
				m_pReceivers.Add(uDP_DataReceiver2);
				uDP_DataReceiver2.Start();
			}
		}
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		if (m_pReceivers != null)
		{
			foreach (UDP_DataReceiver pReceiver in m_pReceivers)
			{
				pReceiver.Dispose();
			}
			m_pReceivers = null;
		}
		m_pIPv4Socket.Close();
		m_pIPv4Socket = null;
		if (m_pIPv6Socket != null)
		{
			m_pIPv6Socket.Close();
			m_pIPv6Socket = null;
		}
		m_pTransactions = null;
		m_pRandom = null;
		m_pCache.Dispose();
		m_pCache = null;
	}

	public DNS_ClientTransaction CreateTransaction(DNS_QType queryType, string queryText, int timeout)
	{
		List<IPAddress> list = new List<IPAddress>();
		string[] dnsServers = DnsServers;
		foreach (string text in dnsServers)
		{
			if (Net_Utils.IsIPAddress(text))
			{
				list.Add(IPAddress.Parse(text));
			}
		}
		return CreateTransaction(list.ToArray(), queryType, queryText, timeout);
	}

	public DNS_ClientTransaction CreateTransaction(IPAddress[] dnsServers, DNS_QType queryType, string queryText, int timeout)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (dnsServers == null)
		{
			throw new ArgumentNullException("dnsServers");
		}
		if (queryText == null)
		{
			throw new ArgumentNullException("queryText");
		}
		if (queryText == string.Empty)
		{
			throw new ArgumentException("Argument 'queryText' value may not be \"\".", "queryText");
		}
		if (queryType == DNS_QType.PTR)
		{
			IPAddress address = null;
			if (!IPAddress.TryParse(queryText, out address))
			{
				throw new ArgumentException("Argument 'queryText' value must be IP address if queryType == DNS_QType.PTR.", "queryText");
			}
		}
		if (queryType == DNS_QType.PTR)
		{
			string text = queryText;
			IPAddress iPAddress = IPAddress.Parse(text);
			queryText = "";
			if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
			{
				char[] array = text.Replace(":", "").ToCharArray();
				for (int num = array.Length - 1; num > -1; num--)
				{
					queryText = queryText + array[num] + ".";
				}
				queryText += "IP6.ARPA";
			}
			else
			{
				string[] array2 = text.Split('.');
				for (int num2 = 3; num2 > -1; num2--)
				{
					queryText = queryText + array2[num2] + ".";
				}
				queryText += "in-addr.arpa";
			}
		}
		int num3 = 0;
		lock (m_pTransactions)
		{
			do
			{
				num3 = m_pRandom.Next(65535);
			}
			while (m_pTransactions.ContainsKey(num3));
		}
		DNS_ClientTransaction retVal = new DNS_ClientTransaction(this, dnsServers, num3, queryType, queryText, timeout);
		retVal.StateChanged += delegate(object s1, EventArgs<DNS_ClientTransaction> e1)
		{
			if (retVal.State == DNS_ClientTransactionState.Disposed)
			{
				lock (m_pTransactions)
				{
					m_pTransactions.Remove(e1.Value.ID);
				}
			}
		};
		lock (m_pTransactions)
		{
			m_pTransactions.Add(retVal.ID, retVal);
		}
		return retVal;
	}

	public DnsServerResponse Query(string queryText, DNS_QType queryType)
	{
		return Query(queryText, queryType, 2000);
	}

	public DnsServerResponse Query(string queryText, DNS_QType queryType, int timeout)
	{
		List<IPAddress> list = new List<IPAddress>();
		string[] dnsServers = DnsServers;
		foreach (string text in dnsServers)
		{
			if (Net_Utils.IsIPAddress(text))
			{
				list.Add(IPAddress.Parse(text));
			}
		}
		return Query(list.ToArray(), queryText, queryType, timeout);
	}

	public DnsServerResponse Query(IPAddress[] dnsServers, string queryText, DNS_QType queryType, int timeout)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (dnsServers == null)
		{
			throw new ArgumentNullException("dnsServers");
		}
		if (queryText == null)
		{
			throw new ArgumentNullException("queryText");
		}
		DnsServerResponse retVal = null;
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		DNS_ClientTransaction transaction = CreateTransaction(dnsServers, queryType, queryText, timeout);
		transaction.Timeout += delegate
		{
			if (wait != null)
			{
				wait.Set();
			}
		};
		transaction.StateChanged += delegate
		{
			if (transaction.State == DNS_ClientTransactionState.Completed || transaction.State == DNS_ClientTransactionState.Disposed)
			{
				retVal = transaction.Response;
				if (wait != null)
				{
					wait.Set();
				}
			}
		};
		transaction.Start();
		wait.WaitOne();
		wait.Close();
		wait = null;
		return retVal;
	}

	public IPAddress[] GetHostAddresses(string hostNameOrIP)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (hostNameOrIP == null)
		{
			throw new ArgumentNullException("hostNameOrIP");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using GetHostAddressesAsyncOP getHostAddressesAsyncOP = new GetHostAddressesAsyncOP(hostNameOrIP);
		getHostAddressesAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!GetHostAddressesAsync(getHostAddressesAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (getHostAddressesAsyncOP.Error != null)
		{
			throw getHostAddressesAsyncOP.Error;
		}
		return getHostAddressesAsyncOP.Addresses;
	}

	public bool GetHostAddressesAsync(GetHostAddressesAsyncOP op)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		if (op.State != 0)
		{
			throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'AsyncOP_State.WaitingForStart' state.", "op");
		}
		return op.Start(this);
	}

	public HostEntry[] GetHostsAddresses(string[] hostNames)
	{
		return GetHostsAddresses(hostNames, resolveAny: false);
	}

	public HostEntry[] GetHostsAddresses(string[] hostNames, bool resolveAny)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (hostNames == null)
		{
			throw new ArgumentNullException("hostNames");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using GetHostsAddressesAsyncOP getHostsAddressesAsyncOP = new GetHostsAddressesAsyncOP(hostNames, resolveAny);
		getHostsAddressesAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!GetHostsAddressesAsync(getHostsAddressesAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (getHostsAddressesAsyncOP.Error != null)
		{
			throw getHostsAddressesAsyncOP.Error;
		}
		return getHostsAddressesAsyncOP.HostEntries;
	}

	public bool GetHostsAddressesAsync(GetHostsAddressesAsyncOP op)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		if (op.State != 0)
		{
			throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'AsyncOP_State.WaitingForStart' state.", "op");
		}
		return op.Start(this);
	}

	public HostEntry[] GetEmailHosts(string domain)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (domain == string.Empty)
		{
			throw new ArgumentException("Argument 'domain' value must be specified.", "domain");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using GetEmailHostsAsyncOP getEmailHostsAsyncOP = new GetEmailHostsAsyncOP(domain);
		getEmailHostsAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!GetEmailHostsAsync(getEmailHostsAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (getEmailHostsAsyncOP.Error != null)
		{
			throw getEmailHostsAsyncOP.Error;
		}
		return getEmailHostsAsyncOP.Hosts;
	}

	public bool GetEmailHostsAsync(GetEmailHostsAsyncOP op)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		if (op.State != 0)
		{
			throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'AsyncOP_State.WaitingForStart' state.", "op");
		}
		return op.Start(this);
	}

	internal void Send(IPAddress target, byte[] packet, int count)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		try
		{
			if (target.AddressFamily == AddressFamily.InterNetwork)
			{
				m_pIPv4Socket.SendTo(packet, count, SocketFlags.None, new IPEndPoint(target, 53));
			}
			else if (target.AddressFamily == AddressFamily.InterNetworkV6)
			{
				m_pIPv6Socket.SendTo(packet, count, SocketFlags.None, new IPEndPoint(target, 53));
			}
		}
		catch
		{
		}
	}

	private void ProcessUdpPacket(UDP_e_PacketReceived e)
	{
		try
		{
			if (m_IsDisposed)
			{
				return;
			}
			DnsServerResponse dnsServerResponse = ParseQuery(e.Buffer);
			DNS_ClientTransaction value = null;
			if (m_pTransactions.TryGetValue(dnsServerResponse.ID, out value) && value.State == DNS_ClientTransactionState.Active)
			{
				if (m_UseDnsCache && dnsServerResponse.ResponseCode == DNS_RCode.NO_ERROR)
				{
					m_pCache.AddToCache(value.QName, (int)value.QType, dnsServerResponse);
				}
				value.ProcessResponse(dnsServerResponse);
			}
		}
		catch
		{
		}
	}

	internal static bool GetQName(byte[] reply, ref int offset, ref string name)
	{
		bool qNameI = GetQNameI(reply, ref offset, ref name);
		if (name.Length > 0)
		{
			IdnMapping idnMapping = new IdnMapping();
			name = idnMapping.GetUnicode(name);
		}
		return qNameI;
	}

	private static bool GetQNameI(byte[] reply, ref int offset, ref string name)
	{
		try
		{
			while (true)
			{
				if (offset >= reply.Length)
				{
					return false;
				}
				if (reply[offset] == 0)
				{
					break;
				}
				if ((reply[offset] & 0xC0) == 192)
				{
					int offset2 = ((reply[offset] & 0x3F) << 8) | reply[++offset];
					offset++;
					return GetQNameI(reply, ref offset2, ref name);
				}
				int num = reply[offset] & 0x3F;
				offset++;
				name += Encoding.UTF8.GetString(reply, offset, num);
				offset += num;
				if (reply[offset] != 0)
				{
					name += ".";
				}
			}
			offset++;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private DnsServerResponse ParseQuery(byte[] reply)
	{
		int id = (reply[0] << 8) | reply[1];
		_ = (reply[2] >> 3) & 0xF;
		DNS_RCode rcode = (DNS_RCode)(reply[3] & 0xF);
		int num = (reply[4] << 8) | reply[5];
		int answerCount = (reply[6] << 8) | reply[7];
		int answerCount2 = (reply[8] << 8) | reply[9];
		int answerCount3 = (reply[10] << 8) | reply[11];
		int offset = 12;
		for (int i = 0; i < num; i++)
		{
			string name = "";
			GetQName(reply, ref offset, ref name);
			offset += 4;
		}
		List<DNS_rr> answers = ParseAnswers(reply, answerCount, ref offset);
		List<DNS_rr> authoritiveAnswers = ParseAnswers(reply, answerCount2, ref offset);
		List<DNS_rr> additionalAnswers = ParseAnswers(reply, answerCount3, ref offset);
		return new DnsServerResponse(connectionOk: true, id, rcode, answers, authoritiveAnswers, additionalAnswers);
	}

	private List<DNS_rr> ParseAnswers(byte[] reply, int answerCount, ref int offset)
	{
		List<DNS_rr> list = new List<DNS_rr>();
		for (int i = 0; i < answerCount; i++)
		{
			string name = "";
			if (!GetQName(reply, ref offset, ref name))
			{
				break;
			}
			int num = (reply[offset++] << 8) | reply[offset++];
			_ = reply[offset++];
			_ = reply[offset++];
			int ttl = (reply[offset++] << 24) | (reply[offset++] << 16) | (reply[offset++] << 8) | reply[offset++];
			int num2 = (reply[offset++] << 8) | reply[offset++];
			switch (num)
			{
			case 1:
				list.Add(DNS_rr_A.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 2:
				list.Add(DNS_rr_NS.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 5:
				list.Add(DNS_rr_CNAME.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 6:
				list.Add(DNS_rr_SOA.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 12:
				list.Add(DNS_rr_PTR.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 13:
				list.Add(DNS_rr_HINFO.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 15:
				list.Add(DNS_rr_MX.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 16:
				list.Add(DNS_rr_TXT.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 28:
				list.Add(DNS_rr_AAAA.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 33:
				list.Add(DNS_rr_SRV.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 35:
				list.Add(DNS_rr_NAPTR.Parse(name, reply, ref offset, num2, ttl));
				break;
			case 99:
				list.Add(DNS_rr_SPF.Parse(name, reply, ref offset, num2, ttl));
				break;
			default:
				offset += num2;
				break;
			}
		}
		return list;
	}

	internal static string ReadCharacterString(byte[] data, ref int offset)
	{
		int num = data[offset++];
		string @string = Encoding.Default.GetString(data, offset, num);
		offset += num;
		return @string;
	}

	[Obsolete("Use Dns_Client.GetHostAddresses instead.")]
	public static IPAddress[] Resolve(string[] hosts)
	{
		if (hosts == null)
		{
			throw new ArgumentNullException("hosts");
		}
		List<IPAddress> list = new List<IPAddress>();
		for (int i = 0; i < hosts.Length; i++)
		{
			IPAddress[] array = Resolve(hosts[i]);
			foreach (IPAddress item in array)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	[Obsolete("Use Dns_Client.GetHostAddresses instead.")]
	public static IPAddress[] Resolve(string host)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		try
		{
			return new IPAddress[1] { IPAddress.Parse(host) };
		}
		catch
		{
		}
		if (host.IndexOf(".") == -1)
		{
			return Dns.GetHostEntry(host).AddressList;
		}
		using Dns_Client dns_Client = new Dns_Client();
		DnsServerResponse dnsServerResponse = dns_Client.Query(host, DNS_QType.A);
		if (dnsServerResponse.ResponseCode == DNS_RCode.NO_ERROR)
		{
			DNS_rr_A[] aRecords = dnsServerResponse.GetARecords();
			IPAddress[] array = new IPAddress[aRecords.Length];
			for (int i = 0; i < aRecords.Length; i++)
			{
				array[i] = aRecords[i].IP;
			}
			return array;
		}
		throw new Exception(dnsServerResponse.ResponseCode.ToString());
	}
}
