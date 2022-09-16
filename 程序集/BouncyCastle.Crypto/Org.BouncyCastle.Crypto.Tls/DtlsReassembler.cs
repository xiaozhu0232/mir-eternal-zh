using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

internal class DtlsReassembler
{
	private class Range
	{
		private int mStart;

		private int mEnd;

		public int Start
		{
			get
			{
				return mStart;
			}
			set
			{
				mStart = value;
			}
		}

		public int End
		{
			get
			{
				return mEnd;
			}
			set
			{
				mEnd = value;
			}
		}

		internal Range(int start, int end)
		{
			mStart = start;
			mEnd = end;
		}
	}

	private readonly byte mMsgType;

	private readonly byte[] mBody;

	private readonly IList mMissing = Platform.CreateArrayList();

	internal byte MsgType => mMsgType;

	internal DtlsReassembler(byte msg_type, int length)
	{
		mMsgType = msg_type;
		mBody = new byte[length];
		mMissing.Add(new Range(0, length));
	}

	internal byte[] GetBodyIfComplete()
	{
		if (mMissing.Count != 0)
		{
			return null;
		}
		return mBody;
	}

	internal void ContributeFragment(byte msg_type, int length, byte[] buf, int off, int fragment_offset, int fragment_length)
	{
		int num = fragment_offset + fragment_length;
		if (mMsgType != msg_type || mBody.Length != length || num > length)
		{
			return;
		}
		if (fragment_length == 0)
		{
			if (fragment_offset == 0 && mMissing.Count > 0)
			{
				Range range = (Range)mMissing[0];
				if (range.End == 0)
				{
					mMissing.RemoveAt(0);
				}
			}
			return;
		}
		for (int i = 0; i < mMissing.Count; i++)
		{
			Range range2 = (Range)mMissing[i];
			if (range2.Start >= num)
			{
				break;
			}
			if (range2.End <= fragment_offset)
			{
				continue;
			}
			int num2 = System.Math.Max(range2.Start, fragment_offset);
			int num3 = System.Math.Min(range2.End, num);
			int length2 = num3 - num2;
			Array.Copy(buf, off + num2 - fragment_offset, mBody, num2, length2);
			if (num2 == range2.Start)
			{
				if (num3 == range2.End)
				{
					mMissing.RemoveAt(i--);
				}
				else
				{
					range2.Start = num3;
				}
				continue;
			}
			if (num3 != range2.End)
			{
				mMissing.Insert(++i, new Range(num3, range2.End));
			}
			range2.End = num2;
		}
	}

	internal void Reset()
	{
		mMissing.Clear();
		mMissing.Add(new Range(0, mBody.Length));
	}
}
