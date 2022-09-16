using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC;

public abstract class AbstractF2mFieldElement : ECFieldElement
{
	public virtual bool HasFastTrace => false;

	public virtual ECFieldElement HalfTrace()
	{
		int fieldSize = FieldSize;
		if ((fieldSize & 1) == 0)
		{
			throw new InvalidOperationException("Half-trace only defined for odd m");
		}
		int num = fieldSize + 1 >> 1;
		int num2 = 31 - Integers.NumberOfLeadingZeros(num);
		int num3 = 1;
		ECFieldElement eCFieldElement = this;
		while (num2 > 0)
		{
			eCFieldElement = eCFieldElement.SquarePow(num3 << 1).Add(eCFieldElement);
			num3 = num >> --num2;
			if (((uint)num3 & (true ? 1u : 0u)) != 0)
			{
				eCFieldElement = eCFieldElement.SquarePow(2).Add(this);
			}
		}
		return eCFieldElement;
	}

	public virtual int Trace()
	{
		int fieldSize = FieldSize;
		int num = 31 - Integers.NumberOfLeadingZeros(fieldSize);
		int num2 = 1;
		ECFieldElement eCFieldElement = this;
		while (num > 0)
		{
			eCFieldElement = eCFieldElement.SquarePow(num2).Add(eCFieldElement);
			num2 = fieldSize >> --num;
			if (((uint)num2 & (true ? 1u : 0u)) != 0)
			{
				eCFieldElement = eCFieldElement.Square().Add(this);
			}
		}
		if (eCFieldElement.IsZero)
		{
			return 0;
		}
		if (eCFieldElement.IsOne)
		{
			return 1;
		}
		throw new InvalidOperationException("Internal error in trace calculation");
	}
}
