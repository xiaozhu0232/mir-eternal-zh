using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.模板类;

public sealed class 游戏地图
{
	public static Dictionary<byte, 游戏地图> 数据表;

	public byte 地图编号;

	public string 地图名字;

	public string 地图别名;

	public string 地形文件;

	public int 限制人数;

	public byte 限制等级;

	public byte 分线数量;

	public bool 下线传送;

	public byte 传送地图;

	public bool 副本地图;

	public static void 载入数据()
	{
		数据表 = new Dictionary<byte, 游戏地图>();
		string text = 自定义类.游戏数据目录 + "\\System\\游戏地图\\地图数据";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(游戏地图));
			foreach (object obj in array)
			{
				数据表.Add(((游戏地图)obj).地图编号, (游戏地图)obj);
			}
		}
	}

	public override string ToString()
	{
		return 地图名字;
	}

	static 游戏地图()
	{
	}
}
