using System;

namespace Org.BouncyCastle.Utilities.Date;

public sealed class DateTimeObject
{
	private readonly DateTime dt;

	public DateTime Value => dt;

	public DateTimeObject(DateTime dt)
	{
		this.dt = dt;
	}

	public override string ToString()
	{
		return dt.ToString();
	}
}
