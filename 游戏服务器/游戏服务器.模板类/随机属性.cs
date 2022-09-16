using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.模板类;

public sealed class 随机属性
{
	public static Dictionary<int, 随机属性> 数据表;

	public 游戏对象属性 对应属性;

	public int 属性数值;

	public int 属性编号;

	public int 战力加成;

	public string 属性描述;

	public static void 载入数据()
	{
		数据表 = new Dictionary<int, 随机属性>();
		string text = 自定义类.游戏数据目录 + "\\System\\物品数据\\随机属性\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(随机属性));
			foreach (object obj in array)
			{
				数据表.Add(((随机属性)obj).属性编号, (随机属性)obj);
			}
		}
	}

	static 随机属性()
	{
	}
}
