using System;
using System.Collections.Generic;
using System.IO;
using 游戏服务器.模板类;

namespace 游戏服务器.数据类;

public class 物品数据 : 游戏数据
{
	public static byte 数据版本;

	public readonly 数据监视器<游戏物品> 对应模板;

	public readonly 数据监视器<DateTime> 生成时间;

	public readonly 数据监视器<角色数据> 生成来源;

	public readonly 数据监视器<int> 当前持久;

	public readonly 数据监视器<int> 最大持久;

	public readonly 数据监视器<byte> 物品容器;

	public readonly 数据监视器<byte> 物品位置;

	public int 回购编号;

	public 游戏物品 物品模板 => 对应模板.V;

	public 物品出售分类 出售类型 => 物品模板.商店类型;

	public 物品使用分类 物品类型 => 物品模板.物品分类;

	public 物品持久分类 持久类型 => 物品模板.持久类型;

	public 游戏对象职业 需要职业 => 物品模板.需要职业;

	public 游戏对象性别 需要性别 => 物品模板.需要性别;

	public string 物品名字 => 物品模板.物品名字;

	public int 需要等级 => 物品模板.需要等级;

	public int 物品编号 => 对应模板.V.物品编号;

	public int 物品重量
	{
		get
		{
			if (持久类型 != 物品持久分类.堆叠)
			{
				return 物品模板.物品重量;
			}
			return 当前持久.V * 物品模板.物品重量;
		}
	}

	public int 出售价格
	{
		get
		{
			switch (对应模板.V.持久类型)
			{
			default:
				return 0;
			case 物品持久分类.无:
				return 1;
			case 物品持久分类.装备:
			{
				装备数据 装备数据2 = this as 装备数据;
				游戏装备 obj = 对应模板.V as 游戏装备;
				int v3 = 装备数据2.当前持久.V;
				int num2 = obj.物品持久 * 1000;
				int num3 = obj.出售价格;
				int num4 = Math.Max((sbyte)0, 装备数据2.幸运等级.V);
				int num5 = 装备数据2.升级攻击.V * 100 + 装备数据2.升级魔法.V * 100 + 装备数据2.升级道术.V * 100 + 装备数据2.升级刺术.V * 100 + 装备数据2.升级弓术.V * 100;
				int num6 = 0;
				foreach (铭文技能 value in 装备数据2.铭文技能.Values)
				{
					if (value != null)
					{
						num6++;
					}
				}
				int num7 = 0;
				foreach (随机属性 item in 装备数据2.随机属性)
				{
					num7 += item.战力加成 * 100;
				}
				int num8 = 0;
				using (IEnumerator<游戏物品> enumerator3 = 装备数据2.镶嵌灵石.Values.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						switch (enumerator3.Current.物品名字)
						{
						case "命朱灵石3级":
						case "蔚蓝灵石3级":
						case "盈绿灵石3级":
						case "守阳灵石3级":
						case "进击幻彩灵石3级":
						case "橙黄灵石3级":
						case "驭朱灵石3级":
						case "精绿灵石3级":
						case "韧紫灵石3级":
						case "新阳灵石3级":
						case "赤褐灵石3级":
						case "纯紫灵石3级":
						case "狂热幻彩灵石3级":
						case "深灰灵石3级":
						case "透蓝灵石3级":
						case "抵御幻彩灵石3级":
							num8 += 3000;
							break;
						case "命朱灵石4级":
						case "蔚蓝灵石4级":
						case "新阳灵石4级":
						case "盈绿灵石4级":
						case "进击幻彩灵石4级":
						case "橙黄灵石4级":
						case "驭朱灵石4级":
						case "深灰灵石4级":
						case "赤褐灵石4级":
						case "狂热幻彩灵石4级":
						case "韧紫灵石4级":
						case "纯紫灵石4级":
						case "精绿灵石4级":
						case "透蓝灵石4级":
						case "守阳灵石4级":
						case "抵御幻彩灵石4级":
							num8 += 4000;
							break;
						case "进击幻彩灵石8级":
						case "狂热幻彩灵石8级":
						case "盈绿灵石8级":
						case "驭朱灵石8级":
						case "抵御幻彩灵石8级":
						case "韧紫灵石8级":
						case "精绿灵石8级":
						case "赤褐灵石8级":
						case "橙黄灵石8级":
						case "命朱灵石8级":
						case "蔚蓝灵石8级":
						case "纯紫灵石8级":
						case "深灰灵石8级":
						case "守阳灵石8级":
						case "透蓝灵石8级":
						case "新阳灵石8级":
							num8 += 8000;
							break;
						case "进击幻彩灵石6级":
						case "蔚蓝灵石6级":
						case "深灰灵石6级":
						case "盈绿灵石6级":
						case "命朱灵石6级":
						case "赤褐灵石6级":
						case "橙黄灵石6级":
						case "抵御幻彩灵石6级":
						case "透蓝灵石6级":
						case "纯紫灵石6级":
						case "狂热幻彩灵石6级":
						case "精绿灵石6级":
						case "新阳灵石6级":
						case "驭朱灵石6级":
						case "守阳灵石6级":
						case "韧紫灵石6级":
							num8 += 6000;
							break;
						case "进击幻彩灵石5级":
						case "透蓝灵石5级":
						case "盈绿灵石5级":
						case "深灰灵石5级":
						case "精绿灵石5级":
						case "橙黄灵石5级":
						case "新阳灵石5级":
						case "命朱灵石5级":
						case "蔚蓝灵石5级":
						case "狂热幻彩灵石5级":
						case "纯紫灵石5级":
						case "抵御幻彩灵石5级":
						case "韧紫灵石5级":
						case "驭朱灵石5级":
						case "赤褐灵石5级":
						case "守阳灵石5级":
							num8 += 5000;
							break;
						case "守阳灵石10级":
						case "命朱灵石10级":
						case "赤褐灵石10级":
						case "蔚蓝灵石10级":
						case "狂热幻彩灵石10级":
						case "透蓝灵石10级":
						case "精绿灵石10级":
						case "进击幻彩灵石10级":
						case "橙黄灵石10级":
						case "抵御幻彩灵石10级":
						case "韧紫灵石10级":
						case "驭朱灵石10级":
						case "新阳灵石10级":
						case "纯紫灵石10级":
						case "盈绿灵石10级":
						case "深灰灵石10级":
							num8 += 10000;
							break;
						case "纯紫灵石1级":
						case "赤褐灵石1级":
						case "精绿灵石1级":
						case "狂热幻彩灵石1级":
						case "驭朱灵石1级":
						case "透蓝灵石1级":
						case "守阳灵石1级":
						case "韧紫灵石1级":
						case "命朱灵石1级":
						case "深灰灵石1级":
						case "橙黄灵石1级":
						case "抵御幻彩灵石1级":
						case "进击幻彩灵石1级":
						case "新阳灵石1级":
						case "蔚蓝灵石1级":
						case "盈绿灵石1级":
							num8 += 1000;
							break;
						case "透蓝灵石9级":
						case "精绿灵石9级":
						case "驭朱灵石9级":
						case "橙黄灵石9级":
						case "抵御幻彩灵石9级":
						case "蔚蓝灵石9级":
						case "狂热幻彩灵石9级":
						case "进击幻彩灵石9级":
						case "守阳灵石9级":
						case "新阳灵石9级":
						case "盈绿灵石9级":
						case "韧紫灵石9级":
						case "命朱灵石9级":
						case "纯紫灵石9级":
						case "赤褐灵石9级":
						case "深灰灵石9级":
							num8 += 9000;
							break;
						case "守阳灵石2级":
						case "透蓝灵石2级":
						case "纯紫灵石2级":
						case "精绿灵石2级":
						case "橙黄灵石2级":
						case "驭朱灵石2级":
						case "蔚蓝灵石2级":
						case "狂热幻彩灵石2级":
						case "韧紫灵石2级":
						case "盈绿灵石2级":
						case "抵御幻彩灵石2级":
						case "赤褐灵石2级":
						case "命朱灵石2级":
						case "深灰灵石2级":
						case "进击幻彩灵石2级":
						case "新阳灵石2级":
							num8 += 2000;
							break;
						case "精绿灵石7级":
						case "纯紫灵石7级":
						case "狂热幻彩灵石7级":
						case "命朱灵石7级":
						case "抵御幻彩灵石7级":
						case "赤褐灵石7级":
						case "守阳灵石7级":
						case "橙黄灵石7级":
						case "透蓝灵石7级":
						case "进击幻彩灵石7级":
						case "驭朱灵石7级":
						case "韧紫灵石7级":
						case "深灰灵石7级":
						case "蔚蓝灵石7级":
						case "盈绿灵石7级":
						case "新阳灵石7级":
							num8 += 7000;
							break;
						}
					}
				}
				int num9 = num3 + num4 + num5 + num6 + num7 + num8;
				decimal num10 = (decimal)v3 / (decimal)num2 * 0.9m * (decimal)num9;
				decimal num11 = (decimal)num9 * 0.1m;
				return (int)(num10 + num11);
			}
			case 物品持久分类.消耗:
			{
				int v2 = 当前持久.V;
				int 物品持久 = 对应模板.V.物品持久;
				int num = 对应模板.V.出售价格;
				return (int)((decimal)v2 / (decimal)物品持久 * (decimal)num);
			}
			case 物品持久分类.堆叠:
			{
				int v = 当前持久.V;
				return 对应模板.V.出售价格 * v;
			}
			case 物品持久分类.回复:
				return 1;
			case 物品持久分类.容器:
				return 对应模板.V.出售价格;
			case 物品持久分类.纯度:
				return 对应模板.V.出售价格;
			}
		}
	}

	public int 堆叠上限 => 对应模板.V.物品持久;

	public int 默认持久
	{
		get
		{
			if (持久类型 != 物品持久分类.装备)
			{
				return 对应模板.V.物品持久;
			}
			return 对应模板.V.物品持久 * 1000;
		}
	}

	public byte 当前位置
	{
		get
		{
			return 物品位置.V;
		}
		set
		{
			物品位置.V = value;
		}
	}

	public bool 是否绑定 => 物品模板.是否绑定;

	public bool 资源物品 => 对应模板.V.资源物品;

	public bool 能否出售 => 物品模板.能否出售;

	public bool 能否堆叠 => 对应模板.V.持久类型 == 物品持久分类.堆叠;

	public bool 能否掉落 => 物品模板.能否掉落;

	public ushort 技能编号 => 物品模板.附加技能;

	public byte 分组编号 => 物品模板.物品分组;

	public int 分组冷却 => 物品模板.分组冷却;

	public int 冷却时间 => 物品模板.冷却时间;

	public 物品数据()
	{
	}

	public 物品数据(游戏物品 模板, 角色数据 来源, byte 容器, byte 位置, int 持久)
	{
		对应模板.V = 模板;
		生成来源.V = 来源;
		物品容器.V = 容器;
		物品位置.V = 位置;
		生成时间.V = 主程.当前时间;
		最大持久.V = 物品模板.物品持久;
		当前持久.V = Math.Min(持久, 最大持久.V);
		游戏数据网关.物品数据表.添加数据(this, 分配索引: true);
	}

	public override string ToString()
	{
		return 物品名字;
	}

	public virtual byte[] 字节描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(数据版本);
		binaryWriter.Write(生成来源.V?.数据索引.V ?? 0);
		binaryWriter.Write(计算类.时间转换(生成时间.V));
		binaryWriter.Write(对应模板.V.物品编号);
		binaryWriter.Write(物品容器.V);
		binaryWriter.Write(物品位置.V);
		binaryWriter.Write(当前持久.V);
		binaryWriter.Write(最大持久.V);
		binaryWriter.Write((byte)(是否绑定 ? 10u : 0u));
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0);
		return memoryStream.ToArray();
	}

	public virtual byte[] 字节描述(int 数量)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(数据版本);
		binaryWriter.Write(生成来源.V?.数据索引.V ?? 0);
		binaryWriter.Write(计算类.时间转换(生成时间.V));
		binaryWriter.Write(对应模板.V.物品编号);
		binaryWriter.Write(物品容器.V);
		binaryWriter.Write(物品位置.V);
		binaryWriter.Write(数量);
		binaryWriter.Write(最大持久.V);
		binaryWriter.Write((byte)(是否绑定 ? 10u : 0u));
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0);
		return memoryStream.ToArray();
	}

	static 物品数据()
	{
		数据版本 = 14;
	}
}
