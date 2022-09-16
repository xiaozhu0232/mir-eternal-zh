using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

public abstract class UnmodifiableSet : ISet, ICollection, IEnumerable
{
	public abstract int Count { get; }

	public abstract bool IsEmpty { get; }

	public abstract bool IsFixedSize { get; }

	public virtual bool IsReadOnly => true;

	public abstract bool IsSynchronized { get; }

	public abstract object SyncRoot { get; }

	public virtual void Add(object o)
	{
		throw new NotSupportedException();
	}

	public virtual void AddAll(IEnumerable e)
	{
		throw new NotSupportedException();
	}

	public virtual void Clear()
	{
		throw new NotSupportedException();
	}

	public abstract bool Contains(object o);

	public abstract void CopyTo(Array array, int index);

	public abstract IEnumerator GetEnumerator();

	public virtual void Remove(object o)
	{
		throw new NotSupportedException();
	}

	public virtual void RemoveAll(IEnumerable e)
	{
		throw new NotSupportedException();
	}
}
