namespace LumiSoft.Net.Mime.vCard;

public class DeliveryAddress
{
	private Item m_pItem;

	private DeliveryAddressType_enum m_Type = (DeliveryAddressType_enum)92;

	private string m_PostOfficeAddress = "";

	private string m_ExtendedAddress = "";

	private string m_Street = "";

	private string m_Locality = "";

	private string m_Region = "";

	private string m_PostalCode = "";

	private string m_Country = "";

	public Item Item => m_pItem;

	public DeliveryAddressType_enum AddressType
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

	public string PostOfficeAddress
	{
		get
		{
			return m_PostOfficeAddress;
		}
		set
		{
			m_PostOfficeAddress = value;
			Changed();
		}
	}

	public string ExtendedAddress
	{
		get
		{
			return m_ExtendedAddress;
		}
		set
		{
			m_ExtendedAddress = value;
			Changed();
		}
	}

	public string Street
	{
		get
		{
			return m_Street;
		}
		set
		{
			m_Street = value;
			Changed();
		}
	}

	public string Locality
	{
		get
		{
			return m_Locality;
		}
		set
		{
			m_Locality = value;
			Changed();
		}
	}

	public string Region
	{
		get
		{
			return m_Region;
		}
		set
		{
			m_Region = value;
			Changed();
		}
	}

	public string PostalCode
	{
		get
		{
			return m_PostalCode;
		}
		set
		{
			m_PostalCode = value;
			Changed();
		}
	}

	public string Country
	{
		get
		{
			return m_Country;
		}
		set
		{
			m_Country = value;
			Changed();
		}
	}

	internal DeliveryAddress(Item item, DeliveryAddressType_enum addressType, string postOfficeAddress, string extendedAddress, string street, string locality, string region, string postalCode, string country)
	{
		m_pItem = item;
		m_Type = addressType;
		m_PostOfficeAddress = postOfficeAddress;
		m_ExtendedAddress = extendedAddress;
		m_Street = street;
		m_Locality = locality;
		m_Region = region;
		m_PostalCode = postalCode;
		m_Country = country;
	}

	private void Changed()
	{
		string value = vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_PostOfficeAddress) + ";" + vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_ExtendedAddress) + ";" + vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_Street) + ";" + vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_Locality) + ";" + vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_Region) + ";" + vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_PostalCode) + ";" + vCard_Utils.Encode(m_pItem.Owner.Version, m_pItem.Owner.Charset, m_Country);
		m_pItem.ParametersString = AddressTypeToString(m_Type);
		m_pItem.ParametersString += ";CHARSET=utf-8";
		m_pItem.Value = value;
	}

	internal static DeliveryAddress Parse(Item item)
	{
		DeliveryAddressType_enum deliveryAddressType_enum = DeliveryAddressType_enum.NotSpecified;
		if (item.ParametersString.ToUpper().IndexOf("PREF") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Preferred;
		}
		if (item.ParametersString.ToUpper().IndexOf("DOM") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Domestic;
		}
		if (item.ParametersString.ToUpper().IndexOf("INTL") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Ineternational;
		}
		if (item.ParametersString.ToUpper().IndexOf("POSTAL") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Postal;
		}
		if (item.ParametersString.ToUpper().IndexOf("PARCEL") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Parcel;
		}
		if (item.ParametersString.ToUpper().IndexOf("HOME") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Home;
		}
		if (item.ParametersString.ToUpper().IndexOf("WORK") != -1)
		{
			deliveryAddressType_enum |= DeliveryAddressType_enum.Work;
		}
		string[] array = item.DecodedValue.Split(';');
		return new DeliveryAddress(item, deliveryAddressType_enum, (array.Length >= 1) ? array[0] : "", (array.Length >= 2) ? array[1] : "", (array.Length >= 3) ? array[2] : "", (array.Length >= 4) ? array[3] : "", (array.Length >= 5) ? array[4] : "", (array.Length >= 6) ? array[5] : "", (array.Length >= 7) ? array[6] : "");
	}

	internal static string AddressTypeToString(DeliveryAddressType_enum type)
	{
		string text = "";
		if ((type & DeliveryAddressType_enum.Domestic) != 0)
		{
			text += "DOM,";
		}
		if ((type & DeliveryAddressType_enum.Home) != 0)
		{
			text += "HOME,";
		}
		if ((type & DeliveryAddressType_enum.Ineternational) != 0)
		{
			text += "INTL,";
		}
		if ((type & DeliveryAddressType_enum.Parcel) != 0)
		{
			text += "PARCEL,";
		}
		if ((type & DeliveryAddressType_enum.Postal) != 0)
		{
			text += "POSTAL,";
		}
		if ((type & DeliveryAddressType_enum.Preferred) != 0)
		{
			text += "Preferred,";
		}
		if ((type & DeliveryAddressType_enum.Work) != 0)
		{
			text += "Work,";
		}
		if (text.EndsWith(","))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}
}
