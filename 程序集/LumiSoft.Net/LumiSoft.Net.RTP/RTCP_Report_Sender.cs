using System;

namespace LumiSoft.Net.RTP;

public class RTCP_Report_Sender
{
	private ulong m_NtpTimestamp;

	private uint m_RtpTimestamp;

	private uint m_SenderPacketCount;

	private uint m_SenderOctetCount;

	public ulong NtpTimestamp => m_NtpTimestamp;

	public uint RtpTimestamp => m_RtpTimestamp;

	public uint SenderPacketCount => m_SenderPacketCount;

	public uint SenderOctetCount => m_SenderOctetCount;

	internal RTCP_Report_Sender(RTCP_Packet_SR sr)
	{
		if (sr == null)
		{
			throw new ArgumentNullException("sr");
		}
		m_NtpTimestamp = sr.NtpTimestamp;
		m_RtpTimestamp = sr.RtpTimestamp;
		m_SenderPacketCount = sr.SenderPacketCount;
		m_SenderOctetCount = sr.SenderOctetCount;
	}
}
