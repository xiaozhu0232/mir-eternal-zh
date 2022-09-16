using System.IO;

namespace LumiSoft.Net.SMTP.Relay;

public class Relay_QueueItem
{
	private Relay_Queue m_pQueue;

	private Relay_SmartHost m_pTargetServer;

	private string m_From = "";

	private string m_EnvelopeID;

	private SMTP_DSN_Ret m_DSN_Ret;

	private string m_To = "";

	private string m_OriginalRecipient;

	private SMTP_DSN_Notify m_DSN_Notify;

	private string m_MessageID = "";

	private Stream m_pMessageStream;

	private object m_pTag;

	public Relay_Queue Queue => m_pQueue;

	public Relay_SmartHost TargetServer => m_pTargetServer;

	public string From => m_From;

	public string EnvelopeID => m_EnvelopeID;

	public SMTP_DSN_Ret DSN_Ret => m_DSN_Ret;

	public string To => m_To;

	public string OriginalRecipient => m_OriginalRecipient;

	public SMTP_DSN_Notify DSN_Notify => m_DSN_Notify;

	public string MessageID => m_MessageID;

	public Stream MessageStream => m_pMessageStream;

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	internal Relay_QueueItem(Relay_Queue queue, Relay_SmartHost targetServer, string from, string envelopeID, SMTP_DSN_Ret ret, string to, string originalRecipient, SMTP_DSN_Notify notify, string messageID, Stream message, object tag)
	{
		m_pQueue = queue;
		m_pTargetServer = targetServer;
		m_From = from;
		m_EnvelopeID = envelopeID;
		m_DSN_Ret = ret;
		m_To = to;
		m_OriginalRecipient = originalRecipient;
		m_DSN_Notify = notify;
		m_MessageID = messageID;
		m_pMessageStream = message;
		m_pTag = tag;
	}
}
