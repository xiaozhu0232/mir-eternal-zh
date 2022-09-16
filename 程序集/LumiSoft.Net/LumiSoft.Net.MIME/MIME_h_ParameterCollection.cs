using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LumiSoft.Net.MIME;

public class MIME_h_ParameterCollection : IEnumerable
{
	public class _ParameterBuilder
	{
		private string m_Name;

		private SortedList<int, string> m_pParts;

		private Encoding m_pEncoding;

		public string Name => m_Name;

		public _ParameterBuilder(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_Name = name;
			m_pParts = new SortedList<int, string>();
		}

		public void AddPart(int index, bool encoded, string value)
		{
			if (encoded && index == 0)
			{
				string[] array = value.Split('\'');
				m_pEncoding = Encoding.GetEncoding(string.IsNullOrEmpty(array[0]) ? "us-ascii" : array[0]);
				value = array[2];
			}
			m_pParts[index] = value;
		}

		public MIME_h_Parameter GetParamter()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<int, string> pPart in m_pParts)
			{
				stringBuilder.Append(pPart.Value);
			}
			if (m_pEncoding != null)
			{
				return new MIME_h_Parameter(m_Name, DecodeExtOctet(stringBuilder.ToString(), m_pEncoding));
			}
			return new MIME_h_Parameter(m_Name, stringBuilder.ToString());
		}
	}

	private bool m_IsModified;

	private MIME_h m_pOwner;

	private Dictionary<string, MIME_h_Parameter> m_pParameters;

	private bool m_EncodeRfc2047;

	public bool IsModified
	{
		get
		{
			if (m_IsModified)
			{
				return true;
			}
			MIME_h_Parameter[] array = ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsModified)
				{
					return true;
				}
			}
			return false;
		}
	}

	public MIME_h Owner => m_pOwner;

	public int Count => m_pParameters.Count;

	public string this[string name]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			MIME_h_Parameter value = null;
			if (m_pParameters.TryGetValue(name, out value))
			{
				return value.Value;
			}
			return null;
		}
		set
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			MIME_h_Parameter value2 = null;
			if (m_pParameters.TryGetValue(name, out value2))
			{
				value2.Value = value;
			}
			else
			{
				m_pParameters.Add(name, new MIME_h_Parameter(name, value));
			}
		}
	}

	public bool EncodeRfc2047
	{
		get
		{
			return m_EncodeRfc2047;
		}
		set
		{
			m_EncodeRfc2047 = value;
		}
	}

	public MIME_h_ParameterCollection(MIME_h owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		m_pOwner = owner;
		m_pParameters = new Dictionary<string, MIME_h_Parameter>(StringComparer.CurrentCultureIgnoreCase);
	}

	public void Remove(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (m_pParameters.Remove(name))
		{
			m_IsModified = true;
		}
	}

	public void Clear()
	{
		m_pParameters.Clear();
		m_IsModified = true;
	}

	public MIME_h_Parameter[] ToArray()
	{
		MIME_h_Parameter[] array = new MIME_h_Parameter[m_pParameters.Count];
		m_pParameters.Values.CopyTo(array, 0);
		return array;
	}

	public override string ToString()
	{
		return ToString(null);
	}

	public string ToString(Encoding charset)
	{
		if (charset == null)
		{
			charset = Encoding.Default;
		}
		StringBuilder stringBuilder = new StringBuilder();
		MIME_h_Parameter[] array = ToArray();
		foreach (MIME_h_Parameter mIME_h_Parameter in array)
		{
			if (string.IsNullOrEmpty(mIME_h_Parameter.Value))
			{
				stringBuilder.Append(";\r\n\t" + mIME_h_Parameter.Name);
				continue;
			}
			if ((charset == null || Net_Utils.IsAscii(mIME_h_Parameter.Value)) && mIME_h_Parameter.Value.Length < 76)
			{
				stringBuilder.Append(";\r\n\t" + mIME_h_Parameter.Name + "=" + TextUtils.QuoteString(mIME_h_Parameter.Value));
				continue;
			}
			if (m_EncodeRfc2047)
			{
				stringBuilder.Append(";\r\n\t" + mIME_h_Parameter.Name + "=" + TextUtils.QuoteString(MIME_Encoding_EncodedWord.EncodeS(MIME_EncodedWordEncoding.B, Encoding.UTF8, split: false, mIME_h_Parameter.Value)));
				continue;
			}
			byte[] bytes = charset.GetBytes(mIME_h_Parameter.Value);
			List<string> list = new List<string>();
			int num = 0;
			char[] array2 = new char[50];
			byte[] array3 = bytes;
			foreach (byte b in array3)
			{
				if (num >= 47)
				{
					list.Add(new string(array2, 0, num));
					num = 0;
				}
				if (MIME_Reader.IsAttributeChar((char)b))
				{
					array2[num++] = (char)b;
					continue;
				}
				array2[num++] = '%';
				array2[num++] = (b >> 4).ToString("X")[0];
				array2[num++] = (b & 0xF).ToString("X")[0];
			}
			if (num > 0)
			{
				list.Add(new string(array2, 0, num));
			}
			for (int k = 0; k < list.Count; k++)
			{
				if (charset != null && k == 0)
				{
					stringBuilder.Append(";\r\n\t" + mIME_h_Parameter.Name + "*" + k + "*=" + charset.WebName + "''" + list[k]);
				}
				else
				{
					stringBuilder.Append(";\r\n\t" + mIME_h_Parameter.Name + "*" + k + "*=" + list[k]);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Parse(new MIME_Reader(value));
	}

	public void Parse(MIME_Reader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		KeyValueCollection<string, _ParameterBuilder> keyValueCollection = new KeyValueCollection<string, _ParameterBuilder>();
		string[] array = TextUtils.SplitQuotedString(reader.ToEnd(), ';');
		foreach (string text in array)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array2 = text.Trim().Split(new char[1] { '=' }, 2);
			string text2 = array2[0].Trim();
			string value = null;
			if (array2.Length == 2)
			{
				value = TextUtils.UnQuoteString(MIME_Utils.UnfoldHeader(array2[1].Trim()));
			}
			string[] array3 = text2.Split('*');
			int index = 0;
			bool encoded = array3.Length == 3;
			if (array3.Length >= 2)
			{
				try
				{
					index = Convert.ToInt32(array3[1]);
				}
				catch
				{
				}
			}
			if (array3.Length >= 2 || !keyValueCollection.ContainsKey(array3[0]))
			{
				if (!keyValueCollection.ContainsKey(array3[0]))
				{
					keyValueCollection.Add(array3[0], new _ParameterBuilder(array3[0]));
				}
				keyValueCollection[array3[0]].AddPart(index, encoded, value);
			}
		}
		foreach (_ParameterBuilder item in keyValueCollection)
		{
			m_pParameters.Add(item.Name, item.GetParamter());
		}
		m_IsModified = false;
	}

	private static string DecodeExtOctet(string text, Encoding charset)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (charset == null)
		{
			throw new ArgumentNullException("charset");
		}
		int count = 0;
		byte[] array = new byte[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '%')
			{
				array[count++] = byte.Parse(text[i + 1].ToString() + text[i + 2], NumberStyles.HexNumber);
				i += 2;
			}
			else
			{
				array[count++] = (byte)text[i];
			}
		}
		return charset.GetString(array, 0, count);
	}

	public IEnumerator GetEnumerator()
	{
		return m_pParameters.Values.GetEnumerator();
	}
}
