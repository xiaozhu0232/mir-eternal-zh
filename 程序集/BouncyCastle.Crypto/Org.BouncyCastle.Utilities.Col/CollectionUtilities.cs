using System;
using System.Collections;
using System.Text;

namespace Org.BouncyCastle.Utilities.Collections;

public abstract class CollectionUtilities
{
	public static void AddRange(IList to, IEnumerable range)
	{
		foreach (object item in range)
		{
			to.Add(item);
		}
	}

	public static bool CheckElementsAreOfType(IEnumerable e, Type t)
	{
		foreach (object item in e)
		{
			if (!t.IsInstanceOfType(item))
			{
				return false;
			}
		}
		return true;
	}

	public static IDictionary ReadOnly(IDictionary d)
	{
		return new UnmodifiableDictionaryProxy(d);
	}

	public static IList ReadOnly(IList l)
	{
		return new UnmodifiableListProxy(l);
	}

	public static ISet ReadOnly(ISet s)
	{
		return new UnmodifiableSetProxy(s);
	}

	public static object RequireNext(IEnumerator e)
	{
		if (!e.MoveNext())
		{
			throw new InvalidOperationException();
		}
		return e.Current;
	}

	public static string ToString(IEnumerable c)
	{
		IEnumerator enumerator = c.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder("[");
		stringBuilder.Append(enumerator.Current.ToString());
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(", ");
			stringBuilder.Append(enumerator.Current.ToString());
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}
}
