using System;
using System.Collections.Generic;
using System.Reflection;

namespace 游戏服务器.数据类;

public abstract class 数据表基类
{
	public int 当前索引;

	public bool 版本一致;

	public Type 数据类型;

	public FieldInfo 检索字段;

	public 数据映射 当前映射;

	public Dictionary<int, 游戏数据> 数据表;

	public Dictionary<string, 游戏数据> 检索表;

	internal 游戏数据 this[int 索引]
	{
		get
		{
			if (数据表.TryGetValue(索引, out var value))
			{
				return value;
			}
			return null;
		}
	}

	internal 游戏数据 this[string 名称]
	{
		get
		{
			if (检索表.TryGetValue(名称, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public override string ToString()
	{
		return 数据类型?.Name;
	}

	public abstract void 加载数据(byte[] 原始数据, 数据映射 数据映射);

	public abstract void 保存数据();

	public abstract void 强制保存();

	public abstract void 删除数据(游戏数据 数据);

	public abstract void 添加数据(游戏数据 数据, bool 分配索引 = false);

	public abstract byte[] 存表数据();

	static 数据表基类()
	{
	}
}
