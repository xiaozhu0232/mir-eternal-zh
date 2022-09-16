using System;
using System.Collections.Generic;
using 游戏服务器.模板类;

namespace 游戏服务器.数据类;

public sealed class 技能数据 : 游戏数据
{
	public byte 铭文编号;

	public DateTime 计数时间;

	public readonly 数据监视器<ushort> 技能编号;

	public readonly 数据监视器<ushort> 技能经验;

	public readonly 数据监视器<byte> 技能等级;

	public readonly 数据监视器<byte> 快捷栏位;

	public readonly 数据监视器<byte> 剩余次数;

	public int 技能索引 => 技能编号.V * 100 + 铭文编号 * 10 + 技能等级.V;

	public 铭文技能 铭文模板 => 铭文技能.数据表[铭文索引];

	public bool 自动装配 => 铭文模板.被动技能;

	public byte 升级等级
	{
		get
		{
			if (铭文模板.需要角色等级 == null || 铭文模板.需要角色等级.Length <= 技能等级.V + 1)
			{
				return byte.MaxValue;
			}
			if (铭文模板.需要角色等级[技能等级.V] == 0)
			{
				return byte.MaxValue;
			}
			return 铭文模板.需要角色等级[技能等级.V];
		}
	}

	public byte 技能计数 => 铭文模板.技能计数;

	public ushort 计数周期 => 铭文模板.计数周期;

	public ushort 升级经验
	{
		get
		{
			if (铭文模板.需要技能经验 == null || 铭文模板.需要技能经验.Length <= 技能等级.V)
			{
				return 0;
			}
			return 铭文模板.需要技能经验[技能等级.V];
		}
	}

	public ushort 铭文索引 => (ushort)(技能编号.V * 10 + 铭文编号);

	public int 战力加成 => 铭文模板.技能战力加成[技能等级.V];

	public List<ushort> 技能Buff => 铭文模板.铭文附带Buff;

	public List<ushort> 被动技能 => 铭文模板.被动技能列表;

	public Dictionary<游戏对象属性, int> 属性加成
	{
		get
		{
			if (铭文模板.属性加成 != null && 铭文模板.属性加成.Length > 技能等级.V)
			{
				return 铭文模板.属性加成[技能等级.V];
			}
			return null;
		}
	}

	public 技能数据()
	{
	}

	public 技能数据(ushort 编号)
	{
		快捷栏位.V = 100;
		技能编号.V = 编号;
		剩余次数.V = 技能计数;
		游戏数据网关.技能数据表.添加数据(this, 分配索引: true);
	}

	public override string ToString()
	{
		return 铭文模板?.技能名字;
	}

	static 技能数据()
	{
	}
}
