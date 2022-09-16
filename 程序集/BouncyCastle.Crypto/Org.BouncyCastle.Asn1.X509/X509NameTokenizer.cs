using System.Text;

namespace Org.BouncyCastle.Asn1.X509;

public class X509NameTokenizer
{
	private string value;

	private int index;

	private char separator;

	private StringBuilder buffer = new StringBuilder();

	public X509NameTokenizer(string oid)
		: this(oid, ',')
	{
	}

	public X509NameTokenizer(string oid, char separator)
	{
		value = oid;
		index = -1;
		this.separator = separator;
	}

	public bool HasMoreTokens()
	{
		return index != value.Length;
	}

	public string NextToken()
	{
		if (index == value.Length)
		{
			return null;
		}
		int i = index + 1;
		bool flag = false;
		bool flag2 = false;
		buffer.Remove(0, buffer.Length);
		for (; i != value.Length; i++)
		{
			char c = value[i];
			if (c == '"')
			{
				if (!flag2)
				{
					flag = !flag;
					continue;
				}
				buffer.Append(c);
				flag2 = false;
			}
			else if (flag2 || flag)
			{
				if (c == '#' && buffer[buffer.Length - 1] == '=')
				{
					buffer.Append('\\');
				}
				else if (c == '+' && separator != '+')
				{
					buffer.Append('\\');
				}
				buffer.Append(c);
				flag2 = false;
			}
			else if (c == '\\')
			{
				flag2 = true;
			}
			else
			{
				if (c == separator)
				{
					break;
				}
				buffer.Append(c);
			}
		}
		index = i;
		return buffer.ToString().Trim();
	}
}
