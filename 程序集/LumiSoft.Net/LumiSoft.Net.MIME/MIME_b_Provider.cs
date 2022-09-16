using System;
using System.Collections.Generic;
using System.Reflection;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME;

public class MIME_b_Provider
{
	private Dictionary<string, Type> m_pBodyTypes;

	public MIME_b_Provider()
	{
		m_pBodyTypes = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
		m_pBodyTypes.Add("application/pkcs7-mime", typeof(MIME_b_ApplicationPkcs7Mime));
		m_pBodyTypes.Add("message/rfc822", typeof(MIME_b_MessageRfc822));
		m_pBodyTypes.Add("message/delivery-status", typeof(MIME_b_MessageDeliveryStatus));
		m_pBodyTypes.Add("multipart/alternative", typeof(MIME_b_MultipartAlternative));
		m_pBodyTypes.Add("multipart/digest", typeof(MIME_b_MultipartDigest));
		m_pBodyTypes.Add("multipart/encrypted", typeof(MIME_b_MultipartEncrypted));
		m_pBodyTypes.Add("multipart/form-data", typeof(MIME_b_MultipartFormData));
		m_pBodyTypes.Add("multipart/mixed", typeof(MIME_b_MultipartMixed));
		m_pBodyTypes.Add("multipart/parallel", typeof(MIME_b_MultipartParallel));
		m_pBodyTypes.Add("multipart/related", typeof(MIME_b_MultipartRelated));
		m_pBodyTypes.Add("multipart/report", typeof(MIME_b_MultipartReport));
		m_pBodyTypes.Add("multipart/signed", typeof(MIME_b_MultipartSigned));
	}

	public MIME_b Parse(MIME_Entity owner, SmartStream stream, MIME_h_ContentType defaultContentType)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (defaultContentType == null)
		{
			throw new ArgumentNullException("defaultContentType");
		}
		string text = defaultContentType.TypeWithSubtype;
		try
		{
			if (owner.ContentType != null)
			{
				text = owner.ContentType.TypeWithSubtype;
			}
		}
		catch
		{
			text = "unknown/unknown";
		}
		Type type = null;
		type = (m_pBodyTypes.ContainsKey(text) ? m_pBodyTypes[text] : (text.Split('/')[0].ToLowerInvariant() switch
		{
			"application" => typeof(MIME_b_Application), 
			"audio" => typeof(MIME_b_Audio), 
			"image" => typeof(MIME_b_Image), 
			"message" => typeof(MIME_b_Message), 
			"multipart" => typeof(MIME_b_Multipart), 
			"text" => typeof(MIME_b_Text), 
			"video" => typeof(MIME_b_Video), 
			_ => typeof(MIME_b_Unknown), 
		}));
		return (MIME_b)type.GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, new object[3] { owner, defaultContentType, stream });
	}
}
