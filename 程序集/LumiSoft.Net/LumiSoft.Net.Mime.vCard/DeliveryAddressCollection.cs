using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.Mime.vCard;

public class DeliveryAddressCollection : IEnumerable
{
	private vCard m_pOwner;

	private List<DeliveryAddress> m_pCollection;

	public int Count => m_pCollection.Count;

	public DeliveryAddress this[int index] => m_pCollection[index];

	internal DeliveryAddressCollection(vCard owner)
	{
		m_pOwner = owner;
		m_pCollection = new List<DeliveryAddress>();
		Item[] array = owner.Items.Get("ADR");
		foreach (Item item in array)
		{
			m_pCollection.Add(DeliveryAddress.Parse(item));
		}
	}

	public void Add(DeliveryAddressType_enum type, string postOfficeAddress, string extendedAddress, string street, string locality, string region, string postalCode, string country)
	{
		string value = vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, postOfficeAddress) + ";" + vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, extendedAddress) + ";" + vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, street) + ";" + vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, locality) + ";" + vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, region) + ";" + vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, postalCode) + ";" + vCard_Utils.Encode(m_pOwner.Version, m_pOwner.Charset, country);
		Item item = m_pOwner.Items.Add("ADR", DeliveryAddress.AddressTypeToString(type), "");
		item.FoldLongLines = false;
		if (m_pOwner.Version.StartsWith("2"))
		{
			item.ParametersString += ";ENCODING=QUOTED-PRINTABLE";
		}
		item.ParametersString = item.ParametersString + ";CHARSET=" + m_pOwner.Charset.WebName;
		item.Value = value;
		m_pCollection.Add(new DeliveryAddress(item, type, postOfficeAddress, extendedAddress, street, locality, region, postalCode, country));
	}

	public void Remove(DeliveryAddress item)
	{
		m_pOwner.Items.Remove(item.Item);
		m_pCollection.Remove(item);
	}

	public void Clear()
	{
		foreach (DeliveryAddress item in m_pCollection)
		{
			m_pOwner.Items.Remove(item.Item);
		}
		m_pCollection.Clear();
	}

	public IEnumerator GetEnumerator()
	{
		return m_pCollection.GetEnumerator();
	}
}
