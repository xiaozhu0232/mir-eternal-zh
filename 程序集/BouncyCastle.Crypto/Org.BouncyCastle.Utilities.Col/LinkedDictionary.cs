using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

public class LinkedDictionary : IDictionary, ICollection, IEnumerable
{
	internal readonly IDictionary hash = Platform.CreateHashtable();

	internal readonly IList keys = Platform.CreateArrayList();

	public virtual int Count => hash.Count;

	public virtual bool IsFixedSize => false;

	public virtual bool IsReadOnly => false;

	public virtual bool IsSynchronized => false;

	public virtual object SyncRoot => false;

	public virtual ICollection Keys => Platform.CreateArrayList(keys);

	public virtual ICollection Values
	{
		get
		{
			IList list = Platform.CreateArrayList(keys.Count);
			foreach (object key in keys)
			{
				list.Add(hash[key]);
			}
			return list;
		}
	}

	public virtual object this[object k]
	{
		get
		{
			return hash[k];
		}
		set
		{
			if (!hash.Contains(k))
			{
				keys.Add(k);
			}
			hash[k] = value;
		}
	}

	public virtual void Add(object k, object v)
	{
		hash.Add(k, v);
		keys.Add(k);
	}

	public virtual void Clear()
	{
		hash.Clear();
		keys.Clear();
	}

	public virtual bool Contains(object k)
	{
		return hash.Contains(k);
	}

	public virtual void CopyTo(Array array, int index)
	{
		foreach (object key in keys)
		{
			array.SetValue(hash[key], index++);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return new LinkedDictionaryEnumerator(this);
	}

	public virtual void Remove(object k)
	{
		hash.Remove(k);
		keys.Remove(k);
	}
}
