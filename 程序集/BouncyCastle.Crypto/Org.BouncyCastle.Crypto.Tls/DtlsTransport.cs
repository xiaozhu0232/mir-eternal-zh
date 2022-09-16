using System;
using System.IO;
using System.Net.Sockets;

namespace Org.BouncyCastle.Crypto.Tls;

public class DtlsTransport : DatagramTransport, TlsCloseable
{
	private readonly DtlsRecordLayer mRecordLayer;

	internal DtlsTransport(DtlsRecordLayer recordLayer)
	{
		mRecordLayer = recordLayer;
	}

	public virtual int GetReceiveLimit()
	{
		return mRecordLayer.GetReceiveLimit();
	}

	public virtual int GetSendLimit()
	{
		return mRecordLayer.GetSendLimit();
	}

	public virtual int Receive(byte[] buf, int off, int len, int waitMillis)
	{
		if (buf == null)
		{
			throw new ArgumentNullException("buf");
		}
		if (off < 0 || off >= buf.Length)
		{
			throw new ArgumentException("invalid offset: " + off, "off");
		}
		if (len < 0 || len > buf.Length - off)
		{
			throw new ArgumentException("invalid length: " + len, "len");
		}
		if (waitMillis < 0)
		{
			throw new ArgumentException("cannot be negative", "waitMillis");
		}
		try
		{
			return mRecordLayer.Receive(buf, off, len, waitMillis);
		}
		catch (TlsFatalAlert tlsFatalAlert)
		{
			mRecordLayer.Fail(tlsFatalAlert.AlertDescription);
			throw tlsFatalAlert;
		}
		catch (IOException ex)
		{
			mRecordLayer.Fail(80);
			throw ex;
		}
		catch (SocketException ex2)
		{
			if (TlsUtilities.IsTimeout(ex2))
			{
				throw ex2;
			}
			mRecordLayer.Fail(80);
			throw new TlsFatalAlert(80, ex2);
		}
		catch (Exception alertCause)
		{
			mRecordLayer.Fail(80);
			throw new TlsFatalAlert(80, alertCause);
		}
	}

	public virtual void Send(byte[] buf, int off, int len)
	{
		if (buf == null)
		{
			throw new ArgumentNullException("buf");
		}
		if (off < 0 || off >= buf.Length)
		{
			throw new ArgumentException("invalid offset: " + off, "off");
		}
		if (len < 0 || len > buf.Length - off)
		{
			throw new ArgumentException("invalid length: " + len, "len");
		}
		try
		{
			mRecordLayer.Send(buf, off, len);
		}
		catch (TlsFatalAlert tlsFatalAlert)
		{
			mRecordLayer.Fail(tlsFatalAlert.AlertDescription);
			throw tlsFatalAlert;
		}
		catch (IOException ex)
		{
			mRecordLayer.Fail(80);
			throw ex;
		}
		catch (SocketException ex2)
		{
			if (TlsUtilities.IsTimeout(ex2))
			{
				throw ex2;
			}
			mRecordLayer.Fail(80);
			throw new TlsFatalAlert(80, ex2);
		}
		catch (Exception alertCause)
		{
			mRecordLayer.Fail(80);
			throw new TlsFatalAlert(80, alertCause);
		}
	}

	public virtual void Close()
	{
		mRecordLayer.Close();
	}
}
