using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509.Store;

internal class X509CollectionStore : IX509Store
{
	private ICollection _local;

	internal X509CollectionStore(ICollection collection)
	{
		_local = Platform.CreateArrayList(collection);
	}

	public ICollection GetMatches(IX509Selector selector)
	{
		if (selector == null)
		{
			return Platform.CreateArrayList(_local);
		}
		IList list = Platform.CreateArrayList();
		foreach (object item in _local)
		{
			if (selector.Match(item))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
