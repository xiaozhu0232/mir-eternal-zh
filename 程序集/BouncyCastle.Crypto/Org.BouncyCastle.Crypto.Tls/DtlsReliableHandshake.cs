using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Crypto.Tls;

internal class DtlsReliableHandshake
{
	internal class Message
	{
		private readonly int mMessageSeq;

		private readonly byte mMsgType;

		private readonly byte[] mBody;

		public int Seq => mMessageSeq;

		public byte Type => mMsgType;

		public byte[] Body => mBody;

		internal Message(int message_seq, byte msg_type, byte[] body)
		{
			mMessageSeq = message_seq;
			mMsgType = msg_type;
			mBody = body;
		}
	}

	internal class RecordLayerBuffer : MemoryStream
	{
		internal RecordLayerBuffer(int size)
			: base(size)
		{
		}

		internal void SendToRecordLayer(DtlsRecordLayer recordLayer)
		{
			byte[] buffer = GetBuffer();
			int len = (int)Length;
			recordLayer.Send(buffer, 0, len);
			Platform.Dispose(this);
		}
	}

	internal class Retransmit : DtlsHandshakeRetransmit
	{
		private readonly DtlsReliableHandshake mOuter;

		internal Retransmit(DtlsReliableHandshake outer)
		{
			mOuter = outer;
		}

		public void ReceivedHandshakeRecord(int epoch, byte[] buf, int off, int len)
		{
			mOuter.ProcessRecord(0, epoch, buf, off, len);
		}
	}

	private const int MaxReceiveAhead = 16;

	private const int MessageHeaderLength = 12;

	private const int InitialResendMillis = 1000;

	private const int MaxResendMillis = 60000;

	private readonly DtlsRecordLayer mRecordLayer;

	private readonly Timeout mHandshakeTimeout;

	private TlsHandshakeHash mHandshakeHash;

	private IDictionary mCurrentInboundFlight = Platform.CreateHashtable();

	private IDictionary mPreviousInboundFlight = null;

	private IList mOutboundFlight = Platform.CreateArrayList();

	private int mResendMillis = -1;

	private Timeout mResendTimeout = null;

	private int mMessageSeq = 0;

	private int mNextReceiveSeq = 0;

	internal TlsHandshakeHash HandshakeHash => mHandshakeHash;

	internal DtlsReliableHandshake(TlsContext context, DtlsRecordLayer transport, int timeoutMillis)
	{
		mRecordLayer = transport;
		mHandshakeTimeout = Timeout.ForWaitMillis(timeoutMillis);
		mHandshakeHash = new DeferredHash();
		mHandshakeHash.Init(context);
	}

	internal void NotifyHelloComplete()
	{
		mHandshakeHash = mHandshakeHash.NotifyPrfDetermined();
	}

	internal TlsHandshakeHash PrepareToFinish()
	{
		TlsHandshakeHash result = mHandshakeHash;
		mHandshakeHash = mHandshakeHash.StopTracking();
		return result;
	}

	internal void SendMessage(byte msg_type, byte[] body)
	{
		TlsUtilities.CheckUint24(body.Length);
		if (mResendTimeout != null)
		{
			CheckInboundFlight();
			mResendMillis = -1;
			mResendTimeout = null;
			mOutboundFlight.Clear();
		}
		Message message = new Message(mMessageSeq++, msg_type, body);
		mOutboundFlight.Add(message);
		WriteMessage(message);
		UpdateHandshakeMessagesDigest(message);
	}

	internal byte[] ReceiveMessageBody(byte msg_type)
	{
		Message message = ReceiveMessage();
		if (message.Type != msg_type)
		{
			throw new TlsFatalAlert(10);
		}
		return message.Body;
	}

	internal Message ReceiveMessage()
	{
		long currentTimeMillis = DateTimeUtilities.CurrentUnixMs();
		if (mResendTimeout == null)
		{
			mResendMillis = 1000;
			mResendTimeout = new Timeout(mResendMillis, currentTimeMillis);
			PrepareInboundFlight(Platform.CreateHashtable());
		}
		byte[] array = null;
		while (true)
		{
			if (mRecordLayer.IsClosed)
			{
				throw new TlsFatalAlert(90);
			}
			Message pendingMessage = GetPendingMessage();
			if (pendingMessage != null)
			{
				return pendingMessage;
			}
			int waitMillis = Timeout.GetWaitMillis(mHandshakeTimeout, currentTimeMillis);
			if (waitMillis < 0)
			{
				break;
			}
			int num = System.Math.Max(1, Timeout.GetWaitMillis(mResendTimeout, currentTimeMillis));
			if (waitMillis > 0)
			{
				num = System.Math.Min(num, waitMillis);
			}
			int receiveLimit = mRecordLayer.GetReceiveLimit();
			if (array == null || array.Length < receiveLimit)
			{
				array = new byte[receiveLimit];
			}
			int num2 = mRecordLayer.Receive(array, 0, receiveLimit, num);
			if (num2 < 0)
			{
				ResendOutboundFlight();
			}
			else
			{
				ProcessRecord(16, mRecordLayer.ReadEpoch, array, 0, num2);
			}
			currentTimeMillis = DateTimeUtilities.CurrentUnixMs();
		}
		throw new TlsFatalAlert(40);
	}

	internal void Finish()
	{
		DtlsHandshakeRetransmit retransmit = null;
		if (mResendTimeout != null)
		{
			CheckInboundFlight();
		}
		else
		{
			PrepareInboundFlight(null);
			if (mPreviousInboundFlight != null)
			{
				retransmit = new Retransmit(this);
			}
		}
		mRecordLayer.HandshakeSuccessful(retransmit);
	}

	internal void ResetHandshakeMessagesDigest()
	{
		mHandshakeHash.Reset();
	}

	private int BackOff(int timeoutMillis)
	{
		return System.Math.Min(timeoutMillis * 2, 60000);
	}

	private void CheckInboundFlight()
	{
		foreach (object key in mCurrentInboundFlight.Keys)
		{
			int num = (int)key;
			_ = mNextReceiveSeq;
		}
	}

	private Message GetPendingMessage()
	{
		DtlsReassembler dtlsReassembler = (DtlsReassembler)mCurrentInboundFlight[mNextReceiveSeq];
		if (dtlsReassembler != null)
		{
			byte[] bodyIfComplete = dtlsReassembler.GetBodyIfComplete();
			if (bodyIfComplete != null)
			{
				mPreviousInboundFlight = null;
				return UpdateHandshakeMessagesDigest(new Message(mNextReceiveSeq++, dtlsReassembler.MsgType, bodyIfComplete));
			}
		}
		return null;
	}

	private void PrepareInboundFlight(IDictionary nextFlight)
	{
		ResetAll(mCurrentInboundFlight);
		mPreviousInboundFlight = mCurrentInboundFlight;
		mCurrentInboundFlight = nextFlight;
	}

	private void ProcessRecord(int windowSize, int epoch, byte[] buf, int off, int len)
	{
		bool flag = false;
		while (len >= 12)
		{
			int num = TlsUtilities.ReadUint24(buf, off + 9);
			int num2 = num + 12;
			if (len < num2)
			{
				break;
			}
			int num3 = TlsUtilities.ReadUint24(buf, off + 1);
			int num4 = TlsUtilities.ReadUint24(buf, off + 6);
			if (num4 + num > num3)
			{
				break;
			}
			byte b = TlsUtilities.ReadUint8(buf, off);
			int num5 = ((b == 20) ? 1 : 0);
			if (epoch != num5)
			{
				break;
			}
			int num6 = TlsUtilities.ReadUint16(buf, off + 4);
			if (num6 < mNextReceiveSeq + windowSize)
			{
				if (num6 >= mNextReceiveSeq)
				{
					DtlsReassembler dtlsReassembler = (DtlsReassembler)mCurrentInboundFlight[num6];
					if (dtlsReassembler == null)
					{
						dtlsReassembler = new DtlsReassembler(b, num3);
						mCurrentInboundFlight[num6] = dtlsReassembler;
					}
					dtlsReassembler.ContributeFragment(b, num3, buf, off + 12, num4, num);
				}
				else if (mPreviousInboundFlight != null)
				{
					DtlsReassembler dtlsReassembler2 = (DtlsReassembler)mPreviousInboundFlight[num6];
					if (dtlsReassembler2 != null)
					{
						dtlsReassembler2.ContributeFragment(b, num3, buf, off + 12, num4, num);
						flag = true;
					}
				}
			}
			off += num2;
			len -= num2;
		}
		if (flag && CheckAll(mPreviousInboundFlight))
		{
			ResendOutboundFlight();
			ResetAll(mPreviousInboundFlight);
		}
	}

	private void ResendOutboundFlight()
	{
		mRecordLayer.ResetWriteEpoch();
		for (int i = 0; i < mOutboundFlight.Count; i++)
		{
			WriteMessage((Message)mOutboundFlight[i]);
		}
		mResendMillis = BackOff(mResendMillis);
		mResendTimeout = new Timeout(mResendMillis);
	}

	private Message UpdateHandshakeMessagesDigest(Message message)
	{
		if (message.Type != 0)
		{
			byte[] body = message.Body;
			byte[] array = new byte[12];
			TlsUtilities.WriteUint8(message.Type, array, 0);
			TlsUtilities.WriteUint24(body.Length, array, 1);
			TlsUtilities.WriteUint16(message.Seq, array, 4);
			TlsUtilities.WriteUint24(0, array, 6);
			TlsUtilities.WriteUint24(body.Length, array, 9);
			mHandshakeHash.BlockUpdate(array, 0, array.Length);
			mHandshakeHash.BlockUpdate(body, 0, body.Length);
		}
		return message;
	}

	private void WriteMessage(Message message)
	{
		int sendLimit = mRecordLayer.GetSendLimit();
		int num = sendLimit - 12;
		if (num < 1)
		{
			throw new TlsFatalAlert(80);
		}
		int num2 = message.Body.Length;
		int num3 = 0;
		do
		{
			int num4 = System.Math.Min(num2 - num3, num);
			WriteHandshakeFragment(message, num3, num4);
			num3 += num4;
		}
		while (num3 < num2);
	}

	private void WriteHandshakeFragment(Message message, int fragment_offset, int fragment_length)
	{
		RecordLayerBuffer recordLayerBuffer = new RecordLayerBuffer(12 + fragment_length);
		TlsUtilities.WriteUint8(message.Type, recordLayerBuffer);
		TlsUtilities.WriteUint24(message.Body.Length, recordLayerBuffer);
		TlsUtilities.WriteUint16(message.Seq, recordLayerBuffer);
		TlsUtilities.WriteUint24(fragment_offset, recordLayerBuffer);
		TlsUtilities.WriteUint24(fragment_length, recordLayerBuffer);
		recordLayerBuffer.Write(message.Body, fragment_offset, fragment_length);
		recordLayerBuffer.SendToRecordLayer(mRecordLayer);
	}

	private static bool CheckAll(IDictionary inboundFlight)
	{
		foreach (DtlsReassembler value in inboundFlight.Values)
		{
			if (value.GetBodyIfComplete() == null)
			{
				return false;
			}
		}
		return true;
	}

	private static void ResetAll(IDictionary inboundFlight)
	{
		foreach (DtlsReassembler value in inboundFlight.Values)
		{
			value.Reset();
		}
	}
}
