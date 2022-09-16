using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

public class UnmodifiableDictionaryProxy : UnmodifiableDictionary
{
	private readonly IDictionary d;

	public override int Count => d.Count;

	public override bool IsFixedSize => d.IsFixedSize;

	public override bool IsSynchronized => d.IsSynchronized;

	public override object SyncRoot => d.SyncRoot;

	public override ICollection Keys => d.Keys;

	public override ICollection Values => d.Values;

	public UnmodifiableDictionaryProxy(IDictionary d)
	{
		this.d = d;
	}

	public override bool Contains(object k)
	{
		return d.Contains(k);
	}

	public override void CopyTo(Array array, int index)
	{
		d.CopyTo(array, index);
	}

	public override IDictionaryEnumerator GetEnumerator()
	{
		return d.GetEnumerator();
	}

	protected override object GetValue(object k)
	{
		return d[k];
	}
}
