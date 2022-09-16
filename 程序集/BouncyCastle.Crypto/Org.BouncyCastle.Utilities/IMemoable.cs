namespace Org.BouncyCastle.Utilities;

public interface IMemoable
{
	IMemoable Copy();

	void Reset(IMemoable other);
}
