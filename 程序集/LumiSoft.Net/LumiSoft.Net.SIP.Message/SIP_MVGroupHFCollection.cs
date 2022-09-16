using System.Collections.Generic;

namespace LumiSoft.Net.SIP.Message;

public class SIP_MVGroupHFCollection<T> where T : SIP_t_Value, new()
{
	private SIP_Message m_pMessage;

	private string m_FieldName = "";

	private List<SIP_MultiValueHF<T>> m_pFields;

	public string FieldName => m_FieldName;

	public int Count => m_pFields.Count;

	public SIP_MultiValueHF<T>[] HeaderFields => m_pFields.ToArray();

	public SIP_MVGroupHFCollection(SIP_Message owner, string fieldName)
	{
		m_pMessage = owner;
		m_FieldName = fieldName;
		m_pFields = new List<SIP_MultiValueHF<T>>();
		Refresh();
	}

	private void Refresh()
	{
		m_pFields.Clear();
		foreach (SIP_HeaderField item in m_pMessage.Header)
		{
			if (item.Name.ToLower() == m_FieldName.ToLower())
			{
				m_pFields.Add((SIP_MultiValueHF<T>)item);
			}
		}
	}

	public void AddToTop(string value)
	{
		m_pMessage.Header.Insert(0, m_FieldName, value);
		Refresh();
	}

	public void Add(string value)
	{
		m_pMessage.Header.Add(m_FieldName, value);
		Refresh();
	}

	public void RemoveAll()
	{
		m_pMessage.Header.RemoveAll(m_FieldName);
		m_pFields.Clear();
	}

	public T GetTopMostValue()
	{
		if (m_pFields.Count > 0)
		{
			return m_pFields[0].Values[0];
		}
		return null;
	}

	public void RemoveTopMostValue()
	{
		if (m_pFields.Count > 0)
		{
			SIP_MultiValueHF<T> sIP_MultiValueHF = m_pFields[0];
			if (sIP_MultiValueHF.Count > 1)
			{
				sIP_MultiValueHF.Remove(0);
				return;
			}
			m_pMessage.Header.Remove(m_pFields[0]);
			m_pFields.Remove(m_pFields[0]);
		}
	}

	public void RemoveLastValue()
	{
		SIP_MultiValueHF<T> sIP_MultiValueHF = m_pFields[m_pFields.Count - 1];
		if (sIP_MultiValueHF.Count > 1)
		{
			sIP_MultiValueHF.Remove(sIP_MultiValueHF.Count - 1);
			return;
		}
		m_pMessage.Header.Remove(m_pFields[0]);
		m_pFields.Remove(sIP_MultiValueHF);
	}

	public T[] GetAllValues()
	{
		List<T> list = new List<T>();
		foreach (SIP_MultiValueHF<T> pField in m_pFields)
		{
			foreach (T value in pField.Values)
			{
				list.Add((T)value);
			}
		}
		return list.ToArray();
	}
}
