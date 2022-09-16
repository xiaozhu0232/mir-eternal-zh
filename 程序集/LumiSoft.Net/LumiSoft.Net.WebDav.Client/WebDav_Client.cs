using System;
using System.IO;
using System.Net;
using System.Text;

namespace LumiSoft.Net.WebDav.Client;

public class WebDav_Client
{
	private NetworkCredential m_pCredentials;

	public NetworkCredential Credentials
	{
		get
		{
			return m_pCredentials;
		}
		set
		{
			m_pCredentials = value;
		}
	}

	public WebDav_MultiStatus PropFind(string requestUri, string[] propertyNames, int depth)
	{
		if (requestUri == null)
		{
			throw new ArgumentNullException("requestUri");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n");
		stringBuilder.Append("<propfind xmlns=\"DAV:\">\r\n");
		stringBuilder.Append("<prop>\r\n");
		if (propertyNames == null || propertyNames.Length == 0)
		{
			stringBuilder.Append("   <propname/>\r\n");
		}
		else
		{
			foreach (string text in propertyNames)
			{
				stringBuilder.Append("<" + text + "/>");
			}
		}
		stringBuilder.Append("</prop>\r\n");
		stringBuilder.Append("</propfind>\r\n");
		byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
		httpWebRequest.Method = "PROPFIND";
		httpWebRequest.ContentType = "application/xml";
		httpWebRequest.ContentLength = bytes.Length;
		httpWebRequest.Credentials = m_pCredentials;
		if (depth > -1)
		{
			httpWebRequest.Headers.Add("Depth: " + depth);
		}
		httpWebRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
		return WebDav_MultiStatus.Parse(httpWebRequest.GetResponse().GetResponseStream());
	}

	public void MkCol(string uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(uri);
		obj.Method = "MKCOL";
		obj.Credentials = m_pCredentials;
		_ = (HttpWebResponse)obj.GetResponse();
	}

	public Stream Get(string uri, out long contentSize)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(uri);
		obj.Method = "GET";
		obj.Credentials = m_pCredentials;
		HttpWebResponse httpWebResponse = (HttpWebResponse)obj.GetResponse();
		contentSize = httpWebResponse.ContentLength;
		return httpWebResponse.GetResponseStream();
	}

	public void Delete(string uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(uri);
		obj.Method = "DELETE";
		obj.Credentials = m_pCredentials;
		obj.GetResponse();
	}

	public void Put(string targetUri, Stream stream)
	{
		if (targetUri == null)
		{
			throw new ArgumentNullException("targetUri");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		try
		{
			HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(targetUri);
			obj.Credentials = m_pCredentials;
			obj.UnsafeAuthenticatedConnectionSharing = true;
			obj.PreAuthenticate = true;
			obj.Method = "HEAD";
			((HttpWebResponse)obj.GetResponse()).Close();
		}
		catch
		{
		}
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(targetUri);
		httpWebRequest.Method = "PUT";
		httpWebRequest.ContentType = "application/octet-stream";
		httpWebRequest.Credentials = m_pCredentials;
		httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
		httpWebRequest.PreAuthenticate = true;
		httpWebRequest.AllowWriteStreamBuffering = false;
		httpWebRequest.Timeout = -1;
		if (stream.CanSeek)
		{
			httpWebRequest.ContentLength = stream.Length - stream.Position;
		}
		using (Stream target = httpWebRequest.GetRequestStream())
		{
			Net_Utils.StreamCopy(stream, target, 32000);
		}
		((HttpWebResponse)httpWebRequest.GetResponse()).Close();
	}

	public void Copy(string sourceUri, string targetUri, int depth, bool overwrite)
	{
		if (sourceUri == null)
		{
			throw new ArgumentNullException(sourceUri);
		}
		if (targetUri == null)
		{
			throw new ArgumentNullException(targetUri);
		}
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(sourceUri);
		httpWebRequest.Method = "COPY";
		httpWebRequest.Headers.Add("Destination: " + targetUri);
		httpWebRequest.Headers.Add("Overwrite: " + (overwrite ? "T" : "F"));
		if (depth > -1)
		{
			httpWebRequest.Headers.Add("Depth: " + depth);
		}
		httpWebRequest.Credentials = m_pCredentials;
		httpWebRequest.GetResponse();
	}

	public void Move(string sourceUri, string targetUri, int depth, bool overwrite)
	{
		if (sourceUri == null)
		{
			throw new ArgumentNullException(sourceUri);
		}
		if (targetUri == null)
		{
			throw new ArgumentNullException(targetUri);
		}
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(sourceUri);
		httpWebRequest.Method = "MOVE";
		httpWebRequest.Headers.Add("Destination: " + targetUri);
		httpWebRequest.Headers.Add("Overwrite: " + (overwrite ? "T" : "F"));
		if (depth > -1)
		{
			httpWebRequest.Headers.Add("Depth: " + depth);
		}
		httpWebRequest.Credentials = m_pCredentials;
		httpWebRequest.GetResponse();
	}
}
