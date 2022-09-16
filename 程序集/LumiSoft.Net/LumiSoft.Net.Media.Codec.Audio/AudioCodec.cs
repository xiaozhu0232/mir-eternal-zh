namespace LumiSoft.Net.Media.Codec.Audio;

public abstract class AudioCodec : Codec
{
	public abstract AudioFormat AudioFormat { get; }

	public abstract AudioFormat CompressedAudioFormat { get; }
}
