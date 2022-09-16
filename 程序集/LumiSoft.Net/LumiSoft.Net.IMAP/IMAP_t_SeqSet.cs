using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_SeqSet
{
	private List<Range_long> m_pSequenceParts;

	private string m_SequenceString = "";

	public Range_long[] Items => m_pSequenceParts.ToArray();

	private IMAP_t_SeqSet()
	{
		m_pSequenceParts = new List<Range_long>();
	}

	public static IMAP_t_SeqSet Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		long seqMaxValue = long.MaxValue;
		IMAP_t_SeqSet iMAP_t_SeqSet = new IMAP_t_SeqSet();
		string[] array = value.Trim().Split(',');
		foreach (string text in array)
		{
			if (text.IndexOf(":") > -1)
			{
				string[] array2 = text.Split(':');
				if (array2.Length != 2)
				{
					throw new Exception("Invalid <seq-range> '" + text + "' value !");
				}
				long num = iMAP_t_SeqSet.Parse_Seq_Number(array2[0], seqMaxValue);
				long num2 = iMAP_t_SeqSet.Parse_Seq_Number(array2[1], seqMaxValue);
				if (num <= num2)
				{
					iMAP_t_SeqSet.m_pSequenceParts.Add(new Range_long(num, num2));
				}
				else
				{
					iMAP_t_SeqSet.m_pSequenceParts.Add(new Range_long(num2, num));
				}
			}
			else
			{
				iMAP_t_SeqSet.m_pSequenceParts.Add(new Range_long(iMAP_t_SeqSet.Parse_Seq_Number(text, seqMaxValue)));
			}
		}
		iMAP_t_SeqSet.m_SequenceString = value;
		return iMAP_t_SeqSet;
	}

	public bool Contains(long seqNumber)
	{
		foreach (Range_long pSequencePart in m_pSequenceParts)
		{
			if (pSequencePart.Contains(seqNumber))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return m_SequenceString;
	}

	private long Parse_Seq_Number(string seqNumberValue, long seqMaxValue)
	{
		seqNumberValue = seqNumberValue.Trim();
		if (seqNumberValue == "*")
		{
			return seqMaxValue;
		}
		try
		{
			return Convert.ToInt64(seqNumberValue);
		}
		catch
		{
			throw new Exception("Invalid <seq-number> '" + seqNumberValue + "' value !");
		}
	}
}
