using System;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_Multipart : MIME_b
{
	public class _MultipartReader : Stream
	{
		internal enum State
		{
			SeekFirst,
			ReadNext,
			InBoundary,
			Done
		}

		private class _DataLine
		{
			private byte[] m_pLineBuffer;

			private int m_BytesInBuffer;

			public byte[] LineBuffer => m_pLineBuffer;

			public int BytesInBuffer => m_BytesInBuffer;

			public _DataLine(int lineBufferSize)
			{
				m_pLineBuffer = new byte[lineBufferSize];
			}

			public void AssignFrom(SmartStream.ReadLineAsyncOP op)
			{
				if (op == null)
				{
					throw new ArgumentNullException();
				}
				m_BytesInBuffer = op.BytesInBuffer;
				Array.Copy(op.Buffer, m_pLineBuffer, op.BytesInBuffer);
			}
		}

		private State m_State;

		private SmartStream m_pStream;

		private string m_Boundary = "";

		private _DataLine m_pPreviousLine;

		private SmartStream.ReadLineAsyncOP m_pReadLineOP;

		private StringBuilder m_pTextPreamble;

		private StringBuilder m_pTextEpilogue;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public string TextPreamble => m_pTextPreamble.ToString();

		public string TextEpilogue => m_pTextEpilogue.ToString();

		internal State ReaderState => m_State;

		public _MultipartReader(SmartStream stream, string boundary)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (boundary == null)
			{
				throw new ArgumentNullException("boundary");
			}
			m_pStream = stream;
			m_Boundary = boundary;
			m_pReadLineOP = new SmartStream.ReadLineAsyncOP(new byte[stream.LineBufferSize], SizeExceededAction.ThrowException);
			m_pTextPreamble = new StringBuilder();
			m_pTextEpilogue = new StringBuilder();
		}

		public bool Next()
		{
			if (m_State == State.InBoundary)
			{
				throw new InvalidOperationException("You must read all boundary data, before calling this method.");
			}
			if (m_State == State.Done)
			{
				return false;
			}
			if (m_State == State.SeekFirst)
			{
				m_pPreviousLine = null;
				while (true)
				{
					m_pStream.ReadLine(m_pReadLineOP, async: false);
					if (m_pReadLineOP.Error != null)
					{
						throw m_pReadLineOP.Error;
					}
					if (m_pReadLineOP.BytesInBuffer == 0)
					{
						m_State = State.Done;
						return false;
					}
					if (m_pReadLineOP.LineUtf8.Trim() == "--" + m_Boundary)
					{
						break;
					}
					m_pTextPreamble.Append(m_pReadLineOP.LineUtf8 + "\r\n");
				}
				m_State = State.InBoundary;
				return true;
			}
			if (m_State == State.ReadNext)
			{
				m_pPreviousLine = null;
				m_State = State.InBoundary;
				return true;
			}
			return false;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (m_State == State.SeekFirst)
			{
				throw new InvalidOperationException("Read method is not valid in '" + m_State.ToString() + "' state.");
			}
			if (m_State == State.ReadNext || m_State == State.Done)
			{
				return 0;
			}
			if (m_pPreviousLine == null)
			{
				m_pPreviousLine = new _DataLine(m_pStream.LineBufferSize);
				m_pStream.ReadLine(m_pReadLineOP, async: false);
				if (m_pReadLineOP.Error != null)
				{
					throw m_pReadLineOP.Error;
				}
				if (m_pReadLineOP.BytesInBuffer == 0)
				{
					m_State = State.Done;
					return 0;
				}
				if (m_pReadLineOP.Buffer[0] == 45 && string.Equals("--" + m_Boundary + "--", m_pReadLineOP.LineUtf8))
				{
					m_State = State.Done;
					while (true)
					{
						m_pStream.ReadLine(m_pReadLineOP, async: false);
						if (m_pReadLineOP.Error != null)
						{
							throw m_pReadLineOP.Error;
						}
						if (m_pReadLineOP.BytesInBuffer == 0)
						{
							break;
						}
						m_pTextEpilogue.Append(m_pReadLineOP.LineUtf8 + "\r\n");
					}
					return 0;
				}
				if (m_pReadLineOP.Buffer[0] == 45 && string.Equals("--" + m_Boundary, m_pReadLineOP.LineUtf8))
				{
					m_State = State.ReadNext;
					return 0;
				}
				m_pPreviousLine.AssignFrom(m_pReadLineOP);
			}
			m_pStream.ReadLine(m_pReadLineOP, async: false);
			if (m_pReadLineOP.Error != null)
			{
				throw m_pReadLineOP.Error;
			}
			if (m_pReadLineOP.BytesInBuffer == 0)
			{
				m_State = State.Done;
				if (count < m_pPreviousLine.BytesInBuffer)
				{
					throw new ArgumentException("Argument 'buffer' is to small. This should never happen.");
				}
				if (m_pPreviousLine.BytesInBuffer > 0)
				{
					Array.Copy(m_pPreviousLine.LineBuffer, 0, buffer, offset, m_pPreviousLine.BytesInBuffer);
				}
				return m_pPreviousLine.BytesInBuffer;
			}
			if (m_pReadLineOP.Buffer[0] == 45 && string.Equals("--" + m_Boundary + "--", m_pReadLineOP.LineUtf8))
			{
				m_State = State.Done;
				if (m_pReadLineOP.Buffer[m_pReadLineOP.BytesInBuffer - 1] == 10)
				{
					m_pTextEpilogue.Append("\r\n");
				}
				while (true)
				{
					m_pStream.ReadLine(m_pReadLineOP, async: false);
					if (m_pReadLineOP.Error != null)
					{
						throw m_pReadLineOP.Error;
					}
					if (m_pReadLineOP.BytesInBuffer == 0)
					{
						break;
					}
					m_pTextEpilogue.Append(m_pReadLineOP.LineUtf8 + "\r\n");
				}
				if (count < m_pPreviousLine.BytesInBuffer)
				{
					throw new ArgumentException("Argument 'buffer' is to small. This should never happen.");
				}
				if (m_pPreviousLine.BytesInBuffer > 2)
				{
					Array.Copy(m_pPreviousLine.LineBuffer, 0, buffer, offset, m_pPreviousLine.BytesInBuffer - 2);
					return m_pPreviousLine.BytesInBuffer - 2;
				}
				return 0;
			}
			if (m_pReadLineOP.Buffer[0] == 45 && string.Equals("--" + m_Boundary, m_pReadLineOP.LineUtf8))
			{
				m_State = State.ReadNext;
				if (count < m_pPreviousLine.BytesInBuffer)
				{
					throw new ArgumentException("Argument 'buffer' is to small. This should never happen.");
				}
				if (m_pPreviousLine.BytesInBuffer > 2)
				{
					Array.Copy(m_pPreviousLine.LineBuffer, 0, buffer, offset, m_pPreviousLine.BytesInBuffer - 2);
					return m_pPreviousLine.BytesInBuffer - 2;
				}
				return 0;
			}
			if (count < m_pPreviousLine.BytesInBuffer)
			{
				throw new ArgumentException("Argument 'buffer' is to small. This should never happen.");
			}
			Array.Copy(m_pPreviousLine.LineBuffer, 0, buffer, offset, m_pPreviousLine.BytesInBuffer);
			int bytesInBuffer = m_pPreviousLine.BytesInBuffer;
			m_pPreviousLine.AssignFrom(m_pReadLineOP);
			return bytesInBuffer;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}

	private MIME_EntityCollection m_pBodyParts;

	private string m_TextPreamble = "";

	private string m_TextEpilogue = "";

	public override bool IsModified => m_pBodyParts.IsModified;

	public virtual MIME_h_ContentType DefaultBodyPartContentType => new MIME_h_ContentType("text/plain")
	{
		Param_Charset = "US-ASCII"
	};

	public MIME_EntityCollection BodyParts => m_pBodyParts;

	public string TextPreamble
	{
		get
		{
			return m_TextPreamble;
		}
		set
		{
			m_TextPreamble = value;
		}
	}

	public string TextEpilogue
	{
		get
		{
			return m_TextEpilogue;
		}
		set
		{
			m_TextEpilogue = value;
		}
	}

	public MIME_b_Multipart(MIME_h_ContentType contentType)
		: base(contentType)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		if (string.IsNullOrEmpty(contentType.Param_Boundary))
		{
			throw new ArgumentException("Argument 'contentType' doesn't contain required boundary parameter.");
		}
		m_pBodyParts = new MIME_EntityCollection();
	}

	internal MIME_b_Multipart()
	{
		m_pBodyParts = new MIME_EntityCollection();
	}

	protected new static MIME_b Parse(MIME_Entity owner, MIME_h_ContentType defaultContentType, SmartStream stream)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (defaultContentType == null)
		{
			throw new ArgumentNullException("defaultContentType");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (owner.ContentType == null || owner.ContentType.Param_Boundary == null)
		{
			throw new ParseException("Multipart entity has not required 'boundary' paramter.");
		}
		MIME_b_Multipart mIME_b_Multipart = new MIME_b_Multipart(owner.ContentType);
		ParseInternal(owner, owner.ContentType.TypeWithSubtype, stream, mIME_b_Multipart);
		return mIME_b_Multipart;
	}

	protected static void ParseInternal(MIME_Entity owner, string mediaType, SmartStream stream, MIME_b_Multipart body)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (mediaType == null)
		{
			throw new ArgumentNullException("mediaType");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (owner.ContentType == null || owner.ContentType.Param_Boundary == null)
		{
			throw new ParseException("Multipart entity has not required 'boundary' parameter.");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		_MultipartReader multipartReader = new _MultipartReader(stream, owner.ContentType.Param_Boundary);
		while (multipartReader.Next())
		{
			MIME_Entity mIME_Entity = new MIME_Entity();
			mIME_Entity.Parse(new SmartStream(multipartReader, owner: false), Encoding.UTF8, body.DefaultBodyPartContentType);
			body.m_pBodyParts.Add(mIME_Entity);
			mIME_Entity.SetParent(owner);
		}
		body.m_TextPreamble = multipartReader.TextPreamble;
		body.m_TextEpilogue = multipartReader.TextEpilogue;
		body.BodyParts.SetModified(isModified: false);
	}

	internal override void SetParent(MIME_Entity entity, bool setContentType)
	{
		base.SetParent(entity, setContentType);
		if (setContentType && (base.Entity.ContentType == null || !string.Equals(base.Entity.ContentType.TypeWithSubtype, base.MediaType, StringComparison.InvariantCultureIgnoreCase)))
		{
			base.Entity.ContentType = base.ContentType;
		}
	}

	protected internal override void ToStream(Stream stream, MIME_Encoding_EncodedWord headerWordEncoder, Encoding headerParmetersCharset, bool headerReencode)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!string.IsNullOrEmpty(m_TextPreamble))
		{
			if (m_TextPreamble.EndsWith("\r\n"))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(m_TextPreamble);
				stream.Write(bytes, 0, bytes.Length);
			}
			else
			{
				byte[] bytes2 = Encoding.UTF8.GetBytes(m_TextPreamble + "\r\n");
				stream.Write(bytes2, 0, bytes2.Length);
			}
		}
		for (int i = 0; i < m_pBodyParts.Count; i++)
		{
			MIME_Entity mIME_Entity = m_pBodyParts[i];
			if (i == 0)
			{
				byte[] bytes3 = Encoding.UTF8.GetBytes("--" + base.Entity.ContentType.Param_Boundary + "\r\n");
				stream.Write(bytes3, 0, bytes3.Length);
			}
			else
			{
				byte[] bytes4 = Encoding.UTF8.GetBytes("\r\n--" + base.Entity.ContentType.Param_Boundary + "\r\n");
				stream.Write(bytes4, 0, bytes4.Length);
			}
			mIME_Entity.ToStream(stream, headerWordEncoder, headerParmetersCharset, headerReencode);
			if (i == m_pBodyParts.Count - 1)
			{
				byte[] bytes5 = Encoding.UTF8.GetBytes("\r\n--" + base.Entity.ContentType.Param_Boundary + "--");
				stream.Write(bytes5, 0, bytes5.Length);
			}
		}
		if (!string.IsNullOrEmpty(m_TextEpilogue))
		{
			if (m_TextEpilogue.StartsWith("\r\n"))
			{
				byte[] bytes6 = Encoding.UTF8.GetBytes(m_TextEpilogue);
				stream.Write(bytes6, 0, bytes6.Length);
			}
			else
			{
				byte[] bytes7 = Encoding.UTF8.GetBytes("\r\n" + m_TextEpilogue);
				stream.Write(bytes7, 0, bytes7.Length);
			}
		}
	}
}
