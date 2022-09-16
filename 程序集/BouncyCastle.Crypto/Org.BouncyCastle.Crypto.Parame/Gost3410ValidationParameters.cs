namespace Org.BouncyCastle.Crypto.Parameters;

public class Gost3410ValidationParameters
{
	private int x0;

	private int c;

	private long x0L;

	private long cL;

	public int C => c;

	public int X0 => x0;

	public long CL => cL;

	public long X0L => x0L;

	public Gost3410ValidationParameters(int x0, int c)
	{
		this.x0 = x0;
		this.c = c;
	}

	public Gost3410ValidationParameters(long x0L, long cL)
	{
		this.x0L = x0L;
		this.cL = cL;
	}

	public override bool Equals(object obj)
	{
		if (obj is Gost3410ValidationParameters gost3410ValidationParameters && gost3410ValidationParameters.c == c && gost3410ValidationParameters.x0 == x0 && gost3410ValidationParameters.cL == cL)
		{
			return gost3410ValidationParameters.x0L == x0L;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return c.GetHashCode() ^ x0.GetHashCode() ^ cL.GetHashCode() ^ x0L.GetHashCode();
	}
}
