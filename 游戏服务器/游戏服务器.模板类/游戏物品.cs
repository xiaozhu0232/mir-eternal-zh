using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.模板类;

public class 游戏物品
{
	public static Dictionary<int, 游戏物品> 数据表;

	public static Dictionary<string, 游戏物品> 检索表;

	public string 物品名字;

	public int 物品编号;

	public int 物品持久;

	public int 物品重量;

	public int 物品等级;

	public int 需要等级;

	public int 冷却时间;

	public byte 物品分组;

	public int 分组冷却;

	public int 出售价格;

	public ushort 附加技能;

	public bool 是否绑定;

	public bool 能否分解;

	public bool 能否掉落;

	public bool 能否出售;

	public bool 贵重物品;

	public bool 资源物品;

	public 物品使用分类 物品分类;

	public 游戏对象职业 需要职业;

	public 游戏对象性别 需要性别;

	public 物品持久分类 持久类型;

	public 物品出售分类 商店类型;

	public static 游戏物品 获取数据(int 索引)
	{
		if (!数据表.TryGetValue(索引, out var value))
		{
			return null;
		}
		return value;
	}

	public static 游戏物品 获取数据(string 名字)
	{
		if (!检索表.TryGetValue(名字, out var value))
		{
			return null;
		}
		return value;
	}

	public static void 载入数据()
	{
		数据表 = new Dictionary<int, 游戏物品>();
		检索表 = new Dictionary<string, 游戏物品>();
		string text = 自定义类.游戏数据目录 + "\\System\\物品数据\\普通物品\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(游戏物品));
			for (int i = 0; i < array.Length; i++)
			{
				游戏物品 游戏物品2 = array[i] as 游戏物品;
				数据表.Add(游戏物品2.物品编号, 游戏物品2);
				检索表.Add(游戏物品2.物品名字, 游戏物品2);
			}
		}
		text = 自定义类.游戏数据目录 + "\\System\\物品数据\\装备物品\\";
		if (Directory.Exists(text))
		{
			object[] array2 = 序列化类.反序列化(text, typeof(游戏装备));
			for (int j = 0; j < array2.Length; j++)
			{
				游戏装备 游戏装备2 = array2[j] as 游戏装备;
				数据表.Add(游戏装备2.物品编号, 游戏装备2);
				检索表.Add(游戏装备2.物品名字, 游戏装备2);
			}
		}
	}

	static 游戏物品()
	{
	}
}
