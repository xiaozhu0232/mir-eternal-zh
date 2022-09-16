using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

public readonly struct JEnumerable<T> : IJEnumerable<T>, IEnumerable<T>, IEnumerable, IEquatable<JEnumerable<T>> where T : JToken
{
	public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

	private readonly IEnumerable<T> _enumerable;

	public IJEnumerable<JToken> this[object key]
	{
		get
		{
			if (_enumerable == null)
			{
				return JEnumerable<JToken>.Empty;
			}
			return new JEnumerable<JToken>(_enumerable.Values<T, JToken>(key));
		}
	}

	public JEnumerable(IEnumerable<T> enumerable)
	{
		ValidationUtils.ArgumentNotNull(enumerable, "enumerable");
		_enumerable = enumerable;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)(_enumerable ?? ((object)Empty))).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Equals(JEnumerable<T> other)
	{
		return object.Equals(_enumerable, other._enumerable);
	}

	public override bool Equals(object obj)
	{
		if (obj is JEnumerable<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_enumerable == null)
		{
			return 0;
		}
		return _enumerable.GetHashCode();
	}
}
