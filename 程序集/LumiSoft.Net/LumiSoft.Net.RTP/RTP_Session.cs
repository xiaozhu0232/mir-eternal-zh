using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using LumiSoft.Net.Media.Codec;
using LumiSoft.Net.STUN.Client;
using LumiSoft.Net.UDP;

namespace LumiSoft.Net.RTP;

public class RTP_Session : IDisposable
{
	private object m_pLock = new object();

	private bool m_IsDisposed;

	private bool m_IsStarted;

	private RTP_MultimediaSession m_pSession;

	private RTP_Address m_pLocalEP;

	private RTP_Clock m_pRtpClock;

	private RTP_StreamMode m_StreamMode = RTP_StreamMode.SendReceive;

	private List<RTP_Address> m_pTargets;

	private int m_Payload;

	private int m_Bandwidth = 64000;

	private List<RTP_Source_Local> m_pLocalSources;

	private RTP_Source m_pRtcpSource;

	private Dictionary<uint, RTP_Source> m_pMembers;

	private int m_PMembersCount;

	private Dictionary<uint, RTP_Source> m_pSenders;

	private Dictionary<string, DateTime> m_pConflictingEPs;

	private List<UDP_DataReceiver> m_pUdpDataReceivers;

	private Socket m_pRtpSocket;

	private Socket m_pRtcpSocket;

	private long m_RtpPacketsSent;

	private long m_RtpBytesSent;

	private long m_RtpPacketsReceived;

	private long m_RtpBytesReceived;

	private long m_RtpFailedTransmissions;

	private long m_RtcpPacketsSent;

	private long m_RtcpBytesSent;

	private long m_RtcpPacketsReceived;

	private long m_RtcpBytesReceived;

	private double m_RtcpAvgPacketSize;

	private long m_RtcpFailedTransmissions;

	private long m_RtcpUnknownPacketsReceived;

	private DateTime m_RtcpLastTransmission = DateTime.MinValue;

	private long m_LocalCollisions;

	private long m_RemoteCollisions;

	private long m_LocalPacketsLooped;

	private long m_RemotePacketsLooped;

	private int m_MTU = 1400;

	private TimerEx m_pRtcpTimer;

	private KeyValueCollection<int, Codec> m_pPayloads;

	public bool IsDisposed => m_IsDisposed;

	public RTP_MultimediaSession Session
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSession;
		}
	}

	public RTP_Address LocalEP
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pLocalEP;
		}
	}

	public RTP_Clock RtpClock
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pRtpClock;
		}
	}

	public RTP_StreamMode StreamMode
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_StreamMode;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_StreamMode = value;
		}
	}

	public RTP_Address[] Targets
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pTargets.ToArray();
		}
	}

	public int MTU
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_MTU;
		}
	}

	public int Payload
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Payload;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_Payload != value)
			{
				m_Payload = value;
				OnPayloadChanged();
			}
		}
	}

	public int Bandwidth
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Bandwidth;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value < 8)
			{
				throw new ArgumentException("Property 'Bandwidth' value must be >= 8.");
			}
			m_Bandwidth = value;
		}
	}

	public RTP_Source[] Members
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (m_pMembers)
			{
				RTP_Source[] array = new RTP_Source[m_pMembers.Count];
				m_pMembers.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public RTP_Source[] Senders
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (m_pSenders)
			{
				RTP_Source[] array = new RTP_Source[m_pSenders.Count];
				m_pSenders.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public RTP_SendStream[] SendStreams
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (m_pLocalSources)
			{
				List<RTP_SendStream> list = new List<RTP_SendStream>();
				foreach (RTP_Source_Local pLocalSource in m_pLocalSources)
				{
					if (pLocalSource.Stream != null)
					{
						list.Add(pLocalSource.Stream);
					}
				}
				return list.ToArray();
			}
		}
	}

	public RTP_ReceiveStream[] ReceiveStreams
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (m_pSenders)
			{
				List<RTP_ReceiveStream> list = new List<RTP_ReceiveStream>();
				foreach (RTP_Source value in m_pSenders.Values)
				{
					if (!value.IsLocal)
					{
						list.Add(((RTP_Source_Remote)value).Stream);
					}
				}
				return list.ToArray();
			}
		}
	}

	public long RtpPacketsSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpPacketsSent;
		}
	}

	public long RtpBytesSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpBytesSent;
		}
	}

	public long RtpPacketsReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpPacketsReceived;
		}
	}

	public long RtpBytesReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpBytesReceived;
		}
	}

	public long RtpFailedTransmissions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtpFailedTransmissions;
		}
	}

	public long RtcpPacketsSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtcpPacketsSent;
		}
	}

	public long RtcpBytesSent
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtcpBytesSent;
		}
	}

	public long RtcpPacketsReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtcpPacketsReceived;
		}
	}

	public long RtcpBytesReceived
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtcpBytesReceived;
		}
	}

	public long RtcpFailedTransmissions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtcpFailedTransmissions;
		}
	}

	public int RtcpInterval
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (int)(m_pRtcpTimer.Interval / 1000.0);
		}
	}

	public DateTime RtcpLastTransmission
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RtcpLastTransmission;
		}
	}

	public long LocalCollisions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LocalCollisions;
		}
	}

	public long RemoteCollisions
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RemoteCollisions;
		}
	}

	public long LocalPacketsLooped
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LocalPacketsLooped;
		}
	}

	public long RemotePacketsLooped
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_RemotePacketsLooped;
		}
	}

	public KeyValueCollection<int, Codec> Payloads
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pPayloads;
		}
	}

	public event EventHandler Disposed;

	public event EventHandler Closed;

	public event EventHandler<RTP_SendStreamEventArgs> NewSendStream;

	public event EventHandler<RTP_ReceiveStreamEventArgs> NewReceiveStream;

	public event EventHandler PayloadChanged;

	internal RTP_Session(RTP_MultimediaSession session, RTP_Address localEP, RTP_Clock clock)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		if (clock == null)
		{
			throw new ArgumentNullException("clock");
		}
		m_pSession = session;
		m_pLocalEP = localEP;
		m_pRtpClock = clock;
		m_pLocalSources = new List<RTP_Source_Local>();
		m_pTargets = new List<RTP_Address>();
		m_pMembers = new Dictionary<uint, RTP_Source>();
		m_pSenders = new Dictionary<uint, RTP_Source>();
		m_pConflictingEPs = new Dictionary<string, DateTime>();
		m_pPayloads = new KeyValueCollection<int, Codec>();
		m_pUdpDataReceivers = new List<UDP_DataReceiver>();
		m_pRtpSocket = new Socket(localEP.IP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		m_pRtpSocket.Bind(localEP.RtpEP);
		m_pRtcpSocket = new Socket(localEP.IP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		m_pRtcpSocket.Bind(localEP.RtcpEP);
		m_pRtcpTimer = new TimerEx();
		m_pRtcpTimer.Elapsed += delegate
		{
			SendRtcp();
		};
		m_pRtcpTimer.AutoReset = false;
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		foreach (UDP_DataReceiver pUdpDataReceiver in m_pUdpDataReceivers)
		{
			pUdpDataReceiver.Dispose();
		}
		m_pUdpDataReceivers = null;
		if (m_pRtcpTimer != null)
		{
			m_pRtcpTimer.Dispose();
			m_pRtcpTimer = null;
		}
		m_pSession = null;
		m_pLocalEP = null;
		m_pTargets = null;
		RTP_Source_Local[] array = m_pLocalSources.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
		m_pLocalSources = null;
		m_pRtcpSource = null;
		foreach (RTP_Source value in m_pMembers.Values)
		{
			value.Dispose();
		}
		m_pMembers = null;
		m_pSenders = null;
		m_pConflictingEPs = null;
		m_pRtpSocket.Close();
		m_pRtpSocket = null;
		m_pRtcpSocket.Close();
		m_pRtcpSocket = null;
		m_pUdpDataReceivers = null;
		OnDisposed();
		this.Disposed = null;
		this.Closed = null;
		this.NewSendStream = null;
		this.NewReceiveStream = null;
	}

	public void Close(string closeReason)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		RTCP_CompoundPacket rTCP_CompoundPacket = new RTCP_CompoundPacket();
		RTCP_Packet_RR rTCP_Packet_RR = new RTCP_Packet_RR();
		rTCP_Packet_RR.SSRC = m_pRtcpSource.SSRC;
		rTCP_CompoundPacket.Packets.Add(rTCP_Packet_RR);
		RTCP_Packet_SDES rTCP_Packet_SDES = new RTCP_Packet_SDES();
		rTCP_Packet_SDES.Chunks.Add(new RTCP_Packet_SDES_Chunk(m_pRtcpSource.SSRC, m_pSession.LocalParticipant.CNAME));
		rTCP_CompoundPacket.Packets.Add(rTCP_Packet_SDES);
		int num = 0;
		while (num < m_pLocalSources.Count)
		{
			uint[] array = new uint[Math.Min(m_pLocalSources.Count - num, 31)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = m_pLocalSources[num].SSRC;
				num++;
			}
			RTCP_Packet_BYE rTCP_Packet_BYE = new RTCP_Packet_BYE();
			rTCP_Packet_BYE.Sources = array;
			rTCP_CompoundPacket.Packets.Add(rTCP_Packet_BYE);
		}
		SendRtcpPacket(rTCP_CompoundPacket);
		OnClosed();
		Dispose();
	}

	public void Start()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!m_IsStarted)
		{
			m_IsStarted = true;
			m_PMembersCount = 1;
			m_RtcpAvgPacketSize = 100.0;
			m_pRtcpSource = CreateLocalSource();
			m_pMembers.Add(m_pRtcpSource.SSRC, m_pRtcpSource);
			UDP_DataReceiver uDP_DataReceiver = new UDP_DataReceiver(m_pRtpSocket);
			uDP_DataReceiver.PacketReceived += delegate(object s1, UDP_e_PacketReceived e1)
			{
				ProcessRtp(e1.Buffer, e1.Count, e1.RemoteEP);
			};
			m_pUdpDataReceivers.Add(uDP_DataReceiver);
			uDP_DataReceiver.Start();
			UDP_DataReceiver uDP_DataReceiver2 = new UDP_DataReceiver(m_pRtcpSocket);
			uDP_DataReceiver2.PacketReceived += delegate(object s1, UDP_e_PacketReceived e1)
			{
				ProcessRtcp(e1.Buffer, e1.Count, e1.RemoteEP);
			};
			m_pUdpDataReceivers.Add(uDP_DataReceiver2);
			uDP_DataReceiver2.Start();
			Schedule(ComputeRtcpTransmissionInterval(m_pMembers.Count, m_pSenders.Count, (double)m_Bandwidth * 0.25, we_sent: false, m_RtcpAvgPacketSize, initial: true));
		}
	}

	public void Stop()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		throw new NotImplementedException();
	}

	public RTP_SendStream CreateSendStream()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		RTP_Source_Local rTP_Source_Local = CreateLocalSource();
		rTP_Source_Local.CreateStream();
		OnNewSendStream(rTP_Source_Local.Stream);
		return rTP_Source_Local.Stream;
	}

	public void AddTarget(RTP_Address target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (m_pLocalEP.Equals(target))
		{
			throw new ArgumentException("Argument 'target' value collapses with property 'LocalEP'.", "target");
		}
		RTP_Address[] targets = Targets;
		for (int i = 0; i < targets.Length; i++)
		{
			if (targets[i].Equals(target))
			{
				throw new ArgumentException("Specified target already exists.", "target");
			}
		}
		m_pTargets.Add(target);
	}

	public void RemoveTarget(RTP_Address target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		m_pTargets.Remove(target);
	}

	public void RemoveTargets()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		m_pTargets.Clear();
	}

	public bool StunPublicEndPoints(string server, int port, out IPEndPoint rtpEP, out IPEndPoint rtcpEP)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (m_IsStarted)
		{
			throw new InvalidOperationException("Method 'StunPublicEndPoints' may be called only if RTP session has not started.");
		}
		rtpEP = null;
		rtcpEP = null;
		try
		{
			STUN_Result sTUN_Result = STUN_Client.Query(server, port, m_pRtpSocket);
			if (sTUN_Result.NetType == STUN_NetType.FullCone || sTUN_Result.NetType == STUN_NetType.PortRestrictedCone || sTUN_Result.NetType == STUN_NetType.RestrictedCone)
			{
				rtpEP = sTUN_Result.PublicEndPoint;
				rtcpEP = STUN_Client.GetPublicEP(server, port, m_pRtcpSocket);
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	internal int SendRtcpPacket(RTCP_CompoundPacket packet)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		byte[] array = packet.ToByte();
		RTP_Address[] targets = Targets;
		foreach (RTP_Address rTP_Address in targets)
		{
			try
			{
				m_pRtcpSocket.SendTo(array, array.Length, SocketFlags.None, rTP_Address.RtcpEP);
				m_RtcpPacketsSent++;
				m_RtcpBytesSent += array.Length;
				_ = array.Length;
				m_RtcpAvgPacketSize = 0.0 + 0.0 * m_RtcpAvgPacketSize;
			}
			catch
			{
				m_RtcpFailedTransmissions++;
			}
		}
		return array.Length;
	}

	internal int SendRtpPacket(RTP_SendStream stream, RTP_Packet packet)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		lock (m_pMembers)
		{
			if (!m_pMembers.ContainsKey(stream.Source.SSRC))
			{
				m_pMembers.Add(stream.Source.SSRC, stream.Source);
			}
		}
		lock (m_pSenders)
		{
			if (!m_pSenders.ContainsKey(stream.Source.SSRC))
			{
				m_pSenders.Add(stream.Source.SSRC, stream.Source);
			}
		}
		byte[] buffer = new byte[m_MTU];
		int offset = 0;
		packet.ToByte(buffer, ref offset);
		RTP_Address[] targets = Targets;
		foreach (RTP_Address rTP_Address in targets)
		{
			try
			{
				m_pRtpSocket.BeginSendTo(buffer, 0, offset, SocketFlags.None, rTP_Address.RtpEP, RtpAsyncSocketSendCompleted, null);
			}
			catch
			{
				m_RtpFailedTransmissions++;
			}
		}
		return offset;
	}

	private void ProcessRtcp(byte[] buffer, int count, IPEndPoint remoteEP)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		m_RtcpPacketsReceived++;
		m_RtcpBytesReceived += count;
		m_RtcpAvgPacketSize = 0.0 + 0.0 * m_RtcpAvgPacketSize;
		try
		{
			foreach (RTCP_Packet packet in RTCP_CompoundPacket.Parse(buffer, count).Packets)
			{
				if (packet.Type == 204)
				{
					RTCP_Packet_APP rTCP_Packet_APP = (RTCP_Packet_APP)packet;
					RTP_Source_Remote orCreateSource = GetOrCreateSource(rtcp_rtp: true, rTCP_Packet_APP.Source, null, remoteEP);
					if (orCreateSource != null)
					{
						orCreateSource.SetLastRtcpPacket(DateTime.Now);
						orCreateSource.OnAppPacket(rTCP_Packet_APP);
					}
				}
				else if (packet.Type == 203)
				{
					RTCP_Packet_BYE rTCP_Packet_BYE = (RTCP_Packet_BYE)packet;
					bool flag = false;
					uint[] sources = rTCP_Packet_BYE.Sources;
					foreach (uint num in sources)
					{
						RTP_Source orCreateSource2 = GetOrCreateSource(rtcp_rtp: true, num, null, remoteEP);
						if (orCreateSource2 != null)
						{
							flag = true;
							m_pMembers.Remove(num);
							orCreateSource2.Close(rTCP_Packet_BYE.LeavingReason);
						}
						m_pSenders.Remove(num);
					}
					if (flag)
					{
						DoReverseReconsideration();
					}
				}
				else if (packet.Type == 201)
				{
					RTCP_Packet_RR rTCP_Packet_RR = (RTCP_Packet_RR)packet;
					RTP_Source orCreateSource3 = GetOrCreateSource(rtcp_rtp: true, rTCP_Packet_RR.SSRC, null, remoteEP);
					if (orCreateSource3 == null)
					{
						continue;
					}
					orCreateSource3.SetLastRtcpPacket(DateTime.Now);
					foreach (RTCP_Packet_ReportBlock reportBlock in rTCP_Packet_RR.ReportBlocks)
					{
						orCreateSource3 = GetOrCreateSource(rtcp_rtp: true, rTCP_Packet_RR.SSRC, null, remoteEP);
						if (orCreateSource3 != null)
						{
							orCreateSource3.SetLastRtcpPacket(DateTime.Now);
							orCreateSource3.SetRR(reportBlock);
						}
					}
				}
				else if (packet.Type == 202)
				{
					foreach (RTCP_Packet_SDES_Chunk chunk in ((RTCP_Packet_SDES)packet).Chunks)
					{
						RTP_Source orCreateSource4 = GetOrCreateSource(rtcp_rtp: true, chunk.Source, chunk.CName, remoteEP);
						if (orCreateSource4 != null)
						{
							orCreateSource4.SetLastRtcpPacket(DateTime.Now);
							RTP_Participant_Remote orCreateParticipant = m_pSession.GetOrCreateParticipant(string.IsNullOrEmpty(chunk.CName) ? "null" : chunk.CName);
							((RTP_Source_Remote)orCreateSource4).SetParticipant(orCreateParticipant);
							orCreateParticipant.EnsureSource(orCreateSource4);
							orCreateParticipant.Update(chunk);
						}
					}
				}
				else if (packet.Type == 200)
				{
					RTCP_Packet_SR rTCP_Packet_SR = (RTCP_Packet_SR)packet;
					RTP_Source_Remote orCreateSource5 = GetOrCreateSource(rtcp_rtp: true, rTCP_Packet_SR.SSRC, null, remoteEP);
					if (orCreateSource5 == null)
					{
						continue;
					}
					orCreateSource5.SetLastRtcpPacket(DateTime.Now);
					orCreateSource5.OnSenderReport(new RTCP_Report_Sender(rTCP_Packet_SR));
					foreach (RTCP_Packet_ReportBlock reportBlock2 in rTCP_Packet_SR.ReportBlocks)
					{
						orCreateSource5 = GetOrCreateSource(rtcp_rtp: true, rTCP_Packet_SR.SSRC, null, remoteEP);
						if (orCreateSource5 != null)
						{
							orCreateSource5.SetLastRtcpPacket(DateTime.Now);
							orCreateSource5.SetRR(reportBlock2);
						}
					}
				}
				else
				{
					m_RtcpUnknownPacketsReceived++;
				}
			}
		}
		catch (Exception exception)
		{
			m_pSession.OnError(exception);
		}
	}

	private void ProcessRtp(byte[] buffer, int count, IPEndPoint remoteEP)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		m_RtpPacketsReceived++;
		m_RtpBytesReceived += count;
		try
		{
			RTP_Packet rTP_Packet = RTP_Packet.Parse(buffer, count);
			RTP_Source orCreateSource = GetOrCreateSource(rtcp_rtp: false, rTP_Packet.SSRC, null, remoteEP);
			if (orCreateSource == null)
			{
				return;
			}
			uint[] cSRC = rTP_Packet.CSRC;
			for (int i = 0; i < cSRC.Length; i++)
			{
				_ = cSRC[i];
				GetOrCreateSource(rtcp_rtp: false, rTP_Packet.SSRC, null, remoteEP);
			}
			lock (m_pSenders)
			{
				if (!m_pSenders.ContainsKey(orCreateSource.SSRC))
				{
					m_pSenders.Add(orCreateSource.SSRC, orCreateSource);
				}
			}
			((RTP_Source_Remote)orCreateSource).OnRtpPacketReceived(rTP_Packet, count);
		}
		catch (Exception exception)
		{
			m_pSession.OnError(exception);
		}
	}

	internal RTP_Source_Local CreateLocalSource()
	{
		uint num = RTP_Utils.GenerateSSRC();
		while (m_pMembers.ContainsKey(num))
		{
			num = RTP_Utils.GenerateSSRC();
		}
		RTP_Source_Local source = new RTP_Source_Local(this, num, m_pLocalEP.RtcpEP, m_pLocalEP.RtpEP);
		source.Disposing += delegate
		{
			m_pSenders.Remove(source.SSRC);
			m_pMembers.Remove(source.SSRC);
			m_pLocalSources.Remove(source);
		};
		m_pLocalSources.Add(source);
		m_pSession.LocalParticipant.EnsureSource(source);
		return source;
	}

	private RTP_Source_Remote GetOrCreateSource(bool rtcp_rtp, uint src, string cname, IPEndPoint packetEP)
	{
		if (packetEP == null)
		{
			throw new ArgumentNullException("packetEP");
		}
		RTP_Source value = null;
		lock (m_pMembers)
		{
			m_pMembers.TryGetValue(src, out value);
			if (value == null)
			{
				value = new RTP_Source_Remote(this, src);
				if (rtcp_rtp)
				{
					value.SetRtcpEP(packetEP);
				}
				else
				{
					value.SetRtpEP(packetEP);
				}
				m_pMembers.Add(src, value);
			}
			else if ((rtcp_rtp ? value.RtcpEP : value.RtpEP) == null)
			{
				if (rtcp_rtp)
				{
					value.SetRtcpEP(packetEP);
				}
				else
				{
					value.SetRtpEP(packetEP);
				}
			}
			else if (!packetEP.Equals(rtcp_rtp ? value.RtcpEP : value.RtpEP))
			{
				if (!value.IsLocal)
				{
					if (cname != null && cname != value.CName)
					{
						m_RemoteCollisions++;
					}
					else
					{
						m_RemotePacketsLooped++;
					}
					return null;
				}
				if (m_pConflictingEPs.ContainsKey(packetEP.ToString()))
				{
					if (cname == null || cname == value.CName)
					{
						m_LocalPacketsLooped++;
					}
					m_pConflictingEPs[packetEP.ToString()] = DateTime.Now;
					return null;
				}
				m_LocalCollisions++;
				m_pConflictingEPs.Add(packetEP.ToString(), DateTime.Now);
				m_pMembers.Remove(value.SSRC);
				m_pSenders.Remove(value.SSRC);
				uint sSRC = value.SSRC;
				value.GenerateNewSSRC();
				while (m_pMembers.ContainsKey(value.SSRC))
				{
					value.GenerateNewSSRC();
				}
				m_pMembers.Add(value.SSRC, value);
				RTCP_CompoundPacket rTCP_CompoundPacket = new RTCP_CompoundPacket();
				RTCP_Packet_RR rTCP_Packet_RR = new RTCP_Packet_RR();
				rTCP_Packet_RR.SSRC = m_pRtcpSource.SSRC;
				rTCP_CompoundPacket.Packets.Add(rTCP_Packet_RR);
				RTCP_Packet_SDES rTCP_Packet_SDES = new RTCP_Packet_SDES();
				RTCP_Packet_SDES_Chunk item = new RTCP_Packet_SDES_Chunk(value.SSRC, m_pSession.LocalParticipant.CNAME);
				rTCP_Packet_SDES.Chunks.Add(item);
				rTCP_CompoundPacket.Packets.Add(rTCP_Packet_SDES);
				RTCP_Packet_BYE rTCP_Packet_BYE = new RTCP_Packet_BYE();
				rTCP_Packet_BYE.Sources = new uint[1] { sSRC };
				rTCP_Packet_BYE.LeavingReason = "Collision, changing SSRC.";
				rTCP_CompoundPacket.Packets.Add(rTCP_Packet_BYE);
				SendRtcpPacket(rTCP_CompoundPacket);
				value = new RTP_Source_Remote(this, src);
				if (rtcp_rtp)
				{
					value.SetRtcpEP(packetEP);
				}
				else
				{
					value.SetRtpEP(packetEP);
				}
				m_pMembers.Add(src, value);
			}
		}
		return (RTP_Source_Remote)value;
	}

	private void Schedule(int seconds)
	{
		m_pRtcpTimer.Stop();
		m_pRtcpTimer.Interval = seconds * 1000;
		m_pRtcpTimer.Enabled = true;
	}

	private int ComputeRtcpTransmissionInterval(int members, int senders, double rtcp_bw, bool we_sent, double avg_rtcp_size, bool initial)
	{
		double num = 0.25;
		double num2 = 1.21828;
		double num3 = 5.0;
		if (initial)
		{
			num3 /= 2.0;
		}
		int num4 = members;
		if ((double)senders <= (double)members * num)
		{
			if (we_sent)
			{
				rtcp_bw *= num;
				num4 = senders;
			}
			else
			{
				rtcp_bw *= num;
				num4 -= senders;
			}
		}
		double num5 = avg_rtcp_size * (double)num4 / rtcp_bw;
		if (num5 < num3)
		{
			num5 = num3;
		}
		num5 *= (double)new Random().Next(5, 15) / 10.0;
		num5 /= num2;
		return (int)Math.Max(num5, 2.0);
	}

	private void DoReverseReconsideration()
	{
		DateTime dateTime = ((m_RtcpLastTransmission == DateTime.MinValue) ? DateTime.Now : m_RtcpLastTransmission.AddMilliseconds(m_pRtcpTimer.Interval));
		Schedule((int)Math.Max((double)(m_pMembers.Count / m_PMembersCount) * (dateTime - DateTime.Now).TotalSeconds, 2.0));
		m_PMembersCount = m_pMembers.Count;
	}

	private void TimeOutSsrc()
	{
		bool flag = false;
		RTP_Source[] array = new RTP_Source[m_pSenders.Count];
		m_pSenders.Values.CopyTo(array, 0);
		RTP_Source[] array2 = array;
		foreach (RTP_Source rTP_Source in array2)
		{
			if (rTP_Source.LastRtpPacket.AddMilliseconds(2.0 * m_pRtcpTimer.Interval) < DateTime.Now)
			{
				m_pSenders.Remove(rTP_Source.SSRC);
				rTP_Source.SetActivePassive(active: false);
			}
		}
		int num = ComputeRtcpTransmissionInterval(m_pMembers.Count, m_pSenders.Count, (double)m_Bandwidth * 0.25, we_sent: false, m_RtcpAvgPacketSize, initial: false);
		array2 = Members;
		foreach (RTP_Source rTP_Source2 in array2)
		{
			if (rTP_Source2.LastActivity.AddSeconds(5 * num) < DateTime.Now)
			{
				m_pMembers.Remove(rTP_Source2.SSRC);
				if (!rTP_Source2.IsLocal)
				{
					rTP_Source2.Dispose();
				}
				flag = true;
			}
		}
		if (flag)
		{
			DoReverseReconsideration();
		}
	}

	private void SendRtcp()
	{
		bool flag = false;
		try
		{
			m_pRtcpSource.SetLastRtcpPacket(DateTime.Now);
			RTCP_CompoundPacket rTCP_CompoundPacket = new RTCP_CompoundPacket();
			RTCP_Packet_RR rTCP_Packet_RR = null;
			List<RTP_SendStream> list = new List<RTP_SendStream>();
			RTP_SendStream[] sendStreams = SendStreams;
			foreach (RTP_SendStream rTP_SendStream in sendStreams)
			{
				if (rTP_SendStream.RtcpCyclesSinceWeSent < 2)
				{
					list.Add(rTP_SendStream);
					flag = true;
				}
				rTP_SendStream.RtcpCycle();
			}
			if (flag)
			{
				for (int j = 0; j < list.Count; j++)
				{
					RTP_SendStream rTP_SendStream2 = list[j];
					RTCP_Packet_SR rTCP_Packet_SR = new RTCP_Packet_SR(rTP_SendStream2.Source.SSRC);
					rTCP_Packet_SR.NtpTimestamp = RTP_Utils.DateTimeToNTP64(DateTime.Now);
					rTCP_Packet_SR.RtpTimestamp = m_pRtpClock.RtpTimestamp;
					rTCP_Packet_SR.SenderPacketCount = (uint)rTP_SendStream2.RtpPacketsSent;
					rTCP_Packet_SR.SenderOctetCount = (uint)rTP_SendStream2.RtpBytesSent;
					rTCP_CompoundPacket.Packets.Add(rTCP_Packet_SR);
				}
			}
			else
			{
				rTCP_Packet_RR = new RTCP_Packet_RR();
				rTCP_Packet_RR.SSRC = m_pRtcpSource.SSRC;
				rTCP_CompoundPacket.Packets.Add(rTCP_Packet_RR);
			}
			RTCP_Packet_SDES rTCP_Packet_SDES = new RTCP_Packet_SDES();
			RTCP_Packet_SDES_Chunk rTCP_Packet_SDES_Chunk = new RTCP_Packet_SDES_Chunk(m_pRtcpSource.SSRC, m_pSession.LocalParticipant.CNAME);
			m_pSession.LocalParticipant.AddNextOptionalSdesItem(rTCP_Packet_SDES_Chunk);
			rTCP_Packet_SDES.Chunks.Add(rTCP_Packet_SDES_Chunk);
			foreach (RTP_SendStream item in list)
			{
				rTCP_Packet_SDES.Chunks.Add(new RTCP_Packet_SDES_Chunk(item.Source.SSRC, m_pSession.LocalParticipant.CNAME));
			}
			rTCP_CompoundPacket.Packets.Add(rTCP_Packet_SDES);
			RTP_Source[] senders = Senders;
			DateTime[] array = new DateTime[senders.Length];
			RTP_ReceiveStream[] array2 = new RTP_ReceiveStream[senders.Length];
			int num = 0;
			RTP_Source[] array3 = senders;
			foreach (RTP_Source rTP_Source in array3)
			{
				if (!rTP_Source.IsLocal && rTP_Source.LastRtpPacket > m_RtcpLastTransmission)
				{
					array[num] = rTP_Source.LastRRTime;
					array2[num] = ((RTP_Source_Remote)rTP_Source).Stream;
					num++;
				}
			}
			if (rTCP_Packet_RR == null)
			{
				rTCP_Packet_RR = new RTCP_Packet_RR();
				rTCP_Packet_RR.SSRC = m_pRtcpSource.SSRC;
				rTCP_CompoundPacket.Packets.Add(rTCP_Packet_RR);
			}
			Array.Sort(array, array2, 0, num);
			for (int k = 1; k < 31 && num - k >= 0; k++)
			{
				rTCP_Packet_RR.ReportBlocks.Add(array2[num - k].CreateReceiverReport());
			}
			SendRtcpPacket(rTCP_CompoundPacket);
			lock (m_pConflictingEPs)
			{
				string[] array4 = new string[m_pConflictingEPs.Count];
				m_pConflictingEPs.Keys.CopyTo(array4, 0);
				string[] array5 = array4;
				foreach (string key in array5)
				{
					if (m_pConflictingEPs[key].AddMinutes(3.0) < DateTime.Now)
					{
						m_pConflictingEPs.Remove(key);
					}
				}
			}
			TimeOutSsrc();
		}
		catch (Exception exception)
		{
			if (IsDisposed)
			{
				return;
			}
			m_pSession.OnError(exception);
		}
		m_RtcpLastTransmission = DateTime.Now;
		Schedule(ComputeRtcpTransmissionInterval(m_pMembers.Count, m_pSenders.Count, (double)m_Bandwidth * 0.25, flag, m_RtcpAvgPacketSize, initial: false));
	}

	private void RtpAsyncSocketSendCompleted(IAsyncResult ar)
	{
		try
		{
			m_RtpBytesSent += m_pRtpSocket.EndSendTo(ar);
			m_RtpPacketsSent++;
		}
		catch
		{
			m_RtpFailedTransmissions++;
		}
	}

	private void OnDisposed()
	{
		if (this.Disposed != null)
		{
			this.Disposed(this, new EventArgs());
		}
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed(this, new EventArgs());
		}
	}

	private void OnNewSendStream(RTP_SendStream stream)
	{
		if (this.NewSendStream != null)
		{
			this.NewSendStream(this, new RTP_SendStreamEventArgs(stream));
		}
	}

	internal void OnNewReceiveStream(RTP_ReceiveStream stream)
	{
		if (this.NewReceiveStream != null)
		{
			this.NewReceiveStream(this, new RTP_ReceiveStreamEventArgs(stream));
		}
	}

	private void OnPayloadChanged()
	{
		if (this.PayloadChanged != null)
		{
			this.PayloadChanged(this, new EventArgs());
		}
	}
}
