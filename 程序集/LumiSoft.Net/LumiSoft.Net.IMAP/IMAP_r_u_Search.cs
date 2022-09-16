using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Search : IMAP_r_u
{
	private int[] m_pValues;

	public int[] Values => m_pValues;

	public IMAP_r_u_Search(int[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		m_pValues = values;
	}

	public static IMAP_r_u_Search Parse(string response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		List<int> list = new List<int>();
		if (response.Split(' ').Length > 2)
		{
			string[] array = response.Split(new char[1] { ' ' }, 3)[2].Trim().Split(' ');
			foreach (string value in array)
			{
				if (!string.IsNullOrEmpty(value))
				{
					list.Add(Convert.ToInt32(value));
				}
			}
		}
		return new IMAP_r_u_Search(list.ToArray());
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* SEARCH");
		int[] pValues = m_pValues;
		foreach (int num in pValues)
		{
			stringBuilder.Append(" " + num);
		}
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}
}
