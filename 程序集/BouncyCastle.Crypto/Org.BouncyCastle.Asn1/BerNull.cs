using System;

namespace Org.BouncyCastle.Asn1;

public class BerNull : DerNull
{
	public new static readonly BerNull Instance = new BerNull(0);

	[Obsolete("Use static Instance object")]
	public BerNull()
	{
	}

	private BerNull(int dummy)
		: base(dummy)
	{
	}

	internal override void Encode(DerOutputStream derOut)
	{
		if (derOut is Asn1OutputStream || derOut is BerOutputStream)
		{
			derOut.WriteByte(5);
		}
		else
		{
			base.Encode(derOut);
		}
	}
}
