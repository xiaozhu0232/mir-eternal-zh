using System;

namespace Org.BouncyCastle.Math.EC;

public class SimpleLookupTable : AbstractECLookupTable
{
	private readonly ECPoint[] points;

	public override int Size => points.Length;

	private static ECPoint[] Copy(ECPoint[] points, int off, int len)
	{
		ECPoint[] array = new ECPoint[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = points[off + i];
		}
		return array;
	}

	public SimpleLookupTable(ECPoint[] points, int off, int len)
	{
		this.points = Copy(points, off, len);
	}

	public override ECPoint Lookup(int index)
	{
		throw new NotSupportedException("Constant-time lookup not supported");
	}

	public override ECPoint LookupVar(int index)
	{
		return points[index];
	}
}
