using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using 游戏服务器.模板类;

namespace 游戏服务器.数据类;

public sealed class 数据字段
{
	internal static readonly Dictionary<Type, Func<BinaryReader, 游戏数据, 数据字段, object>> 字段读取方法表;

	internal static readonly Dictionary<Type, Action<BinaryWriter, object>> 字段写入方法表;

	public string 字段名字 { get; }

	public Type 字段类型 { get; }

	public FieldInfo 字段详情 { get; }

	static 数据字段()
	{
		字段读取方法表 = new Dictionary<Type, Func<BinaryReader, 游戏数据, 数据字段, object>>
		{
			[typeof(数据监视器<int>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<int> obj22 = new 数据监视器<int>(o);
				obj22.QuietlySetValue(r.ReadInt32());
				return obj22;
			},
			[typeof(数据监视器<uint>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<uint> obj21 = new 数据监视器<uint>(o);
				obj21.QuietlySetValue(r.ReadUInt32());
				return obj21;
			},
			[typeof(数据监视器<long>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<long> obj20 = new 数据监视器<long>(o);
				obj20.QuietlySetValue(r.ReadInt64());
				return obj20;
			},
			[typeof(数据监视器<bool>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<bool> obj19 = new 数据监视器<bool>(o);
				obj19.QuietlySetValue(r.ReadBoolean());
				return obj19;
			},
			[typeof(数据监视器<byte>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<byte> obj18 = new 数据监视器<byte>(o);
				obj18.QuietlySetValue(r.ReadByte());
				return obj18;
			},
			[typeof(数据监视器<sbyte>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<sbyte> obj17 = new 数据监视器<sbyte>(o);
				obj17.QuietlySetValue(r.ReadSByte());
				return obj17;
			},
			[typeof(数据监视器<string>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<string> obj16 = new 数据监视器<string>(o);
				obj16.QuietlySetValue(r.ReadString());
				return obj16;
			},
			[typeof(数据监视器<ushort>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<ushort> obj15 = new 数据监视器<ushort>(o);
				obj15.QuietlySetValue(r.ReadUInt16());
				return obj15;
			},
			[typeof(数据监视器<Point>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<Point> obj14 = new 数据监视器<Point>(o);
				obj14.QuietlySetValue(new Point(r.ReadInt32(), r.ReadInt32()));
				return obj14;
			},
			[typeof(数据监视器<TimeSpan>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<TimeSpan> obj13 = new 数据监视器<TimeSpan>(o);
				obj13.QuietlySetValue(TimeSpan.FromTicks(r.ReadInt64()));
				return obj13;
			},
			[typeof(数据监视器<DateTime>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<DateTime> obj12 = new 数据监视器<DateTime>(o);
				obj12.QuietlySetValue(DateTime.FromBinary(r.ReadInt64()));
				return obj12;
			},
			[typeof(数据监视器<随机属性>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<随机属性> obj11 = new 数据监视器<随机属性>(o);
				obj11.QuietlySetValue(随机属性.数据表.TryGetValue(r.ReadInt32(), out var value9) ? value9 : null);
				return obj11;
			},
			[typeof(数据监视器<铭文技能>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<铭文技能> obj10 = new 数据监视器<铭文技能>(o);
				obj10.QuietlySetValue((!铭文技能.数据表.TryGetValue(r.ReadUInt16(), out var value8)) ? null : value8);
				return obj10;
			},
			[typeof(数据监视器<游戏物品>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<游戏物品> obj9 = new 数据监视器<游戏物品>(o);
				obj9.QuietlySetValue(游戏物品.数据表.TryGetValue(r.ReadInt32(), out var value7) ? value7 : null);
				return obj9;
			},
			[typeof(数据监视器<宠物模式>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<宠物模式> obj8 = new 数据监视器<宠物模式>(o);
				obj8.QuietlySetValue((宠物模式)r.ReadInt32());
				return obj8;
			},
			[typeof(数据监视器<攻击模式>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<攻击模式> obj7 = new 数据监视器<攻击模式>(o);
				obj7.QuietlySetValue((攻击模式)r.ReadInt32());
				return obj7;
			},
			[typeof(数据监视器<游戏方向>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<游戏方向> obj6 = new 数据监视器<游戏方向>(o);
				obj6.QuietlySetValue((游戏方向)r.ReadInt32());
				return obj6;
			},
			[typeof(数据监视器<对象发型分类>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<对象发型分类> obj5 = new 数据监视器<对象发型分类>(o);
				obj5.QuietlySetValue((对象发型分类)r.ReadInt32());
				return obj5;
			},
			[typeof(数据监视器<对象发色分类>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<对象发色分类> obj4 = new 数据监视器<对象发色分类>(o);
				obj4.QuietlySetValue((对象发色分类)r.ReadInt32());
				return obj4;
			},
			[typeof(数据监视器<对象脸型分类>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<对象脸型分类> obj3 = new 数据监视器<对象脸型分类>(o);
				obj3.QuietlySetValue((对象脸型分类)r.ReadInt32());
				return obj3;
			},
			[typeof(数据监视器<游戏对象性别>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<游戏对象性别> obj2 = new 数据监视器<游戏对象性别>(o);
				obj2.QuietlySetValue((游戏对象性别)r.ReadInt32());
				return obj2;
			},
			[typeof(数据监视器<游戏对象职业>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<游戏对象职业> obj = new 数据监视器<游戏对象职业>(o);
				obj.QuietlySetValue((游戏对象职业)r.ReadInt32());
				return obj;
			},
			[typeof(数据监视器<师门数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<师门数据> 数据监视器11 = new 数据监视器<师门数据>(o);
				数据关联表.添加任务(o, f, 数据监视器11, typeof(师门数据), r.ReadInt32());
				return 数据监视器11;
			},
			[typeof(数据监视器<行会数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<行会数据> 数据监视器10 = new 数据监视器<行会数据>(o);
				数据关联表.添加任务(o, f, 数据监视器10, typeof(行会数据), r.ReadInt32());
				return 数据监视器10;
			},
			[typeof(数据监视器<队伍数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<队伍数据> 数据监视器9 = new 数据监视器<队伍数据>(o);
				数据关联表.添加任务(o, f, 数据监视器9, typeof(队伍数据), r.ReadInt32());
				return 数据监视器9;
			},
			[typeof(数据监视器<Buff数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<Buff数据> 数据监视器8 = new 数据监视器<Buff数据>(o);
				数据关联表.添加任务(o, f, 数据监视器8, typeof(Buff数据), r.ReadInt32());
				return 数据监视器8;
			},
			[typeof(数据监视器<邮件数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<邮件数据> 数据监视器7 = new 数据监视器<邮件数据>(o);
				数据关联表.添加任务(o, f, 数据监视器7, typeof(邮件数据), r.ReadInt32());
				return 数据监视器7;
			},
			[typeof(数据监视器<账号数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<账号数据> 数据监视器6 = new 数据监视器<账号数据>(o);
				数据关联表.添加任务(o, f, 数据监视器6, typeof(账号数据), r.ReadInt32());
				return 数据监视器6;
			},
			[typeof(数据监视器<角色数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<角色数据> 数据监视器5 = new 数据监视器<角色数据>(o);
				数据关联表.添加任务(o, f, 数据监视器5, typeof(角色数据), r.ReadInt32());
				return 数据监视器5;
			},
			[typeof(数据监视器<装备数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<装备数据> 数据监视器4 = new 数据监视器<装备数据>(o);
				数据关联表.添加任务(o, f, 数据监视器4, typeof(装备数据), r.ReadInt32());
				return 数据监视器4;
			},
			[typeof(数据监视器<物品数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				数据监视器<物品数据> 数据监视器3 = new 数据监视器<物品数据>(o);
				数据关联表.添加任务(o, f, 数据监视器3, (!r.ReadBoolean()) ? typeof(物品数据) : typeof(装备数据), r.ReadInt32());
				return 数据监视器3;
			},
			[typeof(列表监视器<int>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<int> 列表监视器21 = new 列表监视器<int>(o);
				int num62 = r.ReadInt32();
				for (int num63 = 0; num63 < num62; num63++)
				{
					列表监视器21.QuietlyAdd(r.ReadInt32());
				}
				return 列表监视器21;
			},
			[typeof(列表监视器<uint>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<uint> 列表监视器20 = new 列表监视器<uint>(o);
				int num60 = r.ReadInt32();
				for (int num61 = 0; num61 < num60; num61++)
				{
					列表监视器20.QuietlyAdd(r.ReadUInt32());
				}
				return 列表监视器20;
			},
			[typeof(列表监视器<bool>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<bool> 列表监视器19 = new 列表监视器<bool>(o);
				int num58 = r.ReadInt32();
				for (int num59 = 0; num59 < num58; num59++)
				{
					列表监视器19.QuietlyAdd(r.ReadBoolean());
				}
				return 列表监视器19;
			},
			[typeof(列表监视器<byte>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<byte> 列表监视器18 = new 列表监视器<byte>(o);
				int num56 = r.ReadInt32();
				for (int num57 = 0; num57 < num56; num57++)
				{
					列表监视器18.QuietlyAdd(r.ReadByte());
				}
				return 列表监视器18;
			},
			[typeof(列表监视器<角色数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<角色数据> 列表监视器17 = new 列表监视器<角色数据>(o);
				int num54 = r.ReadInt32();
				for (int num55 = 0; num55 < num54; num55++)
				{
					数据关联表.添加任务(o, f, 列表监视器17.IList, typeof(角色数据), r.ReadInt32());
				}
				return 列表监视器17;
			},
			[typeof(列表监视器<宠物数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<宠物数据> 列表监视器16 = new 列表监视器<宠物数据>(o);
				int num52 = r.ReadInt32();
				for (int num53 = 0; num53 < num52; num53++)
				{
					数据关联表.添加任务(o, f, 列表监视器16.IList, typeof(宠物数据), r.ReadInt32());
				}
				return 列表监视器16;
			},
			[typeof(列表监视器<行会数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<行会数据> 列表监视器15 = new 列表监视器<行会数据>(o);
				int num50 = r.ReadInt32();
				for (int num51 = 0; num51 < num50; num51++)
				{
					数据关联表.添加任务(o, f, 列表监视器15.IList, typeof(行会数据), r.ReadInt32());
				}
				return 列表监视器15;
			},
			[typeof(列表监视器<行会事记>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<行会事记> 列表监视器14 = new 列表监视器<行会事记>(o);
				int num48 = r.ReadInt32();
				for (int num49 = 0; num49 < num48; num49++)
				{
					列表监视器14.QuietlyAdd(new 行会事记
					{
						事记类型 = (事记类型)r.ReadByte(),
						第一参数 = r.ReadInt32(),
						第二参数 = r.ReadInt32(),
						第三参数 = r.ReadInt32(),
						第四参数 = r.ReadInt32(),
						事记时间 = r.ReadInt32()
					});
				}
				return 列表监视器14;
			},
			[typeof(列表监视器<随机属性>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<随机属性> 列表监视器13 = new 列表监视器<随机属性>(o);
				int num46 = r.ReadInt32();
				for (int num47 = 0; num47 < num46; num47++)
				{
					if (随机属性.数据表.TryGetValue(r.ReadInt32(), out var value6))
					{
						列表监视器13.QuietlyAdd(value6);
					}
				}
				return 列表监视器13;
			},
			[typeof(列表监视器<装备孔洞颜色>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				列表监视器<装备孔洞颜色> 列表监视器12 = new 列表监视器<装备孔洞颜色>(o);
				int num44 = r.ReadInt32();
				for (int num45 = 0; num45 < num44; num45++)
				{
					列表监视器12.QuietlyAdd((装备孔洞颜色)r.ReadInt32());
				}
				return 列表监视器12;
			},
			[typeof(哈希监视器<宠物数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				哈希监视器<宠物数据> 哈希监视器7 = new 哈希监视器<宠物数据>(o);
				int num42 = r.ReadInt32();
				for (int num43 = 0; num43 < num42; num43++)
				{
					数据关联表.添加任务(o, f, 哈希监视器7.ISet, r.ReadInt32());
				}
				return 哈希监视器7;
			},
			[typeof(哈希监视器<角色数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				哈希监视器<角色数据> 哈希监视器6 = new 哈希监视器<角色数据>(o);
				int num40 = r.ReadInt32();
				for (int num41 = 0; num41 < num40; num41++)
				{
					数据关联表.添加任务(o, f, 哈希监视器6.ISet, r.ReadInt32());
				}
				return 哈希监视器6;
			},
			[typeof(哈希监视器<邮件数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				哈希监视器<邮件数据> 哈希监视器5 = new 哈希监视器<邮件数据>(o);
				int num38 = r.ReadInt32();
				for (int num39 = 0; num39 < num38; num39++)
				{
					数据关联表.添加任务(o, f, 哈希监视器5.ISet, r.ReadInt32());
				}
				return 哈希监视器5;
			},
			[typeof(字典监视器<byte, int>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, int> 字典监视器39 = new 字典监视器<byte, int>(o);
				int num36 = r.ReadInt32();
				for (int num37 = 0; num37 < num36; num37++)
				{
					byte key10 = r.ReadByte();
					int value5 = r.ReadInt32();
					字典监视器39.QuietlyAdd(key10, value5);
				}
				return 字典监视器39;
			},
			[typeof(字典监视器<int, int>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<int, int> 字典监视器38 = new 字典监视器<int, int>(o);
				int num34 = r.ReadInt32();
				for (int num35 = 0; num35 < num34; num35++)
				{
					int key9 = r.ReadInt32();
					int value4 = r.ReadInt32();
					字典监视器38.QuietlyAdd(key9, value4);
				}
				return 字典监视器38;
			},
			[typeof(字典监视器<int, DateTime>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<int, DateTime> 字典监视器37 = new 字典监视器<int, DateTime>(o);
				int num32 = r.ReadInt32();
				for (int num33 = 0; num33 < num32; num33++)
				{
					int key8 = r.ReadInt32();
					long dateData6 = r.ReadInt64();
					字典监视器37.QuietlyAdd(key8, DateTime.FromBinary(dateData6));
				}
				return 字典监视器37;
			},
			[typeof(字典监视器<byte, DateTime>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, DateTime> 字典监视器36 = new 字典监视器<byte, DateTime>(o);
				int num30 = r.ReadInt32();
				for (int num31 = 0; num31 < num30; num31++)
				{
					byte key7 = r.ReadByte();
					long dateData5 = r.ReadInt64();
					字典监视器36.QuietlyAdd(key7, DateTime.FromBinary(dateData5));
				}
				return 字典监视器36;
			},
			[typeof(字典监视器<string, DateTime>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<string, DateTime> 字典监视器35 = new 字典监视器<string, DateTime>(o);
				int num28 = r.ReadInt32();
				for (int num29 = 0; num29 < num28; num29++)
				{
					string key6 = r.ReadString();
					long dateData4 = r.ReadInt64();
					字典监视器35.QuietlyAdd(key6, DateTime.FromBinary(dateData4));
				}
				return 字典监视器35;
			},
			[typeof(字典监视器<byte, 游戏物品>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, 游戏物品> 字典监视器34 = new 字典监视器<byte, 游戏物品>(o);
				int num26 = r.ReadInt32();
				for (int num27 = 0; num27 < num26; num27++)
				{
					byte key4 = r.ReadByte();
					int key5 = r.ReadInt32();
					if (游戏物品.数据表.TryGetValue(key5, out var value3))
					{
						字典监视器34.QuietlyAdd(key4, value3);
					}
				}
				return 字典监视器34;
			},
			[typeof(字典监视器<byte, 铭文技能>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, 铭文技能> 字典监视器33 = new 字典监视器<byte, 铭文技能>(o);
				int num24 = r.ReadInt32();
				for (int num25 = 0; num25 < num24; num25++)
				{
					byte key2 = r.ReadByte();
					ushort key3 = r.ReadUInt16();
					if (铭文技能.数据表.TryGetValue(key3, out var value2))
					{
						字典监视器33.QuietlyAdd(key2, value2);
					}
				}
				return 字典监视器33;
			},
			[typeof(字典监视器<ushort, Buff数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<ushort, Buff数据> 字典监视器32 = new 字典监视器<ushort, Buff数据>(o);
				int num21 = r.ReadInt32();
				for (int num22 = 0; num22 < num21; num22++)
				{
					ushort num23 = r.ReadUInt16();
					int 值索引8 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器32.IDictionary_0, num23, null, typeof(ushort), typeof(Buff数据), 0, 值索引8);
				}
				return 字典监视器32;
			},
			[typeof(字典监视器<ushort, 技能数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<ushort, 技能数据> 字典监视器31 = new 字典监视器<ushort, 技能数据>(o);
				int num18 = r.ReadInt32();
				for (int num19 = 0; num19 < num18; num19++)
				{
					ushort num20 = r.ReadUInt16();
					int 值索引7 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器31.IDictionary_0, num20, null, typeof(ushort), typeof(技能数据), 0, 值索引7);
				}
				return 字典监视器31;
			},
			[typeof(字典监视器<byte, 技能数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, 技能数据> 字典监视器30 = new 字典监视器<byte, 技能数据>(o);
				int num16 = r.ReadInt32();
				for (int num17 = 0; num17 < num16; num17++)
				{
					byte b4 = r.ReadByte();
					int 值索引6 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器30.IDictionary_0, b4, null, typeof(byte), typeof(技能数据), 0, 值索引6);
				}
				return 字典监视器30;
			},
			[typeof(字典监视器<byte, 装备数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, 装备数据> 字典监视器29 = new 字典监视器<byte, 装备数据>(o);
				int num14 = r.ReadInt32();
				for (int num15 = 0; num15 < num14; num15++)
				{
					byte b3 = r.ReadByte();
					int 值索引5 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器29.IDictionary_0, b3, null, typeof(byte), typeof(装备数据), 0, 值索引5);
				}
				return 字典监视器29;
			},
			[typeof(字典监视器<byte, 物品数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<byte, 物品数据> 字典监视器28 = new 字典监视器<byte, 物品数据>(o);
				int num12 = r.ReadInt32();
				for (int num13 = 0; num13 < num12; num13++)
				{
					byte b2 = r.ReadByte();
					bool flag = r.ReadBoolean();
					int 值索引4 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器28.IDictionary_0, b2, null, typeof(byte), (!flag) ? typeof(物品数据) : typeof(装备数据), 0, 值索引4);
				}
				return 字典监视器28;
			},
			[typeof(字典监视器<int, 角色数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<int, 角色数据> 字典监视器27 = new 字典监视器<int, 角色数据>(o);
				int num9 = r.ReadInt32();
				for (int num10 = 0; num10 < num9; num10++)
				{
					int num11 = r.ReadInt32();
					int 值索引3 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器27.IDictionary_0, num11, null, typeof(int), typeof(角色数据), 0, 值索引3);
				}
				return 字典监视器27;
			},
			[typeof(字典监视器<int, 邮件数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<int, 邮件数据> 字典监视器26 = new 字典监视器<int, 邮件数据>(o);
				int num7 = r.ReadInt32();
				for (int n = 0; n < num7; n++)
				{
					int num8 = r.ReadInt32();
					int 值索引2 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器26.IDictionary_0, num8, null, typeof(int), typeof(邮件数据), 0, 值索引2);
				}
				return 字典监视器26;
			},
			[typeof(字典监视器<游戏货币, int>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<游戏货币, int> 字典监视器25 = new 字典监视器<游戏货币, int>(o);
				int num6 = r.ReadInt32();
				for (int m = 0; m < num6; m++)
				{
					int key = r.ReadInt32();
					int value = r.ReadInt32();
					字典监视器25.QuietlyAdd((游戏货币)key, value);
				}
				return 字典监视器25;
			},
			[typeof(字典监视器<行会数据, DateTime>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<行会数据, DateTime> 字典监视器24 = new 字典监视器<行会数据, DateTime>(o);
				int num5 = r.ReadInt32();
				for (int l = 0; l < num5; l++)
				{
					int 键索引3 = r.ReadInt32();
					long dateData3 = r.ReadInt64();
					数据关联表.添加任务(o, f, 字典监视器24.IDictionary_0, null, DateTime.FromBinary(dateData3), typeof(行会数据), typeof(DateTime), 键索引3, 0);
				}
				return 字典监视器24;
			},
			[typeof(字典监视器<角色数据, DateTime>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<角色数据, DateTime> 字典监视器23 = new 字典监视器<角色数据, DateTime>(o);
				int num4 = r.ReadInt32();
				for (int k = 0; k < num4; k++)
				{
					int 键索引2 = r.ReadInt32();
					long dateData2 = r.ReadInt64();
					数据关联表.添加任务(o, f, 字典监视器23.IDictionary_0, null, DateTime.FromBinary(dateData2), typeof(角色数据), typeof(DateTime), 键索引2, 0);
				}
				return 字典监视器23;
			},
			[typeof(字典监视器<角色数据, 行会职位>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<角色数据, 行会职位> 字典监视器22 = new 字典监视器<角色数据, 行会职位>(o);
				int num2 = r.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					int 键索引 = r.ReadInt32();
					int num3 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器22.IDictionary_0, null, (行会职位)num3, typeof(角色数据), typeof(行会职位), 键索引, 0);
				}
				return 字典监视器22;
			},
			[typeof(字典监视器<DateTime, 行会数据>)] = delegate(BinaryReader r, 游戏数据 o, 数据字段 f)
			{
				字典监视器<DateTime, 行会数据> 字典监视器21 = new 字典监视器<DateTime, 行会数据>(o);
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					long dateData = r.ReadInt64();
					int 值索引 = r.ReadInt32();
					数据关联表.添加任务(o, f, 字典监视器21.IDictionary_0, DateTime.FromBinary(dateData), null, typeof(DateTime), typeof(行会数据), 0, 值索引);
				}
				return 字典监视器21;
			}
		};
		字段写入方法表 = new Dictionary<Type, Action<BinaryWriter, object>>
		{
			[typeof(数据监视器<int>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<int>)o).V);
			},
			[typeof(数据监视器<uint>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<uint>)o).V);
			},
			[typeof(数据监视器<long>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<long>)o).V);
			},
			[typeof(数据监视器<bool>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<bool>)o).V);
			},
			[typeof(数据监视器<byte>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<byte>)o).V);
			},
			[typeof(数据监视器<sbyte>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<sbyte>)o).V);
			},
			[typeof(数据监视器<string>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<string>)o).V ?? "");
			},
			[typeof(数据监视器<ushort>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<ushort>)o).V);
			},
			[typeof(数据监视器<Point>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<Point>)o).V.X);
				b.Write(((数据监视器<Point>)o).V.Y);
			},
			[typeof(数据监视器<TimeSpan>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<TimeSpan>)o).V.Ticks);
			},
			[typeof(数据监视器<DateTime>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<DateTime>)o).V.ToBinary());
			},
			[typeof(数据监视器<随机属性>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<随机属性>)o).V?.属性编号 ?? 0);
			},
			[typeof(数据监视器<铭文技能>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<铭文技能>)o).V?.铭文索引 ?? 0);
			},
			[typeof(数据监视器<游戏物品>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<游戏物品>)o).V?.物品编号 ?? 0);
			},
			[typeof(数据监视器<宠物模式>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<宠物模式>)o).V);
			},
			[typeof(数据监视器<攻击模式>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<攻击模式>)o).V);
			},
			[typeof(数据监视器<游戏方向>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<游戏方向>)o).V);
			},
			[typeof(数据监视器<对象发型分类>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<对象发型分类>)o).V);
			},
			[typeof(数据监视器<对象发色分类>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<对象发色分类>)o).V);
			},
			[typeof(数据监视器<对象脸型分类>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<对象脸型分类>)o).V);
			},
			[typeof(数据监视器<游戏对象性别>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<游戏对象性别>)o).V);
			},
			[typeof(数据监视器<游戏对象职业>)] = delegate(BinaryWriter b, object o)
			{
				b.Write((int)((数据监视器<游戏对象职业>)o).V);
			},
			[typeof(数据监视器<师门数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<师门数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<行会数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<行会数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<队伍数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<队伍数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<Buff数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<Buff数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<邮件数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<邮件数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<账号数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<账号数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<角色数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<角色数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<装备数据>)] = delegate(BinaryWriter b, object o)
			{
				b.Write(((数据监视器<装备数据>)o).V?.数据索引.V ?? 0);
			},
			[typeof(数据监视器<物品数据>)] = delegate(BinaryWriter b, object o)
			{
				数据监视器<物品数据> 数据监视器2 = (数据监视器<物品数据>)o;
				b.Write(数据监视器2.V is 装备数据);
				b.Write(数据监视器2.V?.数据索引.V ?? 0);
			},
			[typeof(列表监视器<int>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<int> 列表监视器11 = (列表监视器<int>)o;
				b.Write(列表监视器11?.Count ?? 0);
				foreach (int item in 列表监视器11)
				{
					b.Write(item);
				}
			},
			[typeof(列表监视器<uint>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<uint> 列表监视器10 = (列表监视器<uint>)o;
				b.Write(列表监视器10?.Count ?? 0);
				foreach (uint item2 in 列表监视器10)
				{
					b.Write(item2);
				}
			},
			[typeof(列表监视器<bool>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<bool> 列表监视器9 = (列表监视器<bool>)o;
				b.Write(列表监视器9?.Count ?? 0);
				foreach (bool item3 in 列表监视器9)
				{
					b.Write(item3);
				}
			},
			[typeof(列表监视器<byte>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<byte> 列表监视器8 = (列表监视器<byte>)o;
				b.Write(列表监视器8?.Count ?? 0);
				foreach (byte item4 in 列表监视器8)
				{
					b.Write(item4);
				}
			},
			[typeof(列表监视器<角色数据>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<角色数据> 列表监视器7 = (列表监视器<角色数据>)o;
				b.Write(列表监视器7?.Count ?? 0);
				foreach (角色数据 item5 in 列表监视器7)
				{
					b.Write(item5.数据索引.V);
				}
			},
			[typeof(列表监视器<宠物数据>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<宠物数据> 列表监视器6 = (列表监视器<宠物数据>)o;
				b.Write(列表监视器6?.Count ?? 0);
				foreach (宠物数据 item6 in 列表监视器6)
				{
					b.Write(item6.数据索引.V);
				}
			},
			[typeof(列表监视器<行会数据>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<行会数据> 列表监视器5 = (列表监视器<行会数据>)o;
				b.Write(列表监视器5?.Count ?? 0);
				foreach (行会数据 item7 in 列表监视器5)
				{
					b.Write(item7.数据索引.V);
				}
			},
			[typeof(列表监视器<行会事记>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<行会事记> 列表监视器4 = (列表监视器<行会事记>)o;
				b.Write(列表监视器4?.Count ?? 0);
				foreach (行会事记 item8 in 列表监视器4)
				{
					b.Write((byte)item8.事记类型);
					b.Write(item8.第一参数);
					b.Write(item8.第二参数);
					b.Write(item8.第三参数);
					b.Write(item8.第四参数);
					b.Write(item8.事记时间);
				}
			},
			[typeof(列表监视器<随机属性>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<随机属性> 列表监视器3 = (列表监视器<随机属性>)o;
				b.Write(列表监视器3?.Count ?? 0);
				foreach (随机属性 item9 in 列表监视器3)
				{
					b.Write(item9.属性编号);
				}
			},
			[typeof(列表监视器<装备孔洞颜色>)] = delegate(BinaryWriter b, object o)
			{
				列表监视器<装备孔洞颜色> 列表监视器2 = (列表监视器<装备孔洞颜色>)o;
				b.Write(列表监视器2?.Count ?? 0);
				foreach (装备孔洞颜色 item10 in 列表监视器2)
				{
					b.Write((int)item10);
				}
			},
			[typeof(哈希监视器<宠物数据>)] = delegate(BinaryWriter b, object o)
			{
				哈希监视器<宠物数据> 哈希监视器4 = (哈希监视器<宠物数据>)o;
				b.Write(哈希监视器4?.Count ?? 0);
				foreach (宠物数据 item11 in 哈希监视器4)
				{
					b.Write(item11.数据索引.V);
				}
			},
			[typeof(哈希监视器<角色数据>)] = delegate(BinaryWriter b, object o)
			{
				哈希监视器<角色数据> 哈希监视器3 = (哈希监视器<角色数据>)o;
				b.Write(哈希监视器3?.Count ?? 0);
				foreach (角色数据 item12 in 哈希监视器3)
				{
					b.Write(item12.数据索引.V);
				}
			},
			[typeof(哈希监视器<邮件数据>)] = delegate(BinaryWriter b, object o)
			{
				哈希监视器<邮件数据> 哈希监视器2 = (哈希监视器<邮件数据>)o;
				b.Write(哈希监视器2?.Count ?? 0);
				foreach (邮件数据 item13 in 哈希监视器2)
				{
					b.Write(item13.数据索引.V);
				}
			},
			[typeof(字典监视器<byte, int>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, int> 字典监视器20 = (字典监视器<byte, int>)o;
				b.Write(字典监视器20?.Count ?? 0);
				foreach (KeyValuePair<byte, int> item14 in 字典监视器20)
				{
					b.Write(item14.Key);
					b.Write(item14.Value);
				}
			},
			[typeof(字典监视器<int, int>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<int, int> 字典监视器19 = (字典监视器<int, int>)o;
				b.Write(字典监视器19?.Count ?? 0);
				foreach (KeyValuePair<int, int> item15 in 字典监视器19)
				{
					b.Write(item15.Key);
					b.Write(item15.Value);
				}
			},
			[typeof(字典监视器<int, DateTime>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<int, DateTime> 字典监视器18 = (字典监视器<int, DateTime>)o;
				b.Write(字典监视器18?.Count ?? 0);
				foreach (KeyValuePair<int, DateTime> item16 in 字典监视器18)
				{
					b.Write(item16.Key);
					b.Write(item16.Value.ToBinary());
				}
			},
			[typeof(字典监视器<byte, DateTime>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, DateTime> 字典监视器17 = (字典监视器<byte, DateTime>)o;
				b.Write(字典监视器17?.Count ?? 0);
				foreach (KeyValuePair<byte, DateTime> item17 in 字典监视器17)
				{
					b.Write(item17.Key);
					b.Write(item17.Value.ToBinary());
				}
			},
			[typeof(字典监视器<string, DateTime>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<string, DateTime> 字典监视器16 = (字典监视器<string, DateTime>)o;
				b.Write(字典监视器16?.Count ?? 0);
				foreach (KeyValuePair<string, DateTime> item18 in 字典监视器16)
				{
					b.Write(item18.Key);
					b.Write(item18.Value.ToBinary());
				}
			},
			[typeof(字典监视器<byte, 游戏物品>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, 游戏物品> 字典监视器15 = (字典监视器<byte, 游戏物品>)o;
				b.Write(字典监视器15?.Count ?? 0);
				foreach (KeyValuePair<byte, 游戏物品> item19 in 字典监视器15)
				{
					b.Write(item19.Key);
					b.Write(item19.Value.物品编号);
				}
			},
			[typeof(字典监视器<byte, 铭文技能>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, 铭文技能> 字典监视器14 = (字典监视器<byte, 铭文技能>)o;
				b.Write(字典监视器14?.Count ?? 0);
				foreach (KeyValuePair<byte, 铭文技能> item20 in 字典监视器14)
				{
					b.Write(item20.Key);
					b.Write(item20.Value.铭文索引);
				}
			},
			[typeof(字典监视器<ushort, Buff数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<ushort, Buff数据> 字典监视器13 = (字典监视器<ushort, Buff数据>)o;
				b.Write(字典监视器13?.Count ?? 0);
				foreach (KeyValuePair<ushort, Buff数据> item21 in 字典监视器13)
				{
					b.Write(item21.Key);
					b.Write(item21.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<ushort, 技能数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<ushort, 技能数据> 字典监视器12 = (字典监视器<ushort, 技能数据>)o;
				b.Write(字典监视器12?.Count ?? 0);
				foreach (KeyValuePair<ushort, 技能数据> item22 in 字典监视器12)
				{
					b.Write(item22.Key);
					b.Write(item22.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<byte, 技能数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, 技能数据> 字典监视器11 = (字典监视器<byte, 技能数据>)o;
				b.Write(字典监视器11?.Count ?? 0);
				foreach (KeyValuePair<byte, 技能数据> item23 in 字典监视器11)
				{
					b.Write(item23.Key);
					b.Write(item23.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<byte, 装备数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, 装备数据> 字典监视器10 = (字典监视器<byte, 装备数据>)o;
				b.Write(字典监视器10?.Count ?? 0);
				foreach (KeyValuePair<byte, 装备数据> item24 in 字典监视器10)
				{
					b.Write(item24.Key);
					b.Write(item24.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<byte, 物品数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<byte, 物品数据> 字典监视器9 = (字典监视器<byte, 物品数据>)o;
				b.Write(字典监视器9?.Count ?? 0);
				foreach (KeyValuePair<byte, 物品数据> item25 in 字典监视器9)
				{
					b.Write(item25.Key);
					b.Write(item25.Value is 装备数据);
					b.Write(item25.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<int, 角色数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<int, 角色数据> 字典监视器8 = (字典监视器<int, 角色数据>)o;
				b.Write(字典监视器8?.Count ?? 0);
				foreach (KeyValuePair<int, 角色数据> item26 in 字典监视器8)
				{
					b.Write(item26.Key);
					b.Write(item26.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<int, 邮件数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<int, 邮件数据> 字典监视器7 = (字典监视器<int, 邮件数据>)o;
				b.Write(字典监视器7?.Count ?? 0);
				foreach (KeyValuePair<int, 邮件数据> item27 in 字典监视器7)
				{
					b.Write(item27.Key);
					b.Write(item27.Value.数据索引.V);
				}
			},
			[typeof(字典监视器<游戏货币, int>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<游戏货币, int> 字典监视器6 = (字典监视器<游戏货币, int>)o;
				b.Write(字典监视器6?.Count ?? 0);
				foreach (KeyValuePair<游戏货币, int> item28 in 字典监视器6)
				{
					b.Write((int)item28.Key);
					b.Write(item28.Value);
				}
			},
			[typeof(字典监视器<行会数据, DateTime>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<行会数据, DateTime> 字典监视器5 = (字典监视器<行会数据, DateTime>)o;
				b.Write(字典监视器5?.Count ?? 0);
				foreach (KeyValuePair<行会数据, DateTime> item29 in 字典监视器5)
				{
					b.Write(item29.Key.数据索引.V);
					b.Write(item29.Value.ToBinary());
				}
			},
			[typeof(字典监视器<角色数据, DateTime>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<角色数据, DateTime> 字典监视器4 = (字典监视器<角色数据, DateTime>)o;
				b.Write(字典监视器4?.Count ?? 0);
				foreach (KeyValuePair<角色数据, DateTime> item30 in 字典监视器4)
				{
					b.Write(item30.Key.数据索引.V);
					b.Write(item30.Value.ToBinary());
				}
			},
			[typeof(字典监视器<角色数据, 行会职位>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<角色数据, 行会职位> 字典监视器3 = (字典监视器<角色数据, 行会职位>)o;
				b.Write(字典监视器3?.Count ?? 0);
				foreach (KeyValuePair<角色数据, 行会职位> item31 in 字典监视器3)
				{
					b.Write(item31.Key.数据索引.V);
					b.Write((int)item31.Value);
				}
			},
			[typeof(字典监视器<DateTime, 行会数据>)] = delegate(BinaryWriter b, object o)
			{
				字典监视器<DateTime, 行会数据> 字典监视器2 = (字典监视器<DateTime, 行会数据>)o;
				b.Write(字典监视器2?.Count ?? 0);
				foreach (KeyValuePair<DateTime, 行会数据> item32 in 字典监视器2)
				{
					b.Write(item32.Key.ToBinary());
					b.Write(item32.Value.数据索引.V);
				}
			}
		};
	}

	public override string ToString()
	{
		return 字段名字;
	}

	public 数据字段(BinaryReader 读取流, Type 数据类型)
	{
		字段名字 = 读取流.ReadString();
		字段类型 = Type.GetType(读取流.ReadString());
		字段详情 = 数据类型?.GetField(字段名字);
	}

	public 数据字段(FieldInfo 当前字段)
	{
		字段详情 = 当前字段;
		字段名字 = 当前字段.Name;
		字段类型 = 当前字段.FieldType;
	}

	public bool 检查字段版本(数据字段 对比字段)
	{
		if (string.Compare(字段名字, 对比字段.字段名字, StringComparison.Ordinal) == 0)
		{
			return 字段类型 == 对比字段.字段类型;
		}
		return false;
	}

	public void 保存字段描述(BinaryWriter 写入流)
	{
		写入流.Write(字段名字);
		写入流.Write(字段类型.FullName);
	}

	public void 保存字段内容(BinaryWriter 写入流, object 数据)
	{
		if (字段写入方法表.ContainsKey(字段类型))
		{
			字段写入方法表[字段类型](写入流, 数据);
		}
	}

	public object 读取字段内容(BinaryReader 读取流, object 数据, 数据字段 字段)
	{
		return 字段读取方法表[字段类型](读取流, (游戏数据)数据, 字段);
	}
}
