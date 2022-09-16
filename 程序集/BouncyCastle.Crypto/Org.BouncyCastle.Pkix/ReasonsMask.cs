using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Pkix;

internal class ReasonsMask
{
	private int _reasons;

	internal static readonly ReasonsMask AllReasons = new ReasonsMask(33023);

	internal bool IsAllReasons => _reasons == AllReasons._reasons;

	public ReasonFlags Reasons => new ReasonFlags(_reasons);

	internal ReasonsMask(int reasons)
	{
		_reasons = reasons;
	}

	internal ReasonsMask()
		: this(0)
	{
	}

	internal void AddReasons(ReasonsMask mask)
	{
		_reasons |= mask.Reasons.IntValue;
	}

	internal ReasonsMask Intersect(ReasonsMask mask)
	{
		ReasonsMask reasonsMask = new ReasonsMask();
		reasonsMask.AddReasons(new ReasonsMask(_reasons & mask.Reasons.IntValue));
		return reasonsMask;
	}

	internal bool HasNewReasons(ReasonsMask mask)
	{
		return (_reasons | (mask.Reasons.IntValue ^ _reasons)) != 0;
	}
}
