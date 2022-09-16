namespace LumiSoft.Net.Media;

public class AudioFormat
{
	private int m_SamplesPerSecond;

	private int m_BitsPerSample;

	private int m_Channels;

	public int SamplesPerSecond => m_SamplesPerSecond;

	public int BitsPerSample => m_BitsPerSample;

	public int Channels => m_Channels;

	public AudioFormat(int samplesPerSecond, int bitsPerSample, int channels)
	{
		m_SamplesPerSecond = samplesPerSecond;
		m_BitsPerSample = bitsPerSample;
		m_Channels = channels;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is AudioFormat))
		{
			return false;
		}
		AudioFormat audioFormat = (AudioFormat)obj;
		if (audioFormat.SamplesPerSecond != SamplesPerSecond)
		{
			return false;
		}
		if (audioFormat.BitsPerSample != BitsPerSample)
		{
			return false;
		}
		if (audioFormat.Channels != Channels)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
