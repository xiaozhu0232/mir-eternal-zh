namespace LumiSoft.Net.Mime.vCard;

public class EmailAddress
{
	private Item m_pItem;

	private EmailAddressType_enum m_Type = EmailAddressType_enum.Internet;

	private string m_EmailAddress = "";

	public Item Item => m_pItem;

	public EmailAddressType_enum EmailType
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
			Changed();
		}
	}

	public string Email
	{
		get
		{
			return m_EmailAddress;
		}
		set
		{
			m_EmailAddress = value;
			Changed();
		}
	}

	internal EmailAddress(Item item, EmailAddressType_enum type, string emailAddress)
	{
		m_pItem = item;
		m_Type = type;
		m_EmailAddress = emailAddress;
	}

	private void Changed()
	{
		m_pItem.ParametersString = EmailTypeToString(m_Type);
		m_pItem.SetDecodedValue(m_EmailAddress);
	}

	internal static EmailAddress Parse(Item item)
	{
		EmailAddressType_enum emailAddressType_enum = EmailAddressType_enum.NotSpecified;
		if (item.ParametersString.ToUpper().IndexOf("PREF") != -1)
		{
			emailAddressType_enum |= EmailAddressType_enum.Preferred;
		}
		if (item.ParametersString.ToUpper().IndexOf("INTERNET") != -1)
		{
			emailAddressType_enum |= EmailAddressType_enum.Internet;
		}
		if (item.ParametersString.ToUpper().IndexOf("X400") != -1)
		{
			emailAddressType_enum |= EmailAddressType_enum.X400;
		}
		return new EmailAddress(item, emailAddressType_enum, item.DecodedValue);
	}

	internal static string EmailTypeToString(EmailAddressType_enum type)
	{
		string text = "";
		if ((type & EmailAddressType_enum.Internet) != 0)
		{
			text += "INTERNET,";
		}
		if ((type & EmailAddressType_enum.Preferred) != 0)
		{
			text += "PREF,";
		}
		if ((type & EmailAddressType_enum.X400) != 0)
		{
			text += "X400,";
		}
		if (text.EndsWith(","))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}
}
