using System;
using System.Net.Sockets;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Crypto.Tls;

internal class DtlsRecordLayer : DatagramTransport, TlsCloseable
{
	private const int RECORD_HEADER_LENGTH = 13;

	private const int MAX_FRAGMENT_LENGTH = 16384;

	private const long TCP_MSL = 120000L;

	private const long RETRANSMIT_TIMEOUT = 240000L;

	private readonly DatagramTransport mTransport;

	private readonly TlsContext mContext;

	private readonly TlsPeer mPeer;

	private readonly ByteQueue mRecordQueue = new ByteQueue();

	private volatile bool mClosed = false;

	private volatile bool mFailed = false;

	private volatile ProtocolVersion mReadVersion = null;

	private volatile ProtocolVersion mWriteVersion = null;

	private volatile bool mInHandshake;

	private volatile int mPlaintextLimit;

	private DtlsEpoch mCurrentEpoch;

	private DtlsEpoch mPendingEpoch;

	private DtlsEpoch mReadEpoch;

	private DtlsEpoch mWriteEpoch;

	private DtlsHandshakeRetransmit mRetransmit = null;

	private DtlsEpoch mRetransmitEpoch = null;

	private Timeout mRetransmitTimeout = null;

	internal bool IsClosed => mClosed;

	internal virtual int ReadEpoch => mReadEpoch.Epoch;

	internal virtual ProtocolVersion ReadVersion
	{
		get
		{
			return mReadVersion;
		}
		set
		{
			mReadVersion = value;
		}
	}

	private static void SendDatagram(DatagramTransport sender, byte[] buf, int off, int len)
	{
		sender.Send(buf, off, len);
	}

	internal DtlsRecordLayer(DatagramTransport transport, TlsContext context, TlsPeer peer, byte contentType)
	{
		mTransport = transport;
		mContext = context;
		mPeer = peer;
		mInHandshake = true;
		mCurrentEpoch = new DtlsEpoch(0, new TlsNullCipher(context));
		mPendingEpoch = null;
		mReadEpoch = mCurrentEpoch;
		mWriteEpoch = mCurrentEpoch;
		SetPlaintextLimit(16384);
	}

	internal virtual void SetPlaintextLimit(int plaintextLimit)
	{
		mPlaintextLimit = plaintextLimit;
	}

	internal virtual void SetWriteVersion(ProtocolVersion writeVersion)
	{
		mWriteVersion = writeVersion;
	}

	internal virtual void InitPendingEpoch(TlsCipher pendingCipher)
	{
		if (mPendingEpoch != null)
		{
			throw new InvalidOperationException();
		}
		mPendingEpoch = new DtlsEpoch(mWriteEpoch.Epoch + 1, pendingCipher);
	}

	internal virtual void HandshakeSuccessful(DtlsHandshakeRetransmit retransmit)
	{
		if (mReadEpoch == mCurrentEpoch || mWriteEpoch == mCurrentEpoch)
		{
			throw new InvalidOperationException();
		}
		if (retransmit != null)
		{
			mRetransmit = retransmit;
			mRetransmitEpoch = mCurrentEpoch;
			mRetransmitTimeout = new Timeout(240000L);
		}
		mInHandshake = false;
		mCurrentEpoch = mPendingEpoch;
		mPendingEpoch = null;
	}

	internal virtual void ResetWriteEpoch()
	{
		if (mRetransmitEpoch != null)
		{
			mWriteEpoch = mRetransmitEpoch;
		}
		else
		{
			mWriteEpoch = mCurrentEpoch;
		}
	}

	public virtual int GetReceiveLimit()
	{
		return System.Math.Min(mPlaintextLimit, mReadEpoch.Cipher.GetPlaintextLimit(mTransport.GetReceiveLimit() - 13));
	}

	public virtual int GetSendLimit()
	{
		return System.Math.Min(mPlaintextLimit, mWriteEpoch.Cipher.GetPlaintextLimit(mTransport.GetSendLimit() - 13));
	}

	public virtual int Receive(byte[] buf, int off, int len, int waitMillis)
	{
		long currentTimeMillis = DateTimeUtilities.CurrentUnixMs();
		Timeout timeout = Timeout.ForWaitMillis(waitMillis, currentTimeMillis);
		byte[] array = null;
		while (waitMillis >= 0)
		{
			if (mRetransmitTimeout != null && mRetransmitTimeout.RemainingMillis(currentTimeMillis) < 1)
			{
				mRetransmit = null;
				mRetransmitEpoch = null;
				mRetransmitTimeout = null;
			}
			int num = System.Math.Min(len, GetReceiveLimit()) + 13;
			if (array == null || array.Length < num)
			{
				array = new byte[num];
			}
			int received = ReceiveRecord(array, 0, num, waitMillis);
			int num2 = ProcessRecord(received, array, buf, off);
			if (num2 >= 0)
			{
				return num2;
			}
			currentTimeMillis = DateTimeUtilities.CurrentUnixMs();
			waitMillis = Timeout.GetWaitMillis(timeout, currentTimeMillis);
		}
		return -1;
	}

	public virtual void Send(byte[] buf, int off, int len)
	{
		byte contentType = 23;
		if (mInHandshake || mWriteEpoch == mRetransmitEpoch)
		{
			contentType = 22;
			byte b = TlsUtilities.ReadUint8(buf, off);
			if (b == 20)
			{
				DtlsEpoch dtlsEpoch = null;
				if (mInHandshake)
				{
					dtlsEpoch = mPendingEpoch;
				}
				else if (mWriteEpoch == mRetransmitEpoch)
				{
					dtlsEpoch = mCurrentEpoch;
				}
				if (dtlsEpoch == null)
				{
					throw new InvalidOperationException();
				}
				byte[] array = new byte[1] { 1 };
				SendRecord(20, array, 0, array.Length);
				mWriteEpoch = dtlsEpoch;
			}
		}
		SendRecord(contentType, buf, off, len);
	}

	public virtual void Close()
	{
		if (!mClosed)
		{
			if (mInHandshake)
			{
				Warn(90, "User canceled handshake");
			}
			CloseTransport();
		}
	}

	internal virtual void Failed()
	{
		if (!mClosed)
		{
			mFailed = true;
			CloseTransport();
		}
	}

	internal virtual void Fail(byte alertDescription)
	{
		if (!mClosed)
		{
			try
			{
				RaiseAlert(2, alertDescription, null, null);
			}
			catch (Exception)
			{
			}
			mFailed = true;
			CloseTransport();
		}
	}

	internal virtual void Warn(byte alertDescription, string message)
	{
		RaiseAlert(1, alertDescription, message, null);
	}

	private void CloseTransport()
	{
		if (mClosed)
		{
			return;
		}
		try
		{
			if (!mFailed)
			{
				Warn(0, null);
			}
			mTransport.Close();
		}
		catch (Exception)
		{
		}
		mClosed = true;
	}

	private void RaiseAlert(byte alertLevel, byte alertDescription, string message, Exception cause)
	{
		mPeer.NotifyAlertRaised(alertLevel, alertDescription, message, cause);
		SendRecord(21, new byte[2] { alertLevel, alertDescription }, 0, 2);
	}

	private int ReceiveDatagram(byte[] buf, int off, int len, int waitMillis)
	{
		try
		{
			return mTransport.Receive(buf, off, len, waitMillis);
		}
		catch (SocketException ex)
		{
			if (TlsUtilities.IsTimeout(ex))
			{
				return -1;
			}
			throw ex;
		}
	}

	private int ProcessRecord(int received, byte[] record, byte[] buf, int off)
	{
		if (received < 13)
		{
			return -1;
		}
		int num = TlsUtilities.ReadUint16(record, 11);
		if (received != num + 13)
		{
			return -1;
		}
		byte b = TlsUtilities.ReadUint8(record, 0);
		switch (b)
		{
		default:
			return -1;
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		{
			int num2 = TlsUtilities.ReadUint16(record, 3);
			DtlsEpoch dtlsEpoch = null;
			if (num2 == mReadEpoch.Epoch)
			{
				dtlsEpoch = mReadEpoch;
			}
			else if (b == 22 && mRetransmitEpoch != null && num2 == mRetransmitEpoch.Epoch)
			{
				dtlsEpoch = mRetransmitEpoch;
			}
			if (dtlsEpoch == null)
			{
				return -1;
			}
			long num3 = TlsUtilities.ReadUint48(record, 5);
			if (dtlsEpoch.ReplayWindow.ShouldDiscard(num3))
			{
				return -1;
			}
			ProtocolVersion protocolVersion = TlsUtilities.ReadVersion(record, 1);
			if (!protocolVersion.IsDtls)
			{
				return -1;
			}
			if (mReadVersion != null && !mReadVersion.Equals(protocolVersion))
			{
				return -1;
			}
			byte[] array = dtlsEpoch.Cipher.DecodeCiphertext(GetMacSequenceNumber(dtlsEpoch.Epoch, num3), b, record, 13, received - 13);
			dtlsEpoch.ReplayWindow.ReportAuthenticated(num3);
			if (array.Length > mPlaintextLimit)
			{
				return -1;
			}
			if (mReadVersion == null)
			{
				mReadVersion = protocolVersion;
			}
			switch (b)
			{
			case 21:
				if (array.Length == 2)
				{
					byte b3 = array[0];
					byte b4 = array[1];
					mPeer.NotifyAlertReceived(b3, b4);
					if (b3 == 2)
					{
						Failed();
						throw new TlsFatalAlert(b4);
					}
					if (b4 == 0)
					{
						CloseTransport();
					}
				}
				return -1;
			case 23:
				if (mInHandshake)
				{
					return -1;
				}
				break;
			case 20:
			{
				for (int i = 0; i < array.Length; i++)
				{
					byte b2 = TlsUtilities.ReadUint8(array, i);
					if (b2 == 1 && mPendingEpoch != null)
					{
						mReadEpoch = mPendingEpoch;
					}
				}
				return -1;
			}
			case 22:
				if (!mInHandshake)
				{
					if (mRetransmit != null)
					{
						mRetransmit.ReceivedHandshakeRecord(num2, array, 0, array.Length);
					}
					return -1;
				}
				break;
			case 24:
				return -1;
			}
			if (!mInHandshake && mRetransmit != null)
			{
				mRetransmit = null;
				mRetransmitEpoch = null;
				mRetransmitTimeout = null;
			}
			Array.Copy(array, 0, buf, off, array.Length);
			return array.Length;
		}
		}
	}

	private int ReceiveRecord(byte[] buf, int off, int len, int waitMillis)
	{
		if (mRecordQueue.Available > 0)
		{
			int num = 0;
			if (mRecordQueue.Available >= 13)
			{
				byte[] buf2 = new byte[2];
				mRecordQueue.Read(buf2, 0, 2, 11);
				num = TlsUtilities.ReadUint16(buf2, 0);
			}
			int num2 = System.Math.Min(mRecordQueue.Available, 13 + num);
			mRecordQueue.RemoveData(buf, off, num2, 0);
			return num2;
		}
		int num3 = ReceiveDatagram(buf, off, len, waitMillis);
		if (num3 >= 13)
		{
			int num4 = TlsUtilities.ReadUint16(buf, off + 11);
			int num5 = 13 + num4;
			if (num3 > num5)
			{
				mRecordQueue.AddData(buf, off + num5, num3 - num5);
				num3 = num5;
			}
		}
		return num3;
	}

	private void SendRecord(byte contentType, byte[] buf, int off, int len)
	{
		if (mWriteVersion != null)
		{
			if (len > mPlaintextLimit)
			{
				throw new TlsFatalAlert(80);
			}
			if (len < 1 && contentType != 23)
			{
				throw new TlsFatalAlert(80);
			}
			int epoch = mWriteEpoch.Epoch;
			long num = mWriteEpoch.AllocateSequenceNumber();
			byte[] array = mWriteEpoch.Cipher.EncodePlaintext(GetMacSequenceNumber(epoch, num), contentType, buf, off, len);
			byte[] array2 = new byte[array.Length + 13];
			TlsUtilities.WriteUint8(contentType, array2, 0);
			ProtocolVersion version = mWriteVersion;
			TlsUtilities.WriteVersion(version, array2, 1);
			TlsUtilities.WriteUint16(epoch, array2, 3);
			TlsUtilities.WriteUint48(num, array2, 5);
			TlsUtilities.WriteUint16(array.Length, array2, 11);
			Array.Copy(array, 0, array2, 13, array.Length);
			SendDatagram(mTransport, array2, 0, array2.Length);
		}
	}

	private static long GetMacSequenceNumber(int epoch, long sequence_number)
	{
		return ((epoch & 0xFFFFFFFFu) << 48) | sequence_number;
	}
}
