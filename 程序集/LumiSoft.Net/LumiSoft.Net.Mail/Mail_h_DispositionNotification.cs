using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_h_DispositionNotificationOptions : MIME_h
{
	public override bool IsModified => true;

	public override string Name => "Disposition-Notification-Options";

	public string Address => "TODO:";

	public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset, bool reEncode)
	{
		return "TODO:";
	}
}
