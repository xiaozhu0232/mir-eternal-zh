using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IO;
using LumiSoft.Net.Mail;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.SMTP.Server;

public class SMTP_Session : TCP_ServerSession
{
	private class ReadCommandAsyncOP
	{
	}

	private class SendResponseAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private SMTP_t_ReplyLine[] m_pReplyLines;

		private SMTP_Session m_pSession;

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

		public event EventHandler<EventArgs<SendResponseAsyncOP>> CompletedAsync;

		public SendResponseAsyncOP(SMTP_t_ReplyLine reply)
		{
			if (reply == null)
			{
				throw new ArgumentNullException("reply");
			}
			m_pReplyLines = new SMTP_t_ReplyLine[1] { reply };
		}

		public SendResponseAsyncOP(SMTP_t_ReplyLine[] replyLines)
		{
			if (replyLines == null)
			{
				throw new ArgumentNullException("replyLines");
			}
			if (replyLines.Length < 1)
			{
				throw new ArgumentException("Argument 'replyLines' must contain at least 1 item.", "replyLines");
			}
			m_pReplyLines = replyLines;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pReplyLines = null;
				m_pSession = null;
				this.CompletedAsync = null;
			}
		}

		public bool Start(SMTP_Session owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSession = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				SMTP_t_ReplyLine[] pReplyLines = m_pReplyLines;
				foreach (SMTP_t_ReplyLine sMTP_t_ReplyLine in pReplyLines)
				{
					stringBuilder.Append(sMTP_t_ReplyLine.ToString());
				}
				byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
				m_pSession.LogAddWrite(bytes.Length, stringBuilder.ToString());
				m_pSession.TcpStream.BeginWrite(bytes, 0, bytes.Length, ResponseSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pSession.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ResponseSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSession.TcpStream.EndWrite(ar);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pSession.LogAddException("Exception: " + m_pException.Message, m_pException);
			}
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<SendResponseAsyncOP>(this));
			}
		}
	}

	private class Cmd_DATA : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private SMTP_Session m_pSession;

		private DateTime m_StartTime;

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

		public event EventHandler<EventArgs<Cmd_DATA>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pSession = null;
				this.CompletedAsync = null;
			}
		}

		public bool Start(SMTP_Session owner, string cmdText)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSession = owner;
			m_StartTime = DateTime.Now;
			SetState(AsyncOP_State.Active);
			try
			{
				if (m_pSession.m_SessionRejected)
				{
					SendFinalResponse(new SMTP_t_ReplyLine(503, "Bad sequence of commands: Session rejected.", isLastLine: true));
				}
				else if (string.IsNullOrEmpty(m_pSession.m_EhloHost))
				{
					SendFinalResponse(new SMTP_t_ReplyLine(503, "Bad sequence of commands: Send EHLO/HELO first.", isLastLine: true));
				}
				else if (m_pSession.m_pFrom == null)
				{
					SendFinalResponse(new SMTP_t_ReplyLine(503, "Bad sequence of commands: Send 'MAIL FROM:' first.", isLastLine: true));
				}
				else if (m_pSession.m_pTo.Count == 0)
				{
					SendFinalResponse(new SMTP_t_ReplyLine(503, "Bad sequence of commands: Send 'RCPT TO:' first.", isLastLine: true));
				}
				else if (!string.IsNullOrEmpty(cmdText))
				{
					SendFinalResponse(new SMTP_t_ReplyLine(500, "Command line syntax error.", isLastLine: true));
				}
				else
				{
					m_pSession.m_pMessageStream = m_pSession.OnGetMessageStream();
					if (m_pSession.m_pMessageStream == null)
					{
						m_pSession.m_pMessageStream = new MemoryStreamEx(32000);
					}
					SendResponseAsyncOP sendResponseOP = new SendResponseAsyncOP(new SMTP_t_ReplyLine(354, "Start mail input; end with <CRLF>.<CRLF>", isLastLine: true));
					sendResponseOP.CompletedAsync += delegate
					{
						Send354ResponseCompleted(sendResponseOP);
					};
					if (!m_pSession.SendResponseAsync(sendResponseOP))
					{
						Send354ResponseCompleted(sendResponseOP);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pSession.LogAddException("Exception: " + m_pException.Message, m_pException);
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
				if (m_State == AsyncOP_State.Completed)
				{
					m_pSession.Reset();
				}
				if (m_State == AsyncOP_State.Completed && m_RiseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void SendFinalResponse(SMTP_t_ReplyLine reply)
		{
			try
			{
				if (reply == null)
				{
					throw new ArgumentNullException("reply");
				}
				SendResponseAsyncOP sendResponseOP = new SendResponseAsyncOP(reply);
				sendResponseOP.CompletedAsync += delegate
				{
					SendFinalResponseCompleted(sendResponseOP);
				};
				if (!m_pSession.SendResponseAsync(sendResponseOP))
				{
					SendFinalResponseCompleted(sendResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pSession.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void SendFinalResponseCompleted(SendResponseAsyncOP op)
		{
			if (op.Error != null)
			{
				m_pException = op.Error;
			}
			SetState(AsyncOP_State.Completed);
			op.Dispose();
		}

		private void Send354ResponseCompleted(SendResponseAsyncOP op)
		{
			try
			{
				byte[] array = m_pSession.CreateReceivedHeader();
				m_pSession.m_pMessageStream.Write(array, 0, array.Length);
				SmartStream.ReadPeriodTerminatedAsyncOP readPeriodTermOP = new SmartStream.ReadPeriodTerminatedAsyncOP(m_pSession.m_pMessageStream, m_pSession.Server.MaxMessageSize, SizeExceededAction.JunkAndThrowException);
				readPeriodTermOP.CompletedAsync += delegate
				{
					MessageReadingCompleted(readPeriodTermOP);
				};
				if (m_pSession.TcpStream.ReadPeriodTerminated(readPeriodTermOP, async: true))
				{
					MessageReadingCompleted(readPeriodTermOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pSession.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void MessageReadingCompleted(SmartStream.ReadPeriodTerminatedAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					if (op.Error is LineSizeExceededException)
					{
						SendFinalResponse(new SMTP_t_ReplyLine(500, "Line too long.", isLastLine: true));
					}
					else if (op.Error is DataSizeExceededException)
					{
						SendFinalResponse(new SMTP_t_ReplyLine(552, "Too much mail data.", isLastLine: true));
					}
					else
					{
						m_pException = op.Error;
					}
					m_pSession.OnMessageStoringCanceled();
				}
				else
				{
					m_pSession.LogAddRead(op.BytesStored, "Readed " + op.BytesStored + " message bytes.");
					SMTP_Reply reply = new SMTP_Reply(250, "DATA completed in " + (DateTime.Now - m_StartTime).TotalSeconds.ToString("f2") + " seconds.");
					reply = m_pSession.OnMessageStoringCompleted(reply);
					SendFinalResponse(SMTP_t_ReplyLine.Parse(reply.ReplyCode + " " + reply.ReplyLines[0]));
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			if (m_pException != null)
			{
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<Cmd_DATA>(this));
			}
		}
	}

	private Dictionary<string, AUTH_SASL_ServerMechanism> m_pAuthentications;

	private int m_BadCommands;

	private int m_Transactions;

	private bool m_SessionRejected;

	private string m_EhloHost;

	private GenericIdentity m_pUser;

	private SMTP_MailFrom m_pFrom;

	private Dictionary<string, SMTP_RcptTo> m_pTo;

	private Stream m_pMessageStream;

	private int m_BDatReadedCount;

	public new SMTP_Server Server
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (SMTP_Server)base.Server;
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

	public int Transactions
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Transactions;
		}
	}

	public string EhloHost
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_EhloHost;
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

	public SMTP_MailFrom From
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pFrom;
		}
	}

	public SMTP_RcptTo[] To
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (m_pTo)
			{
				SMTP_RcptTo[] array = new SMTP_RcptTo[m_pTo.Count];
				m_pTo.Values.CopyTo(array, 0);
				return array;
			}
		}
	}

	public event EventHandler<SMTP_e_Started> Started;

	public event EventHandler<SMTP_e_Ehlo> Ehlo;

	public event EventHandler<SMTP_e_MailFrom> MailFrom;

	public event EventHandler<SMTP_e_RcptTo> RcptTo;

	public event EventHandler<SMTP_e_Message> GetMessageStream;

	public event EventHandler MessageStoringCanceled;

	public event EventHandler<SMTP_e_MessageStored> MessageStoringCompleted;

	public SMTP_Session()
	{
		m_pAuthentications = new Dictionary<string, AUTH_SASL_ServerMechanism>(StringComparer.CurrentCultureIgnoreCase);
		m_pTo = new Dictionary<string, SMTP_RcptTo>();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			m_pAuthentications = null;
			m_EhloHost = null;
			m_pUser = null;
			m_pFrom = null;
			m_pTo = null;
			if (m_pMessageStream != null)
			{
				m_pMessageStream.Dispose();
				m_pMessageStream = null;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		try
		{
			SMTP_Reply sMTP_Reply = null;
			sMTP_Reply = ((!string.IsNullOrEmpty(Server.GreetingText)) ? new SMTP_Reply(220, Server.GreetingText) : new SMTP_Reply(220, "<" + Net_Utils.GetLocalHostName(base.LocalHostName) + "> Simple Mail Transfer Service Ready."));
			sMTP_Reply = OnStarted(sMTP_Reply);
			WriteLine(sMTP_Reply.ToString());
			if (sMTP_Reply.ReplyCode >= 300)
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
				WriteLine("500 Internal server error.");
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
			if (m_pMessageStream != null)
			{
				OnMessageStoringCanceled();
			}
			WriteLine("421 Idle timeout, closing connection.");
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
			if (Server.Logger != null)
			{
				Server.Logger.AddRead(ID, AuthenticatedUserIdentity, op.BytesInBuffer, op.LineUtf8, LocalEndPoint, RemoteEndPoint);
			}
			string[] array = Encoding.UTF8.GetString(op.Buffer, 0, op.LineBytesInBuffer).Split(new char[1] { ' ' }, 2);
			string text = array[0].ToUpperInvariant();
			string cmdText = ((array.Length == 2) ? array[1] : "");
			switch (text)
			{
			case "EHLO":
				EHLO(cmdText);
				return result;
			case "HELO":
				HELO(cmdText);
				return result;
			case "STARTTLS":
				STARTTLS(cmdText);
				return result;
			case "AUTH":
				AUTH(cmdText);
				return result;
			case "MAIL":
				MAIL(cmdText);
				return result;
			case "RCPT":
				RCPT(cmdText);
				return result;
			case "DATA":
			{
				Cmd_DATA cmdData = new Cmd_DATA();
				cmdData.CompletedAsync += delegate
				{
					if (cmdData.Error != null)
					{
						if (cmdData.Error is IncompleteDataException)
						{
							LogAddText("Disposing SMTP session, remote endpoint closed socket.");
						}
						else
						{
							LogAddText("Disposing SMTP session, fatal error:" + cmdData.Error.Message);
							OnError(cmdData.Error);
						}
						Dispose();
					}
					else
					{
						BeginReadCmd();
					}
					cmdData.Dispose();
				};
				if (!cmdData.Start(this, cmdText))
				{
					if (cmdData.Error != null)
					{
						if (cmdData.Error is IncompleteDataException)
						{
							LogAddText("Disposing SMTP session, remote endpoint closed socket.");
						}
						else
						{
							LogAddText("Disposing SMTP session, fatal error:" + cmdData.Error.Message);
							OnError(cmdData.Error);
						}
						Dispose();
						result = false;
					}
					cmdData.Dispose();
					return result;
				}
				result = false;
				return result;
			}
			case "BDAT":
				result = BDAT(cmdText);
				return result;
			case "RSET":
				RSET(cmdText);
				return result;
			case "NOOP":
				NOOP(cmdText);
				return result;
			case "QUIT":
				QUIT(cmdText);
				result = false;
				return result;
			default:
				m_BadCommands++;
				if (Server.MaxBadCommands != 0 && m_BadCommands > Server.MaxBadCommands)
				{
					WriteLine("421 Too many bad commands, closing transmission channel.");
					Disconnect();
					return false;
				}
				WriteLine("502 Error: command '" + text + "' not recognized.");
				return result;
			}
		}
		catch (Exception x)
		{
			OnError(x);
			return result;
		}
	}

	private void ReadCommandAsync(ReadCommandAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
	}

	private void ReadCommandCompleted(ReadCommandAsyncOP op)
	{
		if (!base.IsDisposed)
		{
		}
	}

	private bool SendResponseAsync(SendResponseAsyncOP op)
	{
		if (base.IsDisposed)
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

	private void EHLO(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
			return;
		}
		if (string.IsNullOrEmpty(cmdText) || cmdText.Split(' ').Length != 1)
		{
			WriteLine("501 Syntax error, syntax: \"EHLO\" SP hostname CRLF");
			return;
		}
		List<string> list = new List<string>();
		list.Add(Net_Utils.GetLocalHostName(base.LocalHostName));
		if (Server.Extentions.Contains(SMTP_ServiceExtensions.PIPELINING))
		{
			list.Add(SMTP_ServiceExtensions.PIPELINING);
		}
		if (Server.Extentions.Contains(SMTP_ServiceExtensions.SIZE))
		{
			list.Add(SMTP_ServiceExtensions.SIZE + " " + Server.MaxMessageSize);
		}
		if (Server.Extentions.Contains(SMTP_ServiceExtensions.STARTTLS) && !IsSecureConnection && base.Certificate != null)
		{
			list.Add(SMTP_ServiceExtensions.STARTTLS);
		}
		if (Server.Extentions.Contains(SMTP_ServiceExtensions._8BITMIME))
		{
			list.Add(SMTP_ServiceExtensions._8BITMIME);
		}
		if (Server.Extentions.Contains(SMTP_ServiceExtensions.BINARYMIME))
		{
			list.Add(SMTP_ServiceExtensions.BINARYMIME);
		}
		if (Server.Extentions.Contains(SMTP_ServiceExtensions.CHUNKING))
		{
			list.Add(SMTP_ServiceExtensions.CHUNKING);
		}
		if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN))
		{
			list.Add(SMTP_ServiceExtensions.DSN);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (AUTH_SASL_ServerMechanism value in Authentications.Values)
		{
			if (!value.RequireSSL || (value.RequireSSL && IsSecureConnection))
			{
				stringBuilder.Append(value.Name + " ");
			}
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(SMTP_ServiceExtensions.AUTH + " " + stringBuilder.ToString().Trim());
		}
		SMTP_Reply reply = new SMTP_Reply(250, list.ToArray());
		reply = OnEhlo(cmdText, reply);
		if (reply.ReplyCode < 300)
		{
			m_EhloHost = cmdText;
			Reset();
		}
		WriteLine(reply.ToString());
	}

	private void HELO(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
			return;
		}
		if (string.IsNullOrEmpty(cmdText) || cmdText.Split(' ').Length != 1)
		{
			WriteLine("501 Syntax error, syntax: \"HELO\" SP hostname CRLF");
			return;
		}
		SMTP_Reply reply = new SMTP_Reply(250, Net_Utils.GetLocalHostName(base.LocalHostName));
		reply = OnEhlo(cmdText, reply);
		if (reply.ReplyCode < 300)
		{
			m_EhloHost = cmdText;
			Reset();
		}
		WriteLine(reply.ToString());
	}

	private void STARTTLS(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!string.IsNullOrEmpty(cmdText))
		{
			WriteLine("501 Syntax error: No parameters allowed.");
			return;
		}
		if (IsSecureConnection)
		{
			WriteLine("503 Bad sequence of commands: Connection is already secure.");
			return;
		}
		if (base.Certificate == null)
		{
			WriteLine("454 TLS not available: Server has no SSL certificate.");
			return;
		}
		WriteLine("220 Ready to start TLS.");
		try
		{
			SwitchToSecure();
			LogAddText("TLS negotiation completed successfully.");
			m_EhloHost = null;
			Reset();
		}
		catch (Exception ex)
		{
			LogAddText("TLS negotiation failed: " + ex.Message + ".");
			Disconnect();
		}
	}

	private void AUTH(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.IsAuthenticated)
		{
			WriteLine("503 Bad sequence of commands: you are already authenticated.");
			return;
		}
		if (m_pFrom != null)
		{
			WriteLine("503 Bad sequence of commands: The AUTH command is not permitted during a mail transaction.");
			return;
		}
		string[] array = cmdText.Split(' ');
		if (array.Length > 2)
		{
			WriteLine("501 Syntax error, syntax: AUTH SP mechanism [SP initial-response] CRLF");
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
				WriteLine("501 Syntax error: Parameter 'initial-response' value must be BASE64 or contain a single character '='.");
				return;
			}
		}
		string key = array[0];
		if (!Authentications.ContainsKey(key))
		{
			WriteLine("501 Not supported authentication mechanism.");
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
					WriteLine("235 2.7.0 Authentication succeeded.");
				}
				else
				{
					WriteLine("535 5.7.8 Authentication credentials invalid.");
				}
				return;
			}
			if (array4.Length == 0)
			{
				WriteLine("334 ");
			}
			else
			{
				WriteLine("334 " + Convert.ToBase64String(array4));
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
				WriteLine("501 Invalid client response '" + array3?.ToString() + "'.");
				return;
			}
		}
		WriteLine("501 Authentication canceled.");
	}

	private void MAIL(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
		}
		else if (string.IsNullOrEmpty(m_EhloHost))
		{
			WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
		}
		else if (m_pFrom != null)
		{
			WriteLine("503 Bad sequence of commands: nested MAIL command.");
		}
		else if (m_pMessageStream != null)
		{
			WriteLine("503 Bad sequence of commands: BDAT command is pending.");
		}
		else if (Server.MaxTransactions != 0 && m_Transactions >= Server.MaxTransactions)
		{
			WriteLine("503 Bad sequence of commands: Maximum allowed mail transactions exceeded.");
		}
		else if (cmdText.ToUpper().StartsWith("FROM:"))
		{
			cmdText = cmdText.Substring(5).Trim();
			string text = "";
			int result = -1;
			string body = null;
			SMTP_DSN_Ret ret = SMTP_DSN_Ret.NotSpecified;
			string envid = null;
			if (!cmdText.StartsWith("<") || cmdText.IndexOf('>') == -1)
			{
				WriteLine("501 Syntax error, syntax: \"MAIL FROM:\" \"<\" address \">\" / \"<>\" [SP Mail-parameters] CRLF");
				return;
			}
			text = cmdText.Substring(1, cmdText.IndexOf('>') - 1).Trim();
			cmdText = cmdText.Substring(cmdText.IndexOf('>') + 1).Trim();
			string[] array = (string.IsNullOrEmpty(cmdText) ? new string[0] : cmdText.Split(' '));
			foreach (string text2 in array)
			{
				string[] array2 = text2.Split(new char[1] { '=' }, 2);
				if (Server.Extentions.Contains(SMTP_ServiceExtensions.SIZE) && array2[0].ToUpper() == "SIZE")
				{
					if (array2.Length == 1)
					{
						WriteLine("501 Syntax error: SIZE parameter value must be specified.");
						return;
					}
					if (!int.TryParse(array2[1], out result))
					{
						WriteLine("501 Syntax error: SIZE parameter value must be integer.");
						return;
					}
					if (result > Server.MaxMessageSize)
					{
						WriteLine("552 Message exceeds fixed maximum message size.");
						return;
					}
				}
				else if (Server.Extentions.Contains(SMTP_ServiceExtensions._8BITMIME) && array2[0].ToUpper() == "BODY")
				{
					if (array2.Length == 1)
					{
						WriteLine("501 Syntax error: BODY parameter value must be specified.");
						return;
					}
					if (array2[1].ToUpper() != "7BIT" && array2[1].ToUpper() != "8BITMIME" && array2[1].ToUpper() != "BINARYMIME")
					{
						WriteLine("501 Syntax error: BODY parameter value must be \"7BIT\",\"8BITMIME\" or \"BINARYMIME\".");
						return;
					}
					body = array2[1].ToUpper();
				}
				else if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) && array2[0].ToUpper() == "RET")
				{
					if (array2.Length == 1)
					{
						WriteLine("501 Syntax error: RET parameter value must be specified.");
						return;
					}
					if (array2[1].ToUpper() != "FULL")
					{
						ret = SMTP_DSN_Ret.FullMessage;
						continue;
					}
					if (!(array2[1].ToUpper() != "HDRS"))
					{
						WriteLine("501 Syntax error: RET parameter value must be \"FULL\" or \"HDRS\".");
						return;
					}
					ret = SMTP_DSN_Ret.Headers;
				}
				else if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) && array2[0].ToUpper() == "ENVID")
				{
					if (array2.Length == 1)
					{
						WriteLine("501 Syntax error: ENVID parameter value must be specified.");
						return;
					}
					envid = array2[1].ToUpper();
				}
				else if (!(array2[0].ToUpper() == "AUTH"))
				{
					WriteLine("555 Unsupported parameter: " + text2);
					return;
				}
			}
			SMTP_MailFrom sMTP_MailFrom = new SMTP_MailFrom(text, result, body, ret, envid);
			SMTP_Reply reply = new SMTP_Reply(250, "OK.");
			reply = OnMailFrom(sMTP_MailFrom, reply);
			if (reply.ReplyCode < 300)
			{
				m_pFrom = sMTP_MailFrom;
				m_Transactions++;
			}
			WriteLine(reply.ToString());
		}
		else
		{
			WriteLine("501 Syntax error, syntax: \"MAIL FROM:\" \"<\" address \">\" / \"<>\" [SP Mail-parameters] CRLF");
		}
	}

	private void RCPT(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
		}
		else if (string.IsNullOrEmpty(m_EhloHost))
		{
			WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
		}
		else if (m_pFrom == null)
		{
			WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
		}
		else if (m_pMessageStream != null)
		{
			WriteLine("503 Bad sequence of commands: BDAT command is pending.");
		}
		else if (cmdText.ToUpper().StartsWith("TO:"))
		{
			cmdText = cmdText.Substring(3).Trim();
			string text = "";
			SMTP_DSN_Notify sMTP_DSN_Notify = SMTP_DSN_Notify.NotSpecified;
			string orcpt = null;
			if (!cmdText.StartsWith("<") || cmdText.IndexOf('>') == -1)
			{
				WriteLine("501 Syntax error, syntax: \"RCPT TO:\" \"<\" address \">\" [SP Rcpt-parameters] CRLF");
				return;
			}
			text = cmdText.Substring(1, cmdText.IndexOf('>') - 1).Trim();
			cmdText = cmdText.Substring(cmdText.IndexOf('>') + 1).Trim();
			if (text == string.Empty)
			{
				WriteLine("501 Syntax error('address' value must be specified), syntax: \"RCPT TO:\" \"<\" address \">\" [SP Rcpt-parameters] CRLF");
				return;
			}
			string[] array = (string.IsNullOrEmpty(cmdText) ? new string[0] : cmdText.Split(' '));
			foreach (string text2 in array)
			{
				string[] array2 = text2.Split(new char[1] { '=' }, 2);
				if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) && array2[0].ToUpper() == "NOTIFY")
				{
					if (array2.Length == 1)
					{
						WriteLine("501 Syntax error: NOTIFY parameter value must be specified.");
						return;
					}
					string[] array3 = array2[1].ToUpper().Split(',');
					foreach (string text3 in array3)
					{
						if (text3.Trim().ToUpper() == "NEVER")
						{
							sMTP_DSN_Notify |= SMTP_DSN_Notify.Never;
							continue;
						}
						if (text3.Trim().ToUpper() == "SUCCESS")
						{
							sMTP_DSN_Notify |= SMTP_DSN_Notify.Success;
							continue;
						}
						if (text3.Trim().ToUpper() == "FAILURE")
						{
							sMTP_DSN_Notify |= SMTP_DSN_Notify.Failure;
							continue;
						}
						if (text3.Trim().ToUpper() == "DELAY")
						{
							sMTP_DSN_Notify |= SMTP_DSN_Notify.Delay;
							continue;
						}
						WriteLine("501 Syntax error: Not supported NOTIFY parameter value '" + text3 + "'.");
						return;
					}
				}
				else if (Server.Extentions.Contains(SMTP_ServiceExtensions.DSN) && array2[0].ToUpper() == "ORCPT")
				{
					if (array2.Length == 1)
					{
						WriteLine("501 Syntax error: ORCPT parameter value must be specified.");
						return;
					}
					orcpt = array2[1].ToUpper();
				}
				else
				{
					WriteLine("555 Unsupported parameter: " + text2);
				}
			}
			if (m_pTo.Count >= Server.MaxRecipients)
			{
				WriteLine("452 Too many recipients");
				return;
			}
			SMTP_RcptTo sMTP_RcptTo = new SMTP_RcptTo(text, sMTP_DSN_Notify, orcpt);
			SMTP_Reply reply = new SMTP_Reply(250, "OK.");
			reply = OnRcptTo(sMTP_RcptTo, reply);
			if (reply.ReplyCode < 300 && !m_pTo.ContainsKey(text.ToLower()))
			{
				m_pTo.Add(text.ToLower(), sMTP_RcptTo);
			}
			WriteLine(reply.ToString());
		}
		else
		{
			WriteLine("501 Syntax error, syntax: \"RCPT TO:\" \"<\" address \">\" [SP Rcpt-parameters] CRLF");
		}
	}

	private bool DATA(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
			return true;
		}
		if (string.IsNullOrEmpty(m_EhloHost))
		{
			WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
			return true;
		}
		if (m_pFrom == null)
		{
			WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
			return true;
		}
		if (m_pTo.Count == 0)
		{
			WriteLine("503 Bad sequence of commands: send 'RCPT TO:' first.");
			return true;
		}
		if (m_pMessageStream != null)
		{
			WriteLine("503 Bad sequence of commands: DATA and BDAT commands cannot be used in the same transaction.");
			return true;
		}
		DateTime startTime = DateTime.Now;
		m_pMessageStream = OnGetMessageStream();
		if (m_pMessageStream == null)
		{
			m_pMessageStream = new MemoryStreamEx(32000);
		}
		byte[] array = CreateReceivedHeader();
		m_pMessageStream.Write(array, 0, array.Length);
		WriteLine("354 Start mail input; end with <CRLF>.<CRLF>");
		SmartStream.ReadPeriodTerminatedAsyncOP readPeriodTermOP = new SmartStream.ReadPeriodTerminatedAsyncOP(m_pMessageStream, Server.MaxMessageSize, SizeExceededAction.JunkAndThrowException);
		readPeriodTermOP.CompletedAsync += delegate
		{
			DATA_End(startTime, readPeriodTermOP);
		};
		if (TcpStream.ReadPeriodTerminated(readPeriodTermOP, async: true))
		{
			DATA_End(startTime, readPeriodTermOP);
			return true;
		}
		return false;
	}

	private void DATA_End(DateTime startTime, SmartStream.ReadPeriodTerminatedAsyncOP op)
	{
		try
		{
			if (op.Error != null)
			{
				if (op.Error is LineSizeExceededException)
				{
					WriteLine("500 Line too long.");
				}
				else if (op.Error is DataSizeExceededException)
				{
					WriteLine("552 Too much mail data.");
				}
				else
				{
					OnError(op.Error);
				}
				OnMessageStoringCanceled();
			}
			else
			{
				SMTP_Reply reply = new SMTP_Reply(250, "DATA completed in " + (DateTime.Now - startTime).TotalSeconds.ToString("f2") + " seconds.");
				reply = OnMessageStoringCompleted(reply);
				WriteLine(reply.ToString());
			}
		}
		catch (Exception x)
		{
			OnError(x);
		}
		Reset();
		BeginReadCmd();
	}

	private bool BDAT(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
			return true;
		}
		if (string.IsNullOrEmpty(m_EhloHost))
		{
			WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
			return true;
		}
		if (m_pFrom == null)
		{
			WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
			return true;
		}
		if (m_pTo.Count == 0)
		{
			WriteLine("503 Bad sequence of commands: send 'RCPT TO:' first.");
			return true;
		}
		DateTime startTime = DateTime.Now;
		int chunkSize = 0;
		bool last = false;
		string[] array = cmdText.Split(' ');
		if (cmdText == string.Empty || array.Length > 2)
		{
			WriteLine("501 Syntax error, syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
			return true;
		}
		if (!int.TryParse(array[0], out chunkSize))
		{
			WriteLine("501 Syntax error(chunk-size must be integer), syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
			return true;
		}
		if (array.Length == 2)
		{
			if (array[1].ToUpperInvariant() != "LAST")
			{
				WriteLine("501 Syntax error, syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
				return true;
			}
			last = true;
		}
		if (m_pMessageStream == null)
		{
			m_pMessageStream = OnGetMessageStream();
			if (m_pMessageStream == null)
			{
				m_pMessageStream = new MemoryStreamEx(32000);
			}
			byte[] array2 = CreateReceivedHeader();
			m_pMessageStream.Write(array2, 0, array2.Length);
		}
		Stream storeStream = m_pMessageStream;
		if (m_BDatReadedCount + chunkSize > Server.MaxMessageSize)
		{
			storeStream = new JunkingStream();
		}
		TcpStream.BeginReadFixedCount(storeStream, chunkSize, delegate(IAsyncResult ar)
		{
			try
			{
				TcpStream.EndReadFixedCount(ar);
				m_BDatReadedCount += chunkSize;
				if (m_BDatReadedCount > Server.MaxMessageSize)
				{
					WriteLine("552 Too much mail data.");
					OnMessageStoringCanceled();
				}
				else
				{
					SMTP_Reply sMTP_Reply = new SMTP_Reply(250, chunkSize + " bytes received in " + (DateTime.Now - startTime).TotalSeconds.ToString("f2") + " seconds.");
					if (last)
					{
						sMTP_Reply = OnMessageStoringCompleted(sMTP_Reply);
					}
					WriteLine(sMTP_Reply.ToString());
				}
				if (last)
				{
					Reset();
				}
			}
			catch (Exception x)
			{
				OnError(x);
			}
			BeginReadCmd();
		}, null);
		return false;
	}

	private void RSET(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
			return;
		}
		if (m_pMessageStream != null)
		{
			OnMessageStoringCanceled();
		}
		Reset();
		WriteLine("250 OK.");
	}

	private void NOOP(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("503 bad sequence of commands: Session rejected.");
		}
		else
		{
			WriteLine("250 OK.");
		}
	}

	private void QUIT(string cmdText)
	{
		try
		{
			WriteLine("221 <" + Net_Utils.GetLocalHostName(base.LocalHostName) + "> Service closing transmission channel.");
		}
		catch
		{
		}
		Disconnect();
		Dispose();
	}

	private void Reset()
	{
		if (!base.IsDisposed)
		{
			m_pFrom = null;
			m_pTo.Clear();
			m_pMessageStream = null;
			m_BDatReadedCount = 0;
		}
	}

	private byte[] CreateReceivedHeader()
	{
		Mail_h_Received mail_h_Received = new Mail_h_Received(EhloHost, Net_Utils.GetLocalHostName(base.LocalHostName), DateTime.Now);
		mail_h_Received.From_TcpInfo = new Mail_t_TcpInfo(RemoteEndPoint.Address, null);
		mail_h_Received.Via = "TCP";
		if (!base.IsAuthenticated && !IsSecureConnection)
		{
			mail_h_Received.With = "ESMTP";
		}
		else if (base.IsAuthenticated && !IsSecureConnection)
		{
			mail_h_Received.With = "ESMTPA";
		}
		else if (!base.IsAuthenticated && IsSecureConnection)
		{
			mail_h_Received.With = "ESMTPS";
		}
		else if (base.IsAuthenticated && IsSecureConnection)
		{
			mail_h_Received.With = "ESMTPSA";
		}
		return Encoding.UTF8.GetBytes(mail_h_Received.ToString());
	}

	private void WriteLine(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		int num = TcpStream.WriteLine(line);
		if (Server.Logger != null)
		{
			Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, num, line, LocalEndPoint, RemoteEndPoint);
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
		try
		{
			if (Server.Logger != null)
			{
				Server.Logger.AddText(IsConnected ? ID : "", IsConnected ? AuthenticatedUserIdentity : null, text, IsConnected ? LocalEndPoint : null, IsConnected ? RemoteEndPoint : null);
			}
		}
		catch
		{
		}
	}

	public void LogAddException(string text, Exception x)
	{
		try
		{
			if (Server.Logger != null)
			{
				Server.Logger.AddException(IsConnected ? ID : "", IsConnected ? AuthenticatedUserIdentity : null, text, IsConnected ? LocalEndPoint : null, IsConnected ? RemoteEndPoint : null, x);
			}
		}
		catch
		{
		}
	}

	private SMTP_Reply OnStarted(SMTP_Reply reply)
	{
		if (this.Started != null)
		{
			SMTP_e_Started sMTP_e_Started = new SMTP_e_Started(this, reply);
			this.Started(this, sMTP_e_Started);
			return sMTP_e_Started.Reply;
		}
		return reply;
	}

	private SMTP_Reply OnEhlo(string domain, SMTP_Reply reply)
	{
		if (this.Ehlo != null)
		{
			SMTP_e_Ehlo sMTP_e_Ehlo = new SMTP_e_Ehlo(this, domain, reply);
			this.Ehlo(this, sMTP_e_Ehlo);
			return sMTP_e_Ehlo.Reply;
		}
		return reply;
	}

	private SMTP_Reply OnMailFrom(SMTP_MailFrom from, SMTP_Reply reply)
	{
		if (this.MailFrom != null)
		{
			SMTP_e_MailFrom sMTP_e_MailFrom = new SMTP_e_MailFrom(this, from, reply);
			this.MailFrom(this, sMTP_e_MailFrom);
			return sMTP_e_MailFrom.Reply;
		}
		return reply;
	}

	private SMTP_Reply OnRcptTo(SMTP_RcptTo to, SMTP_Reply reply)
	{
		if (this.RcptTo != null)
		{
			SMTP_e_RcptTo sMTP_e_RcptTo = new SMTP_e_RcptTo(this, to, reply);
			this.RcptTo(this, sMTP_e_RcptTo);
			return sMTP_e_RcptTo.Reply;
		}
		return reply;
	}

	private Stream OnGetMessageStream()
	{
		if (this.GetMessageStream != null)
		{
			SMTP_e_Message sMTP_e_Message = new SMTP_e_Message(this);
			this.GetMessageStream(this, sMTP_e_Message);
			return sMTP_e_Message.Stream;
		}
		return null;
	}

	private void OnMessageStoringCanceled()
	{
		if (this.MessageStoringCanceled != null)
		{
			this.MessageStoringCanceled(this, new EventArgs());
		}
	}

	private SMTP_Reply OnMessageStoringCompleted(SMTP_Reply reply)
	{
		if (this.MessageStoringCompleted != null)
		{
			SMTP_e_MessageStored sMTP_e_MessageStored = new SMTP_e_MessageStored(this, m_pMessageStream, reply);
			this.MessageStoringCompleted(this, sMTP_e_MessageStored);
			return sMTP_e_MessageStored.Reply;
		}
		return reply;
	}
}
