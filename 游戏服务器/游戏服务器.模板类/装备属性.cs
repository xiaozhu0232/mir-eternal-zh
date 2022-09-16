using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace 游戏服务器.模板类;

public sealed class 装备属性
{
	public class 属性详情
	{
		public int 属性编号;

		public int 属性概率;

		static 属性详情()
		{
		}
	}

	public static Dictionary<byte, 装备属性> 数据表;

	public static Dictionary<byte, 随机属性[]> 概率表;

	public 物品使用分类 装备部位;

	public float 极品概率;

	public int 单条概率;

	public int 两条概率;

	public 属性详情[] 属性列表;

	public static List<随机属性> 生成属性(物品使用分类 部位, bool 重铸装备 = false)
	{
		if (数据表.TryGetValue((byte)部位, out var value) && 概率表.TryGetValue((byte)部位, out var value2) && value2.Length != 0 && (重铸装备 || 计算类.计算概率(value.极品概率)))
		{
			int num = 主程.随机数.Next(100);
			Dictionary<游戏对象属性, 随机属性> dictionary = new Dictionary<游戏对象属性, 随机属性>();
			int num2 = ((num < value.单条概率) ? 1 : ((num < value.两条概率) ? 2 : 3));
			for (int i = 0; i < num2; i++)
			{
				随机属性 随机属性2 = value2[主程.随机数.Next(value2.Length)];
				if (!dictionary.ContainsKey(随机属性2.对应属性))
				{
					dictionary[随机属性2.对应属性] = 随机属性2;
				}
			}
			return dictionary.Values.ToList();
		}
		return new List<随机属性>();
	}

	public static void 载入数据()
	{
		数据表 = new Dictionary<byte, 装备属性>();
		string text = 自定义类.游戏数据目录 + "\\System\\物品数据\\装备属性\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(装备属性));
			foreach (object obj in array)
			{
				数据表.Add((byte)((装备属性)obj).装备部位, (装备属性)obj);
			}
		}
		概率表 = new Dictionary<byte, 随机属性[]>();
		foreach (KeyValuePair<byte, 装备属性> item in 数据表)
		{
			List<随机属性> list = new List<随机属性>();
			属性详情[] array2 = item.Value.属性列表;
			foreach (属性详情 属性详情 in array2)
			{
				if (随机属性.数据表.TryGetValue(属性详情.属性编号, out var value))
				{
					for (int j = 0; j < 属性详情.属性概率; j++)
					{
						list.Add(value);
					}
				}
			}
			概率表[item.Key] = list.ToArray();
		}
	}

	static 装备属性()
	{
	}
}
