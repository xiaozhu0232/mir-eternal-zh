using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace 游戏服务器.模板类;

public sealed class 守卫刷新
{
	public static HashSet<守卫刷新> 数据表;

	public ushort 守卫编号;

	public byte 所处地图;

	public string 所处地名;

	public Point 所处坐标;

	public 游戏方向 所处方向;

	public string 区域名字;

	public static void 载入数据()
	{
		数据表 = new HashSet<守卫刷新>();
		string text = 自定义类.游戏数据目录 + "\\System\\游戏地图\\守卫刷新\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(守卫刷新));
			foreach (object obj in array)
			{
				数据表.Add((守卫刷新)obj);
			}
		}
	}

	static 守卫刷新()
	{
	}
}
