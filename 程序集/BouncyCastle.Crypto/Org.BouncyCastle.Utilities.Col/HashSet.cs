using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

public class HashSet : ISet, ICollection, IEnumerable
{
	private readonly IDictionary impl = Platform.CreateHashtable();

	public virtual int Count => impl.Count;

	public virtual bool IsEmpty => impl.Count == 0;

	public virtual bool IsFixedSize => impl.IsFixedSize;

	public virtual bool IsReadOnly => impl.IsReadOnly;

	public virtual bool IsSynchronized => impl.IsSynchronized;

	public virtual object SyncRoot => impl.SyncRoot;

	public HashSet()
	{
	}

	public HashSet(IEnumerable s)
	{
		foreach (object item in s)
		{
			Add(item);
		}
	}

	public virtual void Add(object o)
	{
		impl[o] = null;
	}

	public virtual void AddAll(IEnumerable e)
	{
		foreach (object item in e)
		{
			Add(item);
		}
	}

	public virtual void Clear()
	{
		impl.Clear();
	}

	public virtual bool Contains(object o)
	{
		return impl.Contains(o);
	}

	public virtual void CopyTo(Array array, int index)
	{
		impl.Keys.CopyTo(array, index);
	}

	public virtual IEnumerator GetEnumerator()
	{
		return impl.Keys.GetEnumerator();
	}

	public virtual void Remove(object o)
	{
		impl.Remove(o);
	}

	public virtual void RemoveAll(IEnumerable e)
	{
		foreach (object item in e)
		{
			Remove(item);
		}
	}
}
