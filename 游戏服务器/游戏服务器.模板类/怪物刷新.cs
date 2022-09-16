using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace 游戏服务器.模板类;

public class 怪物刷新
{
	public static HashSet<怪物刷新> 数据表;

	public byte 所处地图;

	public string 所处地名;

	public Point 所处坐标;

	public string 区域名字;

	public int 区域半径;

	public 刷新信息[] 刷新列表;

	public HashSet<Point> 范围坐标;

	public static void 载入数据()
	{
		数据表 = new HashSet<怪物刷新>();
		string text = 自定义类.游戏数据目录 + "\\System\\游戏地图\\怪物刷新\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(怪物刷新));
			foreach (object obj in array)
			{
				数据表.Add((怪物刷新)obj);
			}
		}
	}

	static 怪物刷新()
	{
	}
}
