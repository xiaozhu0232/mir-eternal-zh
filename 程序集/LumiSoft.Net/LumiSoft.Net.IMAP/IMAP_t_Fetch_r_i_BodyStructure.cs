using System;
using System.Collections.Generic;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.IMAP;

public abstract class IMAP_t_Fetch_r_i_BodyStructure_e
{
	private IMAP_t_Fetch_r_i_BodyStructure_e_Multipart m_pParent;

	public abstract MIME_h_ContentType ContentType { get; }

	public abstract MIME_h_ContentDisposition ContentDisposition { get; }

	public abstract string Language { get; }

	public abstract string Location { get; }

	public string PartSpecifier
	{
		get
		{
			string text = "";
			if (m_pParent == null)
			{
				text = "";
			}
			else
			{
				IMAP_t_Fetch_r_i_BodyStructure_e bodyPart = this;
				for (IMAP_t_Fetch_r_i_BodyStructure_e_Multipart pParent = m_pParent; pParent != null; pParent = pParent.m_pParent)
				{
					int num = pParent.IndexOfBodyPart(bodyPart) + 1;
					text = ((!string.IsNullOrEmpty(text)) ? (num + "." + text) : num.ToString());
					bodyPart = pParent;
				}
			}
			return text;
		}
	}

	internal void SetParent(IMAP_t_Fetch_r_i_BodyStructure_e_Multipart parent)
	{
		m_pParent = parent;
	}
}
public class IMAP_t_Fetch_r_i_BodyStructure_e_Multipart : IMAP_t_Fetch_r_i_BodyStructure_e
{
	private MIME_h_ContentType m_pContentType;

	private MIME_h_ContentDisposition m_pContentDisposition;

	private string m_Language;

	private string m_Location;

	private List<IMAP_t_Fetch_r_i_BodyStructure_e> m_pBodyParts;

	public override MIME_h_ContentType ContentType => m_pContentType;

	public override MIME_h_ContentDisposition ContentDisposition => m_pContentDisposition;

	public override string Language => m_Language;

	public override string Location => m_Location;

	public IMAP_t_Fetch_r_i_BodyStructure_e[] BodyParts => m_pBodyParts.ToArray();

	private IMAP_t_Fetch_r_i_BodyStructure_e_Multipart()
	{
		m_pBodyParts = new List<IMAP_t_Fetch_r_i_BodyStructure_e>();
	}

	public static IMAP_t_Fetch_r_i_BodyStructure_e_Multipart Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		IMAP_t_Fetch_r_i_BodyStructure_e_Multipart iMAP_t_Fetch_r_i_BodyStructure_e_Multipart = new IMAP_t_Fetch_r_i_BodyStructure_e_Multipart();
		while (r.Available > 0)
		{
			r.ReadToFirstChar();
			if (!r.StartsWith("("))
			{
				break;
			}
			StringReader stringReader = new StringReader(r.ReadParenthesized());
			stringReader.ReadToFirstChar();
			IMAP_t_Fetch_r_i_BodyStructure_e iMAP_t_Fetch_r_i_BodyStructure_e = null;
			iMAP_t_Fetch_r_i_BodyStructure_e = ((!stringReader.StartsWith("(")) ? ((IMAP_t_Fetch_r_i_BodyStructure_e)IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.Parse(stringReader)) : ((IMAP_t_Fetch_r_i_BodyStructure_e)Parse(stringReader)));
			iMAP_t_Fetch_r_i_BodyStructure_e.SetParent(iMAP_t_Fetch_r_i_BodyStructure_e_Multipart);
			iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pBodyParts.Add(iMAP_t_Fetch_r_i_BodyStructure_e);
		}
		string text = IMAP_Utils.ReadString(r);
		if (!string.IsNullOrEmpty(text))
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pContentType = new MIME_h_ContentType("multipart/" + text);
		}
		r.ReadToFirstChar();
		if (r.StartsWith("("))
		{
			StringReader stringReader2 = new StringReader(r.ReadParenthesized());
			if (iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pContentType != null)
			{
				while (stringReader2.Available > 0)
				{
					string text2 = IMAP_Utils.ReadString(stringReader2);
					if (string.IsNullOrEmpty(text2))
					{
						break;
					}
					string text3 = IMAP_Utils.ReadString(stringReader2);
					if (text3 == null)
					{
						text3 = "";
					}
					iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pContentType.Parameters[text2] = MIME_Encoding_EncodedWord.DecodeTextS(text3);
				}
			}
		}
		else
		{
			IMAP_Utils.ReadString(r);
		}
		if (r.StartsWith("("))
		{
			string text4 = IMAP_Utils.ReadString(r);
			if (!string.IsNullOrEmpty(text4))
			{
				iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pContentDisposition = new MIME_h_ContentDisposition(text4);
			}
			r.ReadToFirstChar();
			if (r.StartsWith("("))
			{
				StringReader stringReader3 = new StringReader(r.ReadParenthesized());
				if (iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pContentDisposition != null)
				{
					while (stringReader3.Available > 0)
					{
						string text5 = IMAP_Utils.ReadString(stringReader3);
						if (string.IsNullOrEmpty(text5))
						{
							break;
						}
						string text6 = IMAP_Utils.ReadString(stringReader3);
						if (text6 == null)
						{
							text6 = "";
						}
						iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_pContentDisposition.Parameters[text5] = MIME_Encoding_EncodedWord.DecodeTextS(text6);
					}
				}
			}
			else
			{
				IMAP_Utils.ReadString(r);
			}
		}
		else
		{
			IMAP_Utils.ReadString(r);
		}
		r.ReadToFirstChar();
		if (r.StartsWith("("))
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_Language = r.ReadParenthesized();
		}
		else
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_Language = IMAP_Utils.ReadString(r);
		}
		iMAP_t_Fetch_r_i_BodyStructure_e_Multipart.m_Location = IMAP_Utils.ReadString(r);
		return iMAP_t_Fetch_r_i_BodyStructure_e_Multipart;
	}

	internal int IndexOfBodyPart(IMAP_t_Fetch_r_i_BodyStructure_e bodyPart)
	{
		return m_pBodyParts.IndexOf(bodyPart);
	}
}
public class IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart : IMAP_t_Fetch_r_i_BodyStructure_e
{
	private MIME_h_ContentType m_pContentType;

	private string m_ContentID;

	private string m_ContentDescription;

	private string m_ContentTransferEncoding;

	private long m_ContentSize = -1L;

	private int m_LinesCount = -1;

	private string m_Md5;

	private MIME_h_ContentDisposition m_pContentDisposition;

	private string m_Language;

	private string m_Location;

	public override MIME_h_ContentType ContentType => m_pContentType;

	public string ContentID => m_ContentID;

	public string ContentDescription => m_ContentDescription;

	public string ContentTransferEncoding => m_ContentTransferEncoding;

	public long ContentSize => m_ContentSize;

	public int LinesCount => m_LinesCount;

	public string Md5 => m_Md5;

	public override MIME_h_ContentDisposition ContentDisposition => m_pContentDisposition;

	public override string Language => m_Language;

	public override string Location => m_Location;

	private IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart()
	{
	}

	public static IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart = new IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart();
		string text = IMAP_Utils.ReadString(r);
		string text2 = IMAP_Utils.ReadString(r);
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_pContentType = new MIME_h_ContentType(text + "/" + text2);
		}
		r.ReadToFirstChar();
		if (r.StartsWith("("))
		{
			StringReader stringReader = new StringReader(r.ReadParenthesized());
			if (iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_pContentType != null)
			{
				while (stringReader.Available > 0)
				{
					string text3 = IMAP_Utils.ReadString(stringReader);
					if (string.IsNullOrEmpty(text3))
					{
						break;
					}
					string text4 = IMAP_Utils.ReadString(stringReader);
					if (text4 == null)
					{
						text4 = "";
					}
					iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_pContentType.Parameters[text3] = MIME_Encoding_EncodedWord.DecodeTextS(text4);
				}
			}
		}
		else
		{
			IMAP_Utils.ReadString(r);
		}
		iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_ContentID = IMAP_Utils.ReadString(r);
		iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_ContentDescription = IMAP_Utils.ReadString(r);
		iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_ContentTransferEncoding = IMAP_Utils.ReadString(r);
		string value = IMAP_Utils.ReadString(r);
		if (string.IsNullOrEmpty(value))
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_ContentSize = -1L;
		}
		else
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_ContentSize = Convert.ToInt64(value);
		}
		if (string.Equals("text", text, StringComparison.InvariantCultureIgnoreCase))
		{
			string value2 = IMAP_Utils.ReadString(r);
			if (string.IsNullOrEmpty(value))
			{
				iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_LinesCount = -1;
			}
			else
			{
				iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_LinesCount = Convert.ToInt32(value2);
			}
		}
		if (string.Equals("message", text, StringComparison.InvariantCultureIgnoreCase))
		{
			r.ReadToFirstChar();
			if (r.StartsWith("("))
			{
				r.ReadParenthesized();
			}
			else
			{
				IMAP_Utils.ReadString(r);
			}
			r.ReadToFirstChar();
			if (r.StartsWith("("))
			{
				r.ReadParenthesized();
			}
			else
			{
				IMAP_Utils.ReadString(r);
			}
			string value3 = IMAP_Utils.ReadString(r);
			if (string.IsNullOrEmpty(value))
			{
				iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_LinesCount = -1;
			}
			else
			{
				iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_LinesCount = Convert.ToInt32(value3);
			}
		}
		iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_Md5 = IMAP_Utils.ReadString(r);
		if (r.StartsWith("("))
		{
			string text5 = IMAP_Utils.ReadString(r);
			if (!string.IsNullOrEmpty(text5))
			{
				iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_pContentDisposition = new MIME_h_ContentDisposition(text5);
			}
			r.ReadToFirstChar();
			if (r.StartsWith("("))
			{
				StringReader stringReader2 = new StringReader(r.ReadParenthesized());
				if (iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_pContentDisposition != null)
				{
					while (stringReader2.Available > 0)
					{
						string text6 = IMAP_Utils.ReadString(stringReader2);
						if (string.IsNullOrEmpty(text6))
						{
							break;
						}
						string text7 = IMAP_Utils.ReadString(stringReader2);
						if (text7 == null)
						{
							text7 = "";
						}
						iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_pContentDisposition.Parameters[text6] = MIME_Encoding_EncodedWord.DecodeTextS(text7);
					}
				}
			}
			else
			{
				IMAP_Utils.ReadString(r);
			}
		}
		else
		{
			IMAP_Utils.ReadString(r);
		}
		r.ReadToFirstChar();
		if (r.StartsWith("("))
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_Language = r.ReadParenthesized();
		}
		else
		{
			iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_Language = IMAP_Utils.ReadString(r);
		}
		iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.m_Location = IMAP_Utils.ReadString(r);
		return iMAP_t_Fetch_r_i_BodyStructure_e_SinglePart;
	}
}
public class IMAP_t_Fetch_r_i_BodyStructure : IMAP_t_Fetch_r_i
{
	private IMAP_t_Fetch_r_i_BodyStructure_e m_pMessage;

	public bool IsSigned
	{
		get
		{
			IMAP_t_Fetch_r_i_BodyStructure_e[] allEntities = AllEntities;
			foreach (IMAP_t_Fetch_r_i_BodyStructure_e iMAP_t_Fetch_r_i_BodyStructure_e in allEntities)
			{
				if (string.Equals(iMAP_t_Fetch_r_i_BodyStructure_e.ContentType.TypeWithSubtype, MIME_MediaTypes.Application.pkcs7_mime, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
				if (string.Equals(iMAP_t_Fetch_r_i_BodyStructure_e.ContentType.TypeWithSubtype, MIME_MediaTypes.Multipart.signed, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}

	public IMAP_t_Fetch_r_i_BodyStructure_e Message => m_pMessage;

	public IMAP_t_Fetch_r_i_BodyStructure_e[] AllEntities
	{
		get
		{
			List<IMAP_t_Fetch_r_i_BodyStructure_e> list = new List<IMAP_t_Fetch_r_i_BodyStructure_e>();
			List<IMAP_t_Fetch_r_i_BodyStructure_e> list2 = new List<IMAP_t_Fetch_r_i_BodyStructure_e>();
			list2.Add(m_pMessage);
			while (list2.Count > 0)
			{
				IMAP_t_Fetch_r_i_BodyStructure_e iMAP_t_Fetch_r_i_BodyStructure_e = list2[0];
				list2.RemoveAt(0);
				list.Add(iMAP_t_Fetch_r_i_BodyStructure_e);
				if (iMAP_t_Fetch_r_i_BodyStructure_e is IMAP_t_Fetch_r_i_BodyStructure_e_Multipart)
				{
					IMAP_t_Fetch_r_i_BodyStructure_e[] bodyParts = ((IMAP_t_Fetch_r_i_BodyStructure_e_Multipart)iMAP_t_Fetch_r_i_BodyStructure_e).BodyParts;
					for (int i = 0; i < bodyParts.Length; i++)
					{
						list2.Insert(i, bodyParts[i]);
					}
				}
			}
			return list.ToArray();
		}
	}

	public IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart[] Attachments => GetAttachments(includeInline: false);

	public IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart BodyTextEntity
	{
		get
		{
			IMAP_t_Fetch_r_i_BodyStructure_e[] allEntities = AllEntities;
			foreach (IMAP_t_Fetch_r_i_BodyStructure_e iMAP_t_Fetch_r_i_BodyStructure_e in allEntities)
			{
				if (string.Equals(iMAP_t_Fetch_r_i_BodyStructure_e.ContentType.TypeWithSubtype, MIME_MediaTypes.Text.plain, StringComparison.InvariantCultureIgnoreCase))
				{
					return (IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e;
				}
			}
			return null;
		}
	}

	public IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart BodyTextHtmlEntity
	{
		get
		{
			IMAP_t_Fetch_r_i_BodyStructure_e[] allEntities = AllEntities;
			foreach (IMAP_t_Fetch_r_i_BodyStructure_e iMAP_t_Fetch_r_i_BodyStructure_e in allEntities)
			{
				if (string.Equals(iMAP_t_Fetch_r_i_BodyStructure_e.ContentType.TypeWithSubtype, MIME_MediaTypes.Text.html, StringComparison.InvariantCultureIgnoreCase))
				{
					return (IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e;
				}
			}
			return null;
		}
	}

	private IMAP_t_Fetch_r_i_BodyStructure()
	{
	}

	public static IMAP_t_Fetch_r_i_BodyStructure Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		IMAP_t_Fetch_r_i_BodyStructure iMAP_t_Fetch_r_i_BodyStructure = new IMAP_t_Fetch_r_i_BodyStructure();
		r.ReadToFirstChar();
		if (r.StartsWith("("))
		{
			iMAP_t_Fetch_r_i_BodyStructure.m_pMessage = IMAP_t_Fetch_r_i_BodyStructure_e_Multipart.Parse(r);
		}
		else
		{
			iMAP_t_Fetch_r_i_BodyStructure.m_pMessage = IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart.Parse(r);
		}
		return iMAP_t_Fetch_r_i_BodyStructure;
	}

	public IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart[] GetAttachments(bool includeInline)
	{
		List<IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart> list = new List<IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart>();
		IMAP_t_Fetch_r_i_BodyStructure_e[] allEntities = AllEntities;
		foreach (IMAP_t_Fetch_r_i_BodyStructure_e iMAP_t_Fetch_r_i_BodyStructure_e in allEntities)
		{
			MIME_h_ContentType contentType = iMAP_t_Fetch_r_i_BodyStructure_e.ContentType;
			MIME_h_ContentDisposition contentDisposition = iMAP_t_Fetch_r_i_BodyStructure_e.ContentDisposition;
			if (!(iMAP_t_Fetch_r_i_BodyStructure_e is IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart))
			{
				continue;
			}
			if (contentDisposition != null && string.Equals(contentDisposition.DispositionType, "attachment", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
			}
			else if (contentDisposition != null && string.Equals(contentDisposition.DispositionType, "inline", StringComparison.InvariantCultureIgnoreCase))
			{
				if (includeInline)
				{
					list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
				}
			}
			else if (contentType != null && string.Equals(contentType.Type, "application", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
			}
			else if (contentType != null && string.Equals(contentType.Type, "image", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
			}
			else if (contentType != null && string.Equals(contentType.Type, "video", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
			}
			else if (contentType != null && string.Equals(contentType.Type, "audio", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
			}
			else if (contentType != null && string.Equals(contentType.Type, "message", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add((IMAP_t_Fetch_r_i_BodyStructure_e_SinglePart)iMAP_t_Fetch_r_i_BodyStructure_e);
			}
		}
		return list.ToArray();
	}
}
