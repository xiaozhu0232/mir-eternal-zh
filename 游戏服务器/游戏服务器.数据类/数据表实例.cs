using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace 游戏服务器.数据类;

public sealed class 数据表实例<T> : 数据表基类 where T : 游戏数据, new()
{
	public 数据表实例()
	{
		当前映射 = new 数据映射(数据类型 = typeof(T));
		数据表 = new Dictionary<int, 游戏数据>();
		检索表 = new Dictionary<string, 游戏数据>();
		数据快速检索 customAttribute = 数据类型.GetCustomAttribute<数据快速检索>();
		if (customAttribute != null)
		{
			检索字段 = 数据类型.GetField(customAttribute.检索字段, BindingFlags.Instance | BindingFlags.Public);
		}
		if (数据类型 == typeof(行会数据))
		{
			当前索引 = 1610612736;
		}
		if (数据类型 == typeof(队伍数据))
		{
			当前索引 = 1879048192;
		}
	}

	public override void 添加数据(游戏数据 数据, bool 分配索引 = false)
	{
		if (分配索引)
		{
			数据.数据索引.V = ++当前索引;
		}
		if (数据.数据索引.V == 0)
		{
			MessageBox.Show("数据表添加数据异常, 索引为零.");
		}
		数据.数据存表 = this;
		数据表.Add(数据.数据索引.V, 数据);
		if (检索字段 != null)
		{
			检索表.Add((检索字段.GetValue(数据) as 数据监视器<string>).V, 数据);
		}
		游戏数据网关.已经修改 = true;
	}

	public override void 删除数据(游戏数据 数据)
	{
		数据表.Remove(数据.数据索引.V);
		if (检索字段 != null)
		{
			检索表.Remove((检索字段.GetValue(数据) as 数据监视器<string>).V);
		}
		游戏数据网关.已经修改 = true;
	}

	public override void 保存数据()
	{
		foreach (KeyValuePair<int, 游戏数据> item in 数据表)
		{
			if (!版本一致 || item.Value.已经修改)
			{
				item.Value.保存数据();
			}
		}
		版本一致 = true;
	}

	public override void 强制保存()
	{
		foreach (KeyValuePair<int, 游戏数据> item in 数据表)
		{
			item.Value.保存数据();
		}
		版本一致 = true;
	}

	public override void 加载数据(byte[] 存表数据, 数据映射 历史映射)
	{
		版本一致 = 历史映射.检查映射版本(当前映射);
		using MemoryStream input = new MemoryStream(存表数据);
		using BinaryReader binaryReader = new BinaryReader(input);
		当前索引 = binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			T val = new T
			{
				数据存表 = this
			};
			val.原始数据 = binaryReader.ReadBytes(binaryReader.ReadInt32());
			val.加载数据(历史映射);
			数据表[val.数据索引.V] = val;
			if (检索字段 != null && 检索字段.GetValue(val) is 数据监视器<string> 数据监视器2 && 数据监视器2.V != null)
			{
				检索表[数据监视器2.V] = val;
			}
		}
		主窗口.添加系统日志($"{数据类型.Name}已经加载,  数量: {num}");
	}

	public override byte[] 存表数据()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(当前索引);
		binaryWriter.Write(数据表.Count);
		foreach (KeyValuePair<int, 游戏数据> item in 数据表)
		{
			binaryWriter.Write(item.Value.原始数据.Length);
			binaryWriter.Write(item.Value.原始数据);
		}
		memoryStream.Seek(4L, SeekOrigin.Begin);
		return memoryStream.ToArray();
	}
}
