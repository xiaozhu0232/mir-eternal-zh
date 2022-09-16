using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

public static class Extensions
{
	public static IJEnumerable<JToken> Ancestors<T>(this IEnumerable<T> source) where T : JToken
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		return source.SelectMany((T j) => j.Ancestors()).AsJEnumerable();
	}

	public static IJEnumerable<JToken> AncestorsAndSelf<T>(this IEnumerable<T> source) where T : JToken
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		return source.SelectMany((T j) => j.AncestorsAndSelf()).AsJEnumerable();
	}

	public static IJEnumerable<JToken> Descendants<T>(this IEnumerable<T> source) where T : JContainer
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		return source.SelectMany((T j) => j.Descendants()).AsJEnumerable();
	}

	public static IJEnumerable<JToken> DescendantsAndSelf<T>(this IEnumerable<T> source) where T : JContainer
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		return source.SelectMany((T j) => j.DescendantsAndSelf()).AsJEnumerable();
	}

	public static IJEnumerable<JProperty> Properties(this IEnumerable<JObject> source)
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		return source.SelectMany((JObject d) => d.Properties()).AsJEnumerable();
	}

	public static IJEnumerable<JToken> Values(this IEnumerable<JToken> source, object? key)
	{
		return source.Values<JToken, JToken>(key).AsJEnumerable();
	}

	public static IJEnumerable<JToken> Values(this IEnumerable<JToken> source)
	{
		return source.Values(null);
	}

	public static IEnumerable<U?> Values<U>(this IEnumerable<JToken> source, object key)
	{
		return source.Values<JToken, U>(key);
	}

	public static IEnumerable<U?> Values<U>(this IEnumerable<JToken> source)
	{
		return source.Values<JToken, U>(null);
	}

	public static U? Value<U>(this IEnumerable<JToken> value)
	{
		return value.Value<JToken, U>();
	}

	public static U? Value<T, U>(this IEnumerable<T> value) where T : JToken
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		return ((value as JToken) ?? throw new ArgumentException("Source value must be a JToken.")).Convert<JToken, U>();
	}

	internal static IEnumerable<U?> Values<T, U>(this IEnumerable<T> source, object? key) where T : JToken
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		if (key == null)
		{
			foreach (T item in source)
			{
				if (item is JValue token)
				{
					yield return token.Convert<JValue, U>();
					continue;
				}
				foreach (JToken item2 in item.Children())
				{
					yield return item2.Convert<JToken, U>();
				}
			}
			yield break;
		}
		foreach (T item3 in source)
		{
			JToken jToken = item3[key];
			if (jToken != null)
			{
				yield return jToken.Convert<JToken, U>();
			}
		}
	}

	public static IJEnumerable<JToken> Children<T>(this IEnumerable<T> source) where T : JToken
	{
		return source.Children<T, JToken>().AsJEnumerable();
	}

	public static IEnumerable<U?> Children<T, U>(this IEnumerable<T> source) where T : JToken
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		return source.SelectMany((T c) => c.Children()).Convert<JToken, U>();
	}

	internal static IEnumerable<U?> Convert<T, U>(this IEnumerable<T> source) where T : JToken
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		foreach (T item in source)
		{
			yield return item.Convert<JToken, U>();
		}
	}

	internal static U? Convert<T, U>(this T token) where T : JToken?
	{
		if (token == null)
		{
			return default(U);
		}
		if (token is U)
		{
			U result = (U)(object)((token is U) ? token : null);
			if (typeof(U) != typeof(IComparable) && typeof(U) != typeof(IFormattable))
			{
				return result;
			}
		}
		if (!(token is JValue jValue))
		{
			throw new InvalidCastException("Cannot cast {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, token.GetType(), typeof(T)));
		}
		object value = jValue.Value;
		if (value is U)
		{
			return (U)value;
		}
		Type type = typeof(U);
		if (ReflectionUtils.IsNullableType(type))
		{
			if (jValue.Value == null)
			{
				return default(U);
			}
			type = Nullable.GetUnderlyingType(type);
		}
		return (U)System.Convert.ChangeType(jValue.Value, type, CultureInfo.InvariantCulture);
	}

	public static IJEnumerable<JToken> AsJEnumerable(this IEnumerable<JToken> source)
	{
		return source.AsJEnumerable<JToken>();
	}

	public static IJEnumerable<T> AsJEnumerable<T>(this IEnumerable<T> source) where T : JToken
	{
		if (source == null)
		{
			return null;
		}
		if (source is IJEnumerable<T> result)
		{
			return result;
		}
		return new JEnumerable<T>(source);
	}
}
