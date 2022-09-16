using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class NewSessionTicket
{
	protected readonly long mTicketLifetimeHint;

	protected readonly byte[] mTicket;

	public virtual long TicketLifetimeHint => mTicketLifetimeHint;

	public virtual byte[] Ticket => mTicket;

	public NewSessionTicket(long ticketLifetimeHint, byte[] ticket)
	{
		mTicketLifetimeHint = ticketLifetimeHint;
		mTicket = ticket;
	}

	public virtual void Encode(Stream output)
	{
		TlsUtilities.WriteUint32(mTicketLifetimeHint, output);
		TlsUtilities.WriteOpaque16(mTicket, output);
	}

	public static NewSessionTicket Parse(Stream input)
	{
		long ticketLifetimeHint = TlsUtilities.ReadUint32(input);
		byte[] ticket = TlsUtilities.ReadOpaque16(input);
		return new NewSessionTicket(ticketLifetimeHint, ticket);
	}
}
