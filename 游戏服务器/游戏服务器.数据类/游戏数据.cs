using System;
using System.IO;
using System.Reflection;

namespace 游戏服务器.数据类;

public abstract class 游戏数据
{
	public readonly 数据监视器<int> 数据索引;

	public readonly Type 数据类型;

	public readonly MemoryStream 内存流;

	public readonly BinaryWriter 写入流;

	public byte[] 原始数据 { get; set; }

	public bool 已经修改 { get; set; }

	public 数据表基类 数据存表 { get; set; }

	protected void 创建字段()
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType.IsGenericType)
			{
				Type genericTypeDefinition = fieldInfo.FieldType.GetGenericTypeDefinition();
				if (!(genericTypeDefinition != typeof(数据监视器<>)) || !(genericTypeDefinition != typeof(列表监视器<>)) || !(genericTypeDefinition != typeof(哈希监视器<>)) || !(genericTypeDefinition != typeof(字典监视器<, >)))
				{
					fieldInfo.SetValue(this, Activator.CreateInstance(fieldInfo.FieldType, this));
				}
			}
		}
	}

	public override string ToString()
	{
		return 数据类型?.Name;
	}

	public 游戏数据()
	{
		数据类型 = GetType();
		内存流 = new MemoryStream();
		写入流 = new BinaryWriter(内存流);
		创建字段();
	}

	public void 保存数据()
	{
		内存流.SetLength(0L);
		foreach (数据字段 item in 数据存表.当前映射.字段列表)
		{
			item.保存字段内容(写入流, item.字段详情.GetValue(this));
		}
		原始数据 = 内存流.ToArray();
		已经修改 = false;
	}

	public void 加载数据(数据映射 历史映射)
	{
		using MemoryStream input = new MemoryStream(原始数据);
		using BinaryReader 读取流 = new BinaryReader(input);
		foreach (数据字段 item in 历史映射.字段列表)
		{
			object value = item.读取字段内容(读取流, this, item);
			if (!(item.字段详情 == null) && item.字段类型 == item.字段详情.FieldType)
			{
				item.字段详情.SetValue(this, value);
			}
		}
	}

	public virtual void 删除数据()
	{
		数据存表?.删除数据(this);
	}

	public virtual void 加载完成()
	{
	}

	static 游戏数据()
	{
	}
}
