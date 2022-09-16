using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LumiSoft.Net.IO;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail;

public class Mail_Message : MIME_Message
{
	public DateTime Date
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Date");
			if (first != null)
			{
				try
				{
					return MIME_Utils.ParseRfc2822DateTime(((MIME_h_Unstructured)first).Value);
				}
				catch
				{
					throw new ParseException("Header field 'Date' parsing failed.");
				}
			}
			return DateTime.MinValue;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == DateTime.MinValue)
			{
				base.Header.RemoveAll("Date");
			}
			else if (base.Header.GetFirst("Date") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Date", MIME_Utils.DateTimeToRfc2822(value)));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Date", MIME_Utils.DateTimeToRfc2822(value)));
			}
		}
	}

	public Mail_t_MailboxList From
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("From");
			if (first != null)
			{
				if (!(first is Mail_h_MailboxList))
				{
					throw new ParseException("Header field 'From' parsing failed.");
				}
				return ((Mail_h_MailboxList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("From");
			}
			else if (base.Header.GetFirst("From") == null)
			{
				base.Header.Add(new Mail_h_MailboxList("From", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_MailboxList("From", value));
			}
		}
	}

	public Mail_t_Mailbox Sender
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Sender");
			if (first != null)
			{
				if (!(first is Mail_h_Mailbox))
				{
					throw new ParseException("Header field 'Sender' parsing failed.");
				}
				return ((Mail_h_Mailbox)first).Address;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Sender");
			}
			else if (base.Header.GetFirst("Sender") == null)
			{
				base.Header.Add(new Mail_h_Mailbox("Sender", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_Mailbox("Sender", value));
			}
		}
	}

	public Mail_t_AddressList ReplyTo
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Reply-To");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Reply-To' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Reply-To");
			}
			else if (base.Header.GetFirst("Reply-To") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Reply-To", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Reply-To", value));
			}
		}
	}

	public Mail_t_AddressList To
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("To");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'To' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("To");
			}
			else if (base.Header.GetFirst("To") == null)
			{
				base.Header.Add(new Mail_h_AddressList("To", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("To", value));
			}
		}
	}

	public Mail_t_AddressList Cc
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Cc");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Cc' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Cc");
			}
			else if (base.Header.GetFirst("Cc") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Cc", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Cc", value));
			}
		}
	}

	public Mail_t_AddressList Bcc
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Bcc");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Bcc' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Bcc");
			}
			else if (base.Header.GetFirst("Bcc") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Bcc", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Bcc", value));
			}
		}
	}

	public string MessageID
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Message-ID");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Message-ID");
			}
			else if (base.Header.GetFirst("Message-ID") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Message-ID", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Message-ID", value));
			}
		}
	}

	public string InReplyTo
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("In-Reply-To");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("In-Reply-To");
			}
			else if (base.Header.GetFirst("In-Reply-To") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("In-Reply-To", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("In-Reply-To", value));
			}
		}
	}

	public string References
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("References");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("References");
			}
			else if (base.Header.GetFirst("References") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("References", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("References", value));
			}
		}
	}

	public string Subject
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Subject");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Subject");
			}
			else if (base.Header.GetFirst("Subject") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Subject", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Subject", value));
			}
		}
	}

	public string Comments
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Comments");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Comments");
			}
			else if (base.Header.GetFirst("Comments") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Comments", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Comments", value));
			}
		}
	}

	public string Keywords
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Keywords");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Keywords");
			}
			else if (base.Header.GetFirst("Keywords") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Keywords", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Keywords", value));
			}
		}
	}

	public DateTime ResentDate
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-Date");
			if (first != null)
			{
				try
				{
					return MIME_Utils.ParseRfc2822DateTime(((MIME_h_Unstructured)first).Value);
				}
				catch
				{
					throw new ParseException("Header field 'Resent-Date' parsing failed.");
				}
			}
			return DateTime.MinValue;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == DateTime.MinValue)
			{
				base.Header.RemoveAll("Resent-Date");
			}
			else if (base.Header.GetFirst("Resent-Date") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Resent-Date", MIME_Utils.DateTimeToRfc2822(value)));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Resent-Date", MIME_Utils.DateTimeToRfc2822(value)));
			}
		}
	}

	public Mail_t_MailboxList ResentFrom
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-From");
			if (first != null)
			{
				if (!(first is Mail_h_MailboxList))
				{
					throw new ParseException("Header field 'Resent-From' parsing failed.");
				}
				return ((Mail_h_MailboxList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-From");
			}
			else if (base.Header.GetFirst("Resent-From") == null)
			{
				base.Header.Add(new Mail_h_MailboxList("Resent-From", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_MailboxList("Resent-From", value));
			}
		}
	}

	public Mail_t_Mailbox ResentSender
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-Sender");
			if (first != null)
			{
				if (!(first is Mail_h_Mailbox))
				{
					throw new ParseException("Header field 'Resent-Sender' parsing failed.");
				}
				return ((Mail_h_Mailbox)first).Address;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-Sender");
			}
			else if (base.Header.GetFirst("Resent-Sender") == null)
			{
				base.Header.Add(new Mail_h_Mailbox("Resent-Sender", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_Mailbox("Resent-Sender", value));
			}
		}
	}

	public Mail_t_AddressList ResentTo
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-To");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Resent-To' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-To");
			}
			else if (base.Header.GetFirst("Resent-To") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Resent-To", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Resent-To", value));
			}
		}
	}

	public Mail_t_AddressList ResentCc
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-Cc");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Resent-Cc' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-Cc");
			}
			else if (base.Header.GetFirst("Resent-Cc") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Resent-Cc", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Resent-Cc", value));
			}
		}
	}

	public Mail_t_AddressList ResentBcc
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-Bcc");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Resent-Bcc' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-Bcc");
			}
			else if (base.Header.GetFirst("Resent-Bcc") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Resent-Bcc", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Resent-Bcc", value));
			}
		}
	}

	public Mail_t_AddressList ResentReplyTo
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-Reply-To");
			if (first != null)
			{
				if (!(first is Mail_h_AddressList))
				{
					throw new ParseException("Header field 'Resent-Reply-To' parsing failed.");
				}
				return ((Mail_h_AddressList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-Reply-To");
			}
			else if (base.Header.GetFirst("Resent-Reply-To") == null)
			{
				base.Header.Add(new Mail_h_AddressList("Resent-Reply-To", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_AddressList("Resent-Reply-To", value));
			}
		}
	}

	public string ResentMessageID
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Resent-Message-ID");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Resent-Message-ID");
			}
			else if (base.Header.GetFirst("Resent-Message-ID") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Resent-Message-ID", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Resent-Message-ID", value));
			}
		}
	}

	public Mail_h_ReturnPath ReturnPath
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Return-Path");
			if (first != null)
			{
				if (!(first is Mail_h_ReturnPath))
				{
					throw new ParseException("Header field 'Return-Path' parsing failed.");
				}
				return (Mail_h_ReturnPath)first;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Return-Path");
			}
			else if (base.Header.GetFirst("Return-Path") == null)
			{
				base.Header.Add(value);
			}
			else
			{
				base.Header.ReplaceFirst(value);
			}
		}
	}

	public Mail_h_Received[] Received
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h[] array = base.Header["Received"];
			if (array != null)
			{
				List<Mail_h_Received> list = new List<Mail_h_Received>();
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i] is Mail_h_Received))
					{
						throw new ParseException("Header field 'Received' parsing failed.");
					}
					list.Add((Mail_h_Received)array[i]);
				}
				return list.ToArray();
			}
			return null;
		}
	}

	public Mail_t_MailboxList DispositionNotificationTo
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Disposition-Notification-To");
			if (first != null)
			{
				if (!(first is Mail_h_MailboxList))
				{
					throw new ParseException("Header field 'From' parsing failed.");
				}
				return ((Mail_h_MailboxList)first).Addresses;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Disposition-Notification-To");
			}
			else if (base.Header.GetFirst("Disposition-Notification-To") == null)
			{
				base.Header.Add(new Mail_h_MailboxList("Disposition-Notification-To", value));
			}
			else
			{
				base.Header.ReplaceFirst(new Mail_h_MailboxList("Disposition-Notification-To", value));
			}
		}
	}

	public Mail_h_DispositionNotificationOptions DispositionNotificationOptions
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Disposition-Notification-Options");
			if (first != null)
			{
				if (!(first is Mail_h_DispositionNotificationOptions))
				{
					throw new ParseException("Header field 'Disposition-Notification-Options' parsing failed.");
				}
				return (Mail_h_DispositionNotificationOptions)first;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Disposition-Notification-Options");
			}
			else if (base.Header.GetFirst("Disposition-Notification-Options") == null)
			{
				base.Header.Add(value);
			}
			else
			{
				base.Header.ReplaceFirst(value);
			}
		}
	}

	public string AcceptLanguage
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Accept-Language");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Accept-Language");
			}
			else if (base.Header.GetFirst("Accept-Language") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Accept-Language", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Accept-Language", value));
			}
		}
	}

	public string OriginalMessageID
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Original-Message-ID");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Original-Message-ID");
			}
			else if (base.Header.GetFirst("Original-Message-ID") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Original-Message-ID", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Original-Message-ID", value));
			}
		}
	}

	public string PICSLabel
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("PICS-Label");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("PICS-LabelD");
			}
			else if (base.Header.GetFirst("PICS-Label") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("PICS-Label", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("PICS-Label", value));
			}
		}
	}

	public string ListArchive
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-Archive");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-Archive");
			}
			else if (base.Header.GetFirst("List-Archive") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-Archive", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-Archive", value));
			}
		}
	}

	public string ListHelp
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-Help");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-Help");
			}
			else if (base.Header.GetFirst("List-Help") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-Help", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-Help", value));
			}
		}
	}

	public string ListID
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-ID");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-ID");
			}
			else if (base.Header.GetFirst("List-ID") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-ID", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-ID", value));
			}
		}
	}

	public string ListOwner
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-Owner");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-Owner");
			}
			else if (base.Header.GetFirst("List-Owner") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-Owner", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-Owner", value));
			}
		}
	}

	public string ListPost
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-Post");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-Post");
			}
			else if (base.Header.GetFirst("List-Post") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-Post", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-Post", value));
			}
		}
	}

	public string ListSubscribe
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-Subscribe");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-Subscribe");
			}
			else if (base.Header.GetFirst("List-Subscribe") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-Subscribe", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-Subscribe", value));
			}
		}
	}

	public string ListUnsubscribe
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("List-Unsubscribe");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("List-Unsubscribe");
			}
			else if (base.Header.GetFirst("List-Unsubscribe") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("List-Unsubscribe", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("List-Unsubscribe", value));
			}
		}
	}

	public string MessageContext
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Message-Context");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Message-Context");
			}
			else if (base.Header.GetFirst("Message-Context") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Message-Context", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Message-Context", value));
			}
		}
	}

	public string Importance
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Importance");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Importance");
			}
			else if (base.Header.GetFirst("Importance") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Importance", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Importance", value));
			}
		}
	}

	public string Priority
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_h first = base.Header.GetFirst("Priority");
			if (first != null)
			{
				return ((MIME_h_Unstructured)first).Value;
			}
			return null;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (value == null)
			{
				base.Header.RemoveAll("Priority");
			}
			else if (base.Header.GetFirst("Priority") == null)
			{
				base.Header.Add(new MIME_h_Unstructured("Priority", value));
			}
			else
			{
				base.Header.ReplaceFirst(new MIME_h_Unstructured("Priority", value));
			}
		}
	}

	public MIME_Entity[] Attachments
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return GetAttachments(includeInline: false);
		}
	}

	public Encoding BodyTextEncoding
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_Entity[] allEntities = GetAllEntities(includeEmbbedMessage: false);
			foreach (MIME_Entity mIME_Entity in allEntities)
			{
				if (mIME_Entity.Body.MediaType.ToLower() == MIME_MediaTypes.Text.plain)
				{
					return ((MIME_b_Text)mIME_Entity.Body).GetCharset();
				}
			}
			return null;
		}
	}

	public string BodyText
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_Entity[] allEntities = GetAllEntities(includeEmbbedMessage: false);
			foreach (MIME_Entity mIME_Entity in allEntities)
			{
				if (mIME_Entity.Body.MediaType.ToLower() == MIME_MediaTypes.Text.plain)
				{
					return ((MIME_b_Text)mIME_Entity.Body).Text;
				}
			}
			return null;
		}
	}

	public Encoding BodyHtmlTextEncoding
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_Entity[] allEntities = GetAllEntities(includeEmbbedMessage: false);
			foreach (MIME_Entity mIME_Entity in allEntities)
			{
				if (mIME_Entity.Body.MediaType.ToLower() == MIME_MediaTypes.Text.html)
				{
					return ((MIME_b_Text)mIME_Entity.Body).GetCharset();
				}
			}
			return null;
		}
	}

	public string BodyHtmlText
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			MIME_Entity[] allEntities = GetAllEntities(includeEmbbedMessage: false);
			foreach (MIME_Entity mIME_Entity in allEntities)
			{
				if (mIME_Entity.Body.MediaType.ToLower() == MIME_MediaTypes.Text.html)
				{
					return ((MIME_b_Text)mIME_Entity.Body).Text;
				}
			}
			return null;
		}
	}

	public Mail_Message()
	{
		base.Header.FieldsProvider.HeaderFields.Add("From", typeof(Mail_h_MailboxList));
		base.Header.FieldsProvider.HeaderFields.Add("Sender", typeof(Mail_h_Mailbox));
		base.Header.FieldsProvider.HeaderFields.Add("Reply-To", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("To", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Cc", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Bcc", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Resent-From", typeof(Mail_h_MailboxList));
		base.Header.FieldsProvider.HeaderFields.Add("Resent-Sender", typeof(Mail_h_Mailbox));
		base.Header.FieldsProvider.HeaderFields.Add("Resent-To", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Resent-Cc", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Resent-Bcc", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Resent-Reply-To", typeof(Mail_h_AddressList));
		base.Header.FieldsProvider.HeaderFields.Add("Return-Path", typeof(Mail_h_ReturnPath));
		base.Header.FieldsProvider.HeaderFields.Add("Received", typeof(Mail_h_Received));
		base.Header.FieldsProvider.HeaderFields.Add("Disposition-Notification-To", typeof(Mail_h_MailboxList));
		base.Header.FieldsProvider.HeaderFields.Add("Disposition-Notification-Options", typeof(Mail_h_DispositionNotificationOptions));
	}

	public static Mail_Message Create(Mail_t_Mailbox from, Mail_t_Address[] to, Mail_t_Address[] cc, Mail_t_Address[] bcc, string subject, string text, string html, Mail_t_Attachment[] attachments)
	{
		Mail_Message mail_Message = new Mail_Message();
		mail_Message.MimeVersion = "1.0";
		mail_Message.MessageID = MIME_Utils.CreateMessageID();
		mail_Message.Date = DateTime.Now;
		if (from != null)
		{
			mail_Message.From = new Mail_t_MailboxList();
			mail_Message.From.Add(from);
		}
		if (to != null)
		{
			mail_Message.To = new Mail_t_AddressList();
			foreach (Mail_t_Address value in to)
			{
				mail_Message.To.Add(value);
			}
		}
		mail_Message.Subject = subject;
		if (attachments == null || attachments.Length == 0)
		{
			if (string.IsNullOrEmpty(html))
			{
				MIME_b_Text mIME_b_Text = (MIME_b_Text)(mail_Message.Body = new MIME_b_Text(MIME_MediaTypes.Text.plain));
				mIME_b_Text.SetText(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text);
			}
			else
			{
				MIME_b_MultipartAlternative mIME_b_MultipartAlternative = (MIME_b_MultipartAlternative)(mail_Message.Body = new MIME_b_MultipartAlternative());
				mIME_b_MultipartAlternative.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
				mIME_b_MultipartAlternative.BodyParts.Add(MIME_Entity.CreateEntity_Text_Html(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, html));
			}
		}
		else if (string.IsNullOrEmpty(html))
		{
			MIME_b_MultipartMixed mIME_b_MultipartMixed = (MIME_b_MultipartMixed)(mail_Message.Body = new MIME_b_MultipartMixed());
			mIME_b_MultipartMixed.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
			Mail_t_Attachment[] array = attachments;
			foreach (Mail_t_Attachment mail_t_Attachment in array)
			{
				try
				{
					mIME_b_MultipartMixed.BodyParts.Add(MIME_Entity.CreateEntity_Attachment(mail_t_Attachment.Name, mail_t_Attachment.GetStream()));
				}
				finally
				{
					mail_t_Attachment.CloseStream();
				}
			}
		}
		else
		{
			MIME_b_MultipartMixed mIME_b_MultipartMixed2 = (MIME_b_MultipartMixed)(mail_Message.Body = new MIME_b_MultipartMixed());
			MIME_Entity mIME_Entity = new MIME_Entity();
			MIME_b_MultipartAlternative mIME_b_MultipartAlternative2 = (MIME_b_MultipartAlternative)(mIME_Entity.Body = new MIME_b_MultipartAlternative());
			mIME_b_MultipartMixed2.BodyParts.Add(mIME_Entity);
			mIME_b_MultipartAlternative2.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
			mIME_b_MultipartAlternative2.BodyParts.Add(MIME_Entity.CreateEntity_Text_Html(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, html));
			Mail_t_Attachment[] array = attachments;
			foreach (Mail_t_Attachment mail_t_Attachment2 in array)
			{
				try
				{
					mIME_b_MultipartMixed2.BodyParts.Add(MIME_Entity.CreateEntity_Attachment(mail_t_Attachment2.Name, mail_t_Attachment2.GetStream()));
				}
				finally
				{
					mail_t_Attachment2.CloseStream();
				}
			}
		}
		return mail_Message;
	}

	public static Mail_Message Create_MultipartSigned(X509Certificate2 signerCert, Mail_t_Mailbox from, Mail_t_Address[] to, Mail_t_Address[] cc, Mail_t_Address[] bcc, string subject, string text, string html, Mail_t_Attachment[] attachments)
	{
		if (signerCert == null)
		{
			throw new ArgumentNullException("signerCert");
		}
		Mail_Message mail_Message = new Mail_Message();
		mail_Message.MimeVersion = "1.0";
		mail_Message.MessageID = MIME_Utils.CreateMessageID();
		mail_Message.Date = DateTime.Now;
		if (from != null)
		{
			mail_Message.From = new Mail_t_MailboxList();
			mail_Message.From.Add(from);
		}
		if (to != null)
		{
			mail_Message.To = new Mail_t_AddressList();
			foreach (Mail_t_Address value in to)
			{
				mail_Message.To.Add(value);
			}
		}
		mail_Message.Subject = subject;
		if (attachments == null || attachments.Length == 0)
		{
			if (string.IsNullOrEmpty(html))
			{
				MIME_b_MultipartSigned mIME_b_MultipartSigned = (MIME_b_MultipartSigned)(mail_Message.Body = new MIME_b_MultipartSigned());
				mIME_b_MultipartSigned.SetCertificate(signerCert);
				mIME_b_MultipartSigned.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
			}
			else
			{
				MIME_b_MultipartSigned mIME_b_MultipartSigned2 = (MIME_b_MultipartSigned)(mail_Message.Body = new MIME_b_MultipartSigned());
				mIME_b_MultipartSigned2.SetCertificate(signerCert);
				MIME_Entity mIME_Entity = new MIME_Entity();
				MIME_b_MultipartAlternative mIME_b_MultipartAlternative = (MIME_b_MultipartAlternative)(mIME_Entity.Body = new MIME_b_MultipartAlternative());
				mIME_b_MultipartSigned2.BodyParts.Add(mIME_Entity);
				mIME_b_MultipartAlternative.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
				mIME_b_MultipartAlternative.BodyParts.Add(MIME_Entity.CreateEntity_Text_Html(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, html));
			}
		}
		else if (string.IsNullOrEmpty(html))
		{
			MIME_b_MultipartSigned mIME_b_MultipartSigned3 = (MIME_b_MultipartSigned)(mail_Message.Body = new MIME_b_MultipartSigned());
			mIME_b_MultipartSigned3.SetCertificate(signerCert);
			MIME_Entity mIME_Entity2 = new MIME_Entity();
			MIME_b_MultipartMixed mIME_b_MultipartMixed = (MIME_b_MultipartMixed)(mIME_Entity2.Body = new MIME_b_MultipartMixed());
			mIME_b_MultipartSigned3.BodyParts.Add(mIME_Entity2);
			mIME_b_MultipartMixed.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
			Mail_t_Attachment[] array = attachments;
			foreach (Mail_t_Attachment mail_t_Attachment in array)
			{
				try
				{
					mIME_b_MultipartMixed.BodyParts.Add(MIME_Entity.CreateEntity_Attachment(mail_t_Attachment.Name, mail_t_Attachment.GetStream()));
				}
				finally
				{
					mail_t_Attachment.CloseStream();
				}
			}
		}
		else
		{
			MIME_b_MultipartSigned mIME_b_MultipartSigned4 = (MIME_b_MultipartSigned)(mail_Message.Body = new MIME_b_MultipartSigned());
			mIME_b_MultipartSigned4.SetCertificate(signerCert);
			MIME_Entity mIME_Entity3 = new MIME_Entity();
			MIME_b_MultipartMixed mIME_b_MultipartMixed2 = (MIME_b_MultipartMixed)(mIME_Entity3.Body = new MIME_b_MultipartMixed());
			mIME_b_MultipartSigned4.BodyParts.Add(mIME_Entity3);
			MIME_Entity mIME_Entity4 = new MIME_Entity();
			MIME_b_MultipartAlternative mIME_b_MultipartAlternative2 = (MIME_b_MultipartAlternative)(mIME_Entity4.Body = new MIME_b_MultipartAlternative());
			mIME_b_MultipartMixed2.BodyParts.Add(mIME_Entity4);
			mIME_b_MultipartAlternative2.BodyParts.Add(MIME_Entity.CreateEntity_Text_Plain(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, text));
			mIME_b_MultipartAlternative2.BodyParts.Add(MIME_Entity.CreateEntity_Text_Html(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, html));
			Mail_t_Attachment[] array = attachments;
			foreach (Mail_t_Attachment mail_t_Attachment2 in array)
			{
				try
				{
					mIME_b_MultipartMixed2.BodyParts.Add(MIME_Entity.CreateEntity_Attachment(mail_t_Attachment2.Name, mail_t_Attachment2.GetStream()));
				}
				finally
				{
					mail_t_Attachment2.CloseStream();
				}
			}
		}
		return mail_Message;
	}

	public static Mail_Message ParseFromByte(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return ParseFromStream(new MemoryStream(data));
	}

	public static Mail_Message ParseFromByte(byte[] data, Encoding headerEncoding)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (headerEncoding == null)
		{
			throw new ArgumentNullException("headerEncoding");
		}
		return ParseFromStream(new MemoryStream(data), headerEncoding);
	}

	public new static Mail_Message ParseFromFile(string file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file == "")
		{
			throw new ArgumentException("Argument 'file' value must be specified.");
		}
		using FileStream stream = File.OpenRead(file);
		return ParseFromStream(stream);
	}

	public new static Mail_Message ParseFromFile(string file, Encoding headerEncoding)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file == "")
		{
			throw new ArgumentException("Argument 'file' value must be specified.");
		}
		if (headerEncoding == null)
		{
			throw new ArgumentNullException("headerEncoding");
		}
		using FileStream stream = File.OpenRead(file);
		return ParseFromStream(stream, headerEncoding);
	}

	public new static Mail_Message ParseFromStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return ParseFromStream(stream, Encoding.UTF8);
	}

	public new static Mail_Message ParseFromStream(Stream stream, Encoding headerEncoding)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (headerEncoding == null)
		{
			throw new ArgumentNullException("headerEncoding");
		}
		Mail_Message mail_Message = new Mail_Message();
		mail_Message.Parse(new SmartStream(stream, owner: false), headerEncoding, new MIME_h_ContentType("text/plain"));
		return mail_Message;
	}

	public Mail_Message Clone()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		MemoryStreamEx memoryStreamEx = new MemoryStreamEx(64000);
		ToStream(memoryStreamEx, null, null);
		memoryStreamEx.Position = 0L;
		return ParseFromStream(memoryStreamEx);
	}

	public MIME_Entity[] GetAttachments(bool includeInline)
	{
		return GetAttachments(includeInline, includeEmbbedMessage: true);
	}

	public MIME_Entity[] GetAttachments(bool includeInline, bool includeEmbbedMessage)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		List<MIME_Entity> list = new List<MIME_Entity>();
		MIME_Entity[] allEntities = GetAllEntities(includeEmbbedMessage);
		foreach (MIME_Entity mIME_Entity in allEntities)
		{
			MIME_h_ContentType mIME_h_ContentType = null;
			try
			{
				mIME_h_ContentType = mIME_Entity.ContentType;
			}
			catch
			{
			}
			MIME_h_ContentDisposition mIME_h_ContentDisposition = null;
			try
			{
				mIME_h_ContentDisposition = mIME_Entity.ContentDisposition;
			}
			catch
			{
			}
			if (mIME_h_ContentDisposition != null && string.Equals(mIME_h_ContentDisposition.DispositionType, "attachment", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add(mIME_Entity);
			}
			else if (mIME_h_ContentDisposition != null && string.Equals(mIME_h_ContentDisposition.DispositionType, "inline", StringComparison.InvariantCultureIgnoreCase))
			{
				if (includeInline)
				{
					list.Add(mIME_Entity);
				}
			}
			else if (mIME_h_ContentType != null && mIME_h_ContentType.Type.ToLower() == "application")
			{
				list.Add(mIME_Entity);
			}
			else if (mIME_h_ContentType != null && mIME_h_ContentType.Type.ToLower() == "image")
			{
				list.Add(mIME_Entity);
			}
			else if (mIME_h_ContentType != null && mIME_h_ContentType.Type.ToLower() == "video")
			{
				list.Add(mIME_Entity);
			}
			else if (mIME_h_ContentType != null && mIME_h_ContentType.Type.ToLower() == "audio")
			{
				list.Add(mIME_Entity);
			}
			else if (mIME_h_ContentType != null && mIME_h_ContentType.Type.ToLower() == "message")
			{
				list.Add(mIME_Entity);
			}
		}
		return list.ToArray();
	}
}
