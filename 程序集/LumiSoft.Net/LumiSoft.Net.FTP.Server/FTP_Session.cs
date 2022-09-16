using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.FTP.Server;

public class FTP_Session : TCP_ServerSession
{
	private class DataConnection
	{
		private bool m_IsDisposed;

		private FTP_Session m_pSession;

		private Stream m_pStream;

		private bool m_Read_Write;

		private Socket m_pSocket;

		public DataConnection(FTP_Session session, Stream stream, bool read_write)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pSession = session;
			m_pStream = stream;
			m_Read_Write = read_write;
		}

		public void Dispose()
		{
			if (!m_IsDisposed)
			{
				m_IsDisposed = true;
				if (m_pSession.m_pPassiveSocket != null)
				{
					m_pSession.m_pPassiveSocket.Close();
					m_pSession.m_pPassiveSocket = null;
				}
				m_pSession.m_PassiveMode = false;
				m_pSession = null;
				if (m_pStream != null)
				{
					m_pStream.Dispose();
					m_pStream = null;
				}
				if (m_pSocket != null)
				{
					m_pSocket.Close();
					m_pSocket = null;
				}
			}
		}

		public void Start()
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_pSession.PassiveMode)
			{
				WriteLine("150 Waiting data connection on port '" + ((IPEndPoint)m_pSession.m_pPassiveSocket.LocalEndPoint).Port + "'.");
				TimerEx timer = new TimerEx(10000.0, autoReset: false);
				timer.Elapsed += delegate
				{
					WriteLine("550 Data connection wait timeout.");
					Dispose();
				};
				timer.Enabled = true;
				m_pSession.m_pPassiveSocket.BeginAccept(delegate(IAsyncResult ar)
				{
					try
					{
						timer.Dispose();
						m_pSocket = m_pSession.m_pPassiveSocket.EndAccept(ar);
						m_pSession.LogAddText("Data connection opened.");
						StartDataTransfer();
					}
					catch
					{
						WriteLine("425 Opening data connection failed.");
						Dispose();
					}
				}, null);
				return;
			}
			WriteLine("150 Opening data connection to '" + m_pSession.m_pDataConEndPoint.ToString() + "'.");
			m_pSocket = new Socket(m_pSession.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			m_pSocket.BeginConnect(m_pSession.m_pDataConEndPoint, delegate(IAsyncResult ar)
			{
				try
				{
					m_pSocket.EndConnect(ar);
					m_pSession.LogAddText("Data connection opened.");
					StartDataTransfer();
				}
				catch
				{
					WriteLine("425 Opening data connection to '" + m_pSession.m_pDataConEndPoint.ToString() + "' failed.");
					Dispose();
				}
			}, null);
		}

		public void Abort()
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			WriteLine("426 Data connection closed; transfer aborted.");
			Dispose();
		}

		private void WriteLine(string line)
		{
			if (line == null)
			{
				throw new ArgumentNullException("line");
			}
			if (!m_IsDisposed)
			{
				m_pSession.WriteLine(line);
			}
		}

		private void StartDataTransfer()
		{
			try
			{
				if (m_Read_Write)
				{
					Net_Utils.StreamCopy(new NetworkStream(m_pSocket, ownsSocket: false), m_pStream, 64000);
				}
				else
				{
					Net_Utils.StreamCopy(m_pStream, new NetworkStream(m_pSocket, ownsSocket: false), 64000);
				}
				m_pSocket.Shutdown(SocketShutdown.Both);
				m_pSession.WriteLine("226 Transfer Complete.");
			}
			catch
			{
			}
			Dispose();
		}
	}

	private Dictionary<string, AUTH_SASL_ServerMechanism> m_pAuthentications;

	private bool m_SessionRejected;

	private int m_BadCommands;

	private string m_UserName;

	private GenericIdentity m_pUser;

	private string m_CurrentDir = "/";

	private string m_RenameFrom = "";

	private DataConnection m_pDataConnection;

	private bool m_PassiveMode;

	private Socket m_pPassiveSocket;

	private IPEndPoint m_pDataConEndPoint;

	public new FTP_Server Server
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return (FTP_Server)base.Server;
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

	public string CurrentDir
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_CurrentDir;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_CurrentDir = value;
		}
	}

	public bool PassiveMode => m_PassiveMode;

	public event EventHandler<FTP_e_Started> Started;

	public event EventHandler<FTP_e_Authenticate> Authenticate;

	public event EventHandler<FTP_e_GetFile> GetFile;

	public event EventHandler<FTP_e_Stor> Stor;

	public event EventHandler<FTP_e_GetFileSize> GetFileSize;

	public event EventHandler<FTP_e_Dele> Dele;

	public event EventHandler<FTP_e_Appe> Appe;

	public event EventHandler<FTP_e_Cwd> Cwd;

	public event EventHandler<FTP_e_Cdup> Cdup;

	public event EventHandler<FTP_e_Rmd> Rmd;

	public event EventHandler<FTP_e_Mkd> Mkd;

	public event EventHandler<FTP_e_GetDirListing> GetDirListing;

	public event EventHandler<FTP_e_Rnto> Rnto;

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			if (m_pDataConnection != null)
			{
				m_pDataConnection.Dispose();
				m_pDataConnection = null;
			}
			if (m_pPassiveSocket != null)
			{
				m_pPassiveSocket.Close();
				m_pPassiveSocket = null;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		try
		{
			string text = null;
			text = ((!string.IsNullOrEmpty(Server.GreetingText)) ? ("220 " + Server.GreetingText) : ("220 [" + Net_Utils.GetLocalHostName(base.LocalHostName) + "] FTP Service Ready."));
			FTP_e_Started fTP_e_Started = OnStarted(text);
			if (!string.IsNullOrEmpty(fTP_e_Started.Response))
			{
				WriteLine(text.ToString());
			}
			if (string.IsNullOrEmpty(fTP_e_Started.Response) || fTP_e_Started.Response.ToUpper().StartsWith("500"))
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
			WriteLine("500 Idle timeout, closing connection.");
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
			string argsText = ((array.Length == 2) ? array[1] : "");
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
			if (text == "AUTH")
			{
				AUTH(argsText);
				return result;
			}
			if (text == "USER")
			{
				USER(argsText);
				return result;
			}
			if (text == "PASS")
			{
				PASS(argsText);
				return result;
			}
			if (text == "CWD" || text == "XCWD")
			{
				CWD(argsText);
				return result;
			}
			if (text == "CDUP" || text == "XCUP")
			{
				CDUP(argsText);
				return result;
			}
			if (text == "PWD" || text == "XPWD")
			{
				PWD(argsText);
				return result;
			}
			if (text == "ABOR")
			{
				ABOR(argsText);
				return result;
			}
			if (text == "RETR")
			{
				RETR(argsText);
				return result;
			}
			if (text == "STOR")
			{
				STOR(argsText);
				return result;
			}
			if (text == "DELE")
			{
				DELE(argsText);
				return result;
			}
			switch (text)
			{
			case "APPE":
				APPE(argsText);
				return result;
			case "SIZE":
				SIZE(argsText);
				return result;
			case "RNFR":
				RNFR(argsText);
				return result;
			case "DELE":
				DELE(argsText);
				return result;
			case "RNTO":
				RNTO(argsText);
				return result;
			case "RMD":
			case "XRMD":
				RMD(argsText);
				return result;
			case "MKD":
			case "XMKD":
				MKD(argsText);
				return result;
			case "LIST":
				LIST(argsText);
				return result;
			case "NLST":
				NLST(argsText);
				return result;
			case "TYPE":
				TYPE(argsText);
				return result;
			case "PORT":
				PORT(argsText);
				return result;
			case "PASV":
				PASV(argsText);
				return result;
			case "SYST":
				SYST(argsText);
				return result;
			case "NOOP":
				NOOP(argsText);
				return result;
			case "QUIT":
				QUIT(argsText);
				result = false;
				return result;
			case "FEAT":
				FEAT(argsText);
				return result;
			case "OPTS":
				OPTS(argsText);
				return result;
			default:
				m_BadCommands++;
				if (Server.MaxBadCommands != 0 && m_BadCommands > Server.MaxBadCommands)
				{
					WriteLine("500 Too many bad commands, closing transmission channel.");
					Disconnect();
					return false;
				}
				WriteLine("500 Error: command '" + text + "' not recognized.");
				return result;
			}
		}
		catch (Exception x)
		{
			OnError(x);
			return result;
		}
	}

	private void AUTH(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.Certificate == null)
		{
			WriteLine("500 TLS not available: Server has no SSL certificate.");
			return;
		}
		if (string.Equals("TLS", argsText))
		{
			WriteLine("234 Ready to start TLS.");
			try
			{
				SwitchToSecure();
				LogAddText("TLS negotiation completed successfully.");
				return;
			}
			catch (Exception ex)
			{
				LogAddText("TLS negotiation failed: " + ex.Message + ".");
				Disconnect();
				return;
			}
		}
		WriteLine("500 Error in arguments.");
	}

	private void USER(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.IsAuthenticated)
		{
			WriteLine("500 You are already authenticated");
			return;
		}
		if (!string.IsNullOrEmpty(m_UserName))
		{
			WriteLine("500 username is already specified, please specify password");
			return;
		}
		string[] array = argsText.Split(' ');
		if (argsText.Length > 0 && array.Length == 1)
		{
			string text = array[0];
			WriteLine("331 Password required or user:'" + text + "'");
			m_UserName = text;
		}
		else
		{
			WriteLine("500 Syntax error. Syntax:{USER username}");
		}
	}

	private void PASS(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (base.IsAuthenticated)
		{
			WriteLine("500 You are already authenticated");
			return;
		}
		if (m_UserName.Length == 0)
		{
			WriteLine("503 please specify username first");
			return;
		}
		string[] array = argsText.Split(' ');
		if (array.Length == 1)
		{
			string password = array[0];
			if (OnAuthenticate(m_UserName, password).IsAuthenticated)
			{
				WriteLine("230 Password ok");
				m_pUser = new GenericIdentity(m_UserName, "FTP-USER/PASS");
			}
			else
			{
				WriteLine("530 UserName or Password is incorrect");
				m_UserName = "";
			}
		}
		else
		{
			WriteLine("500 Syntax error. Syntax:{PASS userName}");
		}
	}

	private void CWD(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		FTP_e_Cwd fTP_e_Cwd = new FTP_e_Cwd(argsText);
		OnCwd(fTP_e_Cwd);
		if (fTP_e_Cwd.Response == null)
		{
			WriteLine("500 Internal server error: FTP server didn't provide response for CWD command.");
			return;
		}
		FTP_t_ReplyLine[] response = fTP_e_Cwd.Response;
		foreach (FTP_t_ReplyLine fTP_t_ReplyLine in response)
		{
			WriteLine(fTP_t_ReplyLine.ToString());
		}
	}

	private void CDUP(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (!string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Error in arguments.");
		}
		FTP_e_Cdup fTP_e_Cdup = new FTP_e_Cdup();
		OnCdup(fTP_e_Cdup);
		if (fTP_e_Cdup.Response == null)
		{
			WriteLine("500 Internal server error: FTP server didn't provide response for CDUP command.");
			return;
		}
		FTP_t_ReplyLine[] response = fTP_e_Cdup.Response;
		foreach (FTP_t_ReplyLine fTP_t_ReplyLine in response)
		{
			WriteLine(fTP_t_ReplyLine.ToString());
		}
	}

	private void PWD(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
		}
		else if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
		}
		else
		{
			WriteLine("257 \"" + m_CurrentDir + "\" is current directory.");
		}
	}

	private void ABOR(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (!string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Error in arguments. !");
			return;
		}
		if (m_pDataConnection != null)
		{
			m_pDataConnection.Abort();
		}
		WriteLine("226 ABOR command successful.");
	}

	private void RETR(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid file name. !");
			return;
		}
		FTP_e_GetFile fTP_e_GetFile = new FTP_e_GetFile(argsText);
		OnGetFile(fTP_e_GetFile);
		if (fTP_e_GetFile.Error != null)
		{
			FTP_t_ReplyLine[] error = fTP_e_GetFile.Error;
			foreach (FTP_t_ReplyLine fTP_t_ReplyLine in error)
			{
				WriteLine(fTP_t_ReplyLine.ToString());
			}
		}
		else if (fTP_e_GetFile.FileStream == null)
		{
			WriteLine("500 Internal server error: File stream not provided by server.");
		}
		else
		{
			m_pDataConnection = new DataConnection(this, fTP_e_GetFile.FileStream, read_write: false);
			m_pDataConnection.Start();
		}
	}

	private void STOR(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid file name.");
		}
		FTP_e_Stor fTP_e_Stor = new FTP_e_Stor(argsText);
		OnStor(fTP_e_Stor);
		if (fTP_e_Stor.Error != null)
		{
			FTP_t_ReplyLine[] error = fTP_e_Stor.Error;
			foreach (FTP_t_ReplyLine fTP_t_ReplyLine in error)
			{
				WriteLine(fTP_t_ReplyLine.ToString());
			}
		}
		else if (fTP_e_Stor.FileStream == null)
		{
			WriteLine("500 Internal server error: File stream not provided by server.");
		}
		else
		{
			m_pDataConnection = new DataConnection(this, fTP_e_Stor.FileStream, read_write: true);
			m_pDataConnection.Start();
		}
	}

	private void DELE(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid file name.");
		}
		FTP_e_Dele fTP_e_Dele = new FTP_e_Dele(argsText);
		OnDele(fTP_e_Dele);
		if (fTP_e_Dele.Response == null)
		{
			WriteLine("500 Internal server error: FTP server didn't provide response for DELE command.");
			return;
		}
		FTP_t_ReplyLine[] response = fTP_e_Dele.Response;
		foreach (FTP_t_ReplyLine fTP_t_ReplyLine in response)
		{
			WriteLine(fTP_t_ReplyLine.ToString());
		}
	}

	private void APPE(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid file name.");
		}
		FTP_e_Appe fTP_e_Appe = new FTP_e_Appe(argsText);
		OnAppe(fTP_e_Appe);
		if (fTP_e_Appe.Error != null)
		{
			FTP_t_ReplyLine[] error = fTP_e_Appe.Error;
			foreach (FTP_t_ReplyLine fTP_t_ReplyLine in error)
			{
				WriteLine(fTP_t_ReplyLine.ToString());
			}
		}
		else if (fTP_e_Appe.FileStream == null)
		{
			WriteLine("500 Internal server error: File stream not provided by server.");
		}
		else
		{
			m_pDataConnection = new DataConnection(this, fTP_e_Appe.FileStream, read_write: true);
			m_pDataConnection.Start();
		}
	}

	private void SIZE(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid file name.");
		}
		FTP_e_GetFileSize fTP_e_GetFileSize = new FTP_e_GetFileSize(argsText);
		OnGetFileSize(fTP_e_GetFileSize);
		if (fTP_e_GetFileSize.Error != null)
		{
			FTP_t_ReplyLine[] error = fTP_e_GetFileSize.Error;
			foreach (FTP_t_ReplyLine fTP_t_ReplyLine in error)
			{
				WriteLine(fTP_t_ReplyLine.ToString());
			}
		}
		else
		{
			WriteLine("213 " + fTP_e_GetFileSize.FileSize);
		}
	}

	private void RNFR(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid path value.");
		}
		m_RenameFrom = argsText;
		WriteLine("350 OK, waiting for RNTO command.");
	}

	private void RNTO(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid path value.");
		}
		if (m_RenameFrom.Length == 0)
		{
			WriteLine("503 Bad sequence of commands.");
			return;
		}
		FTP_e_Rnto fTP_e_Rnto = new FTP_e_Rnto(m_RenameFrom, argsText);
		OnRnto(fTP_e_Rnto);
		if (fTP_e_Rnto.Response == null)
		{
			WriteLine("500 Internal server error: FTP server didn't provide response for RNTO command.");
			return;
		}
		FTP_t_ReplyLine[] response = fTP_e_Rnto.Response;
		foreach (FTP_t_ReplyLine fTP_t_ReplyLine in response)
		{
			WriteLine(fTP_t_ReplyLine.ToString());
		}
	}

	private void RMD(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid directory name.");
		}
		FTP_e_Rmd fTP_e_Rmd = new FTP_e_Rmd(argsText);
		OnRmd(fTP_e_Rmd);
		if (fTP_e_Rmd.Response == null)
		{
			WriteLine("500 Internal server error: FTP server didn't provide response for RMD command.");
			return;
		}
		FTP_t_ReplyLine[] response = fTP_e_Rmd.Response;
		foreach (FTP_t_ReplyLine fTP_t_ReplyLine in response)
		{
			WriteLine(fTP_t_ReplyLine.ToString());
		}
	}

	private void MKD(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		if (string.IsNullOrEmpty(argsText))
		{
			WriteLine("501 Invalid directory name.");
		}
		FTP_e_Mkd fTP_e_Mkd = new FTP_e_Mkd(argsText);
		OnMkd(fTP_e_Mkd);
		if (fTP_e_Mkd.Response == null)
		{
			WriteLine("500 Internal server error: FTP server didn't provide response for MKD command.");
			return;
		}
		FTP_t_ReplyLine[] response = fTP_e_Mkd.Response;
		foreach (FTP_t_ReplyLine fTP_t_ReplyLine in response)
		{
			WriteLine(fTP_t_ReplyLine.ToString());
		}
	}

	private void LIST(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		FTP_e_GetDirListing fTP_e_GetDirListing = new FTP_e_GetDirListing(argsText);
		OnGetDirListing(fTP_e_GetDirListing);
		if (fTP_e_GetDirListing.Error != null)
		{
			FTP_t_ReplyLine[] error = fTP_e_GetDirListing.Error;
			foreach (FTP_t_ReplyLine fTP_t_ReplyLine in error)
			{
				WriteLine(fTP_t_ReplyLine.ToString());
			}
			return;
		}
		MemoryStreamEx memoryStreamEx = new MemoryStreamEx(8000);
		foreach (FTP_ListItem item in fTP_e_GetDirListing.Items)
		{
			if (item.IsDir)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(item.Modified.ToString("MM-dd-yy HH:mm") + " <DIR> " + item.Name + "\r\n");
				memoryStreamEx.Write(bytes, 0, bytes.Length);
				continue;
			}
			byte[] bytes2 = Encoding.UTF8.GetBytes(item.Modified.ToString("MM-dd-yy HH:mm") + " " + item.Size + " " + item.Name + "\r\n");
			memoryStreamEx.Write(bytes2, 0, bytes2.Length);
		}
		memoryStreamEx.Position = 0L;
		m_pDataConnection = new DataConnection(this, memoryStreamEx, read_write: false);
		m_pDataConnection.Start();
	}

	private void NLST(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		FTP_e_GetDirListing fTP_e_GetDirListing = new FTP_e_GetDirListing(argsText);
		OnGetDirListing(fTP_e_GetDirListing);
		if (fTP_e_GetDirListing.Error != null)
		{
			FTP_t_ReplyLine[] error = fTP_e_GetDirListing.Error;
			foreach (FTP_t_ReplyLine fTP_t_ReplyLine in error)
			{
				WriteLine(fTP_t_ReplyLine.ToString());
			}
			return;
		}
		MemoryStreamEx memoryStreamEx = new MemoryStreamEx(8000);
		foreach (FTP_ListItem item in fTP_e_GetDirListing.Items)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(item.Name + "\r\n");
			memoryStreamEx.Write(bytes, 0, bytes.Length);
		}
		memoryStreamEx.Position = 0L;
		m_pDataConnection = new DataConnection(this, memoryStreamEx, read_write: false);
		m_pDataConnection.Start();
	}

	private void TYPE(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
		}
		else if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
		}
		else if (argsText.Trim().ToUpper() == "A" || argsText.Trim().ToUpper() == "I")
		{
			WriteLine("200 Type is set to " + argsText + ".");
		}
		else
		{
			WriteLine("500 Invalid type " + argsText + ".");
		}
	}

	private void PORT(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		string[] array = argsText.Split(',');
		if (array.Length != 6)
		{
			WriteLine("550 Invalid arguments.");
			return;
		}
		string ipString = array[0] + "." + array[1] + "." + array[2] + "." + array[3];
		int port = (Convert.ToInt32(array[4]) << 8) | Convert.ToInt32(array[5]);
		m_pDataConEndPoint = new IPEndPoint(IPAddress.Parse(ipString), port);
		WriteLine("200 PORT Command successful.");
	}

	private void PASV(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
			return;
		}
		if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
			return;
		}
		int num = Server.PassiveStartPort;
		if (m_pPassiveSocket != null)
		{
			num = ((IPEndPoint)m_pPassiveSocket.LocalEndPoint).Port;
		}
		else
		{
			m_pPassiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			for (int i = num; i < 65535; i++)
			{
				try
				{
					m_pPassiveSocket.Bind(new IPEndPoint(IPAddress.Any, i));
					m_pPassiveSocket.Listen(1);
					num = i;
				}
				catch
				{
					continue;
				}
				break;
			}
		}
		if (Server.PassivePublicIP != null)
		{
			WriteLine("227 Entering Passive Mode (" + Server.PassivePublicIP.ToString().Replace(".", ",") + "," + (num >> 8) + "," + (num & 0xFF) + ").");
		}
		else
		{
			WriteLine("227 Entering Passive Mode (" + LocalEndPoint.Address.ToString().Replace(".", ",") + "," + (num >> 8) + "," + (num & 0xFF) + ").");
		}
		m_PassiveMode = true;
	}

	private void SYST(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
		}
		else if (!base.IsAuthenticated)
		{
			WriteLine("530 Please authenticate firtst !");
		}
		else
		{
			WriteLine("215 Windows_NT");
		}
	}

	private void NOOP(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
		}
		else
		{
			WriteLine("200 OK");
		}
	}

	private void QUIT(string argsText)
	{
		try
		{
			WriteLine("221 FTP server signing off");
		}
		catch
		{
		}
		Disconnect();
		Dispose();
	}

	private void FEAT(string argsText)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("211-Extensions supported:\r\n");
		if (!IsSecureConnection && base.Certificate != null)
		{
			stringBuilder.Append(" TLS\r\n");
		}
		stringBuilder.Append(" SIZE\r\n");
		stringBuilder.Append("211 End of extentions.\r\n");
		WriteLine(stringBuilder.ToString());
	}

	private void OPTS(string argsText)
	{
		if (m_SessionRejected)
		{
			WriteLine("500 Bad sequence of commands: Session rejected.");
		}
		else if (string.Equals(argsText, "UTF8 ON", StringComparison.InvariantCultureIgnoreCase))
		{
			WriteLine("200 Ok.");
		}
		else
		{
			WriteLine("501 OPTS parameter not supported.");
		}
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
			Server.Logger.AddWrite(ID, AuthenticatedUserIdentity, num, line.TrimEnd(), LocalEndPoint, RemoteEndPoint);
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

	private FTP_e_Started OnStarted(string reply)
	{
		FTP_e_Started fTP_e_Started = new FTP_e_Started(reply);
		if (this.Started != null)
		{
			this.Started(this, fTP_e_Started);
		}
		return fTP_e_Started;
	}

	private FTP_e_Authenticate OnAuthenticate(string user, string password)
	{
		FTP_e_Authenticate fTP_e_Authenticate = new FTP_e_Authenticate(user, password);
		if (this.Authenticate != null)
		{
			this.Authenticate(this, fTP_e_Authenticate);
		}
		return fTP_e_Authenticate;
	}

	private void OnGetFile(FTP_e_GetFile e)
	{
		if (this.GetFile != null)
		{
			this.GetFile(this, e);
		}
	}

	private void OnStor(FTP_e_Stor e)
	{
		if (this.Stor != null)
		{
			this.Stor(this, e);
		}
	}

	private void OnGetFileSize(FTP_e_GetFileSize e)
	{
		if (this.GetFileSize != null)
		{
			this.GetFileSize(this, e);
		}
	}

	private void OnDele(FTP_e_Dele e)
	{
		if (this.Dele != null)
		{
			this.Dele(this, e);
		}
	}

	private void OnAppe(FTP_e_Appe e)
	{
		if (this.Appe != null)
		{
			this.Appe(this, e);
		}
	}

	private void OnCwd(FTP_e_Cwd e)
	{
		if (this.Cwd != null)
		{
			this.Cwd(this, e);
		}
	}

	private void OnCdup(FTP_e_Cdup e)
	{
		if (this.Cdup != null)
		{
			this.Cdup(this, e);
		}
	}

	private void OnRmd(FTP_e_Rmd e)
	{
		if (this.Rmd != null)
		{
			this.Rmd(this, e);
		}
	}

	private void OnMkd(FTP_e_Mkd e)
	{
		if (this.Mkd != null)
		{
			this.Mkd(this, e);
		}
	}

	private void OnGetDirListing(FTP_e_GetDirListing e)
	{
		if (this.GetDirListing != null)
		{
			this.GetDirListing(this, e);
		}
	}

	private void OnRnto(FTP_e_Rnto e)
	{
		if (this.Rnto != null)
		{
			this.Rnto(this, e);
		}
	}
}
