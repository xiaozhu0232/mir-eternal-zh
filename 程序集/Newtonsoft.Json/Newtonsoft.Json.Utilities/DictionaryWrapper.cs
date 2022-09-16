using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Newtonsoft.Json.Utilities;

internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IWrappedDictionary, IDictionary, ICollection
{
	private readonly struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IDictionaryEnumerator, IEnumerator
	{
		private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

		public DictionaryEntry Entry => (DictionaryEntry)Current;

		public object Key => Entry.Key;

		public object Value => Entry.Value;

		public object Current => new DictionaryEntry(_e.Current.Key, _e.Current.Value);

		public DictionaryEnumerator(IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
		{
			ValidationUtils.ArgumentNotNull(e, "e");
			_e = e;
		}

		public bool MoveNext()
		{
			return _e.MoveNext();
		}

		public void Reset()
		{
			_e.Reset();
		}
	}

	private readonly IDictionary? _dictionary;

	private readonly IDictionary<TKey, TValue>? _genericDictionary;

	private readonly IReadOnlyDictionary<TKey, TValue>? _readOnlyDictionary;

	private object? _syncRoot;

	internal IDictionary<TKey, TValue> GenericDictionary => _genericDictionary;

	public ICollection<TKey> Keys
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary!.Keys.Cast<TKey>().ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary!.Keys.ToList();
			}
			return GenericDictionary.Keys;
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary!.Values.Cast<TValue>().ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary!.Values.ToList();
			}
			return GenericDictionary.Values;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (_dictionary != null)
			{
				return (TValue)_dictionary![key];
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary![key];
			}
			return GenericDictionary[key];
		}
		set
		{
			if (_dictionary != null)
			{
				_dictionary![key] = value;
				return;
			}
			if (_readOnlyDictionary != null)
			{
				throw new NotSupportedException();
			}
			GenericDictionary[key] = value;
		}
	}

	public int Count
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary!.Count;
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary!.Count;
			}
			return GenericDictionary.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary!.IsReadOnly;
			}
			if (_readOnlyDictionary != null)
			{
				return true;
			}
			return GenericDictionary.IsReadOnly;
		}
	}

	object? IDictionary.this[object key]
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary![key];
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary![(TKey)key];
			}
			return GenericDictionary[(TKey)key];
		}
		set
		{
			if (_dictionary != null)
			{
				_dictionary![key] = value;
				return;
			}
			if (_readOnlyDictionary != null)
			{
				throw new NotSupportedException();
			}
			GenericDictionary[(TKey)key] = (TValue)value;
		}
	}

	bool IDictionary.IsFixedSize
	{
		get
		{
			if (_genericDictionary != null)
			{
				return false;
			}
			if (_readOnlyDictionary != null)
			{
				return true;
			}
			return _dictionary!.IsFixedSize;
		}
	}

	ICollection IDictionary.Keys
	{
		get
		{
			if (_genericDictionary != null)
			{
				return _genericDictionary!.Keys.ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary!.Keys.ToList();
			}
			return _dictionary!.Keys;
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			if (_genericDictionary != null)
			{
				return _genericDictionary!.Values.ToList();
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary!.Values.ToList();
			}
			return _dictionary!.Values;
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary!.IsSynchronized;
			}
			return false;
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			}
			return _syncRoot;
		}
	}

	public object UnderlyingDictionary
	{
		get
		{
			if (_dictionary != null)
			{
				return _dictionary;
			}
			if (_readOnlyDictionary != null)
			{
				return _readOnlyDictionary;
			}
			return GenericDictionary;
		}
	}

	public DictionaryWrapper(IDictionary dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		_dictionary = dictionary;
	}

	public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		_genericDictionary = dictionary;
	}

	public DictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		_readOnlyDictionary = dictionary;
	}

	public void Add(TKey key, TValue value)
	{
		if (_dictionary != null)
		{
			_dictionary!.Add(key, value);
			return;
		}
		if (_genericDictionary != null)
		{
			_genericDictionary!.Add(key, value);
			return;
		}
		throw new NotSupportedException();
	}

	public bool ContainsKey(TKey key)
	{
		if (_dictionary != null)
		{
			return _dictionary!.Contains(key);
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary!.ContainsKey(key);
		}
		return GenericDictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		if (_dictionary != null)
		{
			if (_dictionary!.Contains(key))
			{
				_dictionary!.Remove(key);
				return true;
			}
			return false;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return GenericDictionary.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue? value)
	{
		if (_dictionary != null)
		{
			if (!_dictionary!.Contains(key))
			{
				value = default(TValue);
				return false;
			}
			value = (TValue)_dictionary![key];
			return true;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return GenericDictionary.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary != null)
		{
			((IList)_dictionary).Add(item);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		_genericDictionary?.Add(item);
	}

	public void Clear()
	{
		if (_dictionary != null)
		{
			_dictionary!.Clear();
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary != null)
		{
			return ((IList)_dictionary).Contains(item);
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary.Contains(item);
		}
		return GenericDictionary.Contains(item);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (_dictionary != null)
		{
			foreach (DictionaryEntry item in _dictionary!)
			{
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)item.Key, (TValue)item.Value);
			}
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (_dictionary != null)
		{
			if (_dictionary!.Contains(item.Key))
			{
				if (object.Equals(_dictionary![item.Key], item.Value))
				{
					_dictionary!.Remove(item.Key);
					return true;
				}
				return false;
			}
			return true;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return GenericDictionary.Remove(item);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		if (_dictionary != null)
		{
			return (from DictionaryEntry de in _dictionary
				select new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary!.GetEnumerator();
		}
		return GenericDictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void IDictionary.Add(object key, object value)
	{
		if (_dictionary != null)
		{
			_dictionary!.Add(key, value);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.Add((TKey)key, (TValue)value);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		if (_dictionary != null)
		{
			return _dictionary!.GetEnumerator();
		}
		if (_readOnlyDictionary != null)
		{
			return new DictionaryEnumerator<TKey, TValue>(_readOnlyDictionary!.GetEnumerator());
		}
		return new DictionaryEnumerator<TKey, TValue>(GenericDictionary.GetEnumerator());
	}

	bool IDictionary.Contains(object key)
	{
		if (_genericDictionary != null)
		{
			return _genericDictionary!.ContainsKey((TKey)key);
		}
		if (_readOnlyDictionary != null)
		{
			return _readOnlyDictionary!.ContainsKey((TKey)key);
		}
		return _dictionary!.Contains(key);
	}

	public void Remove(object key)
	{
		if (_dictionary != null)
		{
			_dictionary!.Remove(key);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.Remove((TKey)key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (_dictionary != null)
		{
			_dictionary!.CopyTo(array, index);
			return;
		}
		if (_readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		GenericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
	}
}
