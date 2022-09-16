using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

public class BerGenerator : Asn1Generator
{
	private bool _tagged = false;

	private bool _isExplicit;

	private int _tagNo;

	protected BerGenerator(Stream outStream)
		: base(outStream)
	{
	}

	public BerGenerator(Stream outStream, int tagNo, bool isExplicit)
		: base(outStream)
	{
		_tagged = true;
		_isExplicit = isExplicit;
		_tagNo = tagNo;
	}

	public override void AddObject(Asn1Encodable obj)
	{
		new BerOutputStream(base.Out).WriteObject(obj);
	}

	public override Stream GetRawOutputStream()
	{
		return base.Out;
	}

	public override void Close()
	{
		WriteBerEnd();
	}

	private void WriteHdr(int tag)
	{
		base.Out.WriteByte((byte)tag);
		base.Out.WriteByte(128);
	}

	protected void WriteBerHeader(int tag)
	{
		if (_tagged)
		{
			int num = _tagNo | 0x80;
			if (_isExplicit)
			{
				WriteHdr(num | 0x20);
				WriteHdr(tag);
			}
			else if (((uint)tag & 0x20u) != 0)
			{
				WriteHdr(num | 0x20);
			}
			else
			{
				WriteHdr(num);
			}
		}
		else
		{
			WriteHdr(tag);
		}
	}

	protected void WriteBerBody(Stream contentStream)
	{
		Streams.PipeAll(contentStream, base.Out);
	}

	protected void WriteBerEnd()
	{
		base.Out.WriteByte(0);
		base.Out.WriteByte(0);
		if (_tagged && _isExplicit)
		{
			base.Out.WriteByte(0);
			base.Out.WriteByte(0);
		}
	}
}
