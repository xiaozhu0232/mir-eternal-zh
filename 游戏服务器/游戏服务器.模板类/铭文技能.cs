using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.模板类;

public sealed class 铭文技能
{
	public static Dictionary<ushort, 铭文技能> 数据表;

	private static Dictionary<byte, List<铭文技能>> 概率表;

	public string 技能名字;

	public 游戏对象职业 技能职业;

	public ushort 技能编号;

	public byte 铭文编号;

	public byte 技能计数;

	public ushort 计数周期;

	public bool 被动技能;

	public byte 铭文品质;

	public int 洗练概率;

	public bool 广播通知;

	public string 铭文描述;

	public byte[] 需要角色等级;

	public ushort[] 需要技能经验;

	public int[] 技能战力加成;

	public 铭文属性[] 铭文属性加成;

	public List<ushort> 铭文附带Buff;

	public List<ushort> 被动技能列表;

	public List<string> 主体技能列表;

	public List<string> 开关技能列表;

	private Dictionary<游戏对象属性, int>[] _属性加成;

	public ushort 铭文索引 => (ushort)(技能编号 * 10 + 铭文编号);

	public Dictionary<游戏对象属性, int>[] 属性加成
	{
		get
		{
			if (_属性加成 == null)
			{
				_属性加成 = new Dictionary<游戏对象属性, int>[4]
				{
					new Dictionary<游戏对象属性, int>(),
					new Dictionary<游戏对象属性, int>(),
					new Dictionary<游戏对象属性, int>(),
					new Dictionary<游戏对象属性, int>()
				};
				if (铭文属性加成 != null)
				{
					铭文属性[] array = 铭文属性加成;
					for (int i = 0; i < array.Length; i++)
					{
						铭文属性 铭文属性2 = array[i];
						_属性加成[0][铭文属性2.属性] = 铭文属性2.零级;
						_属性加成[1][铭文属性2.属性] = 铭文属性2.一级;
						_属性加成[2][铭文属性2.属性] = 铭文属性2.二级;
						_属性加成[3][铭文属性2.属性] = 铭文属性2.三级;
					}
				}
				return _属性加成;
			}
			return _属性加成;
		}
	}

	public static 铭文技能 随机洗练(byte 洗练职业)
	{
		if (概率表.TryGetValue(洗练职业, out var value) && value.Count > 0)
		{
			return value[主程.随机数.Next(value.Count)];
		}
		return null;
	}

	public static void 载入数据()
	{
		数据表 = new Dictionary<ushort, 铭文技能>();
		string text = 自定义类.游戏数据目录 + "\\System\\技能数据\\铭文数据\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(铭文技能));
			foreach (object obj in array)
			{
				数据表.Add(((铭文技能)obj).铭文索引, (铭文技能)obj);
			}
		}
		概率表 = new Dictionary<byte, List<铭文技能>>
		{
			[0] = new List<铭文技能>(),
			[1] = new List<铭文技能>(),
			[2] = new List<铭文技能>(),
			[3] = new List<铭文技能>(),
			[4] = new List<铭文技能>(),
			[5] = new List<铭文技能>()
		};
		foreach (铭文技能 value2 in 数据表.Values)
		{
			if (value2.铭文编号 != 0)
			{
				for (int j = 0; j < value2.洗练概率; j++)
				{
					概率表[(byte)value2.技能职业].Add(value2);
				}
			}
		}
		foreach (List<铭文技能> value3 in 概率表.Values)
		{
			for (int k = 0; k < value3.Count; k++)
			{
				铭文技能 value = value3[k];
				int index = 主程.随机数.Next(value3.Count);
				value3[k] = value3[index];
				value3[index] = value;
			}
		}
	}

	static 铭文技能()
	{
	}
}
