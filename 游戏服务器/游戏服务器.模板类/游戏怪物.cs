using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.模板类;

public sealed class 游戏怪物
{
	public static Dictionary<string, 游戏怪物> 数据表;

	public string 怪物名字;

	public ushort 怪物编号;

	public byte 怪物等级;

	public 技能范围类型 怪物体型;

	public 怪物种族分类 怪物分类;

	public 怪物级别分类 怪物级别;

	public bool 怪物禁止移动;

	public bool 脱战自动石化;

	public ushort 石化状态编号;

	public bool 可见隐身目标;

	public bool 可被技能推动;

	public bool 可被技能控制;

	public bool 可被技能诱惑;

	public float 基础诱惑概率;

	public ushort 怪物移动间隔;

	public ushort 怪物漫游间隔;

	public ushort 尸体保留时长;

	public bool 主动攻击目标;

	public byte 怪物仇恨范围;

	public ushort 怪物仇恨时间;

	public string 普通攻击技能;

	public string 概率触发技能;

	public string 进入战斗技能;

	public string 退出战斗技能;

	public string 移动释放技能;

	public string 出生释放技能;

	public string 死亡释放技能;

	public 基础属性[] 怪物基础;

	public 成长属性[] 怪物成长;

	public 属性继承[] 继承属性;

	public ushort 怪物提供经验;

	public List<怪物掉落> 怪物掉落物品;

	public Dictionary<游戏物品, long> 掉落统计;

	private Dictionary<游戏对象属性, int> _基础属性;

	private Dictionary<游戏对象属性, int>[] _成长属性;

	public Dictionary<游戏对象属性, int> 基础属性
	{
		get
		{
			if (_基础属性 != null)
			{
				return _基础属性;
			}
			_基础属性 = new Dictionary<游戏对象属性, int>();
			if (怪物基础 != null)
			{
				基础属性[] array = 怪物基础;
				for (int i = 0; i < array.Length; i++)
				{
					基础属性 基础属性2 = array[i];
					_基础属性[基础属性2.属性] = 基础属性2.数值;
				}
			}
			return _基础属性;
		}
	}

	public Dictionary<游戏对象属性, int>[] 成长属性
	{
		get
		{
			if (_成长属性 != null)
			{
				return _成长属性;
			}
			_成长属性 = new Dictionary<游戏对象属性, int>[8]
			{
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>(),
				new Dictionary<游戏对象属性, int>()
			};
			if (怪物成长 != null)
			{
				成长属性[] array = 怪物成长;
				for (int i = 0; i < array.Length; i++)
				{
					成长属性 成长属性2 = array[i];
					_成长属性[0][成长属性2.属性] = 成长属性2.零级;
					_成长属性[1][成长属性2.属性] = 成长属性2.一级;
					_成长属性[2][成长属性2.属性] = 成长属性2.二级;
					_成长属性[3][成长属性2.属性] = 成长属性2.三级;
					_成长属性[4][成长属性2.属性] = 成长属性2.四级;
					_成长属性[5][成长属性2.属性] = 成长属性2.五级;
					_成长属性[6][成长属性2.属性] = 成长属性2.六级;
					_成长属性[7][成长属性2.属性] = 成长属性2.七级;
				}
			}
			return _成长属性;
		}
	}

	public static void 载入数据()
	{
		数据表 = new Dictionary<string, 游戏怪物>();
		string text = 自定义类.游戏数据目录 + "\\System\\Npc数据\\怪物数据\\";
		if (!Directory.Exists(text))
		{
			return;
		}
		object[] array = 序列化类.反序列化(text, typeof(游戏怪物));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is 游戏怪物 游戏怪物2)
			{
				数据表.Add(游戏怪物2.怪物名字, 游戏怪物2);
			}
		}
	}

	public 游戏怪物()
	{
		掉落统计 = new Dictionary<游戏物品, long>();
	}

	static 游戏怪物()
	{
	}
}
