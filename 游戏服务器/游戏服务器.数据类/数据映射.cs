using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace 游戏服务器.数据类;

public sealed class 数据映射
{
	public Type 数据类型;

	public List<数据字段> 字段列表 { get; }

	public override string ToString()
	{
		return 数据类型?.Name;
	}

	public 数据映射(BinaryReader 读取流)
	{
		字段列表 = new List<数据字段>();
		string name = 读取流.ReadString();
		数据类型 = Assembly.GetEntryAssembly().GetType(name) ?? Assembly.GetCallingAssembly().GetType(name);
		int num = 读取流.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			字段列表.Add(new 数据字段(读取流, 数据类型));
		}
	}

	public 数据映射(Type 数据类型)
	{
		字段列表 = new List<数据字段>();
		this.数据类型 = 数据类型;
		FieldInfo[] fields = this.数据类型.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType.IsGenericType)
			{
				Type genericTypeDefinition = fieldInfo.FieldType.GetGenericTypeDefinition();
				if (!(genericTypeDefinition != typeof(数据监视器<>)) || !(genericTypeDefinition != typeof(列表监视器<>)) || !(genericTypeDefinition != typeof(哈希监视器<>)) || !(genericTypeDefinition != typeof(字典监视器<, >)))
				{
					字段列表.Add(new 数据字段(fieldInfo));
				}
			}
		}
	}

	public void 保存映射描述(BinaryWriter 写入流)
	{
		写入流.Write(数据类型.FullName);
		写入流.Write(字段列表.Count);
		foreach (数据字段 item in 字段列表)
		{
			item.保存字段描述(写入流);
		}
	}

	public bool 检查映射版本(数据映射 对比映射)
	{
		if (字段列表.Count != 对比映射.字段列表.Count)
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < 字段列表.Count)
			{
				if (!字段列表[num].检查字段版本(对比映射.字段列表[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	static 数据映射()
	{
	}
}
