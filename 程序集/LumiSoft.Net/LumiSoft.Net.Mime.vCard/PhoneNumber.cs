namespace LumiSoft.Net.Mime.vCard;

public class PhoneNumber
{
	private Item m_pItem;

	private PhoneNumberType_enum m_Type = PhoneNumberType_enum.Voice;

	private string m_Number = "";

	public Item Item => m_pItem;

	public PhoneNumberType_enum NumberType
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

	public string Number
	{
		get
		{
			return m_Number;
		}
		set
		{
			m_Number = value;
			Changed();
		}
	}

	internal PhoneNumber(Item item, PhoneNumberType_enum type, string number)
	{
		m_pItem = item;
		m_Type = type;
		m_Number = number;
	}

	private void Changed()
	{
		m_pItem.ParametersString = PhoneTypeToString(m_Type);
		m_pItem.Value = m_Number;
	}

	internal static PhoneNumber Parse(Item item)
	{
		PhoneNumberType_enum phoneNumberType_enum = PhoneNumberType_enum.NotSpecified;
		if (item.ParametersString.ToUpper().IndexOf("PREF") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Preferred;
		}
		if (item.ParametersString.ToUpper().IndexOf("HOME") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Home;
		}
		if (item.ParametersString.ToUpper().IndexOf("MSG") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Msg;
		}
		if (item.ParametersString.ToUpper().IndexOf("WORK") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Work;
		}
		if (item.ParametersString.ToUpper().IndexOf("VOICE") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Voice;
		}
		if (item.ParametersString.ToUpper().IndexOf("FAX") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Fax;
		}
		if (item.ParametersString.ToUpper().IndexOf("CELL") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Cellular;
		}
		if (item.ParametersString.ToUpper().IndexOf("VIDEO") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Video;
		}
		if (item.ParametersString.ToUpper().IndexOf("PAGER") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Pager;
		}
		if (item.ParametersString.ToUpper().IndexOf("BBS") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.BBS;
		}
		if (item.ParametersString.ToUpper().IndexOf("MODEM") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Modem;
		}
		if (item.ParametersString.ToUpper().IndexOf("CAR") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.Car;
		}
		if (item.ParametersString.ToUpper().IndexOf("ISDN") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.ISDN;
		}
		if (item.ParametersString.ToUpper().IndexOf("PCS") != -1)
		{
			phoneNumberType_enum |= PhoneNumberType_enum.PCS;
		}
		return new PhoneNumber(item, phoneNumberType_enum, item.Value);
	}

	internal static string PhoneTypeToString(PhoneNumberType_enum type)
	{
		string text = "";
		if ((type & PhoneNumberType_enum.BBS) != 0)
		{
			text += "BBS,";
		}
		if ((type & PhoneNumberType_enum.Car) != 0)
		{
			text += "CAR,";
		}
		if ((type & PhoneNumberType_enum.Cellular) != 0)
		{
			text += "CELL,";
		}
		if ((type & PhoneNumberType_enum.Fax) != 0)
		{
			text += "FAX,";
		}
		if ((type & PhoneNumberType_enum.Home) != 0)
		{
			text += "HOME,";
		}
		if ((type & PhoneNumberType_enum.ISDN) != 0)
		{
			text += "ISDN,";
		}
		if ((type & PhoneNumberType_enum.Modem) != 0)
		{
			text += "MODEM,";
		}
		if ((type & PhoneNumberType_enum.Msg) != 0)
		{
			text += "MSG,";
		}
		if ((type & PhoneNumberType_enum.Pager) != 0)
		{
			text += "PAGER,";
		}
		if ((type & PhoneNumberType_enum.PCS) != 0)
		{
			text += "PCS,";
		}
		if ((type & PhoneNumberType_enum.Preferred) != 0)
		{
			text += "PREF,";
		}
		if ((type & PhoneNumberType_enum.Video) != 0)
		{
			text += "VIDEO,";
		}
		if ((type & PhoneNumberType_enum.Voice) != 0)
		{
			text += "VOICE,";
		}
		if ((type & PhoneNumberType_enum.Work) != 0)
		{
			text += "WORK,";
		}
		if (text.EndsWith(","))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}
}
