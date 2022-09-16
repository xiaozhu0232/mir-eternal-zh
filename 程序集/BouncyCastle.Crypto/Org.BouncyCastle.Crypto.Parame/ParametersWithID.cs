using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class ParametersWithID : ICipherParameters
{
	private readonly ICipherParameters parameters;

	private readonly byte[] id;

	public ICipherParameters Parameters => parameters;

	public ParametersWithID(ICipherParameters parameters, byte[] id)
		: this(parameters, id, 0, id.Length)
	{
	}

	public ParametersWithID(ICipherParameters parameters, byte[] id, int idOff, int idLen)
	{
		this.parameters = parameters;
		this.id = Arrays.CopyOfRange(id, idOff, idOff + idLen);
	}

	public byte[] GetID()
	{
		return id;
	}
}
