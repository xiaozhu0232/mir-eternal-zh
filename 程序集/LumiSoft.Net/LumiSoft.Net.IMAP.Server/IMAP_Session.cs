using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IO;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.IMAP.Server;

public class IMAP_Session : TCP_ServerSession
{
	private class _SelectedFolder
	{
		private string m_Folder;

		private bool m_IsReadOnly;

		private List<IMAP_MessageInfo> m_pMessagesInfo;

		public string Folder => m_Folder;

		public bool IsReadOnly => m_IsReadOnly;

		internal IMAP_MessageInfo[] MessagesInfo => m_pMessagesInfo.ToArray();

		public _SelectedFolder(string folder, bool isReadOnly, List<IMAP_MessageInfo> messagesInfo)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (messagesInfo == null)
			{
				throw new ArgumentNullException("messagesInfo");
			}
			m_Folder = folder;
			m_IsReadOnly = isReadOnly;
			m_pMessagesInfo = messagesInfo;
			Reindex();
		}

		internal IMAP_MessageInfo[] Filter(bool uid, IMAP_t_SeqSet seqSet)
		{
			if (seqSet == null)
			{
				throw new ArgumentNullException("seqSet");
			}
			List<IMAP_MessageInfo> list = new List<IMAP_MessageInfo>();
			for (int i = 0; i < m_pMessagesInfo.Count; i++)
			{
				IMAP_MessageInfo iMAP_MessageInfo = m_pMessagesInfo[i];
				if (uid)
				{
					if (seqSet.Contains(iMAP_MessageInfo.UID))
					{
						list.Add(iMAP_MessageInfo);
					}
				}
				else if (seqSet.Contains(i + 1))
				{
					list.Add(iMAP_MessageInfo);
				}
			}
			return list.ToArray();
		}

		internal void RemoveMessage(IMAP_MessageInfo message)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}
			m_pMessagesInfo.Remove(message);
		}

		internal int GetSeqNo(IMAP_MessageInfo msgInfo)
		{
			if (msgInfo == null)
			{
				throw new ArgumentNullException("msgInfo");
			}
			return m_pMessagesInfo.IndexOf(msgInfo) + 1;
		}

		internal int GetSeqNo(long uid)
		{
			foreach (IMAP_MessageInfo item in m_pMessagesInfo)
			{
				if (item.UID == uid)
				{
					return item.SeqNo;
				}
			}
			return -1;
		}

		internal void Reindex()
		{
			for (int i = 0; i < m_pMessagesInfo.Count; i++)
			{
				m_pMessagesInfo[i].SeqNo = i + 1;
			}
		}
	}

	private class _CmdReader
	{
		private IMAP_Session m_pSession;

		private string m_InitialCmdLine;

		private Encoding m_pCharset;

		private int m_MaxLiteralCount = 99;

		private int m_LiteralCount;

		private string m_CmdLine;

		private int m_MaxLiteralSize = 32000;

		public string CmdLine => m_CmdLine;

		public _CmdReader(IMAP_Session session, string initialCmdLine, Encoding charset, int maxLiteralCount)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			if (initialCmdLine == null)
			{
				throw new ArgumentNullException("initialCmdLine");
			}
			if (charset == null)
			{
				throw new ArgumentNullException("charset");
			}
			if (maxLiteralCount < 1)
			{
				throw new ArgumentException("Tere must be at least 1 string literal !", "maxLiteralCount");
			}
			m_pSession = session;
			m_InitialCmdLine = initialCmdLine;
			m_pCharset = charset;
			m_MaxLiteralCount = maxLiteralCount;
		}

		public void Start()
		{
			if (EndsWithLiteralString(m_InitialCmdLine))
			{
				StringBuilder stringBuilder = new StringBuilder();
				int literalSize = GetLiteralSize(m_InitialCmdLine);
				stringBuilder.Append(RemoveLiteralSpecifier(m_InitialCmdLine));
				SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
				while (m_MaxLiteralCount > m_LiteralCount)
				{
					if (literalSize > m_MaxLiteralSize)
					{
						throw new DataSizeExceededException();
					}
					m_pSession.WriteLine("+ Continue.");
					MemoryStream memoryStream = new MemoryStream();
					m_pSession.TcpStream.ReadFixedCount(memoryStream, literalSize);
					m_pSession.LogAddRead(literalSize, m_pCharset.GetString(memoryStream.ToArray()));
					stringBuilder.Append(TextUtils.QuoteString(m_pCharset.GetString(memoryStream.ToArray())));
					m_pSession.TcpStream.ReadLine(readLineAsyncOP, async: false);
					if (readLineAsyncOP.Error != null)
					{
						throw readLineAsyncOP.Error;
					}
					string lineUtf = readLineAsyncOP.LineUtf8;
					m_pSession.LogAddRead(readLineAsyncOP.BytesInBuffer, lineUtf);
					if (EndsWithLiteralString(lineUtf))
					{
						stringBuilder.Append(RemoveLiteralSpecifier(lineUtf));
					}
					else
					{
						stringBuilder.Append(lineUtf);
					}
					if (!EndsWithLiteralString(lineUtf))
					{
						break;
					}
					literalSize = GetLiteralSize(lineUtf);
				}
				m_CmdLine = stringBuilder.ToString();
				m_LiteralCount++;
			}
			else
			{
				m_CmdLine = m_InitialCmdLine;
			}
		}

		public static bool EndsWithLiteralString(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.EndsWith("}"))
			{
				int num = 0;
				char[] array = value.ToCharArray();
				int num2 = array.Length - 2;
				while (num2 >= 0 && array[num2] != '{')
				{
					if (char.IsDigit(array[num2]))
					{
						num++;
						num2--;
						continue;
					}
					return false;
				}
				if (num > 0)
				{
					return true;
				}
			}
			return false;
		}

		private int GetLiteralSize(string cmdLine)
		{
			if (cmdLine == null)
			{
				throw new ArgumentNullException("cmdLine");
			}
			return Convert.ToInt32(cmdLine.Substring(cmdLine.LastIndexOf('{') + 1, cmdLine.Length - cmdLine.LastIndexOf('{') - 2));
		}

		private string RemoveLiteralSpecifier(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return value.Substring(0, value.LastIndexOf('{'));
		}
	}

	private class ResponseSender
	{
		private class QueueItem
		{
			private bool m_IsSent;

			private IMAP_r m_pResponse;

			private EventHandler<EventArgs<Exception>> m_pCompletedAsyncCallback;

			public bool IsSent
			{
				get
				{
					return m_IsSent;
				}
				set
				{
					m_IsSent = value;
				}
			}

			public IMAP_r Response => m_pResponse;

			public EventHandler<EventArgs<Exception>> CompletedAsyncCallback => m_pCompletedAsyncCallback;

			public QueueItem(IMAP_r response, EventHandler<EventArgs<Exception>> completedAsyncCallback)
			{
				if (response == null)
				{
					throw new ArgumentNullException("response");
				}
				m_pResponse = response;
				m_pCompletedAsyncCallback = completedAsyncCallback;
			}
		}

		private object m_pLock = new object();

		private IMAP_Session m_pImap;

		private bool m_IsSending;

		private Queue<QueueItem> m_pResponses;

		public ResponseSender(IMAP_Session session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			m_pImap = session;
			m_pResponses = new Queue<QueueItem>();
		}

		public void Dispose()
		{
		}

		public void SendResponseAsync(IMAP_r response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			SendResponseAsync(response, null);
		}

		public bool SendResponseAsync(IMAP_r response, EventHandler<EventArgs<Exception>> completedAsyncCallback)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			QueueItem queueItem = new QueueItem(response, completedAsyncCallback);
			m_pResponses.Enqueue(queueItem);
			SendResponsesAsync(calledFromAsync: false);
			return !queueItem.IsSent;
		}

		private void SendResponsesAsync(bool calledFromAsync)
		{
			lock (m_pLock)
			{
				if (m_IsSending || m_pResponses.Count == 0)
				{
					return;
				}
				m_IsSending = true;
			}
			QueueItem responseItem = null;
			EventHandler<EventArgs<Exception>> completedAsyncCallback = delegate(object s, EventArgs<Exception> e)
			{
				try
				{
					if (responseItem.CompletedAsyncCallback != null)
					{
						responseItem.CompletedAsyncCallback(this, e);
					}
					m_IsSending = false;
					SendResponsesAsync(calledFromAsync: true);
				}
				catch (Exception x2)
				{
					m_pImap.OnError(x2);
					m_IsSending = false;
				}
			};
			try
			{
				while (m_pResponses.Count > 0)
				{
					responseItem = m_pResponses.Dequeue();
					if (responseItem.Response.SendAsync(m_pImap, completedAsyncCallback))
					{
						break;
					}
					responseItem.IsSent = true;
					if (calledFromAsync && responseItem.CompletedAsyncCallback != null)
					{
						responseItem.CompletedAsyncCallback(this, new EventArgs<Exception>(null));
					}
					lock (m_pLock)
					{
						if (m_pResponses.Count == 0)
						{
							m_IsSending = false;
							break;
						}
					}
				}
			}
			catch (Exception x)
			{
				m_pImap.OnError(x);
				m_IsSending = false;
			}
		}
	}

	private Dictionary<string, AUTH_SASL_ServerMechanism> m_pAuthentications;

	private bool m_SessionRejected;

	private int m_BadCommands;

	private List<string> m_pCapabilities;

	private char m_FolderSeparator = '/';

	private GenericIdentity m_pUser;

	private _SelectedFolder m_pSelectedFolder;

	private IMAP_Mailbox_Encoding m_MailboxEncoding = IMAP_Mailbox_Encoding.ImapUtf7;

	private ResponseSender m_pResponseSender;

	public new IMAP_Server Server
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (IMAP_Server)base.Server;
		}
	}

	public Dictionary<string, AUTH_SASL_ServerMechanism> Authentications
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pAuthentications;
		}
	}

	public int BadCommands
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_BadCommands;
		}
	}

	public override GenericIdentity AuthenticatedUserIdentity
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pUser;
		}
	}

	public List<string> Capabilities
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pCapabilities;
		}
	}

	public string SelectedFolderName
	{
		get
		{
			if (m_pSelectedFolder == null)
			{
				return null;
			}
			return m_pSelectedFolder.Folder;
		}
	}

	internal IMAP_Mailbox_Encoding MailboxEncoding => m_MailboxEncoding;

	public event EventHandler<IMAP_e_Started> Started;

	public event EventHandler<IMAP_e_Login> Login;

	public event EventHandler<IMAP_e_Namespace> Namespace;

	public event EventHandler<IMAP_e_List> List;

	public event EventHandler<IMAP_e_Folder> Create;

	public event EventHandler<IMAP_e_Folder> Delete;

	public event EventHandler<IMAP_e_Rename> Rename;

	public event EventHandler<IMAP_e_LSub> LSub;

	public event EventHandler<IMAP_e_Folder> Subscribe;

	public event EventHandler<IMAP_e_Folder> Unsubscribe;

	public event EventHandler<IMAP_e_Select> Select;

	public event EventHandler<IMAP_e_MessagesInfo> GetMessagesInfo;

	public event EventHandler<IMAP_e_Append> Append;

	public event EventHandler<IMAP_e_GetQuotaRoot> GetQuotaRoot;

	public event EventHandler<IMAP_e_GetQuota> GetQuota;

	public event EventHandler<IMAP_e_GetAcl> GetAcl;

	public event EventHandler<IMAP_e_SetAcl> SetAcl;

	public event EventHandler<IMAP_e_DeleteAcl> DeleteAcl;

	public event EventHandler<IMAP_e_ListRights> ListRights;

	public event EventHandler<IMAP_e_MyRights> MyRights;

	public event EventHandler<IMAP_e_Fetch> Fetch;

	public event EventHandler<IMAP_e_Search> Search;

	public event EventHandler<IMAP_e_Store> Store;

	public event EventHandler<IMAP_e_Copy> Copy;

	public event EventHandler<IMAP_e_Expunge> Expunge;

	public IMAP_Session()
	{
		m_pAuthentications = new Dictionary<string, AUTH_SASL_ServerMechanism>(StringComparer.CurrentCultureIgnoreCase);
		m_pCapabilities = new List<string>();
		m_pCapabilities.AddRange(new string[8] { "IMAP4rev1", "NAMESPACE", "QUOTA", "ACL", "IDLE", "ENABLE", "UTF8=ACCEPT", "SASL-IR" });
		m_pResponseSender = new ResponseSender(this);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			m_pAuthentications = null;
			m_pCapabilities = null;
			m_pUser = null;
			m_pSelectedFolder = null;
			if (m_pResponseSender != null)
			{
				m_pResponseSender.Dispose();
			}
			this.Started = null;
			this.Login = null;
			this.Namespace = null;
			this.List = null;
			this.Create = null;
			this.Delete = null;
			this.Rename = null;
			this.LSub = null;
			this.Subscribe = null;
			this.Unsubscribe = null;
			this.Select = null;
			this.GetMessagesInfo = null;
			this.Append = null;
			this.GetQuotaRoot = null;
			this.GetQuota = null;
			this.GetAcl = null;
			this.SetAcl = null;
			this.DeleteAcl = null;
			this.ListRights = null;
			this.MyRights = null;
			this.Fetch = null;
			this.Search = null;
			this.Store = null;
			this.Copy = null;
			this.Expunge = null;
		}
	}

	protected override void Start()
	{
		base.Start();
		try
		{
			IMAP_r_u_ServerStatus iMAP_r_u_ServerStatus = null;
			iMAP_r_u_ServerStatus = ((!string.IsNullOrEmpty(Server.GreetingText)) ? new IMAP_r_u_ServerStatus("OK", Server.GreetingText) : new IMAP_r_u_ServerStatus("OK", "<" + Net_Utils.GetLocalHostName(base.LocalHostName) + "> IMAP4rev1 server ready."));
			IMAP_e_Started iMAP_e_Started = OnStarted(iMAP_r_u_ServerStatus);
			if (iMAP_e_Started.Response != null)
			{
				m_pResponseSender.SendResponseAsync(iMAP_e_Started.Response);
			}
			if (iMAP_e_Started.Response == null || iMAP_e_Started.Response.ResponseCode.Equals("NO", StringComparison.InvariantCultureIgnoreCase))
			{
				m_SessionRejected = true;
			}
			BeginReadCmd();
		}
		catch (Exception x)
		{
			OnError(x);
		}
	}

	protected override void OnError(Exception x)
	{
		if (base.IsDisposed || x == null)
		{
			return;
		}
		try
		{
			LogAddText("Exception: " + x.Message);
			if (x is IOException || x is SocketException)
			{
				Dispose();
				return;
			}
			base.OnError(x);
			try
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("BAD", "Internal server error: " + x.Message));
			}
			catch
			{
				Dispose();
			}
		}
		catch
		{
		}
	}

	protected override void OnTimeout()
	{
		try
		{
			WriteLine("* BYE Idle timeout, closing connection.");
		}
		catch
		{
		}
	}

	private void BeginReadCmd()
	{
		if (base.IsDisposed)
		{
			return;
		}
		try
		{
			SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
			readLineOP.CompletedAsync += delegate
			{
				if (ProcessCmd(readLineOP))
				{
					BeginReadCmd();
				}
			};
			while (TcpStream.ReadLine(readLineOP, async: true) && ProcessCmd(readLineOP))
			{
			}
		}
		catch (Exception x)
		{
			OnError(x);
		}
	}

	private bool ProcessCmd(SmartStream.ReadLineAsyncOP op)
	{
		bool result = true;
		try
		{
			if (base.IsDisposed)
			{
				return false;
			}
			if (op.Error != null)
			{
				OnError(op.Error);
			}
			if (op.BytesInBuffer == 0)
			{
				LogAddText("The remote host '" + RemoteEndPoint.ToString() + "' shut down socket.");
				Dispose();
				return false;
			}
			string[] array = Encoding.UTF8.GetString(op.Buffer, 0, op.LineBytesInBuffer).Split(new char[1] { ' ' }, 3);
			if (array.Length < 2)
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("BAD", "Error: Command '" + op.LineUtf8 + "' not recognized."));
				return true;
			}
			string text = array[0];
			string text2 = array[1].ToUpperInvariant();
			string text3 = ((array.Length == 3) ? array[2] : "");
			if (Server.Logger != null)
			{
				if (text2 == "LOGIN")
				{
					Server.Logger.AddRead(ID, AuthenticatedUserIdentity, op.BytesInBuffer, op.LineUtf8.Substring(0, op.LineUtf8.LastIndexOf(' ')) + " <***REMOVED***>", LocalEndPoint, RemoteEndPoint);
				}
				else
				{
					Server.Logger.AddRead(ID, AuthenticatedUserIdentity, op.BytesInBuffer, op.LineUtf8, LocalEndPoint, RemoteEndPoint);
				}
			}
			if (_CmdReader.EndsWithLiteralString(text3) && !string.Equals(text2, "APPEND", StringComparison.InvariantCultureIgnoreCase))
			{
				_CmdReader cmdReader = new _CmdReader(this, text3, Encoding.UTF8, 99);
				cmdReader.Start();
				text3 = cmdReader.CmdLine;
			}
			switch (text2)
			{
			case "STARTTLS":
				STARTTLS(text, text3);
				result = false;
				return result;
			case "LOGIN":
				LOGIN(text, text3);
				return result;
			case "AUTHENTICATE":
				AUTHENTICATE(text, text3);
				return result;
			case "NAMESPACE":
				NAMESPACE(text, text3);
				return result;
			case "LIST":
				LIST(text, text3);
				return result;
			case "CREATE":
				CREATE(text, text3);
				return result;
			case "DELETE":
				DELETE(text, text3);
				return result;
			case "RENAME":
				RENAME(text, text3);
				return result;
			case "LSUB":
				LSUB(text, text3);
				return result;
			case "SUBSCRIBE":
				SUBSCRIBE(text, text3);
				return result;
			case "UNSUBSCRIBE":
				UNSUBSCRIBE(text, text3);
				return result;
			case "STATUS":
				STATUS(text, text3);
				return result;
			case "SELECT":
				SELECT(text, text3);
				return result;
			case "EXAMINE":
				EXAMINE(text, text3);
				return result;
			case "APPEND":
				APPEND(text, text3);
				return false;
			case "GETQUOTAROOT":
				GETQUOTAROOT(text, text3);
				return result;
			case "GETQUOTA":
				GETQUOTA(text, text3);
				return result;
			case "GETACL":
				GETACL(text, text3);
				return result;
			case "SETACL":
				SETACL(text, text3);
				return result;
			case "DELETEACL":
				DELETEACL(text, text3);
				return result;
			case "LISTRIGHTS":
				LISTRIGHTS(text, text3);
				return result;
			case "MYRIGHTS":
				MYRIGHTS(text, text3);
				return result;
			case "ENABLE":
				ENABLE(text, text3);
				return result;
			case "CHECK":
				CHECK(text, text3);
				return result;
			case "CLOSE":
				CLOSE(text, text3);
				return result;
			case "FETCH":
				FETCH(uid: false, text, text3);
				return result;
			case "SEARCH":
				SEARCH(uid: false, text, text3);
				return result;
			case "STORE":
				STORE(uid: false, text, text3);
				return result;
			case "COPY":
				COPY(uid: false, text, text3);
				return result;
			case "UID":
				UID(text, text3);
				return result;
			case "EXPUNGE":
				EXPUNGE(text, text3);
				return result;
			case "IDLE":
				result = IDLE(text, text3);
				return result;
			case "CAPABILITY":
				CAPABILITY(text, text3);
				return result;
			case "NOOP":
				NOOP(text, text3);
				return result;
			case "LOGOUT":
				LOGOUT(text, text3);
				result = false;
				return result;
			default:
				m_BadCommands++;
				if (Server.MaxBadCommands != 0 && m_BadCommands > Server.MaxBadCommands)
				{
					WriteLine("* BYE Too many bad commands, closing transmission channel.");
					Disconnect();
					return false;
				}
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(text, "BAD", "Error: Command '" + text2 + "' not recognized."));
				return result;
			}
		}
		catch (Exception x)
		{
			OnError(x);
			return result;
		}
	}

	private void STARTTLS(string cmdTag, string cmdText)
	{
		if (m_SessionRejected)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Bad sequence of commands: Session rejected."));
			return;
		}
		if (base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "This ommand is only valid in not-authenticated state."));
			return;
		}
		if (IsSecureConnection)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Bad sequence of commands: Connection is already secure."));
			return;
		}
		if (base.Certificate == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "TLS not available: Server has no SSL certificate."));
			return;
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "Begin TLS negotiation now."));
		try
		{
			DateTime startTime = DateTime.Now;
			Action<SwitchToSecureAsyncOP> switchSecureCompleted = delegate(SwitchToSecureAsyncOP e)
			{
				try
				{
					if (e.Error != null)
					{
						LogAddException(e.Error);
						Disconnect();
					}
					else
					{
						LogAddText("SSL negotiation completed successfully in " + (DateTime.Now - startTime).TotalSeconds.ToString("f2") + " seconds.");
						BeginReadCmd();
					}
				}
				catch (Exception exception2)
				{
					LogAddException(exception2);
					Disconnect();
				}
			};
			SwitchToSecureAsyncOP op = new SwitchToSecureAsyncOP();
			op.CompletedAsync += delegate
			{
				switchSecureCompleted(op);
			};
			if (!SwitchToSecureAsync(op))
			{
				switchSecureCompleted(op);
			}
		}
		catch (Exception exception)
		{
			LogAddException(exception);
			Disconnect();
		}
	}

	private void LOGIN(string cmdTag, string cmdText)
	{
		if (m_SessionRejected)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Bad sequence of commands: Session rejected."));
			return;
		}
		if (base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Re-authentication error."));
			return;
		}
		if (SupportsCap("LOGINDISABLED"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command 'LOGIN' is disabled, use AUTHENTICATE instead."));
			return;
		}
		if (string.IsNullOrEmpty(cmdText))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
		}
		else if (OnLogin(array[0], array[1]).IsAuthenticated)
		{
			m_pUser = new GenericIdentity(array[0], "IMAP-LOGIN");
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "LOGIN completed."));
		}
		else
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "LOGIN failed."));
		}
	}

	private void AUTHENTICATE(string cmdTag, string cmdText)
	{
		if (m_SessionRejected)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Bad sequence of commands: Session rejected."));
			return;
		}
		if (base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Re-authentication error."));
			return;
		}
		string[] array = cmdText.Split(' ');
		if (array.Length > 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		byte[] array2 = new byte[0];
		if (array.Length == 2 && !(array[1] == "="))
		{
			try
			{
				array2 = Convert.FromBase64String(array[1]);
			}
			catch
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Syntax error: Parameter 'initial-response' value must be BASE64 or contain a single character '='."));
				return;
			}
		}
		string key = array[0];
		if (!Authentications.ContainsKey(key))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Not supported authentication mechanism."));
			return;
		}
		byte[] array3 = array2;
		AUTH_SASL_ServerMechanism aUTH_SASL_ServerMechanism = Authentications[key];
		aUTH_SASL_ServerMechanism.Reset();
		while (true)
		{
			byte[] array4 = aUTH_SASL_ServerMechanism.Continue(array3);
			if (aUTH_SASL_ServerMechanism.IsCompleted)
			{
				if (aUTH_SASL_ServerMechanism.IsAuthenticated)
				{
					m_pUser = new GenericIdentity(aUTH_SASL_ServerMechanism.UserName, "SASL-" + aUTH_SASL_ServerMechanism.Name);
					m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "Authentication succeeded."));
				}
				else
				{
					m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication credentials invalid."));
				}
				return;
			}
			if (array4.Length == 0)
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus("+", ""));
			}
			else
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus("+", Convert.ToBase64String(array4)));
			}
			SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
			TcpStream.ReadLine(readLineAsyncOP, async: false);
			if (readLineAsyncOP.Error != null)
			{
				throw readLineAsyncOP.Error;
			}
			if (Server.Logger != null)
			{
				Server.Logger.AddRead(ID, AuthenticatedUserIdentity, readLineAsyncOP.BytesInBuffer, "base64 auth-data", LocalEndPoint, RemoteEndPoint);
			}
			if (readLineAsyncOP.LineUtf8 == "*")
			{
				break;
			}
			try
			{
				array3 = Convert.FromBase64String(readLineAsyncOP.LineUtf8);
			}
			catch
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Invalid client response '" + array3?.ToString() + "'."));
				return;
			}
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication canceled."));
	}

	private void NAMESPACE(string cmdTag, string cmdText)
	{
		if (!SupportsCap("NAMESPACE"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		IMAP_e_Namespace iMAP_e_Namespace = OnNamespace(new IMAP_r_ServerStatus(cmdTag, "OK", "NAMESPACE command completed."));
		if (iMAP_e_Namespace.NamespaceResponse != null)
		{
			m_pResponseSender.SendResponseAsync(iMAP_e_Namespace.NamespaceResponse);
		}
		m_pResponseSender.SendResponseAsync(iMAP_e_Namespace.Response);
	}

	private void LIST(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		string refName = IMAP_Utils.DecodeMailbox(array[0]);
		string text = IMAP_Utils.DecodeMailbox(array[1]);
		long ticks = DateTime.Now.Ticks;
		if (text == string.Empty)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_List(m_FolderSeparator));
		}
		else
		{
			foreach (IMAP_r_u_List folder in OnList(refName, text).Folders)
			{
				m_pResponseSender.SendResponseAsync(folder);
			}
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "LIST Completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
	}

	private void CREATE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string folder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		IMAP_e_Folder iMAP_e_Folder = OnCreate(cmdTag, folder, new IMAP_r_ServerStatus(cmdTag, "OK", "CREATE command completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_Folder.Response);
	}

	private void DELETE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string folder = IMAP_Utils.DecodeMailbox(cmdText);
		IMAP_e_Folder iMAP_e_Folder = OnDelete(cmdTag, folder, new IMAP_r_ServerStatus(cmdTag, "OK", "DELETE command completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_Folder.Response);
	}

	private void RENAME(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_e_Rename iMAP_e_Rename = OnRename(cmdTag, IMAP_Utils.DecodeMailbox(array[0]), IMAP_Utils.DecodeMailbox(array[1]));
		if (iMAP_e_Rename.Response == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Internal server error: IMAP Server application didn't return any resposne."));
		}
		else
		{
			m_pResponseSender.SendResponseAsync(iMAP_e_Rename.Response);
		}
	}

	private void LSUB(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		string refName = IMAP_Utils.DecodeMailbox(array[0]);
		string folder = IMAP_Utils.DecodeMailbox(array[1]);
		long ticks = DateTime.Now.Ticks;
		foreach (IMAP_r_u_LSub folder2 in OnLSub(refName, folder).Folders)
		{
			m_pResponseSender.SendResponseAsync(folder2);
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "LSUB Completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
	}

	private void SUBSCRIBE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string text = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		if (string.IsNullOrEmpty(text))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Mailbox name must be specified."));
			return;
		}
		IMAP_e_Folder iMAP_e_Folder = OnSubscribe(cmdTag, text, new IMAP_r_ServerStatus(cmdTag, "OK", "SUBSCRIBE command completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_Folder.Response);
	}

	private void UNSUBSCRIBE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string folder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		IMAP_e_Folder iMAP_e_Folder = OnUnsubscribe(cmdTag, folder, new IMAP_r_ServerStatus(cmdTag, "OK", "UNSUBSCRIBE command completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_Folder.Response);
	}

	private void STATUS(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: false, 2);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		long ticks = DateTime.Now.Ticks;
		try
		{
			string folder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(array[0]));
			if (!array[1].StartsWith("(") || !array[1].EndsWith(")"))
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
				return;
			}
			IMAP_e_Select iMAP_e_Select = OnSelect(cmdTag, folder);
			if (iMAP_e_Select.ErrorResponse != null)
			{
				m_pResponseSender.SendResponseAsync(iMAP_e_Select.ErrorResponse);
				return;
			}
			IMAP_e_MessagesInfo iMAP_e_MessagesInfo = OnGetMessagesInfo(folder);
			int messagesCount = -1;
			int recentCount = -1;
			long uidNext = -1L;
			long folderUid = -1L;
			int unseenCount = -1;
			string[] array2 = array[1].Substring(1, array[1].Length - 2).Split(' ');
			foreach (string a in array2)
			{
				if (string.Equals(a, "MESSAGES", StringComparison.InvariantCultureIgnoreCase))
				{
					messagesCount = iMAP_e_MessagesInfo.Exists;
					continue;
				}
				if (string.Equals(a, "RECENT", StringComparison.InvariantCultureIgnoreCase))
				{
					recentCount = iMAP_e_MessagesInfo.Recent;
					continue;
				}
				if (string.Equals(a, "UIDNEXT", StringComparison.InvariantCultureIgnoreCase))
				{
					uidNext = iMAP_e_MessagesInfo.UidNext;
					continue;
				}
				if (string.Equals(a, "UIDVALIDITY", StringComparison.InvariantCultureIgnoreCase))
				{
					folderUid = iMAP_e_Select.FolderUID;
					continue;
				}
				if (string.Equals(a, "UNSEEN", StringComparison.InvariantCultureIgnoreCase))
				{
					unseenCount = iMAP_e_MessagesInfo.Unseen;
					continue;
				}
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
				return;
			}
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_Status(folder, messagesCount, recentCount, uidNext, folderUid, unseenCount));
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "STATUS completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
		}
		catch (Exception ex)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: " + ex.Message));
		}
	}

	private void SELECT(string cmdTag, string cmdText)
	{
		long ticks = DateTime.Now.Ticks;
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder != null)
		{
			m_pSelectedFolder = null;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ');
		if (array.Length >= 2)
		{
			if (string.Equals(array[1], "(UTF8)", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", new IMAP_t_orc_Unknown("NOT-UTF-8"), "Mailbox does not support UTF-8 access."));
			}
			else
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			}
			return;
		}
		try
		{
			string folder = TextUtils.UnQuoteString(IMAP_Utils.DecodeMailbox(cmdText));
			IMAP_e_Select iMAP_e_Select = OnSelect(cmdTag, folder);
			if (iMAP_e_Select.ErrorResponse == null)
			{
				IMAP_e_MessagesInfo iMAP_e_MessagesInfo = OnGetMessagesInfo(folder);
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_Exists(iMAP_e_MessagesInfo.Exists));
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_Recent(iMAP_e_MessagesInfo.Recent));
				if (iMAP_e_MessagesInfo.FirstUnseen > -1)
				{
					m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_Unseen(iMAP_e_MessagesInfo.FirstUnseen), "Message " + iMAP_e_MessagesInfo.FirstUnseen + " is the first unseen."));
				}
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_UidNext((int)iMAP_e_MessagesInfo.UidNext), "Predicted next message UID."));
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_UidValidity(iMAP_e_Select.FolderUID), "Folder UID value."));
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_Flags(iMAP_e_Select.Flags.ToArray()));
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_PermanentFlags(iMAP_e_Select.PermanentFlags.ToArray()), "Avaliable permanent flags."));
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", new IMAP_t_orc_Unknown(iMAP_e_Select.IsReadOnly ? "READ-ONLY" : "READ-WRITE"), "SELECT completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
				m_pSelectedFolder = new _SelectedFolder(folder, iMAP_e_Select.IsReadOnly, iMAP_e_MessagesInfo.MessagesInfo);
				m_pSelectedFolder.Reindex();
			}
			else
			{
				m_pResponseSender.SendResponseAsync(iMAP_e_Select.ErrorResponse);
			}
		}
		catch (Exception ex)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: " + ex.Message));
		}
	}

	private void EXAMINE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		long ticks = DateTime.Now.Ticks;
		if (m_pSelectedFolder != null)
		{
			m_pSelectedFolder = null;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ');
		if (array.Length >= 2)
		{
			if (string.Equals(array[1], "(UTF8)", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", new IMAP_t_orc_Unknown("NOT-UTF-8"), "Mailbox does not support UTF-8 access."));
			}
			else
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			}
			return;
		}
		string folder = TextUtils.UnQuoteString(IMAP_Utils.DecodeMailbox(cmdText));
		IMAP_e_Select iMAP_e_Select = OnSelect(cmdTag, folder);
		if (iMAP_e_Select.ErrorResponse == null)
		{
			IMAP_e_MessagesInfo iMAP_e_MessagesInfo = OnGetMessagesInfo(folder);
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_Exists(iMAP_e_MessagesInfo.Exists));
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_Recent(iMAP_e_MessagesInfo.Recent));
			if (iMAP_e_MessagesInfo.FirstUnseen > -1)
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_Unseen(iMAP_e_MessagesInfo.FirstUnseen), "Message " + iMAP_e_MessagesInfo.FirstUnseen + " is the first unseen."));
			}
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_UidNext((int)iMAP_e_MessagesInfo.UidNext), "Predicted next message UID."));
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_UidValidity(iMAP_e_Select.FolderUID), "Folder UID value."));
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_Flags(iMAP_e_Select.Flags.ToArray()));
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_ServerStatus("OK", new IMAP_t_orc_PermanentFlags(iMAP_e_Select.PermanentFlags.ToArray()), "Avaliable permanent flags."));
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", new IMAP_t_orc_ReadOnly(), "EXAMINE completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
			m_pSelectedFolder = new _SelectedFolder(folder, iMAP_e_Select.IsReadOnly, iMAP_e_MessagesInfo.MessagesInfo);
			m_pSelectedFolder.Reindex();
		}
		else
		{
			m_pResponseSender.SendResponseAsync(iMAP_e_Select.ErrorResponse);
		}
	}

	private void APPEND(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		long startTime = DateTime.Now.Ticks;
		StringReader stringReader = new StringReader(cmdText);
		stringReader.ReadToFirstChar();
		string text = null;
		if (stringReader.StartsWith("\""))
		{
			text = IMAP_Utils.DecodeMailbox(stringReader.ReadWord());
		}
		else if (stringReader.StartsWith("{"))
		{
			_CmdReader cmdReader = new _CmdReader(this, cmdText, Encoding.UTF8, 1);
			cmdReader.Start();
			stringReader = new StringReader(cmdReader.CmdLine);
			text = ((!stringReader.StartsWith("\"")) ? IMAP_Utils.DecodeMailbox(stringReader.QuotedReadToDelimiter(' ')) : IMAP_Utils.DecodeMailbox(stringReader.ReadWord()));
		}
		else
		{
			text = IMAP_Utils.DecodeMailbox(stringReader.QuotedReadToDelimiter(' '));
		}
		stringReader.ReadToFirstChar();
		List<string> list = new List<string>();
		if (stringReader.StartsWith("("))
		{
			string[] array = stringReader.ReadParenthesized().Split(' ');
			foreach (string text2 in array)
			{
				if (text2.Length > 0 && !list.Contains(text2.Substring(1)))
				{
					list.Add(text2.Substring(1));
				}
			}
		}
		stringReader.ReadToFirstChar();
		DateTime date = DateTime.MinValue;
		if (!stringReader.StartsWith("{"))
		{
			try
			{
				date = IMAP_Utils.ParseDate(stringReader.ReadWord());
			}
			catch
			{
			}
		}
		int size = Convert.ToInt32(stringReader.ReadParenthesized());
		if (stringReader.Available > 0)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_e_Append e = OnAppend(text, list.ToArray(), date, size, new IMAP_r_ServerStatus(cmdTag, "OK", "APPEND command completed in %exectime seconds."));
		if (e.Response.IsError)
		{
			m_pResponseSender.SendResponseAsync(e.Response);
			return;
		}
		if (e.Stream == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Internal server error: No storage stream available."));
			return;
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus("+", "Ready for literal data."));
		AsyncCallback callback = delegate(IAsyncResult ar)
		{
			try
			{
				TcpStream.EndReadFixedCount(ar);
				LogAddRead(size, "Readed " + size + " bytes.");
				SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
				TcpStream.ReadLine(readLineAsyncOP, async: false);
				if (readLineAsyncOP.Error != null)
				{
					OnError(readLineAsyncOP.Error);
				}
				else
				{
					LogAddRead(readLineAsyncOP.BytesInBuffer, readLineAsyncOP.LineUtf8);
					e.OnCompleted();
					m_pResponseSender.SendResponseAsync(IMAP_r_ServerStatus.Parse(e.Response.ToString().TrimEnd().Replace("%exectime", ((decimal)(DateTime.Now.Ticks - startTime) / 10000000m).ToString("f2"))));
					BeginReadCmd();
				}
			}
			catch (Exception x)
			{
				OnError(x);
			}
		};
		TcpStream.BeginReadFixedCount(e.Stream, size, callback, null);
	}

	private void GETQUOTAROOT(string cmdTag, string cmdText)
	{
		if (!SupportsCap("QUOTA"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (string.IsNullOrEmpty(cmdText))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		string folder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		IMAP_e_GetQuotaRoot iMAP_e_GetQuotaRoot = OnGetGuotaRoot(folder, new IMAP_r_ServerStatus(cmdTag, "OK", "GETQUOTAROOT command completed."));
		if (iMAP_e_GetQuotaRoot.QuotaRootResponses.Count > 0)
		{
			foreach (IMAP_r_u_QuotaRoot quotaRootResponse in iMAP_e_GetQuotaRoot.QuotaRootResponses)
			{
				m_pResponseSender.SendResponseAsync(quotaRootResponse);
			}
		}
		if (iMAP_e_GetQuotaRoot.QuotaResponses.Count > 0)
		{
			foreach (IMAP_r_u_Quota quotaResponse in iMAP_e_GetQuotaRoot.QuotaResponses)
			{
				m_pResponseSender.SendResponseAsync(quotaResponse);
			}
		}
		m_pResponseSender.SendResponseAsync(iMAP_e_GetQuotaRoot.Response);
	}

	private void GETQUOTA(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string quotaRoot = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		IMAP_e_GetQuota iMAP_e_GetQuota = OnGetQuota(quotaRoot, new IMAP_r_ServerStatus(cmdTag, "OK", "QUOTA command completed."));
		if (iMAP_e_GetQuota.QuotaResponses.Count > 0)
		{
			foreach (IMAP_r_u_Quota quotaResponse in iMAP_e_GetQuota.QuotaResponses)
			{
				m_pResponseSender.SendResponseAsync(quotaResponse);
			}
		}
		m_pResponseSender.SendResponseAsync(iMAP_e_GetQuota.Response);
	}

	private void GETACL(string cmdTag, string cmdText)
	{
		if (!SupportsCap("ACL"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string folder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		IMAP_e_GetAcl iMAP_e_GetAcl = OnGetAcl(folder, new IMAP_r_ServerStatus(cmdTag, "OK", "GETACL command completed."));
		if (iMAP_e_GetAcl.AclResponses.Count > 0)
		{
			foreach (IMAP_r_u_Acl aclResponse in iMAP_e_GetAcl.AclResponses)
			{
				m_pResponseSender.SendResponseAsync(aclResponse);
			}
		}
		m_pResponseSender.SendResponseAsync(iMAP_e_GetAcl.Response);
	}

	private void SETACL(string cmdTag, string cmdText)
	{
		if (!SupportsCap("ACL"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 3)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		string text = array[2];
		IMAP_Flags_SetType flagsSetType = IMAP_Flags_SetType.Replace;
		if (text.StartsWith("+"))
		{
			flagsSetType = IMAP_Flags_SetType.Add;
			text = text.Substring(1);
		}
		else if (text.StartsWith("-"))
		{
			flagsSetType = IMAP_Flags_SetType.Remove;
			text = text.Substring(1);
		}
		IMAP_e_SetAcl iMAP_e_SetAcl = OnSetAcl(IMAP_Utils.DecodeMailbox(array[0]), IMAP_Utils.DecodeMailbox(array[1]), flagsSetType, text, new IMAP_r_ServerStatus(cmdTag, "OK", "SETACL command completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_SetAcl.Response);
	}

	private void DELETEACL(string cmdTag, string cmdText)
	{
		if (!SupportsCap("ACL"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_e_DeleteAcl iMAP_e_DeleteAcl = OnDeleteAcl(IMAP_Utils.DecodeMailbox(array[0]), IMAP_Utils.DecodeMailbox(array[1]), new IMAP_r_ServerStatus(cmdTag, "OK", "DELETEACL command completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_DeleteAcl.Response);
	}

	private void LISTRIGHTS(string cmdTag, string cmdText)
	{
		if (!SupportsCap("ACL"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string[] array = TextUtils.SplitQuotedString(cmdText, ' ', unquote: true);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_e_ListRights iMAP_e_ListRights = OnListRights(IMAP_Utils.DecodeMailbox(array[0]), IMAP_Utils.DecodeMailbox(array[1]), new IMAP_r_ServerStatus(cmdTag, "OK", "LISTRIGHTS command completed."));
		if (iMAP_e_ListRights.ListRightsResponse != null)
		{
			m_pResponseSender.SendResponseAsync(iMAP_e_ListRights.ListRightsResponse);
		}
		m_pResponseSender.SendResponseAsync(iMAP_e_ListRights.Response);
	}

	private void MYRIGHTS(string cmdTag, string cmdText)
	{
		if (!SupportsCap("ACL"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		string folder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(cmdText));
		IMAP_e_MyRights iMAP_e_MyRights = OnMyRights(folder, new IMAP_r_ServerStatus(cmdTag, "OK", "MYRIGHTS command completed."));
		if (iMAP_e_MyRights.MyRightsResponse != null)
		{
			m_pResponseSender.SendResponseAsync(iMAP_e_MyRights.MyRightsResponse);
		}
		m_pResponseSender.SendResponseAsync(iMAP_e_MyRights.Response);
	}

	private void ENABLE(string cmdTag, string cmdText)
	{
		if (!SupportsCap("ENABLE"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Command 'ENABLE' not supported."));
			return;
		}
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (string.IsNullOrEmpty(cmdText))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "No arguments, or syntax error in an argument."));
			return;
		}
		string[] array = cmdText.Split(' ');
		foreach (string b in array)
		{
			if (string.Equals("UTF8=ACCEPT", b, StringComparison.InvariantCultureIgnoreCase))
			{
				m_MailboxEncoding = IMAP_Mailbox_Encoding.ImapUtf8;
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_Enable(new string[1] { "UTF8=ACCEPT" }));
			}
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "ENABLE command completed."));
	}

	private void CHECK(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		long ticks = DateTime.Now.Ticks;
		UpdateSelectedFolderAndSendChanges();
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "CHECK Completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
	}

	private void CLOSE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		if (m_pSelectedFolder != null && !m_pSelectedFolder.IsReadOnly)
		{
			IMAP_MessageInfo[] messagesInfo = m_pSelectedFolder.MessagesInfo;
			foreach (IMAP_MessageInfo iMAP_MessageInfo in messagesInfo)
			{
				if (iMAP_MessageInfo.ContainsFlag("Deleted"))
				{
					OnExpunge(iMAP_MessageInfo, new IMAP_r_ServerStatus("dummy", "OK", "This is CLOSE command expunge, so this response is not used."));
				}
			}
		}
		m_pSelectedFolder = null;
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "CLOSE completed."));
	}

	private void FETCH(bool uid, string cmdTag, string cmdText)
	{
		long ticks = DateTime.Now.Ticks;
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		string[] array = cmdText.Split(new char[1] { ' ' }, 2);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_t_SeqSet iMAP_t_SeqSet = null;
		try
		{
			iMAP_t_SeqSet = IMAP_t_SeqSet.Parse(array[0]);
		}
		catch
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments: Invalid 'sequence-set' value."));
			return;
		}
		List<IMAP_t_Fetch_i> dataItems = new List<IMAP_t_Fetch_i>();
		bool flag = false;
		string text = array[1].Trim();
		if (text.StartsWith("(") && text.EndsWith(")"))
		{
			text = text.Substring(1, text.Length - 2).Trim();
		}
		text = text.Replace("ALL", "FLAGS INTERNALDATE RFC822.SIZE ENVELOPE");
		text = text.Replace("FAST", "FLAGS INTERNALDATE RFC822.SIZE");
		text = text.Replace("FULL", "FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY");
		StringReader stringReader = new StringReader(text);
		IMAP_Fetch_DataType iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageHeader;
		while (stringReader.Available > 0)
		{
			stringReader.ReadToFirstChar();
			if (stringReader.StartsWith("BODYSTRUCTURE", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_BodyStructure());
				flag = true;
				if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage)
				{
					iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageStructure;
				}
			}
			else if (stringReader.StartsWith("BODY[", case_sensitive: false) || stringReader.StartsWith("BODY.PEEK[", case_sensitive: false))
			{
				bool flag2 = stringReader.StartsWith("BODY.PEEK[", case_sensitive: false);
				stringReader.ReadWord();
				string text2 = stringReader.ReadParenthesized();
				if (string.IsNullOrEmpty(text2))
				{
					iMAP_Fetch_DataType = IMAP_Fetch_DataType.FullMessage;
				}
				else
				{
					StringReader stringReader2 = new StringReader(text2);
					string text3 = stringReader2.ReadWord();
					while (text3.Length > 0)
					{
						string[] array2 = text3.Split(new char[1] { '.' }, 2);
						if (!Net_Utils.IsInteger(array2[0]))
						{
							if (text3.Equals("HEADER", StringComparison.InvariantCultureIgnoreCase))
							{
								if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage && iMAP_Fetch_DataType != IMAP_Fetch_DataType.MessageStructure)
								{
									iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageHeader;
								}
								break;
							}
							if (text3.Equals("HEADER.FIELDS", StringComparison.InvariantCultureIgnoreCase))
							{
								stringReader2.ReadToFirstChar();
								if (stringReader2.StartsWith("("))
								{
									stringReader2.ReadParenthesized();
									if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage && iMAP_Fetch_DataType != IMAP_Fetch_DataType.MessageStructure)
									{
										iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageHeader;
									}
									break;
								}
								WriteLine(cmdTag + " BAD Error in arguments.");
							}
							else if (text3.Equals("HEADER.FIELDS.NOT", StringComparison.InvariantCultureIgnoreCase))
							{
								stringReader2.ReadToFirstChar();
								if (stringReader2.StartsWith("("))
								{
									stringReader2.ReadParenthesized();
									if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage && iMAP_Fetch_DataType != IMAP_Fetch_DataType.MessageStructure)
									{
										iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageHeader;
									}
									break;
								}
								WriteLine(cmdTag + " BAD Error in arguments.");
							}
							else
							{
								if (text3.Equals("MIME", StringComparison.InvariantCultureIgnoreCase))
								{
									iMAP_Fetch_DataType = IMAP_Fetch_DataType.FullMessage;
									break;
								}
								if (text3.Equals("TEXT", StringComparison.InvariantCultureIgnoreCase))
								{
									iMAP_Fetch_DataType = IMAP_Fetch_DataType.FullMessage;
									break;
								}
								WriteLine(cmdTag + " BAD Error in arguments.");
							}
							return;
						}
						if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage)
						{
							iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageStructure;
						}
						text3 = ((array2.Length != 2) ? "" : array2[1]);
					}
				}
				int result = -1;
				int result2 = -1;
				if (stringReader.StartsWith("<"))
				{
					string[] array3 = stringReader.ReadParenthesized().Split('.');
					if (array3.Length > 2)
					{
						WriteLine(cmdTag + " BAD Error in arguments.");
						return;
					}
					if (!int.TryParse(array3[0], out result))
					{
						WriteLine(cmdTag + " BAD Error in arguments.");
						return;
					}
					if (array3.Length == 2 && !int.TryParse(array3[1], out result2))
					{
						WriteLine(cmdTag + " BAD Error in arguments.");
						return;
					}
				}
				if (flag2)
				{
					dataItems.Add(new IMAP_t_Fetch_i_BodyPeek(text2, result, result2));
				}
				else
				{
					dataItems.Add(new IMAP_t_Fetch_i_Body(text2, result, result2));
				}
				flag = true;
			}
			else if (stringReader.StartsWith("BODY", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_BodyS());
				flag = true;
				if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage)
				{
					iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageStructure;
				}
			}
			else if (stringReader.StartsWith("ENVELOPE", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Envelope());
				flag = true;
				if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage && iMAP_Fetch_DataType != IMAP_Fetch_DataType.MessageStructure)
				{
					iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageHeader;
				}
			}
			else if (stringReader.StartsWith("FLAGS", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Flags());
			}
			else if (stringReader.StartsWith("INTERNALDATE", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_InternalDate());
			}
			else if (stringReader.StartsWith("RFC822.HEADER", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Rfc822Header());
				flag = true;
				if (iMAP_Fetch_DataType != IMAP_Fetch_DataType.FullMessage && iMAP_Fetch_DataType != IMAP_Fetch_DataType.MessageStructure)
				{
					iMAP_Fetch_DataType = IMAP_Fetch_DataType.MessageHeader;
				}
			}
			else if (stringReader.StartsWith("RFC822.SIZE", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Rfc822Size());
			}
			else if (stringReader.StartsWith("RFC822.TEXT", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Rfc822Text());
				flag = true;
				iMAP_Fetch_DataType = IMAP_Fetch_DataType.FullMessage;
			}
			else if (stringReader.StartsWith("RFC822", case_sensitive: false))
			{
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Rfc822());
				flag = true;
				iMAP_Fetch_DataType = IMAP_Fetch_DataType.FullMessage;
			}
			else
			{
				if (!stringReader.StartsWith("UID", case_sensitive: false))
				{
					WriteLine(cmdTag + " BAD Error in arguments: Unknown FETCH data-item.");
					return;
				}
				stringReader.ReadWord();
				dataItems.Add(new IMAP_t_Fetch_i_Uid());
			}
		}
		if (uid)
		{
			bool flag3 = true;
			foreach (IMAP_t_Fetch_i item in dataItems)
			{
				if (item is IMAP_t_Fetch_i_Uid)
				{
					flag3 = false;
					break;
				}
			}
			if (flag3)
			{
				dataItems.Add(new IMAP_t_Fetch_i_Uid());
			}
		}
		UpdateSelectedFolderAndSendChanges();
		IMAP_e_Fetch iMAP_e_Fetch = new IMAP_e_Fetch(m_pSelectedFolder.Filter(uid, iMAP_t_SeqSet), iMAP_Fetch_DataType, new IMAP_r_ServerStatus(cmdTag, "OK", "FETCH command completed in %exectime seconds."));
		iMAP_e_Fetch.NewMessageData += delegate(object s, IMAP_e_Fetch.e_NewMessageData e)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("* " + e.MessageInfo.SeqNo + " FETCH (");
			Mail_Message messageData = e.MessageData;
			for (int j = 0; j < dataItems.Count; j++)
			{
				IMAP_t_Fetch_i iMAP_t_Fetch_i = dataItems[j];
				if (j > 0)
				{
					stringBuilder.Append(" ");
				}
				if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_BodyS)
				{
					stringBuilder.Append(ConstructBodyStructure(messageData, bodystructure: false));
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Body || iMAP_t_Fetch_i is IMAP_t_Fetch_i_BodyPeek)
				{
					string text4 = "";
					int num = -1;
					int num2 = -1;
					if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Body)
					{
						text4 = ((IMAP_t_Fetch_i_Body)iMAP_t_Fetch_i).Section;
						num = ((IMAP_t_Fetch_i_Body)iMAP_t_Fetch_i).Offset;
						num2 = ((IMAP_t_Fetch_i_Body)iMAP_t_Fetch_i).MaxCount;
					}
					else
					{
						text4 = ((IMAP_t_Fetch_i_BodyPeek)iMAP_t_Fetch_i).Section;
						num = ((IMAP_t_Fetch_i_BodyPeek)iMAP_t_Fetch_i).Offset;
						num2 = ((IMAP_t_Fetch_i_BodyPeek)iMAP_t_Fetch_i).MaxCount;
					}
					using (MemoryStreamEx memoryStreamEx = new MemoryStreamEx(32000))
					{
						if (string.IsNullOrEmpty(text4))
						{
							messageData.ToStream(memoryStreamEx, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
							memoryStreamEx.Position = 0L;
						}
						else
						{
							MIME_Entity mimeEntity = GetMimeEntity(messageData, ParsePartNumberFromSection(text4));
							if (mimeEntity != null)
							{
								string a = ParsePartSpecifierFromSection(text4);
								if (string.Equals(a, "HEADER", StringComparison.InvariantCultureIgnoreCase))
								{
									mimeEntity.Header.ToStream(memoryStreamEx, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
									if (memoryStreamEx.Length > 0)
									{
										memoryStreamEx.WriteByte(13);
										memoryStreamEx.WriteByte(10);
									}
									memoryStreamEx.Position = 0L;
								}
								else if (string.Equals(a, "HEADER.FIELDS", StringComparison.InvariantCultureIgnoreCase))
								{
									string text5 = text4.Split(new char[1] { ' ' }, 2)[1];
									string[] array5 = text5.Substring(1, text5.Length - 2).Split(' ');
									foreach (string name in array5)
									{
										MIME_h[] array6 = mimeEntity.Header[name];
										if (array6 != null)
										{
											MIME_h[] array7 = array6;
											foreach (MIME_h mIME_h in array7)
											{
												byte[] bytes = Encoding.UTF8.GetBytes(mIME_h.ToString(new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8));
												memoryStreamEx.Write(bytes, 0, bytes.Length);
											}
										}
									}
									if (memoryStreamEx.Length > 0)
									{
										memoryStreamEx.WriteByte(13);
										memoryStreamEx.WriteByte(10);
									}
									memoryStreamEx.Position = 0L;
								}
								else if (string.Equals(a, "HEADER.FIELDS.NOT", StringComparison.InvariantCultureIgnoreCase))
								{
									string text6 = text4.Split(new char[1] { ' ' }, 2)[1];
									string[] array8 = text6.Substring(1, text6.Length - 2).Split(' ');
									foreach (MIME_h item2 in mimeEntity.Header)
									{
										bool flag4 = false;
										string[] array5 = array8;
										foreach (string b in array5)
										{
											if (string.Equals(item2.Name, b, StringComparison.InvariantCultureIgnoreCase))
											{
												flag4 = true;
												break;
											}
										}
										if (!flag4)
										{
											byte[] bytes2 = Encoding.UTF8.GetBytes(item2.ToString(new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8));
											memoryStreamEx.Write(bytes2, 0, bytes2.Length);
										}
									}
									if (memoryStreamEx.Length > 0)
									{
										memoryStreamEx.WriteByte(13);
										memoryStreamEx.WriteByte(10);
									}
									memoryStreamEx.Position = 0L;
								}
								else if (string.Equals(a, "MIME", StringComparison.InvariantCultureIgnoreCase))
								{
									mimeEntity.Header.ToStream(memoryStreamEx, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
									if (memoryStreamEx.Length > 0)
									{
										memoryStreamEx.WriteByte(13);
										memoryStreamEx.WriteByte(10);
									}
									memoryStreamEx.Position = 0L;
								}
								else if (string.Equals(a, "TEXT", StringComparison.InvariantCultureIgnoreCase))
								{
									mimeEntity.Body.ToStream(memoryStreamEx, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8, headerReencode: false);
									memoryStreamEx.Position = 0L;
								}
								else
								{
									mimeEntity.Body.ToStream(memoryStreamEx, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8, headerReencode: false);
									memoryStreamEx.Position = 0L;
								}
							}
						}
						if (num < 0)
						{
							stringBuilder.Append("BODY[" + text4 + "] {" + memoryStreamEx.Length + "}\r\n");
							WriteLine(stringBuilder.ToString());
							stringBuilder = new StringBuilder();
							TcpStream.WriteStream(memoryStreamEx);
							LogAddWrite(memoryStreamEx.Length, "Wrote " + memoryStreamEx.Length + " bytes.");
						}
						else if (num >= memoryStreamEx.Length)
						{
							stringBuilder.Append("BODY[" + text4 + "]<" + num + "> \"\"");
						}
						else
						{
							memoryStreamEx.Position = num;
							int num3 = (int)((num2 > -1) ? Math.Min(num2, memoryStreamEx.Length - memoryStreamEx.Position) : (memoryStreamEx.Length - memoryStreamEx.Position));
							stringBuilder.Append("BODY[" + text4 + "]<" + num + "> {" + num3 + "}");
							WriteLine(stringBuilder.ToString());
							stringBuilder = new StringBuilder();
							TcpStream.WriteStream(memoryStreamEx, num3);
							LogAddWrite(memoryStreamEx.Length, "Wrote " + num3 + " bytes.");
						}
					}
					if (!m_pSelectedFolder.IsReadOnly && iMAP_t_Fetch_i is IMAP_t_Fetch_i_Body)
					{
						try
						{
							OnStore(e.MessageInfo, IMAP_Flags_SetType.Add, new string[1] { "Seen" }, new IMAP_r_ServerStatus("dummy", "OK", "This is FETCH set Seen flag, this response not used."));
						}
						catch
						{
						}
					}
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_BodyStructure)
				{
					stringBuilder.Append(ConstructBodyStructure(messageData, bodystructure: true));
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Envelope)
				{
					stringBuilder.Append(IMAP_t_Fetch_r_i_Envelope.ConstructEnvelope(messageData));
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Flags)
				{
					stringBuilder.Append("FLAGS " + e.MessageInfo.FlagsToImapString());
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_InternalDate)
				{
					stringBuilder.Append("INTERNALDATE \"" + IMAP_Utils.DateTimeToString(e.MessageInfo.InternalDate) + "\"");
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Rfc822)
				{
					using MemoryStreamEx memoryStreamEx2 = new MemoryStreamEx(32000);
					messageData.ToStream(memoryStreamEx2, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
					memoryStreamEx2.Position = 0L;
					stringBuilder.Append("RFC822 {" + memoryStreamEx2.Length + "}\r\n");
					WriteLine(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
					TcpStream.WriteStream(memoryStreamEx2);
					LogAddWrite(memoryStreamEx2.Length, "Wrote " + memoryStreamEx2.Length + " bytes.");
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Rfc822Header)
				{
					MemoryStream memoryStream = new MemoryStream();
					messageData.Header.ToStream(memoryStream, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8);
					memoryStream.Position = 0L;
					stringBuilder.Append("RFC822.HEADER {" + memoryStream.Length + "}\r\n");
					WriteLine(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
					TcpStream.WriteStream(memoryStream);
					LogAddWrite(memoryStream.Length, "Wrote " + memoryStream.Length + " bytes.");
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Rfc822Size)
				{
					stringBuilder.Append("RFC822.SIZE " + e.MessageInfo.Size);
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Rfc822Text)
				{
					using MemoryStreamEx memoryStreamEx3 = new MemoryStreamEx(32000);
					messageData.Body.ToStream(memoryStreamEx3, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8), Encoding.UTF8, headerReencode: false);
					memoryStreamEx3.Position = 0L;
					stringBuilder.Append("RFC822.TEXT {" + memoryStreamEx3.Length + "}\r\n");
					WriteLine(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
					TcpStream.WriteStream(memoryStreamEx3);
					LogAddWrite(memoryStreamEx3.Length, "Wrote " + memoryStreamEx3.Length + " bytes.");
				}
				else if (iMAP_t_Fetch_i is IMAP_t_Fetch_i_Uid)
				{
					stringBuilder.Append("UID " + e.MessageInfo.UID);
				}
			}
			stringBuilder.Append(")\r\n");
			WriteLine(stringBuilder.ToString());
		};
		if (!flag)
		{
			IMAP_MessageInfo[] array4 = m_pSelectedFolder.Filter(uid, iMAP_t_SeqSet);
			foreach (IMAP_MessageInfo msgInfo in array4)
			{
				iMAP_e_Fetch.AddData(msgInfo);
			}
		}
		else
		{
			OnFetch(iMAP_e_Fetch);
		}
		WriteLine(iMAP_e_Fetch.Response.ToString().Replace("%exectime", ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2")));
	}

	private void SEARCH(bool uid, string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		long ticks = DateTime.Now.Ticks;
		StringReader stringReader = new StringReader(cmdText);
		if (stringReader.StartsWith("CHARSET", case_sensitive: false))
		{
			stringReader.ReadWord();
			string a = stringReader.ReadWord();
			if (!string.Equals(a, "US-ASCII", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(a, "UTF-8", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", new IMAP_t_orc_BadCharset(new string[2] { "US-ASCII", "UTF-8" }), "Not supported charset."));
				return;
			}
		}
		try
		{
			IMAP_Search_Key_Group criteria = IMAP_Search_Key_Group.Parse(stringReader);
			UpdateSelectedFolderAndSendChanges();
			List<int> matchedValues = new List<int>();
			IMAP_e_Search iMAP_e_Search = new IMAP_e_Search(criteria, new IMAP_r_ServerStatus(cmdTag, "OK", "SEARCH completed in %exectime seconds."));
			iMAP_e_Search.Matched += delegate(object s, EventArgs<long> e)
			{
				if (uid)
				{
					matchedValues.Add((int)e.Value);
				}
				else
				{
					int seqNo = m_pSelectedFolder.GetSeqNo(e.Value);
					if (seqNo != -1)
					{
						matchedValues.Add(seqNo);
					}
				}
			};
			OnSearch(iMAP_e_Search);
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_Search(matchedValues.ToArray()));
			m_pResponseSender.SendResponseAsync(IMAP_r_ServerStatus.Parse(iMAP_e_Search.Response.ToString().TrimEnd().Replace("%exectime", ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2"))));
		}
		catch
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
		}
	}

	private void STORE(bool uid, string cmdTag, string cmdText)
	{
		long ticks = DateTime.Now.Ticks;
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		string[] array = cmdText.Split(new char[1] { ' ' }, 3);
		if (array.Length != 3)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_t_SeqSet iMAP_t_SeqSet = null;
		try
		{
			iMAP_t_SeqSet = IMAP_t_SeqSet.Parse(array[0]);
		}
		catch
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		bool flag = false;
		IMAP_Flags_SetType setType;
		if (string.Equals(array[1], "FLAGS", StringComparison.InvariantCultureIgnoreCase))
		{
			setType = IMAP_Flags_SetType.Replace;
		}
		else if (string.Equals(array[1], "FLAGS.SILENT", StringComparison.InvariantCultureIgnoreCase))
		{
			setType = IMAP_Flags_SetType.Replace;
			flag = true;
		}
		else if (string.Equals(array[1], "+FLAGS", StringComparison.InvariantCultureIgnoreCase))
		{
			setType = IMAP_Flags_SetType.Add;
		}
		else if (string.Equals(array[1], "+FLAGS.SILENT", StringComparison.InvariantCultureIgnoreCase))
		{
			setType = IMAP_Flags_SetType.Add;
			flag = true;
		}
		else if (string.Equals(array[1], "-FLAGS", StringComparison.InvariantCultureIgnoreCase))
		{
			setType = IMAP_Flags_SetType.Remove;
		}
		else
		{
			if (!string.Equals(array[1], "-FLAGS.SILENT", StringComparison.InvariantCultureIgnoreCase))
			{
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
				return;
			}
			setType = IMAP_Flags_SetType.Remove;
			flag = true;
		}
		if (!array[2].StartsWith("(") || !array[2].EndsWith(")"))
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		List<string> list = new List<string>();
		string[] array2 = array[2].Substring(1, array[2].Length - 2).Split(' ');
		foreach (string text in array2)
		{
			if (text.Length > 0 && !list.Contains(text.Substring(1)))
			{
				list.Add(text.Substring(1));
			}
		}
		IMAP_r_ServerStatus iMAP_r_ServerStatus = new IMAP_r_ServerStatus(cmdTag, "OK", "STORE command completed in %exectime seconds.");
		IMAP_MessageInfo[] array3 = m_pSelectedFolder.Filter(uid, iMAP_t_SeqSet);
		foreach (IMAP_MessageInfo iMAP_MessageInfo in array3)
		{
			IMAP_e_Store iMAP_e_Store = OnStore(iMAP_MessageInfo, setType, list.ToArray(), iMAP_r_ServerStatus);
			iMAP_r_ServerStatus = iMAP_e_Store.Response;
			if (!string.Equals(iMAP_e_Store.Response.ResponseCode, "OK", StringComparison.InvariantCultureIgnoreCase))
			{
				break;
			}
			iMAP_MessageInfo.UpdateFlags(setType, list.ToArray());
			if (!flag)
			{
				if (uid)
				{
					m_pResponseSender.SendResponseAsync(new IMAP_r_u_Fetch(m_pSelectedFolder.GetSeqNo(iMAP_MessageInfo), new IMAP_t_Fetch_r_i[2]
					{
						new IMAP_t_Fetch_r_i_Flags(IMAP_t_MsgFlags.Parse(iMAP_MessageInfo.FlagsToImapString())),
						new IMAP_t_Fetch_r_i_Uid(iMAP_MessageInfo.UID)
					}));
				}
				else
				{
					m_pResponseSender.SendResponseAsync(new IMAP_r_u_Fetch(m_pSelectedFolder.GetSeqNo(iMAP_MessageInfo), new IMAP_t_Fetch_r_i[1]
					{
						new IMAP_t_Fetch_r_i_Flags(new IMAP_t_MsgFlags(iMAP_MessageInfo.Flags))
					}));
				}
			}
		}
		m_pResponseSender.SendResponseAsync(IMAP_r_ServerStatus.Parse(iMAP_r_ServerStatus.ToString().TrimEnd().Replace("%exectime", ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2"))));
	}

	private void COPY(bool uid, string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		string[] array = cmdText.Split(new char[1] { ' ' }, 2);
		if (array.Length != 2)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		IMAP_t_SeqSet iMAP_t_SeqSet = null;
		try
		{
			iMAP_t_SeqSet = IMAP_t_SeqSet.Parse(array[0]);
		}
		catch
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
			return;
		}
		string targetFolder = IMAP_Utils.DecodeMailbox(TextUtils.UnQuoteString(array[1]));
		UpdateSelectedFolderAndSendChanges();
		IMAP_e_Copy iMAP_e_Copy = OnCopy(targetFolder, m_pSelectedFolder.Filter(uid, iMAP_t_SeqSet), new IMAP_r_ServerStatus(cmdTag, "OK", "COPY completed."));
		m_pResponseSender.SendResponseAsync(iMAP_e_Copy.Response);
	}

	private void UID(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		string[] array = cmdText.Split(new char[1] { ' ' }, 2);
		if (string.Equals(array[0], "COPY", StringComparison.InvariantCultureIgnoreCase))
		{
			COPY(uid: true, cmdTag, array[1]);
		}
		else if (string.Equals(array[0], "FETCH", StringComparison.InvariantCultureIgnoreCase))
		{
			FETCH(uid: true, cmdTag, array[1]);
		}
		else if (string.Equals(array[0], "STORE", StringComparison.InvariantCultureIgnoreCase))
		{
			STORE(uid: true, cmdTag, array[1]);
		}
		else if (string.Equals(array[0], "SEARCH", StringComparison.InvariantCultureIgnoreCase))
		{
			SEARCH(uid: true, cmdTag, array[1]);
		}
		else
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "BAD", "Error in arguments."));
		}
	}

	private void EXPUNGE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return;
		}
		if (m_pSelectedFolder == null)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Error: This command is valid only in selected state."));
			return;
		}
		long ticks = DateTime.Now.Ticks;
		IMAP_r_ServerStatus response = new IMAP_r_ServerStatus(cmdTag, "OK", "EXPUNGE completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds.");
		for (int i = 0; i < m_pSelectedFolder.MessagesInfo.Length; i++)
		{
			IMAP_MessageInfo iMAP_MessageInfo = m_pSelectedFolder.MessagesInfo[i];
			if (iMAP_MessageInfo.ContainsFlag("Deleted"))
			{
				IMAP_e_Expunge iMAP_e_Expunge = OnExpunge(iMAP_MessageInfo, response);
				if (!string.Equals(iMAP_e_Expunge.Response.ResponseCode, "OK", StringComparison.InvariantCultureIgnoreCase))
				{
					m_pResponseSender.SendResponseAsync(iMAP_e_Expunge.Response);
					return;
				}
				m_pSelectedFolder.RemoveMessage(iMAP_MessageInfo);
				m_pResponseSender.SendResponseAsync(new IMAP_r_u_Expunge(i + 1));
			}
		}
		m_pSelectedFolder.Reindex();
		m_pResponseSender.SendResponseAsync(response);
	}

	private bool IDLE(string cmdTag, string cmdText)
	{
		if (!base.IsAuthenticated)
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "NO", "Authentication required."));
			return true;
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus("+", "idling"));
		TimerEx timer = new TimerEx(30000.0, autoReset: true);
		timer.Elapsed += delegate
		{
			try
			{
				UpdateSelectedFolderAndSendChanges();
			}
			catch
			{
			}
		};
		timer.Enabled = true;
		SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
		readLineOP.CompletedAsync += delegate
		{
			try
			{
				if (readLineOP.Error != null)
				{
					LogAddText("Error: " + readLineOP.Error.Message);
					timer.Dispose();
				}
				else if (readLineOP.BytesInBuffer == 0)
				{
					LogAddText("Remote host(connected client) closed IMAP connection.");
					timer.Dispose();
					Dispose();
				}
				else
				{
					LogAddRead(readLineOP.BytesInBuffer, readLineOP.LineUtf8);
					if (string.Equals(readLineOP.LineUtf8, "DONE", StringComparison.InvariantCultureIgnoreCase))
					{
						timer.Dispose();
						m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "IDLE terminated."));
						BeginReadCmd();
					}
					else
					{
						while (TcpStream.ReadLine(readLineOP, async: true))
						{
							if (readLineOP.Error != null)
							{
								LogAddText("Error: " + readLineOP.Error.Message);
								timer.Dispose();
								break;
							}
							LogAddRead(readLineOP.BytesInBuffer, readLineOP.LineUtf8);
							if (string.Equals(readLineOP.LineUtf8, "DONE", StringComparison.InvariantCultureIgnoreCase))
							{
								timer.Dispose();
								m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "IDLE terminated."));
								BeginReadCmd();
								break;
							}
						}
					}
				}
			}
			catch (Exception x)
			{
				timer.Dispose();
				OnError(x);
			}
		};
		while (TcpStream.ReadLine(readLineOP, async: true))
		{
			if (readLineOP.Error != null)
			{
				LogAddText("Error: " + readLineOP.Error.Message);
				timer.Dispose();
				break;
			}
			LogAddRead(readLineOP.BytesInBuffer, readLineOP.LineUtf8);
			if (string.Equals(readLineOP.LineUtf8, "DONE", StringComparison.InvariantCultureIgnoreCase))
			{
				timer.Dispose();
				m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "IDLE terminated."));
				BeginReadCmd();
				break;
			}
		}
		return false;
	}

	private void CAPABILITY(string cmdTag, string cmdText)
	{
		List<string> list = new List<string>();
		if (!IsSecureConnection && base.Certificate != null)
		{
			list.Add("STARTTLS");
		}
		foreach (string pCapability in m_pCapabilities)
		{
			list.Add(pCapability);
		}
		foreach (AUTH_SASL_ServerMechanism value in Authentications.Values)
		{
			list.Add("AUTH=" + value.Name);
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_u_Capability(list.ToArray()));
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "CAPABILITY completed."));
	}

	private void NOOP(string cmdTag, string cmdText)
	{
		long ticks = DateTime.Now.Ticks;
		if (m_pSelectedFolder != null)
		{
			UpdateSelectedFolderAndSendChanges();
		}
		m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "NOOP Completed in " + ((decimal)(DateTime.Now.Ticks - ticks) / 10000000m).ToString("f2") + " seconds."));
	}

	private void LOGOUT(string cmdTag, string cmdText)
	{
		try
		{
			m_pResponseSender.SendResponseAsync(new IMAP_r_u_Bye("IMAP4rev1 Server logging out."));
			EventHandler<EventArgs<Exception>> completedAsyncCallback = delegate
			{
				try
				{
					Disconnect();
					Dispose();
				}
				catch
				{
				}
			};
			if (!m_pResponseSender.SendResponseAsync(new IMAP_r_ServerStatus(cmdTag, "OK", "LOGOUT completed."), completedAsyncCallback))
			{
				Disconnect();
				Dispose();
			}
		}
		catch
		{
			Disconnect();
			Dispose();
		}
	}

	private void WriteLine(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		byte[] array = null;
		array = ((!line.EndsWith("\r\n")) ? Encoding.UTF8.GetBytes(line + "\r\n") : Encoding.UTF8.GetBytes(line));
		TcpStream.Write(array, 0, array.Length);
		if (Server.Logger != null)
		{
			Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, array.Length, line, LocalEndPoint, RemoteEndPoint);
		}
	}

	public void LogAddRead(long size, string text)
	{
		try
		{
			if (Server.Logger != null)
			{
				Server.Logger.AddRead(ID, AuthenticatedUserIdentity, size, text, LocalEndPoint, RemoteEndPoint);
			}
		}
		catch
		{
		}
	}

	public void LogAddWrite(long size, string text)
	{
		try
		{
			if (Server.Logger != null)
			{
				Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, size, text, LocalEndPoint, RemoteEndPoint);
			}
		}
		catch
		{
		}
	}

	public void LogAddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		try
		{
			if (Server.Logger != null)
			{
				Server.Logger.AddText(ID, AuthenticatedUserIdentity, text, LocalEndPoint, RemoteEndPoint);
			}
		}
		catch
		{
		}
	}

	public void LogAddException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		try
		{
			if (Server.Logger != null)
			{
				Server.Logger.AddException(ID, AuthenticatedUserIdentity, exception.Message, LocalEndPoint, RemoteEndPoint, exception);
			}
		}
		catch
		{
		}
	}

	private void UpdateSelectedFolderAndSendChanges()
	{
		if (m_pSelectedFolder == null)
		{
			return;
		}
		IMAP_e_MessagesInfo iMAP_e_MessagesInfo = OnGetMessagesInfo(m_pSelectedFolder.Folder);
		int num = m_pSelectedFolder.MessagesInfo.Length;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (IMAP_MessageInfo item in iMAP_e_MessagesInfo.MessagesInfo)
		{
			dictionary.Add(item.ID, null);
		}
		StringBuilder stringBuilder = new StringBuilder();
		IMAP_MessageInfo[] messagesInfo = m_pSelectedFolder.MessagesInfo;
		foreach (IMAP_MessageInfo iMAP_MessageInfo in messagesInfo)
		{
			if (!dictionary.ContainsKey(iMAP_MessageInfo.ID))
			{
				stringBuilder.Append("* " + m_pSelectedFolder.GetSeqNo(iMAP_MessageInfo) + " EXPUNGE\r\n");
				m_pSelectedFolder.RemoveMessage(iMAP_MessageInfo);
			}
		}
		if (num != iMAP_e_MessagesInfo.MessagesInfo.Count)
		{
			stringBuilder.Append("* " + iMAP_e_MessagesInfo.MessagesInfo.Count + " EXISTS\r\n");
		}
		if (stringBuilder.Length > 0)
		{
			WriteLine(stringBuilder.ToString());
		}
		m_pSelectedFolder = new _SelectedFolder(m_pSelectedFolder.Folder, m_pSelectedFolder.IsReadOnly, iMAP_e_MessagesInfo.MessagesInfo);
	}

	private bool SupportsCap(string capability)
	{
		foreach (string pCapability in m_pCapabilities)
		{
			if (string.Equals(pCapability, capability, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private string ParsePartNumberFromSection(string section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = section.Split('.');
		foreach (string value in array)
		{
			if (!Net_Utils.IsInteger(value))
			{
				break;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(".");
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	private string ParsePartSpecifierFromSection(string section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = section.Split(' ')[0].Split('.');
		foreach (string value in array)
		{
			if (!Net_Utils.IsInteger(value))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(".");
				}
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	public MIME_Entity GetMimeEntity(Mail_Message message, string partNumber)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		if (partNumber == string.Empty)
		{
			return message;
		}
		if (message.ContentType == null || message.ContentType.Type.ToLower() != "multipart")
		{
			if (Convert.ToInt32(partNumber) == 1)
			{
				return message;
			}
			return null;
		}
		MIME_Entity mIME_Entity = message;
		string[] array = partNumber.Split('.');
		for (int i = 0; i < array.Length; i++)
		{
			int num = Convert.ToInt32(array[i]) - 1;
			if (mIME_Entity.Body is MIME_b_Multipart)
			{
				MIME_b_Multipart mIME_b_Multipart = (MIME_b_Multipart)mIME_Entity.Body;
				if (num > -1 && num < mIME_b_Multipart.BodyParts.Count)
				{
					mIME_Entity = mIME_b_Multipart.BodyParts[num];
					continue;
				}
				return null;
			}
			return null;
		}
		return mIME_Entity;
	}

	public string ConstructBodyStructure(Mail_Message message, bool bodystructure)
	{
		if (bodystructure)
		{
			return "BODYSTRUCTURE " + ConstructParts(message, bodystructure);
		}
		return "BODY " + ConstructParts(message, bodystructure);
	}

	private string ConstructParts(MIME_Entity entity, bool bodystructure)
	{
		MIME_Encoding_EncodedWord mIME_Encoding_EncodedWord = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8);
		mIME_Encoding_EncodedWord.Split = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (entity.Body is MIME_b_Multipart)
		{
			stringBuilder.Append("(");
			foreach (MIME_Entity bodyPart in ((MIME_b_Multipart)entity.Body).BodyParts)
			{
				stringBuilder.Append(ConstructParts(bodyPart, bodystructure));
			}
			if (entity.ContentType != null && entity.ContentType.SubType != null)
			{
				stringBuilder.Append(" \"" + entity.ContentType.SubType + "\"");
			}
			else
			{
				stringBuilder.Append(" \"plain\"");
			}
			stringBuilder.Append(")");
		}
		else
		{
			stringBuilder.Append("(");
			if (entity.ContentType != null && entity.ContentType.Type != null)
			{
				stringBuilder.Append("\"" + entity.ContentType.Type + "\"");
			}
			else
			{
				stringBuilder.Append("\"text\"");
			}
			if (entity.ContentType != null && entity.ContentType.SubType != null)
			{
				stringBuilder.Append(" \"" + entity.ContentType.SubType + "\"");
			}
			else
			{
				stringBuilder.Append(" \"plain\"");
			}
			if (entity.ContentType != null)
			{
				if (entity.ContentType.Parameters.Count > 0)
				{
					stringBuilder.Append(" (");
					bool flag = true;
					foreach (MIME_h_Parameter parameter in entity.ContentType.Parameters)
					{
						if (flag)
						{
							flag = false;
						}
						else
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append("\"" + parameter.Name + "\" \"" + mIME_Encoding_EncodedWord.Encode(parameter.Value) + "\"");
					}
					stringBuilder.Append(")");
				}
				else
				{
					stringBuilder.Append(" NIL");
				}
			}
			else
			{
				stringBuilder.Append(" NIL");
			}
			string contentID = entity.ContentID;
			if (contentID != null)
			{
				stringBuilder.Append(" \"" + mIME_Encoding_EncodedWord.Encode(contentID) + "\"");
			}
			else
			{
				stringBuilder.Append(" NIL");
			}
			string contentDescription = entity.ContentDescription;
			if (contentDescription != null)
			{
				stringBuilder.Append(" \"" + mIME_Encoding_EncodedWord.Encode(contentDescription) + "\"");
			}
			else
			{
				stringBuilder.Append(" NIL");
			}
			if (entity.ContentTransferEncoding != null)
			{
				stringBuilder.Append(" \"" + mIME_Encoding_EncodedWord.Encode(entity.ContentTransferEncoding) + "\"");
			}
			else
			{
				stringBuilder.Append(" \"7bit\"");
			}
			if (entity.Body is MIME_b_SinglepartBase)
			{
				stringBuilder.Append(" " + ((MIME_b_SinglepartBase)entity.Body).EncodedData.Length);
			}
			else
			{
				stringBuilder.Append(" 0");
			}
			if (entity.Body is MIME_b_MessageRfc822)
			{
				stringBuilder.Append(" " + IMAP_t_Fetch_r_i_Envelope.ConstructEnvelope(((MIME_b_MessageRfc822)entity.Body).Message));
				stringBuilder.Append(" NIL");
				stringBuilder.Append(" NIL");
			}
			if (entity.Body is MIME_b_Text)
			{
				long num = 0L;
				StreamLineReader streamLineReader = new StreamLineReader(new MemoryStream(((MIME_b_SinglepartBase)entity.Body).EncodedData));
				for (byte[] array = streamLineReader.ReadLine(); array != null; array = streamLineReader.ReadLine())
				{
					num++;
				}
				stringBuilder.Append(" " + num);
			}
			if (bodystructure)
			{
				stringBuilder.Append(" NIL");
				if (entity.ContentDisposition != null && entity.ContentDisposition.Parameters.Count > 0)
				{
					stringBuilder.Append(" (\"" + entity.ContentDisposition.DispositionType + "\"");
					if (entity.ContentDisposition.Parameters.Count > 0)
					{
						stringBuilder.Append(" (");
						bool flag2 = true;
						foreach (MIME_h_Parameter parameter2 in entity.ContentDisposition.Parameters)
						{
							if (flag2)
							{
								flag2 = false;
							}
							else
							{
								stringBuilder.Append(" ");
							}
							stringBuilder.Append("\"" + parameter2.Name + "\" \"" + mIME_Encoding_EncodedWord.Encode(parameter2.Value) + "\"");
						}
						stringBuilder.Append(")");
					}
					else
					{
						stringBuilder.Append(" NIL");
					}
					stringBuilder.Append(")");
				}
				else
				{
					stringBuilder.Append(" NIL");
				}
				stringBuilder.Append(" NIL");
				stringBuilder.Append(" NIL");
			}
			stringBuilder.Append(")");
		}
		return stringBuilder.ToString();
	}

	private IMAP_e_Started OnStarted(IMAP_r_u_ServerStatus response)
	{
		IMAP_e_Started iMAP_e_Started = new IMAP_e_Started(response);
		if (this.Started != null)
		{
			this.Started(this, iMAP_e_Started);
		}
		return iMAP_e_Started;
	}

	private IMAP_e_Login OnLogin(string user, string password)
	{
		IMAP_e_Login iMAP_e_Login = new IMAP_e_Login(user, password);
		if (this.Login != null)
		{
			this.Login(this, iMAP_e_Login);
		}
		return iMAP_e_Login;
	}

	private IMAP_e_Namespace OnNamespace(IMAP_r_ServerStatus response)
	{
		IMAP_e_Namespace iMAP_e_Namespace = new IMAP_e_Namespace(response);
		if (this.Namespace != null)
		{
			this.Namespace(this, iMAP_e_Namespace);
		}
		return iMAP_e_Namespace;
	}

	private IMAP_e_List OnList(string refName, string folder)
	{
		IMAP_e_List iMAP_e_List = new IMAP_e_List(refName, folder);
		if (this.List != null)
		{
			this.List(this, iMAP_e_List);
		}
		return iMAP_e_List;
	}

	private IMAP_e_Folder OnCreate(string cmdTag, string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_Folder iMAP_e_Folder = new IMAP_e_Folder(cmdTag, folder, response);
		if (this.Create != null)
		{
			this.Create(this, iMAP_e_Folder);
		}
		return iMAP_e_Folder;
	}

	private IMAP_e_Folder OnDelete(string cmdTag, string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_Folder iMAP_e_Folder = new IMAP_e_Folder(cmdTag, folder, response);
		if (this.Delete != null)
		{
			this.Delete(this, iMAP_e_Folder);
		}
		return iMAP_e_Folder;
	}

	private IMAP_e_Rename OnRename(string cmdTag, string currentFolder, string newFolder)
	{
		IMAP_e_Rename iMAP_e_Rename = new IMAP_e_Rename(cmdTag, currentFolder, newFolder);
		if (this.Rename != null)
		{
			this.Rename(this, iMAP_e_Rename);
		}
		return iMAP_e_Rename;
	}

	private IMAP_e_LSub OnLSub(string refName, string folder)
	{
		IMAP_e_LSub iMAP_e_LSub = new IMAP_e_LSub(refName, folder);
		if (this.LSub != null)
		{
			this.LSub(this, iMAP_e_LSub);
		}
		return iMAP_e_LSub;
	}

	private IMAP_e_Folder OnSubscribe(string cmdTag, string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_Folder iMAP_e_Folder = new IMAP_e_Folder(cmdTag, folder, response);
		if (this.Subscribe != null)
		{
			this.Subscribe(this, iMAP_e_Folder);
		}
		return iMAP_e_Folder;
	}

	private IMAP_e_Folder OnUnsubscribe(string cmdTag, string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_Folder iMAP_e_Folder = new IMAP_e_Folder(cmdTag, folder, response);
		if (this.Unsubscribe != null)
		{
			this.Unsubscribe(this, iMAP_e_Folder);
		}
		return iMAP_e_Folder;
	}

	private IMAP_e_Select OnSelect(string cmdTag, string folder)
	{
		IMAP_e_Select iMAP_e_Select = new IMAP_e_Select(cmdTag, folder);
		if (this.Select != null)
		{
			this.Select(this, iMAP_e_Select);
		}
		return iMAP_e_Select;
	}

	private IMAP_e_MessagesInfo OnGetMessagesInfo(string folder)
	{
		IMAP_e_MessagesInfo iMAP_e_MessagesInfo = new IMAP_e_MessagesInfo(folder);
		if (this.GetMessagesInfo != null)
		{
			this.GetMessagesInfo(this, iMAP_e_MessagesInfo);
		}
		return iMAP_e_MessagesInfo;
	}

	private IMAP_e_Append OnAppend(string folder, string[] flags, DateTime date, int size, IMAP_r_ServerStatus response)
	{
		IMAP_e_Append iMAP_e_Append = new IMAP_e_Append(folder, flags, date, size, response);
		if (this.Append != null)
		{
			this.Append(this, iMAP_e_Append);
		}
		return iMAP_e_Append;
	}

	private IMAP_e_GetQuotaRoot OnGetGuotaRoot(string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_GetQuotaRoot iMAP_e_GetQuotaRoot = new IMAP_e_GetQuotaRoot(folder, response);
		if (this.GetQuotaRoot != null)
		{
			this.GetQuotaRoot(this, iMAP_e_GetQuotaRoot);
		}
		return iMAP_e_GetQuotaRoot;
	}

	private IMAP_e_GetQuota OnGetQuota(string quotaRoot, IMAP_r_ServerStatus response)
	{
		IMAP_e_GetQuota iMAP_e_GetQuota = new IMAP_e_GetQuota(quotaRoot, response);
		if (this.GetQuota != null)
		{
			this.GetQuota(this, iMAP_e_GetQuota);
		}
		return iMAP_e_GetQuota;
	}

	private IMAP_e_GetAcl OnGetAcl(string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_GetAcl iMAP_e_GetAcl = new IMAP_e_GetAcl(folder, response);
		if (this.GetAcl != null)
		{
			this.GetAcl(this, iMAP_e_GetAcl);
		}
		return iMAP_e_GetAcl;
	}

	private IMAP_e_SetAcl OnSetAcl(string folder, string identifier, IMAP_Flags_SetType flagsSetType, string rights, IMAP_r_ServerStatus response)
	{
		IMAP_e_SetAcl iMAP_e_SetAcl = new IMAP_e_SetAcl(folder, identifier, flagsSetType, rights, response);
		if (this.SetAcl != null)
		{
			this.SetAcl(this, iMAP_e_SetAcl);
		}
		return iMAP_e_SetAcl;
	}

	private IMAP_e_DeleteAcl OnDeleteAcl(string folder, string identifier, IMAP_r_ServerStatus response)
	{
		IMAP_e_DeleteAcl iMAP_e_DeleteAcl = new IMAP_e_DeleteAcl(folder, identifier, response);
		if (this.DeleteAcl != null)
		{
			this.DeleteAcl(this, iMAP_e_DeleteAcl);
		}
		return iMAP_e_DeleteAcl;
	}

	private IMAP_e_ListRights OnListRights(string folder, string identifier, IMAP_r_ServerStatus response)
	{
		IMAP_e_ListRights iMAP_e_ListRights = new IMAP_e_ListRights(folder, identifier, response);
		if (this.ListRights != null)
		{
			this.ListRights(this, iMAP_e_ListRights);
		}
		return iMAP_e_ListRights;
	}

	private IMAP_e_MyRights OnMyRights(string folder, IMAP_r_ServerStatus response)
	{
		IMAP_e_MyRights iMAP_e_MyRights = new IMAP_e_MyRights(folder, response);
		if (this.MyRights != null)
		{
			this.MyRights(this, iMAP_e_MyRights);
		}
		return iMAP_e_MyRights;
	}

	private void OnFetch(IMAP_e_Fetch e)
	{
		if (this.Fetch != null)
		{
			this.Fetch(this, e);
		}
	}

	private void OnSearch(IMAP_e_Search e)
	{
		if (this.Search != null)
		{
			this.Search(this, e);
		}
	}

	private IMAP_e_Store OnStore(IMAP_MessageInfo msgInfo, IMAP_Flags_SetType setType, string[] flags, IMAP_r_ServerStatus response)
	{
		IMAP_e_Store iMAP_e_Store = new IMAP_e_Store(m_pSelectedFolder.Folder, msgInfo, setType, flags, response);
		if (this.Store != null)
		{
			this.Store(this, iMAP_e_Store);
		}
		return iMAP_e_Store;
	}

	private IMAP_e_Copy OnCopy(string targetFolder, IMAP_MessageInfo[] messagesInfo, IMAP_r_ServerStatus response)
	{
		IMAP_e_Copy iMAP_e_Copy = new IMAP_e_Copy(m_pSelectedFolder.Folder, targetFolder, messagesInfo, response);
		if (this.Copy != null)
		{
			this.Copy(this, iMAP_e_Copy);
		}
		return iMAP_e_Copy;
	}

	private IMAP_e_Expunge OnExpunge(IMAP_MessageInfo msgInfo, IMAP_r_ServerStatus response)
	{
		IMAP_e_Expunge iMAP_e_Expunge = new IMAP_e_Expunge(m_pSelectedFolder.Folder, msgInfo, response);
		if (this.Expunge != null)
		{
			this.Expunge(this, iMAP_e_Expunge);
		}
		return iMAP_e_Expunge;
	}
}
