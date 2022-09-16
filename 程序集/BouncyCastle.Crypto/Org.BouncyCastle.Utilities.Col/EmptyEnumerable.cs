using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

public sealed class EmptyEnumerable : IEnumerable
{
	public static readonly IEnumerable Instance = new EmptyEnumerable();

	private EmptyEnumerable()
	{
	}

	public IEnumerator GetEnumerator()
	{
		return EmptyEnumerator.Instance;
	}
}
