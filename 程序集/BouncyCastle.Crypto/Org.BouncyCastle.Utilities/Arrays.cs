using System;
using System.Text;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Utilities;

public abstract class Arrays
{
	public static readonly byte[] EmptyBytes = new byte[0];

	public static readonly int[] EmptyInts = new int[0];

	public static bool AreAllZeroes(byte[] buf, int off, int len)
	{
		uint num = 0u;
		for (int i = 0; i < len; i++)
		{
			num |= buf[off + i];
		}
		return num == 0;
	}

	public static bool AreEqual(bool[] a, bool[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	public static bool AreEqual(char[] a, char[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	public static bool AreEqual(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	public static bool AreEqual(byte[] a, int aFromIndex, int aToIndex, byte[] b, int bFromIndex, int bToIndex)
	{
		int num = aToIndex - aFromIndex;
		int num2 = bToIndex - bFromIndex;
		if (num != num2)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (a[aFromIndex + i] != b[bFromIndex + i])
			{
				return false;
			}
		}
		return true;
	}

	[Obsolete("Use 'AreEqual' method instead")]
	public static bool AreSame(byte[] a, byte[] b)
	{
		return AreEqual(a, b);
	}

	public static bool ConstantTimeAreEqual(byte[] a, byte[] b)
	{
		if (a == null || b == null)
		{
			return false;
		}
		if (a == b)
		{
			return true;
		}
		int num = System.Math.Min(a.Length, b.Length);
		int num2 = a.Length ^ b.Length;
		for (int i = 0; i < num; i++)
		{
			num2 |= a[i] ^ b[i];
		}
		for (int j = num; j < b.Length; j++)
		{
			num2 |= b[j] ^ ~b[j];
		}
		return 0 == num2;
	}

	public static bool ConstantTimeAreEqual(int len, byte[] a, int aOff, byte[] b, int bOff)
	{
		if (a == null)
		{
			throw new ArgumentNullException("a");
		}
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		if (len < 0)
		{
			throw new ArgumentException("cannot be negative", "len");
		}
		if (aOff > a.Length - len)
		{
			throw new IndexOutOfRangeException("'aOff' value invalid for specified length");
		}
		if (bOff > b.Length - len)
		{
			throw new IndexOutOfRangeException("'bOff' value invalid for specified length");
		}
		int num = 0;
		for (int i = 0; i < len; i++)
		{
			num |= a[aOff + i] ^ b[bOff + i];
		}
		return 0 == num;
	}

	public static bool AreEqual(int[] a, int[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	[CLSCompliant(false)]
	public static bool AreEqual(uint[] a, uint[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	public static bool AreEqual(long[] a, long[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	[CLSCompliant(false)]
	public static bool AreEqual(ulong[] a, ulong[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	private static bool HaveSameContents(bool[] a, bool[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameContents(char[] a, char[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameContents(byte[] a, byte[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameContents(int[] a, int[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameContents(uint[] a, uint[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameContents(long[] a, long[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	private static bool HaveSameContents(ulong[] a, ulong[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	public static string ToString(object[] a)
	{
		StringBuilder stringBuilder = new StringBuilder("[");
		if (a.Length > 0)
		{
			stringBuilder.Append(a[0]);
			for (int i = 1; i < a.Length; i++)
			{
				stringBuilder.Append(", ").Append(a[i]);
			}
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	public static int GetHashCode(byte[] data)
	{
		if (data == null)
		{
			return 0;
		}
		int num = data.Length;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= data[num];
		}
		return num2;
	}

	public static int GetHashCode(byte[] data, int off, int len)
	{
		if (data == null)
		{
			return 0;
		}
		int num = len;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= data[off + num];
		}
		return num2;
	}

	public static int GetHashCode(int[] data)
	{
		if (data == null)
		{
			return 0;
		}
		int num = data.Length;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= data[num];
		}
		return num2;
	}

	public static int GetHashCode(int[] data, int off, int len)
	{
		if (data == null)
		{
			return 0;
		}
		int num = len;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= data[off + num];
		}
		return num2;
	}

	[CLSCompliant(false)]
	public static int GetHashCode(uint[] data)
	{
		if (data == null)
		{
			return 0;
		}
		int num = data.Length;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= (int)data[num];
		}
		return num2;
	}

	[CLSCompliant(false)]
	public static int GetHashCode(uint[] data, int off, int len)
	{
		if (data == null)
		{
			return 0;
		}
		int num = len;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= (int)data[off + num];
		}
		return num2;
	}

	[CLSCompliant(false)]
	public static int GetHashCode(ulong[] data)
	{
		if (data == null)
		{
			return 0;
		}
		int num = data.Length;
		int num2 = num + 1;
		while (--num >= 0)
		{
			ulong num3 = data[num];
			num2 *= 257;
			num2 ^= (int)num3;
			num2 *= 257;
			num2 ^= (int)(num3 >> 32);
		}
		return num2;
	}

	[CLSCompliant(false)]
	public static int GetHashCode(ulong[] data, int off, int len)
	{
		if (data == null)
		{
			return 0;
		}
		int num = len;
		int num2 = num + 1;
		while (--num >= 0)
		{
			ulong num3 = data[off + num];
			num2 *= 257;
			num2 ^= (int)num3;
			num2 *= 257;
			num2 ^= (int)(num3 >> 32);
		}
		return num2;
	}

	public static bool[] Clone(bool[] data)
	{
		if (data != null)
		{
			return (bool[])data.Clone();
		}
		return null;
	}

	public static byte[] Clone(byte[] data)
	{
		if (data != null)
		{
			return (byte[])data.Clone();
		}
		return null;
	}

	public static int[] Clone(int[] data)
	{
		if (data != null)
		{
			return (int[])data.Clone();
		}
		return null;
	}

	[CLSCompliant(false)]
	public static uint[] Clone(uint[] data)
	{
		if (data != null)
		{
			return (uint[])data.Clone();
		}
		return null;
	}

	public static long[] Clone(long[] data)
	{
		if (data != null)
		{
			return (long[])data.Clone();
		}
		return null;
	}

	[CLSCompliant(false)]
	public static ulong[] Clone(ulong[] data)
	{
		if (data != null)
		{
			return (ulong[])data.Clone();
		}
		return null;
	}

	public static byte[] Clone(byte[] data, byte[] existing)
	{
		if (data == null)
		{
			return null;
		}
		if (existing == null || existing.Length != data.Length)
		{
			return Clone(data);
		}
		Array.Copy(data, 0, existing, 0, existing.Length);
		return existing;
	}

	[CLSCompliant(false)]
	public static ulong[] Clone(ulong[] data, ulong[] existing)
	{
		if (data == null)
		{
			return null;
		}
		if (existing == null || existing.Length != data.Length)
		{
			return Clone(data);
		}
		Array.Copy(data, 0, existing, 0, existing.Length);
		return existing;
	}

	public static bool Contains(byte[] a, byte n)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] == n)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(short[] a, short n)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] == n)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains(int[] a, int n)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] == n)
			{
				return true;
			}
		}
		return false;
	}

	public static void Fill(byte[] buf, byte b)
	{
		int num = buf.Length;
		while (num > 0)
		{
			buf[--num] = b;
		}
	}

	public static void Fill(byte[] buf, int from, int to, byte b)
	{
		for (int i = from; i < to; i++)
		{
			buf[i] = b;
		}
	}

	public static byte[] CopyOf(byte[] data, int newLength)
	{
		byte[] array = new byte[newLength];
		Array.Copy(data, 0, array, 0, System.Math.Min(newLength, data.Length));
		return array;
	}

	public static char[] CopyOf(char[] data, int newLength)
	{
		char[] array = new char[newLength];
		Array.Copy(data, 0, array, 0, System.Math.Min(newLength, data.Length));
		return array;
	}

	public static int[] CopyOf(int[] data, int newLength)
	{
		int[] array = new int[newLength];
		Array.Copy(data, 0, array, 0, System.Math.Min(newLength, data.Length));
		return array;
	}

	public static long[] CopyOf(long[] data, int newLength)
	{
		long[] array = new long[newLength];
		Array.Copy(data, 0, array, 0, System.Math.Min(newLength, data.Length));
		return array;
	}

	public static BigInteger[] CopyOf(BigInteger[] data, int newLength)
	{
		BigInteger[] array = new BigInteger[newLength];
		Array.Copy(data, 0, array, 0, System.Math.Min(newLength, data.Length));
		return array;
	}

	public static byte[] CopyOfRange(byte[] data, int from, int to)
	{
		int length = GetLength(from, to);
		byte[] array = new byte[length];
		Array.Copy(data, from, array, 0, System.Math.Min(length, data.Length - from));
		return array;
	}

	public static int[] CopyOfRange(int[] data, int from, int to)
	{
		int length = GetLength(from, to);
		int[] array = new int[length];
		Array.Copy(data, from, array, 0, System.Math.Min(length, data.Length - from));
		return array;
	}

	public static long[] CopyOfRange(long[] data, int from, int to)
	{
		int length = GetLength(from, to);
		long[] array = new long[length];
		Array.Copy(data, from, array, 0, System.Math.Min(length, data.Length - from));
		return array;
	}

	public static BigInteger[] CopyOfRange(BigInteger[] data, int from, int to)
	{
		int length = GetLength(from, to);
		BigInteger[] array = new BigInteger[length];
		Array.Copy(data, from, array, 0, System.Math.Min(length, data.Length - from));
		return array;
	}

	private static int GetLength(int from, int to)
	{
		int num = to - from;
		if (num < 0)
		{
			throw new ArgumentException(from + " > " + to);
		}
		return num;
	}

	public static byte[] Append(byte[] a, byte b)
	{
		if (a == null)
		{
			return new byte[1] { b };
		}
		int num = a.Length;
		byte[] array = new byte[num + 1];
		Array.Copy(a, 0, array, 0, num);
		array[num] = b;
		return array;
	}

	public static short[] Append(short[] a, short b)
	{
		if (a == null)
		{
			return new short[1] { b };
		}
		int num = a.Length;
		short[] array = new short[num + 1];
		Array.Copy(a, 0, array, 0, num);
		array[num] = b;
		return array;
	}

	public static int[] Append(int[] a, int b)
	{
		if (a == null)
		{
			return new int[1] { b };
		}
		int num = a.Length;
		int[] array = new int[num + 1];
		Array.Copy(a, 0, array, 0, num);
		array[num] = b;
		return array;
	}

	public static byte[] Concatenate(byte[] a, byte[] b)
	{
		if (a == null)
		{
			return Clone(b);
		}
		if (b == null)
		{
			return Clone(a);
		}
		byte[] array = new byte[a.Length + b.Length];
		Array.Copy(a, 0, array, 0, a.Length);
		Array.Copy(b, 0, array, a.Length, b.Length);
		return array;
	}

	public static byte[] ConcatenateAll(params byte[][] vs)
	{
		byte[][] array = new byte[vs.Length][];
		int num = 0;
		int num2 = 0;
		foreach (byte[] array2 in vs)
		{
			if (array2 != null)
			{
				array[num++] = array2;
				num2 += array2.Length;
			}
		}
		byte[] array3 = new byte[num2];
		int num3 = 0;
		for (int j = 0; j < num; j++)
		{
			byte[] array4 = array[j];
			Array.Copy(array4, 0, array3, num3, array4.Length);
			num3 += array4.Length;
		}
		return array3;
	}

	public static int[] Concatenate(int[] a, int[] b)
	{
		if (a == null)
		{
			return Clone(b);
		}
		if (b == null)
		{
			return Clone(a);
		}
		int[] array = new int[a.Length + b.Length];
		Array.Copy(a, 0, array, 0, a.Length);
		Array.Copy(b, 0, array, a.Length, b.Length);
		return array;
	}

	public static byte[] Prepend(byte[] a, byte b)
	{
		if (a == null)
		{
			return new byte[1] { b };
		}
		int num = a.Length;
		byte[] array = new byte[num + 1];
		Array.Copy(a, 0, array, 1, num);
		array[0] = b;
		return array;
	}

	public static short[] Prepend(short[] a, short b)
	{
		if (a == null)
		{
			return new short[1] { b };
		}
		int num = a.Length;
		short[] array = new short[num + 1];
		Array.Copy(a, 0, array, 1, num);
		array[0] = b;
		return array;
	}

	public static int[] Prepend(int[] a, int b)
	{
		if (a == null)
		{
			return new int[1] { b };
		}
		int num = a.Length;
		int[] array = new int[num + 1];
		Array.Copy(a, 0, array, 1, num);
		array[0] = b;
		return array;
	}

	public static byte[] Reverse(byte[] a)
	{
		if (a == null)
		{
			return null;
		}
		int num = 0;
		int num2 = a.Length;
		byte[] array = new byte[num2];
		while (--num2 >= 0)
		{
			array[num2] = a[num++];
		}
		return array;
	}

	public static int[] Reverse(int[] a)
	{
		if (a == null)
		{
			return null;
		}
		int num = 0;
		int num2 = a.Length;
		int[] array = new int[num2];
		while (--num2 >= 0)
		{
			array[num2] = a[num++];
		}
		return array;
	}

	public static void Clear(byte[] data)
	{
		if (data != null)
		{
			Array.Clear(data, 0, data.Length);
		}
	}

	public static void Clear(int[] data)
	{
		if (data != null)
		{
			Array.Clear(data, 0, data.Length);
		}
	}

	public static bool IsNullOrContainsNull(object[] array)
	{
		if (array == null)
		{
			return true;
		}
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			if (array[i] == null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNullOrEmpty(byte[] array)
	{
		if (array != null)
		{
			return array.Length < 1;
		}
		return true;
	}

	public static bool IsNullOrEmpty(object[] array)
	{
		if (array != null)
		{
			return array.Length < 1;
		}
		return true;
	}
}
