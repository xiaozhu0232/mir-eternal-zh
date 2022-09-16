using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.模板类;

public sealed class 游戏称号
{
	public static Dictionary<byte, 游戏称号> 数据表;

	public byte 称号编号;

	public string 称号名字;

	public int 称号战力;

	public int 有效时间;

	public Dictionary<游戏对象属性, int> 称号属性;

	public static void 载入数据()
	{
		数据表 = new Dictionary<byte, 游戏称号>();
		string text = 自定义类.游戏数据目录 + "\\System\\物品数据\\游戏称号\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(游戏称号));
			for (int i = 0; i < array.Length; i++)
			{
				游戏称号 游戏称号2 = array[i] as 游戏称号;
				数据表.Add(游戏称号2.称号编号, 游戏称号2);
			}
		}
	}

	static 游戏称号()
	{
	}
}
