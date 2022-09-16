using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.POP3.Server;

public class POP3_Session : TCP_ServerSession
{
	private Dictionary<string, AUTH_SASL_ServerMechanism> m_pAuthentications;

	private bool m_SessionRejected;

	private int m_BadCommands;

	private string m_UserName;

	private GenericIdentity m_pUser;

	private KeyValueCollection<string, POP3_ServerMessage> m_pMessages;

	public new POP3_Server Server
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (POP3_Server)base.Server;
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

	public event EventHandler<POP3_e_Started> Started;

	public event EventHandler<POP3_e_Authenticate> Authenticate;

	public event EventHandler<POP3_e_GetMessagesInfo> GetMessagesInfo;

	public event EventHandler<POP3_e_GetTopOfMessage> GetTopOfMessage;

	public event EventHandler<POP3_e_GetMessageStream> GetMessageStream;

	public event EventHandler<POP3_e_DeleteMessage> DeleteMessage;

	public event EventHandler Reset;

	public POP3_Session()
	{
		m_pAuthentications = new Dictionary<string, AUTH_SASL_ServerMechanism>(StringComparer.CurrentCultureIgnoreCase);
		m_pMessages = new KeyValueCollection<string, POP3_ServerMessage>();
	}

	protected override void Start()
	{
		base.Start();
		try
		{
			string text = null;
			text = ((!string.IsNullOrEmpty(Server.GreetingText)) ? ("+OK " + Server.GreetingText) : ("+OK [" + Net_Utils.GetLocalHostName(base.LocalHostName) + "] POP3 Service Ready."));
			POP3_e_Started pOP3_e_Started = OnStarted(text);
			if (!string.IsNullOrEmpty(pOP3_e_Started.Response))
			{
				WriteLine(text.ToString());
			}
			if (string.IsNullOrEmpty(pOP3_e_Started.Response) || pOP3_e_Started.Response.ToUpper().StartsWith("-ERR"))
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
				WriteLine("-ERR Internal server error.");
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
			WriteLine("-ERR Idle timeout, closing connection.");
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
			string[] array = Encoding.UTF8.GetString(op.Buffer, 0, op.LineBytesInBuffer).Split(new char[1] { ' ' }, 2);
			string text = array[0].ToUpperInvariant();
			string cmdText = ((array.Length == 2) ? array[1] : "");
			if (Server.Logger != null)
			{
				if (text == "PASS")
				{
					Server.Logger.AddRead(ID, AuthenticatedUserIdentity, op.BytesInBuffer, "PASS <***REMOVED***>", LocalEndPoint, RemoteEndPoint);
				}
				else
				{
					Server.Logger.AddRead(ID, AuthenticatedUserIdentity, op.BytesInBuffer, op.LineUtf8, LocalEndPoint, RemoteEndPoint);
				}
			}
			switch (text)
			{
			case "STLS":
				STLS(cmdText);
				return result;
			case "USER":
				USER(cmdText);
				return result;
			case "PASS":
				PASS(cmdText);
				return result;
			case "AUTH":
				AUTH(cmdText);
				return result;
			case "STAT":
				STAT(cmdText);
				return result;
			case "LIST":
				LIST(cmdText);
				return result;
			case "UIDL":
				UIDL(cmdText);
				return result;
			case "TOP":
				TOP(cmdText);
				return result;
			case "RETR":
				RETR(cmdText);
				return result;
			case "DELE":
				DELE(cmdText);
				return result;
			case "NOOP":
				NOOP(cmdText);
				return result;
			case "RSET":
				RSET(cmdText);
				return result;
			case "CAPA":
				CAPA(cmdText);
				return result;
			case "QUIT":
				QUIT(cmdText);
				return result;
			default:
				m_BadCommands++;
				if (Server.MaxBadCommands != 0 && m_BadCommands > Server.MaxBadCommands)
				{
					WriteLine("-ERR Too many bad commands, closing transmission channel.");
					Disconnect();
					return false;
				}
				WriteLine("-ERR Error: command '" + text + "' not recognized.");
				return result;
			}
		}
		catch (Exception x)
		{
			OnError(x);
			return result;
		}
	}

	private void STLS(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.IsAuthenticated)
		{
			TcpStream.WriteLine("-ERR This ommand is only valid in AUTHORIZATION state (RFC 2595 4).");
			return;
		}
		if (IsSecureConnection)
		{
			WriteLine("-ERR Bad sequence of commands: Connection is already secure.");
			return;
		}
		if (base.Certificate == null)
		{
			WriteLine("-ERR TLS not available: Server has no SSL certificate.");
			return;
		}
		WriteLine("+OK Ready to start TLS.");
		try
		{
			SwitchToSecure();
			LogAddText("TLS negotiation completed successfully.");
		}
		catch (Exception ex)
		{
			LogAddText("TLS negotiation failed: " + ex.Message + ".");
			Disconnect();
		}
	}

	private void USER(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.IsAuthenticated)
		{
			TcpStream.WriteLine("-ERR Re-authentication error.");
			return;
		}
		if (m_UserName != null)
		{
			TcpStream.WriteLine("-ERR User name already specified.");
			return;
		}
		m_UserName = cmdText;
		TcpStream.WriteLine("+OK User name OK.");
	}

	private void PASS(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
		}
		else if (base.IsAuthenticated)
		{
			TcpStream.WriteLine("-ERR Re-authentication error.");
		}
		else if (m_UserName == null)
		{
			TcpStream.WriteLine("-ERR Specify user name first.");
		}
		else if (string.IsNullOrEmpty(cmdText))
		{
			TcpStream.WriteLine("-ERR Error in arguments.");
		}
		else if (OnAuthenticate(m_UserName, cmdText).IsAuthenticated)
		{
			m_pUser = new GenericIdentity(m_UserName, "POP3-USER/PASS");
			POP3_e_GetMessagesInfo pOP3_e_GetMessagesInfo = OnGetMessagesInfo();
			int num = 1;
			foreach (POP3_ServerMessage message in pOP3_e_GetMessagesInfo.Messages)
			{
				message.SequenceNumber = num++;
				m_pMessages.Add(message.UID, message);
			}
			TcpStream.WriteLine("+OK Authenticated successfully.");
		}
		else
		{
			TcpStream.WriteLine("-ERR Authentication failed.");
		}
	}

	private void AUTH(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.IsAuthenticated)
		{
			TcpStream.WriteLine("-ERR Re-authentication error.");
			return;
		}
		if (string.IsNullOrEmpty(cmdText))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("+OK\r\n");
			foreach (AUTH_SASL_ServerMechanism value in m_pAuthentications.Values)
			{
				stringBuilder.Append(value.Name + "\r\n");
			}
			stringBuilder.Append(".\r\n");
			WriteLine(stringBuilder.ToString());
			return;
		}
		if (!Authentications.ContainsKey(cmdText))
		{
			WriteLine("-ERR Not supported authentication mechanism.");
			return;
		}
		byte[] array = new byte[0];
		AUTH_SASL_ServerMechanism aUTH_SASL_ServerMechanism = Authentications[cmdText];
		aUTH_SASL_ServerMechanism.Reset();
		while (true)
		{
			byte[] array2 = aUTH_SASL_ServerMechanism.Continue(array);
			if (aUTH_SASL_ServerMechanism.IsCompleted)
			{
				if (aUTH_SASL_ServerMechanism.IsAuthenticated)
				{
					m_pUser = new GenericIdentity(aUTH_SASL_ServerMechanism.UserName, "SASL-" + aUTH_SASL_ServerMechanism.Name);
					POP3_e_GetMessagesInfo pOP3_e_GetMessagesInfo = OnGetMessagesInfo();
					int num = 1;
					foreach (POP3_ServerMessage message in pOP3_e_GetMessagesInfo.Messages)
					{
						message.SequenceNumber = num++;
						m_pMessages.Add(message.UID, message);
					}
					WriteLine("+OK Authentication succeeded.");
				}
				else
				{
					WriteLine("-ERR Authentication credentials invalid.");
				}
				return;
			}
			if (array2.Length == 0)
			{
				WriteLine("+ ");
			}
			else
			{
				WriteLine("+ " + Convert.ToBase64String(array2));
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
				array = Convert.FromBase64String(readLineAsyncOP.LineUtf8);
			}
			catch
			{
				WriteLine("-ERR Invalid client response '" + array?.ToString() + "'.");
				return;
			}
		}
		WriteLine("-ERR Authentication canceled.");
	}

	private void STAT(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (POP3_ServerMessage pMessage in m_pMessages)
		{
			if (!pMessage.IsMarkedForDeletion)
			{
				num++;
				num2 += pMessage.Size;
			}
		}
		WriteLine("+OK " + num + " " + num2);
	}

	private void LIST(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		string[] array = cmdText.Split(' ');
		if (string.IsNullOrEmpty(cmdText))
		{
			int num = 0;
			int num2 = 0;
			foreach (POP3_ServerMessage pMessage in m_pMessages)
			{
				if (!pMessage.IsMarkedForDeletion)
				{
					num++;
					num2 += pMessage.Size;
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("+OK " + num + " messages (" + num2 + " bytes).\r\n");
			foreach (POP3_ServerMessage pMessage2 in m_pMessages)
			{
				stringBuilder.Append(pMessage2.SequenceNumber + " " + pMessage2.Size + "\r\n");
			}
			stringBuilder.Append(".");
			WriteLine(stringBuilder.ToString());
			return;
		}
		if (array.Length > 1 || !Net_Utils.IsInteger(array[0]))
		{
			WriteLine("-ERR Error in arguments.");
			return;
		}
		POP3_ServerMessage value = null;
		if (m_pMessages.TryGetValueAt(Convert.ToInt32(array[0]) - 1, out value))
		{
			if (value.IsMarkedForDeletion)
			{
				WriteLine("-ERR Invalid operation: Message marked for deletion.");
			}
			else
			{
				WriteLine("+OK " + value.SequenceNumber + " " + value.Size);
			}
		}
		else
		{
			WriteLine("-ERR no such message or message marked for deletion.");
		}
	}

	private void UIDL(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		string[] array = cmdText.Split(' ');
		if (string.IsNullOrEmpty(cmdText))
		{
			int num = 0;
			int num2 = 0;
			foreach (POP3_ServerMessage pMessage in m_pMessages)
			{
				if (!pMessage.IsMarkedForDeletion)
				{
					num++;
					num2 += pMessage.Size;
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("+OK " + num + " messages (" + num2 + " bytes).\r\n");
			foreach (POP3_ServerMessage pMessage2 in m_pMessages)
			{
				stringBuilder.Append(pMessage2.SequenceNumber + " " + pMessage2.UID + "\r\n");
			}
			stringBuilder.Append(".");
			WriteLine(stringBuilder.ToString());
			return;
		}
		if (array.Length > 1)
		{
			WriteLine("-ERR Error in arguments.");
			return;
		}
		POP3_ServerMessage value = null;
		if (m_pMessages.TryGetValueAt(Convert.ToInt32(array[0]) - 1, out value))
		{
			if (value.IsMarkedForDeletion)
			{
				WriteLine("-ERR Invalid operation: Message marked for deletion.");
			}
			else
			{
				WriteLine("+OK " + value.SequenceNumber + " " + value.UID);
			}
		}
		else
		{
			WriteLine("-ERR no such message or message marked for deletion.");
		}
	}

	private void TOP(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		string[] array = cmdText.Split(' ');
		if (array.Length != 2 || !Net_Utils.IsInteger(array[0]) || !Net_Utils.IsInteger(array[1]))
		{
			WriteLine("-ERR Error in arguments.");
			return;
		}
		POP3_ServerMessage value = null;
		if (m_pMessages.TryGetValueAt(Convert.ToInt32(array[0]) - 1, out value))
		{
			if (value.IsMarkedForDeletion)
			{
				WriteLine("-ERR Invalid operation: Message marked for deletion.");
				return;
			}
			POP3_e_GetTopOfMessage pOP3_e_GetTopOfMessage = OnGetTopOfMessage(value, Convert.ToInt32(array[1]));
			if (pOP3_e_GetTopOfMessage.Data == null)
			{
				WriteLine("-ERR no such message.");
				return;
			}
			WriteLine("+OK Start sending top of message.");
			long size = TcpStream.WritePeriodTerminated(new MemoryStream(pOP3_e_GetTopOfMessage.Data));
			if (Server.Logger != null)
			{
				Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, size, "Wrote top of message(" + size + " bytes).", LocalEndPoint, RemoteEndPoint);
			}
		}
		else
		{
			WriteLine("-ERR no such message.");
		}
	}

	private void RETR(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		string[] array = cmdText.Split(' ');
		if (array.Length != 1 || !Net_Utils.IsInteger(array[0]))
		{
			WriteLine("-ERR Error in arguments.");
			return;
		}
		POP3_ServerMessage value = null;
		if (m_pMessages.TryGetValueAt(Convert.ToInt32(array[0]) - 1, out value))
		{
			if (value.IsMarkedForDeletion)
			{
				WriteLine("-ERR Invalid operation: Message marked for deletion.");
				return;
			}
			POP3_e_GetMessageStream pOP3_e_GetMessageStream = OnGetMessageStream(value);
			if (pOP3_e_GetMessageStream.MessageStream != null)
			{
				try
				{
					WriteLine("+OK Start sending message.");
					long size = TcpStream.WritePeriodTerminated(pOP3_e_GetMessageStream.MessageStream);
					if (Server.Logger != null)
					{
						Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, size, "Wrote message(" + size + " bytes).", LocalEndPoint, RemoteEndPoint);
					}
					return;
				}
				finally
				{
					if (pOP3_e_GetMessageStream.CloseMessageStream)
					{
						pOP3_e_GetMessageStream.MessageStream.Dispose();
					}
				}
			}
			WriteLine("-ERR no such message.");
		}
		else
		{
			WriteLine("-ERR no such message.");
		}
	}

	private void DELE(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		string[] array = cmdText.Split(' ');
		if (array.Length != 1 || !Net_Utils.IsInteger(array[0]))
		{
			WriteLine("-ERR Error in arguments.");
			return;
		}
		POP3_ServerMessage value = null;
		if (m_pMessages.TryGetValueAt(Convert.ToInt32(array[0]) - 1, out value))
		{
			if (!value.IsMarkedForDeletion)
			{
				value.SetIsMarkedForDeletion(value: true);
				WriteLine("+OK Message marked for deletion.");
			}
			else
			{
				WriteLine("-ERR Message already marked for deletion.");
			}
		}
		else
		{
			WriteLine("-ERR no such message.");
		}
	}

	private void NOOP(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
		}
		else if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
		}
		else
		{
			WriteLine("+OK");
		}
	}

	private void RSET(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("-ERR Authentication required.");
			return;
		}
		foreach (POP3_ServerMessage pMessage in m_pMessages)
		{
			pMessage.SetIsMarkedForDeletion(value: false);
		}
		WriteLine("+OK");
		OnReset();
	}

	private void CAPA(string cmdText)
	{
		if (m_SessionRejected)
		{
			WriteLine("-ERR Bad sequence of commands: Session rejected.");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("+OK Capability list follows\r\n");
		stringBuilder.Append("PIPELINING\r\n");
		stringBuilder.Append("UIDL\r\n");
		stringBuilder.Append("TOP\r\n");
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (AUTH_SASL_ServerMechanism value in Authentications.Values)
		{
			if (!value.RequireSSL || (value.RequireSSL && IsSecureConnection))
			{
				stringBuilder2.Append(value.Name + " ");
			}
		}
		if (stringBuilder2.Length > 0)
		{
			stringBuilder.Append("SASL " + stringBuilder2.ToString().Trim() + "\r\n");
		}
		if (!IsSecureConnection && base.Certificate != null)
		{
			stringBuilder.Append("STLS\r\n");
		}
		stringBuilder.Append(".");
		WriteLine(stringBuilder.ToString());
	}

	private void QUIT(string cmdText)
	{
		try
		{
			if (base.IsAuthenticated)
			{
				foreach (POP3_ServerMessage pMessage in m_pMessages)
				{
					if (pMessage.IsMarkedForDeletion)
					{
						OnDeleteMessage(pMessage);
					}
				}
			}
			WriteLine("+OK <" + Net_Utils.GetLocalHostName(base.LocalHostName) + "> Service closing transmission channel.");
		}
		catch
		{
		}
		Disconnect();
		Dispose();
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

	public void LogAddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (Server.Logger != null)
		{
			Server.Logger.AddText(ID, text);
		}
	}

	private POP3_e_Started OnStarted(string reply)
	{
		POP3_e_Started pOP3_e_Started = new POP3_e_Started(reply);
		if (this.Started != null)
		{
			this.Started(this, pOP3_e_Started);
		}
		return pOP3_e_Started;
	}

	private POP3_e_Authenticate OnAuthenticate(string user, string password)
	{
		POP3_e_Authenticate pOP3_e_Authenticate = new POP3_e_Authenticate(user, password);
		if (this.Authenticate != null)
		{
			this.Authenticate(this, pOP3_e_Authenticate);
		}
		return pOP3_e_Authenticate;
	}

	private POP3_e_GetMessagesInfo OnGetMessagesInfo()
	{
		POP3_e_GetMessagesInfo pOP3_e_GetMessagesInfo = new POP3_e_GetMessagesInfo();
		if (this.GetMessagesInfo != null)
		{
			this.GetMessagesInfo(this, pOP3_e_GetMessagesInfo);
		}
		return pOP3_e_GetMessagesInfo;
	}

	private POP3_e_GetTopOfMessage OnGetTopOfMessage(POP3_ServerMessage message, int lines)
	{
		POP3_e_GetTopOfMessage pOP3_e_GetTopOfMessage = new POP3_e_GetTopOfMessage(message, lines);
		if (this.GetTopOfMessage != null)
		{
			this.GetTopOfMessage(this, pOP3_e_GetTopOfMessage);
		}
		return pOP3_e_GetTopOfMessage;
	}

	private POP3_e_GetMessageStream OnGetMessageStream(POP3_ServerMessage message)
	{
		POP3_e_GetMessageStream pOP3_e_GetMessageStream = new POP3_e_GetMessageStream(message);
		if (this.GetMessageStream != null)
		{
			this.GetMessageStream(this, pOP3_e_GetMessageStream);
		}
		return pOP3_e_GetMessageStream;
	}

	private void OnDeleteMessage(POP3_ServerMessage message)
	{
		if (this.DeleteMessage != null)
		{
			this.DeleteMessage(this, new POP3_e_DeleteMessage(message));
		}
	}

	private void OnReset()
	{
		if (this.Reset != null)
		{
			this.Reset(this, new EventArgs());
		}
	}
}
