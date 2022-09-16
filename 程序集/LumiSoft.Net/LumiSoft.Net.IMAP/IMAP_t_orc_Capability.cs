using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_t_orc_Capability : IMAP_t_orc
{
	private string[] m_pCapabilities;

	public string[] Capabilities => m_pCapabilities;

	public IMAP_t_orc_Capability(string[] capabilities)
	{
		if (capabilities == null)
		{
			throw new ArgumentNullException("capabilities");
		}
		m_pCapabilities = capabilities;
	}

	public new static IMAP_t_orc_Capability Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		string[] array = r.ReadParenthesized().Split(new char[1] { ' ' }, 2);
		if (!string.Equals("CAPABILITY", array[0], StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Invalid CAPABILITY response value.", "r");
		}
		if (array.Length != 2)
		{
			throw new ArgumentException("Invalid CAPABILITY response value.", "r");
		}
		return new IMAP_t_orc_Capability(array[1].Split(' '));
	}

	public override string ToString()
	{
		return "CAPABILITY (" + Net_Utils.ArrayToString(m_pCapabilities, " ") + ")";
	}
}
