using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace LumiSoft.Net.Mime.vCard;

public class vCard
{
	private Encoding m_pCharset;

	private ItemCollection m_pItems;

	private DeliveryAddressCollection m_pAddresses;

	private PhoneNumberCollection m_pPhoneNumbers;

	private EmailAddressCollection m_pEmailAddresses;

	public Encoding Charset
	{
		get
		{
			return m_pCharset;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pCharset = value;
		}
	}

	public ItemCollection Items => m_pItems;

	public string Version
	{
		get
		{
			return m_pItems.GetFirst("VERSION")?.DecodedValue;
		}
		set
		{
			m_pItems.SetValue("VERSION", value);
		}
	}

	public Name Name
	{
		get
		{
			Item first = m_pItems.GetFirst("N");
			if (first != null)
			{
				return Name.Parse(first);
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				m_pItems.SetDecodedValue("N", value.ToValueString());
			}
			else
			{
				m_pItems.SetDecodedValue("N", null);
			}
		}
	}

	public string FormattedName
	{
		get
		{
			return m_pItems.GetFirst("FN")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("FN", value);
		}
	}

	public string NickName
	{
		get
		{
			return m_pItems.GetFirst("NICKNAME")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("NICKNAME", value);
		}
	}

	public Image Photo
	{
		get
		{
			Item first = m_pItems.GetFirst("PHOTO");
			if (first != null)
			{
				return Image.FromStream(new MemoryStream(Encoding.Default.GetBytes(first.DecodedValue)));
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				MemoryStream memoryStream = new MemoryStream();
				value.Save(memoryStream, ImageFormat.Jpeg);
				m_pItems.SetValue("PHOTO", "ENCODING=b;TYPE=JPEG", Convert.ToBase64String(memoryStream.ToArray()));
			}
			else
			{
				m_pItems.SetValue("PHOTO", null);
			}
		}
	}

	public DateTime BirthDate
	{
		get
		{
			Item first = m_pItems.GetFirst("BDAY");
			if (first != null)
			{
				string s = first.DecodedValue.Replace("-", "");
				string[] formats = new string[2] { "yyyyMMdd", "yyyyMMddz" };
				return DateTime.ParseExact(s, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
			}
			return DateTime.MinValue;
		}
		set
		{
			if (value != DateTime.MinValue)
			{
				m_pItems.SetValue("BDAY", value.ToString("yyyyMMdd"));
			}
			else
			{
				m_pItems.SetValue("BDAY", null);
			}
		}
	}

	public DeliveryAddressCollection Addresses
	{
		get
		{
			if (m_pAddresses == null)
			{
				m_pAddresses = new DeliveryAddressCollection(this);
			}
			return m_pAddresses;
		}
	}

	public PhoneNumberCollection PhoneNumbers
	{
		get
		{
			if (m_pPhoneNumbers == null)
			{
				m_pPhoneNumbers = new PhoneNumberCollection(this);
			}
			return m_pPhoneNumbers;
		}
	}

	public EmailAddressCollection EmailAddresses
	{
		get
		{
			if (m_pEmailAddresses == null)
			{
				m_pEmailAddresses = new EmailAddressCollection(this);
			}
			return m_pEmailAddresses;
		}
	}

	public string Title
	{
		get
		{
			return m_pItems.GetFirst("TITLE")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("TITLE", value);
		}
	}

	public string Role
	{
		get
		{
			return m_pItems.GetFirst("ROLE")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("ROLE", value);
		}
	}

	public string Organization
	{
		get
		{
			return m_pItems.GetFirst("ORG")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("ORG", value);
		}
	}

	public string NoteText
	{
		get
		{
			return m_pItems.GetFirst("NOTE")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("NOTE", value);
		}
	}

	public string UID
	{
		get
		{
			return m_pItems.GetFirst("UID")?.DecodedValue;
		}
		set
		{
			m_pItems.SetDecodedValue("UID", value);
		}
	}

	public string HomeURL
	{
		get
		{
			Item[] array = m_pItems.Get("URL");
			foreach (Item item in array)
			{
				if (item.ParametersString == "" || item.ParametersString.ToUpper().IndexOf("HOME") > -1)
				{
					return item.DecodedValue;
				}
			}
			return null;
		}
		set
		{
			Item[] array = m_pItems.Get("URL");
			foreach (Item item in array)
			{
				if (item.ParametersString.ToUpper().IndexOf("HOME") > -1)
				{
					if (value != null)
					{
						item.Value = value;
					}
					else
					{
						m_pItems.Remove(item);
					}
					return;
				}
			}
			if (value != null)
			{
				m_pItems.Add("URL", "HOME", value);
			}
		}
	}

	public string WorkURL
	{
		get
		{
			Item[] array = m_pItems.Get("URL");
			foreach (Item item in array)
			{
				if (item.ParametersString.ToUpper().IndexOf("WORK") > -1)
				{
					return item.DecodedValue;
				}
			}
			return null;
		}
		set
		{
			Item[] array = m_pItems.Get("URL");
			foreach (Item item in array)
			{
				if (item.ParametersString.ToUpper().IndexOf("WORK") > -1)
				{
					if (value != null)
					{
						item.Value = value;
					}
					else
					{
						m_pItems.Remove(item);
					}
					return;
				}
			}
			if (value != null)
			{
				m_pItems.Add("URL", "WORK", value);
			}
		}
	}

	public vCard()
	{
		m_pCharset = Encoding.UTF8;
		m_pItems = new ItemCollection(this);
		Version = "3.0";
		UID = Guid.NewGuid().ToString();
	}

	public byte[] ToByte()
	{
		MemoryStream memoryStream = new MemoryStream();
		ToStream(memoryStream);
		return memoryStream.ToArray();
	}

	public void ToFile(string file)
	{
		using FileStream stream = File.Create(file);
		ToStream(stream);
	}

	public void ToStream(Stream stream)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("BEGIN:VCARD\r\n");
		foreach (Item pItem in m_pItems)
		{
			stringBuilder.Append(pItem.ToItemString() + "\r\n");
		}
		stringBuilder.Append("END:VCARD\r\n");
		byte[] bytes = m_pCharset.GetBytes(stringBuilder.ToString());
		stream.Write(bytes, 0, bytes.Length);
	}

	public static List<vCard> ParseMultiple(string file)
	{
		List<vCard> list = new List<vCard>();
		List<string> list2 = new List<string>();
		string text = "";
		bool flag = false;
		using FileStream stream = File.OpenRead(file);
		TextReader textReader = new StreamReader(stream, Encoding.Default);
		while (text != null)
		{
			text = textReader.ReadLine();
			if (text != null && text.ToUpper() == "BEGIN:VCARD")
			{
				flag = true;
			}
			if (flag)
			{
				list2.Add(text);
				if (text != null && text.ToUpper() == "END:VCARD")
				{
					vCard vCard2 = new vCard();
					vCard2.ParseStrings(list2);
					list.Add(vCard2);
					list2.Clear();
					flag = false;
				}
			}
		}
		return list;
	}

	public void Parse(string file)
	{
		List<string> list = new List<string>();
		string[] array = File.ReadAllLines(file, Encoding.Default);
		foreach (string item in array)
		{
			list.Add(item);
		}
		ParseStrings(list);
	}

	public void Parse(FileStream stream)
	{
		List<string> list = new List<string>();
		string text = "";
		TextReader textReader = new StreamReader(stream, Encoding.Default);
		while (text != null)
		{
			text = textReader.ReadLine();
			list.Add(text);
		}
		ParseStrings(list);
	}

	public void Parse(Stream stream)
	{
		List<string> list = new List<string>();
		string text = "";
		TextReader textReader = new StreamReader(stream, Encoding.Default);
		while (text != null)
		{
			text = textReader.ReadLine();
			list.Add(text);
		}
		ParseStrings(list);
	}

	public void ParseStrings(List<string> fileStrings)
	{
		m_pItems.Clear();
		m_pPhoneNumbers = null;
		m_pEmailAddresses = null;
		int index = 0;
		string text = fileStrings[index];
		while (text != null && text.ToUpper() != "BEGIN:VCARD")
		{
			text = fileStrings[index++];
		}
		text = fileStrings[index++];
		while (text != null && text.ToUpper() != "END:VCARD")
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(text);
			text = fileStrings[index++];
			while (text != null && (text.StartsWith("\t") || text.StartsWith(" ")))
			{
				stringBuilder.Append(text.Substring(1));
				text = fileStrings[index++];
			}
			string[] array = stringBuilder.ToString().Split(new char[1] { ':' }, 2);
			string[] array2 = array[0].Split(new char[1] { ';' }, 2);
			string name = array2[0];
			string parametes = "";
			if (array2.Length == 2)
			{
				parametes = array2[1];
			}
			string value = "";
			if (array.Length == 2)
			{
				value = array[1];
			}
			m_pItems.Add(name, parametes, value);
		}
	}
}
