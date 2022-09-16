using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace 游戏服务器.模板类;

public class 地图区域
{
	public static List<地图区域> 数据表;

	public byte 所处地图;

	public string 所处地名;

	public Point 所处坐标;

	public string 区域名字;

	public int 区域半径;

	public 地图区域类型 区域类型;

	public HashSet<Point> 范围坐标;

	private List<Point> _范围坐标列表;

	public Point 随机坐标 => 范围坐标列表[主程.随机数.Next(范围坐标列表.Count)];

	public List<Point> 范围坐标列表
	{
		get
		{
			if (_范围坐标列表 == null)
			{
				_范围坐标列表 = 范围坐标.ToList();
			}
			return _范围坐标列表;
		}
	}

	public static void 载入数据()
	{
		数据表 = new List<地图区域>();
		string text = 自定义类.游戏数据目录 + "\\System\\游戏地图\\地图区域\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(地图区域));
			foreach (object obj in array)
			{
				数据表.Add((地图区域)obj);
			}
		}
	}

	static 地图区域()
	{
	}
}
