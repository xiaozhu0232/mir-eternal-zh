using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LumiSoft.Net.IMAP.Client;
using LumiSoft.Net.IMAP.Server;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.IMAP;

public class IMAP_r_u_Fetch : IMAP_r_u
{
	private int m_MsgSeqNo;

	private List<IMAP_t_Fetch_r_i> m_pDataItems;

	public int SeqNo => m_MsgSeqNo;

	public IMAP_t_Fetch_r_i[] DataItems => m_pDataItems.ToArray();

	public IMAP_t_Fetch_r_i_Body[] Body
	{
		get
		{
			List<IMAP_t_Fetch_r_i_Body> list = new List<IMAP_t_Fetch_r_i_Body>();
			foreach (IMAP_t_Fetch_r_i pDataItem in m_pDataItems)
			{
				if (pDataItem is IMAP_t_Fetch_r_i_Body)
				{
					list.Add((IMAP_t_Fetch_r_i_Body)pDataItem);
				}
			}
			return list.ToArray();
		}
	}

	public IMAP_t_Fetch_r_i_BodyStructure BodyStructure => (IMAP_t_Fetch_r_i_BodyStructure)FilterDataItem(typeof(IMAP_t_Fetch_r_i_BodyStructure));

	public IMAP_t_Fetch_r_i_Envelope Envelope => (IMAP_t_Fetch_r_i_Envelope)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Envelope));

	public IMAP_t_Fetch_r_i_Flags Flags => (IMAP_t_Fetch_r_i_Flags)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Flags));

	public IMAP_t_Fetch_r_i_InternalDate InternalDate => (IMAP_t_Fetch_r_i_InternalDate)FilterDataItem(typeof(IMAP_t_Fetch_r_i_InternalDate));

	public IMAP_t_Fetch_r_i_Rfc822 Rfc822 => (IMAP_t_Fetch_r_i_Rfc822)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Rfc822));

	public IMAP_t_Fetch_r_i_Rfc822Header Rfc822Header => (IMAP_t_Fetch_r_i_Rfc822Header)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Rfc822Header));

	public IMAP_t_Fetch_r_i_Rfc822Size Rfc822Size => (IMAP_t_Fetch_r_i_Rfc822Size)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Rfc822Size));

	public IMAP_t_Fetch_r_i_Rfc822Text Rfc822Text => (IMAP_t_Fetch_r_i_Rfc822Text)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Rfc822Text));

	public IMAP_t_Fetch_r_i_Uid UID => (IMAP_t_Fetch_r_i_Uid)FilterDataItem(typeof(IMAP_t_Fetch_r_i_Uid));

	public IMAP_t_Fetch_r_i_X_GM_MSGID X_GM_MSGID => (IMAP_t_Fetch_r_i_X_GM_MSGID)FilterDataItem(typeof(IMAP_t_Fetch_r_i_X_GM_MSGID));

	public IMAP_t_Fetch_r_i_X_GM_THRID X_GM_THRID => (IMAP_t_Fetch_r_i_X_GM_THRID)FilterDataItem(typeof(IMAP_t_Fetch_r_i_X_GM_THRID));

	public IMAP_r_u_Fetch(int msgSeqNo, IMAP_t_Fetch_r_i[] dataItems)
	{
		if (msgSeqNo < 1)
		{
			throw new ArgumentException("Argument 'msgSeqNo' value must be >= 1.", "msgSeqNo");
		}
		if (dataItems == null)
		{
			throw new ArgumentNullException("dataItems");
		}
		m_MsgSeqNo = msgSeqNo;
		m_pDataItems = new List<IMAP_t_Fetch_r_i>();
		m_pDataItems.AddRange(dataItems);
	}

	internal IMAP_r_u_Fetch(int msgSeqNo)
	{
		if (msgSeqNo < 1)
		{
			throw new ArgumentException("Argument 'msgSeqNo' value must be >= 1.", "msgSeqNo");
		}
		m_MsgSeqNo = msgSeqNo;
		m_pDataItems = new List<IMAP_t_Fetch_r_i>();
	}

	internal void ParseAsync(IMAP_Client imap, string line, EventHandler<EventArgs<Exception>> callback)
	{
		if (imap == null)
		{
			throw new ArgumentNullException("imap");
		}
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		StringReader stringReader = new StringReader(line);
		stringReader.ReadWord();
		m_MsgSeqNo = Convert.ToInt32(stringReader.ReadWord());
		stringReader.ReadWord();
		stringReader.ReadToFirstChar();
		if (stringReader.StartsWith("("))
		{
			stringReader.ReadSpecifiedLength(1);
		}
		ParseDataItems(imap, stringReader, callback);
	}

	protected override bool ToStreamAsync(IMAP_Session session, Stream stream, IMAP_Mailbox_Encoding mailboxEncoding, EventHandler<EventArgs<Exception>> completedAsyncCallback)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("* " + m_MsgSeqNo + " FETCH (");
		for (int i = 0; i < m_pDataItems.Count; i++)
		{
			IMAP_t_Fetch_r_i iMAP_t_Fetch_r_i = m_pDataItems[i];
			if (i > 0)
			{
				stringBuilder.Append(" ");
			}
			if (iMAP_t_Fetch_r_i is IMAP_t_Fetch_r_i_Flags)
			{
				stringBuilder.Append("FLAGS (" + ((IMAP_t_Fetch_r_i_Flags)iMAP_t_Fetch_r_i).Flags.ToString() + ")");
				continue;
			}
			if (iMAP_t_Fetch_r_i is IMAP_t_Fetch_r_i_Uid)
			{
				stringBuilder.Append("UID " + ((IMAP_t_Fetch_r_i_Uid)iMAP_t_Fetch_r_i).UID);
				continue;
			}
			throw new NotImplementedException("Fetch response data-item '" + iMAP_t_Fetch_r_i.ToString() + "' not implemented.");
		}
		stringBuilder.Append(")\r\n");
		string text = stringBuilder.ToString();
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		session?.LogAddWrite(bytes.Length, text.TrimEnd());
		IAsyncResult asyncResult = stream.BeginWrite(bytes, 0, bytes.Length, delegate(IAsyncResult r)
		{
			if (r.CompletedSynchronously)
			{
				return;
			}
			try
			{
				stream.EndWrite(r);
				if (completedAsyncCallback != null)
				{
					completedAsyncCallback(this, new EventArgs<Exception>(null));
				}
			}
			catch (Exception value)
			{
				if (completedAsyncCallback != null)
				{
					completedAsyncCallback(this, new EventArgs<Exception>(value));
				}
			}
		}, null);
		if (asyncResult.CompletedSynchronously)
		{
			stream.EndWrite(asyncResult);
			return false;
		}
		return true;
	}

	private void ParseDataItems(IMAP_Client imap, StringReader r, EventHandler<EventArgs<Exception>> callback)
	{
		if (imap == null)
		{
			throw new ArgumentNullException("imap");
		}
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		while (true)
		{
			r.ReadToFirstChar();
			if (r.StartsWith("BODY[", case_sensitive: false))
			{
				r.ReadWord();
				string section = r.ReadParenthesized();
				int offset = -1;
				if (r.StartsWith("<"))
				{
					offset = Convert.ToInt32(r.ReadParenthesized().Split(' ')[0]);
				}
				IMAP_t_Fetch_r_i_Body iMAP_t_Fetch_r_i_Body = new IMAP_t_Fetch_r_i_Body(section, offset, new MemoryStreamEx(32000));
				m_pDataItems.Add(iMAP_t_Fetch_r_i_Body);
				IMAP_Client_e_FetchGetStoreStream iMAP_Client_e_FetchGetStoreStream = new IMAP_Client_e_FetchGetStoreStream(this, iMAP_t_Fetch_r_i_Body);
				imap.OnFetchGetStoreStream(iMAP_Client_e_FetchGetStoreStream);
				if (iMAP_Client_e_FetchGetStoreStream.Stream != null)
				{
					iMAP_t_Fetch_r_i_Body.Stream.Dispose();
					iMAP_t_Fetch_r_i_Body.SetStream(iMAP_Client_e_FetchGetStoreStream.Stream);
				}
				if (ReadData(imap, r, callback, iMAP_t_Fetch_r_i_Body.Stream))
				{
					return;
				}
			}
			else if (r.StartsWith("BODY ", case_sensitive: false))
			{
				string text = null;
				while (true)
				{
					StringReader stringReader = new StringReader(r.SourceString);
					stringReader.ReadWord();
					stringReader.ReadToFirstChar();
					try
					{
						text = stringReader.ReadParenthesized();
						r = stringReader;
					}
					catch
					{
						if (ReadStringLiteral(imap, r, callback))
						{
							return;
						}
						continue;
					}
					break;
				}
				m_pDataItems.Add(IMAP_t_Fetch_r_i_BodyStructure.Parse(new StringReader(text)));
			}
			else if (r.StartsWith("BODYSTRUCTURE", case_sensitive: false))
			{
				string text2 = null;
				while (true)
				{
					StringReader stringReader2 = new StringReader(r.SourceString);
					stringReader2.ReadWord();
					stringReader2.ReadToFirstChar();
					try
					{
						text2 = stringReader2.ReadParenthesized();
						r = stringReader2;
					}
					catch
					{
						if (ReadStringLiteral(imap, r, callback))
						{
							return;
						}
						continue;
					}
					break;
				}
				m_pDataItems.Add(IMAP_t_Fetch_r_i_BodyStructure.Parse(new StringReader(text2)));
			}
			else if (r.StartsWith("ENVELOPE", case_sensitive: false))
			{
				string text3 = null;
				while (true)
				{
					StringReader stringReader3 = new StringReader(r.SourceString);
					stringReader3.ReadWord();
					stringReader3.ReadToFirstChar();
					try
					{
						text3 = stringReader3.ReadParenthesized();
						r = stringReader3;
					}
					catch
					{
						if (ReadStringLiteral(imap, r, callback))
						{
							return;
						}
						continue;
					}
					break;
				}
				m_pDataItems.Add(IMAP_t_Fetch_r_i_Envelope.Parse(new StringReader(text3)));
			}
			else if (r.StartsWith("FLAGS", case_sensitive: false))
			{
				r.ReadWord();
				m_pDataItems.Add(new IMAP_t_Fetch_r_i_Flags(IMAP_t_MsgFlags.Parse(r.ReadParenthesized())));
			}
			else if (r.StartsWith("INTERNALDATE", case_sensitive: false))
			{
				r.ReadWord();
				m_pDataItems.Add(new IMAP_t_Fetch_r_i_InternalDate(IMAP_Utils.ParseDate(r.ReadWord())));
			}
			else if (r.StartsWith("RFC822 ", case_sensitive: false))
			{
				r.ReadWord();
				r.ReadToFirstChar();
				IMAP_t_Fetch_r_i_Rfc822 iMAP_t_Fetch_r_i_Rfc = new IMAP_t_Fetch_r_i_Rfc822(new MemoryStreamEx(32000));
				m_pDataItems.Add(iMAP_t_Fetch_r_i_Rfc);
				IMAP_Client_e_FetchGetStoreStream iMAP_Client_e_FetchGetStoreStream2 = new IMAP_Client_e_FetchGetStoreStream(this, iMAP_t_Fetch_r_i_Rfc);
				imap.OnFetchGetStoreStream(iMAP_Client_e_FetchGetStoreStream2);
				if (iMAP_Client_e_FetchGetStoreStream2.Stream != null)
				{
					iMAP_t_Fetch_r_i_Rfc.Stream.Dispose();
					iMAP_t_Fetch_r_i_Rfc.SetStream(iMAP_Client_e_FetchGetStoreStream2.Stream);
				}
				if (ReadData(imap, r, callback, iMAP_t_Fetch_r_i_Rfc.Stream))
				{
					return;
				}
			}
			else if (r.StartsWith("RFC822.HEADER", case_sensitive: false))
			{
				r.ReadWord();
				r.ReadToFirstChar();
				IMAP_t_Fetch_r_i_Rfc822Header iMAP_t_Fetch_r_i_Rfc822Header = new IMAP_t_Fetch_r_i_Rfc822Header(new MemoryStreamEx(32000));
				m_pDataItems.Add(iMAP_t_Fetch_r_i_Rfc822Header);
				IMAP_Client_e_FetchGetStoreStream iMAP_Client_e_FetchGetStoreStream3 = new IMAP_Client_e_FetchGetStoreStream(this, iMAP_t_Fetch_r_i_Rfc822Header);
				imap.OnFetchGetStoreStream(iMAP_Client_e_FetchGetStoreStream3);
				if (iMAP_Client_e_FetchGetStoreStream3.Stream != null)
				{
					iMAP_t_Fetch_r_i_Rfc822Header.Stream.Dispose();
					iMAP_t_Fetch_r_i_Rfc822Header.SetStream(iMAP_Client_e_FetchGetStoreStream3.Stream);
				}
				if (ReadData(imap, r, callback, iMAP_t_Fetch_r_i_Rfc822Header.Stream))
				{
					return;
				}
			}
			else if (r.StartsWith("RFC822.SIZE", case_sensitive: false))
			{
				r.ReadWord();
				m_pDataItems.Add(new IMAP_t_Fetch_r_i_Rfc822Size(Convert.ToInt32(r.ReadWord())));
			}
			else if (r.StartsWith("RFC822.TEXT", case_sensitive: false))
			{
				r.ReadWord();
				r.ReadToFirstChar();
				IMAP_t_Fetch_r_i_Rfc822Text iMAP_t_Fetch_r_i_Rfc822Text = new IMAP_t_Fetch_r_i_Rfc822Text(new MemoryStreamEx(32000));
				m_pDataItems.Add(iMAP_t_Fetch_r_i_Rfc822Text);
				IMAP_Client_e_FetchGetStoreStream iMAP_Client_e_FetchGetStoreStream4 = new IMAP_Client_e_FetchGetStoreStream(this, iMAP_t_Fetch_r_i_Rfc822Text);
				imap.OnFetchGetStoreStream(iMAP_Client_e_FetchGetStoreStream4);
				if (iMAP_Client_e_FetchGetStoreStream4.Stream != null)
				{
					iMAP_t_Fetch_r_i_Rfc822Text.Stream.Dispose();
					iMAP_t_Fetch_r_i_Rfc822Text.SetStream(iMAP_Client_e_FetchGetStoreStream4.Stream);
				}
				if (ReadData(imap, r, callback, iMAP_t_Fetch_r_i_Rfc822Text.Stream))
				{
					return;
				}
			}
			else if (r.StartsWith("UID", case_sensitive: false))
			{
				r.ReadWord();
				m_pDataItems.Add(new IMAP_t_Fetch_r_i_Uid(Convert.ToInt64(r.ReadWord())));
			}
			else if (r.StartsWith("X-GM-MSGID", case_sensitive: false))
			{
				r.ReadWord();
				m_pDataItems.Add(new IMAP_t_Fetch_r_i_X_GM_MSGID(Convert.ToUInt64(r.ReadWord())));
			}
			else
			{
				if (!r.StartsWith("X-GM-THRID", case_sensitive: false))
				{
					break;
				}
				r.ReadWord();
				m_pDataItems.Add(new IMAP_t_Fetch_r_i_X_GM_THRID(Convert.ToUInt64(r.ReadWord())));
			}
		}
		if (!r.StartsWith(")", case_sensitive: false))
		{
			throw new ParseException("Not supported FETCH data-item '" + r.ReadToEnd() + "'.");
		}
		callback(this, new EventArgs<Exception>(null));
	}

	private bool ReadStringLiteral(IMAP_Client imap, StringReader r, EventHandler<EventArgs<Exception>> callback)
	{
		if (imap == null)
		{
			throw new ArgumentNullException("imap");
		}
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (r.SourceString.EndsWith("}") && r.SourceString.IndexOf("{") > -1)
		{
			MemoryStream stream = new MemoryStream();
			string value = r.SourceString.Substring(r.SourceString.LastIndexOf("{") + 1, r.SourceString.Length - r.SourceString.LastIndexOf("{") - 2);
			r.RemoveFromEnd(r.SourceString.Length - r.SourceString.LastIndexOf('{'));
			IMAP_Client.ReadStringLiteralAsyncOP op = new IMAP_Client.ReadStringLiteralAsyncOP(stream, Convert.ToInt32(value));
			op.CompletedAsync += delegate
			{
				try
				{
					if (op.Error != null)
					{
						callback(this, new EventArgs<Exception>(op.Error));
					}
					else
					{
						r.AppendString(TextUtils.QuoteString(Encoding.UTF8.GetString(stream.ToArray())));
						if (!ReadNextFetchLine(imap, r, callback))
						{
							ParseDataItems(imap, r, callback);
						}
					}
				}
				catch (Exception value2)
				{
					callback(this, new EventArgs<Exception>(value2));
				}
				finally
				{
					op.Dispose();
				}
			};
			if (!imap.ReadStringLiteralAsync(op))
			{
				try
				{
					if (op.Error != null)
					{
						callback(this, new EventArgs<Exception>(op.Error));
						return true;
					}
					r.AppendString(TextUtils.QuoteString(Encoding.UTF8.GetString(stream.ToArray())));
					return ReadNextFetchLine(imap, r, callback);
				}
				finally
				{
					op.Dispose();
				}
			}
			return true;
		}
		throw new ParseException("No string-literal available '" + r.SourceString + "'.");
	}

	private bool ReadData(IMAP_Client imap, StringReader r, EventHandler<EventArgs<Exception>> callback, Stream stream)
	{
		if (imap == null)
		{
			throw new ArgumentNullException("imap");
		}
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		r.ReadToFirstChar();
		if (r.StartsWith("NIL", case_sensitive: false))
		{
			r.ReadWord();
			return false;
		}
		if (r.StartsWith("{", case_sensitive: false))
		{
			IMAP_Client.ReadStringLiteralAsyncOP op = new IMAP_Client.ReadStringLiteralAsyncOP(stream, Convert.ToInt32(r.ReadParenthesized()));
			op.CompletedAsync += delegate
			{
				try
				{
					if (op.Error != null)
					{
						callback(this, new EventArgs<Exception>(op.Error));
					}
					else if (!ReadNextFetchLine(imap, r, callback))
					{
						ParseDataItems(imap, r, callback);
					}
				}
				catch (Exception value)
				{
					callback(this, new EventArgs<Exception>(value));
				}
				finally
				{
					op.Dispose();
				}
			};
			if (!imap.ReadStringLiteralAsync(op))
			{
				try
				{
					if (op.Error != null)
					{
						callback(this, new EventArgs<Exception>(op.Error));
						return true;
					}
					if (!ReadNextFetchLine(imap, r, callback))
					{
						return false;
					}
					return true;
				}
				finally
				{
					op.Dispose();
				}
			}
			return true;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(r.ReadWord());
		stream.Write(bytes, 0, bytes.Length);
		return false;
	}

	private bool ReadNextFetchLine(IMAP_Client imap, StringReader r, EventHandler<EventArgs<Exception>> callback)
	{
		if (imap == null)
		{
			throw new ArgumentNullException("imap");
		}
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		SmartStream.ReadLineAsyncOP readLineOP = new SmartStream.ReadLineAsyncOP(new byte[64000], SizeExceededAction.JunkAndThrowException);
		readLineOP.CompletedAsync += delegate
		{
			try
			{
				if (readLineOP.Error != null)
				{
					callback(this, new EventArgs<Exception>(readLineOP.Error));
				}
				else
				{
					imap.LogAddRead(readLineOP.BytesInBuffer, readLineOP.LineUtf8);
					r.AppendString(readLineOP.LineUtf8);
					ParseDataItems(imap, r, callback);
				}
			}
			catch (Exception value)
			{
				callback(this, new EventArgs<Exception>(value));
			}
			finally
			{
				readLineOP.Dispose();
			}
		};
		if (imap.TcpStream.ReadLine(readLineOP, async: true))
		{
			try
			{
				if (readLineOP.Error != null)
				{
					callback(this, new EventArgs<Exception>(readLineOP.Error));
					return true;
				}
				imap.LogAddRead(readLineOP.BytesInBuffer, readLineOP.LineUtf8);
				r.AppendString(readLineOP.LineUtf8);
				return false;
			}
			finally
			{
				readLineOP.Dispose();
			}
		}
		return true;
	}

	private IMAP_t_Fetch_r_i FilterDataItem(Type dataItem)
	{
		if (dataItem == null)
		{
			throw new ArgumentNullException("dataItem");
		}
		foreach (IMAP_t_Fetch_r_i pDataItem in m_pDataItems)
		{
			if (pDataItem.GetType() == dataItem)
			{
				return pDataItem;
			}
		}
		return null;
	}
}
