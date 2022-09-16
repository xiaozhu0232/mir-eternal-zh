using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1;

internal class ConstructedOctetStream : BaseInputStream
{
	private readonly Asn1StreamParser _parser;

	private bool _first = true;

	private Stream _currentStream;

	internal ConstructedOctetStream(Asn1StreamParser parser)
	{
		_parser = parser;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_currentStream == null)
		{
			if (!_first)
			{
				return 0;
			}
			Asn1OctetStringParser nextParser = GetNextParser();
			if (nextParser == null)
			{
				return 0;
			}
			_first = false;
			_currentStream = nextParser.GetOctetStream();
		}
		int num = 0;
		while (true)
		{
			int num2 = _currentStream.Read(buffer, offset + num, count - num);
			if (num2 > 0)
			{
				num += num2;
				if (num == count)
				{
					return num;
				}
				continue;
			}
			Asn1OctetStringParser nextParser2 = GetNextParser();
			if (nextParser2 == null)
			{
				break;
			}
			_currentStream = nextParser2.GetOctetStream();
		}
		_currentStream = null;
		return num;
	}

	public override int ReadByte()
	{
		if (_currentStream == null)
		{
			if (!_first)
			{
				return 0;
			}
			Asn1OctetStringParser nextParser = GetNextParser();
			if (nextParser == null)
			{
				return 0;
			}
			_first = false;
			_currentStream = nextParser.GetOctetStream();
		}
		while (true)
		{
			int num = _currentStream.ReadByte();
			if (num >= 0)
			{
				return num;
			}
			Asn1OctetStringParser nextParser2 = GetNextParser();
			if (nextParser2 == null)
			{
				break;
			}
			_currentStream = nextParser2.GetOctetStream();
		}
		_currentStream = null;
		return -1;
	}

	private Asn1OctetStringParser GetNextParser()
	{
		IAsn1Convertible asn1Convertible = _parser.ReadObject();
		if (asn1Convertible == null)
		{
			return null;
		}
		if (asn1Convertible is Asn1OctetStringParser)
		{
			return (Asn1OctetStringParser)asn1Convertible;
		}
		throw new IOException("unknown object encountered: " + Platform.GetTypeName(asn1Convertible));
	}
}
