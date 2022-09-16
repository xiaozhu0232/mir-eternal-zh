namespace LumiSoft.Net.MIME;

public class MIME_MediaTypes
{
	public class Application
	{
		public static readonly string octet_stream = "application/octet-stream";

		public static readonly string pdf = "application/pdf";

		public static readonly string sdp = "application/sdp";

		public static readonly string xml = "application/xml";

		public static readonly string zip = "application/zip";

		public static readonly string x_pkcs7_signature = "application/x-pkcs7-signature";

		public static readonly string pkcs7_mime = "application/pkcs7-mime";
	}

	public class Image
	{
		public static readonly string gif = "image/gif";

		public static readonly string jpeg = "image/jpeg";

		public static readonly string tiff = "image/tiff";
	}

	public class Text
	{
		public static readonly string calendar = "text/calendar";

		public static readonly string css = "text/css";

		public static readonly string html = "text/html";

		public static readonly string plain = "text/plain";

		public static readonly string rfc822_headers = "text/rfc822-headers";

		public static readonly string richtext = "text/richtext";

		public static readonly string xml = "text/xml";
	}

	public class Multipart
	{
		public static readonly string alternative = "multipart/alternative";

		public static readonly string digest = "multipart/digest";

		public static readonly string encrypted = "multipart/digest";

		public static readonly string form_data = "multipart/form-data";

		public static readonly string mixed = "multipart/mixed";

		public static readonly string parallel = "multipart/parallel";

		public static readonly string related = "multipart/related";

		public static readonly string report = "multipart/report";

		public static readonly string signed = "multipart/signed";

		public static readonly string voice_message = "multipart/voice-message";
	}

	public class Message
	{
		public static readonly string rfc822 = "message/rfc822";

		public static readonly string disposition_notification = "message/disposition-notification";

		public static readonly string delivery_status = "message/delivery-status";
	}
}
