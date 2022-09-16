using System;

namespace LumiSoft.Net.Mime;

[Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
public enum MediaType_enum
{
	Text = 1,
	Text_plain = 3,
	Text_html = 5,
	Text_xml = 9,
	Text_rtf = 17,
	Image = 32,
	Image_gif = 96,
	Image_tiff = 160,
	Image_jpeg = 288,
	Audio = 256,
	Video = 1024,
	Application = 2048,
	Application_octet_stream = 6144,
	Multipart = 8192,
	Multipart_mixed = 24576,
	Multipart_alternative = 40960,
	Multipart_parallel = 73728,
	Multipart_related = 139264,
	Multipart_signed = 270336,
	Message = 524288,
	Message_rfc822 = 1572864,
	NotSpecified = 2097152,
	Unknown = 4194304
}
