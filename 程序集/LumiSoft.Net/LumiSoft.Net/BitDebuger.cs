using System;
using System.Text;

namespace LumiSoft.Net;

internal class BitDebuger
{
	public static string ToBit(byte[] buffer, int count, int bytesPerLine)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		int num = 1;
		for (; i < count; i++)
		{
			byte b = buffer[i];
			char[] array = new char[8];
			for (int num2 = 7; num2 >= 0; num2--)
			{
				array[num2] = ((b >> 7 - num2) & 1).ToString()[0];
			}
			stringBuilder.Append(array);
			if (num == bytesPerLine)
			{
				stringBuilder.AppendLine();
				num = 0;
			}
			else
			{
				stringBuilder.Append(" ");
			}
			num++;
		}
		return stringBuilder.ToString();
	}
}
