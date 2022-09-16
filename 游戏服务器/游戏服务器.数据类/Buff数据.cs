using System;
using System.Collections.Generic;
using 游戏服务器.地图类;
using 游戏服务器.模板类;

namespace 游戏服务器.数据类;

public sealed class Buff数据 : 游戏数据
{
	public 地图对象 Buff来源;

	public readonly 数据监视器<ushort> Buff编号;

	public readonly 数据监视器<TimeSpan> 持续时间;

	public readonly 数据监视器<TimeSpan> 剩余时间;

	public readonly 数据监视器<TimeSpan> 处理计时;

	public readonly 数据监视器<byte> 当前层数;

	public readonly 数据监视器<byte> Buff等级;

	public readonly 数据监视器<int> 伤害基数;

	public Buff效果类型 Buff效果 => Buff模板.Buff效果;

	public 技能伤害类型 伤害类型 => Buff模板.Buff伤害类型;

	public 游戏Buff Buff模板
	{
		get
		{
			if (!游戏Buff.数据表.TryGetValue(Buff编号.V, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public bool 增益Buff => Buff模板.作用类型 == Buff作用类型.增益类型;

	public bool Buff同步 => Buff模板.同步至客户端;

	public bool 到期消失 => Buff模板?.到期主动消失 ?? false;

	public bool 下线消失 => Buff模板.角色下线消失;

	public bool 死亡消失 => Buff模板.角色死亡消失;

	public bool 换图消失 => Buff模板.切换地图消失;

	public bool 绑定武器 => Buff模板.切换武器消失;

	public bool 添加冷却 => Buff模板.移除添加冷却;

	public ushort 绑定技能 => Buff模板.绑定技能等级;

	public ushort 冷却时间 => Buff模板.技能冷却时间;

	public int 处理延迟 => Buff模板.Buff处理延迟;

	public int 处理间隔 => Buff模板.Buff处理间隔;

	public byte 最大层数 => Buff模板.Buff最大层数;

	public ushort Buff分组
	{
		get
		{
			if (Buff模板.分组编号 == 0)
			{
				return Buff编号.V;
			}
			return Buff模板.分组编号;
		}
	}

	public ushort[] 依存列表 => Buff模板.依存Buff列表;

	public Dictionary<游戏对象属性, int> 属性加成
	{
		get
		{
			if ((Buff效果 & Buff效果类型.属性增减) != 0)
			{
				return Buff模板.基础属性增减[Buff等级.V];
			}
			return null;
		}
	}

	public Buff数据()
	{
	}

	public Buff数据(地图对象 来源, 地图对象 目标, ushort 编号)
	{
		Buff来源 = 来源;
		Buff编号.V = 编号;
		当前层数.V = Buff模板.Buff初始层数;
		持续时间.V = TimeSpan.FromMilliseconds(Buff模板.Buff持续时间);
		处理计时.V = TimeSpan.FromMilliseconds(Buff模板.Buff处理延迟);
		if (!(来源 is 玩家实例 玩家实例))
		{
			if (来源 is 宠物实例 宠物实例)
			{
				if (Buff模板.绑定技能等级 != 0 && 宠物实例.宠物主人.主体技能表.TryGetValue(Buff模板.绑定技能等级, out var v))
				{
					Buff等级.V = v.技能等级.V;
				}
				if (Buff模板.持续时间延长 && Buff模板.技能等级延时)
				{
					持续时间.V += TimeSpan.FromMilliseconds(Buff等级.V * Buff模板.每级延长时间);
				}
				if (Buff模板.持续时间延长 && Buff模板.角色属性延时)
				{
					持续时间.V += TimeSpan.FromMilliseconds((float)宠物实例.宠物主人[Buff模板.绑定角色属性] * Buff模板.属性延时系数);
				}
				if (Buff模板.持续时间延长 && Buff模板.特定铭文延时 && 宠物实例.宠物主人.主体技能表.TryGetValue((ushort)(Buff模板.特定铭文技能 / 10), out var v2) && v2.铭文编号 == Buff模板.特定铭文技能 % 10)
				{
					持续时间.V += TimeSpan.FromMilliseconds(Buff模板.铭文延长时间);
				}
			}
		}
		else
		{
			if (Buff模板.绑定技能等级 != 0 && 玩家实例.主体技能表.TryGetValue(Buff模板.绑定技能等级, out var v3))
			{
				Buff等级.V = v3.技能等级.V;
			}
			if (Buff模板.持续时间延长 && Buff模板.技能等级延时)
			{
				持续时间.V += TimeSpan.FromMilliseconds(Buff等级.V * Buff模板.每级延长时间);
			}
			if (Buff模板.持续时间延长 && Buff模板.角色属性延时)
			{
				持续时间.V += TimeSpan.FromMilliseconds((float)玩家实例[Buff模板.绑定角色属性] * Buff模板.属性延时系数);
			}
			if (Buff模板.持续时间延长 && Buff模板.特定铭文延时 && 玩家实例.主体技能表.TryGetValue((ushort)(Buff模板.特定铭文技能 / 10), out var v4) && v4.铭文编号 == Buff模板.特定铭文技能 % 10)
			{
				持续时间.V += TimeSpan.FromMilliseconds(Buff模板.铭文延长时间);
			}
		}
		剩余时间.V = 持续时间.V;
		if ((Buff效果 & Buff效果类型.造成伤害) != 0)
		{
			int num = ((Buff模板.Buff伤害基数?.Length > Buff等级.V) ? Buff模板.Buff伤害基数[Buff等级.V] : 0);
			float num2 = ((!(Buff模板.Buff伤害系数?.Length > Buff等级.V)) ? 0f : Buff模板.Buff伤害系数[Buff等级.V]);
			if (来源 is 玩家实例 玩家实例2 && Buff模板.强化铭文编号 != 0 && 玩家实例2.主体技能表.TryGetValue((ushort)(Buff模板.强化铭文编号 / 10), out var v5) && v5.铭文编号 == Buff模板.强化铭文编号 % 10)
			{
				num += Buff模板.铭文强化基数;
				num2 += Buff模板.铭文强化系数;
			}
			int num3 = 0;
			switch (伤害类型)
			{
			case 技能伤害类型.攻击:
				num3 = 计算类.计算攻击(来源[游戏对象属性.最小攻击], 来源[游戏对象属性.最大攻击], 来源[游戏对象属性.幸运等级]);
				break;
			case 技能伤害类型.魔法:
				num3 = 计算类.计算攻击(来源[游戏对象属性.最小魔法], 来源[游戏对象属性.最大魔法], 来源[游戏对象属性.幸运等级]);
				break;
			case 技能伤害类型.道术:
				num3 = 计算类.计算攻击(来源[游戏对象属性.最小道术], 来源[游戏对象属性.最大道术], 来源[游戏对象属性.幸运等级]);
				break;
			case 技能伤害类型.刺术:
				num3 = 计算类.计算攻击(来源[游戏对象属性.最小刺术], 来源[游戏对象属性.最大刺术], 来源[游戏对象属性.幸运等级]);
				break;
			case 技能伤害类型.弓术:
				num3 = 计算类.计算攻击(来源[游戏对象属性.最小弓术], 来源[游戏对象属性.最大弓术], 来源[游戏对象属性.幸运等级]);
				break;
			case 技能伤害类型.毒性:
				num3 = 来源[游戏对象属性.最大道术];
				break;
			case 技能伤害类型.神圣:
				num3 = 计算类.计算攻击(来源[游戏对象属性.最小圣伤], 来源[游戏对象属性.最大圣伤], 0);
				break;
			}
			伤害基数.V = num + (int)((float)num3 * num2);
		}
		if (目标 is 玩家实例)
		{
			游戏数据网关.Buff数据表.添加数据(this, 分配索引: true);
		}
	}

	public override string ToString()
	{
		return Buff模板?.Buff名字;
	}

	static Buff数据()
	{
	}
}
