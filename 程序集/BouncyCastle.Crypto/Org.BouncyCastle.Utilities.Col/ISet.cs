using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

public interface ISet : ICollection, IEnumerable
{
	bool IsEmpty { get; }

	bool IsFixedSize { get; }

	bool IsReadOnly { get; }

	void Add(object o);

	void AddAll(IEnumerable e);

	void Clear();

	bool Contains(object o);

	void Remove(object o);

	void RemoveAll(IEnumerable e);
}
