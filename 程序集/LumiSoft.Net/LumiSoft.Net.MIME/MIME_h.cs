using System.Text;

namespace LumiSoft.Net.MIME;

public abstract class MIME_h
{
	public abstract bool IsModified { get; }

	public abstract string Name { get; }

	public MIME_h()
	{
	}

	public override string ToString()
	{
		return ToString(null, null, reEncode: false);
	}

	public string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
	{
		return ToString(wordEncoder, parmetersCharset, reEncode: false);
	}

	public abstract string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode);

	public string ValueToString()
	{
		return ValueToString(null, null);
	}

	public string ValueToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
	{
		return ToString(wordEncoder, parmetersCharset).Split(new char[1] { ':' }, 2)[1].TrimStart();
	}
}
