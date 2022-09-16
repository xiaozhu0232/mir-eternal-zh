namespace Org.BouncyCastle.Crmf;

public interface IEncryptedValuePadder
{
	byte[] GetPaddedData(byte[] data);

	byte[] GetUnpaddedData(byte[] paddedData);
}
