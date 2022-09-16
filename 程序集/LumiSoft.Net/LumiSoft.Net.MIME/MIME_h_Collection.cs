using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_h_Collection : IEnumerable
{
	private bool m_IsModified;

	private MIME_h_Provider m_pProvider;

	private List<MIME_h> m_pFields;

	public bool IsModified
	{
		get
		{
			if (m_IsModified)
			{
				return true;
			}
			foreach (MIME_h pField in m_pFields)
			{
				if (pField.IsModified)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int Count => m_pFields.Count;

	public MIME_h this[int index]
	{
		get
		{
			if (index < 0 || index >= m_pFields.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_pFields[index];
		}
	}

	public MIME_h[] this[string name]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			List<MIME_h> list = new List<MIME_h>();
			MIME_h[] array = m_pFields.ToArray();
			foreach (MIME_h mIME_h in array)
			{
				if (string.Compare(name, mIME_h.Name, ignoreCase: true) == 0)
				{
					list.Add(mIME_h);
				}
			}
			return list.ToArray();
		}
	}

	public MIME_h_Provider FieldsProvider => m_pProvider;

	public MIME_h_Collection(MIME_h_Provider provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		m_pProvider = provider;
		m_pFields = new List<MIME_h>();
	}

	public void Insert(int index, MIME_h field)
	{
		if (index < 0 || index > m_pFields.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		m_pFields.Insert(index, field);
		m_IsModified = true;
	}

	public MIME_h Add(string field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		MIME_h mIME_h = m_pProvider.Parse(field);
		m_pFields.Add(mIME_h);
		m_IsModified = true;
		return mIME_h;
	}

	public void Add(MIME_h field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		m_pFields.Add(field);
		m_IsModified = true;
	}

	public void Remove(MIME_h field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		m_pFields.Remove(field);
		m_IsModified = true;
	}

	public void RemoveAll(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("Argument 'name' value must be specified.", "name");
		}
		MIME_h[] array = m_pFields.ToArray();
		foreach (MIME_h mIME_h in array)
		{
			if (string.Compare(name, mIME_h.Name, ignoreCase: true) == 0)
			{
				m_pFields.Remove(mIME_h);
			}
		}
		m_IsModified = true;
	}

	public void Clear()
	{
		m_pFields.Clear();
		m_IsModified = true;
	}

	public bool Contains(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == string.Empty)
		{
			throw new ArgumentException("Argument 'name' value must be specified.", "name");
		}
		MIME_h[] array = m_pFields.ToArray();
		foreach (MIME_h mIME_h in array)
		{
			if (string.Compare(name, mIME_h.Name, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(MIME_h field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		return m_pFields.Contains(field);
	}

	public MIME_h GetFirst(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		MIME_h[] array = m_pFields.ToArray();
		foreach (MIME_h mIME_h in array)
		{
			if (string.Equals(name, mIME_h.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return mIME_h;
			}
		}
		return null;
	}

	public void ReplaceFirst(MIME_h field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		for (int i = 0; i < m_pFields.Count; i++)
		{
			if (string.Equals(field.Name, m_pFields[i].Name, StringComparison.CurrentCultureIgnoreCase))
			{
				m_pFields.RemoveAt(i);
				m_pFields.Insert(i, field);
				break;
			}
		}
	}

	public MIME_h[] ToArray()
	{
		return m_pFields.ToArray();
	}

	public void ToFile(string fileName, MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		using FileStream stream = File.Create(fileName);
		ToStream(stream, wordEncoder, parmetersCharset);
	}

	public byte[] ToByte(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
	{
		using MemoryStream memoryStream = new MemoryStream();
		ToStream(memoryStream, wordEncoder, parmetersCharset);
		memoryStream.Position = 0L;
		return memoryStream.ToArray();
	}

	public void ToStream(Stream stream, MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
	{
		ToStream(stream, wordEncoder, parmetersCharset, reEncod: false);
	}

	public void ToStream(Stream stream, MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncod)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(ToString(wordEncoder, parmetersCharset, reEncod));
		stream.Write(bytes, 0, bytes.Length);
	}

	public override string ToString()
	{
		return ToString(null, null, reEncode: false);
	}

	public string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
	{
		return ToString(wordEncoder, parmetersCharset, reEncode: false);
	}

	public string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MIME_h pField in m_pFields)
		{
			stringBuilder.Append(pField.ToString(wordEncoder, parmetersCharset, reEncode));
		}
		return stringBuilder.ToString();
	}

	public void Parse(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Parse(new SmartStream(new MemoryStream(Encoding.UTF8.GetBytes(value)), owner: true));
	}

	public void Parse(SmartStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Parse(stream, Encoding.UTF8);
	}

	public void Parse(SmartStream stream, Encoding encoding)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		StringBuilder stringBuilder = new StringBuilder();
		SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(new byte[84000], SizeExceededAction.ThrowException);
		while (true)
		{
			stream.ReadLine(readLineAsyncOP, async: false);
			if (readLineAsyncOP.Error != null)
			{
				throw readLineAsyncOP.Error;
			}
			if (readLineAsyncOP.BytesInBuffer == 0)
			{
				if (stringBuilder.Length > 0)
				{
					Add(stringBuilder.ToString());
				}
				m_IsModified = false;
				return;
			}
			if (readLineAsyncOP.LineBytesInBuffer == 0)
			{
				break;
			}
			string @string = encoding.GetString(readLineAsyncOP.Buffer, 0, readLineAsyncOP.BytesInBuffer);
			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append(@string);
				continue;
			}
			if (char.IsWhiteSpace(@string[0]))
			{
				stringBuilder.Append(@string);
				continue;
			}
			Add(stringBuilder.ToString());
			stringBuilder = new StringBuilder();
			stringBuilder.Append(@string);
		}
		if (stringBuilder.Length > 0)
		{
			Add(stringBuilder.ToString());
		}
		m_IsModified = false;
	}

	public IEnumerator GetEnumerator()
	{
		return m_pFields.GetEnumerator();
	}
}
