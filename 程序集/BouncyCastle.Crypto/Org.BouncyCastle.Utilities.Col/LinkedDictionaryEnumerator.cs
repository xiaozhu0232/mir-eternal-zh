using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections;

internal class LinkedDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
{
	private readonly LinkedDictionary parent;

	private int pos = -1;

	public virtual object Current => Entry;

	public virtual DictionaryEntry Entry
	{
		get
		{
			object currentKey = CurrentKey;
			return new DictionaryEntry(currentKey, parent.hash[currentKey]);
		}
	}

	public virtual object Key => CurrentKey;

	public virtual object Value => parent.hash[CurrentKey];

	private object CurrentKey
	{
		get
		{
			if (pos < 0 || pos >= parent.keys.Count)
			{
				throw new InvalidOperationException();
			}
			return parent.keys[pos];
		}
	}

	internal LinkedDictionaryEnumerator(LinkedDictionary parent)
	{
		this.parent = parent;
	}

	public virtual bool MoveNext()
	{
		if (pos >= parent.keys.Count)
		{
			return false;
		}
		return ++pos < parent.keys.Count;
	}

	public virtual void Reset()
	{
		pos = -1;
	}
}
