using System.Collections.Generic;

namespace LumiSoft.Net.SIP.Message;

public class SIP_SVGroupHFCollection<T> where T : SIP_t_Value
{
	private SIP_Message m_pMessage;

	private string m_FieldName = "";

	private List<SIP_SingleValueHF<T>> m_pFields;

	public string FieldName => m_FieldName;

	public int Count => m_pFields.Count;

	public SIP_SingleValueHF<T>[] HeaderFields => m_pFields.ToArray();

	public SIP_SVGroupHFCollection(SIP_Message owner, string fieldName)
	{
		m_pMessage = owner;
		m_FieldName = fieldName;
		m_pFields = new List<SIP_SingleValueHF<T>>();
		Refresh();
	}

	private void Refresh()
	{
		m_pFields.Clear();
		foreach (SIP_HeaderField item in m_pMessage.Header)
		{
			if (item.Name.ToLower() == m_FieldName.ToLower())
			{
				m_pFields.Add((SIP_SingleValueHF<T>)item);
			}
		}
	}

	public void Add(string value)
	{
		m_pMessage.Header.Add(m_FieldName, value);
		Refresh();
	}

	public void Remove(int index)
	{
		m_pMessage.Header.Remove(m_pFields[index]);
		m_pFields.RemoveAt(index);
	}

	public void Remove(SIP_SingleValueHF<T> field)
	{
		m_pMessage.Header.Remove(field);
		m_pFields.Remove(field);
	}

	public void RemoveAll()
	{
		foreach (SIP_SingleValueHF<T> pField in m_pFields)
		{
			m_pMessage.Header.Remove(pField);
		}
		m_pFields.Clear();
	}

	public SIP_SingleValueHF<T> GetFirst()
	{
		if (m_pFields.Count > 0)
		{
			return m_pFields[0];
		}
		return null;
	}

	public T[] GetAllValues()
	{
		List<T> list = new List<T>();
		foreach (SIP_SingleValueHF<T> pField in m_pFields)
		{
			list.Add(pField.ValueX);
		}
		return list.ToArray();
	}
}
