using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IO;
using LumiSoft.Net.MIME;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.IMAP.Client;

public class IMAP_Client : TCP_Client
{
	public class SettingsHolder
	{
		private int m_ResponseLineSize = 512000;

		public int ResponseLineSize
		{
			get
			{
				return m_ResponseLineSize;
			}
			set
			{
				if (value < 32000)
				{
					throw new ArgumentException("Value must be >= 32000(32kb).", "value");
				}
				m_ResponseLineSize = value;
			}
		}

		internal SettingsHolder()
		{
		}
	}

	internal class CmdLine
	{
		private byte[] m_pData;

		private string m_LogText;

		public byte[] Data => m_pData;

		public string LogText => m_LogText;

		public CmdLine(byte[] data, string logText)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (logText == null)
			{
				throw new ArgumentNullException("logText");
			}
			m_pData = data;
			m_LogText = logText;
		}
	}

	public abstract class CmdAsyncOP<T> : IDisposable, IAsyncOP where T : IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private List<CmdLine> m_pCmdLines;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		internal List<CmdLine> CmdLines => m_pCmdLines;

		public event EventHandler<EventArgs<T>> CompletedAsync;

		public CmdAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			m_pCallback = callback;
			m_pCmdLines = new List<CmdLine>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				m_pCmdLines = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				OnInitCmdLine(owner);
				SendCmdAndReadRespAsyncOP op = new SendCmdAndReadRespAsyncOP(m_pCmdLines.ToArray(), m_pCallback);
				op.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					try
					{
						if (op.Error != null)
						{
							m_pException = e.Value.Error;
							m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
						}
						else
						{
							m_pFinalResponse = op.FinalResponse;
							if (op.FinalResponse.IsError)
							{
								m_pException = new IMAP_ClientException(op.FinalResponse);
							}
						}
						SetState(AsyncOP_State.Completed);
					}
					finally
					{
						op.Dispose();
					}
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(op))
				{
					try
					{
						if (op.Error != null)
						{
							m_pException = op.Error;
							m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
						}
						else
						{
							m_pFinalResponse = op.FinalResponse;
							if (op.FinalResponse.IsError)
							{
								m_pException = new IMAP_ClientException(op.FinalResponse);
							}
						}
						SetState(AsyncOP_State.Completed);
					}
					finally
					{
						op.Dispose();
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		protected abstract void OnInitCmdLine(IMAP_Client imap);

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

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<T>((T)(object)this));
			}
		}
	}

	public class StartTlsAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private RemoteCertificateValidationCallback m_pCertCallback;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<StartTlsAsyncOP>> CompletedAsync;

		public StartTlsAsyncOP(RemoteCertificateValidationCallback certCallback, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			m_pCertCallback = certCallback;
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " STARTTLS\r\n");
				string cmdLineLogText = Encoding.UTF8.GetString(bytes).TrimEnd();
				SendCmdAndReadRespAsyncOP sendCmdAndReadRespAsyncOP = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				sendCmdAndReadRespAsyncOP.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					ProcessCmdResult(e.Value);
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(sendCmdAndReadRespAsyncOP))
				{
					ProcessCmdResult(sendCmdAndReadRespAsyncOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ProcessCmdResult(SendCmdAndReadRespAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					return;
				}
				m_pFinalResponse = op.FinalResponse;
				if (op.FinalResponse.IsError)
				{
					m_pException = new IMAP_ClientException(op.FinalResponse);
					SetState(AsyncOP_State.Completed);
					return;
				}
				SwitchToSecureAsyncOP switchToSecureAsyncOP = new SwitchToSecureAsyncOP(m_pCertCallback);
				switchToSecureAsyncOP.CompletedAsync += delegate(object sender, EventArgs<SwitchToSecureAsyncOP> e)
				{
					if (e.Value.Error != null)
					{
						m_pException = e.Value.Error;
					}
					SetState(AsyncOP_State.Completed);
				};
				if (!m_pImapClient.SwitchToSecureAsync(switchToSecureAsyncOP))
				{
					if (switchToSecureAsyncOP.Error != null)
					{
						m_pException = switchToSecureAsyncOP.Error;
					}
					SetState(AsyncOP_State.Completed);
				}
			}
			finally
			{
				op.Dispose();
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<StartTlsAsyncOP>(this));
			}
		}
	}

	public class LoginAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private string m_User;

		private string m_Password;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<LoginAsyncOP>> CompletedAsync;

		public LoginAsyncOP(string user, string password, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (string.IsNullOrEmpty(user))
			{
				throw new ArgumentException("Argument 'user' value must be specified.", "user");
			}
			if (password == null)
			{
				throw new ArgumentNullException("password");
			}
			m_User = user;
			m_Password = password;
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " LOGIN " + TextUtils.QuoteString(m_User) + " " + TextUtils.QuoteString(m_Password) + "\r\n");
				string cmdLineLogText = (m_pImapClient.m_CommandIndex - 1).ToString("d5") + " LOGIN " + TextUtils.QuoteString(m_User) + " <PASSWORD-REMOVED>";
				SendCmdAndReadRespAsyncOP args = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				args.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					try
					{
						if (args.Error != null)
						{
							m_pException = e.Value.Error;
						}
						else
						{
							m_pFinalResponse = args.FinalResponse;
							if (args.FinalResponse.IsError)
							{
								m_pException = new IMAP_ClientException(args.FinalResponse);
							}
							else
							{
								m_pImapClient.m_pAuthenticatedUser = new GenericIdentity(m_User, "IMAP-LOGIN");
							}
						}
						SetState(AsyncOP_State.Completed);
					}
					finally
					{
						args.Dispose();
					}
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(args))
				{
					try
					{
						if (args.Error != null)
						{
							m_pException = args.Error;
						}
						else
						{
							m_pFinalResponse = args.FinalResponse;
							if (args.FinalResponse.IsError)
							{
								m_pException = new IMAP_ClientException(args.FinalResponse);
							}
							else
							{
								m_pImapClient.m_pAuthenticatedUser = new GenericIdentity(m_User, "IMAP-LOGIN");
							}
						}
						SetState(AsyncOP_State.Completed);
					}
					finally
					{
						args.Dispose();
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<LoginAsyncOP>(this));
			}
		}
	}

	public class AuthenticateAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_Client m_pImapClient;

		private AUTH_SASL_Client m_pSASL;

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

		public event EventHandler<EventArgs<AuthenticateAsyncOP>> CompletedAsync;

		public AuthenticateAsyncOP(AUTH_SASL_Client sasl)
		{
			if (sasl == null)
			{
				throw new ArgumentNullException("sasl");
			}
			m_pSASL = sasl;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				if (m_pSASL.SupportsInitialResponse && m_pImapClient.SupportsCapability("SASL-IR"))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " AUTHENTICATE " + m_pSASL.Name + " " + Convert.ToBase64String(m_pSASL.Continue(null)) + "\r\n");
					m_pImapClient.LogAddWrite(bytes.Length, Encoding.UTF8.GetString(bytes).TrimEnd());
					m_pImapClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, AuthenticateCommandSendingCompleted, null);
				}
				else
				{
					byte[] bytes2 = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " AUTHENTICATE " + m_pSASL.Name + "\r\n");
					m_pImapClient.LogAddWrite(bytes2.Length, m_pImapClient.m_CommandIndex++.ToString("d5") + " AUTHENTICATE " + m_pSASL.Name);
					m_pImapClient.TcpStream.BeginWrite(bytes2, 0, bytes2.Length, AuthenticateCommandSendingCompleted, null);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + ex.Message, ex);
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

		private void AuthenticateCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pImapClient.TcpStream.EndWrite(ar);
				ReadFinalResponseAsyncOP op = new ReadFinalResponseAsyncOP(null);
				op.CompletedAsync += delegate
				{
					AuthenticateReadResponseCompleted(op);
				};
				if (!m_pImapClient.ReadFinalResponseAsync(op))
				{
					AuthenticateReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void AuthenticateReadResponseCompleted(ReadFinalResponseAsyncOP op)
		{
			try
			{
				if (op.FinalResponse.IsContinue)
				{
					byte[] serverResponse = Convert.FromBase64String(op.FinalResponse.ResponseText);
					byte[] inArray = m_pSASL.Continue(serverResponse);
					byte[] bytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(inArray) + "\r\n");
					m_pImapClient.LogAddWrite(bytes.Length, Convert.ToBase64String(inArray));
					m_pImapClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, AuthenticateCommandSendingCompleted, null);
				}
				else if (!op.FinalResponse.IsError)
				{
					m_pImapClient.m_pAuthenticatedUser = new GenericIdentity(m_pSASL.UserName, m_pSASL.Name);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pException = new IMAP_ClientException(op.FinalResponse);
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<AuthenticateAsyncOP>(this));
			}
		}
	}

	public class GetNamespacesAsyncOP : CmdAsyncOP<GetNamespacesAsyncOP>
	{
		public GetNamespacesAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " NAMESPACE\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class GetFoldersAsyncOP : CmdAsyncOP<GetFoldersAsyncOP>
	{
		private string m_Filter;

		public GetFoldersAsyncOP(string filter, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			m_Filter = filter;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			if (m_Filter != null)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " LIST \"\" " + IMAP_Utils.EncodeMailbox(m_Filter, imap.m_MailboxEncoding) + "\r\n");
				base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
			}
			else
			{
				byte[] bytes2 = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " LIST \"\" \"*\"\r\n");
				base.CmdLines.Add(new CmdLine(bytes2, Encoding.UTF8.GetString(bytes2).TrimEnd()));
			}
		}
	}

	public class CreateFolderAsyncOP : CmdAsyncOP<CreateFolderAsyncOP>
	{
		private string m_Folder;

		public CreateFolderAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " CREATE " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class DeleteFolderAsyncOP : CmdAsyncOP<DeleteFolderAsyncOP>
	{
		private string m_Folder;

		public DeleteFolderAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " DELETE " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class RenameFolderAsyncOP : CmdAsyncOP<RenameFolderAsyncOP>
	{
		private string m_Folder;

		private string m_NewFolder;

		public RenameFolderAsyncOP(string folder, string newFolder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			if (newFolder == null)
			{
				throw new ArgumentNullException("newFolder");
			}
			if (string.IsNullOrEmpty(newFolder))
			{
				throw new ArgumentException("Argument 'newFolder' value must be specified.", "newFolder");
			}
			m_Folder = folder;
			m_NewFolder = newFolder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " RENAME " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + " " + IMAP_Utils.EncodeMailbox(m_NewFolder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class GetSubscribedFoldersAsyncOP : CmdAsyncOP<GetSubscribedFoldersAsyncOP>
	{
		private string m_Filter;

		public GetSubscribedFoldersAsyncOP(string filter, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			m_Filter = filter;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			if (m_Filter != null)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " LSUB \"\" " + IMAP_Utils.EncodeMailbox(m_Filter, imap.m_MailboxEncoding) + "\r\n");
				base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
			}
			else
			{
				byte[] bytes2 = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " LSUB \"\" \"*\"\r\n");
				base.CmdLines.Add(new CmdLine(bytes2, Encoding.UTF8.GetString(bytes2).TrimEnd()));
			}
		}
	}

	public class SubscribeFolderAsyncOP : CmdAsyncOP<SubscribeFolderAsyncOP>
	{
		private string m_Folder;

		public SubscribeFolderAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " SUBSCRIBE " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class UnsubscribeFolderAsyncOP : CmdAsyncOP<UnsubscribeFolderAsyncOP>
	{
		private string m_Folder;

		public UnsubscribeFolderAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " UNSUBSCRIBE " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class FolderStatusAsyncOP : CmdAsyncOP<FolderStatusAsyncOP>
	{
		private string m_Folder;

		public FolderStatusAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " STATUS " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + " (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class SelectFolderAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private string m_Folder;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<SelectFolderAsyncOP>> CompletedAsync;

		public SelectFolderAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				m_pImapClient.m_pSelectedFolder = new IMAP_Client_SelectedFolder(m_Folder);
				byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " SELECT " + IMAP_Utils.EncodeMailbox(m_Folder, m_pImapClient.m_MailboxEncoding) + "\r\n");
				string cmdLineLogText = Encoding.UTF8.GetString(bytes).TrimEnd();
				SendCmdAndReadRespAsyncOP sendCmdAndReadRespAsyncOP = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				sendCmdAndReadRespAsyncOP.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					ProecessCmdResult(e.Value);
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(sendCmdAndReadRespAsyncOP))
				{
					ProecessCmdResult(sendCmdAndReadRespAsyncOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ProecessCmdResult(SendCmdAndReadRespAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else
				{
					m_pFinalResponse = op.FinalResponse;
					if (op.FinalResponse.IsError)
					{
						m_pException = new IMAP_ClientException(op.FinalResponse);
						m_pImapClient.m_pSelectedFolder = null;
					}
					else if (m_pFinalResponse.OptionalResponse != null && m_pFinalResponse.OptionalResponse is IMAP_t_orc_ReadOnly)
					{
						m_pImapClient.m_pSelectedFolder.SetReadOnly(value: true);
					}
				}
				SetState(AsyncOP_State.Completed);
			}
			finally
			{
				op.Dispose();
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<SelectFolderAsyncOP>(this));
			}
		}
	}

	public class ExamineFolderAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private string m_Folder;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<ExamineFolderAsyncOP>> CompletedAsync;

		public ExamineFolderAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				m_pImapClient.m_pSelectedFolder = new IMAP_Client_SelectedFolder(m_Folder);
				byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " EXAMINE " + IMAP_Utils.EncodeMailbox(m_Folder, m_pImapClient.m_MailboxEncoding) + "\r\n");
				string cmdLineLogText = Encoding.UTF8.GetString(bytes).TrimEnd();
				SendCmdAndReadRespAsyncOP sendCmdAndReadRespAsyncOP = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				sendCmdAndReadRespAsyncOP.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					ProecessCmdResult(e.Value);
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(sendCmdAndReadRespAsyncOP))
				{
					ProecessCmdResult(sendCmdAndReadRespAsyncOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ProecessCmdResult(SendCmdAndReadRespAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else
				{
					m_pFinalResponse = op.FinalResponse;
					if (op.FinalResponse.IsError)
					{
						m_pException = new IMAP_ClientException(op.FinalResponse);
						m_pImapClient.m_pSelectedFolder = null;
					}
					else if (m_pFinalResponse.OptionalResponse != null && m_pFinalResponse.OptionalResponse is IMAP_t_orc_ReadOnly)
					{
						m_pImapClient.m_pSelectedFolder.SetReadOnly(value: true);
					}
				}
				SetState(AsyncOP_State.Completed);
			}
			finally
			{
				op.Dispose();
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ExamineFolderAsyncOP>(this));
			}
		}
	}

	public class GetFolderQuotaRootsAsyncOP : CmdAsyncOP<GetFolderQuotaRootsAsyncOP>
	{
		private string m_Folder;

		public GetFolderQuotaRootsAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " GETQUOTAROOT " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class GetQuotaAsyncOP : CmdAsyncOP<GetQuotaAsyncOP>
	{
		private string m_QuotaRootName;

		public GetQuotaAsyncOP(string quotaRootName, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (quotaRootName == null)
			{
				throw new ArgumentNullException("quotaRootName");
			}
			m_QuotaRootName = quotaRootName;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " GETQUOTA " + IMAP_Utils.EncodeMailbox(m_QuotaRootName, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class GetFolderAclAsyncOP : CmdAsyncOP<GetFolderAclAsyncOP>
	{
		private string m_Folder;

		public GetFolderAclAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " GETACL " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class SetFolderAclAsyncOP : CmdAsyncOP<SetFolderAclAsyncOP>
	{
		private string m_Folder;

		private string m_Identifier;

		private IMAP_Flags_SetType m_FlagsSetType = IMAP_Flags_SetType.Replace;

		private IMAP_ACL_Flags m_Permissions;

		public SetFolderAclAsyncOP(string folder, string identifier, IMAP_Flags_SetType setType, IMAP_ACL_Flags permissions, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(identifier))
			{
				throw new ArgumentException("Argument 'identifier' value must be specified.", "identifier");
			}
			m_Folder = folder;
			m_Identifier = identifier;
			m_FlagsSetType = setType;
			m_Permissions = permissions;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(imap.m_CommandIndex++.ToString("d5"));
			stringBuilder.Append(" SETACL");
			stringBuilder.Append(" " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding));
			stringBuilder.Append(" " + TextUtils.QuoteString(m_Identifier));
			if (m_FlagsSetType == IMAP_Flags_SetType.Add)
			{
				stringBuilder.Append(" +" + IMAP_Utils.ACL_to_String(m_Permissions));
			}
			else if (m_FlagsSetType == IMAP_Flags_SetType.Remove)
			{
				stringBuilder.Append(" -" + IMAP_Utils.ACL_to_String(m_Permissions));
			}
			else
			{
				if (m_FlagsSetType != IMAP_Flags_SetType.Replace)
				{
					throw new NotSupportedException("Not supported argument 'setType' value '" + m_FlagsSetType.ToString() + "'.");
				}
				stringBuilder.Append(" " + IMAP_Utils.ACL_to_String(m_Permissions));
			}
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class DeleteFolderAclAsyncOP : CmdAsyncOP<DeleteFolderAclAsyncOP>
	{
		private string m_Folder;

		private string m_Identifier;

		public DeleteFolderAclAsyncOP(string folder, string identifier, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			m_Folder = folder;
			m_Identifier = identifier;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " DELETEACL " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + " " + TextUtils.QuoteString(m_Identifier) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class GetFolderRightsAsyncOP : CmdAsyncOP<GetFolderRightsAsyncOP>
	{
		private string m_Folder;

		private string m_Identifier;

		public GetFolderRightsAsyncOP(string folder, string identifier, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			m_Folder = folder;
			m_Identifier = identifier;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " LISTRIGHTS " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + " " + TextUtils.QuoteString(m_Identifier) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class GetFolderMyRightsAsyncOP : CmdAsyncOP<GetFolderMyRightsAsyncOP>
	{
		private string m_Folder;

		public GetFolderMyRightsAsyncOP(string folder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			m_Folder = folder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " MYRIGHTS " + IMAP_Utils.EncodeMailbox(m_Folder, imap.m_MailboxEncoding) + "\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class StoreMessageAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private string m_Folder;

		private IMAP_t_MsgFlags m_pFlags;

		private DateTime m_InternalDate;

		private Stream m_pStream;

		private long m_Count;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public IMAP_t_orc_AppendUid AppendUid
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pFinalResponse != null && m_pFinalResponse.OptionalResponse != null && m_pFinalResponse.OptionalResponse is IMAP_t_orc_AppendUid)
				{
					return (IMAP_t_orc_AppendUid)m_pFinalResponse.OptionalResponse;
				}
				return null;
			}
		}

		public event EventHandler<EventArgs<StoreMessageAsyncOP>> CompletedAsync;

		public StoreMessageAsyncOP(string folder, IMAP_t_MsgFlags flags, DateTime internalDate, Stream message, long count, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			if (folder == null)
			{
				throw new ArgumentNullException("folder");
			}
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
			}
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}
			if (count < 1)
			{
				throw new ArgumentException("Argument 'count' value must be >= 1.", "count");
			}
			m_Folder = folder;
			m_pFlags = flags;
			m_InternalDate = internalDate;
			m_pStream = message;
			m_Count = count;
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(m_pImapClient.m_CommandIndex++.ToString("d5"));
				stringBuilder.Append(" APPEND");
				stringBuilder.Append(" " + IMAP_Utils.EncodeMailbox(m_Folder, m_pImapClient.m_MailboxEncoding));
				if (m_pFlags != null)
				{
					stringBuilder.Append(" (");
					string[] array = m_pFlags.ToArray();
					for (int i = 0; i < array.Length; i++)
					{
						if (i > 0)
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append(array[i]);
					}
					stringBuilder.Append(")");
				}
				if (m_InternalDate != DateTime.MinValue)
				{
					stringBuilder.Append(" " + TextUtils.QuoteString(IMAP_Utils.DateTimeToString(m_InternalDate)));
				}
				stringBuilder.Append(" {" + m_Count + "}\r\n");
				byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
				string cmdLineLogText = Encoding.UTF8.GetString(bytes).TrimEnd();
				SendCmdAndReadRespAsyncOP args = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				args.CompletedAsync += delegate
				{
					ProcessCmdSendingResult(args);
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(args))
				{
					ProcessCmdSendingResult(args);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ProcessCmdSendingResult(SendCmdAndReadRespAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
				}
				else if (op.FinalResponse.IsContinue)
				{
					SmartStream.WriteStreamAsyncOP writeOP = new SmartStream.WriteStreamAsyncOP(m_pStream, m_Count);
					writeOP.CompletedAsync += delegate
					{
						ProcessMsgSendingResult(writeOP);
					};
					if (!m_pImapClient.TcpStream.WriteStreamAsync(writeOP))
					{
						ProcessMsgSendingResult(writeOP);
					}
				}
				else
				{
					m_pFinalResponse = op.FinalResponse;
					m_pException = new IMAP_ClientException(op.FinalResponse);
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
			finally
			{
				op.Dispose();
			}
		}

		private void ProcessMsgSendingResult(SmartStream.WriteStreamAsyncOP writeOP)
		{
			try
			{
				if (writeOP.Error != null)
				{
					m_pException = writeOP.Error;
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
					return;
				}
				m_pImapClient.LogAddWrite(m_Count, "Wrote " + m_Count + " bytes.");
				SendCmdAndReadRespAsyncOP args = new SendCmdAndReadRespAsyncOP(new byte[2] { 13, 10 }, "", m_pCallback);
				args.CompletedAsync += delegate
				{
					if (args.Error != null)
					{
						m_pException = args.Error;
					}
					else
					{
						if (args.FinalResponse.IsError)
						{
							m_pException = new IMAP_ClientException(args.FinalResponse);
						}
						m_pFinalResponse = args.FinalResponse;
					}
					SetState(AsyncOP_State.Completed);
				};
				if (m_pImapClient.SendCmdAndReadRespAsync(args))
				{
					return;
				}
				if (args.Error != null)
				{
					m_pException = args.Error;
				}
				else
				{
					if (args.FinalResponse.IsError)
					{
						m_pException = new IMAP_ClientException(args.FinalResponse);
					}
					m_pFinalResponse = args.FinalResponse;
				}
				SetState(AsyncOP_State.Completed);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
			finally
			{
				writeOP.Dispose();
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<StoreMessageAsyncOP>(this));
			}
		}
	}

	public class EnableAsyncOP : CmdAsyncOP<EnableAsyncOP>
	{
		private string[] m_pCapabilities;

		public EnableAsyncOP(string[] capabilities, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (capabilities == null)
			{
				throw new ArgumentNullException("capabilities");
			}
			if (capabilities.Length < 1)
			{
				throw new ArgumentException("Argument 'capabilities' must contain at least 1 value.", "capabilities");
			}
			m_pCapabilities = capabilities;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(imap.m_CommandIndex++.ToString("d5") + " ENABLE");
			string[] pCapabilities = m_pCapabilities;
			foreach (string text in pCapabilities)
			{
				stringBuilder.Append(" " + text);
			}
			stringBuilder.Append("\r\n");
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class CloseFolderAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<CloseFolderAsyncOP>> CompletedAsync;

		public CloseFolderAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " CLOSE\r\n");
				string cmdLineLogText = Encoding.UTF8.GetString(bytes).TrimEnd();
				SendCmdAndReadRespAsyncOP sendCmdAndReadRespAsyncOP = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				sendCmdAndReadRespAsyncOP.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					ProecessCmdResult(e.Value);
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(sendCmdAndReadRespAsyncOP))
				{
					ProecessCmdResult(sendCmdAndReadRespAsyncOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ProecessCmdResult(SendCmdAndReadRespAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else
				{
					m_pFinalResponse = op.FinalResponse;
					if (op.FinalResponse.IsError)
					{
						m_pException = new IMAP_ClientException(op.FinalResponse);
					}
					else
					{
						m_pImapClient.m_pSelectedFolder = null;
					}
				}
				SetState(AsyncOP_State.Completed);
			}
			finally
			{
				op.Dispose();
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<CloseFolderAsyncOP>(this));
			}
		}
	}

	public class FetchAsyncOP : CmdAsyncOP<FetchAsyncOP>
	{
		private bool m_Uid;

		private IMAP_t_SeqSet m_pSeqSet;

		private IMAP_t_Fetch_i[] m_pDataItems;

		public FetchAsyncOP(bool uid, IMAP_t_SeqSet seqSet, IMAP_t_Fetch_i[] items, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (seqSet == null)
			{
				throw new ArgumentNullException("seqSet");
			}
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			if (items.Length < 1)
			{
				throw new ArgumentException("Argument 'items' must conatain at least 1 value.", "items");
			}
			m_Uid = uid;
			m_pSeqSet = seqSet;
			m_pDataItems = items;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(imap.m_CommandIndex++.ToString("d5"));
			if (m_Uid)
			{
				stringBuilder.Append(" UID");
			}
			stringBuilder.Append(" FETCH " + m_pSeqSet.ToString() + " (");
			for (int i = 0; i < m_pDataItems.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(m_pDataItems[i].ToString());
			}
			stringBuilder.Append(")\r\n");
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class SearchAsyncOP : CmdAsyncOP<SearchAsyncOP>
	{
		private bool m_Uid;

		private Encoding m_pCharset;

		private IMAP_Search_Key m_pCriteria;

		public SearchAsyncOP(bool uid, Encoding charset, IMAP_Search_Key criteria, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (criteria == null)
			{
				throw new ArgumentNullException("criteria");
			}
			m_Uid = uid;
			m_pCharset = charset;
			m_pCriteria = criteria;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			ByteBuilder byteBuilder = new ByteBuilder();
			List<ByteBuilder> list = new List<ByteBuilder>();
			list.Add(byteBuilder);
			byteBuilder.Append(imap.m_CommandIndex++.ToString("d5"));
			if (m_Uid)
			{
				byteBuilder.Append(" UID");
			}
			byteBuilder.Append(" SEARCH");
			if (m_pCharset != null)
			{
				byteBuilder.Append(" CHARSET " + m_pCharset.WebName.ToUpper());
			}
			byteBuilder.Append(" ");
			List<IMAP_Client_CmdPart> list2 = new List<IMAP_Client_CmdPart>();
			m_pCriteria.ToCmdParts(list2);
			foreach (IMAP_Client_CmdPart item in list2)
			{
				if (item.Type == IMAP_Client_CmdPart_Type.Constant)
				{
					byteBuilder.Append(item.Value);
				}
				else if (IMAP_Utils.MustUseLiteralString(item.Value, m_pCharset == null && imap.m_MailboxEncoding == IMAP_Mailbox_Encoding.ImapUtf8))
				{
					byteBuilder.Append("{" + m_pCharset.GetByteCount(item.Value) + "}\r\n");
					byteBuilder = new ByteBuilder();
					list.Add(byteBuilder);
					byteBuilder.Append(m_pCharset, item.Value);
				}
				else if (m_pCharset == null && imap.m_MailboxEncoding == IMAP_Mailbox_Encoding.ImapUtf8)
				{
					byteBuilder.Append(IMAP_Utils.EncodeMailbox(item.Value, imap.m_MailboxEncoding));
				}
				else
				{
					byteBuilder.Append(TextUtils.QuoteString(item.Value));
				}
			}
			byteBuilder.Append("\r\n");
			new List<string>();
			foreach (ByteBuilder item2 in list)
			{
				base.CmdLines.Add(new CmdLine(item2.ToByte(), Encoding.UTF8.GetString(item2.ToByte()).TrimEnd()));
			}
		}
	}

	public class StoreMessageFlagsAsyncOP : CmdAsyncOP<StoreMessageFlagsAsyncOP>
	{
		private bool m_Uid;

		private IMAP_t_SeqSet m_pSeqSet;

		private bool m_Silent = true;

		private IMAP_Flags_SetType m_FlagsSetType = IMAP_Flags_SetType.Replace;

		private IMAP_t_MsgFlags m_pMsgFlags;

		public StoreMessageFlagsAsyncOP(bool uid, IMAP_t_SeqSet seqSet, bool silent, IMAP_Flags_SetType setType, IMAP_t_MsgFlags msgFlags, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (seqSet == null)
			{
				throw new ArgumentNullException("seqSet");
			}
			if (msgFlags == null)
			{
				throw new ArgumentNullException("msgFlags");
			}
			m_Uid = uid;
			m_pSeqSet = seqSet;
			m_Silent = silent;
			m_FlagsSetType = setType;
			m_pMsgFlags = msgFlags;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(imap.m_CommandIndex++.ToString("d5"));
			if (m_Uid)
			{
				stringBuilder.Append(" UID");
			}
			stringBuilder.Append(" STORE");
			stringBuilder.Append(" " + m_pSeqSet.ToString());
			if (m_FlagsSetType == IMAP_Flags_SetType.Add)
			{
				stringBuilder.Append(" +FLAGS");
			}
			else if (m_FlagsSetType == IMAP_Flags_SetType.Remove)
			{
				stringBuilder.Append(" -FLAGS");
			}
			else
			{
				if (m_FlagsSetType != IMAP_Flags_SetType.Replace)
				{
					throw new NotSupportedException("Not supported argument 'setType' value '" + m_FlagsSetType.ToString() + "'.");
				}
				stringBuilder.Append(" FLAGS");
			}
			if (m_Silent)
			{
				stringBuilder.Append(".SILENT");
			}
			if (m_pMsgFlags != null)
			{
				stringBuilder.Append(" (");
				string[] array = m_pMsgFlags.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(" ");
					}
					stringBuilder.Append(array[i]);
				}
				stringBuilder.Append(")\r\n");
			}
			else
			{
				stringBuilder.Append(" ()\r\n");
			}
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class CopyMessagesAsyncOP : CmdAsyncOP<CopyMessagesAsyncOP>
	{
		private bool m_Uid;

		private IMAP_t_SeqSet m_pSeqSet;

		private string m_TargetFolder;

		public IMAP_t_orc_CopyUid CopyUid
		{
			get
			{
				if (base.State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (base.State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (base.FinalResponse != null && base.FinalResponse.OptionalResponse != null && base.FinalResponse.OptionalResponse is IMAP_t_orc_CopyUid)
				{
					return (IMAP_t_orc_CopyUid)base.FinalResponse.OptionalResponse;
				}
				return null;
			}
		}

		public CopyMessagesAsyncOP(bool uid, IMAP_t_SeqSet seqSet, string targetFolder, EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
			if (seqSet == null)
			{
				throw new ArgumentNullException("seqSet");
			}
			if (targetFolder == null)
			{
				throw new ArgumentNullException("targetFolder");
			}
			if (string.IsNullOrEmpty(targetFolder))
			{
				throw new ArgumentException("Argument 'targetFolder' value must be specified.", "targetFolder");
			}
			m_Uid = uid;
			m_pSeqSet = seqSet;
			m_TargetFolder = targetFolder;
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			if (m_Uid)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " UID COPY " + m_pSeqSet.ToString() + " " + IMAP_Utils.EncodeMailbox(m_TargetFolder, imap.m_MailboxEncoding) + "\r\n");
				base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
			}
			else
			{
				byte[] bytes2 = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " COPY " + m_pSeqSet.ToString() + " " + IMAP_Utils.EncodeMailbox(m_TargetFolder, imap.m_MailboxEncoding) + "\r\n");
				base.CmdLines.Add(new CmdLine(bytes2, Encoding.UTF8.GetString(bytes2).TrimEnd()));
			}
		}
	}

	public class ExpungeAsyncOP : CmdAsyncOP<ExpungeAsyncOP>
	{
		public ExpungeAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " EXPUNGE\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class IdleAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

		private bool m_DoneSent;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<IdleAsyncOP>> CompletedAsync;

		public IdleAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		public void Done()
		{
			if (State != AsyncOP_State.Active)
			{
				throw new InvalidOperationException("Mehtod 'Done' can be called only AsyncOP_State.Active state.");
			}
			if (m_DoneSent)
			{
				throw new InvalidOperationException("Mehtod 'Done' already called, Done is in progress.");
			}
			m_DoneSent = true;
			byte[] bytes = Encoding.ASCII.GetBytes("DONE\r\n");
			m_pImapClient.LogAddWrite(bytes.Length, "DONE");
			m_pImapClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, delegate(IAsyncResult ar)
			{
				try
				{
					m_pImapClient.TcpStream.EndWrite(ar);
				}
				catch (Exception pException)
				{
					Exception ex = (m_pException = pException);
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
				}
			}, null);
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				m_pImapClient.m_pIdle = this;
				byte[] bytes = Encoding.UTF8.GetBytes(m_pImapClient.m_CommandIndex++.ToString("d5") + " IDLE\r\n");
				string cmdLineLogText = Encoding.UTF8.GetString(bytes).TrimEnd();
				SendCmdAndReadRespAsyncOP sendCmdAndReadRespAsyncOP = new SendCmdAndReadRespAsyncOP(bytes, cmdLineLogText, m_pCallback);
				sendCmdAndReadRespAsyncOP.CompletedAsync += delegate(object sender, EventArgs<SendCmdAndReadRespAsyncOP> e)
				{
					ProecessCmdResult(e.Value);
				};
				if (!m_pImapClient.SendCmdAndReadRespAsync(sendCmdAndReadRespAsyncOP))
				{
					ProecessCmdResult(sendCmdAndReadRespAsyncOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ProecessCmdResult(SendCmdAndReadRespAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else if (op.FinalResponse.IsError)
				{
					m_pException = new IMAP_ClientException(op.FinalResponse);
					SetState(AsyncOP_State.Completed);
				}
				else if (op.FinalResponse.IsContinue)
				{
					ReadFinalResponseAsyncOP readFinalResponseAsyncOP = new ReadFinalResponseAsyncOP(m_pCallback);
					readFinalResponseAsyncOP.CompletedAsync += delegate(object sender, EventArgs<ReadFinalResponseAsyncOP> e)
					{
						ProcessReadFinalResponseResult(e.Value);
					};
					if (!m_pImapClient.ReadFinalResponseAsync(readFinalResponseAsyncOP))
					{
						ProcessReadFinalResponseResult(readFinalResponseAsyncOP);
					}
				}
				else
				{
					m_pFinalResponse = op.FinalResponse;
					SetState(AsyncOP_State.Completed);
				}
			}
			finally
			{
				op.Dispose();
			}
		}

		private void ProcessReadFinalResponseResult(ReadFinalResponseAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else if (op.FinalResponse.IsError)
				{
					m_pException = new IMAP_ClientException(op.FinalResponse);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pImapClient.m_pIdle = null;
					m_pFinalResponse = op.FinalResponse;
					SetState(AsyncOP_State.Completed);
				}
			}
			finally
			{
				op.Dispose();
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<IdleAsyncOP>(this));
			}
		}
	}

	public class CapabilityAsyncOP : CmdAsyncOP<CapabilityAsyncOP>
	{
		public CapabilityAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " CAPABILITY\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	public class NoopAsyncOP : CmdAsyncOP<NoopAsyncOP>
	{
		public NoopAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
			: base(callback)
		{
		}

		protected override void OnInitCmdLine(IMAP_Client imap)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(imap.m_CommandIndex++.ToString("d5") + " NOOP\r\n");
			base.CmdLines.Add(new CmdLine(bytes, Encoding.UTF8.GetString(bytes).TrimEnd()));
		}
	}

	private class SendCmdAndReadRespAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private Queue<CmdLine> m_pCmdLines;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<SendCmdAndReadRespAsyncOP>> CompletedAsync;

		public SendCmdAndReadRespAsyncOP(byte[] cmdLine, string cmdLineLogText, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			if (cmdLine == null)
			{
				throw new ArgumentNullException("cmdLine");
			}
			if (cmdLine.Length < 1)
			{
				throw new ArgumentException("Argument 'cmdLine' value must be specified.", "cmdLine");
			}
			if (cmdLineLogText == null)
			{
				throw new ArgumentNullException("cmdLineLogText");
			}
			m_pCallback = callback;
			m_pCmdLines = new Queue<CmdLine>();
			m_pCmdLines.Enqueue(new CmdLine(cmdLine, cmdLineLogText));
		}

		public SendCmdAndReadRespAsyncOP(CmdLine[] cmdLines, EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			if (cmdLines == null)
			{
				throw new ArgumentNullException("cmdLines");
			}
			m_pCmdLines = new Queue<CmdLine>(cmdLines);
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCmdLines = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			SendCmdLine();
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

		private void SendCmdLine()
		{
			try
			{
				if (m_pCmdLines.Count == 0)
				{
					throw new Exception("Internal error: No next IMAP command line.");
				}
				CmdLine cmdLine = m_pCmdLines.Dequeue();
				m_pImapClient.LogAddWrite(cmdLine.Data.Length, cmdLine.LogText);
				m_pImapClient.TcpStream.BeginWrite(cmdLine.Data, 0, cmdLine.Data.Length, ProcessCmdLineSendResult, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void ProcessCmdLineSendResult(IAsyncResult ar)
		{
			try
			{
				m_pImapClient.TcpStream.EndWrite(ar);
				ReadFinalResponseAsyncOP args = new ReadFinalResponseAsyncOP(m_pCallback);
				args.CompletedAsync += delegate(object sender, EventArgs<ReadFinalResponseAsyncOP> e)
				{
					try
					{
						if (args.Error != null)
						{
							m_pException = e.Value.Error;
							SetState(AsyncOP_State.Completed);
						}
						else if (args.FinalResponse.IsContinue && m_pCmdLines.Count > 0)
						{
							SendCmdLine();
						}
						else
						{
							m_pFinalResponse = args.FinalResponse;
							SetState(AsyncOP_State.Completed);
						}
					}
					finally
					{
						args.Dispose();
					}
				};
				if (m_pImapClient.ReadFinalResponseAsync(args))
				{
					return;
				}
				try
				{
					if (args.Error != null)
					{
						m_pException = args.Error;
					}
					else
					{
						m_pFinalResponse = args.FinalResponse;
					}
					SetState(AsyncOP_State.Completed);
				}
				finally
				{
					args.Dispose();
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<SendCmdAndReadRespAsyncOP>(this));
			}
		}
	}

	private class ReadResponseAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r m_pResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private SmartStream.ReadLineAsyncOP m_pReadLineOP;

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

		public IMAP_r Response
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pResponse;
			}
		}

		public event EventHandler<EventArgs<ReadResponseAsyncOP>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pResponse = null;
				if (m_pReadLineOP != null)
				{
					m_pReadLineOP.Dispose();
				}
				m_pReadLineOP = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			m_pReadLineOP = new SmartStream.ReadLineAsyncOP(new byte[m_pImapClient.Settings.ResponseLineSize], SizeExceededAction.JunkAndThrowException);
			m_pReadLineOP.CompletedAsync += m_pReadLineOP_Completed;
			SetState(AsyncOP_State.Active);
			try
			{
				if (owner.TcpStream.ReadLine(m_pReadLineOP, async: true))
				{
					ReadLineCompleted(m_pReadLineOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		public void Reuse()
		{
			if (m_State != AsyncOP_State.Completed)
			{
				throw new InvalidOperationException("Reuse is valid only in Completed state.");
			}
			m_State = AsyncOP_State.WaitingForStart;
			m_pException = null;
			m_pResponse = null;
			m_pImapClient = null;
			m_RiseCompleted = false;
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

		private void m_pReadLineOP_Completed(object sender, EventArgs<SmartStream.ReadLineAsyncOP> e)
		{
			try
			{
				ReadLineCompleted(m_pReadLineOP);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void ReadLineCompleted(SmartStream.ReadLineAsyncOP op)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
				}
				else if (op.BytesInBuffer == 0)
				{
					m_pException = new IOException("The remote host shut-down socket.");
				}
				else
				{
					string lineUtf = op.LineUtf8;
					m_pImapClient.LogAddRead(op.BytesInBuffer, lineUtf);
					if (lineUtf.StartsWith("*"))
					{
						string[] array = lineUtf.Split(new char[1] { ' ' }, 4);
						string text = lineUtf.Split(' ')[1];
						if (text.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
						{
							IMAP_r_u_ServerStatus iMAP_r_u_ServerStatus = (IMAP_r_u_ServerStatus)(m_pResponse = IMAP_r_u_ServerStatus.Parse(lineUtf));
							if (iMAP_r_u_ServerStatus.OptionalResponse != null)
							{
								if (iMAP_r_u_ServerStatus.OptionalResponse is IMAP_t_orc_PermanentFlags)
								{
									if (m_pImapClient.SelectedFolder != null)
									{
										m_pImapClient.SelectedFolder.SetPermanentFlags(((IMAP_t_orc_PermanentFlags)iMAP_r_u_ServerStatus.OptionalResponse).Flags);
									}
								}
								else if (iMAP_r_u_ServerStatus.OptionalResponse is IMAP_t_orc_ReadOnly)
								{
									if (m_pImapClient.SelectedFolder != null)
									{
										m_pImapClient.SelectedFolder.SetReadOnly(value: true);
									}
								}
								else if (iMAP_r_u_ServerStatus.OptionalResponse is IMAP_t_orc_ReadWrite)
								{
									if (m_pImapClient.SelectedFolder != null)
									{
										m_pImapClient.SelectedFolder.SetReadOnly(value: true);
									}
								}
								else if (iMAP_r_u_ServerStatus.OptionalResponse is IMAP_t_orc_UidNext)
								{
									if (m_pImapClient.SelectedFolder != null)
									{
										m_pImapClient.SelectedFolder.SetUidNext(((IMAP_t_orc_UidNext)iMAP_r_u_ServerStatus.OptionalResponse).UidNext);
									}
								}
								else if (iMAP_r_u_ServerStatus.OptionalResponse is IMAP_t_orc_UidValidity)
								{
									if (m_pImapClient.SelectedFolder != null)
									{
										m_pImapClient.SelectedFolder.SetUidValidity(((IMAP_t_orc_UidValidity)iMAP_r_u_ServerStatus.OptionalResponse).Uid);
									}
								}
								else if (iMAP_r_u_ServerStatus.OptionalResponse is IMAP_t_orc_Unseen && m_pImapClient.SelectedFolder != null)
								{
									m_pImapClient.SelectedFolder.SetFirstUnseen(((IMAP_t_orc_Unseen)iMAP_r_u_ServerStatus.OptionalResponse).SeqNo);
								}
							}
							m_pImapClient.OnUntaggedStatusResponse((IMAP_r_u)m_pResponse);
						}
						else if (text.Equals("NO", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_ServerStatus.Parse(lineUtf);
							m_pImapClient.OnUntaggedStatusResponse((IMAP_r_u)m_pResponse);
						}
						else if (text.Equals("BAD", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_ServerStatus.Parse(lineUtf);
							m_pImapClient.OnUntaggedStatusResponse((IMAP_r_u)m_pResponse);
						}
						else if (text.Equals("PREAUTH", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_ServerStatus.Parse(lineUtf);
							m_pImapClient.OnUntaggedStatusResponse((IMAP_r_u)m_pResponse);
						}
						else if (text.Equals("BYE", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_ServerStatus.Parse(lineUtf);
							m_pImapClient.OnUntaggedStatusResponse((IMAP_r_u)m_pResponse);
						}
						else if (text.Equals("CAPABILITY", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Capability.Parse(lineUtf);
							m_pImapClient.m_pCapabilities = new List<string>();
							m_pImapClient.m_pCapabilities.AddRange(((IMAP_r_u_Capability)m_pResponse).Capabilities);
						}
						else if (text.Equals("LIST", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_List.Parse(lineUtf);
						}
						else if (text.Equals("LSUB", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_LSub.Parse(lineUtf);
						}
						else if (text.Equals("STATUS", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Status.Parse(lineUtf);
						}
						else if (text.Equals("SEARCH", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Search.Parse(lineUtf);
						}
						else if (text.Equals("FLAGS", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Flags.Parse(lineUtf);
							if (m_pImapClient.m_pSelectedFolder != null)
							{
								m_pImapClient.m_pSelectedFolder.SetFlags(((IMAP_r_u_Flags)m_pResponse).Flags);
							}
						}
						else if (Net_Utils.IsInteger(text) && array[2].Equals("EXISTS", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Exists.Parse(lineUtf);
							if (m_pImapClient.m_pSelectedFolder != null)
							{
								m_pImapClient.m_pSelectedFolder.SetMessagesCount(((IMAP_r_u_Exists)m_pResponse).MessageCount);
							}
						}
						else if (Net_Utils.IsInteger(text) && array[2].Equals("RECENT", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Recent.Parse(lineUtf);
							if (m_pImapClient.m_pSelectedFolder != null)
							{
								m_pImapClient.m_pSelectedFolder.SetRecentMessagesCount(((IMAP_r_u_Recent)m_pResponse).MessageCount);
							}
						}
						else if (Net_Utils.IsInteger(text) && array[2].Equals("EXPUNGE", StringComparison.InvariantCultureIgnoreCase))
						{
							m_pResponse = IMAP_r_u_Expunge.Parse(lineUtf);
							m_pImapClient.OnMessageExpunged((IMAP_r_u_Expunge)m_pResponse);
						}
						else
						{
							if (Net_Utils.IsInteger(text) && array[2].Equals("FETCH", StringComparison.InvariantCultureIgnoreCase))
							{
								((IMAP_r_u_Fetch)(m_pResponse = new IMAP_r_u_Fetch(1))).ParseAsync(m_pImapClient, lineUtf, (EventHandler<EventArgs<Exception>>)FetchParsingCompleted);
								return;
							}
							if (text.Equals("ACL", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_Acl.Parse(lineUtf);
							}
							else if (text.Equals("LISTRIGHTS", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_ListRights.Parse(lineUtf);
							}
							else if (text.Equals("MYRIGHTS", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_MyRights.Parse(lineUtf);
							}
							else if (text.Equals("QUOTA", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_Quota.Parse(lineUtf);
							}
							else if (text.Equals("QUOTAROOT", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_QuotaRoot.Parse(lineUtf);
							}
							else if (text.Equals("NAMESPACE", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_Namespace.Parse(lineUtf);
							}
							else if (text.Equals("ENABLED", StringComparison.InvariantCultureIgnoreCase))
							{
								m_pResponse = IMAP_r_u_Enable.Parse(lineUtf);
							}
						}
						m_pImapClient.OnUntaggedResponse((IMAP_r_u)m_pResponse);
					}
					else if (lineUtf.StartsWith("+"))
					{
						m_pResponse = IMAP_r_ServerStatus.Parse(lineUtf);
					}
					else
					{
						m_pResponse = IMAP_r_ServerStatus.Parse(lineUtf);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			SetState(AsyncOP_State.Completed);
		}

		private void FetchParsingCompleted(object sender, EventArgs<Exception> e)
		{
			try
			{
				if (e.Value != null)
				{
					m_pException = e.Value;
				}
				m_pImapClient.OnUntaggedResponse((IMAP_r_u)m_pResponse);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ReadResponseAsyncOP>(this));
			}
		}
	}

	private class ReadFinalResponseAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private IMAP_r_ServerStatus m_pFinalResponse;

		private IMAP_Client m_pImapClient;

		private bool m_RiseCompleted;

		private EventHandler<EventArgs<IMAP_r_u>> m_pCallback;

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

		public IMAP_r_ServerStatus FinalResponse
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Response' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				return m_pFinalResponse;
			}
		}

		public event EventHandler<EventArgs<ReadFinalResponseAsyncOP>> CompletedAsync;

		public ReadFinalResponseAsyncOP(EventHandler<EventArgs<IMAP_r_u>> callback)
		{
			m_pCallback = callback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pFinalResponse = null;
				m_pCallback = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				ReadResponseAsyncOP args = new ReadResponseAsyncOP();
				args.CompletedAsync += delegate(object sender, EventArgs<ReadResponseAsyncOP> e)
				{
					try
					{
						ResponseReadingCompleted(e.Value);
						args.Reuse();
						while (m_State == AsyncOP_State.Active && !m_pImapClient.ReadResponseAsync(args))
						{
							ResponseReadingCompleted(args);
							args.Reuse();
						}
					}
					catch (Exception pException2)
					{
						m_pException = pException2;
						m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
						SetState(AsyncOP_State.Completed);
					}
				};
				while (m_State == AsyncOP_State.Active && !m_pImapClient.ReadResponseAsync(args))
				{
					ResponseReadingCompleted(args);
					args.Reuse();
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pImapClient.LogAddException("Exception: " + m_pException.Message, m_pException);
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

		private void ResponseReadingCompleted(ReadResponseAsyncOP op)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					SetState(AsyncOP_State.Completed);
				}
				else if (op.Response is IMAP_r_ServerStatus)
				{
					m_pFinalResponse = (IMAP_r_ServerStatus)op.Response;
					SetState(AsyncOP_State.Completed);
				}
				else if (m_pCallback != null)
				{
					m_pCallback(this, new EventArgs<IMAP_r_u>((IMAP_r_u)op.Response));
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ReadFinalResponseAsyncOP>(this));
			}
		}
	}

	internal class ReadStringLiteralAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private Stream m_pStream;

		private int m_LiteralSize;

		private IMAP_Client m_pImapClient;

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

		public Stream Stream
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
				return m_pStream;
			}
		}

		public event EventHandler<EventArgs<ReadStringLiteralAsyncOP>> CompletedAsync;

		public ReadStringLiteralAsyncOP(Stream stream, int literalSize)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = stream;
			m_LiteralSize = literalSize;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pImapClient = null;
				m_pStream = null;
				this.CompletedAsync = null;
			}
		}

		public bool Start(IMAP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pImapClient = owner;
			SetState(AsyncOP_State.Active);
			owner.TcpStream.BeginReadFixedCount(m_pStream, m_LiteralSize, ReadingCompleted, null);
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

		private void ReadingCompleted(IAsyncResult result)
		{
			try
			{
				m_pImapClient.TcpStream.EndReadFixedCount(result);
				m_pImapClient.LogAddRead(m_LiteralSize, "Readed string-literal " + m_LiteralSize + " bytes.");
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ReadStringLiteralAsyncOP>(this));
			}
		}
	}

	[Obsolete("deprecated")]
	internal class _FetchResponseReader
	{
		private IMAP_Client m_pImap;

		private string m_FetchLine;

		private StringReader m_pFetchReader;

		private IMAP_Client_FetchHandler m_pHandler;

		public _FetchResponseReader(IMAP_Client imap, string fetchLine, IMAP_Client_FetchHandler handler)
		{
			if (imap == null)
			{
				throw new ArgumentNullException("imap");
			}
			if (fetchLine == null)
			{
				throw new ArgumentNullException("fetchLine");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			m_pImap = imap;
			m_FetchLine = fetchLine;
			m_pHandler = handler;
		}

		public void Start()
		{
			int currentSeqNo = Convert.ToInt32(m_FetchLine.Split(' ')[1]);
			m_pHandler.SetCurrentSeqNo(currentSeqNo);
			m_pHandler.OnNextMessage();
			m_pFetchReader = new StringReader(m_FetchLine.Split(new char[1] { ' ' }, 4)[3]);
			if (m_pFetchReader.StartsWith("("))
			{
				m_pFetchReader.ReadSpecifiedLength(1);
			}
			while (m_pFetchReader.Available > 0)
			{
				m_pFetchReader.ReadToFirstChar();
				if (m_pFetchReader.StartsWith("BODY ", case_sensitive: false))
				{
					continue;
				}
				if (m_pFetchReader.StartsWith("BODY[", case_sensitive: false))
				{
					m_pFetchReader.ReadWord();
					string bodySection = m_pFetchReader.ReadParenthesized();
					int offset = -1;
					if (m_pFetchReader.StartsWith("<"))
					{
						offset = Convert.ToInt32(m_pFetchReader.ReadParenthesized().Split(' ')[0]);
					}
					IMAP_Client_Fetch_Body_EArgs iMAP_Client_Fetch_Body_EArgs = new IMAP_Client_Fetch_Body_EArgs(bodySection, offset);
					m_pHandler.OnBody(iMAP_Client_Fetch_Body_EArgs);
					m_pFetchReader.ReadToFirstChar();
					if (m_pFetchReader.StartsWith("NIL", case_sensitive: false))
					{
						m_pFetchReader.ReadWord();
					}
					else if (m_pFetchReader.StartsWith("{", case_sensitive: false))
					{
						if (iMAP_Client_Fetch_Body_EArgs.Stream == null)
						{
							m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()), new JunkingStream());
						}
						else
						{
							m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()), iMAP_Client_Fetch_Body_EArgs.Stream);
						}
						m_pFetchReader = new StringReader(m_pImap.ReadLine());
					}
					else
					{
						m_pFetchReader.ReadWord();
					}
					iMAP_Client_Fetch_Body_EArgs.OnStoringCompleted();
				}
				else
				{
					if (m_pFetchReader.StartsWith("BODYSTRUCTURE ", case_sensitive: false))
					{
						continue;
					}
					if (m_pFetchReader.StartsWith("ENVELOPE ", case_sensitive: false))
					{
						m_pHandler.OnEnvelope(IMAP_Envelope.Parse(this));
						continue;
					}
					if (m_pFetchReader.StartsWith("FLAGS ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord();
						string text = m_pFetchReader.ReadParenthesized();
						string[] flags = new string[0];
						if (!string.IsNullOrEmpty(text))
						{
							flags = text.Split(' ');
						}
						m_pHandler.OnFlags(flags);
						continue;
					}
					if (m_pFetchReader.StartsWith("INTERNALDATE ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord();
						m_pHandler.OnInternalDate(IMAP_Utils.ParseDate(m_pFetchReader.ReadWord()));
						continue;
					}
					if (m_pFetchReader.StartsWith("RFC822 ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord(unQuote: false, new char[1] { ' ' }, removeWordTerminator: false);
						m_pFetchReader.ReadToFirstChar();
						IMAP_Client_Fetch_Rfc822_EArgs iMAP_Client_Fetch_Rfc822_EArgs = new IMAP_Client_Fetch_Rfc822_EArgs();
						m_pHandler.OnRfc822(iMAP_Client_Fetch_Rfc822_EArgs);
						if (m_pFetchReader.StartsWith("NIL", case_sensitive: false))
						{
							m_pFetchReader.ReadWord();
						}
						else if (m_pFetchReader.StartsWith("{", case_sensitive: false))
						{
							if (iMAP_Client_Fetch_Rfc822_EArgs.Stream == null)
							{
								m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()), new JunkingStream());
							}
							else
							{
								m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()), iMAP_Client_Fetch_Rfc822_EArgs.Stream);
							}
							m_pFetchReader = new StringReader(m_pImap.ReadLine());
						}
						else
						{
							m_pFetchReader.ReadWord();
						}
						iMAP_Client_Fetch_Rfc822_EArgs.OnStoringCompleted();
						continue;
					}
					if (m_pFetchReader.StartsWith("RFC822.HEADER ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord(unQuote: false, new char[1] { ' ' }, removeWordTerminator: false);
						m_pFetchReader.ReadToFirstChar();
						string text2 = null;
						if (m_pFetchReader.StartsWith("NIL", case_sensitive: false))
						{
							m_pFetchReader.ReadWord();
							text2 = null;
						}
						else if (m_pFetchReader.StartsWith("{", case_sensitive: false))
						{
							text2 = m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()));
							m_pFetchReader = new StringReader(m_pImap.ReadLine());
						}
						else
						{
							text2 = m_pFetchReader.ReadWord();
						}
						m_pHandler.OnRfc822Header(text2);
						continue;
					}
					if (m_pFetchReader.StartsWith("RFC822.SIZE ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord(unQuote: false, new char[1] { ' ' }, removeWordTerminator: false);
						m_pHandler.OnSize(Convert.ToInt32(m_pFetchReader.ReadWord()));
						continue;
					}
					if (m_pFetchReader.StartsWith("RFC822.TEXT ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord(unQuote: false, new char[1] { ' ' }, removeWordTerminator: false);
						m_pFetchReader.ReadToFirstChar();
						string text3 = null;
						if (m_pFetchReader.StartsWith("NIL", case_sensitive: false))
						{
							m_pFetchReader.ReadWord();
							text3 = null;
						}
						else if (m_pFetchReader.StartsWith("{", case_sensitive: false))
						{
							text3 = m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()));
							m_pFetchReader = new StringReader(m_pImap.ReadLine());
						}
						else
						{
							text3 = m_pFetchReader.ReadWord();
						}
						m_pHandler.OnRfc822Text(text3);
						continue;
					}
					if (m_pFetchReader.StartsWith("UID ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord();
						m_pHandler.OnUID(Convert.ToInt64(m_pFetchReader.ReadWord()));
						continue;
					}
					if (m_pFetchReader.StartsWith("X-GM-MSGID ", case_sensitive: false))
					{
						m_pFetchReader.ReadWord();
						m_pHandler.OnX_GM_MSGID(Convert.ToUInt64(m_pFetchReader.ReadWord()));
						continue;
					}
					if (!m_pFetchReader.StartsWith("X-GM-THRID ", case_sensitive: false))
					{
						if (!m_pFetchReader.StartsWith(")", case_sensitive: false))
						{
							throw new NotSupportedException("Not supported IMAP FETCH data-item '" + m_pFetchReader.ReadToEnd() + "'.");
						}
						break;
					}
					m_pFetchReader.ReadWord();
					m_pHandler.OnX_GM_THRID(Convert.ToUInt64(m_pFetchReader.ReadWord()));
				}
			}
		}

		internal StringReader GetReader()
		{
			return m_pFetchReader;
		}

		internal string ReadString()
		{
			m_pFetchReader.ReadToFirstChar();
			if (m_pFetchReader.StartsWith("NIL", case_sensitive: false))
			{
				m_pFetchReader.ReadWord();
				return null;
			}
			if (m_pFetchReader.StartsWith("{"))
			{
				string result = m_pImap.ReadStringLiteral(Convert.ToInt32(m_pFetchReader.ReadParenthesized()));
				m_pFetchReader = new StringReader(m_pImap.ReadLine());
				return result;
			}
			return MIME_Encoding_EncodedWord.DecodeS(m_pFetchReader.ReadWord());
		}
	}

	private GenericIdentity m_pAuthenticatedUser;

	private string m_GreetingText = "";

	private int m_CommandIndex = 1;

	private List<string> m_pCapabilities;

	private IMAP_Client_SelectedFolder m_pSelectedFolder;

	private IMAP_Mailbox_Encoding m_MailboxEncoding = IMAP_Mailbox_Encoding.ImapUtf7;

	private IdleAsyncOP m_pIdle;

	private SettingsHolder m_pSettings;

	public override GenericIdentity AuthenticatedUserIdentity
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pAuthenticatedUser;
		}
	}

	public string GreetingText
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_GreetingText;
		}
	}

	public string[] Capabilities
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			if (m_pCapabilities == null)
			{
				return new string[0];
			}
			return m_pCapabilities.ToArray();
		}
	}

	public char FolderSeparator
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			IMAP_r_u_List[] folders = GetFolders("");
			if (folders.Length == 0)
			{
				throw new Exception("Unexpected result: IMAP server didn't return LIST response for [... LIST \"\" \"\"].");
			}
			return folders[0].HierarchyDelimiter;
		}
	}

	public IMAP_Client_SelectedFolder SelectedFolder
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pSelectedFolder;
		}
	}

	public IdleAsyncOP IdleOP
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pIdle;
		}
	}

	public SettingsHolder Settings
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pSettings;
		}
	}

	public event EventHandler<EventArgs<IMAP_r_u>> UntaggedStatusResponse;

	public event EventHandler<EventArgs<IMAP_r_u>> UntaggedResponse;

	public event EventHandler<EventArgs<IMAP_r_u_Expunge>> MessageExpunged;

	public event EventHandler<IMAP_Client_e_FetchGetStoreStream> FetchGetStoreStream;

	public IMAP_Client()
	{
		m_pSettings = new SettingsHolder();
	}

	public override void Disconnect()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("IMAP client is not connected.");
		}
		try
		{
			WriteLine(m_CommandIndex++.ToString("d5") + " LOGOUT");
		}
		catch
		{
		}
		try
		{
			base.Disconnect();
		}
		catch
		{
		}
		m_pAuthenticatedUser = null;
		m_GreetingText = "";
		m_CommandIndex = 1;
		m_pCapabilities = null;
		m_pSelectedFolder = null;
		m_MailboxEncoding = IMAP_Mailbox_Encoding.ImapUtf7;
		m_pSettings = null;
	}

	public void StartTls()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (IsSecureConnection)
		{
			throw new InvalidOperationException("Connection is already secure.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("STARTTLS is only valid in not-authenticated state.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		using StartTlsAsyncOP startTlsAsyncOP = new StartTlsAsyncOP(null, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			startTlsAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!StartTlsAsync(startTlsAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (startTlsAsyncOP.Error != null)
			{
				throw startTlsAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool StartTlsAsync(StartTlsAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (IsSecureConnection)
		{
			throw new InvalidOperationException("Connection is already secure.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("STARTTLS is only valid in not-authenticated state.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void Login(string user, string password)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("Re-authentication error, you are already authenticated.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user == string.Empty)
		{
			throw new ArgumentException("Argument 'user' value must be specified.");
		}
		using LoginAsyncOP loginAsyncOP = new LoginAsyncOP(user, password, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			loginAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!LoginAsync(loginAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			wait.Close();
			if (loginAsyncOP.Error != null)
			{
				throw loginAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool LoginAsync(LoginAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("Connection is already authenticated.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void Authenticate(AUTH_SASL_Client sasl)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("Connection is already authenticated.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (sasl == null)
		{
			throw new ArgumentNullException("sasl");
		}
		using AuthenticateAsyncOP authenticateAsyncOP = new AuthenticateAsyncOP(sasl);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			authenticateAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!AuthenticateAsync(authenticateAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (authenticateAsyncOP.Error != null)
			{
				throw authenticateAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool AuthenticateAsync(AuthenticateAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("Connection is already authenticated.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_Namespace[] GetNamespaces()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		List<IMAP_r_u_Namespace> retVal = new List<IMAP_r_u_Namespace>();
		using (GetNamespacesAsyncOP getNamespacesAsyncOP = new GetNamespacesAsyncOP(delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Namespace)
			{
				retVal.Add((IMAP_r_u_Namespace)e.Value);
			}
		}))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getNamespacesAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetNamespacesAsync(getNamespacesAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getNamespacesAsyncOP.Error != null)
				{
					throw getNamespacesAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetNamespacesAsync(GetNamespacesAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_List[] GetFolders(string filter)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		List<IMAP_r_u_List> retVal = new List<IMAP_r_u_List>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_List)
			{
				retVal.Add((IMAP_r_u_List)e.Value);
			}
		};
		using (GetFoldersAsyncOP getFoldersAsyncOP = new GetFoldersAsyncOP(filter, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getFoldersAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetFoldersAsync(getFoldersAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getFoldersAsyncOP.Error != null)
				{
					throw getFoldersAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetFoldersAsync(GetFoldersAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void CreateFolder(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using CreateFolderAsyncOP createFolderAsyncOP = new CreateFolderAsyncOP(folder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			createFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!CreateFolderAsync(createFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (createFolderAsyncOP.Error != null)
			{
				throw createFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool CreateFolderAsync(CreateFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void DeleteFolder(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using DeleteFolderAsyncOP deleteFolderAsyncOP = new DeleteFolderAsyncOP(folder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			deleteFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!DeleteFolderAsync(deleteFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (deleteFolderAsyncOP.Error != null)
			{
				throw deleteFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool DeleteFolderAsync(DeleteFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void RenameFolder(string folder, string newFolder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' name must be specified.", "folder");
		}
		if (newFolder == null)
		{
			throw new ArgumentNullException("newFolder");
		}
		if (newFolder == string.Empty)
		{
			throw new ArgumentException("Argument 'newFolder' name must be specified.", "newFolder");
		}
		using RenameFolderAsyncOP renameFolderAsyncOP = new RenameFolderAsyncOP(folder, newFolder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			renameFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!RenameFolderAsync(renameFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (renameFolderAsyncOP.Error != null)
			{
				throw renameFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool RenameFolderAsync(RenameFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_LSub[] GetSubscribedFolders(string filter)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		List<IMAP_r_u_LSub> retVal = new List<IMAP_r_u_LSub>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_LSub)
			{
				retVal.Add((IMAP_r_u_LSub)e.Value);
			}
		};
		using (GetSubscribedFoldersAsyncOP getSubscribedFoldersAsyncOP = new GetSubscribedFoldersAsyncOP(filter, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getSubscribedFoldersAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetSubscribedFoldersAsync(getSubscribedFoldersAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getSubscribedFoldersAsyncOP.Error != null)
				{
					throw getSubscribedFoldersAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetSubscribedFoldersAsync(GetSubscribedFoldersAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void SubscribeFolder(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using SubscribeFolderAsyncOP subscribeFolderAsyncOP = new SubscribeFolderAsyncOP(folder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			subscribeFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!SubscribeFolderAsync(subscribeFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (subscribeFolderAsyncOP.Error != null)
			{
				throw subscribeFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool SubscribeFolderAsync(SubscribeFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void UnsubscribeFolder(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using UnsubscribeFolderAsyncOP unsubscribeFolderAsyncOP = new UnsubscribeFolderAsyncOP(folder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			unsubscribeFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!UnsubscribeFolderAsync(unsubscribeFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (unsubscribeFolderAsyncOP.Error != null)
			{
				throw unsubscribeFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool UnsubscribeFolderAsync(UnsubscribeFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_Status[] FolderStatus(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		List<IMAP_r_u_Status> retVal = new List<IMAP_r_u_Status>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Status)
			{
				retVal.Add((IMAP_r_u_Status)e.Value);
			}
		};
		using (FolderStatusAsyncOP folderStatusAsyncOP = new FolderStatusAsyncOP(folder, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				folderStatusAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!FolderStatusAsync(folderStatusAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (folderStatusAsyncOP.Error != null)
				{
					throw folderStatusAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool FolderStatusAsync(FolderStatusAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void SelectFolder(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using SelectFolderAsyncOP selectFolderAsyncOP = new SelectFolderAsyncOP(folder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			selectFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!SelectFolderAsync(selectFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (selectFolderAsyncOP.Error != null)
			{
				throw selectFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool SelectFolderAsync(SelectFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void ExamineFolder(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using ExamineFolderAsyncOP examineFolderAsyncOP = new ExamineFolderAsyncOP(folder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			examineFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!ExamineFolderAsync(examineFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (examineFolderAsyncOP.Error != null)
			{
				throw examineFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool ExamineFolderAsync(ExamineFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r[] GetFolderQuotaRoots(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		List<IMAP_r> retVal = new List<IMAP_r>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Quota)
			{
				retVal.Add((IMAP_r_u_Quota)e.Value);
			}
			else if (e.Value is IMAP_r_u_QuotaRoot)
			{
				retVal.Add((IMAP_r_u_QuotaRoot)e.Value);
			}
		};
		using (GetFolderQuotaRootsAsyncOP getFolderQuotaRootsAsyncOP = new GetFolderQuotaRootsAsyncOP(folder, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getFolderQuotaRootsAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetFolderQuotaRootsAsync(getFolderQuotaRootsAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getFolderQuotaRootsAsyncOP.Error != null)
				{
					throw getFolderQuotaRootsAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetFolderQuotaRootsAsync(GetFolderQuotaRootsAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_Quota[] GetQuota(string quotaRootName)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (quotaRootName == null)
		{
			throw new ArgumentNullException("quotaRootName");
		}
		List<IMAP_r_u_Quota> retVal = new List<IMAP_r_u_Quota>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Quota)
			{
				retVal.Add((IMAP_r_u_Quota)e.Value);
			}
		};
		using (GetQuotaAsyncOP getQuotaAsyncOP = new GetQuotaAsyncOP(quotaRootName, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getQuotaAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetQuotaAsync(getQuotaAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getQuotaAsyncOP.Error != null)
				{
					throw getQuotaAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetQuotaAsync(GetQuotaAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	private void SetQuota()
	{
	}

	public IMAP_r_u_Acl[] GetFolderAcl(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		List<IMAP_r_u_Acl> retVal = new List<IMAP_r_u_Acl>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Acl)
			{
				retVal.Add((IMAP_r_u_Acl)e.Value);
			}
		};
		using (GetFolderAclAsyncOP getFolderAclAsyncOP = new GetFolderAclAsyncOP(folder, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getFolderAclAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetFolderAclAsync(getFolderAclAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getFolderAclAsyncOP.Error != null)
				{
					throw getFolderAclAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetFolderAclAsync(GetFolderAclAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void SetFolderAcl(string folder, string user, IMAP_Flags_SetType setType, IMAP_ACL_Flags permissions)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user == string.Empty)
		{
			throw new ArgumentException("Argument 'user' value must be specified.", "user");
		}
		using SetFolderAclAsyncOP setFolderAclAsyncOP = new SetFolderAclAsyncOP(folder, user, setType, permissions, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			setFolderAclAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!SetFolderAclAsync(setFolderAclAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (setFolderAclAsyncOP.Error != null)
			{
				throw setFolderAclAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool SetFolderAclAsync(SetFolderAclAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void DeleteFolderAcl(string folder, string user)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user == string.Empty)
		{
			throw new ArgumentException("Argument 'user' value must be specified.", "user");
		}
		using DeleteFolderAclAsyncOP deleteFolderAclAsyncOP = new DeleteFolderAclAsyncOP(folder, user, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			deleteFolderAclAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!DeleteFolderAclAsync(deleteFolderAclAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (deleteFolderAclAsyncOP.Error != null)
			{
				throw deleteFolderAclAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool DeleteFolderAclAsync(DeleteFolderAclAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_ListRights[] GetFolderRights(string folder, string identifier)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (identifier == string.Empty)
		{
			throw new ArgumentException("Argument 'identifier' value must be specified.", "identifier");
		}
		List<IMAP_r_u_ListRights> retVal = new List<IMAP_r_u_ListRights>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_ListRights)
			{
				retVal.Add((IMAP_r_u_ListRights)e.Value);
			}
		};
		using (GetFolderRightsAsyncOP getFolderRightsAsyncOP = new GetFolderRightsAsyncOP(folder, identifier, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getFolderRightsAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetFolderRightsAsync(getFolderRightsAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getFolderRightsAsyncOP.Error != null)
				{
					throw getFolderRightsAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetFolderRightsAsync(GetFolderRightsAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_MyRights[] GetFolderMyRights(string folder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		List<IMAP_r_u_MyRights> retVal = new List<IMAP_r_u_MyRights>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_MyRights)
			{
				retVal.Add((IMAP_r_u_MyRights)e.Value);
			}
		};
		using (GetFolderMyRightsAsyncOP getFolderMyRightsAsyncOP = new GetFolderMyRightsAsyncOP(folder, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getFolderMyRightsAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetFolderMyRightsAsync(getFolderMyRightsAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getFolderMyRightsAsyncOP.Error != null)
				{
					throw getFolderMyRightsAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool GetFolderMyRightsAsync(GetFolderMyRightsAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void StoreMessage(string folder, string[] flags, DateTime internalDate, Stream message, int count)
	{
		StoreMessage(folder, (flags != null) ? new IMAP_t_MsgFlags(flags) : new IMAP_t_MsgFlags(), internalDate, message, count);
	}

	public void StoreMessage(string folder, IMAP_t_MsgFlags flags, DateTime internalDate, Stream message, int count)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (folder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (folder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		if (count < 1)
		{
			throw new ArgumentException("Argument 'count' value must be >= 1.", "count");
		}
		using StoreMessageAsyncOP storeMessageAsyncOP = new StoreMessageAsyncOP(folder, flags, internalDate, message, count, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			storeMessageAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!StoreMessageAsync(storeMessageAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (storeMessageAsyncOP.Error != null)
			{
				throw storeMessageAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public void StoreMessage(StoreMessageAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			op.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!StoreMessageAsync(op))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (op.Error != null)
			{
				throw op.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool StoreMessageAsync(StoreMessageAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public IMAP_r_u_Enable[] Enable(string[] capabilities)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (SelectedFolder != null)
		{
			throw new InvalidOperationException("The 'ENABLE' command MUST only be used in the authenticated state.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (capabilities == null)
		{
			throw new ArgumentNullException("capabilities");
		}
		if (capabilities.Length < 1)
		{
			throw new ArgumentException("Argument 'capabilities' must contain at least 1 value.", "capabilities");
		}
		List<IMAP_r_u_Enable> retVal = new List<IMAP_r_u_Enable>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Enable)
			{
				retVal.Add((IMAP_r_u_Enable)e.Value);
			}
		};
		using (EnableAsyncOP enableAsyncOP = new EnableAsyncOP(capabilities, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				enableAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!EnableAsync(enableAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (enableAsyncOP.Error != null)
				{
					throw enableAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool EnableAsync(EnableAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (SelectedFolder != null)
		{
			throw new InvalidOperationException("The 'ENABLE' command MUST only be used in the authenticated state.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void EnableUtf8()
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (SelectedFolder != null)
		{
			throw new InvalidOperationException("The 'ENABLE UTF8=ACCEPT' command MUST only be used in the authenticated state.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		Enable(new string[1] { "UTF8=ACCEPT" });
		m_MailboxEncoding = IMAP_Mailbox_Encoding.ImapUtf8;
	}

	public void CloseFolder()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		using CloseFolderAsyncOP closeFolderAsyncOP = new CloseFolderAsyncOP(null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			closeFolderAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!CloseFolderAsync(closeFolderAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (closeFolderAsyncOP.Error != null)
			{
				throw closeFolderAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool CloseFolderAsync(CloseFolderAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void Fetch(bool uid, IMAP_t_SeqSet seqSet, IMAP_t_Fetch_i[] items, EventHandler<EventArgs<IMAP_r_u>> callback)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (items.Length < 1)
		{
			throw new ArgumentException("Argument 'items' must conatain at least 1 value.", "items");
		}
		using FetchAsyncOP fetchAsyncOP = new FetchAsyncOP(uid, seqSet, items, callback);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			fetchAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!FetchAsync(fetchAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (fetchAsyncOP.Error != null)
			{
				throw fetchAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool FetchAsync(FetchAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public int[] Search(bool uid, Encoding charset, IMAP_Search_Key criteria)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (criteria == null)
		{
			throw new ArgumentNullException("criteria");
		}
		List<int> retVal = new List<int>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Search)
			{
				retVal.AddRange(((IMAP_r_u_Search)e.Value).Values);
			}
		};
		using (SearchAsyncOP searchAsyncOP = new SearchAsyncOP(uid, charset, criteria, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				searchAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!SearchAsync(searchAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (searchAsyncOP.Error != null)
				{
					throw searchAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool SearchAsync(SearchAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void StoreMessageFlags(bool uid, IMAP_t_SeqSet seqSet, IMAP_Flags_SetType setType, IMAP_t_MsgFlags flags)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		using StoreMessageFlagsAsyncOP storeMessageFlagsAsyncOP = new StoreMessageFlagsAsyncOP(uid, seqSet, silent: true, setType, flags, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			storeMessageFlagsAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!StoreMessageFlagsAsync(storeMessageFlagsAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (storeMessageFlagsAsyncOP.Error != null)
			{
				throw storeMessageFlagsAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool StoreMessageFlagsAsync(StoreMessageFlagsAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void CopyMessages(bool uid, IMAP_t_SeqSet seqSet, string targetFolder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (targetFolder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (targetFolder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		using CopyMessagesAsyncOP copyMessagesAsyncOP = new CopyMessagesAsyncOP(uid, seqSet, targetFolder, null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			copyMessagesAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!CopyMessagesAsync(copyMessagesAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (copyMessagesAsyncOP.Error != null)
			{
				throw copyMessagesAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public void CopyMessages(CopyMessagesAsyncOP op)
	{
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			op.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!CopyMessagesAsync(op))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (op.Error != null)
			{
				throw op.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool CopyMessagesAsync(CopyMessagesAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void MoveMessages(bool uid, IMAP_t_SeqSet seqSet, string targetFolder, bool expunge)
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (targetFolder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (targetFolder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		CopyMessages(uid, seqSet, targetFolder);
		StoreMessageFlags(uid, seqSet, IMAP_Flags_SetType.Add, IMAP_t_MsgFlags.Parse(IMAP_t_MsgFlags.Deleted));
		if (expunge)
		{
			Expunge();
		}
	}

	public void Expunge()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		using ExpungeAsyncOP expungeAsyncOP = new ExpungeAsyncOP(null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			expungeAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!ExpungeAsync(expungeAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (expungeAsyncOP.Error != null)
			{
				throw expungeAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool ExpungeAsync(ExpungeAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public bool IdleAsync(IdleAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("Already idling !");
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

	public IMAP_r_u_Capability[] Capability()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		List<IMAP_r_u_Capability> retVal = new List<IMAP_r_u_Capability>();
		using (CapabilityAsyncOP capabilityAsyncOP = new CapabilityAsyncOP(delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Capability)
			{
				retVal.Add((IMAP_r_u_Capability)e.Value);
			}
		}))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				capabilityAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!CapabilityAsync(capabilityAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (capabilityAsyncOP.Error != null)
				{
					throw capabilityAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	public bool CapabilityAsync(CapabilityAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	public void Noop()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		using NoopAsyncOP noopAsyncOP = new NoopAsyncOP(null);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			noopAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!NoopAsync(noopAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			if (noopAsyncOP.Error != null)
			{
				throw noopAsyncOP.Error;
			}
		}
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	public bool NoopAsync(NoopAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
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

	protected override void OnConnected(CompleteConnectCallback callback)
	{
		ReadResponseAsyncOP op = new ReadResponseAsyncOP();
		op.CompletedAsync += delegate
		{
			ProcessGreetingResult(op, callback);
		};
		if (!ReadResponseAsync(op))
		{
			ProcessGreetingResult(op, callback);
		}
	}

	private void ProcessGreetingResult(ReadResponseAsyncOP op, CompleteConnectCallback connectCallback)
	{
		Exception error = null;
		try
		{
			if (op.Error != null)
			{
				error = op.Error;
			}
			else if (op.Response is IMAP_r_u_ServerStatus)
			{
				IMAP_r_u_ServerStatus iMAP_r_u_ServerStatus = (IMAP_r_u_ServerStatus)op.Response;
				if (iMAP_r_u_ServerStatus.IsError)
				{
					error = new IMAP_ClientException(iMAP_r_u_ServerStatus.ResponseCode, iMAP_r_u_ServerStatus.ResponseText);
				}
				else
				{
					m_GreetingText = iMAP_r_u_ServerStatus.ResponseText;
				}
			}
			else
			{
				error = new Exception("Unexpected IMAP server greeting response: " + op.Response.ToString());
			}
		}
		catch (Exception ex)
		{
			error = ex;
		}
		connectCallback(error);
	}

	private bool SendCmdAndReadRespAsync(SendCmdAndReadRespAsyncOP op)
	{
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

	private bool ReadResponseAsync(ReadResponseAsyncOP op)
	{
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

	private bool ReadFinalResponseAsync(ReadFinalResponseAsyncOP op)
	{
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

	internal bool ReadStringLiteralAsync(ReadStringLiteralAsyncOP op)
	{
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

	private bool SupportsCapability(string capability)
	{
		if (capability == null)
		{
			throw new ArgumentNullException("capability");
		}
		if (m_pCapabilities == null)
		{
			return false;
		}
		foreach (string pCapability in m_pCapabilities)
		{
			if (string.Equals(pCapability, capability, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void OnUntaggedStatusResponse(IMAP_r_u response)
	{
		if (this.UntaggedStatusResponse != null)
		{
			this.UntaggedStatusResponse(this, new EventArgs<IMAP_r_u>(response));
		}
	}

	private void OnUntaggedResponse(IMAP_r_u response)
	{
		if (this.UntaggedResponse != null)
		{
			this.UntaggedResponse(this, new EventArgs<IMAP_r_u>(response));
		}
	}

	private void OnMessageExpunged(IMAP_r_u_Expunge response)
	{
		if (this.MessageExpunged != null)
		{
			this.MessageExpunged(this, new EventArgs<IMAP_r_u_Expunge>(response));
		}
	}

	internal void OnFetchGetStoreStream(IMAP_Client_e_FetchGetStoreStream e)
	{
		if (this.FetchGetStoreStream != null)
		{
			this.FetchGetStoreStream(this, e);
		}
	}

	[Obsolete("deprecated")]
	private IMAP_r_ServerStatus ReadResponse(List<IMAP_r_u_Capability> capability, IMAP_Client_SelectedFolder folderInfo, List<int> search, List<IMAP_r_u_List> list, List<IMAP_r_u_LSub> lsub, List<IMAP_r_u_Acl> acl, List<IMAP_Response_MyRights> myRights, List<IMAP_r_u_ListRights> listRights, List<IMAP_r_u_Status> status, List<IMAP_r_u_Quota> quota, List<IMAP_r_u_QuotaRoot> quotaRoot, List<IMAP_r_u_Namespace> nspace, IMAP_Client_FetchHandler fetchHandler, List<IMAP_r_u_Enable> enable)
	{
		SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
		string lineUtf;
		while (true)
		{
			TcpStream.ReadLine(readLineAsyncOP, async: false);
			if (readLineAsyncOP.Error != null)
			{
				throw readLineAsyncOP.Error;
			}
			lineUtf = readLineAsyncOP.LineUtf8;
			LogAddRead(readLineAsyncOP.BytesInBuffer, lineUtf);
			if (!lineUtf.StartsWith("*"))
			{
				break;
			}
			string[] array = lineUtf.Split(new char[1] { ' ' }, 4);
			string text = lineUtf.Split(' ')[1];
			if (text.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
			{
				IMAP_r_u_ServerStatus iMAP_r_u_ServerStatus = IMAP_r_u_ServerStatus.Parse(lineUtf);
				if (!string.IsNullOrEmpty(iMAP_r_u_ServerStatus.OptionalResponseCode))
				{
					if (iMAP_r_u_ServerStatus.OptionalResponseCode.Equals("PERMANENTFLAGS", StringComparison.InvariantCultureIgnoreCase))
					{
						if (folderInfo != null)
						{
							StringReader stringReader = new StringReader(iMAP_r_u_ServerStatus.OptionalResponseArgs);
							folderInfo.SetPermanentFlags(stringReader.ReadParenthesized().Split(' '));
						}
					}
					else if (iMAP_r_u_ServerStatus.OptionalResponseCode.Equals("READ-ONLY", StringComparison.InvariantCultureIgnoreCase))
					{
						folderInfo?.SetReadOnly(value: true);
					}
					else if (iMAP_r_u_ServerStatus.OptionalResponseCode.Equals("READ-WRITE", StringComparison.InvariantCultureIgnoreCase))
					{
						folderInfo?.SetReadOnly(value: true);
					}
					else if (iMAP_r_u_ServerStatus.OptionalResponseCode.Equals("UIDNEXT", StringComparison.InvariantCultureIgnoreCase))
					{
						folderInfo?.SetUidNext(Convert.ToInt64(iMAP_r_u_ServerStatus.OptionalResponseArgs));
					}
					else if (iMAP_r_u_ServerStatus.OptionalResponseCode.Equals("UIDVALIDITY", StringComparison.InvariantCultureIgnoreCase))
					{
						folderInfo?.SetUidValidity(Convert.ToInt64(iMAP_r_u_ServerStatus.OptionalResponseArgs));
					}
					else if (iMAP_r_u_ServerStatus.OptionalResponseCode.Equals("UNSEEN", StringComparison.InvariantCultureIgnoreCase))
					{
						folderInfo?.SetFirstUnseen(Convert.ToInt32(iMAP_r_u_ServerStatus.OptionalResponseArgs));
					}
				}
				OnUntaggedStatusResponse(iMAP_r_u_ServerStatus);
			}
			else if (text.Equals("NO", StringComparison.InvariantCultureIgnoreCase))
			{
				OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(lineUtf));
			}
			else if (text.Equals("BAD", StringComparison.InvariantCultureIgnoreCase))
			{
				OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(lineUtf));
			}
			else if (text.Equals("PREAUTH", StringComparison.InvariantCultureIgnoreCase))
			{
				OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(lineUtf));
			}
			else if (text.Equals("BYE", StringComparison.InvariantCultureIgnoreCase))
			{
				OnUntaggedStatusResponse(IMAP_r_u_ServerStatus.Parse(lineUtf));
			}
			else if (text.Equals("CAPABILITY", StringComparison.InvariantCultureIgnoreCase))
			{
				capability?.Add(IMAP_r_u_Capability.Parse(lineUtf));
			}
			else if (text.Equals("LIST", StringComparison.InvariantCultureIgnoreCase))
			{
				list?.Add(IMAP_r_u_List.Parse(lineUtf));
			}
			else if (text.Equals("LSUB", StringComparison.InvariantCultureIgnoreCase))
			{
				lsub?.Add(IMAP_r_u_LSub.Parse(lineUtf));
			}
			else if (text.Equals("STATUS", StringComparison.InvariantCultureIgnoreCase))
			{
				status?.Add(IMAP_r_u_Status.Parse(lineUtf));
			}
			else if (text.Equals("SEARCH", StringComparison.InvariantCultureIgnoreCase))
			{
				if (search != null && lineUtf.Split(' ').Length > 2)
				{
					string[] array2 = lineUtf.Split(new char[1] { ' ' }, 3)[2].Split(' ');
					foreach (string value in array2)
					{
						search.Add(Convert.ToInt32(value));
					}
				}
			}
			else if (text.Equals("FLAGS", StringComparison.InvariantCultureIgnoreCase))
			{
				if (folderInfo != null)
				{
					StringReader stringReader2 = new StringReader(lineUtf.Split(new char[1] { ' ' }, 3)[2]);
					folderInfo.SetFlags(stringReader2.ReadParenthesized().Split(' '));
				}
			}
			else if (Net_Utils.IsInteger(text) && array[2].Equals("EXISTS", StringComparison.InvariantCultureIgnoreCase))
			{
				folderInfo?.SetMessagesCount(Convert.ToInt32(text));
			}
			else if (Net_Utils.IsInteger(text) && array[2].Equals("RECENT", StringComparison.InvariantCultureIgnoreCase))
			{
				folderInfo?.SetRecentMessagesCount(Convert.ToInt32(text));
			}
			else if (Net_Utils.IsInteger(text) && array[2].Equals("EXPUNGE", StringComparison.InvariantCultureIgnoreCase))
			{
				OnMessageExpunged(IMAP_r_u_Expunge.Parse(lineUtf));
			}
			else if (Net_Utils.IsInteger(text) && array[2].Equals("FETCH", StringComparison.InvariantCultureIgnoreCase))
			{
				if (fetchHandler == null)
				{
					fetchHandler = new IMAP_Client_FetchHandler();
				}
				new _FetchResponseReader(this, lineUtf, fetchHandler).Start();
			}
			else if (text.Equals("ACL", StringComparison.InvariantCultureIgnoreCase))
			{
				acl?.Add(IMAP_r_u_Acl.Parse(lineUtf));
			}
			else if (text.Equals("LISTRIGHTS", StringComparison.InvariantCultureIgnoreCase))
			{
				listRights?.Add(IMAP_r_u_ListRights.Parse(lineUtf));
			}
			else if (text.Equals("MYRIGHTS", StringComparison.InvariantCultureIgnoreCase))
			{
				myRights?.Add(IMAP_Response_MyRights.Parse(lineUtf));
			}
			else if (text.Equals("QUOTA", StringComparison.InvariantCultureIgnoreCase))
			{
				quota?.Add(IMAP_r_u_Quota.Parse(lineUtf));
			}
			else if (text.Equals("QUOTAROOT", StringComparison.InvariantCultureIgnoreCase))
			{
				quotaRoot?.Add(IMAP_r_u_QuotaRoot.Parse(lineUtf));
			}
			else if (text.Equals("NAMESPACE", StringComparison.InvariantCultureIgnoreCase))
			{
				nspace?.Add(IMAP_r_u_Namespace.Parse(lineUtf));
			}
			else if (text.Equals("ENABLED", StringComparison.InvariantCultureIgnoreCase))
			{
				enable?.Add(IMAP_r_u_Enable.Parse(lineUtf));
			}
		}
		if (lineUtf.StartsWith("+"))
		{
			return new IMAP_r_ServerStatus("+", "+", "+");
		}
		return IMAP_r_ServerStatus.Parse(lineUtf);
	}

	[Obsolete("Use Search(bool uid,Encoding charset,IMAP_Search_Key criteria) instead.")]
	public int[] Search(bool uid, string charset, string criteria)
	{
		if (criteria == null)
		{
			throw new ArgumentNullException("criteria");
		}
		if (criteria == string.Empty)
		{
			throw new ArgumentException("Argument 'criteria' value must be specified.", "criteria");
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_CommandIndex++.ToString("d5"));
		if (uid)
		{
			stringBuilder.Append(" UID");
		}
		stringBuilder.Append(" SEARCH");
		if (!string.IsNullOrEmpty(charset))
		{
			stringBuilder.Append(" CHARSET " + charset);
		}
		stringBuilder.Append(" " + criteria + "\r\n");
		SendCommand(stringBuilder.ToString());
		List<int> retVal = new List<int>();
		IMAP_r_ServerStatus iMAP_r_ServerStatus = ReadFinalResponse(delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Search)
			{
				retVal.AddRange(((IMAP_r_u_Search)e.Value).Values);
			}
		});
		if (!iMAP_r_ServerStatus.ResponseCode.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new IMAP_ClientException(iMAP_r_ServerStatus.ResponseCode, iMAP_r_ServerStatus.ResponseText);
		}
		return retVal.ToArray();
	}

	[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_Fetch_DataItem[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
	public void Fetch(bool uid, IMAP_SequenceSet seqSet, IMAP_Fetch_DataItem[] items, IMAP_Client_FetchHandler handler)
	{
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (items.Length < 1)
		{
			throw new ArgumentException("Argument 'items' must conatain at least 1 value.", "items");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_CommandIndex++.ToString("d5"));
		if (uid)
		{
			stringBuilder.Append(" UID");
		}
		stringBuilder.Append(" FETCH " + seqSet.ToSequenceSetString() + " (");
		for (int i = 0; i < items.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(items[i].ToString());
		}
		stringBuilder.Append(")\r\n");
		SendCommand(stringBuilder.ToString());
		IMAP_r_ServerStatus iMAP_r_ServerStatus = ReadResponse(null, null, null, null, null, null, null, null, null, null, null, null, handler, null);
		if (!iMAP_r_ServerStatus.ResponseCode.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
		{
			throw new IMAP_ClientException(iMAP_r_ServerStatus.ResponseCode, iMAP_r_ServerStatus.ResponseText);
		}
	}

	[Obsolete("Use method StoreMessage(string folder,IMAP_t_MsgFlags flags,DateTime internalDate,Stream message,int count) instead.")]
	public void StoreMessage(string folder, IMAP_MessageFlags flags, DateTime internalDate, Stream message, int count)
	{
		StoreMessage(folder, IMAP_Utils.MessageFlagsToStringArray(flags), internalDate, message, count);
	}

	[Obsolete("Use method public void StoreMessageFlags(bool uid,IMAP_t_SeqSet seqSet,IMAP_Flags_SetType setType,IMAP_t_MsgFlags flags) instead.")]
	public void StoreMessageFlags(bool uid, IMAP_SequenceSet seqSet, IMAP_Flags_SetType setType, string[] flags)
	{
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (flags == null)
		{
			throw new ArgumentNullException("flags");
		}
		StoreMessageFlags(uid, IMAP_t_SeqSet.Parse(seqSet.ToSequenceSetString()), setType, new IMAP_t_MsgFlags(flags));
	}

	[Obsolete("Use method public void StoreMessageFlags(bool uid,IMAP_t_SeqSet seqSet,IMAP_Flags_SetType setType,IMAP_t_MsgFlags flags) instead.")]
	public void StoreMessageFlags(bool uid, IMAP_SequenceSet seqSet, IMAP_Flags_SetType setType, IMAP_MessageFlags flags)
	{
		StoreMessageFlags(uid, seqSet, setType, IMAP_Utils.MessageFlagsToStringArray(flags));
	}

	[Obsolete("Use method 'CopyMessages(bool uid,IMAP_t_SeqSet seqSet,string targetFolder)' instead.")]
	public void CopyMessages(bool uid, IMAP_SequenceSet seqSet, string targetFolder)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (targetFolder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (targetFolder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		CopyMessages(uid, IMAP_t_SeqSet.Parse(seqSet.ToSequenceSetString()), targetFolder);
	}

	[Obsolete("Use method 'MoveMessages(bool uid,IMAP_t_SeqSet seqSet,string targetFolder,bool expunge)' instead.")]
	public void MoveMessages(bool uid, IMAP_SequenceSet seqSet, string targetFolder, bool expunge)
	{
		if (seqSet == null)
		{
			throw new ArgumentNullException("seqSet");
		}
		if (targetFolder == null)
		{
			throw new ArgumentNullException("folder");
		}
		if (targetFolder == string.Empty)
		{
			throw new ArgumentException("Argument 'folder' value must be specified.", "folder");
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pSelectedFolder == null)
		{
			throw new InvalidOperationException("Not selected state, you need to select some folder first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		MoveMessages(uid, IMAP_t_SeqSet.Parse(seqSet.ToSequenceSetString()), targetFolder, expunge);
	}

	[Obsolete("Use method 'GetQuota' instead.")]
	public IMAP_r_u_Quota[] GetFolderQuota(string quotaRootName)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("Not connected, you need to connect first.");
		}
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("Not authenticated, you need to authenticate first.");
		}
		if (m_pIdle != null)
		{
			throw new InvalidOperationException("This command is not valid in IDLE state, you need stop idling before calling this command.");
		}
		if (quotaRootName == null)
		{
			throw new ArgumentNullException("quotaRootName");
		}
		List<IMAP_r_u_Quota> retVal = new List<IMAP_r_u_Quota>();
		EventHandler<EventArgs<IMAP_r_u>> callback = delegate(object sender, EventArgs<IMAP_r_u> e)
		{
			if (e.Value is IMAP_r_u_Quota)
			{
				retVal.Add((IMAP_r_u_Quota)e.Value);
			}
		};
		using (GetQuotaAsyncOP getQuotaAsyncOP = new GetQuotaAsyncOP(quotaRootName, callback))
		{
			ManualResetEvent wait = new ManualResetEvent(initialState: false);
			try
			{
				getQuotaAsyncOP.CompletedAsync += delegate
				{
					wait.Set();
				};
				if (!GetQuotaAsync(getQuotaAsyncOP))
				{
					wait.Set();
				}
				wait.WaitOne();
				if (getQuotaAsyncOP.Error != null)
				{
					throw getQuotaAsyncOP.Error;
				}
			}
			finally
			{
				if (wait != null)
				{
					((IDisposable)wait).Dispose();
				}
			}
		}
		return retVal.ToArray();
	}

	[Obsolete("deprecated")]
	private string ReadStringLiteral(int count)
	{
		string result = TcpStream.ReadFixedCountString(count);
		LogAddRead(count, "Readed string-literal " + count + " bytes.");
		return result;
	}

	[Obsolete("deprecated")]
	private void ReadStringLiteral(int count, Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		TcpStream.ReadFixedCount(stream, count);
		LogAddRead(count, "Readed string-literal " + count + " bytes.");
	}

	[Obsolete("Deprecated.")]
	private void SendCommand(string command)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(command);
		TcpStream.Write(bytes, 0, bytes.Length);
		LogAddWrite(command.TrimEnd().Length, command.TrimEnd());
	}

	[Obsolete("deprecated")]
	private IMAP_r_ServerStatus ReadFinalResponse(EventHandler<EventArgs<IMAP_r_u>> callback)
	{
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using ReadFinalResponseAsyncOP readFinalResponseAsyncOP = new ReadFinalResponseAsyncOP(callback);
		readFinalResponseAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!ReadFinalResponseAsync(readFinalResponseAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (readFinalResponseAsyncOP.Error != null)
		{
			throw readFinalResponseAsyncOP.Error;
		}
		return readFinalResponseAsyncOP.FinalResponse;
	}
}
