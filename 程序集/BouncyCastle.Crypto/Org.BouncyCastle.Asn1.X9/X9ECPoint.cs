using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X9;

public class X9ECPoint : Asn1Encodable
{
	private readonly Asn1OctetString encoding;

	private ECCurve c;

	private ECPoint p;

	public ECPoint Point
	{
		get
		{
			if (p == null)
			{
				p = c.DecodePoint(encoding.GetOctets()).Normalize();
			}
			return p;
		}
	}

	public bool IsPointCompressed
	{
		get
		{
			byte[] octets = encoding.GetOctets();
			if (octets != null && octets.Length > 0)
			{
				if (octets[0] != 2)
				{
					return octets[0] == 3;
				}
				return true;
			}
			return false;
		}
	}

	public X9ECPoint(ECPoint p)
		: this(p, compressed: false)
	{
	}

	public X9ECPoint(ECPoint p, bool compressed)
	{
		this.p = p.Normalize();
		encoding = new DerOctetString(p.GetEncoded(compressed));
	}

	public X9ECPoint(ECCurve c, byte[] encoding)
	{
		this.c = c;
		this.encoding = new DerOctetString(Arrays.Clone(encoding));
	}

	public X9ECPoint(ECCurve c, Asn1OctetString s)
		: this(c, s.GetOctets())
	{
	}

	public byte[] GetPointEncoding()
	{
		return Arrays.Clone(encoding.GetOctets());
	}

	public override Asn1Object ToAsn1Object()
	{
		return encoding;
	}
}
