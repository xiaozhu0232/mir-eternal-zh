namespace LumiSoft.Net.Mime.vCard;

public class Name
{
	private string m_LastName = "";

	private string m_FirstName = "";

	private string m_AdditionalNames = "";

	private string m_HonorificPrefix = "";

	private string m_HonorificSuffix = "";

	public string LastName => m_LastName;

	public string FirstName => m_FirstName;

	public string AdditionalNames => m_AdditionalNames;

	public string HonorificPerfix => m_HonorificPrefix;

	public string HonorificSuffix => m_HonorificSuffix;

	public Name(string lastName, string firstName, string additionalNames, string honorificPrefix, string honorificSuffix)
	{
		m_LastName = lastName;
		m_FirstName = firstName;
		m_AdditionalNames = additionalNames;
		m_HonorificPrefix = honorificPrefix;
		m_HonorificSuffix = honorificSuffix;
	}

	internal Name()
	{
	}

	public string ToValueString()
	{
		return m_LastName + ";" + m_FirstName + ";" + m_AdditionalNames + ";" + m_HonorificPrefix + ";" + m_HonorificSuffix;
	}

	internal static Name Parse(Item item)
	{
		string[] array = item.DecodedValue.Split(';');
		Name name = new Name();
		if (array.Length >= 1)
		{
			name.m_LastName = array[0];
		}
		if (array.Length >= 2)
		{
			name.m_FirstName = array[1];
		}
		if (array.Length >= 3)
		{
			name.m_AdditionalNames = array[2];
		}
		if (array.Length >= 4)
		{
			name.m_HonorificPrefix = array[3];
		}
		if (array.Length >= 5)
		{
			name.m_HonorificSuffix = array[4];
		}
		return name;
	}
}
