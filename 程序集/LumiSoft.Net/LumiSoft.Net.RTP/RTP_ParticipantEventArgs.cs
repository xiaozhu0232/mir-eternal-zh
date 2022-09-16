using System;

namespace LumiSoft.Net.RTP;

public class RTP_ParticipantEventArgs : EventArgs
{
	private RTP_Participant_Remote m_pParticipant;

	public RTP_Participant_Remote Participant => m_pParticipant;

	public RTP_ParticipantEventArgs(RTP_Participant_Remote participant)
	{
		if (participant == null)
		{
			throw new ArgumentNullException("participant");
		}
		m_pParticipant = participant;
	}
}
