namespace LumiSoft.Net.Media;

public class AudioInDevice
{
	private int m_Index;

	private string m_Name = "";

	private int m_Channels = 1;

	public string Name => m_Name;

	public int Channels => m_Channels;

	internal int Index => m_Index;

	internal AudioInDevice(int index, string name, int channels)
	{
		m_Index = index;
		m_Name = name;
		m_Channels = channels;
	}
}
