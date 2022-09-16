using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace 游戏服务器.网络类;

public abstract class 游戏封包
{
	public static byte 加密字节;

	public static Dictionary<Type, MethodInfo> 封包处理方法表;

	public static Dictionary<ushort, Type> 服务器封包类型表;

	public static Dictionary<ushort, Type> 客户端封包类型表;

	public static Dictionary<Type, ushort> 服务器封包编号表;

	public static Dictionary<Type, ushort> 客户端封包编号表;

	public static Dictionary<ushort, ushort> 服务器封包长度表;

	public static Dictionary<ushort, ushort> 客户端封包长度表;

	public static Dictionary<Type, Func<BinaryReader, 封包字段描述, object>> 封包字段读取表;

	public static Dictionary<Type, Action<BinaryWriter, 封包字段描述, object>> 封包字段写入表;

	public readonly Type 封包类型;

	private readonly ushort 封包编号;

	private readonly ushort 封包长度;

	public virtual bool 是否加密 { get; set; }

	static 游戏封包()
	{
		加密字节 = 129;
		封包处理方法表 = new Dictionary<Type, MethodInfo>();
		封包字段读取表 = new Dictionary<Type, Func<BinaryReader, 封包字段描述, object>>
		{
			[typeof(bool)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return Convert.ToBoolean(读取流.ReadByte());
			},
			[typeof(byte)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return 读取流.ReadByte();
			},
			[typeof(sbyte)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return 读取流.ReadSByte();
			},
			[typeof(byte[])] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				int num = ((描述符.长度 != 0) ? 描述符.长度 : (读取流.ReadUInt16() - 4));
				return (num > 0) ? 读取流.ReadBytes(num) : new byte[0];
			},
			[typeof(short)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return 读取流.ReadInt16();
			},
			[typeof(ushort)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return 读取流.ReadUInt16();
			},
			[typeof(int)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return 读取流.ReadInt32();
			},
			[typeof(uint)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				return 读取流.ReadUInt32();
			},
			[typeof(string)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				byte[] bytes = 读取流.ReadBytes(描述符.长度);
				return Encoding.UTF8.GetString(bytes).Split(new char[1], StringSplitOptions.RemoveEmptyEntries)[0];
			},
			[typeof(Point)] = delegate(BinaryReader 读取流, 封包字段描述 描述符)
			{
				读取流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				Point point2 = new Point(读取流.ReadUInt16(), 读取流.ReadUInt16());
				return 计算类.协议坐标转点阵坐标(描述符.反向 ? new Point(point2.Y, point2.X) : point2);
			}
		};
		封包字段写入表 = new Dictionary<Type, Action<BinaryWriter, 封包字段描述, object>>
		{
			[typeof(bool)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((bool)对象);
			},
			[typeof(byte)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((byte)对象);
			},
			[typeof(sbyte)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((sbyte)对象);
			},
			[typeof(byte[])] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write(对象 as byte[]);
			},
			[typeof(short)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((short)对象);
			},
			[typeof(ushort)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((ushort)对象);
			},
			[typeof(int)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((int)对象);
			},
			[typeof(uint)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write((uint)对象);
			},
			[typeof(string)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				if (对象 is string s)
				{
					写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
					写入流.Write(Encoding.UTF8.GetBytes(s));
				}
			},
			[typeof(Point)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				Point point = 计算类.点阵坐标转协议坐标((Point)对象);
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				if (!描述符.反向)
				{
					写入流.Write((ushort)point.X);
					写入流.Write((ushort)point.Y);
				}
				else
				{
					写入流.Write((ushort)point.Y);
					写入流.Write((ushort)point.X);
				}
			},
			[typeof(DateTime)] = delegate(BinaryWriter 写入流, 封包字段描述 描述符, object 对象)
			{
				写入流.BaseStream.Seek(描述符.下标, SeekOrigin.Begin);
				写入流.Write(计算类.时间转换((DateTime)对象));
			}
		};
		服务器封包类型表 = new Dictionary<ushort, Type>();
		服务器封包编号表 = new Dictionary<Type, ushort>();
		服务器封包长度表 = new Dictionary<ushort, ushort>();
		客户端封包类型表 = new Dictionary<ushort, Type>();
		客户端封包编号表 = new Dictionary<Type, ushort>();
		客户端封包长度表 = new Dictionary<ushort, ushort>();
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsSubclassOf(typeof(游戏封包)))
			{
				continue;
			}
			封包信息描述 customAttribute = type.GetCustomAttribute<封包信息描述>();
			if (customAttribute != null)
			{
				if (customAttribute.来源 != 0)
				{
					服务器封包类型表[customAttribute.编号] = type;
					服务器封包编号表[type] = customAttribute.编号;
					服务器封包长度表[customAttribute.编号] = customAttribute.长度;
				}
				else
				{
					客户端封包类型表[customAttribute.编号] = type;
					客户端封包编号表[type] = customAttribute.编号;
					客户端封包长度表[customAttribute.编号] = customAttribute.长度;
					封包处理方法表[type] = typeof(客户网络).GetMethod("处理封包", new Type[1] { type });
				}
			}
		}
		string text = "";
		foreach (KeyValuePair<ushort, Type> item in 服务器封包类型表)
		{
			text += $"{item.Value.Name}\t{item.Key}\t{服务器封包长度表[item.Key]}\r\n";
		}
		string text2 = "";
		foreach (KeyValuePair<ushort, Type> item2 in 客户端封包类型表)
		{
			text2 += $"{item2.Value.Name}\t{item2.Key}\t{客户端封包长度表[item2.Key]}\r\n";
		}
		File.WriteAllText("./ServerPackRule.txt", text);
		File.WriteAllText("./ClientPackRule.txt", text2);
	}

	public 游戏封包()
	{
		是否加密 = true;
		封包类型 = GetType();
		if (封包类型.GetCustomAttribute<封包信息描述>().来源 == 封包来源.服务器)
		{
			封包编号 = 服务器封包编号表[封包类型];
			封包长度 = 服务器封包长度表[封包编号];
		}
		else
		{
			封包编号 = 客户端封包编号表[封包类型];
			封包长度 = 客户端封包长度表[封包编号];
		}
	}

	public byte[] 取字节()
	{
		using MemoryStream memoryStream = ((封包长度 == 0) ? new MemoryStream() : new MemoryStream(new byte[封包长度]));
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		FieldInfo[] fields = 封包类型.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			封包字段描述 customAttribute = fieldInfo.GetCustomAttribute<封包字段描述>();
			if (customAttribute != null)
			{
				Type fieldType = fieldInfo.FieldType;
				object value = fieldInfo.GetValue(this);
				if (封包字段写入表.TryGetValue(fieldType, out var value2))
				{
					value2(binaryWriter, customAttribute, value);
				}
			}
		}
		binaryWriter.Seek(0, SeekOrigin.Begin);
		binaryWriter.Write(封包编号);
		if (封包长度 == 0)
		{
			binaryWriter.Write((ushort)memoryStream.Length);
		}
		byte[] array = memoryStream.ToArray();
		if (是否加密)
		{
			return 加解密(array);
		}
		return array;
	}

	private void 填封包(byte[] 原始数据)
	{
		using MemoryStream input = new MemoryStream(原始数据);
		using BinaryReader arg = new BinaryReader(input);
		FieldInfo[] fields = 封包类型.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			封包字段描述 customAttribute = fieldInfo.GetCustomAttribute<封包字段描述>();
			if (customAttribute != null)
			{
				Type fieldType = fieldInfo.FieldType;
				if (封包字段读取表.TryGetValue(fieldType, out var value))
				{
					fieldInfo.SetValue(this, value(arg, customAttribute));
				}
			}
		}
	}

	public static 游戏封包 取封包(客户网络 网络连接, byte[] 原始数据, out byte[] 剩余数据)
	{
		剩余数据 = 原始数据;
		if (原始数据.Length < 2)
		{
			return null;
		}
		ushort num = BitConverter.ToUInt16(原始数据, 0);
		if (!客户端封包类型表.TryGetValue(num, out var value))
		{
			网络连接.尝试断开连接(new Exception($"封包组包失败! 封包编号:{num:X4}  封包编号:{num}"));
			return null;
		}
		if (!客户端封包长度表.TryGetValue(num, out var value2))
		{
			网络连接.尝试断开连接(new Exception($"获取封包长度失败! 封包编号:{num:X4}  封包编号:{num}"));
			return null;
		}
		if (value2 != 0 || 原始数据.Length >= 4)
		{
			value2 = ((value2 != 0) ? value2 : BitConverter.ToUInt16(原始数据, 2));
			if (原始数据.Length >= value2)
			{
				游戏封包 obj = (游戏封包)Activator.CreateInstance(value);
				byte[] 原始数据2 = 原始数据.Take(value2).ToArray();
				if (obj.是否加密)
				{
					加解密(原始数据2);
				}
				obj.填封包(原始数据2);
				剩余数据 = 原始数据.Skip(value2).ToArray();
				return obj;
			}
			return null;
		}
		return null;
	}

	private static byte[] 加解密(object 原始数据)
	{
		for (int i = 4; i < ((Array)原始数据).Length; i++)
		{
			((sbyte[])原始数据)[i] = (sbyte)(byte)(((byte[])原始数据)[i] ^ 加密字节);
		}
		return (byte[])原始数据;
	}
}
