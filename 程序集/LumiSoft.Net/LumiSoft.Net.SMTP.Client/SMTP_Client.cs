using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.DNS;
using LumiSoft.Net.DNS.Client;
using LumiSoft.Net.IO;
using LumiSoft.Net.Mail;
using LumiSoft.Net.Mime;
using LumiSoft.Net.MIME;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.SMTP.Client;

public class SMTP_Client : TCP_Client
{
	public class EhloHeloAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private string m_HostName;

		private SMTP_Client m_pSmtpClient;

		private SMTP_t_ReplyLine[] m_pReplyLines;

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

		public SMTP_t_ReplyLine[] ReplyLines
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'ReplyLines' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_pReplyLines;
			}
		}

		public event EventHandler<EventArgs<EhloHeloAsyncOP>> CompletedAsync;

		public EhloHeloAsyncOP(string hostName)
		{
			if (hostName == null)
			{
				throw new ArgumentNullException("hostName");
			}
			if (hostName == string.Empty)
			{
				throw new ArgumentException("Argument 'hostName' value must be specified.", "hostName");
			}
			m_HostName = hostName;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_HostName = null;
				m_pSmtpClient = null;
				m_pReplyLines = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("EHLO " + m_HostName + "\r\n");
				m_pSmtpClient.LogAddWrite(bytes.Length, "EHLO " + m_HostName);
				m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, EhloCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
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

		private void EhloCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					EhloReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					EhloReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void EhloReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pReplyLines = op.ReplyLines;
					if (m_pReplyLines[0].ReplyCode == 250)
					{
						m_pSmtpClient.m_RemoteHostName = m_pReplyLines[0].Text.Split(new char[1] { ' ' }, 2)[0];
						m_pSmtpClient.m_IsEsmtpSupported = true;
						List<string> list = new List<string>();
						SMTP_t_ReplyLine[] pReplyLines = m_pReplyLines;
						foreach (SMTP_t_ReplyLine sMTP_t_ReplyLine in pReplyLines)
						{
							list.Add(sMTP_t_ReplyLine.Text);
						}
						m_pSmtpClient.m_pEsmtpFeatures = list;
						SetState(AsyncOP_State.Completed);
					}
					else
					{
						m_pSmtpClient.LogAddText("EHLO failed, will try HELO.");
						byte[] bytes = Encoding.UTF8.GetBytes("HELO " + m_HostName + "\r\n");
						m_pSmtpClient.LogAddWrite(bytes.Length, "HELO " + m_HostName);
						m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, HeloCommandSendingCompleted, null);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void HeloCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					HeloReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					HeloReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void HeloReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else
				{
					m_pReplyLines = op.ReplyLines;
					if (m_pReplyLines[0].ReplyCode == 250)
					{
						m_pSmtpClient.m_RemoteHostName = m_pReplyLines[0].Text.Split(new char[1] { ' ' }, 2)[0];
						m_pSmtpClient.m_IsEsmtpSupported = true;
						List<string> list = new List<string>();
						SMTP_t_ReplyLine[] pReplyLines = m_pReplyLines;
						foreach (SMTP_t_ReplyLine sMTP_t_ReplyLine in pReplyLines)
						{
							list.Add(sMTP_t_ReplyLine.Text);
						}
						m_pSmtpClient.m_pEsmtpFeatures = list;
					}
					else
					{
						m_pException = new SMTP_ClientException(op.ReplyLines);
						m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			op.Dispose();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<EhloHeloAsyncOP>(this));
			}
		}
	}

	public class StartTlsAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private RemoteCertificateValidationCallback m_pCertCallback;

		private SMTP_Client m_pSmtpClient;

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

		public event EventHandler<EventArgs<StartTlsAsyncOP>> CompletedAsync;

		public StartTlsAsyncOP(RemoteCertificateValidationCallback certCallback)
		{
			m_pCertCallback = certCallback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pCertCallback = null;
				m_pSmtpClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("STARTTLS\r\n");
				m_pSmtpClient.LogAddWrite(bytes.Length, "STARTTLS");
				m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, StartTlsCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
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

		private void StartTlsCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					StartTlsReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					StartTlsReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void StartTlsReadResponseCompleted(ReadResponseAsyncOP op)
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
				else if (op.ReplyLines[0].ReplyCode == 220)
				{
					m_pSmtpClient.LogAddText("Starting TLS handshake.");
					SwitchToSecureAsyncOP switchSecureOP = new SwitchToSecureAsyncOP(m_pCertCallback);
					switchSecureOP.CompletedAsync += delegate
					{
						SwitchToSecureCompleted(switchSecureOP);
					};
					if (!m_pSmtpClient.SwitchToSecureAsync(switchSecureOP))
					{
						SwitchToSecureCompleted(switchSecureOP);
					}
				}
				else
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			op.Dispose();
			if (m_pException != null)
			{
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void SwitchToSecureCompleted(SwitchToSecureAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else
				{
					m_pSmtpClient.LogAddText("TLS handshake completed sucessfully.");
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			op.Dispose();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<StartTlsAsyncOP>(this));
			}
		}
	}

	public class AuthAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private SMTP_Client m_pSmtpClient;

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

		public event EventHandler<EventArgs<AuthAsyncOP>> CompletedAsync;

		public AuthAsyncOP(AUTH_SASL_Client sasl)
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
				m_pSmtpClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				if (m_pSASL.SupportsInitialResponse)
				{
					byte[] bytes = Encoding.UTF8.GetBytes("AUTH " + m_pSASL.Name + " " + Convert.ToBase64String(m_pSASL.Continue(null)) + "\r\n");
					m_pSmtpClient.LogAddWrite(bytes.Length, Encoding.UTF8.GetString(bytes).TrimEnd());
					m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, AuthCommandSendingCompleted, null);
				}
				else
				{
					byte[] bytes2 = Encoding.UTF8.GetBytes("AUTH " + m_pSASL.Name + "\r\n");
					m_pSmtpClient.LogAddWrite(bytes2.Length, "AUTH " + m_pSASL.Name);
					m_pSmtpClient.TcpStream.BeginWrite(bytes2, 0, bytes2.Length, AuthCommandSendingCompleted, null);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
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

		private void AuthCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					AuthReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					AuthReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void AuthReadResponseCompleted(ReadResponseAsyncOP op)
		{
			try
			{
				if (op.ReplyLines[0].ReplyCode == 334)
				{
					byte[] serverResponse = Convert.FromBase64String(op.ReplyLines[0].Text);
					byte[] inArray = m_pSASL.Continue(serverResponse);
					byte[] bytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(inArray) + "\r\n");
					m_pSmtpClient.LogAddWrite(bytes.Length, Convert.ToBase64String(inArray));
					m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, AuthCommandSendingCompleted, null);
				}
				else if (op.ReplyLines[0].ReplyCode == 235)
				{
					m_pSmtpClient.m_pAuthdUserIdentity = new GenericIdentity(m_pSASL.UserName, m_pSASL.Name);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<AuthAsyncOP>(this));
			}
		}
	}

	public class MailFromAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private string m_MailFrom;

		private long m_MessageSize = -1L;

		private SMTP_DSN_Ret m_DsnRet;

		private string m_EnvID;

		private SMTP_Client m_pSmtpClient;

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

		public event EventHandler<EventArgs<MailFromAsyncOP>> CompletedAsync;

		public MailFromAsyncOP(string from, long messageSize)
			: this(from, messageSize, SMTP_DSN_Ret.NotSpecified, null)
		{
		}

		public MailFromAsyncOP(string from, long messageSize, SMTP_DSN_Ret ret, string envid)
		{
			m_MailFrom = from;
			m_MessageSize = messageSize;
			m_DsnRet = ret;
			m_EnvID = envid;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_MailFrom = null;
				m_EnvID = null;
				m_pSmtpClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				bool flag = false;
				string[] esmtpFeatures = m_pSmtpClient.EsmtpFeatures;
				for (int i = 0; i < esmtpFeatures.Length; i++)
				{
					if (esmtpFeatures[i].ToLower().StartsWith("size "))
					{
						flag = true;
						break;
					}
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("MAIL FROM:<" + m_MailFrom + ">");
				if (flag && m_MessageSize > 0)
				{
					stringBuilder.Append(" SIZE=" + m_MessageSize);
				}
				if (m_DsnRet == SMTP_DSN_Ret.FullMessage)
				{
					stringBuilder.Append(" RET=FULL");
				}
				else if (m_DsnRet == SMTP_DSN_Ret.Headers)
				{
					stringBuilder.Append(" RET=HDRS");
				}
				if (!string.IsNullOrEmpty(m_EnvID))
				{
					stringBuilder.Append(" ENVID=" + m_EnvID);
				}
				byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString() + "\r\n");
				m_pSmtpClient.LogAddWrite(bytes.Length, stringBuilder.ToString());
				m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, MailCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
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

		private void MailCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					MailReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					MailReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void MailReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else if (op.ReplyLines[0].ReplyCode == 250)
				{
					m_pSmtpClient.m_MailFrom = m_MailFrom;
				}
				else
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			op.Dispose();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<MailFromAsyncOP>(this));
			}
		}
	}

	public class RcptToAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private string m_To;

		private SMTP_DSN_Notify m_DsnNotify;

		private string m_ORcpt;

		private SMTP_Client m_pSmtpClient;

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

		public event EventHandler<EventArgs<RcptToAsyncOP>> CompletedAsync;

		public RcptToAsyncOP(string to)
			: this(to, SMTP_DSN_Notify.NotSpecified, null)
		{
		}

		public RcptToAsyncOP(string to, SMTP_DSN_Notify notify, string orcpt)
		{
			if (to == null)
			{
				throw new ArgumentNullException("to");
			}
			if (to == string.Empty)
			{
				throw new ArgumentException("Argument 'to' value must be specified.", "to");
			}
			m_To = to;
			m_DsnNotify = notify;
			m_ORcpt = orcpt;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_To = null;
				m_ORcpt = null;
				m_pSmtpClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("RCPT TO:<" + m_To + ">");
				if (m_DsnNotify != 0)
				{
					if (m_DsnNotify == SMTP_DSN_Notify.Never)
					{
						stringBuilder.Append(" NOTIFY=NEVER");
					}
					else
					{
						bool flag = true;
						if ((m_DsnNotify & SMTP_DSN_Notify.Delay) != 0)
						{
							stringBuilder.Append(" NOTIFY=DELAY");
							flag = false;
						}
						if ((m_DsnNotify & SMTP_DSN_Notify.Failure) != 0)
						{
							if (flag)
							{
								stringBuilder.Append(" NOTIFY=FAILURE");
							}
							else
							{
								stringBuilder.Append(",FAILURE");
							}
							flag = false;
						}
						if ((m_DsnNotify & SMTP_DSN_Notify.Success) != 0)
						{
							if (flag)
							{
								stringBuilder.Append(" NOTIFY=SUCCESS");
							}
							else
							{
								stringBuilder.Append(",SUCCESS");
							}
							flag = false;
						}
					}
				}
				if (!string.IsNullOrEmpty(m_ORcpt))
				{
					stringBuilder.Append(" ORCPT=" + m_ORcpt);
				}
				byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString() + "\r\n");
				m_pSmtpClient.LogAddWrite(bytes.Length, stringBuilder.ToString());
				m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, RcptCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
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

		private void RcptCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					RcptReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					RcptReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void RcptReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else if (op.ReplyLines[0].ReplyCode == 250)
				{
					if (!m_pSmtpClient.m_pRecipients.Contains(m_To))
					{
						m_pSmtpClient.m_pRecipients.Add(m_To);
					}
				}
				else
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
			}
			op.Dispose();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<RcptToAsyncOP>(this));
			}
		}
	}

	public class SendMessageAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private Stream m_pStream;

		private bool m_UseBdat;

		private SMTP_Client m_pSmtpClient;

		private byte[] m_pBdatBuffer;

		private int m_BdatBytesInBuffer;

		private byte[] m_BdatSendBuffer;

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

		public event EventHandler<EventArgs<SendMessageAsyncOP>> CompletedAsync;

		public SendMessageAsyncOP(Stream stream, bool useBdatIfPossibe)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = stream;
			m_UseBdat = useBdatIfPossibe;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pStream = null;
				m_pSmtpClient = null;
				m_pBdatBuffer = null;
				m_BdatSendBuffer = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				bool flag = false;
				string[] esmtpFeatures = m_pSmtpClient.EsmtpFeatures;
				for (int i = 0; i < esmtpFeatures.Length; i++)
				{
					if (esmtpFeatures[i].ToUpper() == SMTP_ServiceExtensions.CHUNKING)
					{
						flag = true;
						break;
					}
				}
				if (flag && m_UseBdat)
				{
					m_pBdatBuffer = new byte[64000];
					m_BdatSendBuffer = new byte[64100];
					m_pStream.BeginRead(m_pBdatBuffer, 0, m_pBdatBuffer.Length, BdatChunkReadingCompleted, null);
				}
				else
				{
					byte[] bytes = Encoding.UTF8.GetBytes("DATA\r\n");
					m_pSmtpClient.LogAddWrite(bytes.Length, "DATA");
					m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, DataCommandSendingCompleted, null);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
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

		private void BdatChunkReadingCompleted(IAsyncResult ar)
		{
			try
			{
				m_BdatBytesInBuffer = m_pStream.EndRead(ar);
				if (m_BdatBytesInBuffer > 0)
				{
					byte[] bytes = Encoding.UTF8.GetBytes("BDAT " + m_BdatBytesInBuffer + "\r\n");
					m_pSmtpClient.LogAddWrite(bytes.Length, "BDAT " + m_BdatBytesInBuffer);
					m_pSmtpClient.LogAddWrite(m_BdatBytesInBuffer, "<BDAT data-chunk of " + m_BdatBytesInBuffer + " bytes>");
					Array.Copy(bytes, m_BdatSendBuffer, bytes.Length);
					Array.Copy(m_pBdatBuffer, 0, m_BdatSendBuffer, bytes.Length, m_BdatBytesInBuffer);
					m_pSmtpClient.TcpStream.BeginWrite(m_BdatSendBuffer, 0, bytes.Length + m_BdatBytesInBuffer, BdatCommandSendingCompleted, null);
				}
				else
				{
					byte[] bytes2 = Encoding.UTF8.GetBytes("BDAT 0 LAST\r\n");
					m_pSmtpClient.LogAddWrite(bytes2.Length, "BDAT 0 LAST");
					m_pSmtpClient.TcpStream.BeginWrite(bytes2, 0, bytes2.Length, BdatCommandSendingCompleted, null);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void BdatCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					BdatReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					BdatReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void BdatReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
				}
				else if (op.ReplyLines[0].ReplyCode == 250)
				{
					if (m_BdatBytesInBuffer == 0)
					{
						SetState(AsyncOP_State.Completed);
						return;
					}
					m_pStream.BeginRead(m_pBdatBuffer, 0, m_pBdatBuffer.Length, BdatChunkReadingCompleted, null);
				}
				else
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void DataCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					DataReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					DataReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void DataReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
				}
				else if (op.ReplyLines[0].ReplyCode == 354)
				{
					SmartStream.WritePeriodTerminatedAsyncOP sendMsgOP = new SmartStream.WritePeriodTerminatedAsyncOP(m_pStream);
					sendMsgOP.CompletedAsync += delegate
					{
						DataMsgSendingCompleted(sendMsgOP);
					};
					if (!m_pSmtpClient.TcpStream.WritePeriodTerminatedAsync(sendMsgOP))
					{
						DataMsgSendingCompleted(sendMsgOP);
					}
				}
				else
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void DataMsgSendingCompleted(SmartStream.WritePeriodTerminatedAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pSmtpClient.LogAddWrite(op.BytesWritten, "Sent message " + op.BytesWritten + " bytes.");
					ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
					readResponseOP.CompletedAsync += delegate
					{
						DataReadFinalResponseCompleted(readResponseOP);
					};
					if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
					{
						DataReadFinalResponseCompleted(readResponseOP);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void DataReadFinalResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					if (op.ReplyLines[0].ReplyCode < 200 || op.ReplyLines[0].ReplyCode > 299)
					{
						m_pException = new SMTP_ClientException(op.ReplyLines);
					}
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<SendMessageAsyncOP>(this));
			}
		}
	}

	public class RsetAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private SMTP_Client m_pSmtpClient;

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

		public event EventHandler<EventArgs<RsetAsyncOP>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pSmtpClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("RSET\r\n");
				m_pSmtpClient.LogAddWrite(bytes.Length, "RSET");
				m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, RsetCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
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

		private void RsetCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					RsetReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					RsetReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void RsetReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else if (op.ReplyLines[0].ReplyCode != 250)
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
			}
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<RsetAsyncOP>(this));
			}
		}
	}

	public class NoopAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private SMTP_Client m_pSmtpClient;

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

		public event EventHandler<EventArgs<NoopAsyncOP>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pSmtpClient = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("NOOP\r\n");
				m_pSmtpClient.LogAddWrite(bytes.Length, "NOOP");
				m_pSmtpClient.TcpStream.BeginWrite(bytes, 0, bytes.Length, NoopCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
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

		private void NoopCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pSmtpClient.TcpStream.EndWrite(ar);
				ReadResponseAsyncOP readResponseOP = new ReadResponseAsyncOP();
				readResponseOP.CompletedAsync += delegate
				{
					NoopReadResponseCompleted(readResponseOP);
				};
				if (!m_pSmtpClient.ReadResponseAsync(readResponseOP))
				{
					NoopReadResponseCompleted(readResponseOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
				SetState(AsyncOP_State.Completed);
			}
		}

		private void NoopReadResponseCompleted(ReadResponseAsyncOP op)
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
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else if (op.ReplyLines[0].ReplyCode != 250)
				{
					m_pException = new SMTP_ClientException(op.ReplyLines);
					m_pSmtpClient.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				if (m_pSmtpClient != null)
				{
					m_pSmtpClient.LogAddException("Exception: " + ex.Message, ex);
				}
			}
			op.Dispose();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<NoopAsyncOP>(this));
			}
		}
	}

	private class ReadResponseAsyncOP : IDisposable, IAsyncOP
	{
		private AsyncOP_State m_State;

		private Exception m_pException;

		private SMTP_Client m_pSmtpClient;

		private List<SMTP_t_ReplyLine> m_pReplyLines;

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

		public SMTP_t_ReplyLine[] ReplyLines
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'ReplyLines' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_pReplyLines.ToArray();
			}
		}

		public event EventHandler<EventArgs<ReadResponseAsyncOP>> CompletedAsync;

		public ReadResponseAsyncOP()
		{
			m_pReplyLines = new List<SMTP_t_ReplyLine>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pSmtpClient = null;
				m_pReplyLines = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SMTP_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pSmtpClient = owner;
			try
			{
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					try
					{
						if (!ReadLineCompleted(op))
						{
							SetState(AsyncOP_State.Completed);
							OnCompletedAsync();
						}
						else
						{
							while (owner.TcpStream.ReadLine(op, async: true))
							{
								if (!ReadLineCompleted(op))
								{
									SetState(AsyncOP_State.Completed);
									OnCompletedAsync();
									break;
								}
							}
						}
					}
					catch (Exception pException2)
					{
						m_pException = pException2;
						SetState(AsyncOP_State.Completed);
						OnCompletedAsync();
					}
				};
				while (owner.TcpStream.ReadLine(op, async: true))
				{
					if (!ReadLineCompleted(op))
					{
						SetState(AsyncOP_State.Completed);
						return false;
					}
				}
				return true;
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
				return false;
			}
		}

		private void SetState(AsyncOP_State state)
		{
			m_State = state;
		}

		private bool ReadLineCompleted(SmartStream.ReadLineAsyncOP op)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}
			try
			{
				if (op.Error == null)
				{
					m_pSmtpClient.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					SMTP_t_ReplyLine sMTP_t_ReplyLine = SMTP_t_ReplyLine.Parse(op.LineUtf8);
					m_pReplyLines.Add(sMTP_t_ReplyLine);
					return !sMTP_t_ReplyLine.IsLastLine;
				}
				m_pException = op.Error;
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			return false;
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ReadResponseAsyncOP>(this));
			}
		}
	}

	[Obsolete("Use method 'AuthAsync' instead.")]
	private delegate void AuthenticateDelegate(string userName, string password);

	private delegate void NoopDelegate();

	private delegate void StartTLSDelegate();

	private delegate void RcptToDelegate(string to, SMTP_DSN_Notify notify, string orcpt);

	private delegate void MailFromDelegate(string from, long messageSize, SMTP_DSN_Ret ret, string envid);

	private delegate void ResetDelegate();

	private delegate void SendMessageDelegate(Stream message);

	private delegate string[] GetDomainHostsDelegate(string domain);

	private string m_LocalHostName;

	private string m_RemoteHostName;

	private string m_GreetingText = "";

	private bool m_IsEsmtpSupported;

	private List<string> m_pEsmtpFeatures;

	private string m_MailFrom;

	private List<string> m_pRecipients;

	private GenericIdentity m_pAuthdUserIdentity;

	private bool m_BdatEnabled = true;

	public string LocalHostName
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_LocalHostName;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (IsConnected)
			{
				throw new InvalidOperationException("Property LocalHostName is available only when SMTP client is not connected.");
			}
			m_LocalHostName = value;
		}
	}

	public string RemoteHostName
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
			return m_RemoteHostName;
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

	public bool IsEsmtpSupported
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
			return m_IsEsmtpSupported;
		}
	}

	public string[] EsmtpFeatures
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
			return m_pEsmtpFeatures.ToArray();
		}
	}

	public string[] SaslAuthMethods
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
			string[] esmtpFeatures = EsmtpFeatures;
			foreach (string text in esmtpFeatures)
			{
				if (string.Equals(text.Split(' ')[0], SMTP_ServiceExtensions.AUTH, StringComparison.InvariantCultureIgnoreCase))
				{
					return text.Substring(4).Trim().Split(' ');
				}
			}
			return new string[0];
		}
	}

	public long MaxAllowedMessageSize
	{
		get
		{
			try
			{
				string[] esmtpFeatures = EsmtpFeatures;
				foreach (string text in esmtpFeatures)
				{
					if (text.ToUpper().StartsWith(SMTP_ServiceExtensions.SIZE))
					{
						return Convert.ToInt64(text.Split(' ')[1]);
					}
				}
			}
			catch
			{
			}
			return 0L;
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
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pAuthdUserIdentity;
		}
	}

	[Obsolete("Use method SendMessage argument 'useBdatIfPossibe' instead.")]
	public bool BdatEnabled
	{
		get
		{
			return m_BdatEnabled;
		}
		set
		{
			m_BdatEnabled = value;
		}
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Disconnect()
	{
		Disconnect(sendQuit: true);
	}

	public void Disconnect(bool sendQuit)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("SMTP client is not connected.");
		}
		try
		{
			if (sendQuit)
			{
				WriteLine("QUIT");
				ReadLine();
			}
		}
		catch
		{
		}
		m_LocalHostName = null;
		m_RemoteHostName = null;
		m_GreetingText = "";
		m_IsEsmtpSupported = false;
		m_pEsmtpFeatures = null;
		m_MailFrom = null;
		m_pRecipients = null;
		m_pAuthdUserIdentity = null;
		try
		{
			base.Disconnect();
		}
		catch
		{
		}
	}

	public void EhloHelo(string hostName)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (hostName == string.Empty)
		{
			throw new ArgumentException("Argument 'hostName' value must be specified.", "hostName");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using EhloHeloAsyncOP ehloHeloAsyncOP = new EhloHeloAsyncOP(hostName);
		ehloHeloAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!EhloHeloAsync(ehloHeloAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (ehloHeloAsyncOP.Error != null)
		{
			throw ehloHeloAsyncOP.Error;
		}
	}

	public bool EhloHeloAsync(EhloHeloAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
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

	public void StartTLS()
	{
		StartTLS(null);
	}

	public void StartTLS(RemoteCertificateValidationCallback certCallback)
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
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using StartTlsAsyncOP startTlsAsyncOP = new StartTlsAsyncOP(certCallback);
		startTlsAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!StartTlsAsync(startTlsAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (startTlsAsyncOP.Error != null)
		{
			throw startTlsAsyncOP.Error;
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

	public AUTH_SASL_Client AuthGetStrongestMethod(string userName, string password)
	{
		return AuthGetStrongestMethod(null, userName, password);
	}

	public AUTH_SASL_Client AuthGetStrongestMethod(string domain, string userName, string password)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (password == null)
		{
			throw new ArgumentNullException("userName");
		}
		List<string> list = new List<string>(SaslAuthMethods);
		if (list.Count == 0)
		{
			throw new NotSupportedException("SMTP server does not support authentication.");
		}
		if (list.Contains("NTLM") && (!string.IsNullOrEmpty(domain) || userName.IndexOf('\\') > -1))
		{
			if (!string.IsNullOrEmpty(domain))
			{
				return new AUTH_SASL_Client_Ntlm(domain, userName, password);
			}
			string[] array = userName.Split('\\');
			return new AUTH_SASL_Client_Ntlm(array[0], array[1], password);
		}
		if (list.Contains("DIGEST-MD5"))
		{
			return new AUTH_SASL_Client_DigestMd5("SMTP", RemoteEndPoint.Address.ToString(), userName, password);
		}
		if (list.Contains("CRAM-MD5"))
		{
			return new AUTH_SASL_Client_CramMd5(userName, password);
		}
		if (list.Contains("LOGIN"))
		{
			return new AUTH_SASL_Client_Login(userName, password);
		}
		if (list.Contains("PLAIN"))
		{
			return new AUTH_SASL_Client_Plain(userName, password);
		}
		throw new NotSupportedException("We don't support any of the SMTP server authentication methods.");
	}

	public void Auth(AUTH_SASL_Client sasl)
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
		if (sasl == null)
		{
			throw new ArgumentNullException("sasl");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using AuthAsyncOP authAsyncOP = new AuthAsyncOP(sasl);
		authAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!AuthAsync(authAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (authAsyncOP.Error != null)
		{
			throw authAsyncOP.Error;
		}
	}

	public bool AuthAsync(AuthAsyncOP op)
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

	public void MailFrom(string from, long messageSize)
	{
		MailFrom(from, messageSize, SMTP_DSN_Ret.NotSpecified, null);
	}

	public void MailFrom(string from, long messageSize, SMTP_DSN_Ret ret, string envid)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using MailFromAsyncOP mailFromAsyncOP = new MailFromAsyncOP(from, messageSize, ret, envid);
		mailFromAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!MailFromAsync(mailFromAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (mailFromAsyncOP.Error != null)
		{
			throw mailFromAsyncOP.Error;
		}
	}

	public bool MailFromAsync(MailFromAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
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

	public void RcptTo(string to)
	{
		RcptTo(to, SMTP_DSN_Notify.NotSpecified, null);
	}

	public void RcptTo(string to, SMTP_DSN_Notify notify, string orcpt)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using RcptToAsyncOP rcptToAsyncOP = new RcptToAsyncOP(to, notify, orcpt);
		rcptToAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!RcptToAsync(rcptToAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (rcptToAsyncOP.Error != null)
		{
			throw rcptToAsyncOP.Error;
		}
	}

	public bool RcptToAsync(RcptToAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
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

	public void SendMessage(Stream stream)
	{
		SendMessage(stream, useBdatIfPossibe: false);
	}

	public void SendMessage(Stream stream, bool useBdatIfPossibe)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using SendMessageAsyncOP sendMessageAsyncOP = new SendMessageAsyncOP(stream, useBdatIfPossibe);
		sendMessageAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!SendMessageAsync(sendMessageAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (sendMessageAsyncOP.Error != null)
		{
			throw sendMessageAsyncOP.Error;
		}
	}

	public bool SendMessageAsync(SendMessageAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
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

	public void Rset()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using RsetAsyncOP rsetAsyncOP = new RsetAsyncOP();
		rsetAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!RsetAsync(rsetAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (rsetAsyncOP.Error != null)
		{
			throw rsetAsyncOP.Error;
		}
	}

	public bool RsetAsync(RsetAsyncOP op)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
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
			throw new InvalidOperationException("You must connect first.");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		using NoopAsyncOP noopAsyncOP = new NoopAsyncOP();
		noopAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!NoopAsync(noopAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (noopAsyncOP.Error != null)
		{
			throw noopAsyncOP.Error;
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
		ReadResponseAsyncOP readGreetingOP = new ReadResponseAsyncOP();
		readGreetingOP.CompletedAsync += delegate
		{
			ReadServerGreetingCompleted(readGreetingOP, callback);
		};
		if (!ReadResponseAsync(readGreetingOP))
		{
			ReadServerGreetingCompleted(readGreetingOP, callback);
		}
	}

	private void ReadServerGreetingCompleted(ReadResponseAsyncOP op, CompleteConnectCallback connectCallback)
	{
		Exception error = null;
		try
		{
			if (op.Error != null)
			{
				error = op.Error;
			}
			else if (op.ReplyLines[0].ReplyCode == 220)
			{
				StringBuilder stringBuilder = new StringBuilder();
				SMTP_t_ReplyLine[] replyLines = op.ReplyLines;
				foreach (SMTP_t_ReplyLine sMTP_t_ReplyLine in replyLines)
				{
					stringBuilder.AppendLine(sMTP_t_ReplyLine.Text);
				}
				m_GreetingText = stringBuilder.ToString();
				m_pEsmtpFeatures = new List<string>();
				m_pRecipients = new List<string>();
			}
			else
			{
				error = new SMTP_ClientException(op.ReplyLines);
			}
		}
		catch (Exception ex)
		{
			error = ex;
		}
		connectCallback(error);
	}

	private bool ReadResponseAsync(ReadResponseAsyncOP op)
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

	private bool SupportsCapability(string capability)
	{
		if (capability == null)
		{
			throw new ArgumentNullException("capability");
		}
		if (m_pEsmtpFeatures == null)
		{
			return false;
		}
		foreach (string pEsmtpFeature in m_pEsmtpFeatures)
		{
			if (string.Equals(pEsmtpFeature, capability, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	[Obsolete("Use QuickSend(Mail_Message) instead")]
	public static void QuickSend(LumiSoft.Net.Mime.Mime message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		string from = "";
		if (message.MainEntity.From != null && message.MainEntity.From.Count > 0)
		{
			from = ((MailboxAddress)message.MainEntity.From[0]).EmailAddress;
		}
		List<string> list = new List<string>();
		if (message.MainEntity.To != null)
		{
			MailboxAddress[] mailboxes = message.MainEntity.To.Mailboxes;
			foreach (MailboxAddress mailboxAddress in mailboxes)
			{
				list.Add(mailboxAddress.EmailAddress);
			}
		}
		if (message.MainEntity.Cc != null)
		{
			MailboxAddress[] mailboxes = message.MainEntity.Cc.Mailboxes;
			foreach (MailboxAddress mailboxAddress2 in mailboxes)
			{
				list.Add(mailboxAddress2.EmailAddress);
			}
		}
		if (message.MainEntity.Bcc != null)
		{
			MailboxAddress[] mailboxes = message.MainEntity.Bcc.Mailboxes;
			foreach (MailboxAddress mailboxAddress3 in mailboxes)
			{
				list.Add(mailboxAddress3.EmailAddress);
			}
			message.MainEntity.Bcc.Clear();
		}
		foreach (string item in list)
		{
			QuickSend(null, from, item, new MemoryStream(message.ToByteData()));
		}
	}

	public static void QuickSend(Mail_Message message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		string from = "";
		if (message.From != null && message.From.Count > 0)
		{
			from = message.From[0].Address;
		}
		List<string> list = new List<string>();
		if (message.To != null)
		{
			Mail_t_Mailbox[] mailboxes = message.To.Mailboxes;
			foreach (Mail_t_Mailbox mail_t_Mailbox in mailboxes)
			{
				list.Add(mail_t_Mailbox.Address);
			}
		}
		if (message.Cc != null)
		{
			Mail_t_Mailbox[] mailboxes = message.Cc.Mailboxes;
			foreach (Mail_t_Mailbox mail_t_Mailbox2 in mailboxes)
			{
				list.Add(mail_t_Mailbox2.Address);
			}
		}
		if (message.Bcc != null)
		{
			Mail_t_Mailbox[] mailboxes = message.Bcc.Mailboxes;
			foreach (Mail_t_Mailbox mail_t_Mailbox3 in mailboxes)
			{
				list.Add(mail_t_Mailbox3.Address);
			}
			message.Bcc.Clear();
		}
		foreach (string item in list)
		{
			MemoryStream memoryStream = new MemoryStream();
			message.ToStream(memoryStream, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.Q, Encoding.UTF8), Encoding.UTF8);
			memoryStream.Position = 0L;
			QuickSend(null, from, item, memoryStream);
		}
	}

	public static void QuickSend(string from, string to, Stream message)
	{
		QuickSend(null, from, to, message);
	}

	public static void QuickSend(string localHost, string from, string to, Stream message)
	{
		if (from == null)
		{
			throw new ArgumentNullException("from");
		}
		if (from != "" && !SMTP_Utils.IsValidAddress(from))
		{
			throw new ArgumentException("Argument 'from' has invalid value.");
		}
		if (to == null)
		{
			throw new ArgumentNullException("to");
		}
		if (to == "")
		{
			throw new ArgumentException("Argument 'to' value must be specified.");
		}
		if (!SMTP_Utils.IsValidAddress(to))
		{
			throw new ArgumentException("Argument 'to' has invalid value.");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		QuickSendSmartHost(localHost, Dns_Client.Static.GetEmailHosts(to)[0].HostName, 25, ssl: false, from, new string[1] { to }, message);
	}

	public static void QuickSendSmartHost(string host, int port, bool ssl, Mail_Message message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		QuickSendSmartHost(null, host, port, ssl, null, null, message);
	}

	public static void QuickSendSmartHost(string localHost, string host, int port, TcpClientSecurity security, string userName, string password, Mail_Message message)
	{
		QuickSendSmartHost(localHost, host, port, security == TcpClientSecurity.SSL, userName, password, message);
	}

	public static void QuickSendSmartHost(string localHost, string host, int port, bool ssl, string userName, string password, Mail_Message message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		string from = "";
		if (message.From != null && message.From.Count > 0)
		{
			from = message.From[0].Address;
		}
		List<string> list = new List<string>();
		if (message.To != null)
		{
			Mail_t_Mailbox[] mailboxes = message.To.Mailboxes;
			foreach (Mail_t_Mailbox mail_t_Mailbox in mailboxes)
			{
				list.Add(mail_t_Mailbox.Address);
			}
		}
		if (message.Cc != null)
		{
			Mail_t_Mailbox[] mailboxes = message.Cc.Mailboxes;
			foreach (Mail_t_Mailbox mail_t_Mailbox2 in mailboxes)
			{
				list.Add(mail_t_Mailbox2.Address);
			}
		}
		if (message.Bcc != null)
		{
			Mail_t_Mailbox[] mailboxes = message.Bcc.Mailboxes;
			foreach (Mail_t_Mailbox mail_t_Mailbox3 in mailboxes)
			{
				list.Add(mail_t_Mailbox3.Address);
			}
			message.Bcc.Clear();
		}
		MemoryStream memoryStream = new MemoryStream();
		message.ToStream(memoryStream, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.Q, Encoding.UTF8), Encoding.UTF8);
		memoryStream.Position = 0L;
		QuickSendSmartHost(localHost, host, port, ssl, userName, password, from, list.ToArray(), memoryStream);
	}

	public static void QuickSendSmartHost(string host, int port, string from, string[] to, Stream message)
	{
		QuickSendSmartHost(null, host, port, ssl: false, null, null, from, to, message);
	}

	public static void QuickSendSmartHost(string host, int port, bool ssl, string from, string[] to, Stream message)
	{
		QuickSendSmartHost(null, host, port, ssl, null, null, from, to, message);
	}

	public static void QuickSendSmartHost(string localHost, string host, int port, bool ssl, string from, string[] to, Stream message)
	{
		QuickSendSmartHost(localHost, host, port, ssl, null, null, from, to, message);
	}

	public static void QuickSendSmartHost(string localHost, string host, int port, bool ssl, string userName, string password, string from, string[] to, Stream message)
	{
		QuickSendSmartHost(localHost, host, port, ssl ? TcpClientSecurity.SSL : TcpClientSecurity.None, userName, password, from, to, message);
	}

	public static void QuickSendSmartHost(string localHost, string host, int port, TcpClientSecurity security, string userName, string password, string from, string[] to, Stream message)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (host == "")
		{
			throw new ArgumentException("Argument 'host' value may not be empty.");
		}
		if (port < 1)
		{
			throw new ArgumentException("Argument 'port' value must be >= 1.");
		}
		if (from == null)
		{
			throw new ArgumentNullException("from");
		}
		if (from != "" && !SMTP_Utils.IsValidAddress(from))
		{
			throw new ArgumentException("Argument 'from' has invalid value.");
		}
		if (to == null)
		{
			throw new ArgumentNullException("to");
		}
		if (to.Length == 0)
		{
			throw new ArgumentException("Argument 'to' must contain at least 1 recipient.");
		}
		string[] array = to;
		foreach (string text in array)
		{
			if (!SMTP_Utils.IsValidAddress(text))
			{
				throw new ArgumentException("Argument 'to' has invalid value '" + text + "'.");
			}
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		using SMTP_Client sMTP_Client = new SMTP_Client();
		sMTP_Client.Connect(host, port, security == TcpClientSecurity.SSL);
		if (security == TcpClientSecurity.TLS || (security == TcpClientSecurity.UseTlsIfSupported && sMTP_Client.SupportsCapability(SMTP_ServiceExtensions.STARTTLS)))
		{
			sMTP_Client.EhloHelo((localHost != null) ? localHost : Dns.GetHostName());
			sMTP_Client.StartTLS();
		}
		sMTP_Client.EhloHelo((localHost != null) ? localHost : Dns.GetHostName());
		if (!string.IsNullOrEmpty(userName))
		{
			sMTP_Client.Auth(sMTP_Client.AuthGetStrongestMethod(userName, password));
		}
		sMTP_Client.MailFrom(from, -1L);
		array = to;
		foreach (string to2 in array)
		{
			sMTP_Client.RcptTo(to2);
		}
		sMTP_Client.SendMessage(message);
	}

	[Obsolete("Use method 'Auth' instead.")]
	public void Authenticate(string userName, string password)
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
			throw new InvalidOperationException("Session is already authenticated.");
		}
		if (string.IsNullOrEmpty(userName))
		{
			throw new ArgumentNullException("userName");
		}
		if (password == null)
		{
			password = "";
		}
		string text = "LOGIN";
		List<string> list = new List<string>(SaslAuthMethods);
		if (list.Contains("DIGEST-MD5"))
		{
			text = "DIGEST-MD5";
		}
		else if (list.Contains("CRAM-MD5"))
		{
			text = "CRAM-MD5";
		}
		switch (text)
		{
		case "LOGIN":
		{
			WriteLine("AUTH LOGIN");
			string text5 = ReadLine();
			if (!text5.StartsWith("334"))
			{
				throw new SMTP_ClientException(text5);
			}
			WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(userName)));
			text5 = ReadLine();
			if (!text5.StartsWith("334"))
			{
				throw new SMTP_ClientException(text5);
			}
			WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(password)));
			text5 = ReadLine();
			if (!text5.StartsWith("235"))
			{
				throw new SMTP_ClientException(text5);
			}
			m_pAuthdUserIdentity = new GenericIdentity(userName, "LOGIN");
			break;
		}
		case "CRAM-MD5":
		{
			WriteLine("AUTH CRAM-MD5");
			string text3 = ReadLine();
			if (!text3.StartsWith("334"))
			{
				throw new SMTP_ClientException(text3);
			}
			string text4 = Net_Utils.ToHex(new HMACMD5(Encoding.ASCII.GetBytes(password)).ComputeHash(Convert.FromBase64String(text3.Split(' ')[1]))).ToLower();
			WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + " " + text4)));
			text3 = ReadLine();
			if (!text3.StartsWith("235"))
			{
				throw new SMTP_ClientException(text3);
			}
			m_pAuthdUserIdentity = new GenericIdentity(userName, "CRAM-MD5");
			break;
		}
		case "DIGEST-MD5":
		{
			WriteLine("AUTH DIGEST-MD5");
			string text2 = ReadLine();
			if (!text2.StartsWith("334"))
			{
				throw new SMTP_ClientException(text2);
			}
			AUTH_SASL_DigestMD5_Challenge aUTH_SASL_DigestMD5_Challenge = AUTH_SASL_DigestMD5_Challenge.Parse(Encoding.Default.GetString(Convert.FromBase64String(text2.Split(' ')[1])));
			AUTH_SASL_DigestMD5_Response aUTH_SASL_DigestMD5_Response = new AUTH_SASL_DigestMD5_Response(aUTH_SASL_DigestMD5_Challenge, aUTH_SASL_DigestMD5_Challenge.Realm[0], userName, password, Guid.NewGuid().ToString().Replace("-", ""), 1, aUTH_SASL_DigestMD5_Challenge.QopOptions[0], "smtp/" + RemoteEndPoint.Address.ToString());
			WriteLine(Convert.ToBase64String(Encoding.Default.GetBytes(aUTH_SASL_DigestMD5_Response.ToResponse())));
			text2 = ReadLine();
			if (!text2.StartsWith("334"))
			{
				throw new SMTP_ClientException(text2);
			}
			if (!string.Equals(Encoding.Default.GetString(Convert.FromBase64String(text2.Split(' ')[1])), aUTH_SASL_DigestMD5_Response.ToRspauthResponse(userName, password), StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("SMTP server 'rspauth' value mismatch.");
			}
			WriteLine("");
			text2 = ReadLine();
			if (!text2.StartsWith("235"))
			{
				throw new SMTP_ClientException(text2);
			}
			m_pAuthdUserIdentity = new GenericIdentity(userName, "DIGEST-MD5");
			break;
		}
		}
	}

	[Obsolete("Use method 'AuthAsync' instead.")]
	public IAsyncResult BeginAuthenticate(string userName, string password, AsyncCallback callback, object state)
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
			throw new InvalidOperationException("Session is already authenticated.");
		}
		AuthenticateDelegate authenticateDelegate = Authenticate;
		AsyncResultState asyncResultState = new AsyncResultState(this, authenticateDelegate, callback, state);
		asyncResultState.SetAsyncResult(authenticateDelegate.BeginInvoke(userName, password, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method 'AuthAsync' instead.")]
	public void EndAuthenticate(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument 'asyncResult' was not returned by a call to the BeginAuthenticate method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginAuthenticate was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is AuthenticateDelegate)
		{
			((AuthenticateDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginAuthenticate method.");
	}

	[Obsolete("Use method 'NoopAsync' instead.")]
	public IAsyncResult BeginNoop(AsyncCallback callback, object state)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		NoopDelegate noopDelegate = Noop;
		AsyncResultState asyncResultState = new AsyncResultState(this, noopDelegate, callback, state);
		asyncResultState.SetAsyncResult(noopDelegate.BeginInvoke(asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method 'NoopAsync' instead.")]
	public void EndNoop(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginNoop method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginNoop was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is NoopDelegate)
		{
			((NoopDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginNoop method.");
	}

	[Obsolete("Use method StartTlsAsync instead.")]
	public IAsyncResult BeginStartTLS(AsyncCallback callback, object state)
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
		StartTLSDelegate startTLSDelegate = StartTLS;
		AsyncResultState asyncResultState = new AsyncResultState(this, startTLSDelegate, callback, state);
		asyncResultState.SetAsyncResult(startTLSDelegate.BeginInvoke(asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method StartTlsAsync instead.")]
	public void EndStartTLS(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginReset was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is StartTLSDelegate)
		{
			((StartTLSDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
	}

	[Obsolete("Use method RcptToAsync instead.")]
	public IAsyncResult BeginRcptTo(string to, AsyncCallback callback, object state)
	{
		return BeginRcptTo(to, SMTP_DSN_Notify.NotSpecified, null, callback, state);
	}

	[Obsolete("Use method RcptToAsync instead.")]
	public IAsyncResult BeginRcptTo(string to, SMTP_DSN_Notify notify, string orcpt, AsyncCallback callback, object state)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		RcptToDelegate rcptToDelegate = RcptTo;
		AsyncResultState asyncResultState = new AsyncResultState(this, rcptToDelegate, callback, state);
		asyncResultState.SetAsyncResult(rcptToDelegate.BeginInvoke(to, notify, orcpt, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method RcptToAsync instead.")]
	public void EndRcptTo(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginReset was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is RcptToDelegate)
		{
			((RcptToDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
	}

	[Obsolete("Use method MailFromAsync instead.")]
	public IAsyncResult BeginMailFrom(string from, long messageSize, AsyncCallback callback, object state)
	{
		return BeginMailFrom(from, messageSize, SMTP_DSN_Ret.NotSpecified, null, callback, state);
	}

	[Obsolete("Use method MailFromAsync instead.")]
	public IAsyncResult BeginMailFrom(string from, long messageSize, SMTP_DSN_Ret ret, string envid, AsyncCallback callback, object state)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		MailFromDelegate mailFromDelegate = MailFrom;
		AsyncResultState asyncResultState = new AsyncResultState(this, mailFromDelegate, callback, state);
		asyncResultState.SetAsyncResult(mailFromDelegate.BeginInvoke(from, (int)messageSize, ret, envid, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method MailFromAsync instead.")]
	public void EndMailFrom(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginReset was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is MailFromDelegate)
		{
			((MailFromDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
	}

	[Obsolete("Use 'RsetAsync' method instead.")]
	public IAsyncResult BeginReset(AsyncCallback callback, object state)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		ResetDelegate resetDelegate = Reset;
		AsyncResultState asyncResultState = new AsyncResultState(this, resetDelegate, callback, state);
		asyncResultState.SetAsyncResult(resetDelegate.BeginInvoke(asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use 'RsetAsync' method instead.")]
	public void EndReset(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginReset was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is ResetDelegate)
		{
			((ResetDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginReset method.");
	}

	[Obsolete("Use Rset method instead.")]
	public void Reset()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		WriteLine("RSET");
		string text = ReadLine();
		if (!text.StartsWith("250"))
		{
			throw new SMTP_ClientException(text);
		}
		m_MailFrom = null;
		m_pRecipients.Clear();
	}

	[Obsolete("Use method 'SendMessageAsync' instead.")]
	public IAsyncResult BeginSendMessage(Stream message, AsyncCallback callback, object state)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		SendMessageDelegate sendMessageDelegate = SendMessage;
		AsyncResultState asyncResultState = new AsyncResultState(this, sendMessageDelegate, callback, state);
		asyncResultState.SetAsyncResult(sendMessageDelegate.BeginInvoke(message, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method 'SendMessageAsync' instead.")]
	public void EndSendMessage(IAsyncResult asyncResult)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginSendMessage method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginSendMessage was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is SendMessageDelegate)
		{
			((SendMessageDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
			return;
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginSendMessage method.");
	}

	[Obsolete("Use method Dns_Client.GetEmailHostsAsync instead.")]
	public static IAsyncResult BeginGetDomainHosts(string domain, AsyncCallback callback, object state)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (string.IsNullOrEmpty(domain))
		{
			throw new ArgumentException("Invalid argument 'domain' value, you need to specify domain value.");
		}
		GetDomainHostsDelegate getDomainHostsDelegate = GetDomainHosts;
		AsyncResultState asyncResultState = new AsyncResultState(null, getDomainHostsDelegate, callback, state);
		asyncResultState.SetAsyncResult(getDomainHostsDelegate.BeginInvoke(domain, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use method Dns_Client.GetEmailHostsAsync instead.")]
	public static string[] EndGetDomainHosts(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is AsyncResultState asyncResultState))
		{
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginGetDomainHosts method.");
		}
		if (asyncResultState.IsEndCalled)
		{
			throw new InvalidOperationException("BeginGetDomainHosts was previously called for the asynchronous connection.");
		}
		asyncResultState.IsEndCalled = true;
		if (asyncResultState.AsyncDelegate is GetDomainHostsDelegate)
		{
			return ((GetDomainHostsDelegate)asyncResultState.AsyncDelegate).EndInvoke(asyncResultState.AsyncResult);
		}
		throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginGetDomainHosts method.");
	}

	[Obsolete("Use method Dns_Client.GetEmailHosts instead.")]
	public static string[] GetDomainHosts(string domain)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (string.IsNullOrEmpty(domain))
		{
			throw new ArgumentException("Invalid argument 'domain' value, you need to specify domain value.");
		}
		if (domain.IndexOf("@") > -1)
		{
			domain = domain.Substring(domain.IndexOf('@') + 1);
		}
		List<string> list = new List<string>();
		using (Dns_Client dns_Client = new Dns_Client())
		{
			DnsServerResponse dnsServerResponse = dns_Client.Query(domain, DNS_QType.MX);
			if (dnsServerResponse.ResponseCode != 0)
			{
				throw new DNS_ClientException(dnsServerResponse.ResponseCode);
			}
			DNS_rr_MX[] mXRecords = dnsServerResponse.GetMXRecords();
			foreach (DNS_rr_MX dNS_rr_MX in mXRecords)
			{
				if (!string.IsNullOrEmpty(dNS_rr_MX.Host))
				{
					list.Add(dNS_rr_MX.Host);
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(domain);
		}
		return list.ToArray();
	}
}
