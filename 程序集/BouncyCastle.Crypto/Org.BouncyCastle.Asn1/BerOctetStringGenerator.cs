using System;
using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

public class BerOctetStringGenerator : BerGenerator
{
	private class BufferedBerOctetStream : BaseOutputStream
	{
		private byte[] _buf;

		private int _off;

		private readonly BerOctetStringGenerator _gen;

		private readonly DerOutputStream _derOut;

		internal BufferedBerOctetStream(BerOctetStringGenerator gen, byte[] buf)
		{
			_gen = gen;
			_buf = buf;
			_off = 0;
			_derOut = new DerOutputStream(_gen.Out);
		}

		public override void WriteByte(byte b)
		{
			_buf[_off++] = b;
			if (_off == _buf.Length)
			{
				DerOctetString.Encode(_derOut, _buf, 0, _off);
				_off = 0;
			}
		}

		public override void Write(byte[] buf, int offset, int len)
		{
			while (len > 0)
			{
				int num = System.Math.Min(len, _buf.Length - _off);
				if (num == _buf.Length)
				{
					DerOctetString.Encode(_derOut, buf, offset, num);
				}
				else
				{
					Array.Copy(buf, offset, _buf, _off, num);
					_off += num;
					if (_off < _buf.Length)
					{
						break;
					}
					DerOctetString.Encode(_derOut, _buf, 0, _off);
					_off = 0;
				}
				offset += num;
				len -= num;
			}
		}

		public override void Close()
		{
			if (_off != 0)
			{
				DerOctetString.Encode(_derOut, _buf, 0, _off);
			}
			_gen.WriteBerEnd();
			base.Close();
		}
	}

	public BerOctetStringGenerator(Stream outStream)
		: base(outStream)
	{
		WriteBerHeader(36);
	}

	public BerOctetStringGenerator(Stream outStream, int tagNo, bool isExplicit)
		: base(outStream, tagNo, isExplicit)
	{
		WriteBerHeader(36);
	}

	public Stream GetOctetOutputStream()
	{
		return GetOctetOutputStream(new byte[1000]);
	}

	public Stream GetOctetOutputStream(int bufSize)
	{
		if (bufSize >= 1)
		{
			return GetOctetOutputStream(new byte[bufSize]);
		}
		return GetOctetOutputStream();
	}

	public Stream GetOctetOutputStream(byte[] buf)
	{
		return new BufferedBerOctetStream(this, buf);
	}
}
