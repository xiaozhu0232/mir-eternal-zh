using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public abstract class Mail_t_Address
{
	public Mail_t_Address()
	{
	}

	public abstract string ToString(MIME_Encoding_EncodedWord wordEncoder);
}
