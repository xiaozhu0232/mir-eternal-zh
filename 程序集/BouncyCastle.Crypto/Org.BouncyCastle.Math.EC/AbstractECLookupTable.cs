namespace Org.BouncyCastle.Math.EC;

public abstract class AbstractECLookupTable : ECLookupTable
{
	public abstract int Size { get; }

	public abstract ECPoint Lookup(int index);

	public virtual ECPoint LookupVar(int index)
	{
		return Lookup(index);
	}
}
