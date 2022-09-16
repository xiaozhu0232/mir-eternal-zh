using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Message;

public class SIP_MultiValueHF<T> : SIP_HeaderField where T : SIP_t_Value, new()
{
	private List<T> m_pValues;

	public override string Value
	{
		get
		{
			return ToStringValue();
		}
		set
		{
			if (value != null)
			{
				throw new ArgumentNullException("Property Value value may not be null !");
			}
			Parse(value);
			base.Value = value;
		}
	}

	public List<T> Values => m_pValues;

	public int Count => m_pValues.Count;

	public SIP_MultiValueHF(string name, string value)
		: base(name, value)
	{
		m_pValues = new List<T>();
		SetMultiValue(value: true);
		Parse(value);
	}

	private void Parse(string value)
	{
		m_pValues.Clear();
		StringReader stringReader = new StringReader(value);
		while (stringReader.Available > 0)
		{
			stringReader.ReadToFirstChar();
			if (stringReader.StartsWith(","))
			{
				stringReader.ReadSpecifiedLength(1);
			}
			T val = new T();
			val.Parse(stringReader);
			m_pValues.Add(val);
		}
	}

	private string ToStringValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_pValues.Count; i++)
		{
			stringBuilder.Append(m_pValues[i].ToStringValue());
			if (i < m_pValues.Count - 1)
			{
				stringBuilder.Append(',');
			}
		}
		return stringBuilder.ToString();
	}

	public object[] GetValues()
	{
		return m_pValues.ToArray();
	}

	public void Remove(int index)
	{
		if (index > -1 && index < m_pValues.Count)
		{
			m_pValues.RemoveAt(index);
		}
	}
}
