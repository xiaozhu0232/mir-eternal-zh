using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public abstract class PgpEncryptedData
{
	internal class TruncatedStream : BaseInputStream
	{
		private const int LookAheadSize = 22;

		private const int LookAheadBufSize = 512;

		private const int LookAheadBufLimit = 490;

		private readonly Stream inStr;

		private readonly byte[] lookAhead = new byte[512];

		private int bufStart;

		private int bufEnd;

		internal TruncatedStream(Stream inStr)
		{
			int num = Streams.ReadFully(inStr, lookAhead, 0, lookAhead.Length);
			if (num < 22)
			{
				throw new EndOfStreamException();
			}
			this.inStr = inStr;
			bufStart = 0;
			bufEnd = num - 22;
		}

		private int FillBuffer()
		{
			if (bufEnd < 490)
			{
				return 0;
			}
			Array.Copy(lookAhead, 490, lookAhead, 0, 22);
			bufEnd = Streams.ReadFully(inStr, lookAhead, 22, 490);
			bufStart = 0;
			return bufEnd;
		}

		public override int ReadByte()
		{
			if (bufStart < bufEnd)
			{
				return lookAhead[bufStart++];
			}
			if (FillBuffer() < 1)
			{
				return -1;
			}
			return lookAhead[bufStart++];
		}

		public override int Read(byte[] buf, int off, int len)
		{
			int num = bufEnd - bufStart;
			int num2 = off;
			while (len > num)
			{
				Array.Copy(lookAhead, bufStart, buf, num2, num);
				bufStart += num;
				num2 += num;
				len -= num;
				if ((num = FillBuffer()) < 1)
				{
					return num2 - off;
				}
			}
			Array.Copy(lookAhead, bufStart, buf, num2, len);
			bufStart += len;
			return num2 + len - off;
		}

		internal byte[] GetLookAhead()
		{
			byte[] array = new byte[22];
			Array.Copy(lookAhead, bufStart, array, 0, 22);
			return array;
		}
	}

	internal InputStreamPacket encData;

	internal Stream encStream;

	internal TruncatedStream truncStream;

	internal PgpEncryptedData(InputStreamPacket encData)
	{
		this.encData = encData;
	}

	public virtual Stream GetInputStream()
	{
		return encData.GetInputStream();
	}

	public bool IsIntegrityProtected()
	{
		return encData is SymmetricEncIntegrityPacket;
	}

	public bool Verify()
	{
		if (!IsIntegrityProtected())
		{
			throw new PgpException("data not integrity protected.");
		}
		DigestStream digestStream = (DigestStream)encStream;
		while (encStream.ReadByte() >= 0)
		{
		}
		byte[] lookAhead = truncStream.GetLookAhead();
		IDigest digest = digestStream.ReadDigest();
		digest.BlockUpdate(lookAhead, 0, 2);
		byte[] array = DigestUtilities.DoFinal(digest);
		byte[] array2 = new byte[array.Length];
		Array.Copy(lookAhead, 2, array2, 0, array2.Length);
		return Arrays.ConstantTimeAreEqual(array, array2);
	}
}
