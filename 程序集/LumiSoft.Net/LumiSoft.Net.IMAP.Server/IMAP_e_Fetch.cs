using System;
using LumiSoft.Net.Mail;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_e_Fetch : EventArgs
{
	internal class e_NewMessageData : EventArgs
	{
		private IMAP_MessageInfo m_pMsgInfo;

		private Mail_Message m_pMsgData;

		public IMAP_MessageInfo MessageInfo => m_pMsgInfo;

		public Mail_Message MessageData => m_pMsgData;

		public e_NewMessageData(IMAP_MessageInfo msgInfo, Mail_Message msgData)
		{
			if (msgInfo == null)
			{
				throw new ArgumentNullException("msgInfo");
			}
			m_pMsgInfo = msgInfo;
			m_pMsgData = msgData;
		}
	}

	private IMAP_r_ServerStatus m_pResponse;

	private IMAP_MessageInfo[] m_pMessagesInfo;

	private IMAP_Fetch_DataType m_FetchDataType = IMAP_Fetch_DataType.FullMessage;

	public IMAP_r_ServerStatus Response
	{
		get
		{
			return m_pResponse;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pResponse = value;
		}
	}

	public IMAP_MessageInfo[] MessagesInfo => m_pMessagesInfo;

	public IMAP_Fetch_DataType FetchDataType => m_FetchDataType;

	internal event EventHandler<e_NewMessageData> NewMessageData;

	internal IMAP_e_Fetch(IMAP_MessageInfo[] messagesInfo, IMAP_Fetch_DataType fetchDataType, IMAP_r_ServerStatus response)
	{
		if (messagesInfo == null)
		{
			throw new ArgumentNullException("messagesInfo");
		}
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		m_pMessagesInfo = messagesInfo;
		m_FetchDataType = fetchDataType;
		m_pResponse = response;
	}

	internal void AddData(IMAP_MessageInfo msgInfo)
	{
		OnNewMessageData(msgInfo, null);
	}

	public void AddData(IMAP_MessageInfo msgInfo, Mail_Message msgData)
	{
		if (msgInfo == null)
		{
			throw new ArgumentNullException("msgInfo");
		}
		if (msgData == null)
		{
			throw new ArgumentNullException("msgData");
		}
		OnNewMessageData(msgInfo, msgData);
	}

	private void OnNewMessageData(IMAP_MessageInfo msgInfo, Mail_Message msgData)
	{
		if (this.NewMessageData != null)
		{
			this.NewMessageData(this, new e_NewMessageData(msgInfo, msgData));
		}
	}
}
