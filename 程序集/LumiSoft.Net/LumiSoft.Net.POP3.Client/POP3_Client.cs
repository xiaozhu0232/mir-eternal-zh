using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.POP3.Client;

public class POP3_Client : TCP_Client
{
	public class CapaAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private List<string> m_pResponseLines;

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

		public event EventHandler<EventArgs<CapaAsyncOP>> CompletedAsync;

		public CapaAsyncOP()
		{
			m_pResponseLines = new List<string>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pPop3Client = null;
				m_pResponseLines = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("CAPA\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "CAPA");
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, CapaCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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

		private void CapaCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					CapaReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					CapaReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void CapaReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
						readLineOP.CompletedAsync += delegate
						{
							try
							{
								ReadMultiLineResponseLineCompleted(readLineOP);
								while (State == AsyncOP_State.Active && m_pPop3Client.TcpStream.ReadLine(readLineOP, async: true))
								{
									ReadMultiLineResponseLineCompleted(readLineOP);
								}
							}
							catch (Exception ex2)
							{
								m_pException = ex2;
								m_pPop3Client.LogAddException("Exception: " + ex2.Message, ex2);
								SetState(AsyncOP_State.Completed);
							}
						};
						while (State == AsyncOP_State.Active && m_pPop3Client.TcpStream.ReadLine(readLineOP, async: true))
						{
							ReadMultiLineResponseLineCompleted(readLineOP);
						}
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void ReadMultiLineResponseLineCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
					return;
				}
				m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
				if (op.BytesInBuffer == 0)
				{
					m_pException = new IOException("POP3 server closed connection unexpectedly.");
					SetState(AsyncOP_State.Completed);
				}
				else if (string.Equals(op.LineUtf8, ".", StringComparison.InvariantCultureIgnoreCase))
				{
					m_pPop3Client.m_pExtCapabilities.Clear();
					m_pPop3Client.m_pExtCapabilities.AddRange(m_pResponseLines);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pResponseLines.Add(op.LineUtf8);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<CapaAsyncOP>(this));
			}
		}
	}

	public class StlsAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private RemoteCertificateValidationCallback m_pCertCallback;

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

		public event EventHandler<EventArgs<StlsAsyncOP>> CompletedAsync;

		public StlsAsyncOP(RemoteCertificateValidationCallback certCallback)
		{
			m_pCertCallback = certCallback;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("STLS\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "STLS");
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, StlsCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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

		private void StlsCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					StlsReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					StlsReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void StlsReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						m_pPop3Client.LogAddText("Starting TLS handshake.");
						SwitchToSecureAsyncOP switchSecureOP = new SwitchToSecureAsyncOP(m_pCertCallback);
						switchSecureOP.CompletedAsync += delegate
						{
							SwitchToSecureCompleted(switchSecureOP);
						};
						if (!m_pPop3Client.SwitchToSecureAsync(switchSecureOP))
						{
							SwitchToSecureCompleted(switchSecureOP);
						}
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
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
					m_pPop3Client.LogAddException("Exception: " + m_pException.Message, m_pException);
				}
				else
				{
					m_pPop3Client.LogAddText("TLS handshake completed successfully.");
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + m_pException.Message, m_pException);
			}
			op.Dispose();
			SetState(AsyncOP_State.Completed);
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<StlsAsyncOP>(this));
			}
		}
	}

	public class LoginAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private string m_User;

		private string m_Password;

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

		public event EventHandler<EventArgs<LoginAsyncOP>> CompletedAsync;

		public LoginAsyncOP(string user, string password)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (user == string.Empty)
			{
				throw new ArgumentException("Argument 'user' value must be specified.", "user");
			}
			if (password == null)
			{
				throw new ArgumentNullException("password");
			}
			m_User = user;
			m_Password = password;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("USER " + m_User + "\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "USER " + m_User);
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, UserCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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

		private void UserCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					UserReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					UserReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void UserReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						byte[] bytes = Encoding.UTF8.GetBytes("PASS " + m_Password + "\r\n");
						m_pPop3Client.LogAddWrite(bytes.Length, "PASS <***REMOVED***>");
						m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, PassCommandSendingCompleted, null);
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void PassCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					PassReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					PassReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void PassReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						FillMessagesAsyncOP fillOP = new FillMessagesAsyncOP();
						fillOP.CompletedAsync += delegate
						{
							FillMessagesCompleted(fillOP);
						};
						if (!m_pPop3Client.FillMessagesAsync(fillOP))
						{
							FillMessagesCompleted(fillOP);
						}
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void FillMessagesCompleted(FillMessagesAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.m_pAuthdUserIdentity = new GenericIdentity(m_User, "pop3-user/pass");
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<LoginAsyncOP>(this));
			}
		}
	}

	public class AuthAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

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
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				if (m_pSASL.SupportsInitialResponse)
				{
					byte[] bytes = Encoding.UTF8.GetBytes("AUTH " + m_pSASL.Name + " " + Convert.ToBase64String(m_pSASL.Continue(null)) + "\r\n");
					m_pPop3Client.LogAddWrite(bytes.Length, Encoding.UTF8.GetString(bytes).TrimEnd());
					m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, AuthCommandSendingCompleted, null);
				}
				else
				{
					byte[] bytes2 = Encoding.UTF8.GetBytes("AUTH " + m_pSASL.Name + "\r\n");
					m_pPop3Client.LogAddWrite(bytes2.Length, "AUTH " + m_pSASL.Name);
					m_pPop3Client.TcpStream.BeginWrite(bytes2, 0, bytes2.Length, AuthCommandSendingCompleted, null);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					AuthReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					AuthReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void AuthReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						FillMessagesAsyncOP fillOP = new FillMessagesAsyncOP();
						fillOP.CompletedAsync += delegate
						{
							FillMessagesCompleted(fillOP);
						};
						if (!m_pPop3Client.FillMessagesAsync(fillOP))
						{
							FillMessagesCompleted(fillOP);
						}
					}
					else if (op.LineUtf8.StartsWith("+"))
					{
						byte[] serverResponse = Convert.FromBase64String(op.LineUtf8.Split(new char[1] { ' ' }, 2)[1]);
						byte[] inArray = m_pSASL.Continue(serverResponse);
						byte[] bytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(inArray) + "\r\n");
						m_pPop3Client.LogAddWrite(bytes.Length, Convert.ToBase64String(inArray));
						m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, AuthCommandSendingCompleted, null);
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void FillMessagesCompleted(FillMessagesAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.m_pAuthdUserIdentity = new GenericIdentity(m_pSASL.UserName, m_pSASL.Name);
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<AuthAsyncOP>(this));
			}
		}
	}

	public class NoopAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

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
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("NOOP\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "NOOP");
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, NoopCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					NoopReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					NoopReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void NoopReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						SetState(AsyncOP_State.Completed);
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<NoopAsyncOP>(this));
			}
		}
	}

	public class RsetAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

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
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("RSET\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "RSET");
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, RsetCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					RsetReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					RsetReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void RsetReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						foreach (POP3_ClientMessage pMessage in m_pPop3Client.m_pMessages)
						{
							pMessage.SetMarkedForDeletion(isMarkedForDeletion: false);
						}
						SetState(AsyncOP_State.Completed);
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<RsetAsyncOP>(this));
			}
		}
	}

	private class FillMessagesAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

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

		public event EventHandler<EventArgs<FillMessagesAsyncOP>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				ListAsyncOP listOP = new ListAsyncOP();
				listOP.CompletedAsync += delegate
				{
					ListCompleted(listOP);
				};
				if (!m_pPop3Client.ListAsync(listOP))
				{
					ListCompleted(listOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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

		private void ListCompleted(ListAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.m_pMessages = new POP3_ClientMessageCollection(m_pPop3Client);
					string[] responseLines = op.ResponseLines;
					foreach (string text in responseLines)
					{
						m_pPop3Client.m_pMessages.Add(Convert.ToInt32(text.Trim().Split(' ')[1]));
					}
					UidlAsyncOP uidlOP = new UidlAsyncOP();
					uidlOP.CompletedAsync += delegate
					{
						UidlCompleted(uidlOP);
					};
					if (!m_pPop3Client.UidlAsync(uidlOP))
					{
						UidlCompleted(uidlOP);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void UidlCompleted(UidlAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.m_IsUidlSupported = true;
					string[] responseLines = op.ResponseLines;
					for (int i = 0; i < responseLines.Length; i++)
					{
						string[] array = responseLines[i].Trim().Split(' ');
						m_pPop3Client.m_pMessages[Convert.ToInt32(array[0]) - 1].SetUID(array[1]);
					}
					SetState(AsyncOP_State.Completed);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<FillMessagesAsyncOP>(this));
			}
		}
	}

	private class ListAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private List<string> m_pResponseLines;

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

		public string[] ResponseLines
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
				return m_pResponseLines.ToArray();
			}
		}

		public event EventHandler<EventArgs<ListAsyncOP>> CompletedAsync;

		public ListAsyncOP()
		{
			m_pResponseLines = new List<string>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pPop3Client = null;
				m_pResponseLines = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("LIST\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "LIST");
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, ListCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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

		private void ListCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					ListReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					ListReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void ListReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
						readLineOP.CompletedAsync += delegate
						{
							try
							{
								ReadMultiLineResponseLineCompleted(readLineOP);
								while (State == AsyncOP_State.Active && m_pPop3Client.TcpStream.ReadLine(readLineOP, async: true))
								{
									ReadMultiLineResponseLineCompleted(readLineOP);
								}
							}
							catch (Exception ex2)
							{
								m_pException = ex2;
								m_pPop3Client.LogAddException("Exception: " + ex2.Message, ex2);
								SetState(AsyncOP_State.Completed);
							}
						};
						while (State == AsyncOP_State.Active && m_pPop3Client.TcpStream.ReadLine(readLineOP, async: true))
						{
							ReadMultiLineResponseLineCompleted(readLineOP);
						}
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void ReadMultiLineResponseLineCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
					return;
				}
				m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
				if (op.BytesInBuffer == 0)
				{
					m_pException = new IOException("POP3 server closed connection unexpectedly.");
					SetState(AsyncOP_State.Completed);
				}
				else if (string.Equals(op.LineUtf8, ".", StringComparison.InvariantCultureIgnoreCase))
				{
					m_pPop3Client.m_pExtCapabilities.Clear();
					m_pPop3Client.m_pExtCapabilities.AddRange(m_pResponseLines);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pResponseLines.Add(op.LineUtf8);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ListAsyncOP>(this));
			}
		}
	}

	private class UidlAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private List<string> m_pResponseLines;

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

		public string[] ResponseLines
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
				return m_pResponseLines.ToArray();
			}
		}

		public event EventHandler<EventArgs<UidlAsyncOP>> CompletedAsync;

		public UidlAsyncOP()
		{
			m_pResponseLines = new List<string>();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pPop3Client = null;
				m_pResponseLines = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_Client owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pPop3Client = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("UIDL\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "UIDL");
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, UidlCommandSendingCompleted, null);
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
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

		private void UidlCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					UidlReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					UidlReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void UidlReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
					if (string.Equals(op.LineUtf8.Split(new char[1] { ' ' }, 2)[0], "+OK", StringComparison.InvariantCultureIgnoreCase))
					{
						SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
						readLineOP.CompletedAsync += delegate
						{
							try
							{
								ReadMultiLineResponseLineCompleted(readLineOP);
								while (State == AsyncOP_State.Active && m_pPop3Client.TcpStream.ReadLine(readLineOP, async: true))
								{
									ReadMultiLineResponseLineCompleted(readLineOP);
								}
							}
							catch (Exception ex2)
							{
								m_pException = ex2;
								m_pPop3Client.LogAddException("Exception: " + ex2.Message, ex2);
								SetState(AsyncOP_State.Completed);
							}
						};
						while (State == AsyncOP_State.Active && m_pPop3Client.TcpStream.ReadLine(readLineOP, async: true))
						{
							ReadMultiLineResponseLineCompleted(readLineOP);
						}
					}
					else
					{
						m_pException = new POP3_ClientException(op.LineUtf8);
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
			op.Dispose();
		}

		private void ReadMultiLineResponseLineCompleted(SmartStream.ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					m_pPop3Client.LogAddException("Exception: " + op.Error.Message, op.Error);
					SetState(AsyncOP_State.Completed);
					return;
				}
				m_pPop3Client.LogAddRead(op.BytesInBuffer, op.LineUtf8);
				if (op.BytesInBuffer == 0)
				{
					m_pException = new IOException("POP3 server closed connection unexpectedly.");
					SetState(AsyncOP_State.Completed);
				}
				else if (string.Equals(op.LineUtf8, ".", StringComparison.InvariantCultureIgnoreCase))
				{
					m_pPop3Client.m_pExtCapabilities.Clear();
					m_pPop3Client.m_pExtCapabilities.AddRange(m_pResponseLines);
					SetState(AsyncOP_State.Completed);
				}
				else
				{
					m_pResponseLines.Add(op.LineUtf8);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<UidlAsyncOP>(this));
			}
		}
	}

	private delegate void StartTLSDelegate();

	private delegate void AuthenticateDelegate(string userName, string password, bool tryApop);

	private delegate void NoopDelegate();

	private delegate void ResetDelegate();

	private string m_GreetingText = "";

	private string m_ApopHashKey = "";

	private List<string> m_pExtCapabilities;

	private bool m_IsUidlSupported;

	private POP3_ClientMessageCollection m_pMessages;

	private GenericIdentity m_pAuthdUserIdentity;

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

	[Obsolete("USe ExtendedCapabilities instead !")]
	public string[] ExtenededCapabilities
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
			return m_pExtCapabilities.ToArray();
		}
	}

	public string[] ExtendedCapabilities
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
			return m_pExtCapabilities.ToArray();
		}
	}

	public bool IsUidlSupported
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
			if (!base.IsAuthenticated)
			{
				throw new InvalidOperationException("You must authenticate first.");
			}
			return m_IsUidlSupported;
		}
	}

	public POP3_ClientMessageCollection Messages
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
			if (!base.IsAuthenticated)
			{
				throw new InvalidOperationException("You must authenticate first.");
			}
			return m_pMessages;
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

	public POP3_Client()
	{
		m_pExtCapabilities = new List<string>();
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Disconnect()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("POP3 client is not connected.");
		}
		try
		{
			WriteLine("QUIT");
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
		m_GreetingText = "";
		m_ApopHashKey = "";
		m_pExtCapabilities = new List<string>();
		m_IsUidlSupported = false;
		if (m_pMessages != null)
		{
			m_pMessages.Dispose();
			m_pMessages = null;
		}
		m_pAuthdUserIdentity = null;
	}

	public void Capa()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		using CapaAsyncOP capaAsyncOP = new CapaAsyncOP();
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			capaAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!CapaAsync(capaAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			wait.Close();
			if (capaAsyncOP.Error != null)
			{
				throw capaAsyncOP.Error;
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

	public bool CapaAsync(CapaAsyncOP op)
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

	public void Stls(RemoteCertificateValidationCallback certCallback)
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
			throw new InvalidOperationException("The STLS command is only valid in non-authenticated state.");
		}
		if (IsSecureConnection)
		{
			throw new InvalidOperationException("Connection is already secure.");
		}
		using StlsAsyncOP stlsAsyncOP = new StlsAsyncOP(certCallback);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			stlsAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!StlsAsync(stlsAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			wait.Close();
			if (stlsAsyncOP.Error != null)
			{
				throw stlsAsyncOP.Error;
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

	public bool StlsAsync(StlsAsyncOP op)
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
			throw new InvalidOperationException("The STLS command is only valid in non-authenticated state.");
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
			throw new InvalidOperationException("You must connect first.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("Session is already authenticated.");
		}
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user == string.Empty)
		{
			throw new ArgumentException("Argument 'user' value must be specified.", "user");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		using LoginAsyncOP loginAsyncOP = new LoginAsyncOP(user, password);
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
			throw new InvalidOperationException("Session is already authenticated.");
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
		using AuthAsyncOP authAsyncOP = new AuthAsyncOP(sasl);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
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
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
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
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("The NOOP command is only valid in TRANSACTION state.");
		}
		using NoopAsyncOP noopAsyncOP = new NoopAsyncOP();
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
			wait.Close();
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
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("The NOOP command is only valid in TRANSACTION(authenticated) state.");
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
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("The RSET command is only valid in TRANSACTION state.");
		}
		using RsetAsyncOP rsetAsyncOP = new RsetAsyncOP();
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
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
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
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
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("The RSET command is only valid in TRANSACTION(authenticated) state.");
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

	private bool FillMessagesAsync(FillMessagesAsyncOP op)
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

	private bool ListAsync(ListAsyncOP op)
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

	private bool UidlAsync(UidlAsyncOP op)
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
		SmartStream.ReadLineAsyncOP readGreetingOP = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
		readGreetingOP.CompletedAsync += delegate
		{
			ReadServerGreetingCompleted(readGreetingOP, callback);
		};
		if (TcpStream.ReadLine(readGreetingOP, async: true))
		{
			ReadServerGreetingCompleted(readGreetingOP, callback);
		}
	}

	private void ReadServerGreetingCompleted(SmartStream.ReadLineAsyncOP op, CompleteConnectCallback connectCallback)
	{
		Exception error = null;
		try
		{
			if (op.Error != null)
			{
				error = op.Error;
			}
			else
			{
				string lineUtf = op.LineUtf8;
				LogAddRead(op.BytesInBuffer, lineUtf);
				if (op.LineUtf8.StartsWith("+OK", StringComparison.InvariantCultureIgnoreCase))
				{
					m_GreetingText = lineUtf.Substring(3).Trim();
					if (lineUtf.IndexOf("<") > -1 && lineUtf.IndexOf(">") > -1)
					{
						m_ApopHashKey = lineUtf.Substring(lineUtf.IndexOf("<"), lineUtf.LastIndexOf(">") - lineUtf.IndexOf("<") + 1);
					}
				}
				else
				{
					error = new POP3_ClientException(lineUtf);
				}
			}
		}
		catch (Exception ex)
		{
			error = ex;
		}
		connectCallback(error);
	}

	[Obsolete("Use Stls/StlsAsync method instead.")]
	public void StartTLS()
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
			throw new InvalidOperationException("The STLS command is only valid in non-authenticated state.");
		}
		if (IsSecureConnection)
		{
			throw new InvalidOperationException("Connection is already secure.");
		}
		WriteLine("STLS");
		string text = ReadLine();
		if (!text.ToUpper().StartsWith("+OK"))
		{
			throw new POP3_ClientException(text);
		}
		SwitchToSecure();
	}

	[Obsolete("Use Stls/StlsAsync method instead.")]
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
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("The STLS command is only valid in non-authenticated state.");
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

	[Obsolete("Use Stls/StlsAsync method instead.")]
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

	[Obsolete("Use Login/LoginAsync method instead.")]
	public void Authenticate(string userName, string password, bool tryApop)
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
		if (tryApop && m_ApopHashKey.Length > 0)
		{
			string text = Net_Utils.ComputeMd5(m_ApopHashKey + password, hex: true);
			int num = TcpStream.WriteLine("APOP " + userName + " " + text);
			LogAddWrite(num, "APOP " + userName + " " + text);
			string text2 = ReadLine();
			if (!text2.StartsWith("+OK"))
			{
				throw new POP3_ClientException(text2);
			}
			m_pAuthdUserIdentity = new GenericIdentity(userName, "apop");
		}
		else
		{
			int num2 = TcpStream.WriteLine("USER " + userName);
			LogAddWrite(num2, "USER " + userName);
			string text3 = ReadLine();
			if (!text3.StartsWith("+OK"))
			{
				throw new POP3_ClientException(text3);
			}
			num2 = TcpStream.WriteLine("PASS " + password);
			LogAddWrite(num2, "PASS <***REMOVED***>");
			text3 = ReadLine();
			if (!text3.StartsWith("+OK"))
			{
				throw new POP3_ClientException(text3);
			}
			m_pAuthdUserIdentity = new GenericIdentity(userName, "pop3-user/pass");
		}
		if (base.IsAuthenticated)
		{
			FillMessages();
		}
	}

	[Obsolete("Use Login/LoginAsync method instead.")]
	public IAsyncResult BeginAuthenticate(string userName, string password, bool tryApop, AsyncCallback callback, object state)
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
		asyncResultState.SetAsyncResult(authenticateDelegate.BeginInvoke(userName, password, tryApop, asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use Login/LoginAsync method instead.")]
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
			throw new ArgumentException("Argument asyncResult was not returned by a call to the BeginAuthenticate method.");
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

	[Obsolete("Use Noop/NoopAsync method instead.")]
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

	[Obsolete("Use Noop/NoopAsync method instead.")]
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

	[Obsolete("Use Rset/RsetAsync method instead.")]
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
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("The RSET command is only valid in authenticated state.");
		}
		ResetDelegate resetDelegate = Reset;
		AsyncResultState asyncResultState = new AsyncResultState(this, resetDelegate, callback, state);
		asyncResultState.SetAsyncResult(resetDelegate.BeginInvoke(asyncResultState.CompletedCallback, null));
		return asyncResultState;
	}

	[Obsolete("Use Rset/RsetAsync method instead.")]
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

	[Obsolete("Use Rset/RsetAsync method instead.")]
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
		if (!base.IsAuthenticated)
		{
			throw new InvalidOperationException("The RSET command is only valid in TRANSACTION state.");
		}
		using RsetAsyncOP rsetAsyncOP = new RsetAsyncOP();
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
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
		finally
		{
			if (wait != null)
			{
				((IDisposable)wait).Dispose();
			}
		}
	}

	[Obsolete("deprecated")]
	private void FillMessages()
	{
		m_pMessages = new POP3_ClientMessageCollection(this);
		WriteLine("LIST");
		string text = ReadLine();
		if (text.StartsWith("+OK"))
		{
			while (true)
			{
				text = ReadLine();
				if (text.Trim() == ".")
				{
					break;
				}
				string[] array = text.Trim().Split(' ');
				m_pMessages.Add(Convert.ToInt32(array[1]));
			}
			WriteLine("UIDL");
			text = ReadLine();
			if (text.StartsWith("+OK"))
			{
				m_IsUidlSupported = true;
				while (true)
				{
					text = ReadLine();
					if (!(text.Trim() == "."))
					{
						string[] array2 = text.Trim().Split(' ');
						m_pMessages[Convert.ToInt32(array2[0]) - 1].SetUID(array2[1]);
						continue;
					}
					break;
				}
			}
			else
			{
				m_IsUidlSupported = false;
			}
			return;
		}
		throw new POP3_ClientException(text);
	}
}
