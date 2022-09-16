using System;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes;

public class ChaCha20Poly1305 : IAeadCipher
{
	private enum State
	{
		Uninitialized,
		EncInit,
		EncAad,
		EncData,
		EncFinal,
		DecInit,
		DecAad,
		DecData,
		DecFinal
	}

	private const int BufSize = 64;

	private const int KeySize = 32;

	private const int NonceSize = 12;

	private const int MacSize = 16;

	private const ulong AadLimit = ulong.MaxValue;

	private const ulong DataLimit = 274877906880uL;

	private static readonly byte[] Zeroes = new byte[15];

	private readonly ChaCha7539Engine mChacha20;

	private readonly IMac mPoly1305;

	private readonly byte[] mKey = new byte[32];

	private readonly byte[] mNonce = new byte[12];

	private readonly byte[] mBuf = new byte[80];

	private readonly byte[] mMac = new byte[16];

	private byte[] mInitialAad;

	private ulong mAadCount;

	private ulong mDataCount;

	private State mState = State.Uninitialized;

	private int mBufPos;

	public virtual string AlgorithmName => "ChaCha20Poly1305";

	public ChaCha20Poly1305()
		: this(new Poly1305())
	{
	}

	public ChaCha20Poly1305(IMac poly1305)
	{
		if (poly1305 == null)
		{
			throw new ArgumentNullException("poly1305");
		}
		if (16 != poly1305.GetMacSize())
		{
			throw new ArgumentException("must be a 128-bit MAC", "poly1305");
		}
		mChacha20 = new ChaCha7539Engine();
		mPoly1305 = poly1305;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		KeyParameter keyParameter;
		byte[] array;
		ICipherParameters parameters2;
		if (parameters is AeadParameters)
		{
			AeadParameters aeadParameters = (AeadParameters)parameters;
			int macSize = aeadParameters.MacSize;
			if (128 != macSize)
			{
				throw new ArgumentException("Invalid value for MAC size: " + macSize);
			}
			keyParameter = aeadParameters.Key;
			array = aeadParameters.GetNonce();
			parameters2 = new ParametersWithIV(keyParameter, array);
			mInitialAad = aeadParameters.GetAssociatedText();
		}
		else
		{
			if (!(parameters is ParametersWithIV))
			{
				throw new ArgumentException("invalid parameters passed to ChaCha20Poly1305", "parameters");
			}
			ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
			keyParameter = (KeyParameter)parametersWithIV.Parameters;
			array = parametersWithIV.GetIV();
			parameters2 = parametersWithIV;
			mInitialAad = null;
		}
		if (keyParameter == null)
		{
			if (mState == State.Uninitialized)
			{
				throw new ArgumentException("Key must be specified in initial init");
			}
		}
		else if (32 != keyParameter.GetKey().Length)
		{
			throw new ArgumentException("Key must be 256 bits");
		}
		if (array == null || 12 != array.Length)
		{
			throw new ArgumentException("Nonce must be 96 bits");
		}
		if (mState != 0 && forEncryption && Arrays.AreEqual(mNonce, array) && (keyParameter == null || Arrays.AreEqual(mKey, keyParameter.GetKey())))
		{
			throw new ArgumentException("cannot reuse nonce for ChaCha20Poly1305 encryption");
		}
		if (keyParameter != null)
		{
			Array.Copy(keyParameter.GetKey(), 0, mKey, 0, 32);
		}
		Array.Copy(array, 0, mNonce, 0, 12);
		mChacha20.Init(forEncryption: true, parameters2);
		mState = (forEncryption ? State.EncInit : State.DecInit);
		Reset(clearMac: true, resetCipher: false);
	}

	public virtual int GetOutputSize(int len)
	{
		int num = System.Math.Max(0, len) + mBufPos;
		switch (mState)
		{
		case State.DecInit:
		case State.DecAad:
		case State.DecData:
			return System.Math.Max(0, num - 16);
		case State.EncInit:
		case State.EncAad:
		case State.EncData:
			return num + 16;
		default:
			throw new InvalidOperationException();
		}
	}

	public virtual int GetUpdateOutputSize(int len)
	{
		int num = System.Math.Max(0, len) + mBufPos;
		switch (mState)
		{
		case State.DecInit:
		case State.DecAad:
		case State.DecData:
			num = System.Math.Max(0, num - 16);
			break;
		default:
			throw new InvalidOperationException();
		case State.EncInit:
		case State.EncAad:
		case State.EncData:
			break;
		}
		return num - num % 64;
	}

	public virtual void ProcessAadByte(byte input)
	{
		CheckAad();
		mAadCount = IncrementCount(mAadCount, 1u, ulong.MaxValue);
		mPoly1305.Update(input);
	}

	public virtual void ProcessAadBytes(byte[] inBytes, int inOff, int len)
	{
		if (inBytes == null)
		{
			throw new ArgumentNullException("inBytes");
		}
		if (inOff < 0)
		{
			throw new ArgumentException("cannot be negative", "inOff");
		}
		if (len < 0)
		{
			throw new ArgumentException("cannot be negative", "len");
		}
		Check.DataLength(inBytes, inOff, len, "input buffer too short");
		CheckAad();
		if (len > 0)
		{
			mAadCount = IncrementCount(mAadCount, (uint)len, ulong.MaxValue);
			mPoly1305.BlockUpdate(inBytes, inOff, len);
		}
	}

	public virtual int ProcessByte(byte input, byte[] outBytes, int outOff)
	{
		CheckData();
		switch (mState)
		{
		case State.DecData:
			mBuf[mBufPos] = input;
			if (++mBufPos == mBuf.Length)
			{
				mPoly1305.BlockUpdate(mBuf, 0, 64);
				ProcessData(mBuf, 0, 64, outBytes, outOff);
				Array.Copy(mBuf, 64, mBuf, 0, 16);
				mBufPos = 16;
				return 64;
			}
			return 0;
		case State.EncData:
			mBuf[mBufPos] = input;
			if (++mBufPos == 64)
			{
				ProcessData(mBuf, 0, 64, outBytes, outOff);
				mPoly1305.BlockUpdate(outBytes, outOff, 64);
				mBufPos = 0;
				return 64;
			}
			return 0;
		default:
			throw new InvalidOperationException();
		}
	}

	public virtual int ProcessBytes(byte[] inBytes, int inOff, int len, byte[] outBytes, int outOff)
	{
		if (inBytes == null)
		{
			throw new ArgumentNullException("inBytes");
		}
		if (inOff < 0)
		{
			throw new ArgumentException("cannot be negative", "inOff");
		}
		if (len < 0)
		{
			throw new ArgumentException("cannot be negative", "len");
		}
		Check.DataLength(inBytes, inOff, len, "input buffer too short");
		if (outOff < 0)
		{
			throw new ArgumentException("cannot be negative", "outOff");
		}
		CheckData();
		int num = 0;
		switch (mState)
		{
		case State.DecData:
		{
			for (int i = 0; i < len; i++)
			{
				mBuf[mBufPos] = inBytes[inOff + i];
				if (++mBufPos == mBuf.Length)
				{
					mPoly1305.BlockUpdate(mBuf, 0, 64);
					ProcessData(mBuf, 0, 64, outBytes, outOff + num);
					Array.Copy(mBuf, 64, mBuf, 0, 16);
					mBufPos = 16;
					num += 64;
				}
			}
			break;
		}
		case State.EncData:
			if (mBufPos != 0)
			{
				while (len > 0)
				{
					len--;
					mBuf[mBufPos] = inBytes[inOff++];
					if (++mBufPos == 64)
					{
						ProcessData(mBuf, 0, 64, outBytes, outOff);
						mPoly1305.BlockUpdate(outBytes, outOff, 64);
						mBufPos = 0;
						num = 64;
						break;
					}
				}
			}
			while (len >= 64)
			{
				ProcessData(inBytes, inOff, 64, outBytes, outOff + num);
				mPoly1305.BlockUpdate(outBytes, outOff + num, 64);
				inOff += 64;
				len -= 64;
				num += 64;
			}
			if (len > 0)
			{
				Array.Copy(inBytes, inOff, mBuf, 0, len);
				mBufPos = len;
			}
			break;
		default:
			throw new InvalidOperationException();
		}
		return num;
	}

	public virtual int DoFinal(byte[] outBytes, int outOff)
	{
		if (outBytes == null)
		{
			throw new ArgumentNullException("outBytes");
		}
		if (outOff < 0)
		{
			throw new ArgumentException("cannot be negative", "outOff");
		}
		CheckData();
		Array.Clear(mMac, 0, 16);
		int num = 0;
		switch (mState)
		{
		case State.DecData:
			if (mBufPos < 16)
			{
				throw new InvalidCipherTextException("data too short");
			}
			num = mBufPos - 16;
			Check.OutputLength(outBytes, outOff, num, "output buffer too short");
			if (num > 0)
			{
				mPoly1305.BlockUpdate(mBuf, 0, num);
				ProcessData(mBuf, 0, num, outBytes, outOff);
			}
			FinishData(State.DecFinal);
			if (!Arrays.ConstantTimeAreEqual(16, mMac, 0, mBuf, num))
			{
				throw new InvalidCipherTextException("mac check in ChaCha20Poly1305 failed");
			}
			break;
		case State.EncData:
			num = mBufPos + 16;
			Check.OutputLength(outBytes, outOff, num, "output buffer too short");
			if (mBufPos > 0)
			{
				ProcessData(mBuf, 0, mBufPos, outBytes, outOff);
				mPoly1305.BlockUpdate(outBytes, outOff, mBufPos);
			}
			FinishData(State.EncFinal);
			Array.Copy(mMac, 0, outBytes, outOff + mBufPos, 16);
			break;
		default:
			throw new InvalidOperationException();
		}
		Reset(clearMac: false, resetCipher: true);
		return num;
	}

	public virtual byte[] GetMac()
	{
		return Arrays.Clone(mMac);
	}

	public virtual void Reset()
	{
		Reset(clearMac: true, resetCipher: true);
	}

	private void CheckAad()
	{
		switch (mState)
		{
		case State.DecInit:
			mState = State.DecAad;
			break;
		case State.EncInit:
			mState = State.EncAad;
			break;
		case State.EncFinal:
			throw new InvalidOperationException("ChaCha20Poly1305 cannot be reused for encryption");
		default:
			throw new InvalidOperationException();
		case State.EncAad:
		case State.DecAad:
			break;
		}
	}

	private void CheckData()
	{
		switch (mState)
		{
		case State.DecInit:
		case State.DecAad:
			FinishAad(State.DecData);
			break;
		case State.EncInit:
		case State.EncAad:
			FinishAad(State.EncData);
			break;
		case State.EncFinal:
			throw new InvalidOperationException("ChaCha20Poly1305 cannot be reused for encryption");
		default:
			throw new InvalidOperationException();
		case State.EncData:
		case State.DecData:
			break;
		}
	}

	private void FinishAad(State nextState)
	{
		PadMac(mAadCount);
		mState = nextState;
	}

	private void FinishData(State nextState)
	{
		PadMac(mDataCount);
		byte[] array = new byte[16];
		Pack.UInt64_To_LE(mAadCount, array, 0);
		Pack.UInt64_To_LE(mDataCount, array, 8);
		mPoly1305.BlockUpdate(array, 0, 16);
		mPoly1305.DoFinal(mMac, 0);
		mState = nextState;
	}

	private ulong IncrementCount(ulong count, uint increment, ulong limit)
	{
		if (count > limit - increment)
		{
			throw new InvalidOperationException("Limit exceeded");
		}
		return count + increment;
	}

	private void InitMac()
	{
		byte[] array = new byte[64];
		try
		{
			mChacha20.ProcessBytes(array, 0, 64, array, 0);
			mPoly1305.Init(new KeyParameter(array, 0, 32));
		}
		finally
		{
			Array.Clear(array, 0, 64);
		}
	}

	private void PadMac(ulong count)
	{
		int num = (int)count & 0xF;
		if (num != 0)
		{
			mPoly1305.BlockUpdate(Zeroes, 0, 16 - num);
		}
	}

	private void ProcessData(byte[] inBytes, int inOff, int inLen, byte[] outBytes, int outOff)
	{
		Check.OutputLength(outBytes, outOff, inLen, "output buffer too short");
		mChacha20.ProcessBytes(inBytes, inOff, inLen, outBytes, outOff);
		mDataCount = IncrementCount(mDataCount, (uint)inLen, 274877906880uL);
	}

	private void Reset(bool clearMac, bool resetCipher)
	{
		Array.Clear(mBuf, 0, mBuf.Length);
		if (clearMac)
		{
			Array.Clear(mMac, 0, mMac.Length);
		}
		mAadCount = 0uL;
		mDataCount = 0uL;
		mBufPos = 0;
		switch (mState)
		{
		case State.DecAad:
		case State.DecData:
		case State.DecFinal:
			mState = State.DecInit;
			break;
		case State.EncAad:
		case State.EncData:
		case State.EncFinal:
			mState = State.EncFinal;
			return;
		default:
			throw new InvalidOperationException();
		case State.EncInit:
		case State.DecInit:
			break;
		}
		if (resetCipher)
		{
			mChacha20.Reset();
		}
		InitMac();
		if (mInitialAad != null)
		{
			ProcessAadBytes(mInitialAad, 0, mInitialAad.Length);
		}
	}
}
