using System;
using System.Collections;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public class HeaderFieldParameterCollection : IEnumerable
{
	private ParametizedHeaderField m_pHeaderField;

	public string this[string parameterName]
	{
		get
		{
			parameterName = parameterName.ToLower();
			Hashtable hashtable = m_pHeaderField.ParseParameters();
			if (!hashtable.ContainsKey(parameterName))
			{
				throw new Exception("Specified parameter '" + parameterName + "' doesn't exist !");
			}
			return (string)hashtable[parameterName];
		}
		set
		{
			parameterName = parameterName.ToLower();
			Hashtable hashtable = m_pHeaderField.ParseParameters();
			if (hashtable.ContainsKey(parameterName))
			{
				hashtable[parameterName] = value;
				m_pHeaderField.StoreParameters(m_pHeaderField.Value, hashtable);
			}
		}
	}

	public int Count => m_pHeaderField.ParseParameters().Count;

	internal HeaderFieldParameterCollection(ParametizedHeaderField headerField)
	{
		m_pHeaderField = headerField;
	}

	public void Add(string parameterName, string parameterValue)
	{
		parameterName = parameterName.ToLower();
		Hashtable hashtable = m_pHeaderField.ParseParameters();
		if (!hashtable.ContainsKey(parameterName))
		{
			hashtable.Add(parameterName, parameterValue);
			m_pHeaderField.StoreParameters(m_pHeaderField.Value, hashtable);
			return;
		}
		throw new Exception("Header field '" + m_pHeaderField.Name + "' parameter '" + parameterName + "' already exists !");
	}

	public void Remove(string parameterName)
	{
		parameterName = parameterName.ToLower();
		Hashtable hashtable = m_pHeaderField.ParseParameters();
		if (!hashtable.ContainsKey(parameterName))
		{
			hashtable.Remove(parameterName);
			m_pHeaderField.StoreParameters(m_pHeaderField.Value, hashtable);
		}
	}

	public void Clear()
	{
		Hashtable hashtable = m_pHeaderField.ParseParameters();
		hashtable.Clear();
		m_pHeaderField.StoreParameters(m_pHeaderField.Value, hashtable);
	}

	public bool Contains(string parameterName)
	{
		parameterName = parameterName.ToLower();
		return m_pHeaderField.ParseParameters().ContainsKey(parameterName);
	}

	public IEnumerator GetEnumerator()
	{
		Hashtable hashtable = m_pHeaderField.ParseParameters();
		HeaderFieldParameter[] array = new HeaderFieldParameter[hashtable.Count];
		int num = 0;
		foreach (DictionaryEntry item in hashtable)
		{
			array[num] = new HeaderFieldParameter(item.Key.ToString(), item.Value.ToString());
			num++;
		}
		return array.GetEnumerator();
	}
}
