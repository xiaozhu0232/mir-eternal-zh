using System;

namespace LumiSoft.Net.RTP;

public class RTCP_Report_Receiver
{
	private uint m_FractionLost;

	private uint m_CumulativePacketsLost;

	private uint m_ExtHigestSeqNumber;

	private uint m_Jitter;

	private uint m_LastSR;

	private uint m_DelaySinceLastSR;

	public uint FractionLost => m_FractionLost;

	public uint CumulativePacketsLost => m_CumulativePacketsLost;

	public uint ExtendedSequenceNumber => m_ExtHigestSeqNumber;

	public uint Jitter => m_Jitter;

	public uint LastSR => m_LastSR;

	public uint DelaySinceLastSR => m_DelaySinceLastSR;

	internal RTCP_Report_Receiver(RTCP_Packet_ReportBlock rr)
	{
		if (rr == null)
		{
			throw new ArgumentNullException("rr");
		}
		m_FractionLost = rr.FractionLost;
		m_CumulativePacketsLost = rr.CumulativePacketsLost;
		m_ExtHigestSeqNumber = rr.ExtendedHighestSeqNo;
		m_Jitter = rr.Jitter;
		m_LastSR = rr.LastSR;
		m_DelaySinceLastSR = rr.DelaySinceLastSR;
	}
}
