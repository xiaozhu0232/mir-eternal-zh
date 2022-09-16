using System;
using System.Text;

namespace Org.BouncyCastle.Math.EC.Abc;

internal class SimpleBigDecimal
{
	private readonly BigInteger bigInt;

	private readonly int scale;

	public int IntValue => Floor().IntValue;

	public long LongValue => Floor().LongValue;

	public int Scale => scale;

	public static SimpleBigDecimal GetInstance(BigInteger val, int scale)
	{
		return new SimpleBigDecimal(val.ShiftLeft(scale), scale);
	}

	public SimpleBigDecimal(BigInteger bigInt, int scale)
	{
		if (scale < 0)
		{
			throw new ArgumentException("scale may not be negative");
		}
		this.bigInt = bigInt;
		this.scale = scale;
	}

	private SimpleBigDecimal(SimpleBigDecimal limBigDec)
	{
		bigInt = limBigDec.bigInt;
		scale = limBigDec.scale;
	}

	private void CheckScale(SimpleBigDecimal b)
	{
		if (scale != b.scale)
		{
			throw new ArgumentException("Only SimpleBigDecimal of same scale allowed in arithmetic operations");
		}
	}

	public SimpleBigDecimal AdjustScale(int newScale)
	{
		if (newScale < 0)
		{
			throw new ArgumentException("scale may not be negative");
		}
		if (newScale == scale)
		{
			return this;
		}
		return new SimpleBigDecimal(bigInt.ShiftLeft(newScale - scale), newScale);
	}

	public SimpleBigDecimal Add(SimpleBigDecimal b)
	{
		CheckScale(b);
		return new SimpleBigDecimal(bigInt.Add(b.bigInt), scale);
	}

	public SimpleBigDecimal Add(BigInteger b)
	{
		return new SimpleBigDecimal(bigInt.Add(b.ShiftLeft(scale)), scale);
	}

	public SimpleBigDecimal Negate()
	{
		return new SimpleBigDecimal(bigInt.Negate(), scale);
	}

	public SimpleBigDecimal Subtract(SimpleBigDecimal b)
	{
		return Add(b.Negate());
	}

	public SimpleBigDecimal Subtract(BigInteger b)
	{
		return new SimpleBigDecimal(bigInt.Subtract(b.ShiftLeft(scale)), scale);
	}

	public SimpleBigDecimal Multiply(SimpleBigDecimal b)
	{
		CheckScale(b);
		return new SimpleBigDecimal(bigInt.Multiply(b.bigInt), scale + scale);
	}

	public SimpleBigDecimal Multiply(BigInteger b)
	{
		return new SimpleBigDecimal(bigInt.Multiply(b), scale);
	}

	public SimpleBigDecimal Divide(SimpleBigDecimal b)
	{
		CheckScale(b);
		BigInteger bigInteger = bigInt.ShiftLeft(scale);
		return new SimpleBigDecimal(bigInteger.Divide(b.bigInt), scale);
	}

	public SimpleBigDecimal Divide(BigInteger b)
	{
		return new SimpleBigDecimal(bigInt.Divide(b), scale);
	}

	public SimpleBigDecimal ShiftLeft(int n)
	{
		return new SimpleBigDecimal(bigInt.ShiftLeft(n), scale);
	}

	public int CompareTo(SimpleBigDecimal val)
	{
		CheckScale(val);
		return bigInt.CompareTo(val.bigInt);
	}

	public int CompareTo(BigInteger val)
	{
		return bigInt.CompareTo(val.ShiftLeft(scale));
	}

	public BigInteger Floor()
	{
		return bigInt.ShiftRight(scale);
	}

	public BigInteger Round()
	{
		SimpleBigDecimal simpleBigDecimal = new SimpleBigDecimal(BigInteger.One, 1);
		return Add(simpleBigDecimal.AdjustScale(scale)).Floor();
	}

	public override string ToString()
	{
		if (scale == 0)
		{
			return bigInt.ToString();
		}
		BigInteger bigInteger = Floor();
		BigInteger bigInteger2 = bigInt.Subtract(bigInteger.ShiftLeft(scale));
		if (bigInt.SignValue < 0)
		{
			bigInteger2 = BigInteger.One.ShiftLeft(scale).Subtract(bigInteger2);
		}
		if (bigInteger.SignValue == -1 && !bigInteger2.Equals(BigInteger.Zero))
		{
			bigInteger = bigInteger.Add(BigInteger.One);
		}
		string value = bigInteger.ToString();
		char[] array = new char[scale];
		string text = bigInteger2.ToString(2);
		int length = text.Length;
		int num = scale - length;
		for (int i = 0; i < num; i++)
		{
			array[i] = '0';
		}
		for (int j = 0; j < length; j++)
		{
			array[num + j] = text[j];
		}
		string value2 = new string(array);
		StringBuilder stringBuilder = new StringBuilder(value);
		stringBuilder.Append(".");
		stringBuilder.Append(value2);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is SimpleBigDecimal simpleBigDecimal))
		{
			return false;
		}
		if (bigInt.Equals(simpleBigDecimal.bigInt))
		{
			return scale == simpleBigDecimal.scale;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return bigInt.GetHashCode() ^ scale;
	}
}
