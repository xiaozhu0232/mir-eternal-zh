using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace 游戏服务器.模板类;

public sealed class 对话数据
{
	public static Dictionary<int, string> 数据表;

	public static Dictionary<int, byte[]> 字节表;

	public int 对话编号;

	public string 对话内容;

	public static byte[] 字节数据(int 对话编号)
	{
		if (!字节表.TryGetValue(对话编号, out var value))
		{
			if (!数据表.TryGetValue(对话编号, out var value2))
			{
				return new byte[0];
			}
			return 字节表[对话编号] = Encoding.UTF8.GetBytes(value2 + "\0");
		}
		return value;
	}

	public static byte[] 合并数据(int 对话编号, string 内容)
	{
		if (!字节表.TryGetValue(对话编号, out var value))
		{
			if (!数据表.TryGetValue(对话编号, out var value2))
			{
				return new byte[0];
			}
			byte[] bytes = Encoding.UTF8.GetBytes(内容);
			byte[] array = (字节表[对话编号] = Encoding.UTF8.GetBytes(value2 + "\0"));
			byte[] second = array;
			return bytes.Concat(second).ToArray();
		}
		return Encoding.UTF8.GetBytes(内容).Concat(value).ToArray();
	}

	public static void 载入数据()
	{
		数据表 = new Dictionary<int, string>();
		字节表 = new Dictionary<int, byte[]>();
		string text = 自定义类.游戏数据目录 + "\\System\\Npc数据\\对话数据\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(对话数据));
			foreach (object obj in array)
			{
				数据表.Add(((对话数据)obj).对话编号, ((对话数据)obj).对话内容);
			}
		}
	}

	static 对话数据()
	{
	}
}
