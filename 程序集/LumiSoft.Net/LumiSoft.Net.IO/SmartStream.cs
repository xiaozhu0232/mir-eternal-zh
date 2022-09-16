using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LumiSoft.Net.IO;

public class SmartStream : Stream
{
	private delegate void BufferCallback(Exception x);

	private class ReadAsyncOperation : IAsyncResult
	{
		private SmartStream m_pOwner;

		private byte[] m_pBuffer;

		private int m_OffsetInBuffer;

		private int m_MaxSize;

		private AsyncCallback m_pAsyncCallback;

		private object m_pAsyncState;

		private AutoResetEvent m_pAsyncWaitHandle;

		private bool m_CompletedSynchronously;

		private bool m_IsCompleted;

		private bool m_IsEndCalled;

		private int m_BytesStored;

		private Exception m_pException;

		public object AsyncState => m_pAsyncState;

		public WaitHandle AsyncWaitHandle => m_pAsyncWaitHandle;

		public bool CompletedSynchronously => m_CompletedSynchronously;

		public bool IsCompleted => m_IsCompleted;

		internal bool IsEndCalled
		{
			get
			{
				return m_IsEndCalled;
			}
			set
			{
				m_IsEndCalled = value;
			}
		}

		internal byte[] Buffer => m_pBuffer;

		internal int BytesStored => m_BytesStored;

		public ReadAsyncOperation(SmartStream owner, byte[] buffer, int offset, int maxSize, AsyncCallback callback, object asyncState)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be < buffer.Length.");
			}
			if (maxSize < 0)
			{
				throw new ArgumentOutOfRangeException("maxSize", "Argument 'maxSize' value must be >= 0.");
			}
			if (offset + maxSize > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("maxSize", "Argument 'maxSize' is bigger than than argument 'buffer' can store.");
			}
			m_pOwner = owner;
			m_pBuffer = buffer;
			m_OffsetInBuffer = offset;
			m_MaxSize = maxSize;
			m_pAsyncCallback = callback;
			m_pAsyncState = asyncState;
			m_pAsyncWaitHandle = new AutoResetEvent(initialState: false);
			DoRead();
		}

		private void Buffering_Completed(Exception x)
		{
			if (x != null)
			{
				m_pException = x;
				Completed();
			}
			else if (m_pOwner.BytesInReadBuffer == 0)
			{
				Completed();
			}
			else
			{
				DoRead();
			}
		}

		private void DoRead()
		{
			try
			{
				if (m_pOwner.BytesInReadBuffer == 0)
				{
					if (m_pOwner.BufferRead(async: true, Buffering_Completed))
					{
						return;
					}
					if (m_pOwner.BytesInReadBuffer == 0)
					{
						Completed();
						return;
					}
				}
				int num = Math.Min(m_MaxSize, m_pOwner.BytesInReadBuffer);
				Array.Copy(m_pOwner.m_pReadBuffer, m_pOwner.m_ReadBufferOffset, m_pBuffer, m_OffsetInBuffer, num);
				m_pOwner.m_ReadBufferOffset += num;
				m_pOwner.m_LastActivity = DateTime.Now;
				m_BytesStored += num;
				Completed();
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				Completed();
			}
		}

		private void Completed()
		{
			m_IsCompleted = true;
			m_pAsyncWaitHandle.Set();
			if (m_pAsyncCallback != null)
			{
				m_pAsyncCallback(this);
			}
		}
	}

	public class ReadLineAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private bool m_RiseCompleted;

		private SmartStream m_pOwner;

		private byte[] m_pBuffer;

		private SizeExceededAction m_ExceededAction = SizeExceededAction.JunkAndThrowException;

		private int m_BytesInBuffer;

		private int m_LastByte = -1;

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
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_pException;
			}
		}

		public SizeExceededAction SizeExceededAction
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_ExceededAction;
			}
		}

		public byte[] Buffer
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_pBuffer;
			}
		}

		public int BytesInBuffer
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_BytesInBuffer;
			}
		}

		public int LineBytesInBuffer
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				int num = m_BytesInBuffer;
				if (m_BytesInBuffer > 1)
				{
					if (m_pBuffer[m_BytesInBuffer - 1] == 10)
					{
						num--;
						if (m_pBuffer[m_BytesInBuffer - 2] == 13)
						{
							num--;
						}
					}
				}
				else if (m_BytesInBuffer > 0 && m_pBuffer[m_BytesInBuffer - 1] == 10)
				{
					num--;
				}
				return num;
			}
		}

		public string LineAscii
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				if (BytesInBuffer == 0)
				{
					return null;
				}
				return Encoding.ASCII.GetString(m_pBuffer, 0, LineBytesInBuffer);
			}
		}

		public string LineUtf8
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				if (BytesInBuffer == 0)
				{
					return null;
				}
				return Encoding.UTF8.GetString(m_pBuffer, 0, LineBytesInBuffer);
			}
		}

		public string LineUtf32
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				if (BytesInBuffer == 0)
				{
					return null;
				}
				return Encoding.UTF32.GetString(m_pBuffer, 0, LineBytesInBuffer);
			}
		}

		public event EventHandler<EventArgs<ReadLineAsyncOP>> CompletedAsync;

		[Obsolete("Use CompletedAsync event istead.")]
		public event EventHandler<EventArgs<ReadLineAsyncOP>> Completed;

		public ReadLineAsyncOP(byte[] buffer, SizeExceededAction exceededAction)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			m_pBuffer = buffer;
			m_ExceededAction = exceededAction;
		}

		~ReadLineAsyncOP()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				m_State = AsyncOP_State.Disposed;
				m_pOwner = null;
				m_pBuffer = null;
				m_pException = null;
				this.CompletedAsync = null;
				this.Completed = null;
			}
		}

		internal bool Start(bool async, SmartStream stream)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_State == AsyncOP_State.Active)
			{
				throw new InvalidOperationException("There is existing active operation. There may be only one active operation at same time.");
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pOwner = stream;
			m_State = AsyncOP_State.Active;
			m_RiseCompleted = false;
			m_pException = null;
			m_BytesInBuffer = 0;
			m_LastByte = -1;
			if (DoLineReading(async))
			{
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void Buffering_Completed(Exception x)
		{
			bool flag = false;
			try
			{
				if (x != null)
				{
					m_pException = x;
					flag = true;
				}
				else if (m_pOwner.BytesInReadBuffer == 0)
				{
					flag = true;
				}
				else if (DoLineReading(async: true))
				{
					flag = true;
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				flag = true;
			}
			if (flag)
			{
				SetState(AsyncOP_State.Completed);
			}
		}

		private bool DoLineReading(bool async)
		{
			try
			{
				while (true)
				{
					if (m_pOwner.BytesInReadBuffer == 0)
					{
						if (m_pOwner.BufferRead(async, Buffering_Completed))
						{
							return false;
						}
						if (m_pOwner.BytesInReadBuffer == 0)
						{
							return true;
						}
					}
					byte b = m_pOwner.m_pReadBuffer[m_pOwner.m_ReadBufferOffset++];
					if (m_BytesInBuffer >= m_pBuffer.Length)
					{
						if (m_pException == null)
						{
							m_pException = new LineSizeExceededException();
						}
						if (m_ExceededAction == SizeExceededAction.ThrowException)
						{
							return true;
						}
					}
					else
					{
						m_pBuffer[m_BytesInBuffer++] = b;
					}
					if (b == 10 && (!m_pOwner.CRLFLines || (m_pOwner.CRLFLines && m_LastByte == 13)))
					{
						break;
					}
					m_LastByte = b;
				}
				return true;
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			return true;
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				bool riseCompleted = m_RiseCompleted;
				lock (m_pLock)
				{
					m_State = state;
					riseCompleted = m_RiseCompleted;
				}
				if (m_State == AsyncOP_State.Completed && riseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ReadLineAsyncOP>(this));
			}
			if (this.Completed != null)
			{
				this.Completed(this, new EventArgs<ReadLineAsyncOP>(this));
			}
		}
	}

	public class ReadPeriodTerminatedAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private bool m_RiseCompleted;

		private SmartStream m_pOwner;

		private Stream m_pStream;

		private long m_MaxCount;

		private SizeExceededAction m_ExceededAction = SizeExceededAction.JunkAndThrowException;

		private ReadLineAsyncOP m_pReadLineOP;

		private long m_BytesStored;

		private int m_LinesStored;

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
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
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
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_pStream;
			}
		}

		public long BytesStored
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_BytesStored;
			}
		}

		public int LinesStored
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_LinesStored;
			}
		}

		public event EventHandler<EventArgs<ReadPeriodTerminatedAsyncOP>> CompletedAsync;

		[Obsolete("Use CompletedAsync event istead.")]
		public event EventHandler<EventArgs<ReadPeriodTerminatedAsyncOP>> Completed;

		public ReadPeriodTerminatedAsyncOP(Stream stream, long maxCount, SizeExceededAction exceededAction)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (maxCount < 0)
			{
				throw new ArgumentException("Argument 'maxCount' must be >= 0.", "maxCount");
			}
			m_pStream = stream;
			m_MaxCount = maxCount;
			m_ExceededAction = exceededAction;
			m_pReadLineOP = new ReadLineAsyncOP(new byte[32000], exceededAction);
			m_pReadLineOP.CompletedAsync += m_pReadLineOP_CompletedAsync;
		}

		~ReadPeriodTerminatedAsyncOP()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				m_State = AsyncOP_State.Disposed;
				m_pOwner = null;
				m_pStream = null;
				m_pReadLineOP.Dispose();
				m_pReadLineOP = null;
				m_pException = null;
				this.CompletedAsync = null;
				this.Completed = null;
			}
		}

		internal bool Start(bool async, SmartStream stream)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_State == AsyncOP_State.Active)
			{
				throw new InvalidOperationException("There is existing active operation. There may be only one active operation at same time.");
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pOwner = stream;
			m_State = AsyncOP_State.Active;
			m_RiseCompleted = false;
			m_pException = null;
			m_BytesStored = 0L;
			m_LinesStored = 0;
			if (DoRead(async))
			{
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void m_pReadLineOP_CompletedAsync(object sender, EventArgs<ReadLineAsyncOP> e)
		{
			bool flag = false;
			try
			{
				if (ProcessReadedLine())
				{
					flag = true;
				}
				else if (DoRead(async: true))
				{
					flag = true;
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				flag = true;
			}
			if (flag)
			{
				SetState(AsyncOP_State.Completed);
			}
		}

		private bool DoRead(bool async)
		{
			try
			{
				while (m_pOwner.ReadLine(m_pReadLineOP, async))
				{
					if (ProcessReadedLine())
					{
						return true;
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			return false;
		}

		private bool ProcessReadedLine()
		{
			if (m_pReadLineOP.Error != null)
			{
				m_pException = m_pReadLineOP.Error;
				return true;
			}
			if (m_pReadLineOP.BytesInBuffer == 0)
			{
				m_pException = new IncompleteDataException("Data is not period-terminated.");
				return true;
			}
			if (m_pReadLineOP.LineBytesInBuffer == 1 && m_pReadLineOP.Buffer[0] == 46)
			{
				return true;
			}
			if (m_MaxCount < 1 || m_BytesStored + m_pReadLineOP.BytesInBuffer < m_MaxCount)
			{
				if (m_pReadLineOP.Buffer[0] == 46)
				{
					m_pStream.Write(m_pReadLineOP.Buffer, 1, m_pReadLineOP.BytesInBuffer - 1);
					m_BytesStored += m_pReadLineOP.BytesInBuffer - 1;
					m_LinesStored++;
				}
				else
				{
					m_pStream.Write(m_pReadLineOP.Buffer, 0, m_pReadLineOP.BytesInBuffer);
					m_BytesStored += m_pReadLineOP.BytesInBuffer;
					m_LinesStored++;
				}
			}
			else
			{
				if (m_ExceededAction == SizeExceededAction.ThrowException)
				{
					m_pException = new DataSizeExceededException();
					return true;
				}
				if (m_pException == null)
				{
					m_pException = new DataSizeExceededException();
				}
			}
			return false;
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				bool riseCompleted = m_RiseCompleted;
				lock (m_pLock)
				{
					m_State = state;
					riseCompleted = m_RiseCompleted;
				}
				if (m_State == AsyncOP_State.Completed && riseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<ReadPeriodTerminatedAsyncOP>(this));
			}
			if (this.Completed != null)
			{
				this.Completed(this, new EventArgs<ReadPeriodTerminatedAsyncOP>(this));
			}
		}
	}

	private class BufferReadAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private bool m_RiseCompleted;

		private SmartStream m_pOwner;

		private byte[] m_pBuffer;

		private int m_MaxCount;

		private int m_BytesInBuffer;

		private bool m_IsCallbackCalled;

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
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_pException;
			}
		}

		public byte[] Buffer
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_pBuffer;
			}
		}

		public int BytesInBuffer
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("This property is only valid in AsyncOP_State.Completed state.");
				}
				return m_BytesInBuffer;
			}
		}

		public event EventHandler<EventArgs<BufferReadAsyncOP>> CompletedAsync;

		public BufferReadAsyncOP(SmartStream owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
		}

		~BufferReadAsyncOP()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				return;
			}
			try
			{
				if (m_State == AsyncOP_State.Active)
				{
					m_pException = new ObjectDisposedException("SmartStream");
					m_State = AsyncOP_State.Completed;
					OnCompletedAsync();
				}
			}
			catch
			{
			}
			m_State = AsyncOP_State.Disposed;
			m_pOwner = null;
			m_pBuffer = null;
			this.CompletedAsync = null;
		}

		internal bool Start(bool async, byte[] buffer, int count)
		{
			if (m_State == AsyncOP_State.Disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (m_State == AsyncOP_State.Active)
			{
				throw new InvalidOperationException("There is existing active operation. There may be only one active operation at same time.");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (count < 0)
			{
				throw new ArgumentException("Argument 'count' value must be >= 0.");
			}
			if (count > buffer.Length)
			{
				throw new ArgumentException("Argument 'count' value must be <= buffer.Length.");
			}
			m_State = AsyncOP_State.Active;
			m_RiseCompleted = false;
			m_pException = null;
			m_pBuffer = buffer;
			m_MaxCount = count;
			m_BytesInBuffer = 0;
			m_IsCallbackCalled = false;
			if (async)
			{
				try
				{
					m_pOwner.m_pStream.BeginRead(buffer, 0, count, delegate(IAsyncResult r)
					{
						try
						{
							m_BytesInBuffer = m_pOwner.m_pStream.EndRead(r);
						}
						catch (Exception pException3)
						{
							Exception ex3 = (m_pException = pException3);
						}
						SetState(AsyncOP_State.Completed);
					}, null);
				}
				catch (Exception pException)
				{
					Exception ex = (m_pException = pException);
					SetState(AsyncOP_State.Completed);
				}
			}
			else
			{
				try
				{
					m_BytesInBuffer = m_pOwner.m_pStream.Read(buffer, 0, count);
				}
				catch (Exception pException2)
				{
					Exception ex2 = (m_pException = pException2);
				}
				SetState(AsyncOP_State.Completed);
			}
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		internal void ReleaseEvents()
		{
			this.CompletedAsync = null;
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				bool riseCompleted = m_RiseCompleted;
				lock (m_pLock)
				{
					m_State = state;
					riseCompleted = m_RiseCompleted;
				}
				if (m_State == AsyncOP_State.Completed && riseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void OnCompletedAsync()
		{
			if (!m_IsCallbackCalled && this.CompletedAsync != null)
			{
				m_IsCallbackCalled = true;
				this.CompletedAsync(this, new EventArgs<BufferReadAsyncOP>(this));
			}
		}
	}

	public class WriteStreamAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private bool m_RiseCompleted;

		private SmartStream m_pOwner;

		private Stream m_pStream;

		private long m_Count;

		private byte[] m_pBuffer;

		private long m_BytesWritten;

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

		public long BytesWritten
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Socket' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_BytesWritten;
			}
		}

		public event EventHandler<EventArgs<WriteStreamAsyncOP>> CompletedAsync;

		public WriteStreamAsyncOP(Stream stream, long count)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = stream;
			m_Count = count;
			m_pBuffer = new byte[32000];
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pStream = null;
				m_pOwner = null;
				m_pBuffer = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SmartStream owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
			SetState(AsyncOP_State.Active);
			BeginReadData();
			lock (m_pLock)
			{
				m_RiseCompleted = true;
				return m_State == AsyncOP_State.Active;
			}
		}

		private void SetState(AsyncOP_State state)
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				bool riseCompleted = m_RiseCompleted;
				lock (m_pLock)
				{
					m_State = state;
					riseCompleted = m_RiseCompleted;
				}
				if (m_State == AsyncOP_State.Completed && riseCompleted)
				{
					OnCompletedAsync();
				}
			}
		}

		private void BeginReadData()
		{
			try
			{
				while (true)
				{
					bool isBeginReadCompleted = false;
					bool isCompletedSync = false;
					int count = (int)((m_Count == -1) ? m_pBuffer.Length : Math.Min(m_pBuffer.Length, m_Count - m_BytesWritten));
					IAsyncResult readResult = m_pStream.BeginRead(m_pBuffer, 0, count, delegate(IAsyncResult r)
					{
						lock (m_pLock)
						{
							if (!isBeginReadCompleted)
							{
								isCompletedSync = true;
								return;
							}
						}
						ProcessReadDataResult(r);
					}, null);
					lock (m_pLock)
					{
						isBeginReadCompleted = true;
					}
					if (!isCompletedSync || ProcessReadDataResult(readResult) || State != AsyncOP_State.Active)
					{
						break;
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private bool ProcessReadDataResult(IAsyncResult readResult)
		{
			try
			{
				int countReaded = m_pStream.EndRead(readResult);
				if (countReaded == 0)
				{
					if (m_Count == -1)
					{
						SetState(AsyncOP_State.Completed);
					}
					else
					{
						m_pException = new ArgumentException("Argument 'stream' has less data than specified in 'count'.", "stream");
						SetState(AsyncOP_State.Completed);
					}
				}
				else
				{
					bool isBeginWriteCompleted = false;
					bool isCompletedSync = false;
					IAsyncResult asyncResult = m_pOwner.BeginWrite(m_pBuffer, 0, countReaded, delegate(IAsyncResult r)
					{
						lock (m_pLock)
						{
							if (!isBeginWriteCompleted)
							{
								isCompletedSync = true;
								return;
							}
						}
						try
						{
							m_pOwner.EndWrite(r);
							m_BytesWritten += countReaded;
							if (m_Count == m_BytesWritten)
							{
								SetState(AsyncOP_State.Completed);
							}
							else
							{
								BeginReadData();
							}
						}
						catch (Exception pException2)
						{
							m_pException = pException2;
							SetState(AsyncOP_State.Completed);
						}
					}, null);
					lock (m_pLock)
					{
						isBeginWriteCompleted = true;
					}
					if (!isCompletedSync)
					{
						return true;
					}
					m_pOwner.EndWrite(asyncResult);
					m_BytesWritten += countReaded;
					if (m_Count == m_BytesWritten)
					{
						SetState(AsyncOP_State.Completed);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
			return false;
		}

		private void OnCompletedAsync()
		{
			if (this.CompletedAsync != null)
			{
				this.CompletedAsync(this, new EventArgs<WriteStreamAsyncOP>(this));
			}
		}
	}

	public class WritePeriodTerminatedAsyncOP : IDisposable, IAsyncOP
	{
		private object m_pLock = new object();

		private AsyncOP_State m_State;

		private Exception m_pException;

		private SmartStream m_pStream;

		private SmartStream m_pOwner;

		private ReadLineAsyncOP m_pReadLineOP;

		private int m_BytesWritten;

		private bool m_EndsCRLF;

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

		public int BytesWritten
		{
			get
			{
				if (m_State == AsyncOP_State.Disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				if (m_State != AsyncOP_State.Completed)
				{
					throw new InvalidOperationException("Property 'Socket' is accessible only in 'AsyncOP_State.Completed' state.");
				}
				if (m_pException != null)
				{
					throw m_pException;
				}
				return m_BytesWritten;
			}
		}

		public event EventHandler<EventArgs<WritePeriodTerminatedAsyncOP>> CompletedAsync;

		public WritePeriodTerminatedAsyncOP(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_pStream = new SmartStream(stream, owner: false);
		}

		public void Dispose()
		{
			if (m_State != AsyncOP_State.Disposed)
			{
				SetState(AsyncOP_State.Disposed);
				m_pException = null;
				m_pStream = null;
				m_pOwner = null;
				m_pReadLineOP = null;
				this.CompletedAsync = null;
			}
		}

		internal bool Start(SmartStream owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			m_pOwner = owner;
			SetState(AsyncOP_State.Active);
			try
			{
				m_pReadLineOP = new ReadLineAsyncOP(new byte[32000], SizeExceededAction.ThrowException);
				m_pReadLineOP.CompletedAsync += delegate
				{
					ReadLineCompleted(m_pReadLineOP);
				};
				if (m_pStream.ReadLine(m_pReadLineOP, async: true))
				{
					ReadLineCompleted(m_pReadLineOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
				m_pReadLineOP.Dispose();
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

		private void ReadLineCompleted(ReadLineAsyncOP op)
		{
			try
			{
				if (op.Error != null)
				{
					m_pException = op.Error;
					SetState(AsyncOP_State.Completed);
				}
				else if (op.BytesInBuffer == 0)
				{
					if (m_EndsCRLF)
					{
						m_BytesWritten += 3;
						m_pOwner.BeginWrite(new byte[3] { 46, 13, 10 }, 0, 3, SendTerminatorCompleted, null);
					}
					else
					{
						m_BytesWritten += 5;
						m_pOwner.BeginWrite(new byte[5] { 13, 10, 46, 13, 10 }, 0, 5, SendTerminatorCompleted, null);
					}
					op.Dispose();
				}
				else
				{
					m_BytesWritten += op.BytesInBuffer;
					if (op.BytesInBuffer >= 2 && op.Buffer[op.BytesInBuffer - 2] == 13 && op.Buffer[op.BytesInBuffer - 1] == 10)
					{
						m_EndsCRLF = true;
					}
					else
					{
						m_EndsCRLF = false;
					}
					if (op.Buffer[0] == 46)
					{
						byte[] array = new byte[op.BytesInBuffer + 1];
						array[0] = 46;
						Array.Copy(op.Buffer, 0, array, 1, op.BytesInBuffer);
						m_pOwner.BeginWrite(array, 0, array.Length, SendLineCompleted, null);
					}
					else
					{
						m_pOwner.BeginWrite(op.Buffer, 0, op.BytesInBuffer, SendLineCompleted, null);
					}
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
				op.Dispose();
			}
		}

		private void SendLineCompleted(IAsyncResult ar)
		{
			try
			{
				m_pOwner.EndWrite(ar);
				if (m_pStream.ReadLine(m_pReadLineOP, async: true))
				{
					ReadLineCompleted(m_pReadLineOP);
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				SetState(AsyncOP_State.Completed);
			}
		}

		private void SendTerminatorCompleted(IAsyncResult ar)
		{
			try
			{
				m_pOwner.EndWrite(ar);
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
				this.CompletedAsync(this, new EventArgs<WritePeriodTerminatedAsyncOP>(this));
			}
		}
	}

	private class ReadLineAsyncOperation : IAsyncResult
	{
		private SmartStream m_pOwner;

		private byte[] m_pBuffer;

		private int m_OffsetInBuffer;

		private int m_MaxCount;

		private SizeExceededAction m_SizeExceededAction = SizeExceededAction.JunkAndThrowException;

		private AsyncCallback m_pAsyncCallback;

		private object m_pAsyncState;

		private AutoResetEvent m_pAsyncWaitHandle;

		private bool m_CompletedSynchronously;

		private bool m_IsCompleted;

		private bool m_IsEndCalled;

		private int m_BytesReaded;

		private int m_BytesStored;

		private Exception m_pException;

		public object AsyncState => m_pAsyncState;

		public WaitHandle AsyncWaitHandle => m_pAsyncWaitHandle;

		public bool CompletedSynchronously => m_CompletedSynchronously;

		public bool IsCompleted => m_IsCompleted;

		internal bool IsEndCalled
		{
			get
			{
				return m_IsEndCalled;
			}
			set
			{
				m_IsEndCalled = value;
			}
		}

		internal byte[] Buffer => m_pBuffer;

		internal int BytesReaded => m_BytesReaded;

		internal int BytesStored => m_BytesStored;

		public ReadLineAsyncOperation(SmartStream owner, byte[] buffer, int offset, int maxCount, SizeExceededAction exceededAction, AsyncCallback callback, object asyncState)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be < buffer.Length.");
			}
			if (maxCount < 0)
			{
				throw new ArgumentOutOfRangeException("maxCount", "Argument 'maxCount' value must be >= 0.");
			}
			if (offset + maxCount > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("maxCount", "Argument 'maxCount' is bigger than than argument 'buffer' can store.");
			}
			m_pOwner = owner;
			m_pBuffer = buffer;
			m_OffsetInBuffer = offset;
			m_MaxCount = maxCount;
			m_SizeExceededAction = exceededAction;
			m_pAsyncCallback = callback;
			m_pAsyncState = asyncState;
			m_pAsyncWaitHandle = new AutoResetEvent(initialState: false);
			DoLineReading();
		}

		private void Buffering_Completed(Exception x)
		{
			if (x != null)
			{
				m_pException = x;
				Completed();
			}
			else if (m_pOwner.BytesInReadBuffer == 0)
			{
				Completed();
			}
			else
			{
				DoLineReading();
			}
		}

		private void DoLineReading()
		{
			try
			{
				while (true)
				{
					if (m_pOwner.BytesInReadBuffer == 0)
					{
						if (m_pOwner.BufferRead(async: true, Buffering_Completed))
						{
							return;
						}
						if (m_pOwner.BytesInReadBuffer == 0)
						{
							Completed();
							return;
						}
					}
					byte b = m_pOwner.m_pReadBuffer[m_pOwner.m_ReadBufferOffset++];
					m_BytesReaded++;
					switch (b)
					{
					case 13:
						if (m_pOwner.Peek() != 10)
						{
							break;
						}
						m_pOwner.ReadByte();
						m_BytesReaded++;
						goto end_IL_0000;
					case 10:
						goto end_IL_0000;
					}
					if (b == 13)
					{
						break;
					}
					if (m_BytesStored >= m_MaxCount)
					{
						if (m_SizeExceededAction == SizeExceededAction.ThrowException)
						{
							throw new LineSizeExceededException();
						}
					}
					else
					{
						m_pBuffer[m_OffsetInBuffer++] = b;
						m_BytesStored++;
					}
					continue;
					end_IL_0000:
					break;
				}
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
			}
			Completed();
		}

		private void Completed()
		{
			m_IsCompleted = true;
			m_pAsyncWaitHandle.Set();
			if (m_pAsyncCallback != null)
			{
				m_pAsyncCallback(this);
			}
		}
	}

	private class ReadToTerminatorAsyncOperation : IAsyncResult
	{
		private SmartStream m_pOwner;

		private string m_Terminator = "";

		private byte[] m_pTerminatorBytes;

		private Stream m_pStoreStream;

		private long m_MaxCount;

		private SizeExceededAction m_SizeExceededAction = SizeExceededAction.JunkAndThrowException;

		private AsyncCallback m_pAsyncCallback;

		private object m_pAsyncState;

		private AutoResetEvent m_pAsyncWaitHandle;

		private bool m_CompletedSynchronously;

		private bool m_IsCompleted;

		private bool m_IsEndCalled;

		private byte[] m_pLineBuffer;

		private long m_BytesStored;

		private Exception m_pException;

		public string Terminator => m_Terminator;

		public object AsyncState => m_pAsyncState;

		public WaitHandle AsyncWaitHandle => m_pAsyncWaitHandle;

		public bool CompletedSynchronously => m_CompletedSynchronously;

		public bool IsCompleted => m_IsCompleted;

		internal bool IsEndCalled
		{
			get
			{
				return m_IsEndCalled;
			}
			set
			{
				m_IsEndCalled = value;
			}
		}

		internal long BytesStored => m_BytesStored;

		internal Exception Exception => m_pException;

		public ReadToTerminatorAsyncOperation(SmartStream owner, string terminator, Stream storeStream, long maxCount, SizeExceededAction exceededAction, AsyncCallback callback, object asyncState)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			if (terminator == null)
			{
				throw new ArgumentNullException("terminator");
			}
			if (storeStream == null)
			{
				throw new ArgumentNullException("storeStream");
			}
			if (maxCount < 0)
			{
				throw new ArgumentException("Argument 'maxCount' must be >= 0.");
			}
			m_pOwner = owner;
			m_Terminator = terminator;
			m_pTerminatorBytes = Encoding.ASCII.GetBytes(terminator);
			m_pStoreStream = storeStream;
			m_MaxCount = maxCount;
			m_SizeExceededAction = exceededAction;
			m_pAsyncCallback = callback;
			m_pAsyncState = asyncState;
			m_pAsyncWaitHandle = new AutoResetEvent(initialState: false);
			m_pLineBuffer = new byte[32000];
			m_pOwner.BeginReadLine(m_pLineBuffer, 0, m_pLineBuffer.Length - 2, m_SizeExceededAction, ReadLine_Completed, null);
		}

		private void ReadLine_Completed(IAsyncResult asyncResult)
		{
			try
			{
				int num = 0;
				try
				{
					num = m_pOwner.EndReadLine(asyncResult);
				}
				catch (LineSizeExceededException ex)
				{
					if (m_SizeExceededAction == SizeExceededAction.ThrowException)
					{
						throw ex;
					}
					m_pException = new LineSizeExceededException();
					num = 31998;
				}
				if (num == -1)
				{
					throw new IncompleteDataException();
				}
				if (Net_Utils.CompareArray(m_pTerminatorBytes, m_pLineBuffer, num))
				{
					Completed();
					return;
				}
				if (m_MaxCount > 0 && m_BytesStored + num + 2 > m_MaxCount)
				{
					if (m_SizeExceededAction == SizeExceededAction.ThrowException)
					{
						throw new DataSizeExceededException();
					}
					m_pException = new DataSizeExceededException();
				}
				else
				{
					m_pLineBuffer[num++] = 13;
					m_pLineBuffer[num++] = 10;
					m_pStoreStream.Write(m_pLineBuffer, 0, num);
					m_BytesStored += num;
				}
				m_pOwner.BeginReadLine(m_pLineBuffer, 0, m_pLineBuffer.Length - 2, m_SizeExceededAction, ReadLine_Completed, null);
			}
			catch (Exception pException)
			{
				Exception ex2 = (m_pException = pException);
				Completed();
			}
		}

		private void Completed()
		{
			m_IsCompleted = true;
			m_pAsyncWaitHandle.Set();
			if (m_pAsyncCallback != null)
			{
				m_pAsyncCallback(this);
			}
		}
	}

	private class ReadToStreamAsyncOperation : IAsyncResult
	{
		private SmartStream m_pOwner;

		private Stream m_pStoreStream;

		private long m_Count;

		private AsyncCallback m_pAsyncCallback;

		private object m_pAsyncState;

		private AutoResetEvent m_pAsyncWaitHandle;

		private bool m_CompletedSynchronously;

		private bool m_IsCompleted;

		private bool m_IsEndCalled;

		private long m_BytesStored;

		private Exception m_pException;

		public object AsyncState => m_pAsyncState;

		public WaitHandle AsyncWaitHandle => m_pAsyncWaitHandle;

		public bool CompletedSynchronously => m_CompletedSynchronously;

		public bool IsCompleted => m_IsCompleted;

		internal bool IsEndCalled
		{
			get
			{
				return m_IsEndCalled;
			}
			set
			{
				m_IsEndCalled = value;
			}
		}

		internal long BytesStored => m_BytesStored;

		internal Exception Exception => m_pException;

		public ReadToStreamAsyncOperation(SmartStream owner, Stream storeStream, long count, AsyncCallback callback, object asyncState)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner");
			}
			if (storeStream == null)
			{
				throw new ArgumentNullException("storeStream");
			}
			if (count < 0)
			{
				throw new ArgumentException("Argument 'count' must be >= 0.");
			}
			m_pOwner = owner;
			m_pStoreStream = storeStream;
			m_Count = count;
			m_pAsyncCallback = callback;
			m_pAsyncState = asyncState;
			m_pAsyncWaitHandle = new AutoResetEvent(initialState: false);
			if (m_Count == 0L)
			{
				Completed();
			}
			else
			{
				DoDataReading();
			}
		}

		private void Buffering_Completed(Exception x)
		{
			if (x != null)
			{
				m_pException = x;
				Completed();
			}
			else if (m_pOwner.BytesInReadBuffer == 0)
			{
				m_pException = new IncompleteDataException();
				Completed();
			}
			else
			{
				DoDataReading();
			}
		}

		private void DoDataReading()
		{
			try
			{
				do
				{
					if (m_pOwner.BytesInReadBuffer == 0)
					{
						if (m_pOwner.BufferRead(async: true, Buffering_Completed))
						{
							return;
						}
						if (m_pOwner.BytesInReadBuffer == 0)
						{
							throw new IncompleteDataException();
						}
					}
					int num = (int)Math.Min(m_Count - m_BytesStored, m_pOwner.BytesInReadBuffer);
					m_pStoreStream.Write(m_pOwner.m_pReadBuffer, m_pOwner.m_ReadBufferOffset, num);
					m_BytesStored += num;
					m_pOwner.m_ReadBufferOffset += num;
				}
				while (m_Count != m_BytesStored);
				Completed();
			}
			catch (Exception pException)
			{
				Exception ex = (m_pException = pException);
				Completed();
			}
		}

		private void Completed()
		{
			m_IsCompleted = true;
			m_pAsyncWaitHandle.Set();
			if (m_pAsyncCallback != null)
			{
				m_pAsyncCallback(this);
			}
		}
	}

	private bool m_IsDisposed;

	private Stream m_pStream;

	private bool m_IsOwner;

	private DateTime m_LastActivity;

	private long m_BytesReaded;

	private long m_BytesWritten;

	private int m_BufferSize = 84000;

	private byte[] m_pReadBuffer;

	private int m_ReadBufferOffset;

	private int m_ReadBufferCount;

	private BufferReadAsyncOP m_pReadBufferOP;

	private Encoding m_pEncoding = Encoding.Default;

	private bool m_CRLFLines = true;

	public bool IsDisposed => m_IsDisposed;

	public int LineBufferSize
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_BufferSize;
		}
	}

	public Stream SourceStream
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pStream;
		}
	}

	public bool IsOwner
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_IsOwner;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			m_IsOwner = value;
		}
	}

	public DateTime LastActivity
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_LastActivity;
		}
	}

	public long BytesReaded
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_BytesReaded;
		}
	}

	public long BytesWritten
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_BytesWritten;
		}
	}

	public int BytesInReadBuffer
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_ReadBufferCount - m_ReadBufferOffset;
		}
	}

	public Encoding Encoding
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pEncoding;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_pEncoding = value;
		}
	}

	public bool CRLFLines
	{
		get
		{
			return m_CRLFLines;
		}
		set
		{
			m_CRLFLines = value;
		}
	}

	public override bool CanRead
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pStream.CanRead;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pStream.CanSeek;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pStream.CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pStream.Length;
		}
	}

	public override long Position
	{
		get
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			return m_pStream.Position;
		}
		set
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException("SmartStream");
			}
			m_pStream.Position = value;
			m_ReadBufferOffset = 0;
			m_ReadBufferCount = 0;
		}
	}

	public SmartStream(Stream stream, bool owner)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_pStream = stream;
		m_IsOwner = owner;
		m_pReadBuffer = new byte[m_BufferSize];
		m_pReadBufferOP = new BufferReadAsyncOP(this);
		m_LastActivity = DateTime.Now;
	}

	public new void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
			if (m_pReadBufferOP != null)
			{
				m_pReadBufferOP.Dispose();
			}
			m_pReadBufferOP = null;
			if (m_IsOwner)
			{
				m_pStream.Dispose();
			}
		}
	}

	public bool ReadLine(ReadLineAsyncOP op, bool async)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		return !op.Start(async, this);
	}

	public IAsyncResult BeginReadHeader(Stream storeStream, int maxCount, SizeExceededAction exceededAction, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (storeStream == null)
		{
			throw new ArgumentNullException("storeStream");
		}
		if (maxCount < 0)
		{
			throw new ArgumentException("Argument 'maxCount' must be >= 0.");
		}
		return new ReadToTerminatorAsyncOperation(this, "", storeStream, maxCount, exceededAction, callback, state);
	}

	public int EndReadHeader(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is ReadToTerminatorAsyncOperation))
		{
			throw new ArgumentException("Argument 'asyncResult' was not returned by a call to the BeginReadHeader method.");
		}
		ReadToTerminatorAsyncOperation readToTerminatorAsyncOperation = (ReadToTerminatorAsyncOperation)asyncResult;
		if (readToTerminatorAsyncOperation.IsEndCalled)
		{
			throw new InvalidOperationException("EndReadHeader is already called for specified 'asyncResult'.");
		}
		readToTerminatorAsyncOperation.AsyncWaitHandle.WaitOne();
		readToTerminatorAsyncOperation.AsyncWaitHandle.Close();
		readToTerminatorAsyncOperation.IsEndCalled = true;
		if (readToTerminatorAsyncOperation.Exception != null)
		{
			throw readToTerminatorAsyncOperation.Exception;
		}
		return (int)readToTerminatorAsyncOperation.BytesStored;
	}

	public int ReadHeader(Stream storeStream, int maxCount, SizeExceededAction exceededAction)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (storeStream == null)
		{
			throw new ArgumentNullException("storeStream");
		}
		if (maxCount < 0)
		{
			throw new ArgumentException("Argument 'maxCount' must be >= 0.");
		}
		IAsyncResult asyncResult = BeginReadHeader(storeStream, maxCount, exceededAction, null, null);
		return EndReadHeader(asyncResult);
	}

	public bool ReadPeriodTerminated(ReadPeriodTerminatedAsyncOP op, bool async)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (op == null)
		{
			throw new ArgumentNullException("op");
		}
		return !op.Start(async, this);
	}

	public IAsyncResult BeginReadFixedCount(Stream storeStream, long count, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (storeStream == null)
		{
			throw new ArgumentNullException("storeStream");
		}
		if (count < 0)
		{
			throw new ArgumentException("Argument 'count' value must be >= 0.");
		}
		return new ReadToStreamAsyncOperation(this, storeStream, count, callback, state);
	}

	public void EndReadFixedCount(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is ReadToStreamAsyncOperation))
		{
			throw new ArgumentException("Argument 'asyncResult' was not returned by a call to the BeginReadFixedCount method.");
		}
		ReadToStreamAsyncOperation readToStreamAsyncOperation = (ReadToStreamAsyncOperation)asyncResult;
		if (readToStreamAsyncOperation.IsEndCalled)
		{
			throw new InvalidOperationException("EndReadFixedCount is already called for specified 'asyncResult'.");
		}
		readToStreamAsyncOperation.AsyncWaitHandle.WaitOne();
		readToStreamAsyncOperation.AsyncWaitHandle.Close();
		readToStreamAsyncOperation.IsEndCalled = true;
		if (readToStreamAsyncOperation.Exception != null)
		{
			throw readToStreamAsyncOperation.Exception;
		}
	}

	public void ReadFixedCount(Stream storeStream, long count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (storeStream == null)
		{
			throw new ArgumentNullException("storeStream");
		}
		if (count < 0)
		{
			throw new ArgumentException("Argument 'count' value must be >= 0.");
		}
		IAsyncResult asyncResult = BeginReadFixedCount(storeStream, count, null, null);
		EndReadFixedCount(asyncResult);
	}

	public string ReadFixedCountString(int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (count < 0)
		{
			throw new ArgumentException("Argument 'count' value must be >= 0.");
		}
		MemoryStream memoryStream = new MemoryStream();
		ReadFixedCount(memoryStream, count);
		return m_pEncoding.GetString(memoryStream.ToArray());
	}

	public void ReadAll(Stream stream)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = new byte[m_BufferSize];
		while (true)
		{
			int num = Read(array, 0, array.Length);
			if (num != 0)
			{
				stream.Write(array, 0, num);
				continue;
			}
			break;
		}
	}

	public int Peek()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (BytesInReadBuffer == 0)
		{
			BufferRead(async: false, null);
		}
		if (BytesInReadBuffer == 0)
		{
			return -1;
		}
		return m_pReadBuffer[m_ReadBufferOffset];
	}

	public void Write(string data)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] bytes = Encoding.Default.GetBytes(data);
		Write(bytes, 0, bytes.Length);
		Flush();
	}

	public int WriteLine(string line)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (!line.EndsWith("\r\n"))
		{
			line += "\r\n";
		}
		byte[] bytes = m_pEncoding.GetBytes(line);
		Write(bytes, 0, bytes.Length);
		Flush();
		return bytes.Length;
	}

	public void WriteStream(Stream stream)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = new byte[m_BufferSize];
		while (true)
		{
			int num = stream.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			Write(array, 0, num);
		}
		Flush();
	}

	public void WriteStream(Stream stream, long count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (count < 0)
		{
			throw new ArgumentException("Argument 'count' value must be >= 0.");
		}
		byte[] array = new byte[m_BufferSize];
		long num = 0L;
		while (num < count)
		{
			int num2 = stream.Read(array, 0, (int)Math.Min(array.Length, count - num));
			num += num2;
			Write(array, 0, num2);
		}
		Flush();
	}

	public bool WriteStreamAsync(WriteStreamAsyncOP op)
	{
		if (m_IsDisposed)
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

	public long WritePeriodTerminated(Stream stream)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		ManualResetEvent wait = new ManualResetEvent(initialState: false);
		WritePeriodTerminatedAsyncOP writePeriodTerminatedAsyncOP = new WritePeriodTerminatedAsyncOP(stream);
		writePeriodTerminatedAsyncOP.CompletedAsync += delegate
		{
			wait.Set();
		};
		if (!WritePeriodTerminatedAsync(writePeriodTerminatedAsyncOP))
		{
			wait.Set();
		}
		wait.WaitOne();
		wait.Close();
		if (writePeriodTerminatedAsyncOP.Error != null)
		{
			throw writePeriodTerminatedAsyncOP.Error;
		}
		return writePeriodTerminatedAsyncOP.BytesWritten;
	}

	public bool WritePeriodTerminatedAsync(WritePeriodTerminatedAsyncOP op)
	{
		if (m_IsDisposed)
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

	public void WriteHeader(Stream stream)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		new SmartStream(stream, owner: false).ReadHeader(this, 0, SizeExceededAction.ThrowException);
	}

	public override void Flush()
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		m_pStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		return m_pStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		m_pStream.SetLength(value);
		m_ReadBufferOffset = 0;
		m_ReadBufferCount = 0;
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be < buffer.Length.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' value must be >= 0.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' is bigger than than argument 'buffer' can store.");
		}
		return new ReadAsyncOperation(this, buffer, offset, count, callback, state);
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is ReadAsyncOperation))
		{
			throw new ArgumentException("Argument 'asyncResult' was not returned by a call to the BeginRead method.");
		}
		ReadAsyncOperation obj = (ReadAsyncOperation)asyncResult;
		if (obj.IsEndCalled)
		{
			throw new InvalidOperationException("EndRead is already called for specified 'asyncResult'.");
		}
		obj.AsyncWaitHandle.WaitOne();
		obj.AsyncWaitHandle.Close();
		obj.IsEndCalled = true;
		return obj.BytesStored;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' value must be >= 0.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' is bigger than than argument 'buffer' can store.");
		}
		if (BytesInReadBuffer == 0)
		{
			BufferRead(async: false, null);
		}
		if (BytesInReadBuffer == 0)
		{
			return 0;
		}
		int num = Math.Min(count, BytesInReadBuffer);
		Array.Copy(m_pReadBuffer, m_ReadBufferOffset, buffer, offset, num);
		m_ReadBufferOffset += num;
		return num;
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' value must be >= 0.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count", "Argument 'count' is bigger than than argument 'buffer' can store.");
		}
		m_LastActivity = DateTime.Now;
		m_BytesWritten += count;
		return m_pStream.BeginWrite(buffer, offset, count, callback, state);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		m_pStream.EndWrite(asyncResult);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException("SmartStream");
		}
		m_pStream.Write(buffer, offset, count);
		m_LastActivity = DateTime.Now;
		m_BytesWritten += count;
	}

	private bool BufferRead(bool async, BufferCallback asyncCallback)
	{
		if (BytesInReadBuffer != 0)
		{
			throw new InvalidOperationException("There is already data in read buffer.");
		}
		m_ReadBufferOffset = 0;
		m_ReadBufferCount = 0;
		if (async)
		{
			m_pReadBufferOP.ReleaseEvents();
			m_pReadBufferOP.CompletedAsync += delegate(object s, EventArgs<BufferReadAsyncOP> e)
			{
				try
				{
					if (e.Value.Error != null)
					{
						if (asyncCallback != null)
						{
							asyncCallback(e.Value.Error);
						}
					}
					else
					{
						m_ReadBufferOffset = 0;
						m_ReadBufferCount = e.Value.BytesInBuffer;
						m_BytesReaded += e.Value.BytesInBuffer;
						m_LastActivity = DateTime.Now;
						if (asyncCallback != null)
						{
							asyncCallback(null);
						}
					}
				}
				catch (Exception x)
				{
					if (asyncCallback != null)
					{
						asyncCallback(x);
					}
				}
			};
			if (m_pReadBufferOP.Start(async, m_pReadBuffer, m_pReadBuffer.Length))
			{
				return true;
			}
			if (m_pReadBufferOP.Error != null)
			{
				throw m_pReadBufferOP.Error;
			}
			m_ReadBufferOffset = 0;
			m_ReadBufferCount = m_pReadBufferOP.BytesInBuffer;
			m_BytesReaded += m_pReadBufferOP.BytesInBuffer;
			m_LastActivity = DateTime.Now;
			return false;
		}
		m_BytesReaded += (m_ReadBufferCount = m_pStream.Read(m_pReadBuffer, 0, m_pReadBuffer.Length));
		m_LastActivity = DateTime.Now;
		return false;
	}

	[Obsolete("Use method 'ReadLine' instead.")]
	public IAsyncResult BeginReadLine(byte[] buffer, int offset, int maxCount, SizeExceededAction exceededAction, AsyncCallback callback, object state)
	{
		if (m_IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be >= 0.");
		}
		if (offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset", "Argument 'offset' value must be < buffer.Length.");
		}
		if (maxCount < 0)
		{
			throw new ArgumentOutOfRangeException("maxCount", "Argument 'maxCount' value must be >= 0.");
		}
		if (offset + maxCount > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("maxCount", "Argument 'maxCount' is bigger than than argument 'buffer' can store.");
		}
		return new ReadLineAsyncOperation(this, buffer, offset, maxCount, exceededAction, callback, state);
	}

	[Obsolete("Use method 'ReadLine' instead.")]
	public int EndReadLine(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is ReadLineAsyncOperation))
		{
			throw new ArgumentException("Argument 'asyncResult' was not returned by a call to the BeginReadLine method.");
		}
		ReadLineAsyncOperation readLineAsyncOperation = (ReadLineAsyncOperation)asyncResult;
		if (readLineAsyncOperation.IsEndCalled)
		{
			throw new InvalidOperationException("EndReadLine is already called for specified 'asyncResult'.");
		}
		readLineAsyncOperation.AsyncWaitHandle.WaitOne();
		readLineAsyncOperation.AsyncWaitHandle.Close();
		readLineAsyncOperation.IsEndCalled = true;
		if (readLineAsyncOperation.BytesReaded == 0)
		{
			return -1;
		}
		return readLineAsyncOperation.BytesStored;
	}
}
