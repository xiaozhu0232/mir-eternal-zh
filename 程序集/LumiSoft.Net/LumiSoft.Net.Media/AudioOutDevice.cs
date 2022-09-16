namespace LumiSoft.Net.Media;

public class AudioOutDevice
{
	private int m_Index;

	private string m_Name = "";

	private int m_Channels = 1;

	public string Name => m_Name;

	public int Channels => m_Channels;

	internal int Index => m_Index;

	internal AudioOutDevice(int index, string name, int channels)
	{
		m_Index = index;
		m_Name = name;
		m_Channels = channels;
	}
}
