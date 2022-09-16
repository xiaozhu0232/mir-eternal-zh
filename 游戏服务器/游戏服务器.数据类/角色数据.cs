using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using 游戏服务器.地图类;
using 游戏服务器.模板类;
using 游戏服务器.网络类;

namespace 游戏服务器.数据类;

[数据快速检索(检索字段 = "角色名字")]
public sealed class 角色数据 : 游戏数据
{
	public readonly 数据监视器<string> 角色名字;

	public readonly 数据监视器<string> 网络地址;

	public readonly 数据监视器<string> 物理地址;

	public readonly 数据监视器<DateTime> 创建日期;

	public readonly 数据监视器<DateTime> 登录日期;

	public readonly 数据监视器<DateTime> 冻结日期;

	public readonly 数据监视器<DateTime> 删除日期;

	public readonly 数据监视器<DateTime> 离线日期;

	public readonly 数据监视器<DateTime> 监禁日期;

	public readonly 数据监视器<DateTime> 封禁日期;

	public readonly 数据监视器<TimeSpan> 灰名时间;

	public readonly 数据监视器<TimeSpan> 减PK时间;

	public readonly 数据监视器<DateTime> 武斗日期;

	public readonly 数据监视器<DateTime> 攻沙日期;

	public readonly 数据监视器<DateTime> 领奖日期;

	public readonly 数据监视器<DateTime> 屠魔大厅;

	public readonly 数据监视器<DateTime> 屠魔兑换;

	public readonly 数据监视器<int> 屠魔次数;

	public readonly 数据监视器<DateTime> 分解日期;

	public readonly 数据监视器<int> 分解经验;

	public readonly 数据监视器<游戏对象职业> 角色职业;

	public readonly 数据监视器<游戏对象性别> 角色性别;

	public readonly 数据监视器<对象发型分类> 角色发型;

	public readonly 数据监视器<对象发色分类> 角色发色;

	public readonly 数据监视器<对象脸型分类> 角色脸型;

	public readonly 数据监视器<int> 当前血量;

	public readonly 数据监视器<int> 当前蓝量;

	public readonly 数据监视器<byte> 当前等级;

	public readonly 数据监视器<int> 当前经验;

	public readonly 数据监视器<int> 双倍经验;

	public readonly 数据监视器<int> 当前战力;

	public readonly 数据监视器<int> 当前PK值;

	public readonly 数据监视器<int> 当前地图;

	public readonly 数据监视器<int> 重生地图;

	public readonly 数据监视器<Point> 当前坐标;

	public readonly 数据监视器<游戏方向> 当前朝向;

	public readonly 数据监视器<攻击模式> 攻击模式;

	public readonly 数据监视器<宠物模式> 宠物模式;

	public readonly 哈希监视器<宠物数据> 宠物数据;

	public readonly 数据监视器<byte> 背包大小;

	public readonly 数据监视器<byte> 仓库大小;

	public readonly 数据监视器<byte> 资源背包大小;

	public readonly 数据监视器<long> 消耗元宝;

	public readonly 数据监视器<long> 转出金币;

	public readonly 列表监视器<uint> 玩家设置;

	public readonly 数据监视器<装备数据> 升级装备;

	public readonly 数据监视器<DateTime> 取回时间;

	public readonly 数据监视器<bool> 升级成功;

	public readonly 数据监视器<byte> 当前称号;

	public readonly 字典监视器<byte, int> 历史排名;

	public readonly 字典监视器<byte, int> 当前排名;

	public readonly 字典监视器<byte, DateTime> 称号列表;

	public readonly 字典监视器<游戏货币, int> 角色货币;

	public readonly 字典监视器<byte, 物品数据> 角色背包;

	public readonly 字典监视器<byte, 物品数据> 角色仓库;

	public readonly 字典监视器<byte, 物品数据> 角色资源背包;

	public readonly 字典监视器<byte, 装备数据> 角色装备;

	public readonly 字典监视器<byte, 技能数据> 快捷栏位;

	public readonly 字典监视器<ushort, Buff数据> Buff数据;

	public readonly 字典监视器<ushort, 技能数据> 技能数据;

	public readonly 字典监视器<int, DateTime> 冷却数据;

	public readonly 哈希监视器<邮件数据> 角色邮件;

	public readonly 哈希监视器<邮件数据> 未读邮件;

	public readonly 数据监视器<byte> 预定特权;

	public readonly 数据监视器<byte> 本期特权;

	public readonly 数据监视器<byte> 上期特权;

	public readonly 数据监视器<uint> 本期记录;

	public readonly 数据监视器<uint> 上期记录;

	public readonly 数据监视器<DateTime> 本期日期;

	public readonly 数据监视器<DateTime> 上期日期;

	public readonly 数据监视器<DateTime> 补给日期;

	public readonly 数据监视器<DateTime> 战备日期;

	public readonly 字典监视器<byte, int> 剩余特权;

	public readonly 数据监视器<账号数据> 所属账号;

	public readonly 数据监视器<队伍数据> 所属队伍;

	public readonly 数据监视器<行会数据> 所属行会;

	public readonly 数据监视器<师门数据> 所属师门;

	public readonly 哈希监视器<角色数据> 好友列表;

	public readonly 哈希监视器<角色数据> 偶像列表;

	public readonly 哈希监视器<角色数据> 粉丝列表;

	public readonly 哈希监视器<角色数据> 仇人列表;

	public readonly 哈希监视器<角色数据> 仇恨列表;

	public readonly 哈希监视器<角色数据> 黑名单表;

	public int 角色编号 => 数据索引.V;

	public int 角色经验
	{
		get
		{
			return 当前经验.V;
		}
		set
		{
			当前经验.V = value;
		}
	}

	public byte 角色等级
	{
		get
		{
			return 当前等级.V;
		}
		set
		{
			if (当前等级.V != value)
			{
				当前等级.V = value;
				系统数据.数据.更新等级(this);
			}
		}
	}

	public int 角色战力
	{
		get
		{
			return 当前战力.V;
		}
		set
		{
			if (当前战力.V != value)
			{
				当前战力.V = value;
				系统数据.数据.更新战力(this);
			}
		}
	}

	public int 角色PK值
	{
		get
		{
			return 当前PK值.V;
		}
		set
		{
			if (当前PK值.V != value)
			{
				当前PK值.V = value;
				系统数据.数据.更新PK值(this);
			}
		}
	}

	public int 所需经验 => 角色成长.升级所需经验[角色等级];

	public int 元宝数量
	{
		get
		{
			if (!角色货币.TryGetValue(游戏货币.元宝, out var v))
			{
				return 0;
			}
			return v;
		}
		set
		{
			角色货币[游戏货币.元宝] = value;
			主窗口.更新角色数据(this, "元宝数量", value);
		}
	}

	public int 金币数量
	{
		get
		{
			if (!角色货币.TryGetValue(游戏货币.金币, out var v))
			{
				return 0;
			}
			return v;
		}
		set
		{
			角色货币[游戏货币.金币] = value;
			主窗口.更新角色数据(this, "金币数量", value);
		}
	}

	public int 师门声望
	{
		get
		{
			if (!角色货币.TryGetValue(游戏货币.名师声望, out var v))
			{
				return 0;
			}
			return v;
		}
		set
		{
			角色货币[游戏货币.名师声望] = value;
			主窗口.更新角色数据(this, "师门声望", value);
		}
	}

	public byte 师门参数
	{
		get
		{
			if (当前师门 != null)
			{
				if (当前师门.师父编号 == 角色编号)
				{
					return 2;
				}
				return 1;
			}
			if (角色等级 < 30)
			{
				return 0;
			}
			return 2;
		}
	}

	public 队伍数据 当前队伍
	{
		get
		{
			return 所属队伍.V;
		}
		set
		{
			if (所属队伍.V != value)
			{
				所属队伍.V = value;
			}
		}
	}

	public 师门数据 当前师门
	{
		get
		{
			return 所属师门.V;
		}
		set
		{
			if (所属师门.V != value)
			{
				所属师门.V = value;
			}
		}
	}

	public 行会数据 当前行会
	{
		get
		{
			return 所属行会.V;
		}
		set
		{
			if (所属行会.V != value)
			{
				所属行会.V = value;
			}
		}
	}

	public 客户网络 网络连接 { get; set; }

	public void 获得经验(int 经验值)
	{
		if ((角色等级 < 自定义类.游戏开放等级 || 角色经验 < 所需经验) && (角色经验 += 经验值) > 所需经验 && 角色等级 < 自定义类.游戏开放等级)
		{
			while (角色经验 >= 所需经验)
			{
				角色经验 -= 所需经验;
				角色等级++;
			}
		}
	}

	public void 角色下线()
	{
		网络连接.绑定角色 = null;
		网络连接 = null;
		网络服务网关.已上线连接数--;
		离线日期.V = 主程.当前时间;
		主窗口.更新角色数据(this, "离线日期", 离线日期);
	}

	public void 角色上线(客户网络 网络)
	{
		网络连接 = 网络;
		网络服务网关.已上线连接数++;
		物理地址.V = 网络.物理地址;
		网络地址.V = 网络.网络地址;
		主窗口.更新角色数据(this, "离线日期", null);
		主窗口.添加系统日志($"玩家[{角色名字}][{当前等级}级]进入了游戏");
	}

	public void 发送邮件(邮件数据 邮件)
	{
		邮件.收件地址.V = this;
		角色邮件.Add(邮件);
		未读邮件.Add(邮件);
		网络连接?.发送封包(new 未读邮件提醒
		{
			邮件数量 = 未读邮件.Count
		});
	}

	public bool 角色在线(out 客户网络 网络)
	{
		网络 = 网络连接;
		return 网络 != null;
	}

	public 角色数据()
	{
	}

	public 角色数据(账号数据 账号, string 名字, 游戏对象职业 职业, 游戏对象性别 性别, 对象发型分类 发型, 对象发色分类 发色, 对象脸型分类 脸型)
	{
		当前等级.V = 1;//初始等级
        背包大小.V = 64;
        仓库大小.V = 16;
		资源背包大小.V = 0;
        所属账号.V = 账号;
		角色名字.V = 名字;
		角色职业.V = 职业;
		角色性别.V = 性别;
		角色发型.V = 发型;
		角色发色.V = 发色;
		角色脸型.V = 脸型;
		创建日期.V = 主程.当前时间;
		当前血量.V = 角色成长.获取数据(职业, 1)[游戏对象属性.最大体力];
        当前蓝量.V = 角色成长.获取数据(职业, 1)[游戏对象属性.最大魔力];
		当前朝向.V = 计算类.随机方向();
		当前地图.V = 142;
		重生地图.V = 142;
		当前坐标.V = 地图处理网关.分配地图(142).复活区域.随机坐标;
		for (int i = 0; i <= 19; i++)
		{
			角色货币[(游戏货币)i] = 0;
        }
		角色货币[(游戏货币)1] = 10000;//初始金币       
        玩家设置.SetValue(new uint[128].ToList());
		if (游戏物品.检索表.TryGetValue("金创药(小)包", out var value))
		{
			角色背包[0] = new 物品数据(value, this, 1, 0, 1);
		}
		if (游戏物品.检索表.TryGetValue("魔法药(小)包", out var value2))
		{
			角色背包[1] = new 物品数据(value2, this, 1, 1, 1);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.战士) ? "木剑" : " ", out var value3) && value3 is 游戏装备 模板)
		{
			角色装备[0] = new 装备数据(模板, this, 0, 0);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.法师) ? "木剑" : " ", out value3) && value3 is 游戏装备 模板2)
		{
			角色装备[0] = new 装备数据(模板2, this, 0, 0);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.道士) ? "木剑" : " ", out value3) && value3 is 游戏装备 模板3)
		{
			角色装备[0] = new 装备数据(模板3, this, 0, 0);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.刺客) ? "柴刀" : " ", out value3) && value3 is 游戏装备 模板4)
		{
			角色装备[0] = new 装备数据(模板4, this, 0, 0);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.弓手) ? "木弓" : " ", out value3) && value3 is 游戏装备 模板5)
		{
			角色装备[0] = new 装备数据(模板5, this, 0, 0);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.弓手) ? "守护箭袋" : " ", out value3) && value3 is 游戏装备 模板6)
		{
			角色装备[2] = new 装备数据(模板6, this, 0, 15);
		}
		if (游戏物品.检索表.TryGetValue((职业 == 游戏对象职业.龙枪) ? "木枪" : " ", out value3) && value3 is 游戏装备 模板7)
		{
			角色装备[0] = new 装备数据(模板7, this, 0, 0);
		}
		if (游戏物品.检索表.TryGetValue((性别 == 游戏对象性别.男性) ? "布衣(男)" : "布衣(女)", out var value4) && value4 is 游戏装备 模板8)
		{
			角色装备[1] = new 装备数据(模板8, this, 0, 1);
		}
		Dictionary<ushort, 铭文技能> 数据表 = 铭文技能.数据表;
		if (数据表.TryGetValue((ushort)(职业 switch
		{
			游戏对象职业.战士 => 10300u, 
			游戏对象职业.法师 => 25300u, 
			游戏对象职业.刺客 => 15300u, 
			游戏对象职业.弓手 => 20400u, 
			游戏对象职业.道士 => 30000u, 
			_ => 12000u, 
		}), out var value5))
		{
			技能数据 技能数据2 = new 技能数据(value5.技能编号);
			技能数据.Add(技能数据2.技能编号.V, 技能数据2);
			快捷栏位[0] = 技能数据2;
			技能数据2.快捷栏位.V = 0;
		}
		游戏数据网关.角色数据表.添加数据(this, 分配索引: true);
		账号.角色列表.Add(this);
		加载完成();
	}

	public override string ToString()
	{
		return 角色名字?.V;
	}

	public void 订阅事件()
	{
		所属账号.更改事件 += delegate(账号数据 O)
		{
			主窗口.更新角色数据(this, "所属账号", O);
			主窗口.更新角色数据(this, "账号封禁", (O.封禁日期.V != default(DateTime)) ? O.封禁日期 : null);
		};
		所属账号.V.封禁日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "账号封禁", (O != default(DateTime)) ? ((object)O) : null);
		};
		角色名字.更改事件 += delegate(string O)
		{
			主窗口.更新角色数据(this, "角色名字", O);
		};
		封禁日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "角色封禁", (O != default(DateTime)) ? ((object)O) : null);
		};
		冻结日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "冻结日期", (O != default(DateTime)) ? ((object)O) : null);
		};
		删除日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "删除日期", (!(O != default(DateTime))) ? null : ((object)O));
		};
		登录日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "登录日期", (!(O != default(DateTime))) ? null : ((object)O));
		};
		离线日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "离线日期", (网络连接 == null) ? ((object)O) : null);
		};
		网络地址.更改事件 += delegate(string O)
		{
			主窗口.更新角色数据(this, "网络地址", O);
		};
		物理地址.更改事件 += delegate(string O)
		{
			主窗口.更新角色数据(this, "物理地址", O);
		};
		角色职业.更改事件 += delegate(游戏对象职业 O)
		{
			主窗口.更新角色数据(this, "角色职业", O);
		};
		角色性别.更改事件 += delegate(游戏对象性别 O)
		{
			主窗口.更新角色数据(this, "角色性别", O);
		};
		所属行会.更改事件 += delegate(行会数据 O)
		{
			主窗口.更新角色数据(this, "所属行会", O);
		};
		消耗元宝.更改事件 += delegate(long O)
		{
			主窗口.更新角色数据(this, "消耗元宝", O);
		};
		转出金币.更改事件 += delegate(long O)
		{
			主窗口.更新角色数据(this, "转出金币", O);
		};
		背包大小.更改事件 += delegate(byte O)
		{
			主窗口.更新角色数据(this, "背包大小", O);
		};
		仓库大小.更改事件 += delegate(byte O)
		{
			主窗口.更新角色数据(this, "仓库大小", O);
		};
		本期特权.更改事件 += delegate(byte O)
		{
			主窗口.更新角色数据(this, "本期特权", O);
		};
		本期日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "本期日期", O);
		};
		上期特权.更改事件 += delegate(byte O)
		{
			主窗口.更新角色数据(this, "上期特权", O);
		};
		上期日期.更改事件 += delegate(DateTime O)
		{
			主窗口.更新角色数据(this, "上期日期", O);
		};
		剩余特权.更改事件 += delegate(List<KeyValuePair<byte, int>> O)
		{
			主窗口.更新角色数据(this, "剩余特权", O.Sum((KeyValuePair<byte, int> X) => X.Value));
		};
		当前等级.更改事件 += delegate(byte O)
		{
			主窗口.更新角色数据(this, "当前等级", O);
		};
		当前经验.更改事件 += delegate(int O)
		{
			主窗口.更新角色数据(this, "当前经验", O);
		};
		双倍经验.更改事件 += delegate(int O)
		{
			主窗口.更新角色数据(this, "双倍经验", O);
		};
		当前战力.更改事件 += delegate(int O)
		{
			主窗口.更新角色数据(this, "当前战力", O);
		};
		当前地图.更改事件 += delegate(int O)
		{
			主窗口.更新角色数据(this, "当前地图", (!游戏地图.数据表.TryGetValue((byte)O, out var value)) ? ((object)O) : value);
		};
		当前坐标.更改事件 += delegate(Point O)
		{
			主窗口.更新角色数据(this, "当前坐标", $"{O.X}, {O.Y}");
		};
		当前PK值.更改事件 += delegate(int O)
		{
			主窗口.更新角色数据(this, "当前PK值", O);
		};
		技能数据.更改事件 += delegate(List<KeyValuePair<ushort, 技能数据>> O)
		{
			主窗口.更新角色技能(this, O);
		};
		角色装备.更改事件 += delegate(List<KeyValuePair<byte, 装备数据>> O)
		{
			主窗口.更新角色装备(this, O);
		};
		角色背包.更改事件 += delegate(List<KeyValuePair<byte, 物品数据>> O)
		{
			主窗口.更新角色背包(this, O);
		};
		角色仓库.更改事件 += delegate(List<KeyValuePair<byte, 物品数据>> O)
		{
			主窗口.更新角色仓库(this, O);
		};
	}

	public override void 加载完成()
	{
		订阅事件();
		主窗口.添加角色数据(this);
		主窗口.更新角色技能(this, 技能数据.ToList());
		主窗口.更新角色装备(this, 角色装备.ToList());
		主窗口.更新角色背包(this, 角色背包.ToList());
		主窗口.更新角色仓库(this, 角色仓库.ToList());
	}

	public override void 删除数据()
	{
		所属账号.V.角色列表.Remove(this);
		所属账号.V.冻结列表.Remove(this);
		所属账号.V.删除列表.Remove(this);
		升级装备.V?.删除数据();
		foreach (宠物数据 item in 宠物数据)
		{
			item.删除数据();
		}
		foreach (邮件数据 item2 in 角色邮件)
		{
			item2.删除数据();
		}
		foreach (KeyValuePair<byte, 物品数据> item3 in 角色背包)
		{
			item3.Value.删除数据();
		}
		foreach (KeyValuePair<byte, 装备数据> item4 in 角色装备)
		{
			item4.Value.删除数据();
		}
		foreach (KeyValuePair<byte, 物品数据> item5 in 角色仓库)
		{
			item5.Value.删除数据();
		}
		foreach (KeyValuePair<ushort, 技能数据> item6 in 技能数据)
		{
			item6.Value.删除数据();
		}
		foreach (KeyValuePair<ushort, Buff数据> item7 in Buff数据)
		{
			item7.Value.删除数据();
		}
		if (所属队伍.V != null)
		{
			if (this != 所属队伍.V.队长数据)
			{
				所属队伍.V.队伍成员.Remove(this);
			}
			else
			{
				所属队伍.V.删除数据();
			}
		}
		if (所属师门.V != null)
		{
			if (this != 所属师门.V.师父数据)
			{
				所属师门.V.移除徒弟(this);
			}
			else
			{
				所属师门.V.删除数据();
			}
		}
		if (所属行会.V != null)
		{
			所属行会.V.行会成员.Remove(this);
			所属行会.V.行会禁言.Remove(this);
		}
		foreach (角色数据 item8 in 好友列表)
		{
			item8.好友列表.Remove(this);
		}
		foreach (角色数据 item9 in 粉丝列表)
		{
			item9.偶像列表.Remove(this);
		}
		foreach (角色数据 item10 in 仇恨列表)
		{
			item10.仇人列表.Remove(this);
		}
		base.删除数据();
	}

	public byte[] 角色描述()
	{
		using MemoryStream memoryStream = new MemoryStream(new byte[94]);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(数据索引.V);
		binaryWriter.Write(名字描述());
		binaryWriter.Seek(61, SeekOrigin.Begin);
		binaryWriter.Write((byte)角色职业.V);
		binaryWriter.Write((byte)角色性别.V);
		binaryWriter.Write((byte)角色发型.V);
		binaryWriter.Write((byte)角色发色.V);
		binaryWriter.Write((byte)角色脸型.V);
		binaryWriter.Write((byte)0);
		binaryWriter.Write(角色等级);
		binaryWriter.Write(当前地图.V);
		binaryWriter.Write(角色装备[0]?.升级次数.V ?? 0);
		binaryWriter.Write((角色装备[0]?.对应模板.V?.物品编号).GetValueOrDefault());
		binaryWriter.Write((角色装备[1]?.对应模板.V?.物品编号).GetValueOrDefault());
		binaryWriter.Write((角色装备[2]?.对应模板.V?.物品编号).GetValueOrDefault());
		binaryWriter.Write(计算类.时间转换(离线日期.V));
		binaryWriter.Write((!冻结日期.V.Equals(default(DateTime))) ? 计算类.时间转换(冻结日期.V) : 0);
		return memoryStream.ToArray();
	}

	public byte[] 名字描述()
	{
		return Encoding.UTF8.GetBytes(角色名字.V);
	}

	public byte[] 角色设置()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (uint item in 玩家设置)
		{
			binaryWriter.Write(item);
		}
		return memoryStream.ToArray();
	}

	public byte[] 邮箱描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)角色邮件.Count);
		foreach (邮件数据 item in 角色邮件)
		{
			binaryWriter.Write(item.邮件检索描述());
		}
		return memoryStream.ToArray();
	}

	static 角色数据()
	{
	}
}
