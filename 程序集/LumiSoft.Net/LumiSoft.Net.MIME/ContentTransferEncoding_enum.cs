using System;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public enum ContentTransferEncoding_enum
{
	_7bit = 1,
	_8bit = 2,
	Binary = 3,
	QuotedPrintable = 4,
	Base64 = 5,
	NotSpecified = 30,
	Unknown = 40
}
