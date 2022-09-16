using System;
using System.IO;
using System.Text;
using System.Threading;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.POP3.Client;

public class POP3_ClientMessage
{
	public class MarkForDeletionAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_ClientMessage m_pOwner;

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

		public event EventHandler<EventArgs<MarkForDeletionAsyncOP>> CompletedAsync;

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pOwner = null;
				m_pPop3Client = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_ClientMessage owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
			m_pPop3Client = owner.m_Pop3Client;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("DELE " + owner.SequenceNumber + "\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "DELE " + owner.SequenceNumber);
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, DeleCommandSendingCompleted, null);
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

		private void DeleCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					DeleReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					DeleReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void DeleReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
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
						m_pOwner.m_IsMarkedForDeletion = true;
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
				this.CompletedAsync(this, new EventArgs<MarkForDeletionAsyncOP>(this));
			}
		}
	}

	public class MessageToStreamAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_ClientMessage m_pOwner;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private Stream m_pStream;

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

		public event EventHandler<EventArgs<MessageToStreamAsyncOP>> CompletedAsync;

		public MessageToStreamAsyncOP(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = stream;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pOwner = null;
				m_pPop3Client = null;
				m_pStream = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_ClientMessage owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
			m_pPop3Client = owner.m_Pop3Client;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("RETR " + owner.SequenceNumber + "\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "RETR " + owner.SequenceNumber);
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, RetrCommandSendingCompleted, null);
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

		private void RetrCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					RetrReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					RetrReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void RetrReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
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
						SmartStream.ReadPeriodTerminatedAsyncOP readMsgOP = new SmartStream.ReadPeriodTerminatedAsyncOP(m_pStream, long.MaxValue, SizeExceededAction.ThrowException);
						readMsgOP.CompletedAsync += delegate
						{
							MessageReadingCompleted(readMsgOP);
						};
						if (m_pPop3Client.TcpStream.ReadPeriodTerminated(readMsgOP, async: true))
						{
							MessageReadingCompleted(readMsgOP);
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

		private void MessageReadingCompleted(SmartStream.ReadPeriodTerminatedAsyncOP op)
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
					m_pPop3Client.LogAddRead(op.BytesStored, "Readed period-terminated message " + op.BytesStored + " bytes.");
					SetState(AsyncOP_State.Completed);
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
				this.CompletedAsync(this, new EventArgs<MessageToStreamAsyncOP>(this));
			}
		}
	}

	public class MessageTopLinesToStreamAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private POP3_ClientMessage m_pOwner;

		private POP3_Client m_pPop3Client;

		private bool m_RiseCompleted;

		private Stream m_pStream;

		private int m_LineCount;

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

		public event EventHandler<EventArgs<MessageTopLinesToStreamAsyncOP>> CompletedAsync;

		public MessageTopLinesToStreamAsyncOP(Stream stream, int lineCount)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (lineCount < 0)
			{
				throw new ArgumentException("Argument 'lineCount' value must be >= 0.", "lineCount");
			}
			m_pStream = stream;
			m_LineCount = lineCount;
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pOwner = null;
				m_pPop3Client = null;
				m_pStream = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(POP3_ClientMessage owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
			m_pPop3Client = owner.m_Pop3Client;
			SetState(AsyncOP_State.Active);
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes("TOP " + owner.SequenceNumber + " " + m_LineCount + "\r\n");
				m_pPop3Client.LogAddWrite(bytes.Length, "TOP " + owner.SequenceNumber + " " + m_LineCount);
				m_pPop3Client.TcpStream.BeginWrite(bytes, 0, bytes.Length, TopCommandSendingCompleted, null);
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

		private void TopCommandSendingCompleted(IAsyncResult ar)
		{
			try
			{
				m_pPop3Client.TcpStream.EndWrite(ar);
				SmartStream.ReadLineAsyncOP op = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
				op.CompletedAsync += delegate
				{
					TopReadResponseCompleted(op);
				};
				if (m_pPop3Client.TcpStream.ReadLine(op, async: true))
				{
					TopReadResponseCompleted(op);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				m_pPop3Client.LogAddException("Exception: " + ex.Message, ex);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void TopReadResponseCompleted(SmartStream.ReadLineAsyncOP op)
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
						SmartStream.ReadPeriodTerminatedAsyncOP readMsgOP = new SmartStream.ReadPeriodTerminatedAsyncOP(m_pStream, long.MaxValue, SizeExceededAction.ThrowException);
						readMsgOP.CompletedAsync += delegate
						{
							MessageReadingCompleted(readMsgOP);
						};
						if (m_pPop3Client.TcpStream.ReadPeriodTerminated(readMsgOP, async: true))
						{
							MessageReadingCompleted(readMsgOP);
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

		private void MessageReadingCompleted(SmartStream.ReadPeriodTerminatedAsyncOP op)
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
					m_pPop3Client.LogAddRead(op.BytesStored, "Readed period-terminated message " + op.BytesStored + " bytes.");
					SetState(AsyncOP_State.Completed);
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
				this.CompletedAsync(this, new EventArgs<MessageTopLinesToStreamAsyncOP>(this));
			}
		}
	}

	private POP3_Client m_Pop3Client;

	private int m_SequenceNumber = 1;

	private string m_UID = "";

	private int m_Size;

	private bool m_IsMarkedForDeletion;

	private bool m_IsDisposed;

	public bool IsDisposed => m_IsDisposed;

	public int SequenceNumber
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_SequenceNumber;
		}
	}

	public string UID
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!m_Pop3Client.IsUidlSupported)
			{
				throw new NotSupportedException("POP3 server doesn't support UIDL command.");
			}
			return m_UID;
		}
	}

	public int Size
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_Size;
		}
	}

	public bool IsMarkedForDeletion
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_IsMarkedForDeletion;
		}
	}

	internal POP3_ClientMessage(POP3_Client pop3, int seqNumber, int size)
	{
		m_Pop3Client = pop3;
		m_SequenceNumber = seqNumber;
		m_Size = size;
	}

	public void MarkForDeletion()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			return;
		}
		using MarkForDeletionAsyncOP markForDeletionAsyncOP = new MarkForDeletionAsyncOP();
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			markForDeletionAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!MarkForDeletionAsync(markForDeletionAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			wait.Close();
			if (markForDeletionAsyncOP.Error != null)
			{
				throw markForDeletionAsyncOP.Error;
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

	public bool MarkForDeletionAsync(MarkForDeletionAsyncOP op)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Message is already marked for deletion.");
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

	public string HeaderToString()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		return Encoding.Default.GetString(HeaderToByte());
	}

	public byte[] HeaderToByte()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		MemoryStream memoryStream = new MemoryStream();
		MessageTopLinesToStream(memoryStream, 0);
		return memoryStream.ToArray();
	}

	public void HeaderToStream(Stream stream)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("Argument 'stream' value can't be null.");
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		MessageTopLinesToStream(stream, 0);
	}

	public byte[] MessageToByte()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		MemoryStream memoryStream = new MemoryStream();
		MessageToStream(memoryStream);
		return memoryStream.ToArray();
	}

	public void MessageToStream(Stream stream)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("Argument 'stream' value can't be null.");
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		using MessageToStreamAsyncOP messageToStreamAsyncOP = new MessageToStreamAsyncOP(stream);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			messageToStreamAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!MessageToStreamAsync(messageToStreamAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			wait.Close();
			if (messageToStreamAsyncOP.Error != null)
			{
				throw messageToStreamAsyncOP.Error;
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

	public bool MessageToStreamAsync(MessageToStreamAsyncOP op)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Message is already marked for deletion.");
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

	public byte[] MessageTopLinesToByte(int lineCount)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (lineCount < 0)
		{
			throw new ArgumentException("Argument 'lineCount' value must be >= 0.");
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		MemoryStream memoryStream = new MemoryStream();
		MessageTopLinesToStream(memoryStream, lineCount);
		return memoryStream.ToArray();
	}

	public void MessageTopLinesToStream(Stream stream, int lineCount)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("Argument 'stream' value can't be null.");
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Can't access message, it's marked for deletion.");
		}
		using MessageTopLinesToStreamAsyncOP messageTopLinesToStreamAsyncOP = new MessageTopLinesToStreamAsyncOP(stream, lineCount);
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		try
		{
			messageTopLinesToStreamAsyncOP.CompletedAsync += delegate
			{
				wait.Set();
			};
			if (!MessageTopLinesToStreamAsync(messageTopLinesToStreamAsyncOP))
			{
				wait.Set();
			}
			wait.WaitOne();
			wait.Close();
			if (messageTopLinesToStreamAsyncOP.Error != null)
			{
				throw messageTopLinesToStreamAsyncOP.Error;
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

	public bool MessageTopLinesToStreamAsync(MessageTopLinesToStreamAsyncOP op)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsMarkedForDeletion)
		{
			throw new InvalidOperationException("Message is already marked for deletion.");
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

	internal void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			m_Pop3Client = null;
		}
	}

	internal void SetUID(string uid)
	{
		m_UID = uid;
	}

	internal void SetMarkedForDeletion(bool isMarkedForDeletion)
	{
		m_IsMarkedForDeletion = isMarkedForDeletion;
	}
}
