using System;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public enum ContentDisposition_enum
{
	Attachment = 0,
	Inline = 1,
	NotSpecified = 30,
	Unknown = 40
}
