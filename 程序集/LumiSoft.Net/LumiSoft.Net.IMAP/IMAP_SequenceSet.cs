using System;
using System.Collections.Generic;

namespace LumiSoft.Net.IMAP;

[Obsolete("Use class 'IMAP_t_SeqSet' instead.")]
public class IMAP_SequenceSet
{
	private List<Range_long> m_pSequenceParts;

	private string m_SequenceString = "";

	public Range_long[] Items => m_pSequenceParts.ToArray();

	public IMAP_SequenceSet()
	{
		m_pSequenceParts = new List<Range_long>();
	}

	public void Parse(string sequenceSetString)
	{
		Parse(sequenceSetString, long.MaxValue);
	}

	public void Parse(string sequenceSetString, long seqMaxValue)
	{
		string[] array = sequenceSetString.Trim().Split(',');
		foreach (string text in array)
		{
			if (text.IndexOf(":") > -1)
			{
				string[] array2 = text.Split(':');
				if (array2.Length != 2)
				{
					throw new Exception("Invalid <seq-range> '" + text + "' value !");
				}
				long num = Parse_Seq_Number(array2[0], seqMaxValue);
				long num2 = Parse_Seq_Number(array2[1], seqMaxValue);
				if (num <= num2)
				{
					m_pSequenceParts.Add(new Range_long(num, num2));
				}
				else
				{
					m_pSequenceParts.Add(new Range_long(num2, num));
				}
			}
			else
			{
				m_pSequenceParts.Add(new Range_long(Parse_Seq_Number(text, seqMaxValue)));
			}
		}
		m_SequenceString = sequenceSetString;
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

	public string ToSequenceSetString()
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
