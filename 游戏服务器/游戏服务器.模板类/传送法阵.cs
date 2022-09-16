using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace 游戏服务器.模板类;

public class 传送法阵
{
	public static List<传送法阵> 数据表;

	public byte 法阵编号;

	public byte 所处地图;

	public byte 跳转地图;

	public string 法阵名字;

	public string 所处地名;

	public string 跳转地名;

	public string 所处别名;

	public string 跳转别名;

	public Point 所处坐标;

	public Point 跳转坐标;

	public static void 载入数据()
	{
		数据表 = new List<传送法阵>();
		string text = 自定义类.游戏数据目录 + "\\System\\游戏地图\\法阵数据\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(传送法阵));
			foreach (object obj in array)
			{
				数据表.Add((传送法阵)obj);
			}
		}
	}

	static 传送法阵()
	{
	}
}
