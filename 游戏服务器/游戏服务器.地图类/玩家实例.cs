using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public sealed class 玩家实例 : 地图对象
{
	public 角色数据 角色数据;

	public 铭文技能 洗练铭文;

	public 玩家交易 当前交易;

	public 玩家摊位 当前摊位;

	public byte 雕色部位;

	public byte 重铸部位;

	public int 对话页面;

	public 守卫实例 对话守卫;

	public DateTime 对话超时;

	public int 打开商店;

	public string 打开界面;

	public int 回血次数;

	public int 回魔次数;

	public int 回血基数;

	public int 回魔基数;

	public DateTime 邮件时间;

	public DateTime 药品回血;

	public DateTime 药品回魔;

	public DateTime 称号时间;

	public DateTime 特权时间;

	public DateTime 拾取时间;

	public DateTime 队伍时间;

	public DateTime 战具计时;

	public DateTime 经验计时;

	public List<物品数据> 回购清单;

	public List<宠物实例> 宠物列表;

	public Dictionary<object, int> 战力加成;

	public readonly Dictionary<ushort, 技能数据> 被动技能;

	public 客户网络 网络连接 => 角色数据.网络连接;

	public byte 交易状态
	{
		get
		{
			if (当前交易 != null)
			{
				if (当前交易.交易申请方 == this)
				{
					return 当前交易.申请方状态;
				}
				return 当前交易.接收方状态;
			}
			return 0;
		}
	}

	public byte 摆摊状态
	{
		get
		{
			if (当前摊位 == null)
			{
				return 0;
			}
			return 当前摊位.摊位状态;
		}
		set
		{
			if (当前摊位 != null)
			{
				当前摊位.摊位状态 = value;
			}
		}
	}

	public string 摊位名字
	{
		get
		{
			if (当前摊位 == null)
			{
				return "";
			}
			return 当前摊位.摊位名字;
		}
	}

	public override string 对象名字 => 角色数据.角色名字.V;

	public override int 地图编号 => 角色数据.数据索引.V;

	public override int 当前体力
	{
		get
		{
			return 角色数据.当前血量.V;
		}
		set
		{
			value = Math.Min(this[游戏对象属性.最大体力], Math.Max(0, value));
			if (当前体力 != value)
			{
				角色数据.当前血量.V = value;
				发送封包(new 同步对象体力
				{
					对象编号 = 地图编号,
					当前体力 = 当前体力,
					体力上限 = this[游戏对象属性.最大体力]
				});
			}
		}
	}

	public override int 当前魔力
	{
		get
		{
			return 角色数据.当前蓝量.V;
		}
		set
		{
			value = Math.Min(this[游戏对象属性.最大魔力], Math.Max(0, value));
			if (当前魔力 != value)
			{
				角色数据.当前蓝量.V = Math.Max(0, value);
				网络连接?.发送封包(new 同步对象魔力
				{
					当前魔力 = 当前魔力
				});
			}
		}
	}

	public override byte 当前等级
	{
		get
		{
			return 角色数据.角色等级;
		}
		set
		{
			角色数据.角色等级 = value;
		}
	}

	public override Point 当前坐标
	{
		get
		{
			return 角色数据.当前坐标.V;
		}
		set
		{
			if (角色数据.当前坐标.V != value)
			{
				角色数据.当前坐标.V = value;
				if ((当前地图?.复活区域?.范围坐标.Contains(当前坐标)).GetValueOrDefault())
				{
					重生地图 = 当前地图.地图编号;
				}
			}
		}
	}

	public override 地图实例 当前地图
	{
		get
		{
			return base.当前地图;
		}
		set
		{
			if (当前地图 != value)
			{
				base.当前地图?.移除对象(this);
				base.当前地图 = value;
				base.当前地图?.添加对象(this);
			}
			if (角色数据.当前地图.V != value.地图模板.地图编号)
			{
				角色数据.当前地图.V = value.地图模板.地图编号;
				所属行会?.发送封包(new 同步会员信息
				{
					对象编号 = 地图编号,
					对象信息 = 角色数据.当前地图.V,
					当前等级 = 当前等级
				});
			}
		}
	}

	public override 游戏方向 当前方向
	{
		get
		{
			return 角色数据.当前朝向.V;
		}
		set
		{
			if (角色数据.当前朝向.V != value)
			{
				角色数据.当前朝向.V = value;
				发送封包(new 对象转动方向
				{
					对象编号 = 地图编号,
					对象朝向 = (ushort)value
				});
			}
		}
	}

	public override 游戏对象类型 对象类型 => 游戏对象类型.玩家;

	public override 技能范围类型 对象体型 => 技能范围类型.单体1x1;

	public override int 奔跑耗时 => base.奔跑速度 * 45;

	public override int 行走耗时 => base.行走速度 * 45;

	public override DateTime 忙碌时间
	{
		get
		{
			return base.忙碌时间;
		}
		set
		{
			if (base.忙碌时间 < value)
			{
				DateTime dateTime2 = (硬直时间 = value);
				base.忙碌时间 = dateTime2;
			}
		}
	}

	public override DateTime 硬直时间
	{
		get
		{
			return base.硬直时间;
		}
		set
		{
			if (base.硬直时间 < value)
			{
				base.硬直时间 = value;
				拾取时间 = value.AddMilliseconds(300.0);
			}
		}
	}

	public override DateTime 行走时间
	{
		get
		{
			return base.行走时间;
		}
		set
		{
			if (base.行走时间 < value)
			{
				base.行走时间 = value;
			}
		}
	}

	public override DateTime 奔跑时间
	{
		get
		{
			return base.奔跑时间;
		}
		set
		{
			if (base.奔跑时间 < value)
			{
				base.奔跑时间 = value;
			}
		}
	}

	public override int this[游戏对象属性 属性]
	{
		get
		{
			return base[属性];
		}
		set
		{
			if (base[属性] != value)
			{
				base[属性] = value;
				if ((byte)属性 <= 64)
				{
					网络连接?.发送封包(new 同步属性变动
					{
						属性编号 = (byte)属性,
						属性数值 = value
					});
				}
			}
		}
	}

	public override 字典监视器<ushort, Buff数据> Buff列表 => 角色数据.Buff数据;

	public override 字典监视器<int, DateTime> 冷却记录 => 角色数据.冷却数据;

	public 字典监视器<ushort, 技能数据> 主体技能表 => 角色数据.技能数据;

	public int 最大负重 => this[游戏对象属性.最大负重];

	public int 最大穿戴 => this[游戏对象属性.最大穿戴];

	public int 最大腕力 => this[游戏对象属性.最大腕力];

	public int 背包重量
	{
		get
		{
			int num = 0;
			foreach (物品数据 item in 角色背包.Values.ToList())
			{
				num += item?.物品重量 ?? 0;
			}
			return num;
		}
	}

	public int 装备重量
	{
		get
		{
			int num = 0;
			foreach (装备数据 item in 角色装备.Values.ToList())
			{
				num += ((item != null && item.物品类型 != 物品使用分类.武器) ? item.物品重量 : 0);
			}
			return num;
		}
	}

	public int 当前战力
	{
		get
		{
			return 角色数据.角色战力;
		}
		set
		{
			角色数据.角色战力 = value;
		}
	}

	public int 当前经验
	{
		get
		{
			return 角色数据.角色经验;
		}
		set
		{
			角色数据.角色经验 = value;
		}
	}

	public int 双倍经验
	{
		get
		{
			return 角色数据.双倍经验.V;
		}
		set
		{
			if (角色数据.双倍经验.V != value)
			{
				if (value > 角色数据.双倍经验.V)
				{
					网络连接?.发送封包(new 双倍经验变动
					{
						双倍经验 = value
					});
				}
				角色数据.双倍经验.V = value;
			}
		}
	}

	public int 所需经验 => 角色成长.升级所需经验[当前等级];

	public int 金币数量
	{
		get
		{
			return 角色数据.金币数量;
		}
		set
		{
			if (角色数据.金币数量 != value)
			{
				角色数据.金币数量 = value;
				网络连接?.发送封包(new 货币数量变动
				{
					货币类型 = 1,
					货币数量 = value
				});
			}
		}
	}

	public int 元宝数量
	{
		get
		{
			return 角色数据.元宝数量;
		}
		set
		{
			if (角色数据.元宝数量 != value)
			{
				角色数据.元宝数量 = value;
				网络连接?.发送封包(new 同步元宝数量
				{
					元宝数量 = value
				});
			}
		}
	}

	public int 师门声望
	{
		get
		{
			return 角色数据.师门声望;
		}
		set
		{
			if (角色数据.师门声望 != value)
			{
				角色数据.师门声望 = value;
				网络连接?.发送封包(new 货币数量变动
				{
					货币类型 = 6,
					货币数量 = value
				});
			}
		}
	}

	public int PK值惩罚
	{
		get
		{
			return 角色数据.角色PK值;
		}
		set
		{
			value = Math.Max(0, value);
			if (角色数据.角色PK值 == 0 && value > 0)
			{
				减PK时间 = TimeSpan.FromMinutes(1.0);
			}
			if (角色数据.角色PK值 != value)
			{
				if (角色数据.角色PK值 < 300 && value >= 300)
				{
					灰名时间 = TimeSpan.Zero;
				}
				同步对象惩罚 同步对象惩罚 = new 同步对象惩罚
				{
					对象编号 = 地图编号
				};
				int num2 = (角色数据.角色PK值 = value);
				int pK值惩罚 = num2;
				pK值惩罚 = (同步对象惩罚.PK值惩罚 = pK值惩罚);
				发送封包(同步对象惩罚);
			}
		}
	}

	public int 重生地图
	{
		get
		{
			if (红名玩家)
			{
				return 147;
			}
			return 角色数据.重生地图.V;
		}
		set
		{
			if (角色数据.重生地图.V != value)
			{
				角色数据.重生地图.V = value;
			}
		}
	}

	public bool 红名玩家 => PK值惩罚 >= 300;

	public bool 灰名玩家 => 灰名时间 > TimeSpan.Zero;

	public bool 绑定地图 => 当前地图?[当前坐标].Contains(this) ?? false;

	public byte 背包大小
	{
		get
		{
			return 角色数据.背包大小.V;
		}
		set
		{
			角色数据.背包大小.V = value;
		}
	}

	public byte 背包剩余 => (byte)(背包大小 - 角色背包.Count);

	public byte 仓库大小
	{
		get
		{
			return 角色数据.仓库大小.V;
		}
		set
		{
			角色数据.仓库大小.V = value;
		}
	}

	public byte 资源背包大小
	{
		get
		{
			return 角色数据.资源背包大小.V;
		}
		set
		{
			角色数据.资源背包大小.V = value;
		}
	}

	public byte 宠物上限 { get; set; }

	public byte 宠物数量 => (byte)宠物列表.Count;

	public byte 师门参数
	{
		get
		{
			if (所属师门 == null)
			{
				if (当前等级 >= 30)
				{
					return 2;
				}
				return 0;
			}
			if (所属师门.师父编号 == 地图编号)
			{
				return 2;
			}
			return 1;
		}
	}

	public byte 当前称号
	{
		get
		{
			return 角色数据.当前称号.V;
		}
		set
		{
			if (角色数据.当前称号.V != value)
			{
				角色数据.当前称号.V = value;
			}
		}
	}

	public byte 本期特权
	{
		get
		{
			return 角色数据.本期特权.V;
		}
		set
		{
			if (角色数据.本期特权.V != value)
			{
				角色数据.本期特权.V = value;
			}
		}
	}

	public byte 上期特权
	{
		get
		{
			return 角色数据.上期特权.V;
		}
		set
		{
			if (角色数据.上期特权.V != value)
			{
				角色数据.上期特权.V = value;
			}
		}
	}

	public byte 预定特权
	{
		get
		{
			return 角色数据.预定特权.V;
		}
		set
		{
			if (角色数据.预定特权.V != value)
			{
				角色数据.预定特权.V = value;
			}
		}
	}

	public uint 本期记录
	{
		get
		{
			return 角色数据.本期记录.V;
		}
		set
		{
			if (角色数据.本期记录.V != value)
			{
				角色数据.本期记录.V = value;
			}
		}
	}

	public uint 上期记录
	{
		get
		{
			return 角色数据.上期记录.V;
		}
		set
		{
			if (角色数据.上期记录.V != value)
			{
				角色数据.上期记录.V = value;
			}
		}
	}

	public DateTime 本期日期
	{
		get
		{
			return 角色数据.本期日期.V;
		}
		set
		{
			if (角色数据.本期日期.V != value)
			{
				角色数据.本期日期.V = value;
			}
		}
	}

	public DateTime 上期日期
	{
		get
		{
			return 角色数据.上期日期.V;
		}
		set
		{
			if (角色数据.上期日期.V != value)
			{
				角色数据.上期日期.V = value;
			}
		}
	}

	public TimeSpan 灰名时间
	{
		get
		{
			return 角色数据.灰名时间.V;
		}
		set
		{
			if (角色数据.灰名时间.V <= TimeSpan.Zero && value > TimeSpan.Zero)
			{
				发送封包(new 玩家名字变灰
				{
					对象编号 = 地图编号,
					是否灰名 = true
				});
			}
			else if (角色数据.灰名时间.V > TimeSpan.Zero && value <= TimeSpan.Zero)
			{
				发送封包(new 玩家名字变灰
				{
					对象编号 = 地图编号,
					是否灰名 = false
				});
			}
			if (角色数据.灰名时间.V != value)
			{
				角色数据.灰名时间.V = value;
			}
		}
	}

	public TimeSpan 减PK时间
	{
		get
		{
			return 角色数据.减PK时间.V;
		}
		set
		{
			if (!(角色数据.减PK时间.V > TimeSpan.Zero) || !(value <= TimeSpan.Zero))
			{
				if (角色数据.减PK时间.V != value)
				{
					角色数据.减PK时间.V = value;
				}
			}
			else
			{
				PK值惩罚--;
				角色数据.减PK时间.V = TimeSpan.FromMinutes(1.0);
			}
		}
	}

	public 账号数据 所属账号 => 角色数据.所属账号.V;

	public 行会数据 所属行会
	{
		get
		{
			return 角色数据.所属行会.V;
		}
		set
		{
			if (角色数据.所属行会.V != value)
			{
				角色数据.所属行会.V = value;
			}
		}
	}

	public 队伍数据 所属队伍
	{
		get
		{
			return 角色数据.所属队伍.V;
		}
		set
		{
			if (角色数据.所属队伍.V != value)
			{
				角色数据.所属队伍.V = value;
			}
		}
	}

	public 师门数据 所属师门
	{
		get
		{
			return 角色数据.所属师门.V;
		}
		set
		{
			if (角色数据.所属师门.V != value)
			{
				角色数据.所属师门.V = value;
			}
		}
	}

	public 攻击模式 攻击模式
	{
		get
		{
			return 角色数据.攻击模式.V;
		}
		set
		{
			if (角色数据.攻击模式.V != value)
			{
				角色数据.攻击模式.V = value;
				网络连接?.发送封包(new 同步对战模式
				{
					对象编号 = 地图编号,
					攻击模式 = (byte)value
				});
			}
		}
	}

	public 宠物模式 宠物模式
	{
		get
		{
			if (角色数据.宠物模式.V != 0)
			{
				return 角色数据.宠物模式.V;
			}
			角色数据.宠物模式.V = 宠物模式.攻击;
			return 宠物模式.攻击;
		}
		set
		{
			if (角色数据.宠物模式.V != value)
			{
				角色数据.宠物模式.V = value;
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 9473,
					第一参数 = (int)value
				});
			}
		}
	}

	public 地图实例 复活地图
	{
		get
		{
			if (!红名玩家)
			{
				if (当前地图.地图编号 != 重生地图)
				{
					return 地图处理网关.分配地图(重生地图);
				}
				return 当前地图;
			}
			if (当前地图.地图编号 != 147)
			{
				return 地图处理网关.分配地图(147);
			}
			return 当前地图;
		}
	}

	public 对象发型分类 角色发型
	{
		get
		{
			return 角色数据.角色发型.V;
		}
		set
		{
			角色数据.角色发型.V = value;
		}
	}

	public 对象发色分类 角色发色
	{
		get
		{
			return 角色数据.角色发色.V;
		}
		set
		{
			角色数据.角色发色.V = value;
		}
	}

	public 对象脸型分类 角色脸型
	{
		get
		{
			return 角色数据.角色脸型.V;
		}
		set
		{
			角色数据.角色脸型.V = value;
		}
	}

	public 游戏对象性别 角色性别 => 角色数据.角色性别.V;

	public 游戏对象职业 角色职业 => 角色数据.角色职业.V;

	public 对象名字颜色 对象颜色
	{
		get
		{
			if (角色数据.灰名时间.V > TimeSpan.Zero)
			{
				return 对象名字颜色.灰名;
			}
			if (角色数据.角色PK值 >= 800)
			{
				return 对象名字颜色.深红;
			}
			if (角色数据.角色PK值 >= 300)
			{
				return 对象名字颜色.红名;
			}
			if (角色数据.角色PK值 < 99)
			{
				return 对象名字颜色.白名;
			}
			return 对象名字颜色.黄名;
		}
	}

	public 哈希监视器<宠物数据> 宠物数据 => 角色数据.宠物数据;

	public 哈希监视器<邮件数据> 未读邮件 => 角色数据.未读邮件;

	public 哈希监视器<邮件数据> 全部邮件 => 角色数据.角色邮件;

	public 哈希监视器<角色数据> 好友列表 => 角色数据.好友列表;

	public 哈希监视器<角色数据> 粉丝列表 => 角色数据.粉丝列表;

	public 哈希监视器<角色数据> 偶像列表 => 角色数据.偶像列表;

	public 哈希监视器<角色数据> 仇人列表 => 角色数据.仇人列表;

	public 哈希监视器<角色数据> 仇恨列表 => 角色数据.仇恨列表;

	public 哈希监视器<角色数据> 黑名单表 => 角色数据.黑名单表;

	public 字典监视器<byte, int> 剩余特权 => 角色数据.剩余特权;

	public 字典监视器<byte, 技能数据> 快捷栏位 => 角色数据.快捷栏位;

	public 字典监视器<byte, 物品数据> 角色背包 => 角色数据.角色背包;

	public 字典监视器<byte, 物品数据> 角色仓库 => 角色数据.角色仓库;

	public 字典监视器<byte, 物品数据> 角色资源背包 => 角色数据.角色资源背包;

	public 字典监视器<byte, 装备数据> 角色装备 => 角色数据.角色装备;

	public 字典监视器<byte, DateTime> 称号列表 => 角色数据.称号列表;

	public 玩家实例(角色数据 角色数据, 客户网络 网络连接)
	{
		this.角色数据 = 角色数据;
		宠物列表 = new List<宠物实例>();
		被动技能 = new Dictionary<ushort, 技能数据>();
		属性加成[this] = 角色成长.获取数据(角色职业, 当前等级);
		战力加成 = new Dictionary<object, int> { [this] = 当前等级 * 10 };
		称号时间 = DateTime.MaxValue;
		拾取时间 = 主程.当前时间.AddSeconds(1.0);
		base.恢复时间 = 主程.当前时间.AddSeconds(5.0);
		特权时间 = ((本期特权 > 0) ? 本期日期.AddDays(30.0) : DateTime.MaxValue);
		foreach (装备数据 value2 in 角色装备.Values)
		{
			战力加成[value2] = value2.装备战力;
			if (value2.当前持久.V > 0)
			{
				属性加成[value2] = value2.装备属性;
			}
			if (value2.第一铭文 != null && 主体技能表.TryGetValue(value2.第一铭文.技能编号, out var v))
			{
				v.铭文编号 = value2.第一铭文.铭文编号;
			}
			if (value2.第二铭文 != null && 主体技能表.TryGetValue(value2.第二铭文.技能编号, out var v2))
			{
				v2.铭文编号 = value2.第二铭文.铭文编号;
			}
		}
		foreach (技能数据 value3 in 主体技能表.Values)
		{
			战力加成[value3] = value3.战力加成;
			属性加成[value3] = value3.属性加成;
			foreach (ushort item in value3.被动技能.ToList())
			{
				被动技能.Add(item, value3);
			}
		}
		foreach (Buff数据 value4 in Buff列表.Values)
		{
			if ((value4.Buff效果 & Buff效果类型.属性增减) != 0)
			{
				属性加成.Add(value4, value4.属性加成);
			}
		}
		foreach (KeyValuePair<byte, DateTime> item2 in 称号列表.ToList())
		{
			if (!(主程.当前时间 >= item2.Value))
			{
				if (item2.Value < 称号时间)
				{
					称号时间 = item2.Value;
				}
			}
			else if (称号列表.Remove(item2.Key) && 当前称号 == item2.Key)
			{
				当前称号 = 0;
			}
		}
		if (当前称号 > 0 && 游戏称号.数据表.TryGetValue(当前称号, out var value))
		{
			战力加成[当前称号] = value.称号战力;
			属性加成[当前称号] = value.称号属性;
		}
		if (当前体力 != 0)
		{
			if (游戏地图.数据表[(byte)角色数据.当前地图.V].下线传送)
			{
				if (角色数据.当前地图.V != 152)
				{
					if (游戏地图.数据表[(byte)角色数据.当前地图.V].传送地图 == 0)
					{
						当前地图 = 地图处理网关.分配地图(重生地图);
						当前坐标 = 当前地图.复活区域.随机坐标;
					}
					else
					{
						当前地图 = 地图处理网关.分配地图(游戏地图.数据表[(byte)角色数据.当前地图.V].传送地图);
						当前坐标 = 当前地图.传送区域?.随机坐标 ?? 当前地图.地图区域.First().随机坐标;
					}
				}
				else
				{
					当前地图 = 地图处理网关.沙城地图;
					if (所属行会 == null || 所属行会 != 系统数据.数据.占领行会.V)
					{
						当前坐标 = 地图处理网关.外城复活区域.随机坐标;
					}
					else
					{
						当前坐标 = 地图处理网关.守方传送区域.随机坐标;
					}
				}
			}
			else
			{
				当前地图 = 地图处理网关.分配地图(角色数据.当前地图.V);
			}
		}
		else
		{
			当前地图 = 地图处理网关.分配地图(重生地图);
			当前坐标 = ((!红名玩家) ? 当前地图.复活区域.随机坐标 : 当前地图.红名区域.随机坐标);
			当前体力 = (int)((float)this[游戏对象属性.最大体力] * 0.3f);
			当前魔力 = (int)((float)this[游戏对象属性.最大魔力] * 0.3f);
		}
		更新玩家战力();
		更新对象属性();
		对象死亡 = false;
		阻塞网格 = true;
		地图处理网关.添加地图对象(this);
		激活对象 = true;
		地图处理网关.添加激活对象(this);
		角色数据.登录日期.V = 主程.当前时间;
		角色数据.角色上线(网络连接);
		网络连接?.发送封包(new 同步角色数据
		{
			对象编号 = 地图编号,
			对象坐标 = 当前坐标,
			对象高度 = 当前高度,
			当前经验 = 当前经验,
			双倍经验 = 双倍经验,
			所需经验 = 所需经验,
			PK值惩罚 = PK值惩罚,
			对象朝向 = (ushort)当前方向,
			当前地图 = 当前地图.地图编号,
			当前线路 = 当前地图.路线编号,
			对象职业 = (byte)角色职业,
			对象性别 = (byte)角色性别,
			对象等级 = 当前等级,
			攻击模式 = (byte)攻击模式,
			当前时间 = 计算类.时间转换(主程.当前时间),
			开放等级 = 自定义类.游戏开放等级,
			特修折扣 = (ushort)(自定义类.装备特修折扣 * 10000m)
		});
		网络连接?.发送封包(new 同步补充变量
		{
			变量类型 = 1,
			对象编号 = 地图编号,
			变量索引 = 112,
			变量内容 = 计算类.时间转换(角色数据.补给日期.V)
		});
		网络连接?.发送封包(new 同步补充变量
		{
			变量类型 = 1,
			对象编号 = 地图编号,
			变量索引 = 975,
			变量内容 = 计算类.时间转换(角色数据.战备日期.V)
		});
		网络连接?.发送封包(new 同步背包大小
		{
			背包大小 = 背包大小,
			仓库大小 = 仓库大小,
			资源背包大小 = 资源背包大小
		});
		网络连接?.发送封包(new 同步技能信息
		{
			技能描述 = 全部技能描述()
		});
		网络连接?.发送封包(new 同步技能栏位
		{
			栏位描述 = 快捷栏位描述()
		});
		网络连接?.发送封包(new 同步背包信息
		{
			物品描述 = 全部物品描述()
		});
		网络连接?.发送封包(new 同步角色属性
		{
			属性描述 = 玩家属性描述()
		});
		网络连接?.发送封包(new 同步声望列表());
		网络连接?.发送封包(new 同步客户变量
		{
			字节数据 = 角色数据.角色设置()
		});
		网络连接?.发送封包(new 同步货币数量
		{
			字节描述 = 全部货币描述()
		});
		网络连接?.发送封包(new 同步签到信息());
		网络连接?.发送封包(new 同步特权信息
		{
			字节数组 = 玛法特权描述()
		});
		网络连接?.发送封包(new 同步数据结束
		{
			角色编号 = 地图编号
		});
		网络连接?.发送封包(new 同步师门信息
		{
			师门参数 = 师门参数
		});
		网络连接?.发送封包(new 同步称号信息
		{
			字节描述 = 全部称号描述()
		});
		网络连接?.发送封包(new 玩家名字变灰
		{
			对象编号 = 地图编号,
			是否灰名 = 灰名玩家
		});
		foreach (角色数据 item3 in 粉丝列表)
		{
			item3.网络连接?.发送封包(new 好友上线下线
			{
				对象编号 = 地图编号,
				对象名字 = 对象名字,
				对象职业 = (byte)角色职业,
				对象性别 = (byte)角色性别,
				上线下线 = 0
			});
		}
		foreach (角色数据 item4 in 仇恨列表)
		{
			item4.网络连接?.发送封包(new 好友上线下线
			{
				对象编号 = 地图编号,
				对象名字 = 对象名字,
				对象职业 = (byte)角色职业,
				对象性别 = (byte)角色性别,
				上线下线 = 0
			});
		}
		if (偶像列表.Count != 0 || 仇人列表.Count != 0)
		{
			网络连接?.发送封包(new 同步好友列表
			{
				字节描述 = 社交列表描述()
			});
		}
		if (黑名单表.Count != 0)
		{
			网络连接?.发送封包(new 同步黑名单表
			{
				字节描述 = 社交屏蔽描述()
			});
		}
		if (未读邮件.Count >= 1)
		{
			网络连接?.发送封包(new 未读邮件提醒
			{
				邮件数量 = 未读邮件.Count
			});
		}
		if (所属队伍 != null)
		{
			网络连接?.发送封包(new 玩家加入队伍
			{
				字节描述 = 所属队伍.队伍描述()
			});
		}
		if (所属行会 != null)
		{
			网络连接?.发送封包(new 行会信息公告
			{
				字节数据 = 所属行会.行会信息描述()
			});
			所属行会.发送封包(new 同步会员信息
			{
				对象编号 = 地图编号,
				对象信息 = 当前地图.地图编号,
				当前等级 = 当前等级
			});
			if (所属行会.行会成员[this.角色数据] <= 行会职位.理事 && 所属行会.申请列表.Count > 0)
			{
				网络连接?.发送封包(new 发送行会通知
				{
					提醒类型 = 1
				});
			}
			if (所属行会.行会成员[this.角色数据] <= 行会职位.副长 && 所属行会.结盟申请.Count > 0)
			{
				网络连接?.发送封包(new 发送行会通知
				{
					提醒类型 = 2
				});
			}
			if (所属行会.行会成员[this.角色数据] <= 行会职位.副长 && 所属行会.解除申请.Count > 0)
			{
				网络连接?.发送封包(new 行会外交公告
				{
					字节数据 = 所属行会.解除申请描述()
				});
			}
		}
		if (系统数据.数据.占领行会.V != null)
		{
			网络连接?.发送封包(new 同步占领行会
			{
				行会编号 = 系统数据.数据.占领行会.V.行会编号
			});
		}
		if (所属行会 != null && 所属行会 == 系统数据.数据.占领行会.V && 所属行会.行会成员[角色数据] == 行会职位.会长)
		{
			网络服务网关.发送公告("沙巴克城主 [" + 对象名字 + "] 进入了游戏");
		}
		所属队伍?.发送封包(new 同步队员状态
		{
			对象编号 = 地图编号
		});
	}

	public override void 处理对象数据()
	{
		if (绑定地图)
		{
			if (当前地图.地图编号 == 183 && 主程.当前时间.Hour != 自定义类.武斗场时间一 && 主程.当前时间.Hour != 自定义类.武斗场时间二)
			{
				if (对象死亡)
				{
					玩家请求复活();
				}
				else
				{
					玩家切换地图(复活地图, 地图区域类型.复活区域);
				}
				return;
			}
			foreach (技能数据 item in 主体技能表.Values.ToList())
			{
				if (item.技能计数 <= 0 || item.剩余次数.V >= item.技能计数)
				{
					continue;
				}
				if (item.计数时间 == default(DateTime))
				{
					item.计数时间 = 主程.当前时间.AddMilliseconds((int)item.计数周期);
				}
				else if (主程.当前时间 > item.计数时间)
				{
					if (++item.剩余次数.V < item.技能计数)
					{
						item.计数时间 = 主程.当前时间.AddMilliseconds((int)item.计数周期);
					}
					else
					{
						item.计数时间 = default(DateTime);
					}
					网络连接?.发送封包(new 同步技能计数
					{
						技能编号 = item.技能编号.V,
						技能计数 = item.剩余次数.V,
						技能冷却 = item.计数周期
					});
				}
			}
			foreach (技能实例 item2 in 技能任务.ToList())
			{
				item2.处理任务();
			}
			foreach (KeyValuePair<ushort, Buff数据> item3 in Buff列表.ToList())
			{
				轮询Buff时处理(item3.Value);
			}
			if (主程.当前时间 >= 称号时间)
			{
				DateTime dateTime = DateTime.MaxValue;
				foreach (KeyValuePair<byte, DateTime> item4 in 称号列表.ToList())
				{
					if (主程.当前时间 >= item4.Value)
					{
						玩家称号到期(item4.Key);
					}
					else if (item4.Value < dateTime)
					{
						dateTime = item4.Value;
					}
				}
				称号时间 = dateTime;
			}
			if (主程.当前时间 >= 特权时间)
			{
				玩家特权到期();
				if (剩余特权.TryGetValue(预定特权, out var v) && v >= 30)
				{
					玩家激活特权(预定特权);
					if ((剩余特权[预定特权] -= 30) <= 0)
					{
						预定特权 = 0;
					}
				}
				if (本期特权 == 0)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 65553
					});
				}
				网络连接?.发送封包(new 同步特权信息
				{
					字节数组 = 玛法特权描述()
				});
			}
			if (灰名玩家)
			{
				灰名时间 -= 主程.当前时间 - base.处理计时;
			}
			if (PK值惩罚 > 0)
			{
				减PK时间 -= 主程.当前时间 - base.处理计时;
			}
			if (所属队伍 != null && 主程.当前时间 > 队伍时间)
			{
				所属队伍?.发送封包(new 同步队员信息
				{
					队伍编号 = 所属队伍.队伍编号,
					对象编号 = 地图编号,
					对象等级 = 当前等级,
					最大体力 = this[游戏对象属性.最大体力],
					最大魔力 = this[游戏对象属性.最大魔力],
					当前体力 = 当前体力,
					当前魔力 = 当前魔力,
					当前地图 = 当前地图.地图编号,
					当前线路 = 当前地图.路线编号,
					横向坐标 = 当前坐标.X,
					纵向坐标 = 当前坐标.Y,
					坐标高度 = 当前高度,
					攻击模式 = (byte)攻击模式
				});
				队伍时间 = 主程.当前时间.AddSeconds(5.0);
			}
			if (!对象死亡)
			{
				if (主程.当前时间 > 拾取时间)
				{
					拾取时间 = 拾取时间.AddMilliseconds(1000.0);
					foreach (地图对象 item5 in 当前地图[当前坐标].ToList())
					{
						if (item5 is 物品实例 物品)
						{
							玩家拾取物品(物品);
							break;
						}
					}
				}
				if (主程.当前时间 > base.恢复时间)
				{
					if (!检查状态(游戏对象状态.中毒状态))
					{
						当前体力 += this[游戏对象属性.体力恢复];
						当前魔力 += this[游戏对象属性.魔力恢复];
					}
					base.恢复时间 = base.恢复时间.AddSeconds(30.0);
				}
				if (主程.当前时间 > 战具计时 && 角色装备.TryGetValue(15, out var v2) && v2.当前持久.V > 0)
				{
					if (v2.物品编号 == 99999100 || v2.物品编号 == 99999101)
					{
						int num = Math.Min(10, Math.Min(v2.当前持久.V, this[游戏对象属性.最大体力] - 当前体力));
						if (num > 0)
						{
							当前体力 += num;
							战具损失持久(num);
						}
						战具计时 = 主程.当前时间.AddMilliseconds(1000.0);
					}
					else if (v2.物品编号 == 99999102 || v2.物品编号 == 99999103)
					{
						int num2 = Math.Min(15, Math.Min(v2.当前持久.V, this[游戏对象属性.最大魔力] - 当前魔力));
						if (num2 > 0)
						{
							当前魔力 += num2;
							战具损失持久(num2);
						}
						战具计时 = 主程.当前时间.AddMilliseconds(1000.0);
					}
					else if (v2.物品编号 == 99999110 || v2.物品编号 == 99999111)
					{
						int num3 = Math.Min(10, Math.Min(v2.当前持久.V, this[游戏对象属性.最大体力] - 当前体力));
						if (num3 > 0)
						{
							当前体力 += num3;
							当前魔力 += num3;
							战具损失持久(num3);
						}
						战具计时 = 主程.当前时间.AddMilliseconds(1000.0);
					}
				}
				if (base.治疗次数 > 0 && 主程.当前时间 > base.治疗时间)
				{
					base.治疗次数--;
					当前体力 += base.治疗基数;
					base.治疗时间 = 主程.当前时间.AddMilliseconds(500.0);
				}
				if (回血次数 > 0 && 主程.当前时间 > 药品回血)
				{
					回血次数--;
					药品回血 = 主程.当前时间.AddMilliseconds(1000.0);
					当前体力 += (int)Math.Max(0f, (float)回血基数 * (1f + (float)this[游戏对象属性.药品回血] / 10000f));
				}
				if (回魔次数 > 0 && 主程.当前时间 > 药品回魔)
				{
					回魔次数--;
					药品回魔 = 主程.当前时间.AddMilliseconds(1000.0);
					当前魔力 += (int)Math.Max(0f, (float)回魔基数 * (1f + (float)this[游戏对象属性.药品回魔] / 10000f));
				}
				if (当前地图.地图编号 == 183 && 主程.当前时间 > 经验计时)
				{
					经验计时 = 主程.当前时间.AddSeconds(5.0);
					玩家增加经验(null, (当前地图[当前坐标].FirstOrDefault((地图对象 O) => O is 守卫实例 守卫实例2 && 守卫实例2.模板编号 == 6121) == null) ? 500 : 2500);
				}
			}
			所属行会?.清理数据();
		}
		base.处理对象数据();
	}

	public override void 自身死亡处理(地图对象 对象, bool 技能击杀)
	{
		base.自身死亡处理(对象, 技能击杀);
		foreach (Buff数据 item in Buff列表.Values.ToList())
		{
			if (item.死亡消失)
			{
				删除Buff时处理(item.Buff编号.V);
			}
		}
		回魔次数 = 0;
		回血次数 = 0;
		base.治疗次数 = 0;
		当前交易?.结束交易();
		foreach (宠物实例 item2 in 宠物列表.ToList())
		{
			item2.自身死亡处理(null, 技能击杀: false);
		}
		网络连接?.发送封包(new 离开战斗姿态
		{
			对象编号 = 地图编号
		});
		网络连接?.发送封包(new 发送复活信息());
		玩家实例 玩家实例2 = null;
		if (!(对象 is 玩家实例 玩家实例3))
		{
			if (!(对象 is 宠物实例 宠物实例2))
			{
				if (对象 is 陷阱实例 陷阱实例2 && 陷阱实例2.陷阱来源 is 玩家实例 玩家实例4)
				{
					玩家实例2 = 玩家实例4;
				}
			}
			else
			{
				玩家实例2 = 宠物实例2.宠物主人;
			}
		}
		else
		{
			玩家实例2 = 玩家实例3;
		}
		if (玩家实例2 != null && !当前地图.自由区内(当前坐标) && !灰名玩家 && !红名玩家 && (地图处理网关.沙城节点 < 2 || (当前地图.地图编号 != 152 && 当前地图.地图编号 != 178)))
		{
			玩家实例2.PK值惩罚 += 50;
			if (技能击杀)
			{
				玩家实例2.武器幸运损失();
			}
		}
		if (玩家实例2 != null)
		{
			网络连接?.发送封包(new 同步气泡提示
			{
				泡泡类型 = 1,
				泡泡参数 = 玩家实例2.地图编号
			});
			网络连接?.发送封包(new 同步对战结果
			{
				击杀方式 = 1,
				胜方编号 = 玩家实例2.地图编号,
				败方编号 = 地图编号,
				PK值惩罚 = 50
			});
			string text = ((所属行会 != null) ? $"[{所属行会}]行会的" : "");
			string text2 = ((玩家实例2.所属行会 == null) ? "" : $"[{玩家实例2.所属行会}]行会的");
			网络服务网关.发送公告($"{text}[{this}]在{当前地图}被{text2}[{玩家实例2}]击杀");
		}
		if (玩家实例2 == null || !当前地图.掉落装备(当前坐标, 红名玩家))
		{
			return;
		}
		foreach (装备数据 item3 in 角色装备.Values.ToList())
		{
			if (item3.能否掉落 && 计算类.计算概率(0.05f))
			{
				new 物品实例(item3.物品模板, item3, 当前地图, 当前坐标, new HashSet<角色数据>());
				角色装备.Remove(item3.当前位置);
				玩家穿卸装备((装备穿戴部位)item3.当前位置, item3, null);
				网络连接?.发送封包(new 玩家掉落装备
				{
					物品描述 = item3.字节描述()
				});
				网络连接?.发送封包(new 删除玩家物品
				{
					背包类型 = 0,
					物品位置 = item3.当前位置
				});
			}
		}
		foreach (物品数据 item4 in 角色背包.Values.ToList())
		{
			if (item4.能否掉落 && 计算类.计算概率(0.1f))
			{
				if (item4.持久类型 != 物品持久分类.堆叠 || item4.当前持久.V <= 1)
				{
					new 物品实例(item4.物品模板, item4, 当前地图, 当前坐标, new HashSet<角色数据>());
					角色背包.Remove(item4.当前位置);
					网络连接?.发送封包(new 玩家掉落装备
					{
						物品描述 = item4.字节描述()
					});
					网络连接?.发送封包(new 删除玩家物品
					{
						背包类型 = 1,
						物品位置 = item4.当前位置
					});
				}
				else
				{
					物品实例 物品实例2 = new 物品实例(item4.物品模板, new 物品数据(item4.物品模板, 角色数据, 1, item4.当前位置, 1), 当前地图, 当前坐标, new HashSet<角色数据>());
					网络连接?.发送封包(new 玩家掉落装备
					{
						物品描述 = 物品实例2.物品数据.字节描述()
					});
					item4.当前持久.V--;
					网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = item4.字节描述()
					});
				}
			}
		}
	}

	public void 更新玩家战力()
	{
		int num = 0;
		foreach (int item in 战力加成.Values.ToList())
		{
			num += item;
		}
		当前战力 = num;
	}

	public void 宠物死亡处理(宠物实例 宠物)
	{
		宠物数据.Remove(宠物.宠物数据);
		宠物列表.Remove(宠物);
		if (宠物数量 == 0)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 9473
			});
		}
	}

	public void 玩家升级处理()
	{
		发送封包(new 角色等级提升
		{
			对象编号 = 地图编号,
			对象等级 = 当前等级
		});
		所属行会?.发送封包(new 同步会员信息
		{
			对象编号 = 地图编号,
			对象信息 = 当前地图.地图编号,
			当前等级 = 当前等级
		});
		战力加成[this] = 当前等级 * 10;
		更新玩家战力();
		属性加成[this] = 角色成长.获取数据(角色职业, 当前等级);
		更新对象属性();
		if (!对象死亡)
		{
			当前体力 = this[游戏对象属性.最大体力];
			当前魔力 = this[游戏对象属性.最大魔力];
		}
		所属师门?.发送封包(new 同步师徒等级
		{
			对象编号 = 地图编号,
			对象等级 = 当前等级
		});
		if (所属师门 != null && 所属队伍 != null && 所属师门.师父数据 != 角色数据 && 所属队伍.队伍成员.Contains(所属师门.师父数据))
		{
			所属师门.徒弟经验[角色数据] += (int)((float)角色成长.升级所需经验[当前等级] * 0.05f);
			所属师门.师父经验[角色数据] += (int)((float)角色成长.升级所需经验[当前等级] * 0.05f);
			if (本期特权 != 0)
			{
				所属师门.徒弟金币[角色数据] += (int)((float)角色成长.升级所需经验[当前等级] * 0.01f);
				所属师门.师父金币[角色数据] += (int)((float)角色成长.升级所需经验[当前等级] * 0.02f);
				所属师门.师父声望[角色数据] += (int)((float)角色成长.升级所需经验[当前等级] * 0.03f);
			}
		}
		if (当前等级 == 30 && 所属师门 == null)
		{
			网络连接?.发送封包(new 同步师门信息
			{
				师门参数 = 师门参数
			});
		}
		if (当前等级 >= 36 && 所属师门 != null && 所属师门.师父编号 != 地图编号)
		{
			提交出师申请();
		}
	}

	public void 玩家切换地图(地图实例 跳转地图, 地图区域类型 指定区域, Point 坐标 = default(Point))
	{
		清空邻居时处理();
		解绑网格();
		网络连接?.发送封包(new 玩家离开场景());
		当前坐标 = ((指定区域 == 地图区域类型.未知区域) ? 坐标 : 跳转地图.随机坐标(指定区域));
		if (当前地图.地图编号 == 跳转地图.地图编号)
		{
			网络连接?.发送封包(new 对象角色停止
			{
				对象编号 = 地图编号,
				对象坐标 = 当前坐标,
				对象高度 = 当前高度
			});
			网络连接?.发送封包(new 玩家进入场景
			{
				地图编号 = 当前地图.地图编号,
				当前坐标 = 当前坐标,
				当前高度 = 当前高度,
				路线编号 = 当前地图.路线编号,
				路线状态 = 当前地图.地图状态
			});
			绑定网格();
			更新邻居时处理();
			return;
		}
		bool 副本地图 = 当前地图.副本地图;
		当前地图 = 跳转地图;
		网络连接?.发送封包(new 玩家切换地图
		{
			地图编号 = 当前地图.地图编号,
			路线编号 = 当前地图.路线编号,
			对象坐标 = 当前坐标,
			对象高度 = 当前高度
		});
		if (!副本地图)
		{
			return;
		}
		foreach (宠物实例 item in 宠物列表)
		{
			item.宠物召回处理();
		}
	}

	public void 玩家增加经验(怪物实例 怪物, int 经验增加)
	{
		if (经验增加 <= 0 || (当前等级 >= 自定义类.游戏开放等级 && 当前经验 >= 所需经验))
		{
			return;
		}
		int num = 经验增加;
		int num2 = 0;
		if (怪物 != null)
		{
			num = (int)Math.Max(0.0, (double)num - Math.Round((float)num * 计算类.收益衰减(当前等级, 怪物.当前等级)));
			num = (int)((decimal)num * 自定义类.怪物经验倍率);
			if (当前等级 <= 自定义类.新手扶持等级)
			{
				num *= 2;
			}
			num2 = Math.Min(双倍经验, num);
		}
		int num3 = num + num2;
		双倍经验 -= num2;
		if (num3 <= 0)
		{
			return;
		}
		if ((当前经验 += num3) >= 所需经验 && 当前等级 < 自定义类.游戏开放等级)
		{
			while (当前经验 >= 所需经验)
			{
				当前经验 -= 所需经验;
				当前等级++;
			}
			玩家升级处理();
		}
		网络连接?.发送封包(new 角色经验变动
		{
			经验增加 = num3,
			今日增加 = 0,
			经验上限 = 10000000,
			双倍经验 = num2,
			当前经验 = 当前经验,
			升级所需 = 所需经验
		});
	}

	public void 技能增加经验(ushort 技能编号)
	{
		if (!主体技能表.TryGetValue(技能编号, out var v) || 当前等级 < v.升级等级)
		{
			return;
		}
		int num = 主程.随机数.Next(4);
		if (num > 0)
		{
			if (角色装备.TryGetValue(8, out var v2) && v2.装备名字 == "技巧项链")
			{
				num += num;
			}
			if ((v.技能经验.V += (ushort)num) >= v.升级经验)
			{
				v.技能经验.V -= v.升级经验;
				v.技能等级.V++;
				发送封包(new 玩家技能升级
				{
					技能编号 = v.技能编号.V,
					技能等级 = v.技能等级.V
				});
				战力加成[v] = v.战力加成;
				更新玩家战力();
				属性加成[v] = v.属性加成;
				更新对象属性();
			}
			网络连接?.发送封包(new 同步技能等级
			{
				技能编号 = v.技能编号.V,
				当前经验 = v.技能经验.V,
				当前等级 = v.技能等级.V
			});
		}
	}

	public bool 玩家学习技能(ushort 技能编号)
	{
		if (主体技能表.ContainsKey(技能编号))
		{
			return false;
		}
		主体技能表[技能编号] = new 技能数据(技能编号);
		网络连接?.发送封包(new 角色学习技能
		{
			角色编号 = 地图编号,
			技能编号 = 技能编号
		});
		if (主体技能表[技能编号].自动装配)
		{
			byte b = 0;
			while (b < 8)
			{
				if (角色数据.快捷栏位.ContainsKey(b))
				{
					b = (byte)(b + 1);
					continue;
				}
				角色数据.快捷栏位[b] = 主体技能表[技能编号];
				网络连接?.发送封包(new 角色拖动技能
				{
					技能栏位 = b,
					铭文编号 = 主体技能表[技能编号].铭文编号,
					技能编号 = 主体技能表[技能编号].技能编号.V,
					技能等级 = 主体技能表[技能编号].技能等级.V
				});
				break;
			}
		}
		if (角色装备.TryGetValue(0, out var v))
		{
			if (v.第一铭文?.技能编号 == 技能编号)
			{
				主体技能表[技能编号].铭文编号 = v.第一铭文.铭文编号;
				网络连接?.发送封包(new 角色装配铭文
				{
					技能编号 = 技能编号,
					铭文编号 = v.第一铭文.铭文编号
				});
			}
			if (v.第二铭文?.技能编号 == 技能编号)
			{
				主体技能表[技能编号].铭文编号 = v.第二铭文.铭文编号;
				网络连接?.发送封包(new 角色装配铭文
				{
					技能编号 = 技能编号,
					铭文编号 = v.第二铭文.铭文编号
				});
			}
		}
		foreach (ushort item in 主体技能表[技能编号].被动技能)
		{
			被动技能.Add(item, 主体技能表[技能编号]);
		}
		foreach (ushort item2 in 主体技能表[技能编号].技能Buff)
		{
			添加Buff时处理(item2, this);
		}
		战力加成[主体技能表[技能编号]] = 主体技能表[技能编号].战力加成;
		更新玩家战力();
		属性加成[主体技能表[技能编号]] = 主体技能表[技能编号].属性加成;
		更新对象属性();
		return true;
	}

	public void 玩家装卸铭文(ushort 技能编号, byte 铭文编号)
	{
		if (!主体技能表.TryGetValue(技能编号, out var v) || v.铭文编号 == 铭文编号)
		{
			return;
		}
		foreach (ushort item in v.被动技能)
		{
			被动技能.Remove(item);
		}
		foreach (ushort item2 in v.技能Buff)
		{
			if (Buff列表.ContainsKey(item2))
			{
				删除Buff时处理(item2);
			}
		}
		foreach (宠物实例 item3 in 宠物列表.ToList())
		{
			if (item3.绑定武器)
			{
				item3.自身死亡处理(null, 技能击杀: false);
			}
		}
		v.铭文编号 = 铭文编号;
		网络连接?.发送封包(new 角色装配铭文
		{
			铭文编号 = 铭文编号,
			技能编号 = 技能编号,
			技能等级 = v.技能等级.V
		});
		foreach (ushort item4 in v.被动技能)
		{
			被动技能.Add(item4, v);
		}
		foreach (ushort item5 in v.技能Buff)
		{
			添加Buff时处理(item5, this);
		}
		if (v.技能计数 != 0)
		{
			v.剩余次数.V = 0;
			v.计数时间 = 主程.当前时间.AddMilliseconds((int)v.计数周期);
			冷却记录[技能编号 | 0x1000000] = 主程.当前时间.AddMilliseconds((int)v.计数周期);
			网络连接?.发送封包(new 同步技能计数
			{
				技能编号 = v.技能编号.V,
				技能计数 = v.剩余次数.V,
				技能冷却 = v.计数周期
			});
		}
		战力加成[v] = v.战力加成;
		更新玩家战力();
		属性加成[v] = v.属性加成;
		更新对象属性();
	}

	public void 玩家穿卸装备(装备穿戴部位 装备部位, 装备数据 原有装备, 装备数据 现有装备)
	{
		if (装备部位 == 装备穿戴部位.武器 || 装备部位 == 装备穿戴部位.衣服 || 装备部位 == 装备穿戴部位.披风)
		{
			发送封包(new 同步角色外形
			{
				对象编号 = 地图编号,
				装备部位 = (byte)装备部位,
				装备编号 = (现有装备?.物品编号 ?? 0),
				升级次数 = (现有装备?.升级次数.V ?? 0)
			});
		}
		if (原有装备 != null)
		{
			if (原有装备.物品类型 == 物品使用分类.武器)
			{
				foreach (Buff数据 item in Buff列表.Values.ToList())
				{
					if (item.绑定武器 && (item.Buff来源 == null || item.Buff来源.地图编号 == 地图编号))
					{
						删除Buff时处理(item.Buff编号.V);
					}
				}
			}
			if (原有装备.物品类型 == 物品使用分类.武器)
			{
				foreach (宠物实例 item2 in 宠物列表.ToList())
				{
					if (item2.绑定武器)
					{
						item2.自身死亡处理(null, 技能击杀: false);
					}
				}
			}
			if (原有装备.第一铭文 != null)
			{
				玩家装卸铭文(原有装备.第一铭文.技能编号, 0);
			}
			if (原有装备.第二铭文 != null)
			{
				玩家装卸铭文(原有装备.第二铭文.技能编号, 0);
			}
			战力加成.Remove(原有装备);
			属性加成.Remove(原有装备);
		}
		if (现有装备 != null)
		{
			if (现有装备.第一铭文 != null)
			{
				玩家装卸铭文(现有装备.第一铭文.技能编号, 现有装备.第一铭文.铭文编号);
			}
			if (现有装备.第二铭文 != null)
			{
				玩家装卸铭文(现有装备.第二铭文.技能编号, 现有装备.第二铭文.铭文编号);
			}
			战力加成[现有装备] = 现有装备.装备战力;
			if (现有装备.当前持久.V > 0)
			{
				属性加成.Add(现有装备, 现有装备.装备属性);
			}
		}
		if (原有装备 != null || 现有装备 != null)
		{
			更新玩家战力();
			更新对象属性();
		}
	}

	public void 玩家诱惑目标(技能实例 技能, C_04_计算目标诱惑 参数, 地图对象 诱惑目标)
	{
		if (诱惑目标 == null || 诱惑目标.对象死亡 || 当前等级 + 2 < 诱惑目标.当前等级 || (!(诱惑目标 is 怪物实例) && !(诱惑目标 is 宠物实例)) || (诱惑目标 is 宠物实例 && (技能.技能等级 < 3 || this == (诱惑目标 as 宠物实例).宠物主人)) || (参数.检查铭文技能 && (!主体技能表.TryGetValue((ushort)(参数.检查铭文编号 / 10), out var v) || v.铭文编号 != 参数.检查铭文编号 % 10)))
		{
			return;
		}
		bool num = 参数.特定诱惑列表?.Contains(诱惑目标.对象名字) ?? false;
		bool flag = num;
		bool flag2 = num;
		float num2 = ((!flag) ? 0f : 参数.特定诱惑概率);
		float num3 = ((!(诱惑目标 is 怪物实例)) ? (诱惑目标 as 宠物实例).基础诱惑概率 : (诱惑目标 as 怪物实例).基础诱惑概率);
		if ((num3 += num2) <= 0f)
		{
			return;
		}
		int num4 = ((参数.基础诱惑数量?.Length > 技能.技能等级) ? 参数.基础诱惑数量[技能.技能等级] : 0);
		int num5 = ((参数.初始宠物等级?.Length > 技能.技能等级) ? 参数.初始宠物等级[技能.技能等级] : 0);
		byte 额外诱惑数量 = 参数.额外诱惑数量;
		float 额外诱惑概率 = 参数.额外诱惑概率;
		int 额外诱惑时长 = 参数.额外诱惑时长;
		float num6 = 0f;
		int num7 = 0;
		int num8 = 0;
		foreach (Buff数据 value in Buff列表.Values)
		{
			if ((value.Buff效果 & Buff效果类型.诱惑提升) != 0)
			{
				num6 += value.Buff模板.诱惑概率增加;
				num7 += value.Buff模板.诱惑时长增加;
				num8 += value.Buff模板.诱惑等级增加;
			}
		}
		float num9 = (float)Math.Pow((当前等级 >= 诱惑目标.当前等级) ? 1.2 : 0.8, 计算类.数值限制(0, Math.Abs(诱惑目标.当前等级 - 当前等级), 2));
		if (计算类.计算概率(num3 * num9 * (1f + 额外诱惑概率 + num6)))
		{
			if (!诱惑目标.Buff列表.ContainsKey(参数.狂暴状态编号))
			{
				诱惑目标.添加Buff时处理(参数.瘫痪状态编号, this);
			}
			else if (宠物列表.Count < num4 + 额外诱惑数量)
			{
				int num10 = Math.Min(num5 + num8, 7);
				int 宠物时长 = 自定义类.怪物诱惑时长 + 额外诱惑时长 + num7;
				bool 绑定武器 = flag2 || num5 != 0 || 额外诱惑时长 != 0 || 额外诱惑概率 != 0f || 宠物列表.Count >= num4;
				宠物实例 宠物实例2 = ((!(诱惑目标 is 怪物实例 怪物实例2)) ? new 宠物实例(this, (宠物实例)诱惑目标, (byte)num10, 绑定武器, 宠物时长) : new 宠物实例(this, 怪物实例2, (byte)Math.Max(怪物实例2.宠物等级, num10), 绑定武器, 宠物时长));
				网络连接?.发送封包(new 同步宠物等级
				{
					宠物编号 = 宠物实例2.地图编号,
					宠物等级 = 宠物实例2.宠物等级
				});
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 9473,
					第一参数 = (int)宠物模式
				});
				宠物数据.Add(宠物实例2.宠物数据);
				宠物列表.Add(宠物实例2);
			}
		}
	}

	public void 玩家瞬间移动(技能实例 技能, C_07_计算目标瞬移 参数)
	{
		if (!计算类.计算概率(参数.每级成功概率[技能.技能等级]) || 当前地图.随机传送(当前坐标) == default(Point))
		{
			添加Buff时处理(参数.瞬移失败提示, this);
			添加Buff时处理(参数.失败添加Buff, this);
		}
		else
		{
			玩家切换地图(复活地图, 地图区域类型.随机区域);
		}
		if (参数.增加技能经验)
		{
			技能增加经验(参数.经验技能编号);
		}
	}

	public void 扣除护盾时间(int 技能伤害)
	{
		foreach (Buff数据 item in Buff列表.Values.ToList())
		{
			if (item.Buff分组 == 2535)
			{
				if (!((item.剩余时间.V -= TimeSpan.FromSeconds(Math.Min(15f, (float)技能伤害 * 15f / 50f))) < TimeSpan.Zero))
				{
					发送封包(new 对象状态变动
					{
						对象编号 = 地图编号,
						Buff编号 = item.Buff编号.V,
						Buff索引 = item.Buff编号.V,
						当前层数 = item.当前层数.V,
						剩余时间 = (int)item.剩余时间.V.TotalMilliseconds,
						持续时间 = (int)item.持续时间.V.TotalMilliseconds
					});
				}
				else
				{
					删除Buff时处理(item.Buff编号.V);
				}
			}
		}
	}

	public void 武器损失持久()
	{
		if (角色装备.TryGetValue(0, out var v) && v.当前持久.V > 0 && v.当前持久.V > 0 && (本期特权 != 5 || !v.能否修理) && (本期特权 != 4 || !计算类.计算概率(0.5f)))
		{
			int num2 = (v.当前持久.V = Math.Max(0, v.当前持久.V - 主程.随机数.Next(1, 6)));
			if (num2 <= 0 && 属性加成.Remove(v))
			{
				更新对象属性();
			}
			网络连接?.发送封包(new 装备持久改变
			{
				装备容器 = v.物品容器.V,
				装备位置 = v.物品位置.V,
				当前持久 = v.当前持久.V
			});
		}
	}

	public void 武器幸运损失()
	{
		if (角色装备.TryGetValue(0, out var v) && v.幸运等级.V > -9 && 计算类.计算概率(0.1f))
		{
			v.幸运等级.V--;
			网络连接.发送封包(new 玩家物品变动
			{
				物品描述 = v.字节描述()
			});
		}
	}

	public void 战具损失持久(int 损失持久)
	{
		if (角色装备.TryGetValue(15, out var v))
		{
			if ((v.当前持久.V -= 损失持久) <= 0)
			{
				网络连接?.发送封包(new 删除玩家物品
				{
					背包类型 = v.物品容器.V,
					物品位置 = v.物品位置.V
				});
				玩家穿卸装备(装备穿戴部位.战具, v, null);
				角色装备.Remove(v.物品位置.V);
				v.删除数据();
			}
			else
			{
				网络连接?.发送封包(new 装备持久改变
				{
					装备容器 = v.物品容器.V,
					装备位置 = v.物品位置.V,
					当前持久 = v.当前持久.V
				});
			}
		}
	}

	public void 装备损失持久(int 损失持久)
	{
		损失持久 = Math.Min(10, 损失持久);
		foreach (装备数据 value in 角色装备.Values)
		{
			if (value.当前持久.V > 0 && (本期特权 != 5 || !value.能否修理) && (本期特权 != 4 || !计算类.计算概率(0.5f)) && value.持久类型 == 物品持久分类.装备 && 计算类.计算概率((value.物品类型 == 物品使用分类.衣服) ? 1f : 0.1f))
			{
				int num2 = (value.当前持久.V = Math.Max(0, value.当前持久.V - 损失持久));
				if (num2 <= 0 && 属性加成.Remove(value))
				{
					更新对象属性();
				}
				网络连接?.发送封包(new 装备持久改变
				{
					装备容器 = value.物品容器.V,
					装备位置 = value.物品位置.V,
					当前持久 = value.当前持久.V
				});
			}
		}
	}

	public void 玩家特权到期()
	{
		if (本期特权 == 3)
		{
			玩家称号到期(61);
		}
		else if (本期特权 != 4)
		{
			if (本期特权 == 5)
			{
				玩家称号到期(131);
			}
		}
		else
		{
			玩家称号到期(124);
		}
		上期特权 = 本期特权;
		上期记录 = 本期记录;
		上期日期 = 本期日期;
		本期特权 = 0;
		本期记录 = 0u;
		本期日期 = default(DateTime);
		特权时间 = DateTime.MaxValue;
	}

	public void 玩家激活特权(byte 特权类型)
	{
		switch (特权类型)
		{
		default:
			return;
		case 3:
			玩家获得称号(61);
			break;
		case 4:
			玩家获得称号(124);
			break;
		case 5:
			玩家获得称号(131);
			break;
		}
		本期特权 = 特权类型;
		本期记录 = uint.MaxValue;
		本期日期 = 主程.当前时间;
		特权时间 = 本期日期.AddDays(30.0);
	}

	public void 玩家称号到期(byte 称号编号)
	{
		if (称号列表.Remove(称号编号))
		{
			if (当前称号 == 称号编号)
			{
				当前称号 = 0;
				战力加成.Remove(称号编号);
				更新玩家战力();
				属性加成.Remove(称号编号);
				更新对象属性();
				发送封包(new 同步装配称号
				{
					对象编号 = 地图编号
				});
			}
			网络连接?.发送封包(new 玩家失去称号
			{
				称号编号 = 称号编号
			});
		}
	}

	public void 玩家获得称号(byte 称号编号)
	{
		if (游戏称号.数据表.TryGetValue(称号编号, out var value))
		{
			称号列表[称号编号] = 主程.当前时间.AddMinutes(value.有效时间);
			网络连接?.发送封包(new 玩家获得称号
			{
				称号编号 = 称号编号,
				剩余时间 = (int)(称号列表[称号编号] - 主程.当前时间).TotalMinutes
			});
		}
	}

	public void 玩家获得仇恨(地图对象 对象)
	{
		foreach (宠物实例 item in 宠物列表.ToList())
		{
			if (item.邻居列表.Contains(对象) && !对象.检查状态(游戏对象状态.隐身状态 | 游戏对象状态.潜行状态))
			{
				item.对象仇恨.添加仇恨(对象, default(DateTime), 0);
			}
		}
	}

	public bool 查找背包物品(int 物品编号, out 物品数据 物品)
	{
		byte b = 0;
		while (true)
		{
			if (b < 背包大小)
			{
				if (角色背包.TryGetValue(b, out 物品) && 物品.物品编号 == 物品编号)
				{
					break;
				}
				b = (byte)(b + 1);
				continue;
			}
			物品 = null;
			return false;
		}
		return true;
	}

	public bool 查找背包物品(int 所需总数, int 物品编号, out List<物品数据> 物品列表)
	{
		物品列表 = new List<物品数据>();
		byte b = 0;
		while (true)
		{
			if (b < 背包大小)
			{
				if (角色背包.TryGetValue(b, out var v) && v.物品编号 == 物品编号)
				{
					物品列表.Add(v);
					if ((所需总数 -= v.当前持久.V) <= 0)
					{
						break;
					}
				}
				b = (byte)(b + 1);
				continue;
			}
			return false;
		}
		return true;
	}

	public bool 查找背包物品(int 所需总数, HashSet<int> 物品编号, out List<物品数据> 物品列表)
	{
		物品列表 = new List<物品数据>();
		for (byte b = 0; b < 背包大小; b = (byte)(b + 1))
		{
			if (角色背包.TryGetValue(b, out var v) && 物品编号.Contains(v.物品编号))
			{
				物品列表.Add(v);
				if ((所需总数 -= v.当前持久.V) <= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void 消耗背包物品(int 消耗总数, 物品数据 当前物品)
	{
		if ((当前物品.当前持久.V -= 消耗总数) <= 0)
		{
			网络连接?.发送封包(new 删除玩家物品
			{
				背包类型 = 当前物品.物品容器.V,
				物品位置 = 当前物品.物品位置.V
			});
			角色背包.Remove(当前物品.物品位置.V);
			当前物品.删除数据();
		}
		else
		{
			网络连接?.发送封包(new 玩家物品变动
			{
				物品描述 = 当前物品.字节描述()
			});
		}
	}

	public void 消耗背包物品(int 消耗总数, List<物品数据> 物品列表)
	{
		物品列表.OrderBy((物品数据 O) => O.物品位置);
		foreach (物品数据 item in 物品列表)
		{
			int num = Math.Min(消耗总数, item.当前持久.V);
			消耗背包物品(num, item);
			if ((消耗总数 -= num) <= 0)
			{
				break;
			}
		}
	}

	public void 玩家角色下线()
	{
		当前交易?.结束交易();
		所属队伍?.发送封包(new 同步队员状态
		{
			对象编号 = 地图编号,
			状态编号 = 1
		});
		所属行会?.发送封包(new 同步会员信息
		{
			对象编号 = 地图编号,
			对象信息 = 计算类.时间转换(主程.当前时间)
		});
		foreach (角色数据 item in 粉丝列表)
		{
			item.网络连接?.发送封包(new 好友上线下线
			{
				对象编号 = 地图编号,
				对象名字 = 对象名字,
				对象职业 = (byte)角色职业,
				对象性别 = (byte)角色性别,
				上线下线 = 3
			});
		}
		foreach (角色数据 item2 in 仇恨列表)
		{
			item2.网络连接?.发送封包(new 好友上线下线
			{
				对象编号 = 地图编号,
				对象名字 = 对象名字,
				对象职业 = (byte)角色职业,
				对象性别 = (byte)角色性别,
				上线下线 = 3
			});
		}
		foreach (宠物实例 item3 in 宠物列表.ToList())
		{
			item3.宠物沉睡处理();
		}
		foreach (Buff数据 item4 in Buff列表.Values.ToList())
		{
			if (item4.下线消失)
			{
				删除Buff时处理(item4.Buff编号.V);
			}
		}
		角色数据.角色下线();
		删除对象();
		当前地图.玩家列表.Remove(this);
	}

	public void 玩家进入场景()
	{
		网络连接?.发送封包(new 对象角色停止
		{
			对象编号 = 地图编号,
			对象坐标 = 当前坐标,
			对象高度 = 当前高度
		});
		网络连接?.发送封包(new 玩家进入场景
		{
			地图编号 = 地图编号,
			当前坐标 = 当前坐标,
			当前高度 = 当前高度,
			路线编号 = 当前地图.路线编号,
			路线状态 = 当前地图.地图状态
		});
		网络连接?.发送封包(new 对象进入视野
		{
			出现方式 = 1,
			对象编号 = 地图编号,
			现身坐标 = 当前坐标,
			现身高度 = 当前高度,
			现身方向 = (ushort)当前方向,
			现身姿态 = (byte)((!对象死亡) ? 1u : 13u),
			体力比例 = (byte)(当前体力 * 100 / this[游戏对象属性.最大体力])
		});
		网络连接?.发送封包(new 同步对象体力
		{
			对象编号 = 地图编号,
			当前体力 = 当前体力,
			体力上限 = this[游戏对象属性.最大体力]
		});
		网络连接?.发送封包(new 同步对象魔力
		{
			当前魔力 = 当前魔力
		});
		网络连接?.发送封包(new 同步元宝数量
		{
			元宝数量 = 元宝数量
		});
		网络连接?.发送封包(new 同步冷却列表
		{
			字节描述 = 全部冷却描述()
		});
		网络连接?.发送封包(new 同步状态列表
		{
			字节数据 = 全部Buff描述()
		});
		网络连接?.发送封包(new 切换战斗姿态
		{
			角色编号 = 地图编号
		});
		绑定网格();
		更新邻居时处理();
		if (游戏技能.数据表.TryGetValue("通用-玩家取出武器", out var value))
		{
			new 技能实例(this, value, null, base.动作编号, 当前地图, 当前坐标, null, 当前坐标, null);
		}
		if (宠物列表.Count == 宠物数据.Count)
		{
			return;
		}
		foreach (宠物数据 item in 宠物数据.ToList())
		{
			if (!(主程.当前时间 >= item.叛变时间.V) && 游戏怪物.数据表.ContainsKey(item.宠物名字.V))
			{
				宠物实例 宠物实例2 = new 宠物实例(this, item);
				宠物列表.Add(宠物实例2);
				网络连接?.发送封包(new 同步宠物等级
				{
					宠物编号 = 宠物实例2.地图编号,
					宠物等级 = 宠物实例2.宠物等级
				});
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 9473,
					第一参数 = (int)宠物模式
				});
			}
			else
			{
				item.删除数据();
				宠物数据.Remove(item);
			}
		}
	}

	public void 玩家退出副本()
	{
		if (!对象死亡)
		{
			玩家切换地图(地图处理网关.分配地图(重生地图), 地图区域类型.复活区域);
		}
		else
		{
			玩家请求复活();
		}
	}

	public void 玩家请求复活()
	{
		if (!对象死亡)
		{
			return;
		}
		网络连接?.发送封包(new 玩家角色复活
		{
			对象编号 = 地图编号,
			复活方式 = 3
		});
		当前体力 = (int)((float)this[游戏对象属性.最大体力] * 0.3f);
		当前魔力 = (int)((float)this[游戏对象属性.最大魔力] * 0.3f);
		对象死亡 = false;
		阻塞网格 = true;
		if (当前地图 == 地图处理网关.沙城地图 && 地图处理网关.沙城节点 >= 2)
		{
			if (所属行会 != null && 所属行会 == 系统数据.数据.占领行会.V)
			{
				玩家切换地图(当前地图, 地图区域类型.未知区域, 地图处理网关.守方传送区域.随机坐标);
			}
			else if (所属行会 != null && 所属行会 == 地图处理网关.八卦坛激活行会)
			{
				玩家切换地图(当前地图, 地图区域类型.未知区域, 地图处理网关.内城复活区域.随机坐标);
			}
			else
			{
				玩家切换地图(当前地图, 地图区域类型.未知区域, 地图处理网关.外城复活区域.随机坐标);
			}
		}
		else
		{
			玩家切换地图(复活地图, (!红名玩家) ? 地图区域类型.复活区域 : 地图区域类型.红名区域);
		}
	}

	public void 玩家进入法阵(int 法阵编号)
	{
		if (!绑定地图)
		{
			return;
		}
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3)
		{
			游戏地图 value2;
			if (!当前地图.法阵列表.TryGetValue((byte)法阵编号, out var value))
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 775
				});
			}
			else if (网格距离(value.所处坐标) >= 8)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 4609
				});
			}
			else if (游戏地图.数据表.TryGetValue(value.跳转地图, out value2))
			{
				if (当前等级 < value2.限制等级)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 4624
					});
				}
				else
				{
					玩家切换地图((当前地图.地图编号 == value2.地图编号) ? 当前地图 : 地图处理网关.分配地图(value2.地图编号), 地图区域类型.未知区域, value.跳转坐标);
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 775
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 769
			});
		}
	}

	public void 玩家角色走动(Point 终点坐标)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (!(当前坐标 == 终点坐标))
		{
			if (!能否走动())
			{
				网络连接?.发送封包(new 对象角色停止
				{
					对象编号 = 地图编号,
					对象坐标 = 当前坐标,
					对象高度 = 当前高度
				});
				return;
			}
			游戏方向 方向 = 计算类.计算方向(当前坐标, 终点坐标);
			Point point = 计算类.前方坐标(当前坐标, 方向, 1);
			if (!当前地图.能否通行(point))
			{
				if (当前方向 != (方向 = 计算类.计算方向(当前坐标, point)))
				{
					角色数据.当前朝向.V = 方向;
					发送封包(new 对象转动方向
					{
						对象编号 = 地图编号,
						对象朝向 = (ushort)方向,
						转向耗时 = 100
					});
				}
				发送封包(new 对象角色停止
				{
					对象编号 = 地图编号,
					对象坐标 = 当前坐标,
					对象高度 = 当前高度
				});
				return;
			}
			行走时间 = 主程.当前时间.AddMilliseconds(行走耗时);
			忙碌时间 = 主程.当前时间.AddMilliseconds(行走耗时);
			if (当前方向 != (方向 = 计算类.计算方向(当前坐标, point)))
			{
				角色数据.当前朝向.V = 方向;
				发送封包(new 对象转动方向
				{
					对象编号 = 地图编号,
					对象朝向 = (ushort)方向,
					转向耗时 = 100
				});
			}
			发送封包(new 对象角色走动
			{
				对象编号 = 地图编号,
				移动坐标 = point,
				移动速度 = base.行走速度
			});
			自身移动时处理(point);
		}
		else
		{
			网络连接?.发送封包(new 对象角色停止
			{
				对象编号 = 地图编号,
				对象坐标 = 当前坐标,
				对象高度 = 当前高度
			});
		}
	}

	public void 玩家角色跑动(Point 终点坐标)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (当前坐标 == 终点坐标)
		{
			网络连接?.发送封包(new 对象角色停止
			{
				对象编号 = 地图编号,
				对象坐标 = 当前坐标,
				对象高度 = 当前高度
			});
			return;
		}
		if (!能否跑动())
		{
			if (!能否走动())
			{
				发送封包(new 对象角色停止
				{
					对象编号 = 地图编号,
					对象坐标 = 当前坐标,
					对象高度 = 当前高度
				});
			}
			else
			{
				玩家角色走动(终点坐标);
			}
			return;
		}
		游戏方向 方向 = 计算类.计算方向(当前坐标, 终点坐标);
		Point point = 计算类.前方坐标(当前坐标, 方向, 1);
		Point point2 = 计算类.前方坐标(当前坐标, 方向, 2);
		if (当前地图.能否通行(point))
		{
			if (当前地图.能否通行(point2))
			{
				奔跑时间 = 主程.当前时间.AddMilliseconds(奔跑耗时);
				忙碌时间 = 主程.当前时间.AddMilliseconds(奔跑耗时);
				if (当前方向 != (方向 = 计算类.计算方向(当前坐标, point2)))
				{
					角色数据.当前朝向.V = 方向;
					发送封包(new 对象转动方向
					{
						对象编号 = 地图编号,
						对象朝向 = (ushort)方向,
						转向耗时 = 100
					});
				}
				发送封包(new 对象角色跑动
				{
					对象编号 = 地图编号,
					移动坐标 = point2,
					移动耗时 = base.奔跑速度
				});
				自身移动时处理(point2);
			}
			else
			{
				玩家角色走动(终点坐标);
			}
		}
		else
		{
			if (当前方向 != (方向 = 计算类.计算方向(当前坐标, point)))
			{
				角色数据.当前朝向.V = 方向;
				发送封包(new 对象转动方向
				{
					对象编号 = 地图编号,
					对象朝向 = (ushort)方向,
					转向耗时 = 100
				});
			}
			发送封包(new 对象角色停止
			{
				对象编号 = 地图编号,
				对象坐标 = 当前坐标,
				对象高度 = 当前高度
			});
		}
	}

	public void 玩家角色转动(游戏方向 转动方向)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3 && 能否转动())
		{
			当前方向 = 转动方向;
		}
	}

	public void 玩家切换姿态()
	{
	}

	public void 玩家开关技能(ushort 技能编号)
	{
		if (对象死亡)
		{
			return;
		}
		if (主体技能表.TryGetValue(技能编号, out var v) || 被动技能.TryGetValue(技能编号, out v))
		{
			foreach (string item in v.铭文模板.开关技能列表.ToList())
			{
				if (游戏技能.数据表.TryGetValue(item, out var value))
				{
					if (主体技能表.TryGetValue(value.绑定等级编号, out var v2) && value.需要消耗魔法?.Length > v2.技能等级.V)
					{
						if (当前魔力 < value.需要消耗魔法[v2.技能等级.V])
						{
							continue;
						}
						当前魔力 -= value.需要消耗魔法[v2.技能等级.V];
					}
					new 技能实例(this, value, v, 0, 当前地图, 当前坐标, this, 当前坐标, null);
					break;
				}
			}
			return;
		}
		this?.网络连接.尝试断开连接(new Exception("释放未学会的技能, 尝试断开连接."));
	}

	public void 玩家释放技能(ushort 技能编号, byte 动作编号, int 目标编号, Point 技能锚点)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (!主体技能表.TryGetValue(技能编号, out var v) && !被动技能.TryGetValue(技能编号, out v))
		{
			网络连接.尝试断开连接(new Exception($"错误操作: 玩家释放技能. 错误: 没有学会技能. 技能编号:{技能编号}"));
			return;
		}
		if (!冷却记录.TryGetValue(技能编号 | 0x1000000, out var v2) || !(主程.当前时间 < v2))
		{
			if (角色职业 == 游戏对象职业.刺客)
			{
				foreach (Buff数据 item in Buff列表.Values.ToList())
				{
					if ((item.Buff效果 & Buff效果类型.状态标志) != 0 && (item.Buff模板.角色所处状态 & 游戏对象状态.潜行状态) != 0)
					{
						移除Buff时处理(item.Buff编号.V);
					}
				}
			}
			地图处理网关.地图对象表.TryGetValue(目标编号, out var value);
			{
				foreach (string item2 in v.铭文模板.主体技能列表.ToList())
				{
					int num = 0;
					int num2 = 0;
					List<物品数据> 物品列表 = null;
					if (!游戏技能.数据表.TryGetValue(item2, out var value2) || value2.自身技能编号 != 技能编号)
					{
						continue;
					}
					if (value2.技能分组编号 == 0 || !冷却记录.TryGetValue(value2.技能分组编号 | 0, out var v3) || !(主程.当前时间 < v3))
					{
						if (value2.检查职业武器 && (!角色装备.TryGetValue(0, out var v4) || v4.需要职业 != 角色职业))
						{
							break;
						}
						if (value2.检查技能标记 && !Buff列表.ContainsKey(value2.技能标记编号))
						{
							continue;
						}
						if ((value2.检查被动标记 && this[游戏对象属性.技能标志] != 1) || (value2.检查技能计数 && v.剩余次数.V <= 0))
						{
							break;
						}
						if (!value2.检查忙绿状态 || !(主程.当前时间 < 忙碌时间))
						{
							if (!value2.检查硬直状态 || !(主程.当前时间 < 硬直时间))
							{
								if (value2.计算幸运概率 || value2.计算触发概率 < 1f)
								{
									if (value2.计算幸运概率)
									{
										if (!计算类.计算概率(计算类.计算幸运(this[游戏对象属性.幸运等级])))
										{
											continue;
										}
									}
									else
									{
										float num3 = 0f;
										if (value2.属性提升概率 != 0)
										{
											num3 = Math.Max(0f, (float)this[value2.属性提升概率] * value2.属性提升系数);
										}
										if (!计算类.计算概率(value2.计算触发概率 + num3))
										{
											continue;
										}
									}
								}
								if ((value2.验证已学技能 != 0 && (!主体技能表.TryGetValue(value2.验证已学技能, out var v5) || (value2.验证技能铭文 != 0 && value2.验证技能铭文 != v5.铭文编号))) || (value2.验证角色Buff != 0 && (!Buff列表.TryGetValue(value2.验证角色Buff, out var v6) || v6.当前层数.V < value2.角色Buff层数)) || (value2.验证目标Buff != 0 && (value == null || !value.Buff列表.TryGetValue(value2.验证目标Buff, out var v7) || v7.当前层数.V < value2.目标Buff层数)) || (value2.验证目标类型 != 0 && (value == null || !value.特定类型(this, value2.验证目标类型))) || (主体技能表.TryGetValue(value2.绑定等级编号, out var v8) && value2.需要消耗魔法?.Length > v8.技能等级.V && 当前魔力 < (num = value2.需要消耗魔法[v8.技能等级.V])))
								{
									continue;
								}
								HashSet<int> 需要消耗物品 = value2.需要消耗物品;
								if (需要消耗物品 != null && 需要消耗物品.Count != 0)
								{
									if (!角色装备.TryGetValue(15, out var v9) || v9.当前持久.V < value2.战具扣除点数)
									{
										if (!查找背包物品(value2.消耗物品数量, value2.需要消耗物品, out 物品列表))
										{
											continue;
										}
										num2 = value2.消耗物品数量;
									}
									else
									{
										物品列表 = new List<物品数据> { v9 };
										num2 = value2.战具扣除点数;
									}
								}
								if (num >= 0)
								{
									当前魔力 -= num;
								}
								if (物品列表 == null || 物品列表.Count != 1 || 物品列表[0].物品类型 != 物品使用分类.战具)
								{
									if (物品列表 != null)
									{
										消耗背包物品(num2, 物品列表);
									}
								}
								else
								{
									战具损失持久(num2);
								}
								if (value2.检查被动标记 && this[游戏对象属性.技能标志] == 1)
								{
									this[游戏对象属性.技能标志] = 0;
								}
								new 技能实例(this, value2, v, 动作编号, 当前地图, 当前坐标, value, 技能锚点, null);
								break;
							}
							网络连接?.发送封包(new 添加技能冷却
							{
								冷却编号 = (技能编号 | 0x1000000),
								冷却时间 = (int)(硬直时间 - 主程.当前时间).TotalMilliseconds
							});
							网络连接?.发送封包(new 技能释放完成
							{
								技能编号 = 技能编号,
								动作编号 = 动作编号
							});
							continue;
						}
						网络连接?.发送封包(new 添加技能冷却
						{
							冷却编号 = (技能编号 | 0x1000000),
							冷却时间 = (int)(忙碌时间 - 主程.当前时间).TotalMilliseconds
						});
						网络连接?.发送封包(new 技能释放完成
						{
							技能编号 = 技能编号,
							动作编号 = 动作编号
						});
						break;
					}
					网络连接?.发送封包(new 添加技能冷却
					{
						冷却编号 = (技能编号 | 0x1000000),
						冷却时间 = (int)(v3 - 主程.当前时间).TotalMilliseconds
					});
					网络连接?.发送封包(new 技能释放完成
					{
						技能编号 = 技能编号,
						动作编号 = 动作编号
					});
					break;
				}
				return;
			}
		}
		网络连接?.发送封包(new 添加技能冷却
		{
			冷却编号 = (技能编号 | 0x1000000),
			冷却时间 = (int)(v2 - 主程.当前时间).TotalMilliseconds
		});
		网络连接?.发送封包(new 技能释放完成
		{
			技能编号 = 技能编号,
			动作编号 = 动作编号
		});
		网络连接?.发送封包(new 游戏错误提示
		{
			错误代码 = 1281,
			第一参数 = 技能编号,
			第二参数 = 动作编号
		});
	}

	public void 更改攻击模式(攻击模式 模式)
	{
		攻击模式 = 模式;
	}

	public void 更改宠物模式(宠物模式 模式)
	{
		if (宠物数量 == 0)
		{
			return;
		}
		if (宠物模式 == 宠物模式.休息 && (模式 == 宠物模式.自动 || 模式 == 宠物模式.攻击))
		{
			foreach (宠物实例 item in 宠物列表.ToList())
			{
				item.对象仇恨.仇恨列表.Clear();
			}
			宠物模式 = 宠物模式.攻击;
		}
		else if (宠物模式 == 宠物模式.攻击 && (模式 == 宠物模式.自动 || 模式 == 宠物模式.休息))
		{
			宠物模式 = 宠物模式.休息;
		}
	}

	public void 玩家拖动技能(byte 技能栏位, ushort 技能编号)
	{
		if (技能栏位 <= 7 || 技能栏位 >= 32)
		{
			return;
		}
		if (!主体技能表.TryGetValue(技能编号, out var v))
		{
			if (快捷栏位.TryGetValue(技能栏位, out var v2))
			{
				快捷栏位.Remove(技能栏位);
				v2.快捷栏位.V = 100;
			}
		}
		else if (!v.自动装配 && v.快捷栏位.V != 技能栏位)
		{
			快捷栏位.Remove(v.快捷栏位.V);
			v.快捷栏位.V = 100;
			if (快捷栏位.TryGetValue(技能栏位, out var v3) && v3 != null)
			{
				v3.快捷栏位.V = 100;
			}
			快捷栏位[技能栏位] = v;
			v.快捷栏位.V = 技能栏位;
			网络连接?.发送封包(new 角色拖动技能
			{
				技能栏位 = 技能栏位,
				铭文编号 = v.铭文编号,
				技能编号 = v.技能编号.V,
				技能等级 = v.技能等级.V
			});
		}
	}

	public void 玩家选中对象(int 对象编号)
	{
		if (地图处理网关.地图对象表.TryGetValue(对象编号, out var value))
		{
			网络连接?.发送封包(new 玩家选中目标
			{
				角色编号 = 地图编号,
				目标编号 = value.地图编号
			});
			网络连接?.发送封包(new 选中目标详情
			{
				对象编号 = value.地图编号,
				当前体力 = value.当前体力,
				当前魔力 = value.当前魔力,
				最大体力 = value[游戏对象属性.最大体力],
				最大魔力 = value[游戏对象属性.最大魔力],
				Buff描述 = value.对象Buff详述()
			});
		}
	}

	public void 开始Npcc对话(int 对象编号)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3)
		{
			if (!地图处理网关.守卫对象表.TryGetValue(对象编号, out 对话守卫))
			{
				网络连接.尝试断开连接(new Exception("错误操作: 开始Npcc对话. 错误: 没有找到对象."));
			}
			else if (当前地图 != 对话守卫.当前地图)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 开始Npcc对话. 错误: 跨越地图对话."));
			}
			else if (网格距离(对话守卫) > 12)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 开始Npcc对话. 错误: 超长距离对话."));
			}
			else if (对话数据.数据表.ContainsKey(对话守卫.模板编号 * 100000))
			{
				打开商店 = 对话守卫.商店编号;
				打开界面 = 对话守卫.界面代码;
				对话超时 = 主程.当前时间.AddSeconds(30.0);
				对话页面 = 对话守卫.模板编号 * 100000;
				网络连接?.发送封包(new 同步交互结果
				{
					对象编号 = 对话守卫.地图编号,
					交互文本 = 对话数据.字节数据(对话页面)
				});
			}
		}
	}

	public void 继续Npcc对话(int 选项编号)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (对话守卫 == null)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 继续Npcc对话.  错误: 没有选中守卫."));
		}
		else if (当前地图 != 对话守卫.当前地图)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 开始Npcc对话. 错误: 跨越地图对话."));
		}
		else if (网格距离(对话守卫) <= 12)
		{
			if (主程.当前时间 > 对话超时)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 3333
				});
				return;
			}
			对话超时 = 主程.当前时间.AddSeconds(30.0);
			装备数据 v2;
			switch (对话页面)
			{
			case 479600000:
				if (选项编号 == 1)
				{
					玩家切换地图(地图处理网关.分配地图(重生地图), 地图区域类型.复活区域);
				}
				break;
			case 479500000:
				if (选项编号 != 1)
				{
					break;
				}
				if (主程.当前时间.Hour == 自定义类.武斗场时间一 || 主程.当前时间.Hour == 自定义类.武斗场时间二)
				{
					if (主程.当前时间.Hour == 角色数据.武斗日期.V.Hour)
					{
						对话页面 = 479502000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
						break;
					}
					if (当前等级 < 25)
					{
						对话页面 = 711900001;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{25}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					if (金币数量 < 50000)
					{
						对话页面 = 479503000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{50000}>")
						});
					}
					else
					{
						金币数量 -= 50000;
						角色数据.武斗日期.V = 主程.当前时间;
						玩家切换地图((当前地图.地图编号 == 183) ? 当前地图 : 地图处理网关.分配地图(183), 地图区域类型.传送区域);
					}
				}
				else
				{
					对话页面 = 479501000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{自定义类.武斗场时间一}><#P1:{自定义类.武斗场时间二}>")
					});
				}
				break;
			case 611400000:
			{
				int num4 = 30;
				int num5 = 100000;
				int num6 = 223;
				if (选项编号 != 1)
				{
					break;
				}
				if (当前等级 >= num4)
				{
					if (金币数量 >= num5)
					{
						金币数量 -= num5;
						玩家切换地图((当前地图.地图编号 == num6) ? 当前地图 : 地图处理网关.分配地图(num6), 地图区域类型.传送区域);
						break;
					}
					对话页面 = 711900002;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num5}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else
				{
					对话页面 = 711900001;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num4}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 611300000:
			{
				int num13 = 0;
				int num14 = 0;
				int num15 = 147;
				if (选项编号 == 1)
				{
					if (当前等级 < num13)
					{
						对话页面 = 711900001;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num13}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					else if (金币数量 < num14)
					{
						对话页面 = 711900002;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num14}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					else
					{
						金币数量 -= num14;
						玩家切换地图((当前地图.地图编号 == num15) ? 当前地图 : 地图处理网关.分配地图(num15), 地图区域类型.复活区域);
					}
				}
				break;
			}
			case 612600000:
				switch (选项编号)
				{
				case 1:
					if (!角色装备.TryGetValue(0, out v2))
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						对话页面 = 612604000;
						重铸部位 = 0;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					break;
				case 2:
					对话页面 = 612601000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 3:
					对话页面 = 612602000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				}
				break;
			case 612300000:
				if (地图处理网关.沙城节点 >= 2 && 所属行会 != null && 所属行会 == 地图处理网关.八卦坛激活行会)
				{
					玩家切换地图(地图处理网关.沙城地图, 地图区域类型.未知区域, 地图处理网关.皇宫随机区域.随机坐标);
				}
				break;
			case 612604000:
			{
				if (选项编号 != 1)
				{
					break;
				}
				if (!角色装备.TryGetValue(重铸部位, out var v3))
				{
					对话页面 = 612603000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				}
				int num28 = 500000;
				int num29 = 1;
				int 重铸所需灵气 = v3.重铸所需灵气;
				if (金币数量 >= 500000)
				{
					if (查找背包物品(num29, 重铸所需灵气, out var 物品列表7))
					{
						金币数量 -= num28;
						消耗背包物品(num29, 物品列表7);
						v3.随机属性.SetValue(装备属性.生成属性(v3.物品类型, 重铸装备: true));
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = v3.字节描述()
						});
						属性加成[v3] = v3.装备属性;
						更新对象属性();
						对话页面 = 612606000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, "<#P1:" + v3.属性描述 + ">")
						});
					}
					else
					{
						对话页面 = 612605000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:[{num29}] 个 [{游戏物品.数据表[重铸所需灵气].物品名字}]><#P1:{num28 / 10000}>")
						});
					}
				}
				else
				{
					对话页面 = 612605000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:[{num29}] 个 [{游戏物品.数据表[重铸所需灵气].物品名字}]><#P1:{num28 / 10000}>")
					});
				}
				break;
			}
			case 612602000:
				switch (选项编号)
				{
				case 1:
					if (!角色装备.TryGetValue(9, out v2))
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						对话页面 = 612604000;
						重铸部位 = 9;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					break;
				case 2:
					if (角色装备.TryGetValue(10, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 10;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 3:
					if (!角色装备.TryGetValue(11, out v2))
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						对话页面 = 612604000;
						重铸部位 = 11;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					break;
				case 4:
					if (!角色装备.TryGetValue(12, out v2))
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						对话页面 = 612604000;
						重铸部位 = 12;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					break;
				case 5:
					if (角色装备.TryGetValue(8, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 8;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 6:
					if (角色装备.TryGetValue(14, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 14;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 7:
					if (!角色装备.TryGetValue(13, out v2))
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						对话页面 = 612604000;
						重铸部位 = 13;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					break;
				}
				break;
			case 612601000:
				switch (选项编号)
				{
				case 1:
					if (角色装备.TryGetValue(1, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 1;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 2:
					if (!角色装备.TryGetValue(3, out v2))
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						对话页面 = 612604000;
						重铸部位 = 3;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					break;
				case 3:
					if (角色装备.TryGetValue(6, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 6;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 4:
					if (角色装备.TryGetValue(7, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 7;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 5:
					if (角色装备.TryGetValue(4, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 4;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 6:
					if (角色装备.TryGetValue(5, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 5;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 7:
					if (角色装备.TryGetValue(2, out v2))
					{
						对话页面 = 612604000;
						重铸部位 = 2;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
						});
					}
					else
					{
						对话页面 = 612603000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				}
				break;
			case 619200000:
				switch (选项编号)
				{
				default:
					return;
				case 2:
					对话页面 = 619202000;
					break;
				case 1:
					对话页面 = 619201000;
					break;
				}
				网络连接?.发送封包(new 同步交互结果
				{
					对象编号 = 对话守卫.地图编号,
					交互文本 = 对话数据.字节数据(对话页面)
				});
				break;
			case 612606000:
				if (选项编号 == 1)
				{
					对话页面 = 612604000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(装备穿戴部位)重铸部位}>")
					});
				}
				break;
			case 619202500:
			{
				if (选项编号 != 1)
				{
					break;
				}
				装备数据 v5 = null;
				if (雕色部位 != 1)
				{
					if (雕色部位 != 2)
					{
						if (雕色部位 != 3)
						{
							if (雕色部位 == 4)
							{
								角色装备.TryGetValue(5, out v5);
							}
							else if (雕色部位 != 5)
							{
								if (雕色部位 != 6)
								{
									if (雕色部位 != 7)
									{
										if (雕色部位 != 8)
										{
											break;
										}
										角色装备.TryGetValue(14, out v5);
									}
									else
									{
										角色装备.TryGetValue(2, out v5);
									}
								}
								else
								{
									角色装备.TryGetValue(4, out v5);
								}
							}
							else
							{
								角色装备.TryGetValue(6, out v5);
							}
						}
						else
						{
							角色装备.TryGetValue(7, out v5);
						}
					}
					else
					{
						角色装备.TryGetValue(1, out v5);
					}
				}
				else
				{
					角色装备.TryGetValue(3, out v5);
				}
				if (v5 == null)
				{
					对话页面 = 619202100;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				}
				if (v5.孔洞颜色.Count == 0)
				{
					对话页面 = 619202400;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				}
				if (v5.镶嵌灵石.Count != 0)
				{
					对话页面 = 619202300;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				}
				int num36 = 5;
				int num37 = 100000;
				if (金币数量 >= 100000)
				{
					if (查找背包物品(num36, 91116, out var 物品列表12))
					{
						金币数量 -= num37;
						消耗背包物品(num36, 物品列表12);
						v5.孔洞颜色[主程.随机数.Next(v5.孔洞颜色.Count)] = (装备孔洞颜色)主程.随机数.Next(1, 8);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = v5.字节描述()
						});
						if (v5.孔洞颜色.Count != 1)
						{
							if (v5.孔洞颜色.Count == 2)
							{
								对话页面 = 619202600;
								网络连接?.发送封包(new 同步交互结果
								{
									交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{v5.孔洞颜色[0]}><#P1:{v5.孔洞颜色[1]}>"),
									对象编号 = 对话守卫.地图编号
								});
							}
						}
						else
						{
							对话页面 = 619202500;
							网络连接?.发送封包(new 同步交互结果
							{
								交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{v5.孔洞颜色[0]}><#P1:0>"),
								对象编号 = 对话守卫.地图编号
							});
						}
					}
					else
					{
						对话页面 = 619202200;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num36}><#P1:0><#P1:{num37 / 10000}>"),
							对象编号 = 对话守卫.地图编号
						});
					}
				}
				else
				{
					对话页面 = 619202200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num36}><#P1:0><#P1:{num37 / 10000}>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 619202000:
			{
				装备数据 v4 = null;
				switch (选项编号)
				{
				default:
					return;
				case 1:
					角色装备.TryGetValue(3, out v4);
					break;
				case 2:
					角色装备.TryGetValue(1, out v4);
					break;
				case 3:
					角色装备.TryGetValue(7, out v4);
					break;
				case 4:
					角色装备.TryGetValue(5, out v4);
					break;
				case 5:
					角色装备.TryGetValue(6, out v4);
					break;
				case 6:
					角色装备.TryGetValue(4, out v4);
					break;
				case 7:
					角色装备.TryGetValue(2, out v4);
					break;
				case 8:
					角色装备.TryGetValue(14, out v4);
					break;
				}
				if (v4 == null)
				{
					对话页面 = 619202100;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
				}
				else if (v4.孔洞颜色.Count == 0)
				{
					对话页面 = 619202400;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
				}
				else if (v4.镶嵌灵石.Count == 0)
				{
					雕色部位 = (byte)选项编号;
					if (v4.孔洞颜色.Count != 1)
					{
						if (v4.孔洞颜色.Count == 2)
						{
							对话页面 = 619202600;
							网络连接?.发送封包(new 同步交互结果
							{
								交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{v4.孔洞颜色[0]}><#P1:{v4.孔洞颜色[1]}>"),
								对象编号 = 对话守卫.地图编号
							});
						}
					}
					else
					{
						对话页面 = 619202500;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{v4.孔洞颜色[0]}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
				}
				else
				{
					对话页面 = 619202300;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
				}
				break;
			}
			case 619201000:
			{
				装备数据 v6 = null;
				switch (选项编号)
				{
				default:
					return;
				case 1:
					角色装备.TryGetValue(3, out v6);
					break;
				case 2:
					角色装备.TryGetValue(1, out v6);
					break;
				case 3:
					角色装备.TryGetValue(7, out v6);
					break;
				case 4:
					角色装备.TryGetValue(5, out v6);
					break;
				case 5:
					角色装备.TryGetValue(6, out v6);
					break;
				case 6:
					角色装备.TryGetValue(4, out v6);
					break;
				case 7:
					角色装备.TryGetValue(2, out v6);
					break;
				case 8:
					角色装备.TryGetValue(14, out v6);
					break;
				}
				if (v6 != null)
				{
					if (v6.孔洞颜色.Count >= 2)
					{
						对话页面 = 619201300;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
						break;
					}
					int num43 = ((v6.孔洞颜色.Count != 0) ? 50 : 5);
					if (查找背包物品(num43, 91115, out var 物品列表14))
					{
						消耗背包物品(num43, 物品列表14);
						v6.孔洞颜色.Add(装备孔洞颜色.黄色);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = v6.字节描述()
						});
					}
					else
					{
						对话页面 = 619201200;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num43}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
				}
				else
				{
					对话页面 = 619201100;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
				}
				break;
			}
			case 619400000:
				switch (选项编号)
				{
				case 1:
					对话页面 = 619401000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 2:
					对话页面 = 619402000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 3:
					对话页面 = 619403000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 4:
					对话页面 = 619404000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 5:
					对话页面 = 619405000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 6:
					对话页面 = 619406000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 7:
					对话页面 = 619407000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				case 8:
					对话页面 = 619408000;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
					break;
				}
				break;
			case 619202600:
			{
				if (选项编号 != 1)
				{
					break;
				}
				装备数据 v = null;
				if (雕色部位 == 1)
				{
					角色装备.TryGetValue(3, out v);
				}
				else if (雕色部位 == 2)
				{
					角色装备.TryGetValue(1, out v);
				}
				else if (雕色部位 != 3)
				{
					if (雕色部位 != 4)
					{
						if (雕色部位 == 5)
						{
							角色装备.TryGetValue(6, out v);
						}
						else if (雕色部位 != 6)
						{
							if (雕色部位 != 7)
							{
								if (雕色部位 != 8)
								{
									break;
								}
								角色装备.TryGetValue(14, out v);
							}
							else
							{
								角色装备.TryGetValue(2, out v);
							}
						}
						else
						{
							角色装备.TryGetValue(4, out v);
						}
					}
					else
					{
						角色装备.TryGetValue(5, out v);
					}
				}
				else
				{
					角色装备.TryGetValue(7, out v);
				}
				if (v == null)
				{
					对话页面 = 619202100;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
				}
				else if (v.孔洞颜色.Count != 0)
				{
					if (v.镶嵌灵石.Count != 0)
					{
						对话页面 = 619202300;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
						break;
					}
					int num9 = 5;
					int num10 = 100000;
					if (金币数量 >= 100000)
					{
						if (!查找背包物品(num9, 91116, out var 物品列表2))
						{
							对话页面 = 619202200;
							网络连接?.发送封包(new 同步交互结果
							{
								交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num9}><#P1:{num10 / 10000}>"),
								对象编号 = 对话守卫.地图编号
							});
							break;
						}
						金币数量 -= num10;
						消耗背包物品(num9, 物品列表2);
						v.孔洞颜色[主程.随机数.Next(v.孔洞颜色.Count)] = (装备孔洞颜色)主程.随机数.Next(1, 8);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = v.字节描述()
						});
						if (v.孔洞颜色.Count != 1)
						{
							if (v.孔洞颜色.Count == 2)
							{
								对话页面 = 619202600;
								网络连接?.发送封包(new 同步交互结果
								{
									交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{v.孔洞颜色[0]}><#P1:{v.孔洞颜色[1]}>"),
									对象编号 = 对话守卫.地图编号
								});
							}
						}
						else
						{
							对话页面 = 619202500;
							网络连接?.发送封包(new 同步交互结果
							{
								交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{v.孔洞颜色[0]}><#P1:0>"),
								对象编号 = 对话守卫.地图编号
							});
						}
					}
					else
					{
						对话页面 = 619202200;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num9}><#P1:{num10 / 10000}>"),
							对象编号 = 对话守卫.地图编号
						});
					}
				}
				else
				{
					对话页面 = 619202400;
					网络连接?.发送封包(new 同步交互结果
					{
						对象编号 = 对话守卫.地图编号,
						交互文本 = 对话数据.字节数据(对话页面)
					});
				}
				break;
			}
			case 619403000:
			{
				int num18 = 10;
				int num19;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num19 = 10111;
					break;
				case 1:
					num19 = 10110;
					break;
				}
				if (背包剩余 <= 0)
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
					break;
				}
				if (查找背包物品(num18, num19, out var 物品列表5))
				{
					byte b6 = 0;
					while (b6 < 背包大小)
					{
						if (角色背包.ContainsKey(b6))
						{
							b6 = (byte)(b6 + 1);
							continue;
						}
						消耗背包物品(num18, 物品列表5);
						角色背包[b6] = new 物品数据(游戏物品.数据表[num19 + 1], 角色数据, 1, b6, 1);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b6].字节描述()
						});
						网络连接?.发送封包(new 成功合成灵石
						{
							灵石状态 = 1
						});
						return;
					}
				}
				对话页面 = 619400100;
				网络连接?.发送封包(new 同步交互结果
				{
					交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num18}><#P1:0>"),
					对象编号 = 对话守卫.地图编号
				});
				break;
			}
			case 619402000:
			{
				int num7 = 10;
				int num8;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num8 = 10321;
					break;
				case 1:
					num8 = 10320;
					break;
				}
				if (背包剩余 <= 0)
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
					break;
				}
				if (查找背包物品(num7, num8, out var 物品列表))
				{
					byte b = 0;
					while (b < 背包大小)
					{
						if (角色背包.ContainsKey(b))
						{
							b = (byte)(b + 1);
							continue;
						}
						消耗背包物品(num7, 物品列表);
						角色背包[b] = new 物品数据(游戏物品.数据表[num8 + 1], 角色数据, 1, b, 1);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b].字节描述()
						});
						网络连接?.发送封包(new 成功合成灵石
						{
							灵石状态 = 1
						});
						return;
					}
				}
				对话页面 = 619400100;
				网络连接?.发送封包(new 同步交互结果
				{
					交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num7}><#P1:0>"),
					对象编号 = 对话守卫.地图编号
				});
				break;
			}
			case 619401000:
			{
				int num32 = 10;
				int num33;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num33 = 10421;
					break;
				case 1:
					num33 = 10420;
					break;
				}
				if (背包剩余 <= 0)
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
					break;
				}
				if (查找背包物品(num32, num33, out var 物品列表9))
				{
					byte b9 = 0;
					while (b9 < 背包大小)
					{
						if (角色背包.ContainsKey(b9))
						{
							b9 = (byte)(b9 + 1);
							continue;
						}
						消耗背包物品(num32, 物品列表9);
						角色背包[b9] = new 物品数据(游戏物品.数据表[num33 + 1], 角色数据, 1, b9, 1);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b9].字节描述()
						});
						网络连接?.发送封包(new 成功合成灵石
						{
							灵石状态 = 1
						});
						return;
					}
				}
				对话页面 = 619400100;
				网络连接?.发送封包(new 同步交互结果
				{
					交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num32}><#P1:0>"),
					对象编号 = 对话守卫.地图编号
				});
				break;
			}
			case 619405000:
			{
				int num11 = 10;
				int num12;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num12 = 10721;
					break;
				case 1:
					num12 = 10720;
					break;
				}
				if (背包剩余 > 0)
				{
					if (查找背包物品(num11, num12, out var 物品列表3))
					{
						byte b4 = 0;
						while (b4 < 背包大小)
						{
							if (角色背包.ContainsKey(b4))
							{
								b4 = (byte)(b4 + 1);
								continue;
							}
							消耗背包物品(num11, 物品列表3);
							角色背包[b4] = new 物品数据(游戏物品.数据表[num12 + 1], 角色数据, 1, b4, 1);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b4].字节描述()
							});
							网络连接?.发送封包(new 成功合成灵石
							{
								灵石状态 = 1
							});
							return;
						}
					}
					对话页面 = 619400100;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num11}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 619404000:
			{
				int num41 = 10;
				int num42;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num42 = 10621;
					break;
				case 1:
					num42 = 10620;
					break;
				}
				if (背包剩余 > 0)
				{
					if (查找背包物品(num41, num42, out var 物品列表13))
					{
						byte b12 = 0;
						while (b12 < 背包大小)
						{
							if (角色背包.ContainsKey(b12))
							{
								b12 = (byte)(b12 + 1);
								continue;
							}
							消耗背包物品(num41, 物品列表13);
							角色背包[b12] = new 物品数据(游戏物品.数据表[num42 + 1], 角色数据, 1, b12, 1);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b12].字节描述()
							});
							网络连接?.发送封包(new 成功合成灵石
							{
								灵石状态 = 1
							});
							return;
						}
					}
					对话页面 = 619400100;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num41}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 619408000:
			{
				int num20 = 10;
				int num21;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num21 = 10521;
					break;
				case 1:
					num21 = 10520;
					break;
				}
				if (背包剩余 > 0)
				{
					if (查找背包物品(num20, num21, out var 物品列表6))
					{
						byte b7 = 0;
						while (b7 < 背包大小)
						{
							if (角色背包.ContainsKey(b7))
							{
								b7 = (byte)(b7 + 1);
								continue;
							}
							消耗背包物品(num20, 物品列表6);
							角色背包[b7] = new 物品数据(游戏物品.数据表[num21 + 1], 角色数据, 1, b7, 1);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b7].字节描述()
							});
							网络连接?.发送封包(new 成功合成灵石
							{
								灵石状态 = 1
							});
							return;
						}
					}
					对话页面 = 619400100;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num20}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 619407000:
			{
				int num16 = 10;
				int num17;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num17 = 10221;
					break;
				case 1:
					num17 = 10220;
					break;
				}
				if (背包剩余 <= 0)
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
					break;
				}
				if (查找背包物品(num16, num17, out var 物品列表4))
				{
					byte b5 = 0;
					while (b5 < 背包大小)
					{
						if (角色背包.ContainsKey(b5))
						{
							b5 = (byte)(b5 + 1);
							continue;
						}
						消耗背包物品(num16, 物品列表4);
						角色背包[b5] = new 物品数据(游戏物品.数据表[num17 + 1], 角色数据, 1, b5, 1);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b5].字节描述()
						});
						网络连接?.发送封包(new 成功合成灵石
						{
							灵石状态 = 1
						});
						return;
					}
				}
				对话页面 = 619400100;
				网络连接?.发送封包(new 同步交互结果
				{
					交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num16}><#P1:0>"),
					对象编号 = 对话守卫.地图编号
				});
				break;
			}
			case 619406000:
			{
				int num30 = 10;
				int num31;
				switch (选项编号)
				{
				default:
					return;
				case 2:
					num31 = 10121;
					break;
				case 1:
					num31 = 10120;
					break;
				}
				if (背包剩余 > 0)
				{
					if (查找背包物品(num30, num31, out var 物品列表8))
					{
						byte b8 = 0;
						while (b8 < 背包大小)
						{
							if (角色背包.ContainsKey(b8))
							{
								b8 = (byte)(b8 + 1);
								continue;
							}
							消耗背包物品(num30, 物品列表8);
							角色背包[b8] = new 物品数据(游戏物品.数据表[num31 + 1], 角色数据, 1, b8, 1);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b8].字节描述()
							});
							网络连接?.发送封包(new 成功合成灵石
							{
								灵石状态 = 1
							});
							return;
						}
					}
					对话页面 = 619400100;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num30}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else
				{
					对话页面 = 619400200;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 625200000:
			{
				if (选项编号 != 1)
				{
					break;
				}
				if (!查找背包物品(1, 91127, out var 物品列表11))
				{
					对话页面 = 625201000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
					break;
				}
				消耗背包物品(1, 物品列表11);
				if (角色数据.屠魔兑换.V.Date != 主程.当前时间.Date)
				{
					角色数据.屠魔兑换.V = 主程.当前时间;
					角色数据.屠魔次数.V = 0;
				}
				玩家增加经验(null, (int)Math.Max(100000.0, 1000000.0 * Math.Pow(0.699999988079071, 角色数据.屠魔次数.V)));
				角色数据.屠魔次数.V++;
				break;
			}
			case 624200000:
			{
				if (选项编号 != 1)
				{
					break;
				}
				int 扣除金币 = 100000;
				int 需要等级 = 25;
				地图对象 value3;
				if (所属队伍 == null)
				{
					对话页面 = 624201000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				else if (角色数据 != 所属队伍.队长数据)
				{
					对话页面 = 624202000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				else if (所属队伍.队伍成员.Count < 4)
				{
					对话页面 = 624207000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				else if (所属队伍.队伍成员.FirstOrDefault((角色数据 O) => O.网络连接 == null || !地图处理网关.地图对象表.TryGetValue(O.角色编号, out value3) || !对话守卫.邻居列表.Contains(value3)) == null)
				{
					if (所属队伍.队伍成员.FirstOrDefault((角色数据 O) => O.金币数量 < 扣除金币) != null)
					{
						对话页面 = 624204000;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{扣除金币 / 10000}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					else if (所属队伍.队伍成员.FirstOrDefault((角色数据 O) => O.屠魔大厅.V.Date == 主程.当前时间.Date) != null)
					{
						对话页面 = 624205000;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.字节数据(对话页面),
							对象编号 = 对话守卫.地图编号
						});
					}
					else if (所属队伍.队伍成员.FirstOrDefault((角色数据 O) => O.当前等级.V < 需要等级) != null)
					{
						对话页面 = 624206000;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{需要等级}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					else if (所属队伍.队伍成员.FirstOrDefault((角色数据 O) => 地图处理网关.激活对象表[O.角色编号].对象死亡) != null)
					{
						对话页面 = 624208000;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.字节数据(对话页面),
							对象编号 = 对话守卫.地图编号
						});
					}
					else
					{
						if (!地图处理网关.地图实例表.TryGetValue(1281, out var value5))
						{
							break;
						}
						地图实例 地图实例2 = new 地图实例(游戏地图.数据表[80])
						{
							地形数据 = value5.地形数据,
							地图区域 = value5.地图区域,
							怪物区域 = value5.怪物区域,
							守卫区域 = value5.守卫区域,
							传送区域 = value5.传送区域,
							节点计时 = 主程.当前时间.AddSeconds(20.0),
							怪物波数 = value5.怪物区域.OrderBy((怪物刷新 O) => O.所处坐标.X).ToList(),
							地图对象 = new HashSet<地图对象>[value5.地图大小.X, value5.地图大小.Y]
						};
						地图处理网关.副本实例表.Add(地图实例2);
						地图实例2.副本守卫 = new 守卫实例(地图守卫.数据表[6724], 地图实例2, 游戏方向.左下, new Point(1005, 273));
						{
							foreach (角色数据 item in 所属队伍.队伍成员)
							{
								玩家实例 obj = 地图处理网关.激活对象表[item.角色编号] as 玩家实例;
								obj.当前交易?.结束交易();
								obj.金币数量 -= 扣除金币;
								obj.玩家切换地图(地图实例2, 地图区域类型.传送区域);
							}
							break;
						}
					}
				}
				else
				{
					对话页面 = 624203000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 635800000:
				if (选项编号 == 1)
				{
					玩家切换地图((当前地图.地图编号 == 147) ? 当前地图 : 地图处理网关.分配地图(147), 地图区域类型.复活区域);
				}
				break;
			case 635000000:
			{
				int num22 = 40;
				int num23 = 100000;
				int num24 = 87;
				if (选项编号 == 1)
				{
					if (当前等级 < num22)
					{
						对话页面 = 711900001;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num22}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					else if (金币数量 >= num23)
					{
						金币数量 -= num23;
						玩家切换地图((当前地图.地图编号 == num24) ? 当前地图 : 地图处理网关.分配地图(num24), 地图区域类型.传送区域);
					}
					else
					{
						对话页面 = 711900002;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num23}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
				}
				break;
			}
			case 627400000:
				if (选项编号 != 1)
				{
					break;
				}
				if (所属行会 != null && 系统数据.数据.占领行会.V == 所属行会)
				{
					if (!(角色数据.攻沙日期.V != 系统数据.数据.占领时间.V))
					{
						if (!(角色数据.领奖日期.V.Date == 主程.当前时间.Date))
						{
							byte b2 = byte.MaxValue;
							byte b3 = 0;
							while (b3 < 背包大小)
							{
								if (角色背包.ContainsKey(b3))
								{
									b3 = (byte)(b3 + 1);
									continue;
								}
								b2 = b3;
								if (b2 != byte.MaxValue)
								{
									if (!游戏物品.检索表.TryGetValue("沙城每日宝箱", out var value4))
									{
										网络连接?.发送封包(new 游戏错误提示
										{
											错误代码 = 1802
										});
										return;
									}
									角色数据.领奖日期.V = 主程.当前时间;
									角色背包[b2] = new 物品数据(value4, 角色数据, 1, b2, 1);
									网络连接?.发送封包(new 玩家物品变动
									{
										物品描述 = 角色背包[b2].字节描述()
									});
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 6459
									});
								}
								return;
							}
						}
						对话页面 = 627402000;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.字节数据(对话页面),
							对象编号 = 对话守卫.地图编号
						});
					}
					else
					{
						对话页面 = 627403000;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.字节数据(对话页面),
							对象编号 = 对话守卫.地图编号
						});
					}
				}
				else
				{
					对话页面 = 627401000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			case 636100000:
				if (选项编号 == 1)
				{
					玩家切换地图((当前地图.地图编号 == 87) ? 当前地图 : 地图处理网关.分配地图(87), 地图区域类型.传送区域);
				}
				break;
			case 635900000:
				if (选项编号 == 1)
				{
					玩家切换地图((当前地图.地图编号 == 88) ? 当前地图 : 地图处理网关.分配地图(88), 地图区域类型.传送区域);
				}
				break;
			case 674000000:
				switch (选项编号)
				{
				case 2:
					网络连接?.发送封包(new 查看攻城名单
					{
						字节描述 = 系统数据.数据.沙城申请描述()
					});
					break;
				case 1:
					对话页面 = 674001000;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.字节数据(对话页面),
						对象编号 = 对话守卫.地图编号
					});
					break;
				}
				break;
			case 670508000:
				if (角色数据.升级装备.V == null)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 继续Npcc对话.  错误: 尝试取回武器."));
					break;
				}
				switch (选项编号)
				{
				case 3:
					放弃升级武器();
					break;
				case 1:
					if (背包剩余 > 0)
					{
						int num34 = (角色数据.升级装备.V.升级次数.V + 1) * 100 * 10000;
						if (金币数量 >= num34)
						{
							int num35 = (角色数据.升级装备.V.升级次数.V + 1) * 10;
							if (查找背包物品(num35, 110012, out var 物品列表10))
							{
								byte b10 = byte.MaxValue;
								byte b11 = 0;
								while (b11 < 背包大小)
								{
									if (角色背包.ContainsKey(b11))
									{
										b11 = (byte)(b11 + 1);
										continue;
									}
									b10 = b11;
									if (b10 == byte.MaxValue)
									{
										对话页面 = 670505000;
										网络连接?.发送封包(new 同步交互结果
										{
											对象编号 = 对话守卫.地图编号,
											交互文本 = 对话数据.字节数据(对话页面)
										});
										return;
									}
									金币数量 -= num34;
									消耗背包物品(num35, 物品列表10);
									角色背包[b10] = 角色数据.升级装备.V;
									角色数据.升级装备.V = null;
									角色背包[b10].物品容器.V = 1;
									角色背包[b10].物品位置.V = b10;
									网络连接?.发送封包(new 玩家物品变动
									{
										物品描述 = 角色背包[b10].字节描述()
									});
									网络连接?.发送封包(new 武器升级结果
									{
										升级结果 = 2
									});
									网络连接?.发送封包(new 武器升级结果
									{
										升级结果 = 2
									});
									return;
								}
							}
							对话页面 = 670509000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.字节数据(对话页面)
							});
						}
						else
						{
							对话页面 = 670510000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.字节数据(对话页面)
							});
						}
					}
					else
					{
						对话页面 = 670505000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				}
				break;
			case 670500000:
				switch (选项编号)
				{
				case 1:
					if (角色数据.升级装备.V != null)
					{
						对话页面 = 670501000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else
					{
						打开界面 = "UpgradeCurEquippedWepn";
						对话页面 = 670502000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				case 2:
					if (角色数据.升级装备.V == null)
					{
						对话页面 = 670503000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else if (!(主程.当前时间 < 角色数据.取回时间.V))
					{
						if (背包剩余 <= 0)
						{
							对话页面 = 670505000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.字节数据(对话页面)
							});
						}
						else if (!玩家取回装备(0))
						{
							对话页面 = 670508000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{角色数据.升级装备.V.升级次数.V * 10 + 10}><#P1:{角色数据.升级装备.V.升级次数.V * 100 + 100}>")
							});
						}
						else
						{
							对话页面 = 670507000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.字节数据(对话页面)
							});
						}
					}
					else
					{
						对话页面 = 670504000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{(int)(角色数据.取回时间.V - 主程.当前时间).TotalMinutes + 1}><#P1:0>")
						});
					}
					break;
				case 3:
					if (角色数据.升级装备.V == null)
					{
						对话页面 = 670503000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					else if (金币数量 >= 100000)
					{
						if (背包剩余 <= 0)
						{
							对话页面 = 670505000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.字节数据(对话页面)
							});
						}
						else if (!玩家取回装备(100000))
						{
							角色数据.取回时间.V = 主程.当前时间;
							对话页面 = 670508000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{角色数据.升级装备.V.升级次数.V * 10 + 10}><#P1:{角色数据.升级装备.V.升级次数.V * 100 + 100}>")
							});
						}
						else
						{
							对话页面 = 670507000;
							网络连接?.发送封包(new 同步交互结果
							{
								对象编号 = 对话守卫.地图编号,
								交互文本 = 对话数据.字节数据(对话页面)
							});
						}
					}
					else
					{
						对话页面 = 670506000;
						网络连接?.发送封包(new 同步交互结果
						{
							对象编号 = 对话守卫.地图编号,
							交互文本 = 对话数据.字节数据(对话页面)
						});
					}
					break;
				}
				break;
			case 711900000:
				switch (选项编号)
				{
				default:
					return;
				case 1:
					对话页面 = 711901000;
					break;
				case 2:
					对话页面 = 711902000;
					break;
				case 3:
					对话页面 = 711903000;
					break;
				}
				网络连接?.发送封包(new 同步交互结果
				{
					对象编号 = 对话守卫.地图编号,
					交互文本 = 对话数据.字节数据(对话页面)
				});
				break;
			case 674001000:
				if (选项编号 != 1)
				{
					break;
				}
				if (所属行会 == null)
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6668
					});
				}
				else if (角色数据 != 所属行会.行会会长.V)
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 8961
					});
				}
				else if (所属行会 != 系统数据.数据.占领行会.V)
				{
					物品数据 物品;
					if (系统数据.数据.申请行会.Values.FirstOrDefault((行会数据 O) => O == 所属行会) != null)
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 8964
						});
					}
					else if (金币数量 < 1000000)
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 8962
						});
					}
					else if (查找背包物品(90196, out 物品))
					{
						金币数量 -= 1000000;
						消耗背包物品(1, 物品);
						系统数据.数据.申请行会.Add(主程.当前时间.Date.AddDays(0.0).AddHours(0.0), 所属行会);
						网络服务网关.发送公告($"[{所属行会}]行会已经报名参加次日的沙巴克争夺战", 滚动播报: true);
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 8963
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 8965
					});
				}
				break;
			case 711903000:
			{
				int num38;
				int num39;
				int num40;
				switch (选项编号)
				{
				default:
					return;
				case 1:
					num38 = 15;
					num39 = 2000;
					num40 = 144;
					break;
				case 2:
					num38 = 20;
					num39 = 3000;
					num40 = 148;
					break;
				case 3:
					num38 = 25;
					num39 = 3000;
					num40 = 178;
					break;
				case 4:
					num38 = 25;
					num39 = 3000;
					num40 = 146;
					break;
				case 5:
					num38 = 30;
					num39 = 5000;
					num40 = 175;
					break;
				case 6:
					num38 = 45;
					num39 = 5000;
					num40 = 59;
					break;
				}
				if (当前等级 >= num38)
				{
					if (金币数量 < num39)
					{
						对话页面 = 711900002;
						网络连接?.发送封包(new 同步交互结果
						{
							交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num39}><#P1:0>"),
							对象编号 = 对话守卫.地图编号
						});
					}
					else
					{
						金币数量 -= num39;
						玩家切换地图((当前地图.地图编号 == num40) ? 当前地图 : 地图处理网关.分配地图(num40), 地图区域类型.传送区域);
					}
				}
				else
				{
					对话页面 = 711900001;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num38}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 711902000:
			{
				int num25;
				int num26;
				int num27;
				switch (选项编号)
				{
				default:
					return;
				case 1:
					num25 = 1;
					num26 = 2500;
					num27 = 145;
					break;
				case 2:
					num25 = 40;
					num26 = 6500;
					num27 = 187;
					break;
				case 3:
					num25 = 40;
					num26 = 9500;
					num27 = 191;
					break;
				}
				if (当前等级 >= num25)
				{
					if (金币数量 >= num26)
					{
						金币数量 -= num26;
						玩家切换地图((当前地图.地图编号 == num27) ? 当前地图 : 地图处理网关.分配地图(num27), 地图区域类型.传送区域);
						break;
					}
					对话页面 = 711900002;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num26}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else
				{
					对话页面 = 711900001;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num25}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			case 711901000:
			{
				int num;
				int num2;
				int num3;
				switch (选项编号)
				{
				default:
					return;
				case 1:
					num = 1;
					num2 = 1000;
					num3 = 142;
					break;
				case 2:
					num = 8;
					num2 = 1500;
					num3 = 143;
					break;
				case 3:
					num = 14;
					num2 = 2000;
					num3 = 147;
					break;
				case 4:
					num = 30;
					num2 = 3000;
					num3 = 152;
					break;
				case 5:
					num = 40;
					num2 = 5000;
					num3 = 102;
					break;
				case 6:
					num = 45;
					num2 = 8000;
					num3 = 50;
					break;
				case 7:
					num = 40;
					num2 = 10000;
					num3 = 231;
					break;
				}
				if (当前等级 < num)
				{
					对话页面 = 711900001;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				else if (金币数量 >= num2)
				{
					金币数量 -= num2;
					if (num3 == 152)
					{
						if (所属行会 != null && 所属行会 == 系统数据.数据.占领行会.V)
						{
							玩家切换地图((当前地图.地图编号 == num3) ? 当前地图 : 地图处理网关.分配地图(num3), 地图区域类型.传送区域);
						}
						else
						{
							玩家切换地图((当前地图.地图编号 == num3) ? 当前地图 : 地图处理网关.分配地图(num3), 地图区域类型.复活区域);
						}
					}
					else
					{
						玩家切换地图((当前地图.地图编号 == num3) ? 当前地图 : 地图处理网关.分配地图(num3), 地图区域类型.复活区域);
					}
				}
				else
				{
					对话页面 = 711900002;
					网络连接?.发送封包(new 同步交互结果
					{
						交互文本 = 对话数据.合并数据(对话页面, $"<#P0:{num2}><#P1:0>"),
						对象编号 = 对话守卫.地图编号
					});
				}
				break;
			}
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 开始Npcc对话. 错误: 超长距离对话."));
		}
	}

	public void 玩家更改设置(byte[] 设置)
	{
		using MemoryStream input = new MemoryStream(设置);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = 设置.Length / 5;
		for (int i = 0; i < num; i++)
		{
			byte 索引 = binaryReader.ReadByte();
			uint value = binaryReader.ReadUInt32();
			角色数据.玩家设置[索引] = value;
		}
	}

	public void 查询地图路线()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)当前地图.分线数量);
		binaryWriter.Write(当前地图.地图编号);
		for (int i = 1; i <= 当前地图.分线数量; i++)
		{
			binaryWriter.Write(16777216 + i);
			binaryWriter.Write(地图处理网关.地图实例表[当前地图.地图编号 * 16 + i].地图状态);
		}
		网络连接?.发送封包(new 查询线路信息
		{
			字节数据 = memoryStream.ToArray()
		});
	}

	public void 切换地图路线()
	{
	}

	public void 玩家同步位置()
	{
	}

	private void 扩展背包(byte 位置)
	{
		if (背包大小 + 位置 > 64)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 玩家扩展背包.  错误: 背包超出限制."));
			return;
		}
		int num = 计算类.扩展背包(背包大小 - 32);
		int num2 = 计算类.扩展背包(背包大小 + 位置 - 32) - num;
		if (金币数量 < num2)
		{
			客户网络 客户网络 = 网络连接;
			客户网络.发送封包(new 游戏错误提示
			{
				错误代码 = 1821
			});
			return;
		}
		金币数量 -= num2;
		背包大小 += 位置;
		客户网络 客户网络2 = 网络连接;
		客户网络2.发送封包(new 背包容量改变
		{
			背包类型 = 1,
			背包容量 = 背包大小
		});
	}

	private void 扩展仓库(byte 位置)
	{
		if (仓库大小 + 位置 > 144)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 玩家扩展仓库.  错误: 仓库超出限制."));
			return;
		}
		int num = 计算类.扩展仓库(仓库大小 - 16);
		int num2 = 计算类.扩展仓库(仓库大小 + 位置 - 16) - num;
		if (金币数量 < num2)
		{
			客户网络 客户网络 = 网络连接;
			客户网络.发送封包(new 游戏错误提示
			{
				错误代码 = 1821
			});
			return;
		}
		金币数量 -= num2;
		仓库大小 += 位置;
		客户网络 客户网络2 = 网络连接;
		客户网络2.发送封包(new 背包容量改变
		{
			背包类型 = 2,
			背包容量 = 仓库大小
		});
	}

	private void 扩展资源背包(byte 位置)
	{
		if (仓库大小 + 位置 > 216)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 玩家拓展资源背包.  错误: 背包超出限制."));
			return;
		}
		int num = 位置 - 资源背包大小;
		if (金币数量 < num)
		{
			客户网络 客户网络 = 网络连接;
			客户网络.发送封包(new 游戏错误提示
			{
				错误代码 = 1821
			});
			return;
		}
		金币数量 -= num;
		资源背包大小 += 位置;
		客户网络 客户网络2 = 网络连接;
		客户网络2.发送封包(new 背包容量改变
		{
			背包类型 = 7,
			背包容量 = 资源背包大小
		});
	}

	public void 玩家扩展背包(byte 背包类型, byte 位置)
	{
		if (位置 == 0)
		{
			网络连接?.尝试断开连接(new Exception("错误操作: 玩家扩展背包.  错误: 错误的背包类型."));
			return;
		}
		switch (背包类型)
		{
		case 7:
			扩展资源背包(位置);
			break;
		case 2:
			扩展仓库(位置);
			break;
		case 1:
			扩展背包(位置);
			break;
		}
	}

	public void 商店特修单件(byte 背包类型, byte 装备位置)
	{
		网络连接.尝试断开连接(new Exception("错误操作: 特修单件装备.  错误: 功能已经屏蔽."));
	}

	public void 商店修理单件(byte 背包类型, byte 装备位置)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (对话守卫 != null)
		{
			if (打开商店 == 0)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 没有打开商店."));
				return;
			}
			if (当前地图 != 对话守卫.当前地图 || 网格距离(对话守卫) > 12)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 人物距离太远."));
				return;
			}
			switch (背包类型)
			{
			case 1:
			{
				if (!角色背包.TryGetValue(装备位置, out var v2))
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1802
					});
				}
				else if (v2 is 装备数据 装备数据)
				{
					if (装备数据.能否修理)
					{
						if (金币数量 < 装备数据.修理费用)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1821
							});
							break;
						}
						金币数量 -= 装备数据.修理费用;
						装备数据.最大持久.V = Math.Max(1000, 装备数据.最大持久.V - 334);
						装备数据.当前持久.V = 装备数据.最大持久.V;
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 装备数据.字节描述()
						});
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1814
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1814
					});
				}
				break;
			}
			case 0:
			{
				if (角色装备.TryGetValue(装备位置, out var v))
				{
					if (v.能否修理)
					{
						if (金币数量 >= v.修理费用)
						{
							金币数量 -= v.修理费用;
							v.最大持久.V = Math.Max(1000, v.最大持久.V - (int)((float)(v.最大持久.V - v.当前持久.V) * 0.035f));
							if (v.当前持久.V <= 0)
							{
								属性加成[v] = v.装备属性;
								更新对象属性();
							}
							v.当前持久.V = v.最大持久.V;
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = v.字节描述()
							});
							网络连接?.发送封包(new 修理物品应答());
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1821
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1814
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1802
					});
				}
				break;
			}
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 没有选中Npc."));
		}
	}

	public void 商店修理全部()
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (对话守卫 != null)
		{
			if (打开商店 != 0)
			{
				if (当前地图 == 对话守卫.当前地图 && 网格距离(对话守卫) <= 12)
				{
					if (金币数量 >= 角色装备.Values.Sum((装备数据 O) => O.能否修理 ? O.修理费用 : 0))
					{
						foreach (装备数据 value in 角色装备.Values)
						{
							if (value.能否修理)
							{
								金币数量 -= value.修理费用;
								value.最大持久.V = Math.Max(1000, value.最大持久.V - (int)((float)(value.最大持久.V - value.当前持久.V) * 0.035f));
								if (value.当前持久.V <= 0)
								{
									属性加成[value] = value.装备属性;
									更新对象属性();
								}
								value.当前持久.V = value.最大持久.V;
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = value.字节描述()
								});
							}
						}
						网络连接?.发送封包(new 修理物品应答());
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1821
						});
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 人物距离太远."));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 没有打开商店."));
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 没有选中Npc."));
		}
	}

	public void 随身修理单件(byte 背包类型, byte 装备位置, int 物品编号)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (物品编号 != 0)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 商店修理单件.  错误: 禁止使用物品."));
			return;
		}
		switch (背包类型)
		{
		case 1:
		{
			if (!角色背包.TryGetValue(装备位置, out var v2))
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1802
				});
			}
			else if (v2 is 装备数据 装备数据)
			{
				if (!装备数据.能否修理)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1814
					});
					break;
				}
				if (金币数量 < 装备数据.特修费用)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1821
					});
					break;
				}
				金币数量 -= 装备数据.特修费用;
				if (装备数据.当前持久.V <= 0)
				{
					属性加成[装备数据] = 装备数据.装备属性;
					更新对象属性();
				}
				装备数据.当前持久.V = 装备数据.最大持久.V;
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = 装备数据.字节描述()
				});
				网络连接?.发送封包(new 修理物品应答());
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1814
				});
			}
			break;
		}
		case 0:
		{
			if (角色装备.TryGetValue(装备位置, out var v))
			{
				if (v.能否修理)
				{
					if (金币数量 >= v.特修费用)
					{
						金币数量 -= v.特修费用;
						v.当前持久.V = v.最大持久.V;
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = v.字节描述()
						});
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1821
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1814
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1802
				});
			}
			break;
		}
		}
	}

	public void 随身修理全部()
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (金币数量 < 角色装备.Values.Sum((装备数据 O) => O.能否修理 ? O.特修费用 : 0))
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1821
			});
			return;
		}
		foreach (装备数据 value in 角色装备.Values)
		{
			if (value.能否修理)
			{
				金币数量 -= value.特修费用;
				if (value.当前持久.V <= 0)
				{
					属性加成[value] = value.装备属性;
					更新对象属性();
				}
				value.当前持久.V = value.最大持久.V;
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = value.字节描述()
				});
			}
		}
		网络连接?.发送封包(new 修理物品应答());
	}

	public void 请求商店数据(int 数据版本)
	{
		if (数据版本 == 0 || 数据版本 != 游戏商店.商店文件效验)
		{
			网络连接?.发送封包(new 同步商店数据
			{
				版本编号 = 游戏商店.商店文件效验,
				商品数量 = 游戏商店.商店物品数量,
				文件内容 = 游戏商店.商店文件数据
			});
		}
		else
		{
			网络连接?.发送封包(new 同步商店数据
			{
				版本编号 = 游戏商店.商店文件效验,
				商品数量 = 0,
				文件内容 = new byte[0]
			});
		}
	}

	public void 查询珍宝商店(int 数据版本)
	{
		if (数据版本 != 0 && 数据版本 == 珍宝商品.珍宝商店效验)
		{
			网络连接?.发送封包(new 同步珍宝数据
			{
				版本编号 = 珍宝商品.珍宝商店效验,
				商品数量 = 0,
				商店数据 = new byte[0]
			});
		}
		else
		{
			网络连接?.发送封包(new 同步珍宝数据
			{
				版本编号 = 珍宝商品.珍宝商店效验,
				商品数量 = 珍宝商品.珍宝商店数量,
				商店数据 = 珍宝商品.珍宝商店数据
			});
		}
	}

	public void 查询出售信息()
	{
	}

	public void 购买珍宝商品(int 物品编号, int 购入数量)
	{
		if (!珍宝商品.数据表.TryGetValue(物品编号, out var value) || !游戏物品.数据表.TryGetValue(物品编号, out var value2))
		{
			return;
		}
		if (购入数量 <= 0)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 8451
			});
			return;
		}
		int num = ((购入数量 == 1 || value2.持久类型 != 物品持久分类.堆叠) ? 1 : Math.Min(购入数量, value2.物品持久));
		int num2 = value.商品现价 * num;
		int num3 = -1;
		byte b = 0;
		while (b < 背包大小)
		{
			if (角色背包.TryGetValue(b, out var v) && (value2.持久类型 != 物品持久分类.堆叠 || value2.物品编号 != v.物品编号 || v.当前持久.V + 购入数量 > value2.物品持久))
			{
				b = (byte)(b + 1);
				continue;
			}
			num3 = b;
			break;
		}
		if (num3 != -1)
		{
			if (元宝数量 < num2)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 8451
				});
				return;
			}
			元宝数量 -= num2;
			if (物品编号 <= 1501000 || 物品编号 >= 1501005)
			{
				角色数据.消耗元宝.V += num2;
			}
			if (!角色背包.TryGetValue((byte)num3, out var v2))
			{
				if (value2 is 游戏装备 模板)
				{
					角色背包[(byte)num3] = new 装备数据(模板, 角色数据, 1, (byte)num3);
				}
				else
				{
					int 持久 = 0;
					switch (value2.持久类型)
					{
					case 物品持久分类.堆叠:
						持久 = num;
						break;
					case 物品持久分类.容器:
						持久 = 0;
						break;
					case 物品持久分类.消耗:
					case 物品持久分类.纯度:
						持久 = value2.物品持久;
						break;
					}
					角色背包[(byte)num3] = new 物品数据(value2, 角色数据, 1, (byte)num3, 持久);
				}
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = 角色背包[(byte)num3].字节描述()
				});
			}
			else
			{
				v2.当前持久.V += num;
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = v2.字节描述()
				});
			}
			主程.添加系统日志($"[{对象名字}][{当前等级}级] 购买了 [{value2.物品名字}] * {num}, 消耗元宝[{num2}]");
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1793
			});
		}
	}

	public void 购买每周特惠(int 礼包编号)
	{
		switch (礼包编号)
		{
		default:
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 8467
			});
			break;
		case 2:
			if (元宝数量 >= 3000)
			{
				if (!计算类.日期同周(角色数据.战备日期.V, 主程.当前时间))
				{
					if (!游戏物品.检索表.TryGetValue("强化战具礼盒", out var value2) || !游戏物品.检索表.TryGetValue("命运之证", out var value3))
					{
						break;
					}
					if (!(角色数据.战备日期.V == default(DateTime)))
					{
						byte b2 = byte.MaxValue;
						byte b3 = 0;
						while (b3 < 背包大小)
						{
							if (角色背包.ContainsKey(b3))
							{
								b3 = (byte)(b3 + 1);
								continue;
							}
							b2 = b3;
							break;
						}
						if (b2 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1793
							});
							break;
						}
						元宝数量 -= 3000;
						金币数量 += 875000;
						双倍经验 += 2750000;
						角色数据.消耗元宝.V += 3000L;
						角色背包[b2] = new 物品数据(value2, 角色数据, 1, b2, 1);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b2].字节描述()
						});
						角色数据.战备日期.V = 主程.当前时间;
						网络连接?.发送封包(new 同步补充变量
						{
							变量类型 = 1,
							对象编号 = 地图编号,
							变量索引 = 975,
							变量内容 = 计算类.时间转换(主程.当前时间)
						});
						主程.添加系统日志($"[{对象名字}][{当前等级}级] 购买了 [每周战备礼包], 消耗元宝[3000]");
						break;
					}
					byte b4 = byte.MaxValue;
					byte b5 = byte.MaxValue;
					for (byte b6 = 0; b6 < 背包大小; b6 = (byte)(b6 + 1))
					{
						if (!角色背包.ContainsKey(b6))
						{
							if (b4 != byte.MaxValue)
							{
								b5 = b6;
							}
							else
							{
								b4 = b6;
							}
							if (b5 != byte.MaxValue)
							{
								break;
							}
						}
					}
					if (b5 == byte.MaxValue)
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1793
						});
						break;
					}
					元宝数量 -= 3000;
					金币数量 += 875000;
					双倍经验 += 2750000;
					角色数据.消耗元宝.V += 3000L;
					角色背包[b4] = new 物品数据(value2, 角色数据, 1, b4, 1);
					网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = 角色背包[b4].字节描述()
					});
					角色背包[b5] = new 物品数据(value3, 角色数据, 1, b5, 1);
					网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = 角色背包[b5].字节描述()
					});
					角色数据.战备日期.V = 主程.当前时间;
					网络连接?.发送封包(new 同步补充变量
					{
						变量类型 = 1,
						对象编号 = 地图编号,
						变量索引 = 975,
						变量内容 = 计算类.时间转换(主程.当前时间)
					});
					主程.添加系统日志($"[{对象名字}][{当前等级}级] 购买了 [每周战备礼包], 消耗元宝[3000]");
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 8466
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 8451
				});
			}
			break;
		case 1:
			if (元宝数量 >= 600)
			{
				if (!计算类.日期同周(角色数据.补给日期.V, 主程.当前时间))
				{
					if (背包剩余 > 0)
					{
						if (!游戏物品.检索表.TryGetValue("战具礼盒", out var value))
						{
							break;
						}
						byte b = 0;
						while (true)
						{
							if (b < 背包大小)
							{
								if (!角色背包.ContainsKey(b))
								{
									break;
								}
								b = (byte)(b + 1);
								continue;
							}
							return;
						}
						元宝数量 -= 600;
						金币数量 += 165000;
						双倍经验 += 500000;
						角色数据.消耗元宝.V += 600L;
						角色背包[b] = new 物品数据(value, 角色数据, 1, b, 1);
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b].字节描述()
						});
						角色数据.补给日期.V = 主程.当前时间;
						网络连接?.发送封包(new 同步补充变量
						{
							变量类型 = 1,
							对象编号 = 地图编号,
							变量索引 = 112,
							变量内容 = 计算类.时间转换(主程.当前时间)
						});
						主程.添加系统日志($"[{对象名字}][{当前等级}级] 购买了 [每周补给礼包], 消耗元宝[600]");
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1793
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 8466
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 8451
				});
			}
			break;
		}
	}

	public void 购买玛法特权(byte 特权类型, byte 购买数量)
	{
		int num;
		switch (特权类型)
		{
		case 4:
		case 5:
			num = 28800;
			break;
		case 3:
			num = 12800;
			break;
		default:
			return;
		}
		if (元宝数量 >= num)
		{
			元宝数量 -= num;
			角色数据.消耗元宝.V += num;
			if (本期特权 == 0)
			{
				玩家激活特权(特权类型);
			}
			else
			{
				剩余特权[特权类型] += 30;
			}
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 65548,
				第一参数 = 特权类型
			});
			网络连接?.发送封包(new 同步特权信息
			{
				字节数组 = 玛法特权描述()
			});
			switch (特权类型)
			{
			case 3:
				主程.添加系统日志("[" + 对象名字 + "] 购买了 [玛法名俊], 消耗元宝[12800]");
				break;
			case 4:
				主程.添加系统日志("[" + 对象名字 + "] 购买了 [玛法豪杰], 消耗元宝[28800]");
				break;
			case 5:
				主程.添加系统日志("[" + 对象名字 + "] 购买了 [玛法战将], 消耗元宝[28800]");
				break;
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 8451
			});
		}
	}

	public void 预定玛法特权(byte 特权类型)
	{
		if (剩余特权[特权类型] <= 0)
		{
			return;
		}
		if (本期特权 != 0)
		{
			预定特权 = 特权类型;
		}
		else
		{
			玩家激活特权(特权类型);
			if ((剩余特权[特权类型] -= 30) <= 0)
			{
				预定特权 = 0;
			}
		}
		网络连接?.发送封包(new 游戏错误提示
		{
			错误代码 = 65550,
			第一参数 = 预定特权
		});
		网络连接?.发送封包(new 同步特权信息
		{
			字节数组 = 玛法特权描述()
		});
	}

	public void 领取特权礼包(byte 特权类型, byte 礼包位置)
	{
		if (礼包位置 < 28)
		{
			switch (特权类型)
			{
			default:
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 65556
				});
				break;
			case 2:
				if (上期特权 == 3 || 上期特权 == 4)
				{
					if ((上期记录 & (1 << (int)礼包位置)) == 0L)
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 65546
						});
						break;
					}
					switch ((int)礼包位置 % 7)
					{
					case 0:
						上期记录 &= (uint)(~(1 << (int)礼包位置));
						网络连接?.发送封包(new 同步特权信息
						{
							字节数组 = 玛法特权描述()
						});
						金币数量 += ((上期特权 == 3) ? 50000 : 100000);
						break;
					case 1:
					{
						byte b15 = byte.MaxValue;
						byte b16 = 0;
						while (b16 < 背包大小)
						{
							if (角色背包.ContainsKey(b16))
							{
								b16 = (byte)(b16 + 1);
								continue;
							}
							b15 = b16;
							break;
						}
						if (b15 != byte.MaxValue)
						{
							if (游戏物品.检索表.TryGetValue((上期特权 == 3) ? "名俊铭文石礼包" : "豪杰铭文石礼包", out var value8))
							{
								上期记录 &= (uint)(~(1 << (int)礼包位置));
								网络连接?.发送封包(new 同步特权信息
								{
									字节数组 = 玛法特权描述()
								});
								角色背包[b15] = new 物品数据(value8, 角色数据, 1, b15, 1);
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = 角色背包[b15].字节描述()
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						break;
					}
					case 2:
					{
						byte b19 = byte.MaxValue;
						byte b20 = 0;
						while (b20 < 背包大小)
						{
							if (角色背包.ContainsKey(b20))
							{
								b20 = (byte)(b20 + 1);
								continue;
							}
							b19 = b20;
							break;
						}
						游戏物品 value10;
						if (b19 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						else if (游戏物品.检索表.TryGetValue("随机传送石", out value10))
						{
							上期记录 &= (uint)(~(1 << (int)礼包位置));
							网络连接?.发送封包(new 同步特权信息
							{
								字节数组 = 玛法特权描述()
							});
							角色背包[b19] = new 物品数据(value10, 角色数据, 1, b19, 50);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b19].字节描述()
							});
						}
						break;
					}
					case 3:
					{
						byte b23 = byte.MaxValue;
						byte b24 = 0;
						while (b24 < 背包大小)
						{
							if (角色背包.ContainsKey(b24))
							{
								b24 = (byte)(b24 + 1);
								continue;
							}
							b23 = b24;
							break;
						}
						if (b23 != byte.MaxValue)
						{
							if (游戏物品.检索表.TryGetValue((上期特权 == 3) ? "名俊灵石宝盒" : "豪杰灵石宝盒", out var value12))
							{
								上期记录 &= (uint)(~(1 << (int)礼包位置));
								网络连接?.发送封包(new 同步特权信息
								{
									字节数组 = 玛法特权描述()
								});
								角色背包[b23] = new 物品数据(value12, 角色数据, 1, b23, 1);
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = 角色背包[b23].字节描述()
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						break;
					}
					case 4:
					{
						byte b21 = byte.MaxValue;
						byte b22 = 0;
						while (b22 < 背包大小)
						{
							if (角色背包.ContainsKey(b22))
							{
								b22 = (byte)(b22 + 1);
								continue;
							}
							b21 = b22;
							break;
						}
						if (b21 != byte.MaxValue)
						{
							if (游戏物品.检索表.TryGetValue("雕色石", out var value11))
							{
								上期记录 &= (uint)(~(1 << (int)礼包位置));
								网络连接?.发送封包(new 同步特权信息
								{
									字节数组 = 玛法特权描述()
								});
								角色背包[b21] = new 物品数据(value11, 角色数据, 1, b21, (上期特权 == 3) ? 1 : 2);
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = 角色背包[b21].字节描述()
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						break;
					}
					case 5:
					{
						byte b17 = byte.MaxValue;
						byte b18 = 0;
						while (b18 < 背包大小)
						{
							if (角色背包.ContainsKey(b18))
							{
								b18 = (byte)(b18 + 1);
								continue;
							}
							b17 = b18;
							break;
						}
						游戏物品 value9;
						if (b17 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						else if (游戏物品.检索表.TryGetValue("修复油", out value9))
						{
							上期记录 &= (uint)(~(1 << (int)礼包位置));
							网络连接?.发送封包(new 同步特权信息
							{
								字节数组 = 玛法特权描述()
							});
							角色背包[b17] = new 物品数据(value9, 角色数据, 1, b17, (上期特权 == 3) ? 1 : 2);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b17].字节描述()
							});
						}
						break;
					}
					case 6:
					{
						byte b13 = byte.MaxValue;
						byte b14 = 0;
						while (b14 < 背包大小)
						{
							if (角色背包.ContainsKey(b14))
							{
								b14 = (byte)(b14 + 1);
								continue;
							}
							b13 = b14;
							break;
						}
						游戏物品 value7;
						if (b13 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						else if (游戏物品.检索表.TryGetValue("祝福油", out value7))
						{
							上期记录 &= (uint)(~(1 << (int)礼包位置));
							网络连接?.发送封包(new 同步特权信息
							{
								字节数组 = 玛法特权描述()
							});
							角色背包[b13] = new 物品数据(value7, 角色数据, 1, b13, (上期特权 == 3) ? 2 : 4);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b13].字节描述()
							});
						}
						break;
					}
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 65556
					});
				}
				break;
			case 1:
				if (本期特权 != 3 && 本期特权 != 4)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 65556
					});
				}
				else if ((主程.当前时间.Date.AddDays(1.0) - 本期日期.Date).TotalDays < (double)(int)礼包位置)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 65547
					});
				}
				else if ((本期记录 & (1 << (int)礼包位置)) != 0L)
				{
					switch ((int)礼包位置 % 7)
					{
					case 0:
						本期记录 &= (uint)(~(1 << (int)礼包位置));
						网络连接?.发送封包(new 同步特权信息
						{
							字节数组 = 玛法特权描述()
						});
						金币数量 += ((本期特权 == 3) ? 50000 : 100000);
						break;
					case 1:
					{
						byte b5 = byte.MaxValue;
						byte b6 = 0;
						while (b6 < 背包大小)
						{
							if (角色背包.ContainsKey(b6))
							{
								b6 = (byte)(b6 + 1);
								continue;
							}
							b5 = b6;
							break;
						}
						游戏物品 value3;
						if (b5 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						else if (游戏物品.检索表.TryGetValue((本期特权 == 3) ? "名俊铭文石礼包" : "豪杰铭文石礼包", out value3))
						{
							本期记录 &= (uint)(~(1 << (int)礼包位置));
							网络连接?.发送封包(new 同步特权信息
							{
								字节数组 = 玛法特权描述()
							});
							角色背包[b5] = new 物品数据(value3, 角色数据, 1, b5, 1);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b5].字节描述()
							});
						}
						break;
					}
					case 2:
					{
						byte b9 = byte.MaxValue;
						byte b10 = 0;
						while (b10 < 背包大小)
						{
							if (角色背包.ContainsKey(b10))
							{
								b10 = (byte)(b10 + 1);
								continue;
							}
							b9 = b10;
							break;
						}
						if (b9 != byte.MaxValue)
						{
							if (游戏物品.检索表.TryGetValue("随机传送石", out var value5))
							{
								本期记录 &= (uint)(~(1 << (int)礼包位置));
								网络连接?.发送封包(new 同步特权信息
								{
									字节数组 = 玛法特权描述()
								});
								角色背包[b9] = new 物品数据(value5, 角色数据, 1, b9, 50);
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = 角色背包[b9].字节描述()
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						break;
					}
					case 3:
					{
						byte b7 = byte.MaxValue;
						byte b8 = 0;
						while (b8 < 背包大小)
						{
							if (角色背包.ContainsKey(b8))
							{
								b8 = (byte)(b8 + 1);
								continue;
							}
							b7 = b8;
							break;
						}
						if (b7 != byte.MaxValue)
						{
							if (游戏物品.检索表.TryGetValue((本期特权 == 3) ? "名俊灵石宝盒" : "豪杰灵石宝盒", out var value4))
							{
								本期记录 &= (uint)(~(1 << (int)礼包位置));
								网络连接?.发送封包(new 同步特权信息
								{
									字节数组 = 玛法特权描述()
								});
								角色背包[b7] = new 物品数据(value4, 角色数据, 1, b7, 1);
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = 角色背包[b7].字节描述()
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						break;
					}
					case 4:
					{
						byte b3 = byte.MaxValue;
						byte b4 = 0;
						while (b4 < 背包大小)
						{
							if (角色背包.ContainsKey(b4))
							{
								b4 = (byte)(b4 + 1);
								continue;
							}
							b3 = b4;
							break;
						}
						游戏物品 value2;
						if (b3 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						else if (游戏物品.检索表.TryGetValue("雕色石", out value2))
						{
							本期记录 &= (uint)(~(1 << (int)礼包位置));
							网络连接?.发送封包(new 同步特权信息
							{
								字节数组 = 玛法特权描述()
							});
							角色背包[b3] = new 物品数据(value2, 角色数据, 1, b3, (本期特权 == 3) ? 1 : 2);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b3].字节描述()
							});
						}
						break;
					}
					case 5:
					{
						byte b11 = byte.MaxValue;
						byte b12 = 0;
						while (b12 < 背包大小)
						{
							if (角色背包.ContainsKey(b12))
							{
								b12 = (byte)(b12 + 1);
								continue;
							}
							b11 = b12;
							break;
						}
						游戏物品 value6;
						if (b11 == byte.MaxValue)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						else if (游戏物品.检索表.TryGetValue("修复油", out value6))
						{
							本期记录 &= (uint)(~(1 << (int)礼包位置));
							网络连接?.发送封包(new 同步特权信息
							{
								字节数组 = 玛法特权描述()
							});
							角色背包[b11] = new 物品数据(value6, 角色数据, 1, b11, (本期特权 == 3) ? 1 : 2);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b11].字节描述()
							});
						}
						break;
					}
					case 6:
					{
						byte b = byte.MaxValue;
						byte b2 = 0;
						while (b2 < 背包大小)
						{
							if (角色背包.ContainsKey(b2))
							{
								b2 = (byte)(b2 + 1);
								continue;
							}
							b = b2;
							break;
						}
						if (b != byte.MaxValue)
						{
							if (游戏物品.检索表.TryGetValue("祝福油", out var value))
							{
								本期记录 &= (uint)(~(1 << (int)礼包位置));
								网络连接?.发送封包(new 同步特权信息
								{
									字节数组 = 玛法特权描述()
								});
								角色背包[b] = new 物品数据(value, 角色数据, 1, b, (本期特权 == 3) ? 2 : 4);
								网络连接?.发送封包(new 玩家物品变动
								{
									物品描述 = 角色背包[b].字节描述()
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6459
							});
						}
						break;
					}
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 65546
					});
				}
				break;
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 领取特权礼包  错误: 礼包位置错误"));
		}
	}

	public void 玩家使用称号(byte 称号编号)
	{
		游戏称号 value;
		if (!称号列表.ContainsKey(称号编号))
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5377
			});
		}
		else if (游戏称号.数据表.TryGetValue(称号编号, out value))
		{
			if (当前称号 == 称号编号)
			{
				网络连接?.发送封包(new 同步装配称号
				{
					对象编号 = 地图编号,
					称号编号 = 称号编号
				});
				return;
			}
			if (当前称号 != 0)
			{
				战力加成.Remove(当前称号);
				属性加成.Remove(当前称号);
			}
			当前称号 = 称号编号;
			战力加成[称号编号] = value.称号战力;
			更新玩家战力();
			属性加成[称号编号] = value.称号属性;
			更新对象属性();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1500,
				第一参数 = 称号编号
			});
			发送封包(new 同步装配称号
			{
				对象编号 = 地图编号,
				称号编号 = 称号编号
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5378
			});
		}
	}

	public void 玩家卸下称号()
	{
		if (当前称号 != 0)
		{
			if (战力加成.Remove(当前称号))
			{
				更新玩家战力();
			}
			if (属性加成.Remove(当前称号))
			{
				更新对象属性();
			}
			当前称号 = 0;
			发送封包(new 同步装配称号
			{
				对象编号 = 地图编号
			});
		}
	}

	public void 玩家整理背包(byte 背包类型)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (背包类型 == 1)
		{
			List<物品数据> list = 角色背包.Values.ToList();
			list.Sort((物品数据 a, 物品数据 b) => b.物品编号.CompareTo(a.物品编号));
			for (byte b2 = 0; b2 < list.Count; b2 = (byte)(b2 + 1))
			{
				if (list[b2].能否堆叠 && list[b2].当前持久.V < list[b2].最大持久.V)
				{
					for (int i = b2 + 1; i < list.Count; i++)
					{
						if (list[b2].物品编号 == list[i].物品编号)
						{
							int num;
							list[b2].当前持久.V += (num = Math.Min(list[b2].最大持久.V - list[b2].当前持久.V, list[i].当前持久.V));
							if ((list[i].当前持久.V -= num) <= 0)
							{
								list[i].删除数据();
								list.RemoveAt(i);
								i--;
							}
							if (list[b2].当前持久.V >= list[b2].最大持久.V)
							{
								break;
							}
						}
					}
				}
			}
			角色背包.Clear();
			for (byte b3 = 0; b3 < list.Count; b3 = (byte)(b3 + 1))
			{
				角色背包[b3] = list[b3];
				角色背包[b3].当前位置 = b3;
			}
			网络连接?.发送封包(new 同步背包信息
			{
				物品描述 = 背包物品描述()
			});
		}
		if (背包类型 != 2)
		{
			return;
		}
		List<物品数据> list2 = 角色仓库.Values.ToList();
		list2.Sort((物品数据 a, 物品数据 b) => b.物品编号.CompareTo(a.物品编号));
		for (byte b4 = 0; b4 < list2.Count; b4 = (byte)(b4 + 1))
		{
			if (list2[b4].能否堆叠 && list2[b4].当前持久.V < list2[b4].最大持久.V)
			{
				for (int j = b4 + 1; j < list2.Count; j++)
				{
					if (list2[b4].物品编号 == list2[j].物品编号)
					{
						int num2;
						list2[b4].当前持久.V += (num2 = Math.Min(list2[b4].最大持久.V - list2[b4].当前持久.V, list2[j].当前持久.V));
						if ((list2[j].当前持久.V -= num2) <= 0)
						{
							list2[j].删除数据();
							list2.RemoveAt(j);
							j--;
						}
						if (list2[b4].当前持久.V >= list2[b4].最大持久.V)
						{
							break;
						}
					}
				}
			}
		}
		角色仓库.Clear();
		for (byte b5 = 0; b5 < list2.Count; b5 = (byte)(b5 + 1))
		{
			角色仓库[b5] = list2[b5];
			角色仓库[b5].当前位置 = b5;
		}
		网络连接?.发送封包(new 同步背包信息
		{
			物品描述 = 仓库物品描述()
		});
	}

	public void 玩家拾取物品(物品实例 物品)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (物品.物品绑定 && !物品.物品归属.Contains(角色数据))
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2310
			});
		}
		else if (物品.物品归属.Count != 0 && !物品.物品归属.Contains(角色数据) && 主程.当前时间 < 物品.归属时间)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2307
			});
		}
		else if (物品.物品重量 != 0 && 物品.物品重量 > 最大负重 - 背包重量)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1863
			});
		}
		else if (物品.默认持久 != 0 && 背包剩余 <= 0)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1793
			});
		}
		else if (物品.物品编号 != 1)
		{
			byte b = 0;
			while (true)
			{
				if (b < 背包大小)
				{
					if (!角色背包.ContainsKey(b))
					{
						break;
					}
					b = (byte)(b + 1);
					continue;
				}
				return;
			}
			if (物品.物品数据 != null)
			{
				角色背包[b] = 物品.物品数据;
				物品.物品数据.物品位置.V = b;
				物品.物品数据.物品容器.V = 1;
			}
			else if (!(物品.物品模板 is 游戏装备 模板))
			{
				if (物品.持久类型 == 物品持久分类.容器)
				{
					角色背包[b] = new 物品数据(物品.物品模板, 角色数据, 1, b, 0);
				}
				else if (物品.持久类型 != 物品持久分类.堆叠)
				{
					角色背包[b] = new 物品数据(物品.物品模板, 角色数据, 1, b, 物品.默认持久);
				}
				else
				{
					角色背包[b] = new 物品数据(物品.物品模板, 角色数据, 1, b, 物品.堆叠数量);
				}
			}
			else
			{
				角色背包[b] = new 装备数据(模板, 角色数据, 1, b, 随机生成: true);
			}
			网络连接?.发送封包(new 玩家拾取物品
			{
				物品描述 = 角色背包[b].字节描述(),
				角色编号 = 地图编号
			});
			网络连接?.发送封包(new 玩家物品变动
			{
				物品描述 = 角色背包[b].字节描述()
			});
			物品.物品转移处理();
		}
		else
		{
			网络连接?.发送封包(new 玩家拾取金币
			{
				金币数量 = 物品.堆叠数量
			});
			金币数量 += 物品.堆叠数量;
			物品.物品转移处理();
		}
	}

	public void 玩家丢弃物品(byte 背包类型, byte 物品位置, ushort 丢弃数量)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3 && 当前等级 > 7 && 背包类型 == 1 && 角色背包.TryGetValue(物品位置, out var v))
		{
			if (v.是否绑定)
			{
				new 物品实例(v.物品模板, v, 当前地图, 当前坐标, new HashSet<角色数据> { 角色数据 }, 0, 物品绑定: true);
			}
			else
			{
				new 物品实例(v.物品模板, v, 当前地图, 当前坐标, new HashSet<角色数据>());
			}
			角色背包.Remove(v.物品位置.V);
			网络连接?.发送封包(new 删除玩家物品
			{
				背包类型 = 背包类型,
				物品位置 = 物品位置
			});
		}
	}

	public void 玩家拆分物品(byte 当前背包, byte 物品位置, ushort 拆分数量, byte 目标背包, byte 目标位置)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3 && 当前背包 == 1 && 角色背包.TryGetValue(物品位置, out var v) && 目标背包 == 1 && 目标位置 < 背包大小 && v != null && v.持久类型 == 物品持久分类.堆叠 && v.当前持久.V > 拆分数量 && !角色背包.TryGetValue(目标位置, out var _))
		{
			v.当前持久.V -= 拆分数量;
			网络连接?.发送封包(new 玩家物品变动
			{
				物品描述 = v.字节描述()
			});
			角色背包[目标位置] = new 物品数据(v.物品模板, 角色数据, 目标背包, 目标位置, 拆分数量);
			网络连接?.发送封包(new 玩家物品变动
			{
				物品描述 = 角色背包[目标位置].字节描述()
			});
		}
	}

	public void 玩家分解物品(byte 背包类型, byte 物品位置, byte 分解数量)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3)
		{
			物品数据 v;
			if (背包类型 != 1)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家分解物品.  错误: 背包类型错误."));
			}
			else if (角色背包.TryGetValue(物品位置, out v))
			{
				if (v is 装备数据 装备数据 && 装备数据.能否出售)
				{
					if (角色数据.分解日期.V.Date != 主程.当前时间.Date)
					{
						角色数据.分解日期.V = 主程.当前时间;
						角色数据.分解经验.V = 0;
					}
					int 出售价格 = 装备数据.出售价格;
					int num = (int)Math.Max(0f, (float)出售价格 * (1f - (float)角色数据.分解经验.V / 1500000f));
					金币数量 += Math.Max(1, 出售价格 / 2);
					双倍经验 += num;
					角色数据.分解经验.V += num;
					角色背包.Remove(装备数据.当前位置);
					装备数据.删除数据();
					网络连接?.发送封包(new 删除玩家物品
					{
						背包类型 = 背包类型,
						物品位置 = 物品位置
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1802
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1877
			});
		}
	}

	public void 玩家转移物品(byte 当前背包, byte 当前位置, byte 目标背包, byte 目标位置)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3 || (当前背包 == 0 && 当前位置 >= 16) || (当前背包 == 1 && 当前位置 >= 背包大小) || (当前背包 == 2 && 当前位置 >= 仓库大小) || (当前背包 == 7 && 当前位置 >= 资源背包大小) || (目标背包 == 0 && 目标位置 >= 16) || (目标背包 == 1 && 目标位置 >= 背包大小) || (目标背包 == 2 && 目标位置 >= 仓库大小) || (目标背包 == 7 && 目标位置 >= 资源背包大小))
		{
			return;
		}
		物品数据 物品数据 = null;
		if (当前背包 == 0)
		{
			物品数据 = (角色装备.TryGetValue(当前位置, out var v) ? v : null);
		}
		if (当前背包 == 1)
		{
			物品数据 = (角色背包.TryGetValue(当前位置, out var v2) ? v2 : null);
		}
		if (当前背包 == 2)
		{
			物品数据 = (角色仓库.TryGetValue(当前位置, out var v3) ? v3 : null);
		}
		if (当前背包 == 7)
		{
			物品数据 = (角色资源背包.TryGetValue(当前位置, out var v4) ? v4 : null);
		}
		物品数据 物品数据2 = null;
		if (目标背包 == 0)
		{
			物品数据2 = (角色装备.TryGetValue(目标位置, out var v5) ? v5 : null);
		}
		if (目标背包 == 1)
		{
			物品数据2 = (角色背包.TryGetValue(目标位置, out var v6) ? v6 : null);
		}
		if (目标背包 == 2)
		{
			物品数据2 = (角色仓库.TryGetValue(目标位置, out var v7) ? v7 : null);
		}
		if (目标背包 == 7)
		{
			物品数据2 = (角色资源背包.TryGetValue(目标位置, out var v8) ? v8 : null);
		}
		if ((物品数据 == null && 物品数据2 == null) || (当前背包 == 0 && 目标背包 == 0) || (当前背包 == 0 && 目标背包 == 2) || (当前背包 == 2 && 目标背包 == 0) || (物品数据 != null && 当前背包 == 0 && (物品数据 as 装备数据).禁止卸下) || (物品数据2 != null && 目标背包 == 0 && (物品数据2 as 装备数据).禁止卸下) || (物品数据 != null && 目标背包 == 0 && (!(物品数据 is 装备数据 装备数据) || 装备数据.需要等级 > 当前等级 || (装备数据.需要性别 != 0 && 装备数据.需要性别 != 角色性别) || (装备数据.需要职业 != 游戏对象职业.通用 && 装备数据.需要职业 != 角色职业) || 装备数据.需要攻击 > this[游戏对象属性.最大攻击] || 装备数据.需要魔法 > this[游戏对象属性.最大魔法] || 装备数据.需要道术 > this[游戏对象属性.最大道术] || 装备数据.需要刺术 > this[游戏对象属性.最大刺术] || 装备数据.需要弓术 > this[游戏对象属性.最大弓术] || (目标位置 == 0 && 装备数据.物品重量 > 最大腕力) || (目标位置 != 0 && 装备数据.物品重量 - 物品数据2?.物品重量 > 最大穿戴 - 装备重量) || (目标位置 == 0 && 装备数据.物品类型 != 物品使用分类.武器) || (目标位置 == 1 && 装备数据.物品类型 != 物品使用分类.衣服) || (目标位置 == 2 && 装备数据.物品类型 != 物品使用分类.披风) || (目标位置 == 3 && 装备数据.物品类型 != 物品使用分类.头盔) || (目标位置 == 4 && 装备数据.物品类型 != 物品使用分类.护肩) || (目标位置 == 5 && 装备数据.物品类型 != 物品使用分类.护腕) || (目标位置 == 6 && 装备数据.物品类型 != 物品使用分类.腰带) || (目标位置 == 7 && 装备数据.物品类型 != 物品使用分类.鞋子) || (目标位置 == 8 && 装备数据.物品类型 != 物品使用分类.项链) || (目标位置 == 13 && 装备数据.物品类型 != 物品使用分类.勋章) || (目标位置 == 14 && 装备数据.物品类型 != 物品使用分类.玉佩) || (目标位置 == 15 && 装备数据.物品类型 != 物品使用分类.战具) || (目标位置 == 9 && 装备数据.物品类型 != 物品使用分类.戒指) || (目标位置 == 10 && 装备数据.物品类型 != 物品使用分类.戒指) || (目标位置 == 11 && 装备数据.物品类型 != 物品使用分类.手镯) || (目标位置 == 12 && 装备数据.物品类型 != 物品使用分类.手镯))) || (物品数据2 != null && 当前背包 == 0 && (!(物品数据2 is 装备数据 装备数据2) || 装备数据2.需要等级 > 当前等级 || (装备数据2.需要性别 != 0 && 装备数据2.需要性别 != 角色性别) || (装备数据2.需要职业 != 游戏对象职业.通用 && 装备数据2.需要职业 != 角色职业) || 装备数据2.需要攻击 > this[游戏对象属性.最大攻击] || 装备数据2.需要魔法 > this[游戏对象属性.最大魔法] || 装备数据2.需要道术 > this[游戏对象属性.最大道术] || 装备数据2.需要刺术 > this[游戏对象属性.最大刺术] || 装备数据2.需要弓术 > this[游戏对象属性.最大弓术] || (目标位置 == 0 && 装备数据2.物品重量 > 最大腕力) || (当前位置 == 0 && 装备数据2.物品重量 > 最大腕力) || (当前位置 != 0 && 装备数据2.物品重量 - 物品数据?.物品重量 > 最大穿戴 - 装备重量) || (当前位置 == 0 && 装备数据2.物品类型 != 物品使用分类.武器) || (当前位置 == 1 && 装备数据2.物品类型 != 物品使用分类.衣服) || (当前位置 == 2 && 装备数据2.物品类型 != 物品使用分类.披风) || (当前位置 == 3 && 装备数据2.物品类型 != 物品使用分类.头盔) || (当前位置 == 4 && 装备数据2.物品类型 != 物品使用分类.护肩) || (当前位置 == 5 && 装备数据2.物品类型 != 物品使用分类.护腕) || (当前位置 == 6 && 装备数据2.物品类型 != 物品使用分类.腰带) || (当前位置 == 7 && 装备数据2.物品类型 != 物品使用分类.鞋子) || (当前位置 == 8 && 装备数据2.物品类型 != 物品使用分类.项链) || (当前位置 == 13 && 装备数据2.物品类型 != 物品使用分类.勋章) || (当前位置 == 14 && 装备数据2.物品类型 != 物品使用分类.玉佩) || (当前位置 == 15 && 装备数据2.物品类型 != 物品使用分类.战具) || (当前位置 == 9 && 装备数据2.物品类型 != 物品使用分类.戒指) || (当前位置 == 10 && 装备数据2.物品类型 != 物品使用分类.戒指) || (当前位置 == 11 && 装备数据2.物品类型 != 物品使用分类.手镯) || (当前位置 == 12 && 装备数据2.物品类型 != 物品使用分类.手镯))))
		{
			return;
		}
		if (物品数据 != null && 物品数据2 != null && 物品数据.能否堆叠 && 物品数据2.物品编号 == 物品数据.物品编号 && 物品数据.堆叠上限 > 物品数据.当前持久.V && 物品数据2.堆叠上限 > 物品数据2.当前持久.V)
		{
			int num = Math.Min(物品数据.当前持久.V, 物品数据2.堆叠上限 - 物品数据2.当前持久.V);
			物品数据2.当前持久.V += num;
			物品数据.当前持久.V -= num;
			客户网络 客户网络 = 网络连接;
			客户网络?.发送封包(new 玩家物品变动
			{
				物品描述 = 物品数据2.字节描述()
			});
			if (物品数据.当前持久.V <= 0)
			{
				物品数据.删除数据();
				switch (当前背包)
				{
				case 1:
					角色背包.Remove(当前位置);
					break;
				case 2:
					角色仓库.Remove(当前位置);
					break;
				}
				客户网络.发送封包(new 删除玩家物品
				{
					背包类型 = 当前背包,
					物品位置 = 当前位置
				});
			}
			else
			{
				客户网络.发送封包(new 玩家物品变动
				{
					物品描述 = 物品数据.字节描述()
				});
			}
			return;
		}
		if (物品数据 != null)
		{
			switch (当前背包)
			{
			case 0:
				角色装备.Remove(当前位置);
				break;
			case 1:
				角色背包.Remove(当前位置);
				break;
			case 2:
				角色仓库.Remove(当前位置);
				break;
			case 7:
				角色资源背包.Remove(当前位置);
				break;
			}
			物品数据.物品容器.V = 目标背包;
			物品数据.物品位置.V = 目标位置;
		}
		if (物品数据2 != null)
		{
			switch (目标背包)
			{
			case 0:
				角色装备.Remove(目标位置);
				break;
			case 1:
				角色背包.Remove(目标位置);
				break;
			case 2:
				角色仓库.Remove(目标位置);
				break;
			case 7:
				角色资源背包.Remove(目标位置);
				break;
			}
			物品数据2.物品容器.V = 当前背包;
			物品数据2.物品位置.V = 当前位置;
		}
		if (物品数据 != null)
		{
			switch (目标背包)
			{
			case 0:
				角色装备[目标位置] = 物品数据 as 装备数据;
				break;
			case 1:
				角色背包[目标位置] = 物品数据;
				break;
			case 2:
				角色仓库[目标位置] = 物品数据;
				break;
			case 7:
				角色资源背包[目标位置] = 物品数据;
				break;
			}
		}
		if (物品数据2 != null)
		{
			switch (当前背包)
			{
			case 0:
				角色装备[当前位置] = 物品数据2 as 装备数据;
				break;
			case 1:
				角色背包[当前位置] = 物品数据2;
				break;
			case 2:
				角色仓库[当前位置] = 物品数据2;
				break;
			case 7:
				角色资源背包[当前位置] = 物品数据2;
				break;
			}
		}
		网络连接?.发送封包(new 玩家转移物品
		{
			原有容器 = 当前背包,
			目标容器 = 目标背包,
			原有位置 = 当前位置,
			目标位置 = 目标位置
		});
		if (目标背包 == 0)
		{
			玩家穿卸装备((装备穿戴部位)目标位置, (装备数据)物品数据2, (装备数据)物品数据);
		}
		else if (当前背包 == 0)
		{
			玩家穿卸装备((装备穿戴部位)当前位置, (装备数据)物品数据, (装备数据)物品数据2);
		}
	}

	public void 玩家使用物品(byte 背包类型, byte 物品位置)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3)
		{
			物品数据 v;
			if (背包类型 != 1)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家使用物品.  错误: 背包类型错误."));
			}
			else if (!角色背包.TryGetValue(物品位置, out v))
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1802
				});
			}
			else if (当前等级 < v.需要等级)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家使用物品.  错误: 等级无法使用."));
			}
			else if (v.需要职业 == 游戏对象职业.通用 || 角色职业 == v.需要职业)
			{
				if (v.需要职业 == 游戏对象职业.通用 || 角色职业 == v.需要职业)
				{
					if (!冷却记录.TryGetValue(v.物品编号 | 0x2000000, out var v2) || !(主程.当前时间 < v2))
					{
						if (v.分组编号 <= 0 || !冷却记录.TryGetValue(v.分组编号 | 0, out var v3) || !(主程.当前时间 < v3))
						{
							switch (v.物品名字)
							{
							case "豪杰灵石宝盒":
							{
								byte b5 = byte.MaxValue;
								byte b6 = 0;
								while (b6 < 背包大小)
								{
									if (角色背包.ContainsKey(b6))
									{
										b6 = (byte)(b6 + 1);
										continue;
									}
									b5 = b6;
									break;
								}
								if (b5 == byte.MaxValue)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
									break;
								}
								游戏物品 value3 = null;
								switch (主程.随机数.Next(8))
								{
								case 0:
									游戏物品.检索表.TryGetValue("驭朱灵石1级", out value3);
									break;
								case 1:
									游戏物品.检索表.TryGetValue("命朱灵石1级", out value3);
									break;
								case 2:
									游戏物品.检索表.TryGetValue("守阳灵石1级", out value3);
									break;
								case 3:
									游戏物品.检索表.TryGetValue("蔚蓝灵石1级", out value3);
									break;
								case 4:
									游戏物品.检索表.TryGetValue("精绿灵石1级", out value3);
									break;
								case 5:
									游戏物品.检索表.TryGetValue("纯紫灵石1级", out value3);
									break;
								case 6:
									游戏物品.检索表.TryGetValue("深灰灵石1级", out value3);
									break;
								case 7:
									游戏物品.检索表.TryGetValue("橙黄灵石1级", out value3);
									break;
								}
								if (value3 != null)
								{
									消耗背包物品(1, v);
									角色背包[b5] = new 物品数据(value3, 角色数据, 背包类型, b5, 2);
									网络连接?.发送封包(new 玩家物品变动
									{
										物品描述 = 角色背包[b5].字节描述()
									});
								}
								break;
							}
							case "精准打击":
								if (玩家学习技能(2042))
								{
									消耗背包物品(1, v);
								}
								break;
							case "神圣战甲术":
								if (玩家学习技能(3007))
								{
									消耗背包物品(1, v);
								}
								break;
							case "施毒术":
								if (玩家学习技能(3004))
								{
									消耗背包物品(1, v);
								}
								break;
							case "寒冰掌":
								if (玩家学习技能(2550))
								{
									消耗背包物品(1, v);
								}
								break;
							case "金创药(中量)":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								药品回血 = 主程.当前时间.AddSeconds(1.0);
								回血基数 = 10;
								回血次数 = 5;
								break;
							case "战具礼盒":
							{
								byte b35 = byte.MaxValue;
								byte b36 = 0;
								while (b36 < 背包大小)
								{
									if (角色背包.ContainsKey(b36))
									{
										b36 = (byte)(b36 + 1);
										continue;
									}
									b35 = b36;
									break;
								}
								if (b35 == byte.MaxValue)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
									break;
								}
								游戏物品 value21 = null;
								if (角色职业 == 游戏对象职业.战士)
								{
									游戏物品.检索表.TryGetValue("气血石", out value21);
								}
								else if (角色职业 != 游戏对象职业.法师)
								{
									if (角色职业 != 游戏对象职业.道士)
									{
										if (角色职业 == 游戏对象职业.刺客)
										{
											游戏物品.检索表.TryGetValue("吸血令", out value21);
										}
										else if (角色职业 == 游戏对象职业.弓手)
										{
											游戏物品.检索表.TryGetValue("守护箭袋", out value21);
										}
										else if (角色职业 == 游戏对象职业.龙枪)
										{
											游戏物品.检索表.TryGetValue("血精石", out value21);
										}
									}
									else
									{
										游戏物品.检索表.TryGetValue("万灵符", out value21);
									}
								}
								else
								{
									游戏物品.检索表.TryGetValue("魔法石", out value21);
								}
								if (value21 != null && value21 is 游戏装备 模板2)
								{
									消耗背包物品(1, v);
									角色背包[b35] = new 装备数据(模板2, 角色数据, 背包类型, b35);
									网络连接?.发送封包(new 玩家物品变动
									{
										物品描述 = 角色背包[b35].字节描述()
									});
								}
								break;
							}
							case "御龙晶甲":
								if (玩家学习技能(1209))
								{
									消耗背包物品(1, v);
								}
								break;
							case "击飞射击":
								if (玩家学习技能(2046))
								{
									消耗背包物品(1, v);
								}
								break;
							case "大火球":
								if (玩家学习技能(2549))
								{
									消耗背包物品(1, v);
								}
								break;
							case "疗伤药包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("疗伤药", out var value8))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b15 = 0;
									byte b16 = 0;
									while (b15 < 背包大小 && b16 < 6)
									{
										if (!角色背包.ContainsKey(b15))
										{
											角色背包[b15] = new 物品数据(value8, 角色数据, 1, b15, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b15].字节描述()
											});
											b16 = (byte)(b16 + 1);
										}
										b15 = (byte)(b15 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "强效金创药":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								药品回血 = 主程.当前时间.AddSeconds(1.0);
								回血基数 = 15;
								回血次数 = 6;
								break;
							case "万年雪霜":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								当前体力 += (int)Math.Max(75f * (1f + (float)this[游戏对象属性.药品回血] / 10000f), 0f);
								当前魔力 += (int)Math.Max(100f * (1f + (float)this[游戏对象属性.药品回魔] / 10000f), 0f);
								break;
							case "地狱火":
								if (玩家学习技能(2544))
								{
									消耗背包物品(1, v);
								}
								break;
							case "鬼灵步":
								if (玩家学习技能(1537))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·羿神庇佑":
								if (玩家学习技能(2049))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔龙城回城卷包":
								if (背包大小 - 角色背包.Count < 5)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								else
								{
									if (!游戏物品.检索表.TryGetValue("魔龙城回城卷", out var value20))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b33 = 0;
									byte b34 = 0;
									while (b33 < 背包大小 && b34 < 6)
									{
										if (!角色背包.ContainsKey(b33))
										{
											角色背包[b33] = new 物品数据(value20, 角色数据, 1, b33, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b33].字节描述()
											});
											b34 = (byte)(b34 + 1);
										}
										b33 = (byte)(b33 + 1);
									}
								}
								break;
							case "中平枪术":
								if (玩家学习技能(1201))
								{
									消耗背包物品(1, v);
								}
								break;
							case "燃血化元":
								if (玩家学习技能(1211))
								{
									消耗背包物品(1, v);
								}
								break;
							case "冰咆哮":
								if (玩家学习技能(2537))
								{
									消耗背包物品(1, v);
								}
								break;
							case "二连射":
								if (玩家学习技能(2043))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·法神奥义":
								if (玩家学习技能(2557))
								{
									消耗背包物品(1, v);
								}
								break;
							case "强效太阳水":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								当前体力 += (int)Math.Max(50f * (1f + (float)this[游戏对象属性.药品回血] / 10000f), 0f);
								当前魔力 += (int)Math.Max(80f * (1f + (float)this[游戏对象属性.药品回魔] / 10000f), 0f);
								break;
							case "元宝袋(小)":
								消耗背包物品(1, v);
								元宝数量 += 100;
								break;
							case "献祭":
								if (玩家学习技能(1545))
								{
									消耗背包物品(1, v);
								}
								break;
							case "幽灵盾":
								if (玩家学习技能(3006))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔法药(小)包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("魔法药(小量)", out var value12))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b23 = 0;
									byte b24 = 0;
									while (b23 < 背包大小 && b24 < 6)
									{
										if (!角色背包.ContainsKey(b23))
										{
											角色背包[b23] = new 物品数据(value12, 角色数据, 1, b23, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b23].字节描述()
											});
											b24 = (byte)(b24 + 1);
										}
										b23 = (byte)(b23 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "盟重回城卷包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("盟重回城卷", out var value23))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b39 = 0;
									byte b40 = 0;
									while (b39 < 背包大小 && b40 < 6)
									{
										if (!角色背包.ContainsKey(b39))
										{
											角色背包[b39] = new 物品数据(value23, 角色数据, 1, b39, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b39].字节描述()
											});
											b40 = (byte)(b40 + 1);
										}
										b39 = (byte)(b39 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "火球术":
								if (玩家学习技能(2531))
								{
									消耗背包物品(1, v);
								}
								break;
							case "守护箭羽":
								if (玩家学习技能(2052))
								{
									消耗背包物品(1, v);
								}
								break;
							case "灵魂火符":
								if (玩家学习技能(3005))
								{
									消耗背包物品(1, v);
								}
								break;
							case "狮子吼":
								if (玩家学习技能(1037))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔法盾":
								if (玩家学习技能(2535))
								{
									消耗背包物品(1, v);
								}
								break;
							case "盟重回城卷":
								消耗背包物品(1, v);
								玩家切换地图((当前地图.地图编号 == 147) ? 当前地图 : 地图处理网关.分配地图(147), 地图区域类型.复活区域);
								break;
							case "回避射击":
								if (玩家学习技能(2056))
								{
									消耗背包物品(1, v);
								}
								break;
							case "困魔咒":
								if (玩家学习技能(3011))
								{
									消耗背包物品(1, v);
								}
								break;
							case "噬血术":
								if (玩家学习技能(3010))
								{
									消耗背包物品(1, v);
								}
								break;
							case "祝福油":
							{
								if (!角色装备.TryGetValue(0, out var v5))
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1927
									});
								}
								else if (v5.幸运等级.V < 7)
								{
									消耗背包物品(1, v);
									int num2 = 0;
									num2 = v5.幸运等级.V switch
									{
										0 => 80, 
										1 => 10, 
										2 => 8, 
										3 => 6, 
										4 => 5, 
										5 => 4, 
										6 => 3, 
										_ => 80, 
									};
									int num3 = 主程.随机数.Next(100);
									if (num3 < num2)
									{
										v5.幸运等级.V++;
										网络连接?.发送封包(new 玩家物品变动
										{
											物品描述 = v5.字节描述()
										});
										网络连接?.发送封包(new 武器幸运变化
										{
											幸运变化 = 1
										});
										属性加成[v5] = v5.装备属性;
										更新对象属性();
										if (v5.幸运等级.V >= 5)
										{
											网络服务网关.发送公告($"[{对象名字}] 成功将 [{v5.物品名字}] 升到幸运 {v5.幸运等级.V} 级.");
										}
									}
									else if (num3 >= 95 && v5.幸运等级.V > -9)
									{
										v5.幸运等级.V--;
										网络连接?.发送封包(new 玩家物品变动
										{
											物品描述 = v5.字节描述()
										});
										网络连接?.发送封包(new 武器幸运变化
										{
											幸运变化 = -1
										});
										属性加成[v5] = v5.装备属性;
										更新对象属性();
									}
									else
									{
										网络连接?.发送封包(new 武器幸运变化
										{
											幸运变化 = 0
										});
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1843
									});
								}
								break;
							}
							case "金创药(小量)":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								药品回血 = 主程.当前时间.AddSeconds(1.0);
								回血基数 = 5;
								回血次数 = 4;
								break;
							case "灭天火":
								if (玩家学习技能(2539))
								{
									消耗背包物品(1, v);
								}
								break;
							case "穿刺射击":
								if (玩家学习技能(2050))
								{
									消耗背包物品(1, v);
								}
								break;
							case "圣言术":
								if (玩家学习技能(2547))
								{
									消耗背包物品(1, v);
								}
								break;
							case "连环暗雷":
								if (玩家学习技能(2047))
								{
									消耗背包物品(1, v);
								}
								break;
							case "爆裂火焰":
								if (玩家学习技能(2545))
								{
									消耗背包物品(1, v);
								}
								break;
							case "群体治愈术":
								if (玩家学习技能(3012))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·百战军魂":
								if (玩家学习技能(1214))
								{
									消耗背包物品(1, v);
								}
								break;
							case "万年雪霜包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("万年雪霜", out var value5))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b9 = 0;
									byte b10 = 0;
									while (b9 < 背包大小 && b10 < 6)
									{
										if (!角色背包.ContainsKey(b9))
										{
											角色背包[b9] = new 物品数据(value5, 角色数据, 1, b9, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b9].字节描述()
											});
											b10 = (byte)(b10 + 1);
										}
										b9 = (byte)(b9 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "觉醒·金钟罩":
								if (玩家学习技能(1047))
								{
									消耗背包物品(1, v);
								}
								break;
							case "太阳水包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("太阳水", out var value24))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b41 = 0;
									byte b42 = 0;
									while (b41 < 背包大小 && b42 < 6)
									{
										if (!角色背包.ContainsKey(b41))
										{
											角色背包[b41] = new 物品数据(value24, 角色数据, 1, b41, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b41].字节描述()
											});
											b42 = (byte)(b42 + 1);
										}
										b41 = (byte)(b41 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "比奇回城卷":
								消耗背包物品(1, v);
								玩家切换地图((当前地图.地图编号 == 143) ? 当前地图 : 地图处理网关.分配地图(143), 地图区域类型.复活区域);
								break;
							case "气功波":
								if (玩家学习技能(3018))
								{
									消耗背包物品(1, v);
								}
								break;
							case "基础射击":
								if (玩家学习技能(2041))
								{
									消耗背包物品(1, v);
								}
								break;
							case "枪出如龙":
								if (玩家学习技能(1208))
								{
									消耗背包物品(1, v);
								}
								break;
							case "抗拒火环":
								if (玩家学习技能(2532))
								{
									消耗背包物品(1, v);
								}
								break;
							case "瞬息移动":
								if (玩家学习技能(2538))
								{
									消耗背包物品(1, v);
								}
								break;
							case "无极真气":
								if (玩家学习技能(3015))
								{
									消耗背包物品(1, v);
								}
								break;
							case "基本剑术":
								if (玩家学习技能(1031))
								{
									消耗背包物品(1, v);
								}
								break;
							case "强袭":
								if (玩家学习技能(2048))
								{
									消耗背包物品(1, v);
								}
								break;
							case "太阳水":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								当前体力 += (int)Math.Max(30f * (1f + (float)this[游戏对象属性.药品回血] / 10000f), 0f);
								当前魔力 += (int)Math.Max(40f * (1f + (float)this[游戏对象属性.药品回魔] / 10000f), 0f);
								break;
							case "旋风腿":
								if (玩家学习技能(1536))
								{
									消耗背包物品(1, v);
								}
								break;
							case "伏波荡寇":
								if (玩家学习技能(1202))
								{
									消耗背包物品(1, v);
								}
								break;
							case "狂飙突刺":
								if (玩家学习技能(1204))
								{
									消耗背包物品(1, v);
								}
								break;
							case "金创药(小)包":
								if (背包大小 - 角色背包.Count < 5)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								else
								{
									if (!游戏物品.检索表.TryGetValue("金创药(小量)", out var value4))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b7 = 0;
									byte b8 = 0;
									while (b7 < 背包大小 && b8 < 6)
									{
										if (!角色背包.ContainsKey(b7))
										{
											角色背包[b7] = new 物品数据(value4, 角色数据, 1, b7, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b7].字节描述()
											});
											b8 = (byte)(b8 + 1);
										}
										b7 = (byte)(b7 + 1);
									}
								}
								break;
							case "致残毒药":
								if (玩家学习技能(1533))
								{
									消耗背包物品(1, v);
								}
								break;
							case "铭文位切换神符":
							{
								if (角色装备.TryGetValue(0, out var v4))
								{
									if (v4.双铭文栏.V)
									{
										if (v4.第一铭文 != null)
										{
											玩家装卸铭文(v4.第一铭文.技能编号, 0);
										}
										if (v4.第二铭文 != null)
										{
											玩家装卸铭文(v4.第二铭文.技能编号, 0);
										}
										v4.当前铭栏.V = (byte)((v4.当前铭栏.V == 0) ? 1u : 0u);
										if (v4.第一铭文 != null)
										{
											玩家装卸铭文(v4.第一铭文.技能编号, v4.第一铭文.铭文编号);
										}
										if (v4.第二铭文 != null)
										{
											玩家装卸铭文(v4.第二铭文.技能编号, v4.第二铭文.铭文编号);
										}
										网络连接?.发送封包(new 玩家物品变动
										{
											物品描述 = v4.字节描述()
										});
										客户网络 客户网络 = 网络连接;
										if (客户网络 != null)
										{
											双铭文位切换 双铭文位切换 = new 双铭文位切换
											{
												当前栏位 = v4.当前铭栏.V
											};
											双铭文位切换.第一铭文 = v4.第一铭文?.铭文索引 ?? 0;
											双铭文位切换.第二铭文 = v4.第二铭文?.铭文索引 ?? 0;
											客户网络.发送封包(双铭文位切换);
										}
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
										消耗背包物品(1, v);
										客户网络 客户网络2 = 网络连接;
										if (客户网络2 != null)
										{
											双铭文位切换 双铭文位切换2 = new 双铭文位切换
											{
												当前栏位 = v4.当前铭栏.V
											};
											双铭文位切换2.第一铭文 = v4.第一铭文?.铭文索引 ?? 0;
											双铭文位切换2.第二铭文 = v4.第二铭文?.铭文索引 ?? 0;
											客户网络2.发送封包(双铭文位切换2);
										}
									}
									else
									{
										网络连接?.发送封包(new 游戏错误提示
										{
											错误代码 = 1926
										});
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1927
									});
								}
								break;
							}
							case "道尊天谕":
								if (玩家学习技能(3022))
								{
									消耗背包物品(1, v);
								}
								break;
							case "强化战具礼盒":
							{
								byte b11 = byte.MaxValue;
								byte b12 = 0;
								while (b12 < 背包大小)
								{
									if (角色背包.ContainsKey(b12))
									{
										b12 = (byte)(b12 + 1);
										continue;
									}
									b11 = b12;
									break;
								}
								if (b11 != byte.MaxValue)
								{
									游戏物品 value6 = null;
									if (角色职业 != 0)
									{
										if (角色职业 == 游戏对象职业.法师)
										{
											游戏物品.检索表.TryGetValue("幻魔石", out value6);
										}
										else if (角色职业 == 游戏对象职业.道士)
										{
											游戏物品.检索表.TryGetValue("圣灵符", out value6);
										}
										else if (角色职业 != 游戏对象职业.刺客)
										{
											if (角色职业 != 游戏对象职业.弓手)
											{
												if (角色职业 == 游戏对象职业.龙枪)
												{
													游戏物品.检索表.TryGetValue("龙晶石", out value6);
												}
											}
											else
											{
												游戏物品.检索表.TryGetValue("射手箭袋", out value6);
											}
										}
										else
										{
											游戏物品.检索表.TryGetValue("狂血令", out value6);
										}
									}
									else
									{
										游戏物品.检索表.TryGetValue("灵疗石", out value6);
									}
									if (value6 != null && value6 is 游戏装备 模板)
									{
										消耗背包物品(1, v);
										角色背包[b11] = new 装备数据(模板, 角色数据, 背包类型, b11);
										网络连接?.发送封包(new 玩家物品变动
										{
											物品描述 = 角色背包[b11].字节描述()
										});
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							}
							case "随机传送卷包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("随机传送卷", out var value26))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b45 = 0;
									byte b46 = 0;
									while (b45 < 背包大小 && b46 < 6)
									{
										if (!角色背包.ContainsKey(b45))
										{
											角色背包[b45] = new 物品数据(value26, 角色数据, 1, b45, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b45].字节描述()
											});
											b46 = (byte)(b46 + 1);
										}
										b45 = (byte)(b45 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "觉醒·盘龙枪势":
								if (玩家学习技能(1213))
								{
									消耗背包物品(1, v);
								}
								break;
							case "随机传送卷":
							{
								Point point2 = 当前地图.随机传送(当前坐标);
								if (point2 != default(Point))
								{
									消耗背包物品(1, v);
									玩家切换地图(当前地图, 地图区域类型.未知区域, point2);
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 776
									});
								}
								break;
							}
							case "地狱雷光":
								if (玩家学习技能(2546))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·召唤月灵":
								if (玩家学习技能(3024))
								{
									消耗背包物品(1, v);
								}
								break;
							case "神威盾甲":
								if (玩家学习技能(1046))
								{
									消耗背包物品(1, v);
								}
								break;
							case "逐日剑法":
								if (玩家学习技能(1038))
								{
									消耗背包物品(1, v);
								}
								break;
							case "刺杀剑术":
								if (玩家学习技能(1033))
								{
									消耗背包物品(1, v);
								}
								break;
							case "诱惑之光":
								if (玩家学习技能(2541))
								{
									消耗背包物品(1, v);
								}
								break;
							case "疗伤药":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								当前体力 += (int)Math.Max(100f * (1f + (float)this[游戏对象属性.药品回血] / 10000f), 0f);
								当前魔力 += (int)Math.Max(160f * (1f + (float)this[游戏对象属性.药品回魔] / 10000f), 0f);
								break;
							case "乾坤斗气":
								if (玩家学习技能(1206))
								{
									消耗背包物品(1, v);
								}
								break;
							case "沙巴克回城卷包":
								if (背包大小 - 角色背包.Count < 5)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								else
								{
									if (!游戏物品.检索表.TryGetValue("沙巴克回城卷", out var value14))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b27 = 0;
									byte b28 = 0;
									while (b27 < 背包大小 && b28 < 6)
									{
										if (!角色背包.ContainsKey(b27))
										{
											角色背包[b27] = new 物品数据(value14, 角色数据, 1, b27, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b27].字节描述()
											});
											b28 = (byte)(b28 + 1);
										}
										b27 = (byte)(b27 + 1);
									}
								}
								break;
							case "召唤骷髅":
								if (玩家学习技能(3003))
								{
									消耗背包物品(1, v);
								}
								break;
							case "超级金创药":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("强效金创药", out var value10))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b19 = 0;
									byte b20 = 0;
									while (b19 < 背包大小 && b20 < 6)
									{
										if (!角色背包.ContainsKey(b19))
										{
											角色背包[b19] = new 物品数据(value10, 角色数据, 1, b19, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b19].字节描述()
											});
											b20 = (byte)(b20 + 1);
										}
										b19 = (byte)(b19 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "横扫六合":
								if (玩家学习技能(1203))
								{
									消耗背包物品(1, v);
								}
								break;
							case "三发散射":
								if (玩家学习技能(2045))
								{
									消耗背包物品(1, v);
								}
								break;
							case "暴击术":
								if (玩家学习技能(1531))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔法药(小量)":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								药品回魔 = 主程.当前时间.AddSeconds(1.0);
								回魔基数 = 10;
								回魔次数 = 3;
								break;
							case "雷电术":
								if (玩家学习技能(2533))
								{
									消耗背包物品(1, v);
								}
								break;
							case "强效魔法药":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								药品回魔 = 主程.当前时间.AddSeconds(1.0);
								回魔基数 = 25;
								回魔次数 = 6;
								break;
							case "觉醒·魔刃天旋":
								if (玩家学习技能(1547))
								{
									消耗背包物品(1, v);
								}
								break;
							case "凝神":
								if (玩家学习技能(2051))
								{
									消耗背包物品(1, v);
								}
								break;
							case "凌云枪法":
								if (玩家学习技能(1210))
								{
									消耗背包物品(1, v);
								}
								break;
							case "追魂镖":
								if (玩家学习技能(1541))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·雷霆剑法":
								if (玩家学习技能(1049))
								{
									消耗背包物品(1, v);
								}
								break;
							case "神威镇域":
								if (玩家学习技能(1207))
								{
									消耗背包物品(1, v);
								}
								break;
							case "沙城每日宝箱":
							{
								byte b29 = byte.MaxValue;
								byte b30 = 0;
								while (b30 < 背包大小)
								{
									if (角色背包.ContainsKey(b30))
									{
										b30 = (byte)(b30 + 1);
										continue;
									}
									b29 = b30;
									break;
								}
								if (b29 != byte.MaxValue)
								{
									int num = 主程.随机数.Next(100);
									游戏物品 value18;
									if (num < 60)
									{
										消耗背包物品(1, v);
										双倍经验 += 500000;
									}
									else if (num < 80)
									{
										消耗背包物品(1, v);
										金币数量 += 100000;
									}
									else if (num >= 90)
									{
										游戏物品 value17;
										if (num < 95)
										{
											游戏物品 value15 = null;
											if (角色职业 != 0)
											{
												if (角色职业 != 游戏对象职业.法师)
												{
													if (角色职业 == 游戏对象职业.道士)
													{
														游戏物品.检索表.TryGetValue("道士铭文石", out value15);
													}
													else if (角色职业 != 游戏对象职业.刺客)
													{
														if (角色职业 == 游戏对象职业.弓手)
														{
															游戏物品.检索表.TryGetValue("弓手铭文石", out value15);
														}
														else if (角色职业 == 游戏对象职业.龙枪)
														{
															游戏物品.检索表.TryGetValue("龙枪铭文石", out value15);
														}
													}
													else
													{
														游戏物品.检索表.TryGetValue("刺客铭文石", out value15);
													}
												}
												else
												{
													游戏物品.检索表.TryGetValue("法师铭文石", out value15);
												}
											}
											else
											{
												游戏物品.检索表.TryGetValue("战士铭文石", out value15);
											}
											if (value15 != null)
											{
												消耗背包物品(1, v);
												角色背包[b29] = new 物品数据(value15, 角色数据, 背包类型, b29, 3);
												网络连接?.发送封包(new 玩家物品变动
												{
													物品描述 = 角色背包[b29].字节描述()
												});
											}
										}
										else if (num >= 98)
										{
											if (游戏物品.检索表.TryGetValue("沙城奖杯", out var value16))
											{
												消耗背包物品(1, v);
												角色背包[b29] = new 物品数据(value16, 角色数据, 背包类型, b29, 1);
												网络连接?.发送封包(new 玩家物品变动
												{
													物品描述 = 角色背包[b29].字节描述()
												});
											}
										}
										else if (游戏物品.检索表.TryGetValue("祝福油", out value17))
										{
											消耗背包物品(1, v);
											角色背包[b29] = new 物品数据(value17, 角色数据, 背包类型, b29, 2);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b29].字节描述()
											});
										}
									}
									else if (游戏物品.检索表.TryGetValue("元宝袋(小)", out value18))
									{
										消耗背包物品(1, v);
										角色背包[b29] = new 物品数据(value18, 角色数据, 背包类型, b29, 5);
										网络连接?.发送封包(new 玩家物品变动
										{
											物品描述 = 角色背包[b29].字节描述()
										});
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							}
							case "盟重回城石":
								消耗背包物品(1, v);
								玩家切换地图((当前地图.地图编号 == 147) ? 当前地图 : 地图处理网关.分配地图(147), 地图区域类型.复活区域);
								break;
							case "战术标记":
								if (玩家学习技能(2044))
								{
									消耗背包物品(1, v);
								}
								break;
							case "潜行术":
								if (玩家学习技能(1532))
								{
									消耗背包物品(1, v);
								}
								break;
							case "半月弯刀":
								if (玩家学习技能(1034))
								{
									消耗背包物品(1, v);
								}
								break;
							case "超级魔法药":
								if (背包大小 - 角色背包.Count < 5)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								else
								{
									if (!游戏物品.检索表.TryGetValue("强效魔法药", out var value11))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b21 = 0;
									byte b22 = 0;
									while (b21 < 背包大小 && b22 < 6)
									{
										if (!角色背包.ContainsKey(b21))
										{
											角色背包[b21] = new 物品数据(value11, 角色数据, 1, b21, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b21].字节描述()
											});
											b22 = (byte)(b22 + 1);
										}
										b21 = (byte)(b21 + 1);
									}
								}
								break;
							case "野蛮冲撞":
								if (玩家学习技能(1035))
								{
									消耗背包物品(1, v);
								}
								break;
							case "名俊铭文石礼包":
							{
								byte b17 = byte.MaxValue;
								byte b18 = 0;
								while (b18 < 背包大小)
								{
									if (角色背包.ContainsKey(b18))
									{
										b18 = (byte)(b18 + 1);
										continue;
									}
									b17 = b18;
									break;
								}
								if (b17 == byte.MaxValue)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
									break;
								}
								游戏物品 value9 = null;
								if (角色职业 != 0)
								{
									if (角色职业 != 游戏对象职业.法师)
									{
										if (角色职业 == 游戏对象职业.道士)
										{
											游戏物品.检索表.TryGetValue("道士铭文石", out value9);
										}
										else if (角色职业 != 游戏对象职业.刺客)
										{
											if (角色职业 == 游戏对象职业.弓手)
											{
												游戏物品.检索表.TryGetValue("弓手铭文石", out value9);
											}
											else if (角色职业 == 游戏对象职业.龙枪)
											{
												游戏物品.检索表.TryGetValue("龙枪铭文石", out value9);
											}
										}
										else
										{
											游戏物品.检索表.TryGetValue("刺客铭文石", out value9);
										}
									}
									else
									{
										游戏物品.检索表.TryGetValue("法师铭文石", out value9);
									}
								}
								else
								{
									游戏物品.检索表.TryGetValue("战士铭文石", out value9);
								}
								if (value9 != null)
								{
									消耗背包物品(1, v);
									角色背包[b17] = new 物品数据(value9, 角色数据, 背包类型, b17, 5);
									网络连接?.发送封包(new 玩家物品变动
									{
										物品描述 = 角色背包[b17].字节描述()
									});
								}
								break;
							}
							case "冷酷":
								if (玩家学习技能(1538))
								{
									消耗背包物品(1, v);
								}
								break;
							case "炎龙波":
								if (玩家学习技能(1535))
								{
									消耗背包物品(1, v);
								}
								break;
							case "火镰狂舞":
								if (玩家学习技能(1539))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔能闪":
								if (玩家学习技能(2554))
								{
									消耗背包物品(1, v);
								}
								break;
							case "霹雳弹":
								if (玩家学习技能(1542))
								{
									消耗背包物品(1, v);
								}
								break;
							case "名俊灵石宝盒":
							{
								byte b3 = byte.MaxValue;
								byte b4 = 0;
								while (b4 < 背包大小)
								{
									if (角色背包.ContainsKey(b4))
									{
										b4 = (byte)(b4 + 1);
										continue;
									}
									b3 = b4;
									break;
								}
								if (b3 != byte.MaxValue)
								{
									游戏物品 value2 = null;
									switch (主程.随机数.Next(8))
									{
									case 0:
										游戏物品.检索表.TryGetValue("驭朱灵石1级", out value2);
										break;
									case 1:
										游戏物品.检索表.TryGetValue("命朱灵石1级", out value2);
										break;
									case 2:
										游戏物品.检索表.TryGetValue("守阳灵石1级", out value2);
										break;
									case 3:
										游戏物品.检索表.TryGetValue("蔚蓝灵石1级", out value2);
										break;
									case 4:
										游戏物品.检索表.TryGetValue("精绿灵石1级", out value2);
										break;
									case 5:
										游戏物品.检索表.TryGetValue("纯紫灵石1级", out value2);
										break;
									case 6:
										游戏物品.检索表.TryGetValue("深灰灵石1级", out value2);
										break;
									case 7:
										游戏物品.检索表.TryGetValue("橙黄灵石1级", out value2);
										break;
									}
									if (value2 != null)
									{
										消耗背包物品(1, v);
										角色背包[b3] = new 物品数据(value2, 角色数据, 背包类型, b3, 1);
										网络连接?.发送封包(new 玩家物品变动
										{
											物品描述 = 角色背包[b3].字节描述()
										});
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							}
							case "比奇回城石":
								消耗背包物品(1, v);
								玩家切换地图((当前地图.地图编号 == 143) ? 当前地图 : 地图处理网关.分配地图(143), 地图区域类型.复活区域);
								break;
							case "镇魔古城回城卷包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("镇魔古城回城卷", out var value25))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b43 = 0;
									byte b44 = 0;
									while (b43 < 背包大小 && b44 < 6)
									{
										if (!角色背包.ContainsKey(b43))
										{
											角色背包[b43] = new 物品数据(value25, 角色数据, 1, b43, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b43].字节描述()
											});
											b44 = (byte)(b44 + 1);
										}
										b43 = (byte)(b43 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "集体隐身术":
								if (玩家学习技能(3014))
								{
									消耗背包物品(1, v);
								}
								break;
							case "钩镰枪法":
								if (玩家学习技能(1205))
								{
									消耗背包物品(1, v);
								}
								break;
							case "治愈术":
								if (玩家学习技能(3002))
								{
									消耗背包物品(1, v);
								}
								break;
							case "强效太阳水包":
								if (背包大小 - 角色背包.Count < 5)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								else
								{
									if (!游戏物品.检索表.TryGetValue("强效太阳水", out var value22))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b37 = 0;
									byte b38 = 0;
									while (b37 < 背包大小 && b38 < 6)
									{
										if (!角色背包.ContainsKey(b37))
										{
											角色背包[b37] = new 物品数据(value22, 角色数据, 1, b37, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b37].字节描述()
											});
											b38 = (byte)(b38 + 1);
										}
										b37 = (byte)(b37 + 1);
									}
								}
								break;
							case "比奇回城卷包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("比奇回城卷", out var value19))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b31 = 0;
									byte b32 = 0;
									while (b31 < 背包大小 && b32 < 6)
									{
										if (!角色背包.ContainsKey(b31))
										{
											角色背包[b31] = new 物品数据(value19, 角色数据, 1, b31, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b31].字节描述()
											});
											b32 = (byte)(b32 + 1);
										}
										b31 = (byte)(b31 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "金创药(中)包":
								if (背包大小 - 角色背包.Count < 5)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								else
								{
									if (!游戏物品.检索表.TryGetValue("金创药(中量)", out var value13))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b25 = 0;
									byte b26 = 0;
									while (b25 < 背包大小 && b26 < 6)
									{
										if (!角色背包.ContainsKey(b25))
										{
											角色背包[b25] = new 物品数据(value13, 角色数据, 1, b25, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b25].字节描述()
											});
											b26 = (byte)(b26 + 1);
										}
										b25 = (byte)(b25 + 1);
									}
								}
								break;
							case "流星火雨":
								if (玩家学习技能(2540))
								{
									消耗背包物品(1, v);
								}
								break;
							case "召唤龙驹":
								if (玩家学习技能(1212))
								{
									消耗背包物品(1, v);
								}
								break;
							case "召唤神兽":
								if (玩家学习技能(3008))
								{
									消耗背包物品(1, v);
								}
								break;
							case "爆炎剑诀":
								if (玩家学习技能(1042))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔法药(中量)":
								if (v.分组编号 > 0 && v.分组冷却 > 0)
								{
									冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.分组编号 | 0),
										冷却时间 = v.分组冷却
									});
								}
								if (v.冷却时间 > 0)
								{
									冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
									网络连接?.发送封包(new 添加技能冷却
									{
										冷却编号 = (v.物品编号 | 0x2000000),
										冷却时间 = v.冷却时间
									});
								}
								消耗背包物品(1, v);
								药品回魔 = 主程.当前时间.AddSeconds(1.0);
								回魔基数 = 16;
								回魔次数 = 5;
								break;
							case "元宝袋(大)":
								消耗背包物品(1, v);
								元宝数量 += 10000;
								break;
							case "疾光电影":
								if (玩家学习技能(2536))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·暗影守卫":
								if (玩家学习技能(1546))
								{
									消耗背包物品(1, v);
								}
								break;
							case "元宝袋(超)":
								消耗背包物品(1, v);
								元宝数量 += 100000;
								break;
							case "烈火剑法":
								if (玩家学习技能(1036))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·万箭穿心":
								if (玩家学习技能(2057))
								{
									消耗背包物品(1, v);
								}
								break;
							case "觉醒·元素星辰":
								if (玩家学习技能(2558))
								{
									消耗背包物品(1, v);
								}
								break;
							case "精准术":
								if (玩家学习技能(1534))
								{
									消耗背包物品(1, v);
								}
								break;
							case "攻杀剑术":
								if (玩家学习技能(1032))
								{
									消耗背包物品(1, v);
								}
								break;
							case "隐身术":
								if (玩家学习技能(3009))
								{
									消耗背包物品(1, v);
								}
								break;
							case "随机传送石(大)":
							case "随机传送石":
							{
								Point point = 当前地图.随机传送(当前坐标);
								if (!(point != default(Point)))
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 776
									});
								}
								else
								{
									消耗背包物品(1, v);
									玩家切换地图(当前地图, 地图区域类型.未知区域, point);
								}
								break;
							}
							case "火墙":
								if (玩家学习技能(2534))
								{
									消耗背包物品(1, v);
								}
								break;
							case "魔法药(中)包":
								if (背包大小 - 角色背包.Count >= 5)
								{
									if (!游戏物品.检索表.TryGetValue("魔法药(中量)", out var value7))
									{
										break;
									}
									if (v.分组编号 > 0 && v.分组冷却 > 0)
									{
										冷却记录[v.分组编号 | 0] = 主程.当前时间.AddMilliseconds(v.分组冷却);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.分组编号 | 0),
											冷却时间 = v.分组冷却
										});
									}
									if (v.冷却时间 > 0)
									{
										冷却记录[v.物品编号 | 0x2000000] = 主程.当前时间.AddMilliseconds(v.冷却时间);
										网络连接?.发送封包(new 添加技能冷却
										{
											冷却编号 = (v.物品编号 | 0x2000000),
											冷却时间 = v.冷却时间
										});
									}
									消耗背包物品(1, v);
									byte b13 = 0;
									byte b14 = 0;
									while (b13 < 背包大小 && b14 < 6)
									{
										if (!角色背包.ContainsKey(b13))
										{
											角色背包[b13] = new 物品数据(value7, 角色数据, 1, b13, 1);
											网络连接?.发送封包(new 玩家物品变动
											{
												物品描述 = 角色背包[b13].字节描述()
											});
											b14 = (byte)(b14 + 1);
										}
										b13 = (byte)(b13 + 1);
									}
								}
								else
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
								}
								break;
							case "精神力战法":
								if (玩家学习技能(3001))
								{
									消耗背包物品(1, v);
								}
								break;
							case "豪杰铭文石礼包":
							{
								byte b = byte.MaxValue;
								byte b2 = 0;
								while (b2 < 背包大小)
								{
									if (角色背包.ContainsKey(b2))
									{
										b2 = (byte)(b2 + 1);
										continue;
									}
									b = b2;
									break;
								}
								if (b == byte.MaxValue)
								{
									网络连接?.发送封包(new 游戏错误提示
									{
										错误代码 = 1793
									});
									break;
								}
								游戏物品 value = null;
								if (角色职业 != 0)
								{
									if (角色职业 == 游戏对象职业.法师)
									{
										游戏物品.检索表.TryGetValue("法师铭文石", out value);
									}
									else if (角色职业 != 游戏对象职业.道士)
									{
										if (角色职业 == 游戏对象职业.刺客)
										{
											游戏物品.检索表.TryGetValue("刺客铭文石", out value);
										}
										else if (角色职业 == 游戏对象职业.弓手)
										{
											游戏物品.检索表.TryGetValue("弓手铭文石", out value);
										}
										else if (角色职业 == 游戏对象职业.龙枪)
										{
											游戏物品.检索表.TryGetValue("龙枪铭文石", out value);
										}
									}
									else
									{
										游戏物品.检索表.TryGetValue("道士铭文石", out value);
									}
								}
								else
								{
									游戏物品.检索表.TryGetValue("战士铭文石", out value);
								}
								if (value != null)
								{
									消耗背包物品(1, v);
									角色背包[b] = new 物品数据(value, 角色数据, 背包类型, b, 10);
									网络连接?.发送封包(new 玩家物品变动
									{
										物品描述 = 角色背包[b].字节描述()
									});
								}
								break;
							}
							case "觉醒·阴阳道盾":
								if (玩家学习技能(3025))
								{
									消耗背包物品(1, v);
								}
								break;
							case "元宝袋(中)":
								消耗背包物品(1, v);
								元宝数量 += 1000;
								break;
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1825
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1825
						});
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 玩家使用物品.  错误: 职业无法使用."));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家使用物品.  错误: 性别无法使用."));
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1877
			});
		}
	}

	public void 玩家喝修复油(byte 背包类型, byte 物品位置)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1877
			});
			return;
		}
		装备数据 装备数据 = null;
		if (背包类型 == 0 && 角色装备.TryGetValue(物品位置, out var v))
		{
			装备数据 = v;
		}
		if (背包类型 == 1 && 角色背包.TryGetValue(物品位置, out var v2) && v2 is 装备数据 装备数据2)
		{
			装备数据 = 装备数据2;
		}
		if (装备数据 == null)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1802
			});
		}
		else if (装备数据.能否修理)
		{
			物品数据 物品;
			if (装备数据.最大持久.V >= 装备数据.默认持久 * 2)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1953
				});
			}
			else if (查找背包物品(110012, out 物品))
			{
				消耗背包物品(1, 物品);
				if (计算类.计算概率(1f - (float)装备数据.最大持久.V * 0.5f / (float)装备数据.默认持久))
				{
					装备数据.最大持久.V += 1000;
					网络连接?.发送封包(new 修复最大持久
					{
						修复失败 = false
					});
					网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = 装备数据.字节描述()
					});
				}
				else
				{
					网络连接?.发送封包(new 修复最大持久
					{
						修复失败 = true
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1802
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1814
			});
		}
	}

	public void 玩家合成物品()
	{
		if (!对象死亡 && 摆摊状态 <= 0)
		{
			_ = 交易状态;
		}
	}

	public void 玩家出售物品(byte 背包类型, byte 物品位置, ushort 出售数量)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3 && 对话守卫 != null && 当前地图 == 对话守卫.当前地图 && 网格距离(对话守卫) <= 12 && 打开商店 != 0 && 出售数量 > 0 && 游戏商店.数据表.TryGetValue(打开商店, out var value))
		{
			物品数据 v = null;
			if (背包类型 == 1)
			{
				角色背包.TryGetValue(物品位置, out v);
			}
			if (v != null && !v.是否绑定 && v.出售类型 != 0 && value.回收类型 == v.出售类型)
			{
				角色背包.Remove(物品位置);
				value.出售物品(v);
				金币数量 += v.出售价格;
				网络连接?.发送封包(new 删除玩家物品
				{
					背包类型 = 背包类型,
					物品位置 = 物品位置
				});
			}
		}
	}

	public void 玩家购买物品(int 商店编号, int 物品位置, ushort 购入数量)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3 || 对话守卫 == null || 当前地图 != 对话守卫.当前地图 || 网格距离(对话守卫) > 12 || 打开商店 == 0 || 购入数量 <= 0 || 打开商店 != 商店编号 || !游戏商店.数据表.TryGetValue(打开商店, out var value) || value.商品列表.Count <= 物品位置 || !游戏物品.数据表.TryGetValue(value.商品列表[物品位置].商品编号, out var value2))
		{
			return;
		}
		int num = ((购入数量 == 1 || value2.持久类型 != 物品持久分类.堆叠) ? 1 : Math.Min(购入数量, value2.物品持久));
		游戏商品 游戏商品 = value.商品列表[物品位置];
		int num2 = -1;
		byte b = 0;
		while (b < 背包大小)
		{
			if (角色背包.TryGetValue(b, out var v) && (value2.持久类型 != 物品持久分类.堆叠 || value2.物品编号 != v.物品编号 || v.当前持久.V + 购入数量 > value2.物品持久))
			{
				b = (byte)(b + 1);
				continue;
			}
			num2 = b;
			break;
		}
		if (num2 != -1)
		{
			int num3 = 游戏商品.商品价格 * num;
			if (游戏商品.货币类型 > 19)
			{
				if (!查找背包物品(num3, 游戏商品.货币类型, out var 物品列表))
				{
					return;
				}
				消耗背包物品(num3, 物品列表);
			}
			else
			{
				if (!Enum.TryParse<游戏货币>(游戏商品.货币类型.ToString(), out var result) || !Enum.IsDefined(typeof(游戏货币), result))
				{
					return;
				}
				if (result == 游戏货币.名师声望 || result == 游戏货币.道义点数)
				{
					num3 *= 1000;
				}
				switch (result)
				{
				case 游戏货币.元宝:
					if (元宝数量 >= num3)
					{
						元宝数量 -= num3;
						break;
					}
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 13057
					});
					return;
				case 游戏货币.名师声望:
					if (师门声望 < num3)
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 13057
						});
						return;
					}
					师门声望 -= num3;
					break;
				default:
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 13057
					});
					return;
				case 游戏货币.金币:
					if (金币数量 >= num3)
					{
						金币数量 -= num3;
						break;
					}
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 13057
					});
					return;
				}
				网络连接?.发送封包(new 货币数量变动
				{
					货币类型 = (byte)游戏商品.货币类型,
					货币数量 = 角色数据.角色货币[result]
				});
			}
			if (角色背包.TryGetValue((byte)num2, out var v2))
			{
				v2.当前持久.V += num;
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = v2.字节描述()
				});
				return;
			}
			if (!(value2 is 游戏装备 模板))
			{
				int 持久 = 0;
				switch (value2.持久类型)
				{
				case 物品持久分类.堆叠:
					持久 = num;
					break;
				case 物品持久分类.容器:
					持久 = 0;
					break;
				case 物品持久分类.消耗:
				case 物品持久分类.纯度:
					持久 = value2.物品持久;
					break;
				}
				角色背包[(byte)num2] = new 物品数据(value2, 角色数据, 1, (byte)num2, 持久);
			}
			else
			{
				角色背包[(byte)num2] = new 装备数据(模板, 角色数据, 1, (byte)num2);
			}
			网络连接?.发送封包(new 玩家物品变动
			{
				物品描述 = 角色背包[(byte)num2].字节描述()
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1793
			});
		}
	}

	public void 玩家回购物品(byte 物品位置)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3 || 对话守卫 == null || 当前地图 != 对话守卫.当前地图 || 网格距离(对话守卫) > 12 || 打开商店 == 0 || !游戏商店.数据表.TryGetValue(打开商店, out var value) || 回购清单.Count <= 物品位置)
		{
			return;
		}
		物品数据 物品数据 = 回购清单[物品位置];
		int num = -1;
		byte b = 0;
		while (b < 背包大小)
		{
			if (角色背包.TryGetValue(b, out var v) && (!物品数据.能否堆叠 || 物品数据.物品编号 != v.物品编号 || v.当前持久.V + 物品数据.当前持久.V > v.最大持久.V))
			{
				b = (byte)(b + 1);
				continue;
			}
			num = b;
			break;
		}
		if (num != -1)
		{
			if (金币数量 < 物品数据.出售价格)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1821
				});
				return;
			}
			if (!value.回购物品(物品数据))
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 12807
				});
				return;
			}
			金币数量 -= 物品数据.出售价格;
			if (!角色背包.TryGetValue((byte)num, out var v2))
			{
				角色背包[(byte)num] = 物品数据;
				物品数据.物品位置.V = (byte)num;
				物品数据.物品容器.V = 1;
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = 角色背包[(byte)num].字节描述()
				});
			}
			else
			{
				v2.当前持久.V += 物品数据.当前持久.V;
				value.回购物品(物品数据);
				物品数据.删除数据();
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = v2.字节描述()
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1793
			});
		}
	}

	public void 请求回购清单()
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3 || 对话守卫 == null || 当前地图 != 对话守卫.当前地图 || 网格距离(对话守卫) > 12 || 打开商店 == 0 || !游戏商店.数据表.TryGetValue(打开商店, out var value))
		{
			return;
		}
		回购清单 = value.回购列表.ToList();
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)回购清单.Count);
		foreach (物品数据 item in 回购清单)
		{
			binaryWriter.Write(item.字节描述());
		}
		网络连接?.发送封包(new 同步回购列表
		{
			字节描述 = memoryStream.ToArray()
		});
	}

	public void 玩家镶嵌灵石(byte 装备类型, byte 装备位置, byte 装备孔位, byte 灵石类型, byte 灵石位置)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (!(打开界面 != "SoulEmbed"))
		{
			物品数据 v;
			if (装备类型 != 1 || 灵石类型 != 1)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 不是角色背包"));
			}
			else if (角色背包.TryGetValue(装备位置, out v) && v is 装备数据 装备数据)
			{
				if (!角色背包.TryGetValue(灵石位置, out var v2))
				{
					网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 没有找到灵石"));
				}
				else if (装备数据.孔洞颜色.Count > 装备孔位)
				{
					if (装备数据.镶嵌灵石.ContainsKey(装备孔位))
					{
						网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 已有镶嵌灵石"));
					}
					else if ((装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.绿色 || v2.物品名字.IndexOf("精绿灵石") != -1) && (装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.黄色 || v2.物品名字.IndexOf("守阳灵石") != -1) && (装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.蓝色 || v2.物品名字.IndexOf("蔚蓝灵石") != -1) && (装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.紫色 || v2.物品名字.IndexOf("纯紫灵石") != -1) && (装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.灰色 || v2.物品名字.IndexOf("深灰灵石") != -1) && (装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.橙色 || v2.物品名字.IndexOf("橙黄灵石") != -1) && (装备数据.孔洞颜色[装备孔位] != 装备孔洞颜色.红色 || v2.物品名字.IndexOf("驭朱灵石") != -1 || v2.物品名字.IndexOf("命朱灵石") != -1))
					{
						消耗背包物品(1, v2);
						装备数据.镶嵌灵石[装备孔位] = v2.物品模板;
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 装备数据.字节描述()
						});
						网络连接?.发送封包(new 成功镶嵌灵石
						{
							灵石状态 = 1
						});
					}
					else
					{
						网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 指定灵石错误"));
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 装备孔位错误"));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 没有找到装备"));
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 没有打开界面"));
		}
	}

	public void 玩家拆除灵石(byte 装备类型, byte 装备位置, byte 装备孔位)
	{
		int num = 0;
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (!(打开界面 != "SoulEmbed"))
		{
			if (装备类型 != 1)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 不是角色背包"));
			}
			else if (背包剩余 > 0)
			{
				if (角色背包.TryGetValue(装备位置, out var v) && v is 装备数据 装备数据)
				{
					if (装备数据.镶嵌灵石.TryGetValue(装备孔位, out var v2))
					{
						if (v2.物品名字.IndexOf("1级") > 0)
						{
							int num2 = 金币数量;
							num = 100000;
							if (num2 < 100000)
							{
								goto IL_0291;
							}
						}
						if (v2.物品名字.IndexOf("2级") > 0)
						{
							int num3 = 金币数量;
							num = 500000;
							if (num3 < 500000)
							{
								goto IL_0291;
							}
						}
						if (v2.物品名字.IndexOf("3级") > 0)
						{
							int num4 = 金币数量;
							num = 2500000;
							if (num4 < 2500000)
							{
								goto IL_0291;
							}
						}
						if (v2.物品名字.IndexOf("4级") > 0)
						{
							int num5 = 金币数量;
							num = 10000000;
							if (num5 < 10000000)
							{
								goto IL_0291;
							}
						}
						if (v2.物品名字.IndexOf("5级") > 0)
						{
							int num6 = 金币数量;
							num = 25000000;
							if (num6 < 25000000)
							{
								goto IL_0291;
							}
						}
						byte b = 0;
						while (b < 背包大小)
						{
							if (角色背包.ContainsKey(b))
							{
								b = (byte)(b + 1);
								continue;
							}
							金币数量 -= num;
							装备数据.镶嵌灵石.Remove(装备孔位);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 装备数据.字节描述()
							});
							角色背包[b] = new 物品数据(v2, 角色数据, 1, b, 1);
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 角色背包[b].字节描述()
							});
							网络连接?.发送封包(new 成功取下灵石
							{
								灵石状态 = 1
							});
							break;
						}
					}
					else
					{
						网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 没有镶嵌灵石"));
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 没有找到装备"));
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1793
				});
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 玩家镶嵌灵石.  错误: 没有打开界面"));
		}
		return;
		IL_0291:
		网络连接?.发送封包(new 游戏错误提示
		{
			错误代码 = 1821
		});
	}

	public void 普通铭文洗练(byte 装备类型, byte 装备位置, int 物品编号)
	{
		装备数据 装备数据 = null;
		if (装备类型 == 0 && 角色装备.TryGetValue(装备位置, out var v))
		{
			装备数据 = v;
		}
		if (装备类型 == 1 && 角色背包.TryGetValue(装备位置, out var v2) && v2 is 装备数据 装备数据2)
		{
			装备数据 = 装备数据2;
		}
		if (!(打开界面 != "WeaponRune"))
		{
			if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
			{
				return;
			}
			if (金币数量 >= 10000)
			{
				if (装备数据 == null)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1802
					});
					return;
				}
				if (装备数据.物品类型 != 物品使用分类.武器)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 物品类型错误."));
					return;
				}
				if (物品编号 <= 0)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 材料编号错误."));
					return;
				}
				if (!查找背包物品(物品编号, out var 物品))
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1835
					});
					return;
				}
				if (物品.物品类型 != 物品使用分类.普通铭文)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 材料类型错误."));
					return;
				}
				金币数量 -= 10000;
				消耗背包物品(1, 物品);
				byte 洗练职业 = 0;
				switch (物品编号)
				{
				case 21001:
					洗练职业 = 0;
					break;
				case 21002:
					洗练职业 = 1;
					break;
				case 21003:
					洗练职业 = 4;
					break;
				case 21004:
					洗练职业 = 2;
					break;
				case 21005:
					洗练职业 = 3;
					break;
				case 21006:
					洗练职业 = 5;
					break;
				}
				if (装备数据.第一铭文 == null)
				{
					装备数据.第一铭文 = 铭文技能.随机洗练(洗练职业);
					玩家装卸铭文(装备数据.第一铭文.技能编号, 装备数据.第一铭文.铭文编号);
					if (装备数据.第一铭文.广播通知)
					{
						网络服务网关.发送公告("恭喜[" + 对象名字 + "]在铭文洗炼中获得稀有铭文[" + 装备数据.第一铭文.技能名字.Split('-').Last() + "]");
					}
				}
				else if (装备数据.传承材料 != 0 && (装备数据.双铭文点 += 主程.随机数.Next(1, 6)) >= 1200 && 装备数据.第二铭文 == null)
				{
					铭文技能 铭文技能3;
					do
					{
						铭文技能 铭文技能2 = (装备数据.第二铭文 = 铭文技能.随机洗练(洗练职业));
						铭文技能3 = 铭文技能2;
					}
					while (铭文技能3.技能编号 == 装备数据.第一铭文?.技能编号);
					玩家装卸铭文(装备数据.第二铭文.技能编号, 装备数据.第二铭文.铭文编号);
					if (装备数据.第二铭文.广播通知)
					{
						网络服务网关.发送公告("恭喜[" + 对象名字 + "]在铭文洗炼中获得稀有铭文[" + 装备数据.第二铭文.技能名字.Split('-').Last() + "]");
					}
				}
				else
				{
					if (装备类型 == 0)
					{
						玩家装卸铭文(装备数据.第一铭文.技能编号, 0);
					}
					铭文技能 铭文技能5;
					do
					{
						铭文技能 铭文技能2 = (装备数据.第一铭文 = 铭文技能.随机洗练(洗练职业));
						铭文技能5 = 铭文技能2;
					}
					while (铭文技能5.技能编号 == 装备数据.第二铭文?.技能编号);
					if (装备类型 == 0)
					{
						玩家装卸铭文(装备数据.第一铭文.技能编号, 装备数据.第一铭文.铭文编号);
					}
					if (装备数据.第一铭文.广播通知)
					{
						网络服务网关.发送公告("恭喜[" + 对象名字 + "]在铭文洗炼中获得稀有铭文[" + 装备数据.第一铭文.技能名字.Split('-').Last() + "]");
					}
				}
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = 装备数据.字节描述()
				});
				客户网络 客户网络 = 网络连接;
				if (客户网络 != null)
				{
					玩家普通洗练 玩家普通洗练 = new 玩家普通洗练();
					玩家普通洗练.铭文位一 = 装备数据.第一铭文?.铭文索引 ?? 0;
					玩家普通洗练.铭文位二 = 装备数据.第二铭文?.铭文索引 ?? 0;
					客户网络.发送封包(玩家普通洗练);
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1821
				});
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 没有打开界面"));
		}
	}

	public void 高级铭文洗练(byte 装备类型, byte 装备位置, int 物品编号)
	{
		装备数据 装备数据 = null;
		if (装备类型 == 0 && 角色装备.TryGetValue(装备位置, out var v))
		{
			装备数据 = v;
		}
		if (装备类型 == 1 && 角色背包.TryGetValue(装备位置, out var v2) && v2 is 装备数据 装备数据2)
		{
			装备数据 = 装备数据2;
		}
		if (!(打开界面 != "WeaponRune"))
		{
			if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
			{
				return;
			}
			if (金币数量 >= 100000)
			{
				if (装备数据 == null)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1802
					});
				}
				else if (装备数据.物品类型 != 物品使用分类.武器)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 物品类型错误."));
				}
				else if (装备数据.第二铭文 != null)
				{
					if (物品编号 <= 0)
					{
						网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 材料编号错误."));
						return;
					}
					if (!查找背包物品(物品编号, out var 物品))
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1835
						});
						return;
					}
					if (物品.物品类型 != 物品使用分类.普通铭文)
					{
						网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 材料类型错误."));
						return;
					}
					金币数量 -= 100000;
					消耗背包物品(1, 物品);
					byte 洗练职业 = 0;
					switch (物品编号)
					{
					case 21001:
						洗练职业 = 0;
						break;
					case 21002:
						洗练职业 = 1;
						break;
					case 21003:
						洗练职业 = 4;
						break;
					case 21004:
						洗练职业 = 2;
						break;
					case 21005:
						洗练职业 = 3;
						break;
					case 21006:
						洗练职业 = 5;
						break;
					}
					while ((洗练铭文 = 铭文技能.随机洗练(洗练职业)).技能编号 == 装备数据.最优铭文.技能编号)
					{
					}
					if (装备数据.最优铭文 == 装备数据.第一铭文)
					{
						网络连接?.发送封包(new 玩家高级洗练
						{
							洗练结果 = 1,
							铭文位一 = 装备数据.最优铭文.铭文索引,
							铭文位二 = 洗练铭文.铭文索引
						});
					}
					else
					{
						网络连接?.发送封包(new 玩家高级洗练
						{
							洗练结果 = 1,
							铭文位一 = 洗练铭文.铭文索引,
							铭文位二 = 装备数据.最优铭文.铭文索引
						});
					}
					if (洗练铭文.广播通知)
					{
						网络服务网关.发送公告("恭喜[" + 对象名字 + "]在铭文洗炼中获得稀有铭文[" + 洗练铭文.技能名字.Split('-').Last() + "]");
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 第二铭文为空."));
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1821
				});
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 没有打开界面"));
		}
	}

	public void 替换铭文洗练(byte 装备类型, byte 装备位置, int 物品编号)
	{
		装备数据 装备数据 = null;
		int num = 10;
		if (装备类型 == 0 && 角色装备.TryGetValue(装备位置, out var v))
		{
			装备数据 = v;
		}
		if (装备类型 == 1 && 角色背包.TryGetValue(装备位置, out var v2) && v2 is 装备数据 装备数据2)
		{
			装备数据 = 装备数据2;
		}
		if (打开界面 != "WeaponRune")
		{
			网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 没有打开界面"));
		}
		else
		{
			if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
			{
				return;
			}
			if (金币数量 < 1000000)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1821
				});
			}
			else if (装备数据 != null)
			{
				if (装备数据.物品类型 == 物品使用分类.武器)
				{
					if (装备数据.第二铭文 == null)
					{
						网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 第二铭文为空."));
					}
					else if (物品编号 > 0)
					{
						if (!查找背包物品(num, 物品编号, out var 物品列表))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1835
							});
							return;
						}
						if (物品列表.FirstOrDefault((物品数据 O) => O.物品类型 != 物品使用分类.普通铭文) != null)
						{
							网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 材料类型错误."));
							return;
						}
						金币数量 -= 1000000;
						消耗背包物品(num, 物品列表);
						byte 洗练职业 = 0;
						switch (物品编号)
						{
						case 21001:
							洗练职业 = 0;
							break;
						case 21002:
							洗练职业 = 1;
							break;
						case 21003:
							洗练职业 = 4;
							break;
						case 21004:
							洗练职业 = 2;
							break;
						case 21005:
							洗练职业 = 3;
							break;
						case 21006:
							洗练职业 = 5;
							break;
						}
						while ((洗练铭文 = 铭文技能.随机洗练(洗练职业)).技能编号 == 装备数据.最差铭文.技能编号)
						{
						}
						网络连接?.发送封包(new 玩家高级洗练
						{
							洗练结果 = 1,
							铭文位一 = 装备数据.最差铭文.铭文索引,
							铭文位二 = 洗练铭文.铭文索引
						});
						if (洗练铭文.广播通知)
						{
							网络服务网关.发送公告("恭喜[" + 对象名字 + "]在铭文洗炼中获得稀有铭文[" + 洗练铭文.技能名字.Split('-').Last() + "]");
						}
					}
					else
					{
						网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 材料编号错误."));
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 物品类型错误."));
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1802
				});
			}
		}
	}

	public void 高级洗练确认(byte 装备类型, byte 装备位置)
	{
		装备数据 装备数据 = null;
		if (装备类型 == 0 && 角色装备.TryGetValue(装备位置, out var v))
		{
			装备数据 = v;
		}
		if (装备类型 == 1 && 角色背包.TryGetValue(装备位置, out var v2) && v2 is 装备数据 装备数据2)
		{
			装备数据 = 装备数据2;
		}
		if (打开界面 != "WeaponRune")
		{
			网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 没有打开界面"));
		}
		else if (装备数据 == null)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1802
			});
		}
		else if (洗练铭文 != null)
		{
			if (装备数据.物品类型 != 物品使用分类.武器)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 物品类型错误."));
				return;
			}
			if (装备数据.第二铭文 == null)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 第二铭文为空."));
				return;
			}
			if (装备类型 == 0)
			{
				玩家装卸铭文(装备数据.最差铭文.技能编号, 0);
			}
			装备数据.最差铭文 = 洗练铭文;
			if (装备类型 == 0)
			{
				玩家装卸铭文(洗练铭文.技能编号, 洗练铭文.铭文编号);
			}
			网络连接?.发送封包(new 玩家物品变动
			{
				物品描述 = 装备数据.字节描述()
			});
			网络连接?.发送封包(new 确认替换铭文
			{
				确定替换 = true
			});
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 确定替换铭文.  错误: 没有没有洗练记录."));
		}
	}

	public void 替换洗练确认(byte 装备类型, byte 装备位置)
	{
		装备数据 装备数据 = null;
		if (装备类型 == 0 && 角色装备.TryGetValue(装备位置, out var v))
		{
			装备数据 = v;
		}
		if (装备类型 == 1 && 角色背包.TryGetValue(装备位置, out var v2) && v2 is 装备数据 装备数据2)
		{
			装备数据 = 装备数据2;
		}
		if (打开界面 != "WeaponRune")
		{
			网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 没有打开界面"));
		}
		else if (装备数据 == null)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1802
			});
		}
		else if (洗练铭文 != null)
		{
			if (装备数据.物品类型 == 物品使用分类.武器)
			{
				if (装备数据.第二铭文 != null)
				{
					if (装备类型 == 0)
					{
						玩家装卸铭文(装备数据.最优铭文.技能编号, 0);
					}
					装备数据.最优铭文 = 洗练铭文;
					if (装备类型 == 0)
					{
						玩家装卸铭文(洗练铭文.技能编号, 洗练铭文.铭文编号);
					}
					网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = 装备数据.字节描述()
					});
					网络连接?.发送封包(new 确认替换铭文
					{
						确定替换 = true
					});
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 第二铭文为空."));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 普通铭文洗练.  错误: 物品类型错误."));
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 确定替换铭文.  错误: 没有没有洗练记录."));
		}
	}

	public void 放弃替换铭文()
	{
		洗练铭文 = null;
		网络连接?.发送封包(new 确认替换铭文
		{
			确定替换 = false
		});
	}

	public void 解锁双铭文位(byte 装备类型, byte 装备位置, byte 操作参数)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (打开界面 != "WeaponRune")
		{
			网络连接.尝试断开连接(new Exception("错误操作: 解锁双铭文位.  错误: 没有打开界面"));
		}
		else if (装备类型 == 1)
		{
			if (角色背包.TryGetValue(装备位置, out var v) && v is 装备数据 装备数据)
			{
				if (装备数据.物品类型 == 物品使用分类.武器)
				{
					if (操作参数 != 1)
					{
						return;
					}
					int num = 2000000;
					if (!装备数据.双铭文栏.V)
					{
						if (金币数量 >= num)
						{
							金币数量 -= num;
							装备数据.双铭文栏.V = true;
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 装备数据.字节描述()
							});
							客户网络 客户网络 = 网络连接;
							if (客户网络 != null)
							{
								双铭文位切换 双铭文位切换 = new 双铭文位切换
								{
									当前栏位 = 装备数据.当前铭栏.V
								};
								双铭文位切换.第一铭文 = 装备数据.第一铭文?.铭文索引 ?? 0;
								双铭文位切换.第二铭文 = 装备数据.第二铭文?.铭文索引 ?? 0;
								客户网络.发送封包(双铭文位切换);
							}
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1821
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1909
						});
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 解锁双铭文位.  错误: 物品类型错误."));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 解锁双铭文位.  错误: 不是装备类型."));
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1839
			});
		}
	}

	public void 切换双铭文位(byte 装备类型, byte 装备位置, byte 操作参数)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		if (!(打开界面 != "WeaponRune"))
		{
			物品数据 v;
			if (装备类型 != 1)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1839
				});
			}
			else if (角色背包.TryGetValue(装备位置, out v) && v is 装备数据 装备数据)
			{
				if (装备数据.物品类型 != 物品使用分类.武器)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 切换双铭文位.  错误: 物品类型错误."));
				}
				else if (装备数据.双铭文栏.V)
				{
					if (操作参数 == 装备数据.当前铭栏.V)
					{
						网络连接.尝试断开连接(new Exception("错误操作: 切换双铭文位.  错误: 切换铭位错误."));
						return;
					}
					装备数据.当前铭栏.V = 操作参数;
					网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = 装备数据.字节描述()
					});
					客户网络 客户网络 = 网络连接;
					if (客户网络 != null)
					{
						双铭文位切换 双铭文位切换 = new 双铭文位切换
						{
							当前栏位 = 装备数据.当前铭栏.V
						};
						双铭文位切换.第一铭文 = 装备数据.第一铭文?.铭文索引 ?? 0;
						双铭文位切换.第二铭文 = 装备数据.第二铭文?.铭文索引 ?? 0;
						客户网络.发送封包(双铭文位切换);
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1926
					});
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 切换双铭文位.  错误: 不是装备类型."));
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 切换双铭文位.  错误: 没有打开界面"));
		}
	}

	public void 传承武器铭文(byte 来源类型, byte 来源位置, byte 目标类型, byte 目标位置)
	{
		int num = 1000000;
		int num2 = 150;
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		物品数据 v;
		物品数据 v2;
		if (打开界面 != "WeaponRune")
		{
			网络连接.尝试断开连接(new Exception("错误操作: 传承武器铭文.  错误: 没有打开界面"));
		}
		else if (来源类型 != 1 || 目标类型 != 1)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1839
			});
		}
		else if (角色背包.TryGetValue(来源位置, out v) && v is 装备数据 装备数据 && 角色背包.TryGetValue(目标位置, out v2) && v2 is 装备数据 装备数据2)
		{
			if (装备数据.物品类型 == 物品使用分类.武器 && 装备数据2.物品类型 == 物品使用分类.武器)
			{
				if (装备数据.传承材料 != 0 && 装备数据2.传承材料 != 0 && 装备数据.传承材料 == 装备数据2.传承材料)
				{
					if (装备数据.第二铭文 != null && 装备数据2.第二铭文 != null)
					{
						if (金币数量 >= num)
						{
							if (!查找背包物品(num2, 装备数据.传承材料, out var 物品列表))
							{
								网络连接?.发送封包(new 游戏错误提示
								{
									错误代码 = 1835
								});
								return;
							}
							金币数量 -= num;
							消耗背包物品(num2, 物品列表);
							装备数据2.第一铭文 = 装备数据.第一铭文;
							装备数据2.第二铭文 = 装备数据.第二铭文;
							装备数据.铭文技能.Remove((byte)(装备数据.当前铭栏.V * 2));
							装备数据.铭文技能.Remove((byte)(装备数据.当前铭栏.V * 2 + 1));
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 装备数据.字节描述()
							});
							网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = 装备数据2.字节描述()
							});
							网络连接?.发送封包(new 铭文传承应答());
						}
						else
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1821
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1887
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1887
					});
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 传承武器铭文.  错误: 物品类型错误."));
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 传承武器铭文.  错误: 不是装备类型."));
		}
	}

	public void 升级武器普通(byte[] 首饰组, byte[] 材料组)
	{
		if (对象死亡 || 摆摊状态 > 0 || 交易状态 >= 3)
		{
			return;
		}
		装备数据 v;
		if (角色数据.升级装备.V != null)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1854
			});
		}
		else if (金币数量 < 10000)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1821
			});
		}
		else if (角色装备.TryGetValue(0, out v))
		{
			if (v.最大持久.V > 3000 && (float)v.最大持久.V > (float)v.默认持久 * 0.5f)
			{
				if (v.升级次数.V >= 9)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 1815
					});
					return;
				}
				Dictionary<byte, 装备数据> dictionary = new Dictionary<byte, 装备数据>();
				byte[] array = 首饰组;
				foreach (byte b in array)
				{
					if (b != byte.MaxValue)
					{
						if (!角色背包.TryGetValue(b, out var v2))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1859
							});
							return;
						}
						if (!(v2 is 装备数据 装备数据) || (装备数据.物品类型 != 物品使用分类.项链 && 装备数据.物品类型 != 物品使用分类.手镯 && 装备数据.物品类型 != 物品使用分类.戒指))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1859
							});
							return;
						}
						if (dictionary.ContainsKey(b))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1859
							});
							return;
						}
						dictionary.Add(b, 装备数据);
					}
				}
				Dictionary<byte, 物品数据> dictionary2 = new Dictionary<byte, 物品数据>();
				array = 材料组;
				foreach (byte b2 in array)
				{
					if (b2 != byte.MaxValue)
					{
						if (!角色背包.TryGetValue(b2, out var v3))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1859
							});
							return;
						}
						if (v3.物品类型 != 物品使用分类.武器锻造)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1859
							});
							return;
						}
						if (dictionary2.ContainsKey(b2))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 1859
							});
							return;
						}
						dictionary2.Add(b2, v3);
					}
				}
				金币数量 -= 10000;
				array = 首饰组;
				foreach (byte b3 in array)
				{
					if (b3 != byte.MaxValue)
					{
						角色背包[b3].删除数据();
						角色背包.Remove(b3);
						网络连接?.发送封包(new 删除玩家物品
						{
							背包类型 = 1,
							物品位置 = b3
						});
					}
				}
				array = 材料组;
				foreach (byte b4 in array)
				{
					if (b4 != byte.MaxValue)
					{
						角色背包[b4].删除数据();
						角色背包.Remove(b4);
						网络连接?.发送封包(new 删除玩家物品
						{
							背包类型 = 1,
							物品位置 = b4
						});
					}
				}
				角色装备.Remove(0);
				玩家穿卸装备(装备穿戴部位.武器, v, null);
				网络连接?.发送封包(new 删除玩家物品
				{
					背包类型 = 0,
					物品位置 = 0
				});
				网络连接?.发送封包(new 放入升级武器());
				Dictionary<byte, Dictionary<装备数据, int>> dictionary3 = new Dictionary<byte, Dictionary<装备数据, int>>
				{
					[0] = new Dictionary<装备数据, int>(),
					[1] = new Dictionary<装备数据, int>(),
					[2] = new Dictionary<装备数据, int>(),
					[3] = new Dictionary<装备数据, int>(),
					[4] = new Dictionary<装备数据, int>()
				};
				foreach (装备数据 value in dictionary.Values)
				{
					Dictionary<游戏对象属性, int> 装备属性 = value.装备属性;
					int num = 0;
					if ((num = (装备属性.ContainsKey(游戏对象属性.最小攻击) ? 装备属性[游戏对象属性.最小攻击] : 0) + (装备属性.ContainsKey(游戏对象属性.最大攻击) ? 装备属性[游戏对象属性.最大攻击] : 0)) > 0)
					{
						dictionary3[0][value] = num;
					}
					if ((num = (装备属性.ContainsKey(游戏对象属性.最小魔法) ? 装备属性[游戏对象属性.最小魔法] : 0) + (装备属性.ContainsKey(游戏对象属性.最大魔法) ? 装备属性[游戏对象属性.最大魔法] : 0)) > 0)
					{
						dictionary3[1][value] = num;
					}
					if ((num = (装备属性.ContainsKey(游戏对象属性.最小道术) ? 装备属性[游戏对象属性.最小道术] : 0) + (装备属性.ContainsKey(游戏对象属性.最大道术) ? 装备属性[游戏对象属性.最大道术] : 0)) > 0)
					{
						dictionary3[2][value] = num;
					}
					if ((num = (装备属性.ContainsKey(游戏对象属性.最小刺术) ? 装备属性[游戏对象属性.最小刺术] : 0) + (装备属性.ContainsKey(游戏对象属性.最大刺术) ? 装备属性[游戏对象属性.最大刺术] : 0)) > 0)
					{
						dictionary3[3][value] = num;
					}
					if ((num = (装备属性.ContainsKey(游戏对象属性.最小弓术) ? 装备属性[游戏对象属性.最小弓术] : 0) + (装备属性.ContainsKey(游戏对象属性.最大弓术) ? 装备属性[游戏对象属性.最大弓术] : 0)) > 0)
					{
						dictionary3[4][value] = num;
					}
				}
				List<KeyValuePair<byte, Dictionary<装备数据, int>>> 排序属性 = dictionary3.ToList().OrderByDescending(delegate(KeyValuePair<byte, Dictionary<装备数据, int>> x)
				{
					KeyValuePair<byte, Dictionary<装备数据, int>> keyValuePair2 = x;
					return keyValuePair2.Value.Values.Sum();
				}).ToList();
				List<KeyValuePair<byte, Dictionary<装备数据, int>>> list = 排序属性.Where((KeyValuePair<byte, Dictionary<装备数据, int>> O) => O.Value.Values.Sum() == 排序属性[0].Value.Values.Sum()).ToList();
				byte key = list[主程.随机数.Next(list.Count)].Key;
				List<KeyValuePair<装备数据, int>> list2 = dictionary3[key].ToList().OrderByDescending(delegate(KeyValuePair<装备数据, int> x)
				{
					KeyValuePair<装备数据, int> keyValuePair = x;
					return keyValuePair.Value;
				}).ToList();
				float num2 = Math.Min(10f, (float)((list2.Count < 1) ? 1 : list2[0].Value) + (float)((list2.Count >= 2) ? list2[1].Value : 0) / 3f);
				int num3 = dictionary2.Values.Sum((物品数据 x) => x.当前持久.V);
				float num4 = Math.Max(0f, num3 - 146);
				int num5 = 90 - v.升级次数.V * 10;
				float 概率 = num2 * (float)num5 * 0.001f + num4 * 0.01f;
				角色数据.升级装备.V = v;
				角色数据.取回时间.V = 主程.当前时间.AddHours(2.0);
				if (角色数据.升级成功.V = 计算类.计算概率(概率))
				{
					v.升级次数.V++;
					switch (key)
					{
					case 0:
						v.升级攻击.V++;
						break;
					case 1:
						v.升级魔法.V++;
						break;
					case 2:
						v.升级道术.V++;
						break;
					case 3:
						v.升级刺术.V++;
						break;
					case 4:
						v.升级弓术.V++;
						break;
					}
				}
				if (num3 < 30)
				{
					v.最大持久.V -= 3000;
					v.当前持久.V = Math.Min(v.当前持久.V, v.最大持久.V);
				}
				else if (num3 < 60)
				{
					v.最大持久.V -= 2000;
					v.当前持久.V = Math.Min(v.当前持久.V, v.最大持久.V);
				}
				else if (num3 >= 99)
				{
					if (num3 > 130 && 计算类.计算概率(1f - (float)v.最大持久.V * 0.5f / (float)v.默认持久))
					{
						v.最大持久.V += 1000;
						v.当前持久.V = Math.Min(v.当前持久.V, v.最大持久.V);
					}
				}
				else
				{
					v.最大持久.V -= 1000;
					v.当前持久.V = Math.Min(v.当前持久.V, v.最大持久.V);
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 1856
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 1853
			});
		}
	}

	public bool 玩家取回装备(int 扣除金币)
	{
		if (角色数据.升级装备.V == null)
		{
			return false;
		}
		if (角色数据.升级成功.V)
		{
			byte b = 0;
			while (b < 背包大小)
			{
				if (角色背包.ContainsKey(b))
				{
					b = (byte)(b + 1);
					continue;
				}
				金币数量 -= 扣除金币;
				角色背包[b] = 角色数据.升级装备.V;
				角色背包[b].物品位置.V = b;
				角色背包[b].物品容器.V = 1;
				网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = 角色背包[b].字节描述()
				});
				网络连接?.发送封包(new 取回升级武器());
				网络连接?.发送封包(new 武器升级结果());
				if (角色数据.升级装备.V.升级次数.V >= 5)
				{
					网络服务网关.发送公告($"[{对象名字}] 成功将 [{角色数据.升级装备.V.物品名字}] 升级到 {角色数据.升级装备.V.升级次数.V} 级.");
				}
				角色数据.升级装备.V = null;
				return 角色数据.升级成功.V;
			}
			角色数据.升级装备.V = null;
		}
		return 角色数据.升级成功.V;
	}

	public void 放弃升级武器()
	{
		角色数据.升级装备.V?.删除数据();
		角色数据.升级装备.V = null;
		网络连接?.发送封包(new 武器升级结果
		{
			升级结果 = 1
		});
	}

	public void 玩家发送广播(byte[] 数据)
	{
		uint num = BitConverter.ToUInt32(数据, 0);
		byte b = 数据[4];
		byte[] array = 数据.Skip(5).ToArray();
		switch (num)
		{
		default:
			网络连接.尝试断开连接(new Exception($"玩家发送广播时, 提供错误的频道参数. 频道: {num:X8}"));
			break;
		case 2415919107u:
		{
			switch (b)
			{
			case 1:
				if (金币数量 < 1000)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 4873
					});
					return;
				}
				金币数量 -= 1000;
				break;
			default:
				网络连接.尝试断开连接(new Exception($"传音或广播时提供错误的频道参数, 断开连接. 频道: {num:X8}  参数:{b}"));
				return;
			case 6:
			{
				if (!查找背包物品(2201, out var 物品))
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 4869
					});
					return;
				}
				消耗背包物品(1, 物品);
				break;
			}
			}
			byte[] 字节描述2 = null;
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				using BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
				binaryWriter2.Write(地图编号);
				binaryWriter2.Write(2415919107u);
				binaryWriter2.Write((int)b);
				binaryWriter2.Write((int)当前等级);
				binaryWriter2.Write(array);
				binaryWriter2.Write(Encoding.UTF8.GetBytes(对象名字));
				binaryWriter2.Write((byte)0);
				字节描述2 = memoryStream2.ToArray();
			}
			网络服务网关.发送封包(new 接收聊天消息
			{
				字节描述 = 字节描述2
			});
			主程.添加聊天日志("[" + ((b == 1) ? "广播" : "传音") + "][" + 对象名字 + "]: ", array);
			break;
		}
		case 2415919105u:
		{
			byte[] 字节描述 = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(2415919105u);
				binaryWriter.Write(地图编号);
				binaryWriter.Write(1);
				binaryWriter.Write((int)当前等级);
				binaryWriter.Write(array);
				binaryWriter.Write(Encoding.UTF8.GetBytes(对象名字));
				binaryWriter.Write((byte)0);
				字节描述 = memoryStream.ToArray();
			}
			发送封包(new 接收聊天信息
			{
				字节描述 = 字节描述
			});
			主程.添加聊天日志("[附近][" + 对象名字 + "]: ", array);
			break;
		}
		}
	}

	public void 玩家发送消息(byte[] 数据)
	{
		int num = BitConverter.ToInt32(数据, 0);
		byte[] array = 数据.Skip(4).ToArray();
		switch (num >> 28)
		{
		case 7:
		{
			if (所属队伍 == null)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 3853
				});
				break;
			}
			using MemoryStream memoryStream3 = new MemoryStream();
			using BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream3);
			binaryWriter3.Write(地图编号);
			binaryWriter3.Write(1879048192);
			binaryWriter3.Write(1);
			binaryWriter3.Write((int)当前等级);
			binaryWriter3.Write(array);
			binaryWriter3.Write(Encoding.UTF8.GetBytes(对象名字 + "\0"));
			所属队伍.发送封包(new 接收聊天消息
			{
				字节描述 = memoryStream3.ToArray()
			});
			主程.添加聊天日志("[队伍][" + 对象名字 + "]: ", array);
			break;
		}
		case 6:
			if (所属行会 != null)
			{
				if (!所属行会.行会禁言.ContainsKey(this.角色数据))
				{
					using (MemoryStream memoryStream4 = new MemoryStream())
					{
						using BinaryWriter binaryWriter4 = new BinaryWriter(memoryStream4);
						binaryWriter4.Write(地图编号);
						binaryWriter4.Write(1610612736);
						binaryWriter4.Write(1);
						binaryWriter4.Write((int)当前等级);
						binaryWriter4.Write(array);
						binaryWriter4.Write(Encoding.UTF8.GetBytes(对象名字));
						binaryWriter4.Write((byte)0);
						所属行会.发送封包(new 接收聊天消息
						{
							字节描述 = memoryStream4.ToArray()
						});
						主程.添加聊天日志("[行会][" + 对象名字 + "]: ", array);
						break;
					}
				}
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 4870
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6668
				});
			}
			break;
		case 0:
		{
			if (游戏数据网关.角色数据表.数据表.TryGetValue(num, out var value) && value is 角色数据 角色数据)
			{
				if (地图编号 == 角色数据.角色编号 || this.角色数据.黑名单表.Contains(this.角色数据) || 角色数据.网络连接 == null)
				{
					break;
				}
				byte[] 字节描述 = null;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write(角色数据.角色编号);
					binaryWriter.Write(地图编号);
					binaryWriter.Write(1);
					binaryWriter.Write((int)当前等级);
					binaryWriter.Write(array);
					binaryWriter.Write(Encoding.UTF8.GetBytes(对象名字));
					binaryWriter.Write((byte)0);
					字节描述 = memoryStream.ToArray();
				}
				网络连接?.发送封包(new 接收聊天消息
				{
					字节描述 = 字节描述
				});
				byte[] 字节描述2 = null;
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					using BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
					binaryWriter2.Write(地图编号);
					binaryWriter2.Write(角色数据.角色编号);
					binaryWriter2.Write(1);
					binaryWriter2.Write((int)当前等级);
					binaryWriter2.Write(array);
					binaryWriter2.Write(Encoding.UTF8.GetBytes(对象名字));
					binaryWriter2.Write((byte)0);
					字节描述2 = memoryStream2.ToArray();
				}
				角色数据.网络连接.发送封包(new 接收聊天消息
				{
					字节描述 = 字节描述2
				});
				主程.添加聊天日志($"[私聊][{对象名字}]=>[{角色数据.角色名字}]: ", array);
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 4868
				});
			}
			break;
		}
		}
	}

	public void 玩家好友聊天(byte[] 数据)
	{
		int key = BitConverter.ToInt32(数据, 0);
		byte[] array = 数据.Skip(4).ToArray();
		if (游戏数据网关.角色数据表.数据表.TryGetValue(key, out var value) && value is 角色数据 角色数据 && 好友列表.Contains(角色数据))
		{
			if (角色数据.网络连接 != null)
			{
				byte[] 字节数据 = null;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write(地图编号);
					binaryWriter.Write((int)当前等级);
					binaryWriter.Write(array);
					字节数据 = memoryStream.ToArray();
				}
				角色数据.网络连接.发送封包(new 发送好友消息
				{
					字节数据 = 字节数据
				});
				主程.添加聊天日志($"[好友][{对象名字}]=>[{角色数据}]: ", array);
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5124
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 4868
			});
		}
	}

	public void 玩家添加关注(int 对象编号, string 对象名字)
	{
		if (偶像列表.Count < 100)
		{
			游戏数据 value2;
			if (对象编号 == 0)
			{
				if (游戏数据网关.角色数据表.检索表.TryGetValue(对象名字, out var value) && value is 角色数据 角色数据)
				{
					if (!偶像列表.Contains(角色数据))
					{
						if (黑名单表.Contains(角色数据))
						{
							玩家解除屏蔽(角色数据.角色编号);
						}
						if (仇人列表.Contains(角色数据))
						{
							玩家删除仇人(角色数据.角色编号);
						}
						偶像列表.Add(角色数据);
						角色数据.粉丝列表.Add(this.角色数据);
						网络连接?.发送封包(new 玩家添加关注
						{
							对象编号 = 角色数据.数据索引.V,
							对象名字 = 角色数据.角色名字.V,
							是否好友 = (粉丝列表.Contains(角色数据) || 角色数据.偶像列表.Contains(this.角色数据))
						});
						网络连接?.发送封包(new 好友上线下线
						{
							对象编号 = 角色数据.数据索引.V,
							对象名字 = 角色数据.角色名字.V,
							对象职业 = (byte)角色数据.角色职业.V,
							对象性别 = (byte)角色数据.角色性别.V,
							上线下线 = (byte)((角色数据.网络连接 == null) ? 3u : 0u)
						});
						if (粉丝列表.Contains(角色数据) || 角色数据.偶像列表.Contains(this.角色数据))
						{
							好友列表.Add(角色数据);
							角色数据.好友列表.Add(this.角色数据);
						}
						角色数据.网络连接?.发送封包(new 对方关注自己
						{
							对象编号 = 地图编号,
							对象名字 = this.对象名字
						});
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 5122
						});
					}
				}
				else
				{
					网络连接.发送封包(new 游戏错误提示
					{
						错误代码 = 6732
					});
				}
			}
			else if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out value2) && value2 is 角色数据 角色数据2)
			{
				if (!偶像列表.Contains(角色数据2))
				{
					if (黑名单表.Contains(角色数据2))
					{
						玩家解除屏蔽(角色数据2.角色编号);
					}
					if (仇人列表.Contains(角色数据2))
					{
						玩家删除仇人(角色数据2.角色编号);
					}
					偶像列表.Add(角色数据2);
					角色数据2.粉丝列表.Add(this.角色数据);
					网络连接?.发送封包(new 玩家添加关注
					{
						对象编号 = 角色数据2.数据索引.V,
						对象名字 = 角色数据2.角色名字.V,
						是否好友 = (粉丝列表.Contains(角色数据2) || 角色数据2.偶像列表.Contains(this.角色数据))
					});
					网络连接?.发送封包(new 好友上线下线
					{
						对象编号 = 角色数据2.数据索引.V,
						对象名字 = 角色数据2.角色名字.V,
						对象职业 = (byte)角色数据2.角色职业.V,
						对象性别 = (byte)角色数据2.角色性别.V,
						上线下线 = (byte)((角色数据2.网络连接 == null) ? 3u : 0u)
					});
					if (粉丝列表.Contains(角色数据2) || 角色数据2.偶像列表.Contains(this.角色数据))
					{
						好友列表.Add(角色数据2);
						角色数据2.好友列表.Add(this.角色数据);
					}
					角色数据2.网络连接?.发送封包(new 对方关注自己
					{
						对象编号 = 地图编号,
						对象名字 = this.对象名字
					});
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5122
					});
				}
			}
			else
			{
				网络连接.发送封包(new 游戏错误提示
				{
					错误代码 = 6732
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5125
			});
		}
	}

	public void 玩家取消关注(int 对象编号)
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(对象名字, out var value) && value is 角色数据 角色数据)
		{
			if (偶像列表.Contains(角色数据))
			{
				偶像列表.Remove(角色数据);
				角色数据.粉丝列表.Remove(this.角色数据);
				网络连接?.发送封包(new 玩家取消关注
				{
					对象编号 = 角色数据.角色编号
				});
				if (好友列表.Contains(角色数据) || 角色数据.好友列表.Contains(this.角色数据))
				{
					好友列表.Remove(角色数据);
					角色数据.好友列表.Remove(this.角色数据);
				}
				角色数据.网络连接?.发送封包(new 对方取消关注
				{
					对象编号 = 地图编号,
					对象名字 = 对象名字
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5121
				});
			}
		}
		else
		{
			网络连接.发送封包(new 游戏错误提示
			{
				错误代码 = 6732
			});
		}
	}

	public void 玩家添加仇人(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (仇人列表.Count >= 100)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5125
				});
			}
			else if (!偶像列表.Contains(角色数据))
			{
				仇人列表.Add(角色数据);
				角色数据.仇恨列表.Add(this.角色数据);
				网络连接?.发送封包(new 玩家标记仇人
				{
					对象编号 = 角色数据.数据索引.V
				});
				网络连接?.发送封包(new 好友上线下线
				{
					对象编号 = 角色数据.数据索引.V,
					对象名字 = 角色数据.角色名字.V,
					对象职业 = (byte)角色数据.角色职业.V,
					对象性别 = (byte)角色数据.角色性别.V,
					上线下线 = (byte)((角色数据.网络连接 == null) ? 3u : 0u)
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5122
				});
			}
		}
		else
		{
			网络连接.发送封包(new 游戏错误提示
			{
				错误代码 = 6732
			});
		}
	}

	public void 玩家删除仇人(int 对象编号)
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(对象名字, out var value) && value is 角色数据 角色数据)
		{
			if (仇人列表.Contains(角色数据))
			{
				仇人列表.Remove(角色数据);
				角色数据.仇恨列表.Remove(this.角色数据);
				网络连接?.发送封包(new 玩家移除仇人
				{
					对象编号 = 角色数据.数据索引.V
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5126
				});
			}
		}
		else
		{
			网络连接.发送封包(new 游戏错误提示
			{
				错误代码 = 6732
			});
		}
	}

	public void 玩家屏蔽目标(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value))
		{
			if (value is 角色数据 角色数据)
			{
				if (黑名单表.Count >= 100)
				{
					return;
				}
				if (黑名单表.Contains(角色数据))
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 7426
					});
				}
				else if (对象编号 != 地图编号)
				{
					if (偶像列表.Contains(角色数据))
					{
						玩家取消关注(角色数据.角色编号);
					}
					黑名单表.Add(角色数据);
					网络连接?.发送封包(new 玩家屏蔽目标
					{
						对象编号 = 角色数据.数据索引.V,
						对象名字 = 角色数据.角色名字.V
					});
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 7429
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 7428
				});
			}
		}
		else
		{
			网络连接.发送封包(new 游戏错误提示
			{
				错误代码 = 6732
			});
		}
	}

	public void 玩家解除屏蔽(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (!黑名单表.Contains(角色数据))
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 7427
				});
			}
			else
			{
				黑名单表.Remove(角色数据);
				网络连接?.发送封包(new 解除玩家屏蔽
				{
					对象编号 = 角色数据.数据索引.V
				});
			}
		}
		else
		{
			网络连接.发送封包(new 游戏错误提示
			{
				错误代码 = 6732
			});
		}
	}

	public void 请求对象外观(int 对象编号, int 状态编号)
	{
		if (!地图处理网关.地图对象表.TryGetValue(对象编号, out var value))
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6732
			});
			return;
		}
		客户网络 客户网络;
		同步扩展数据 同步扩展数据;
		object obj;
		if (!(value is 玩家实例 玩家实例2))
		{
			if (!(value is 怪物实例 怪物实例2))
			{
				if (value is 宠物实例 宠物实例2)
				{
					客户网络 = 网络连接;
					if (客户网络 == null)
					{
						return;
					}
					同步扩展数据 = new 同步扩展数据
					{
						对象类型 = 2,
						对象编号 = 宠物实例2.地图编号,
						模板编号 = 宠物实例2.模板编号,
						当前等级 = 宠物实例2.宠物等级,
						对象等级 = 宠物实例2.当前等级,
						对象质量 = (byte)宠物实例2.宠物级别,
						最大体力 = 宠物实例2[游戏对象属性.最大体力],
						主人编号 = (宠物实例2.宠物主人?.地图编号 ?? 0)
					};
					玩家实例 宠物主人 = 宠物实例2.宠物主人;
					if (宠物主人 == null)
					{
						obj = null;
					}
					else
					{
						obj = 宠物主人.对象名字;
						if (obj != null)
						{
							goto IL_048b;
						}
					}
					obj = "";
					goto IL_048b;
				}
				if (value is 守卫实例 守卫实例2)
				{
					客户网络 客户网络2 = 网络连接;
					if (客户网络2 != null)
					{
						同步Npcc数据 同步Npcc数据 = new 同步Npcc数据
						{
							对象质量 = 3,
							对象编号 = 守卫实例2.地图编号,
							对象等级 = 守卫实例2.当前等级
						};
						同步Npcc数据.对象模板 = 守卫实例2.对象模板?.守卫编号 ?? 0;
						同步Npcc数据.体力上限 = 守卫实例2[游戏对象属性.最大体力];
						客户网络2.发送封包(同步Npcc数据);
					}
				}
				return;
			}
			if (怪物实例2.出生地图 != null)
			{
				客户网络 客户网络3 = 网络连接;
				if (客户网络3 != null)
				{
					同步Npcc数据 同步Npcc数据2 = new 同步Npcc数据
					{
						对象编号 = 怪物实例2.地图编号,
						对象等级 = 怪物实例2.当前等级,
						对象质量 = (byte)怪物实例2.怪物级别
					};
					同步Npcc数据2.对象模板 = 怪物实例2.对象模板?.怪物编号 ?? 0;
					同步Npcc数据2.体力上限 = 怪物实例2[游戏对象属性.最大体力];
					客户网络3.发送封包(同步Npcc数据2);
				}
			}
			else
			{
				网络连接?.发送封包(new 同步扩展数据
				{
					对象类型 = 1,
					主人编号 = 0,
					主人名字 = "",
					对象等级 = 怪物实例2.当前等级,
					对象编号 = 怪物实例2.地图编号,
					模板编号 = 怪物实例2.模板编号,
					当前等级 = 怪物实例2.宠物等级,
					对象质量 = (byte)怪物实例2.怪物级别,
					最大体力 = 怪物实例2[游戏对象属性.最大体力]
				});
			}
			return;
		}
		客户网络 客户网络4 = 网络连接;
		if (客户网络4 != null)
		{
			装备数据 v;
			同步玩家外观 同步玩家外观 = new 同步玩家外观
			{
				对象编号 = 玩家实例2.地图编号,
				对象PK值 = 玩家实例2.PK值惩罚,
				对象职业 = (byte)玩家实例2.角色职业,
				对象性别 = (byte)玩家实例2.角色性别,
				对象发型 = (byte)玩家实例2.角色发型,
				对象发色 = (byte)玩家实例2.角色发色,
				对象脸型 = (byte)玩家实例2.角色脸型,
				摆摊状态 = 玩家实例2.摆摊状态,
				摊位名字 = 玩家实例2.摊位名字,
				武器等级 = (byte)(玩家实例2.角色装备.TryGetValue(0, out v) ? (v?.升级次数.V ?? 0) : 0),
				身上武器 = (v?.对应模板.V.物品编号 ?? 0)
			};
			装备数据 v2;
			int 身上衣服 = (玩家实例2.角色装备.TryGetValue(1, out v2) ? (v2?.对应模板?.V?.物品编号).GetValueOrDefault() : 0);
			同步玩家外观.身上衣服 = 身上衣服;
			装备数据 v3;
			int 身上披风 = (玩家实例2.角色装备.TryGetValue(2, out v3) ? (v3?.对应模板?.V?.物品编号).GetValueOrDefault() : 0);
			同步玩家外观.身上披风 = 身上披风;
			同步玩家外观.当前体力 = 玩家实例2[游戏对象属性.最大体力];
			同步玩家外观.当前魔力 = 玩家实例2[游戏对象属性.最大魔力];
			同步玩家外观.对象名字 = 玩家实例2.对象名字;
			同步玩家外观.行会编号 = 玩家实例2.所属行会?.数据索引.V ?? 0;
			客户网络4.发送封包(同步玩家外观);
		}
		return;
		IL_048b:
		同步扩展数据.主人名字 = (string)obj;
		客户网络.发送封包(同步扩展数据);
	}

	public void 请求角色资料(int 角色编号)
	{
		客户网络 客户网络;
		同步角色信息 同步角色信息;
		object obj;
		if (游戏数据网关.角色数据表.数据表.TryGetValue(角色编号, out var value) && value is 角色数据 角色数据)
		{
			客户网络 = 网络连接;
			if (客户网络 == null)
			{
				return;
			}
			同步角色信息 = new 同步角色信息
			{
				对象编号 = 角色数据.数据索引.V,
				对象名字 = 角色数据.角色名字.V,
				会员等级 = 角色数据.本期特权.V,
				对象职业 = (byte)角色数据.角色职业.V,
				对象性别 = (byte)角色数据.角色性别.V
			};
			行会数据 v = 角色数据.所属行会.V;
			if (v != null)
			{
				obj = v.行会名字.V;
				if (obj != null)
				{
					goto IL_00e2;
				}
			}
			else
			{
				obj = null;
			}
			obj = "";
			goto IL_00e2;
		}
		网络连接?.发送封包(new 社交错误提示
		{
			错误编号 = 6732
		});
		return;
		IL_00e2:
		同步角色信息.行会名字 = (string)obj;
		客户网络.发送封包(同步角色信息);
	}

	public void 查询玩家战力(int 对象编号)
	{
		if (地图处理网关.地图对象表.TryGetValue(对象编号, out var value) && value is 玩家实例 玩家实例2)
		{
			网络连接?.发送封包(new 同步玩家战力
			{
				角色编号 = 玩家实例2.地图编号,
				角色战力 = 玩家实例2.当前战力
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 7171
			});
		}
	}

	public void 查看对象装备(int 对象编号)
	{
		if (地图处理网关.地图对象表.TryGetValue(对象编号, out var value) && value is 玩家实例 玩家实例2)
		{
			网络连接?.发送封包(new 同步角色装备
			{
				对象编号 = 玩家实例2.地图编号,
				装备数量 = (byte)玩家实例2.角色装备.Count,
				字节描述 = 玩家实例2.装备物品描述()
			});
			网络连接?.发送封包(new 同步玛法特权
			{
				玛法特权 = 玩家实例2.本期特权
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 7171
			});
		}
	}

	public void 查询排名榜单(int 榜单类型, int 起始位置)
	{
		if (起始位置 < 0 || 起始位置 > 29)
		{
			return;
		}
		byte b = (byte)榜单类型;
		int num = 0;
		int num2 = 起始位置 * 10;
		int num3 = 起始位置 * 10 + 10;
		列表监视器<角色数据> 列表监视器 = null;
		switch (榜单类型)
		{
		case 37:
			列表监视器 = 系统数据.数据.龙枪战力排名;
			num = 1;
			break;
		case 36:
			列表监视器 = 系统数据.数据.龙枪等级排名;
			num = 0;
			break;
		case 0:
			列表监视器 = 系统数据.数据.个人等级排名;
			num = 0;
			break;
		case 1:
			列表监视器 = 系统数据.数据.战士等级排名;
			num = 0;
			break;
		case 2:
			列表监视器 = 系统数据.数据.法师等级排名;
			num = 0;
			break;
		case 3:
			列表监视器 = 系统数据.数据.道士等级排名;
			num = 0;
			break;
		case 4:
			列表监视器 = 系统数据.数据.刺客等级排名;
			num = 0;
			break;
		case 5:
			列表监视器 = 系统数据.数据.弓手等级排名;
			num = 0;
			break;
		case 6:
			列表监视器 = 系统数据.数据.个人战力排名;
			num = 1;
			break;
		case 7:
			列表监视器 = 系统数据.数据.战士战力排名;
			num = 1;
			break;
		case 8:
			列表监视器 = 系统数据.数据.法师战力排名;
			num = 1;
			break;
		case 9:
			列表监视器 = 系统数据.数据.道士战力排名;
			num = 1;
			break;
		case 10:
			列表监视器 = 系统数据.数据.刺客战力排名;
			num = 1;
			break;
		case 11:
			列表监视器 = 系统数据.数据.弓手战力排名;
			num = 1;
			break;
		case 14:
			列表监视器 = 系统数据.数据.个人声望排名;
			num = 2;
			break;
		case 15:
			列表监视器 = 系统数据.数据.个人PK值排名;
			num = 3;
			break;
		}
		if (列表监视器 == null || 列表监视器.Count == 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(new byte[189]);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(b);
		binaryWriter.Write((ushort)角色数据.当前排名[b]);
		binaryWriter.Write((ushort)角色数据.历史排名[b]);
		binaryWriter.Write(列表监视器.Count);
		for (int i = num2; i < num3; i++)
		{
			binaryWriter.Write((long)(列表监视器[i]?.角色编号 ?? 0));
		}
		for (int j = num2; j < num3; j++)
		{
			switch (num)
			{
			default:
				binaryWriter.Write(0);
				break;
			case 0:
				binaryWriter.Write((long)((ulong)(列表监视器[j]?.角色等级 ?? 0) << 56));
				break;
			case 1:
				binaryWriter.Write((long)(列表监视器[j]?.角色战力 ?? 0));
				break;
			case 2:
				binaryWriter.Write((long)(列表监视器[j]?.师门声望 ?? 0));
				break;
			case 3:
				binaryWriter.Write((long)(列表监视器[j]?.角色PK值 ?? 0));
				break;
			}
		}
		for (int k = num2; k < num3; k++)
		{
			binaryWriter.Write((ushort)(列表监视器[k]?.历史排名[b] ?? 0));
		}
		网络连接?.发送封包(new 查询排行榜单
		{
			字节数据 = memoryStream.ToArray()
		});
	}

	public void 查询附近队伍()
	{
	}

	public void 查询队伍信息(int 对象编号)
	{
		if (对象编号 == 地图编号)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 3852
			});
			return;
		}
		客户网络 客户网络;
		查询队伍应答 查询队伍应答;
		object obj;
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			客户网络 = 网络连接;
			if (客户网络 == null)
			{
				return;
			}
			查询队伍应答 = new 查询队伍应答
			{
				队伍编号 = (角色数据.当前队伍?.队伍编号 ?? 0),
				队长编号 = (角色数据.当前队伍?.队长编号 ?? 0)
			};
			队伍数据 当前队伍 = 角色数据.当前队伍;
			if (当前队伍 != null)
			{
				obj = 当前队伍.队长名字;
				if (obj != null)
				{
					goto IL_00d2;
				}
			}
			else
			{
				obj = null;
			}
			obj = "";
			goto IL_00d2;
		}
		网络连接?.发送封包(new 社交错误提示
		{
			错误编号 = 6732
		});
		return;
		IL_00d2:
		查询队伍应答.队伍名字 = (string)obj;
		客户网络.发送封包(查询队伍应答);
	}

	public void 申请创建队伍(int 对象编号, byte 分配方式)
	{
		游戏数据 value;
		if (所属队伍 != null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 3847
			});
		}
		else if (地图编号 == 对象编号)
		{
			所属队伍 = new 队伍数据(this.角色数据, 1);
			网络连接?.发送封包(new 玩家加入队伍
			{
				字节描述 = 所属队伍.队伍描述()
			});
		}
		else if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out value) && value is 角色数据 角色数据)
		{
			if (角色数据.当前队伍 == null)
			{
				if (!角色数据.角色在线(out var 网络))
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 3844
					});
					return;
				}
				所属队伍 = new 队伍数据(this.角色数据, 1);
				网络连接?.发送封包(new 玩家加入队伍
				{
					字节描述 = 所属队伍.队伍描述()
				});
				所属队伍.邀请列表[角色数据] = 主程.当前时间.AddMinutes(5.0);
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 3842
				});
				网络.发送封包(new 发送组队申请
				{
					组队方式 = 0,
					对象编号 = 地图编号,
					对象职业 = (byte)角色职业,
					对象名字 = 对象名字
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 3847
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6732
			});
		}
	}

	public void 发送组队请求(int 对象编号)
	{
		if (对象编号 != 地图编号)
		{
			if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
			{
				if (所属队伍 == null)
				{
					if (角色数据.当前队伍 == null)
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3860
						});
					}
					else if (角色数据.当前队伍.队员数量 < 11)
					{
						if (角色数据.当前队伍.队长数据.角色在线(out var 网络))
						{
							角色数据.当前队伍.申请列表[this.角色数据] = 主程.当前时间.AddMinutes(5.0);
							网络.发送封包(new 发送组队申请
							{
								组队方式 = 1,
								对象编号 = 地图编号,
								对象职业 = (byte)角色职业,
								对象名字 = 对象名字
							});
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3842
							});
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3844
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3848
						});
					}
				}
				else if (地图编号 == 所属队伍.队长编号)
				{
					if (角色数据.当前队伍 == null)
					{
						if (所属队伍.队员数量 >= 11)
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3848
							});
							return;
						}
						if (!角色数据.角色在线(out var 网络2))
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3844
							});
							return;
						}
						所属队伍.邀请列表[角色数据] = 主程.当前时间.AddMinutes(5.0);
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3842
						});
						网络2.发送封包(new 发送组队申请
						{
							组队方式 = 0,
							对象编号 = 地图编号,
							对象职业 = (byte)角色职业,
							对象名字 = 对象名字
						});
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3847
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 3850
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6732
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 3852
			});
		}
	}

	public void 回应组队请求(int 对象编号, byte 组队方式, byte 回应方式)
	{
		游戏数据 value;
		if (地图编号 == 对象编号)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 3852
			});
		}
		else if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out value) && value is 角色数据 角色数据)
		{
			if (组队方式 != 0)
			{
				if (回应方式 == 0)
				{
					if (所属队伍 != null)
					{
						if (所属队伍.队员数量 >= 11)
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3848
							});
						}
						else if (地图编号 == 所属队伍.队长编号)
						{
							if (所属队伍.申请列表.ContainsKey(角色数据))
							{
								客户网络 网络;
								if (所属队伍.申请列表[角色数据] < 主程.当前时间)
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 3860
									});
								}
								else if (角色数据.当前队伍 != null)
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 3847
									});
								}
								else if (角色数据.角色在线(out 网络))
								{
									所属队伍.发送封包(new 队伍增加成员
									{
										队伍编号 = 所属队伍.队伍编号,
										对象编号 = 角色数据.角色编号,
										对象名字 = 角色数据.角色名字.V,
										对象性别 = (byte)角色数据.角色性别.V,
										对象职业 = (byte)角色数据.角色职业.V,
										在线离线 = 0
									});
									角色数据.当前队伍 = 所属队伍;
									所属队伍.队伍成员.Add(角色数据);
									网络.发送封包(new 玩家加入队伍
									{
										字节描述 = 所属队伍.队伍描述()
									});
								}
							}
							else
							{
								网络连接?.发送封包(new 社交错误提示
								{
									错误编号 = 3860
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3850
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3860
						});
					}
				}
				else
				{
					队伍数据 队伍数据 = 所属队伍;
					if (队伍数据 != null && 队伍数据.申请列表.Remove(角色数据) && 角色数据.角色在线(out var 网络2))
					{
						网络2.发送封包(new 社交错误提示
						{
							错误编号 = 3858
						});
					}
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 3857
					});
				}
			}
			else if (回应方式 == 0)
			{
				if (角色数据.当前队伍 != null)
				{
					if (所属队伍 != null)
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3847
						});
					}
					else if (角色数据.当前队伍.队员数量 >= 11)
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3848
						});
					}
					else if (角色数据.当前队伍.邀请列表.ContainsKey(this.角色数据))
					{
						if (!(角色数据.当前队伍.邀请列表[this.角色数据] < 主程.当前时间))
						{
							角色数据.当前队伍.发送封包(new 队伍增加成员
							{
								队伍编号 = 角色数据.当前队伍.队伍编号,
								对象编号 = 地图编号,
								对象名字 = 对象名字,
								对象性别 = (byte)角色性别,
								对象职业 = (byte)角色职业,
								在线离线 = 0
							});
							所属队伍 = 角色数据.当前队伍;
							角色数据.当前队伍.队伍成员.Add(this.角色数据);
							网络连接?.发送封包(new 玩家加入队伍
							{
								字节描述 = 所属队伍.队伍描述()
							});
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 3860
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3860
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 3860
					});
				}
			}
			else
			{
				队伍数据 当前队伍 = 角色数据.当前队伍;
				if (当前队伍 != null && 当前队伍.邀请列表.Remove(this.角色数据) && 角色数据.角色在线(out var 网络3))
				{
					网络3.发送封包(new 社交错误提示
					{
						错误编号 = 3856
					});
				}
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 3855
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6732
			});
		}
	}

	public void 申请队员离队(int 对象编号)
	{
		游戏数据 value;
		if (所属队伍 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 3854
			});
		}
		else if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out value) && value is 角色数据 角色数据)
		{
			if (this.角色数据 != 角色数据)
			{
				if (所属队伍.队伍成员.Contains(角色数据))
				{
					if (this.角色数据 != 所属队伍.队长数据)
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 3850
						});
						return;
					}
					所属队伍.队伍成员.Remove(角色数据);
					角色数据.当前队伍 = null;
					所属队伍.发送封包(new 队伍成员离开
					{
						队伍编号 = 所属队伍.数据索引.V,
						对象编号 = 角色数据.角色编号
					});
					角色数据.网络连接?.发送封包(new 玩家离开队伍
					{
						队伍编号 = 所属队伍.数据索引.V
					});
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6732
					});
				}
				return;
			}
			所属队伍.队伍成员.Remove(this.角色数据);
			所属队伍.发送封包(new 队伍成员离开
			{
				对象编号 = 地图编号,
				队伍编号 = 所属队伍.数据索引.V
			});
			网络连接?.发送封包(new 玩家离开队伍
			{
				队伍编号 = 所属队伍.数据索引.V
			});
			if (this.角色数据 == 所属队伍.队长数据)
			{
				角色数据 角色数据2 = 所属队伍.队伍成员.FirstOrDefault((角色数据 O) => O.网络连接 != null);
				if (角色数据2 != null)
				{
					所属队伍.队长数据 = 角色数据2;
					所属队伍.发送封包(new 队伍状态改变
					{
						成员上限 = 11,
						队伍编号 = 所属队伍.队伍编号,
						队伍名字 = 所属队伍.队长名字,
						分配方式 = 所属队伍.拾取方式,
						队长编号 = 所属队伍.队长编号
					});
				}
				else
				{
					所属队伍.删除数据();
				}
			}
			this.角色数据.当前队伍 = null;
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6732
			});
		}
	}

	public void 申请移交队长(int 对象编号)
	{
		if (所属队伍 != null)
		{
			游戏数据 value;
			if (this.角色数据 != 所属队伍.队长数据)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 3850
				});
			}
			else if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out value) && value is 角色数据 角色数据)
			{
				if (角色数据 == this.角色数据)
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 3852
					});
					return;
				}
				if (!所属队伍.队伍成员.Contains(角色数据))
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6732
					});
					return;
				}
				所属队伍.队长数据 = 角色数据;
				所属队伍.发送封包(new 队伍状态改变
				{
					成员上限 = 11,
					队伍编号 = 所属队伍.队伍编号,
					队伍名字 = 所属队伍.队长名字,
					分配方式 = 所属队伍.拾取方式,
					队长编号 = 所属队伍.队长编号
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6732
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 3854
			});
		}
	}

	public void 查询邮箱内容()
	{
		网络连接?.发送封包(new 同步邮箱内容
		{
			字节数据 = 全部邮件描述()
		});
	}

	public void 申请发送邮件(byte[] 数据)
	{
		if (数据.Length < 94 || 数据.Length > 839)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 申请发送邮件.  错误: 数据长度错误."));
		}
		else if (!(主程.当前时间 < 邮件时间))
		{
			if (金币数量 >= 1000)
			{
				byte[] array = 数据.Take(32).ToArray();
				byte[] array2 = 数据.Skip(32).Take(61).ToArray();
				数据.Skip(93).Take(4).ToArray();
				byte[] array3 = 数据.Skip(97).ToArray();
				if (array[0] == 0 || array2[0] == 0 || array3[0] == 0)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 申请发送邮件.  错误: 邮件文本错误."));
					return;
				}
				string key = Encoding.UTF8.GetString(array).Split(new char[1], StringSplitOptions.RemoveEmptyEntries)[0];
				string 标题 = Encoding.UTF8.GetString(array2).Split(new char[1], StringSplitOptions.RemoveEmptyEntries)[0];
				string 正文 = Encoding.UTF8.GetString(array3).Split(new char[1], StringSplitOptions.RemoveEmptyEntries)[0];
				if (游戏数据网关.角色数据表.检索表.TryGetValue(key, out var value) && value is 角色数据 角色数据)
				{
					if (角色数据.角色邮件.Count < 100)
					{
						金币数量 -= 1000;
						角色数据.发送邮件(new 邮件数据(this.角色数据, 标题, 正文, null));
						网络连接?.发送封包(new 成功发送邮件());
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6147
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6146
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6149
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6151
			});
		}
	}

	public void 查看邮件内容(int 邮件编号)
	{
		if (游戏数据网关.邮件数据表.数据表.TryGetValue(邮件编号, out var value) && value is 邮件数据 邮件数据)
		{
			if (全部邮件.Contains(邮件数据))
			{
				未读邮件.Remove(邮件数据);
				邮件数据.未读邮件.V = false;
				网络连接?.发送封包(new 同步邮件内容
				{
					字节数据 = 邮件数据.邮件内容描述()
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6148
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6148
			});
		}
	}

	public void 删除指定邮件(int 邮件编号)
	{
		if (游戏数据网关.邮件数据表.数据表.TryGetValue(邮件编号, out var value) && value is 邮件数据 邮件数据)
		{
			if (全部邮件.Contains(邮件数据))
			{
				网络连接?.发送封包(new 邮件删除成功
				{
					邮件编号 = 邮件数据.邮件编号
				});
				未读邮件.Remove(邮件数据);
				全部邮件.Remove(邮件数据);
				邮件数据.邮件附件.V?.删除数据();
				邮件数据.删除数据();
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6148
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6148
			});
		}
	}

	public void 提取邮件附件(int 邮件编号)
	{
		if (游戏数据网关.邮件数据表.数据表.TryGetValue(邮件编号, out var value) && value is 邮件数据 邮件数据)
		{
			if (全部邮件.Contains(邮件数据))
			{
				if (邮件数据.邮件附件.V != null)
				{
					if (背包剩余 > 0)
					{
						int num = -1;
						byte b = 0;
						while (b < 背包大小)
						{
							if (角色背包.ContainsKey(b))
							{
								b = (byte)(b + 1);
								continue;
							}
							num = b;
							break;
						}
						if (num == -1)
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 1793
							});
							return;
						}
						网络连接?.发送封包(new 成功提取附件
						{
							邮件编号 = 邮件数据.邮件编号
						});
						角色背包[(byte)num] = 邮件数据.邮件附件.V;
						邮件数据.邮件附件.V.物品位置.V = (byte)num;
						邮件数据.邮件附件.V.物品容器.V = 1;
						邮件数据.邮件附件.V = null;
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 1793
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6150
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6148
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6148
			});
		}
	}

	public void 查询行会信息(int 行会编号)
	{
		if (游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out var value) && value is 行会数据 行会数据)
		{
			网络连接?.发送封包(new 行会名字应答
			{
				行会编号 = 行会数据.数据索引.V,
				行会名字 = 行会数据.行会名字.V,
				创建时间 = 行会数据.创建日期.V,
				会长编号 = 行会数据.行会会长.V.数据索引.V,
				行会人数 = (byte)行会数据.行会成员.Count,
				行会等级 = 行会数据.行会等级.V
			});
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6669
			});
		}
	}

	public void 更多行会信息()
	{
	}

	public void 更多行会事记()
	{
	}

	public void 查看行会列表(int 行会编号, byte 查看方式)
	{
		游戏数据 value;
		int val = ((游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out value) && value is 行会数据 行会数据) ? (行会数据.行会排名.V - 1) : 0);
		int num = Math.Max(0, val);
		int num2 = ((查看方式 == 2) ? Math.Max(0, num) : Math.Max(0, num - 11));
		int num3 = Math.Min(12, 系统数据.数据.行会人数排名.Count - num2);
		if (num3 > 0)
		{
			List<行会数据> range = 系统数据.数据.行会人数排名.GetRange(num2, num3);
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(查看方式);
			binaryWriter.Write((byte)num3);
			foreach (行会数据 item in range)
			{
				binaryWriter.Write(item.行会检索描述());
			}
			网络连接?.发送封包(new 同步行会列表
			{
				字节数据 = memoryStream.ToArray()
			});
			return;
		}
		using MemoryStream memoryStream2 = new MemoryStream();
		using BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
		binaryWriter2.Write(查看方式);
		binaryWriter2.Write((byte)0);
		网络连接?.发送封包(new 同步行会列表
		{
			字节数据 = memoryStream2.ToArray()
		});
	}

	public void 查找对应行会(int 行会编号, string 行会名字)
	{
		if ((游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out var value) || 游戏数据网关.行会数据表.检索表.TryGetValue(行会名字, out value)) && value is 行会数据 行会数据)
		{
			网络连接?.发送封包(new 查找行会应答
			{
				字节数据 = 行会数据.行会检索描述()
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 6669
			});
		}
	}

	public void 申请解散行会()
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else if (所属行会.行会成员[角色数据] == 行会职位.会长)
		{
			if (所属行会.结盟行会.Count == 0)
			{
				if (所属行会.结盟行会.Count == 0)
				{
					if (!地图处理网关.攻城行会.Contains(所属行会))
					{
						if (所属行会 == 系统数据.数据.占领行会.V)
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 6819
							});
						}
						else
						{
							所属行会.解散行会();
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6819
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6740
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6739
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6709
			});
		}
	}

	public void 申请创建行会(byte[] 数据)
	{
		if (打开界面 != "Guild")
		{
			网络连接.尝试断开连接(new Exception("错误操作: 申请创建行会. 错误: 没有打开界面."));
		}
		else if (所属行会 != null)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 6707
			});
		}
		else if (当前等级 < 12)
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 6699
			});
		}
		else if (金币数量 >= 200000)
		{
			if (!查找背包物品(80002, out var 物品))
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6664
				});
			}
			else if (数据.Length > 25 && 数据.Length < 128)
			{
				string[] array = Encoding.UTF8.GetString(数据.Take(25).ToArray()).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = Encoding.UTF8.GetString(数据.Skip(25).ToArray()).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 0 && array2.Length != 0 && Encoding.UTF8.GetBytes(array[0]).Length < 25 && Encoding.UTF8.GetBytes(array2[0]).Length < 101)
				{
					if (游戏数据网关.行会数据表.检索表.ContainsKey(array[0]))
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 6697
						});
						return;
					}
					金币数量 -= 200000;
					消耗背包物品(1, 物品);
					所属行会 = new 行会数据(this, array[0], array2[0]);
					网络连接?.发送封包(new 创建行会应答
					{
						行会名字 = 所属行会.行会名字.V
					});
					网络连接?.发送封包(new 行会信息公告
					{
						字节数据 = 所属行会.行会信息描述()
					});
					发送封包(new 同步对象行会
					{
						对象编号 = 地图编号,
						行会编号 = 所属行会.行会编号
					});
					网络服务网关.发送公告($"[{对象名字}]创建了行会[{所属行会}]");
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 申请创建行会. 错误: 字符长度错误."));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 申请创建行会. 错误: 数据长度错误."));
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 6699
			});
		}
	}

	public void 更改行会公告(byte[] 数据)
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else if (所属行会.行会成员[角色数据] > 行会职位.监事)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6709
			});
		}
		else if (数据.Length != 0 && 数据.Length < 255)
		{
			if (数据[0] == 0)
			{
				所属行会.更改公告("");
			}
			else
			{
				所属行会.更改公告(Encoding.UTF8.GetString(数据).Split(default(char))[0]);
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 更改行会公告. 错误: 数据长度错误"));
		}
	}

	public void 更改行会宣言(byte[] 数据)
	{
		if (所属行会 != null)
		{
			if (所属行会.行会成员[角色数据] > 行会职位.监事)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
			else if (数据.Length != 0 && 数据.Length < 101)
			{
				if (数据[0] == 0)
				{
					所属行会.更改宣言(角色数据, "");
				}
				else
				{
					所属行会.更改宣言(角色数据, Encoding.UTF8.GetString(数据).Split(default(char))[0]);
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 更改行会公告. 错误: 数据长度错误"));
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 处理入会邀请(int 对象编号, byte 处理类型)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (角色数据.当前行会 == null || !角色数据.当前行会.邀请列表.Remove(this.角色数据))
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6731
				});
			}
			else if (处理类型 != 2)
			{
				角色数据.网络连接?.发送封包(new 行会邀请应答
				{
					对象名字 = 对象名字,
					应答类型 = 2
				});
			}
			else if (所属行会 != null)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6707
				});
			}
			else if (角色数据.所属行会.V.行会成员.Count >= 100)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
			else
			{
				角色数据.网络连接?.发送封包(new 行会邀请应答
				{
					对象名字 = 对象名字,
					应答类型 = 1
				});
				角色数据.当前行会.添加成员(this.角色数据);
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6732
			});
		}
	}

	public void 处理入会申请(int 对象编号, byte 处理类型)
	{
		if (所属行会 != null)
		{
			游戏数据 value;
			if ((byte)所属行会.行会成员[this.角色数据] >= 6)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
			else if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out value) && value is 角色数据 角色数据)
			{
				if (所属行会.申请列表.Remove(角色数据))
				{
					if (处理类型 == 2)
					{
						if (角色数据.当前行会 != null)
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6707
							});
						}
						else
						{
							所属行会.添加成员(角色数据);
							网络连接?.发送封包(new 入会申请应答
							{
								对象编号 = 角色数据.角色编号
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 入会申请应答
						{
							对象编号 = 角色数据.角色编号
						});
						角色数据.发送邮件(new 邮件数据(null, "入会申请被拒绝", "行会[" + 所属行会.行会名字.V + "]拒绝了你的入会申请.", null));
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6731
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6732
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 申请加入行会(int 行会编号, string 行会名字)
	{
		if ((游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out var value) || 游戏数据网关.行会数据表.检索表.TryGetValue(行会名字, out value)) && value is 行会数据 行会数据)
		{
			if (所属行会 != null)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6707
				});
			}
			else if (当前等级 >= 8)
			{
				if (行会数据.行会成员.Count < 100)
				{
					if (行会数据.申请列表.Count <= 20)
					{
						行会数据.申请列表[角色数据] = 主程.当前时间.AddHours(1.0);
						行会数据.行会提醒(行会职位.执事, 1);
						网络连接?.发送封包(new 加入行会应答
						{
							行会编号 = 行会数据.行会编号
						});
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 6703
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6710
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6714
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 6669
			});
		}
	}

	public void 邀请加入行会(string 对象名字)
	{
		if (所属行会 != null)
		{
			foreach (KeyValuePair<角色数据, DateTime> item in 所属行会.邀请列表.ToList())
			{
				if (主程.当前时间 > item.Value)
				{
					所属行会.邀请列表.Remove(item.Key);
				}
			}
		}
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else if (所属行会.行会成员[this.角色数据] == 行会职位.会员)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6709
			});
		}
		else if (所属行会.行会成员.Count < 100)
		{
			if (游戏数据网关.角色数据表.检索表.TryGetValue(对象名字, out var value) && value is 角色数据 角色数据)
			{
				if (!角色数据.角色在线(out var 网络))
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6711
					});
					return;
				}
				if (角色数据.当前行会 != null)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6707
					});
					return;
				}
				if (角色数据.角色等级 < 8)
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6714
					});
					return;
				}
				所属行会.邀请列表[角色数据] = 主程.当前时间.AddHours(1.0);
				网络.发送封包(new 受邀加入行会
				{
					对象编号 = 地图编号,
					对象名字 = this.对象名字,
					行会名字 = 所属行会.行会名字.V
				});
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6713
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6732
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6709
			});
		}
	}

	public void 查看申请列表()
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else
		{
			网络连接?.发送封包(new 查看申请名单
			{
				字节描述 = 所属行会.入会申请描述()
			});
		}
	}

	public void 申请离开行会()
	{
		if (所属行会 != null)
		{
			if (所属行会.行会成员[角色数据] == 行会职位.会长)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6718
				});
			}
			else
			{
				所属行会.退出行会(角色数据);
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 发放行会福利()
	{
	}

	public void 逐出行会成员(int 对象编号)
	{
		if (所属行会 != null)
		{
			if (地图编号 != 对象编号)
			{
				if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据 && 所属行会 == 角色数据.当前行会)
				{
					if (所属行会.行会成员[this.角色数据] >= 行会职位.长老 || 所属行会.行会成员[this.角色数据] >= 所属行会.行会成员[角色数据])
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6709
						});
						return;
					}
					所属行会.逐出成员(this.角色数据, 角色数据);
					角色数据.发送邮件(new 邮件数据(null, "你被逐出行会", "你被[" + 所属行会.行会名字.V + "]的官员[" + 对象名字 + "]逐出了行会.", null));
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6732
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 转移会长职位(int 对象编号)
	{
		if (所属行会 != null)
		{
			if (所属行会.行会成员[this.角色数据] != 行会职位.会长)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6719
				});
			}
			else if (地图编号 != 对象编号)
			{
				if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据 && 角色数据.当前行会 == 所属行会)
				{
					所属行会.转移会长(this.角色数据, 角色数据);
					return;
				}
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6732
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6681
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 捐献行会资金(int 金币数量)
	{
	}

	public void 设置行会禁言(int 对象编号, byte 禁言状态)
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else if (地图编号 != 对象编号)
		{
			if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据 && 角色数据.当前行会 == 所属行会)
			{
				if (所属行会.行会成员[this.角色数据] >= 行会职位.理事 || 所属行会.行会成员[this.角色数据] >= 所属行会.行会成员[角色数据])
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6709
					});
				}
				else
				{
					所属行会.成员禁言(this.角色数据, 角色数据, 禁言状态);
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6732
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6709
			});
		}
	}

	public void 变更会员职位(int 对象编号, byte 对象职位)
	{
		if (所属行会 != null)
		{
			if (地图编号 != 对象编号)
			{
				if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据 && 角色数据.当前行会 == 所属行会)
				{
					if (所属行会.行会成员[this.角色数据] < 行会职位.理事 && 所属行会.行会成员[this.角色数据] < 所属行会.行会成员[角色数据])
					{
						if (对象职位 > 1 && 对象职位 < 8 && 对象职位 != (byte)所属行会.行会成员[角色数据])
						{
							if (对象职位 != 2 || 所属行会.行会成员.Values.Where((行会职位 O) => O == 行会职位.副长).Count() < 2)
							{
								if (对象职位 == 3 && 所属行会.行会成员.Values.Where((行会职位 O) => O == 行会职位.长老).Count() >= 4)
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 6717
									});
								}
								else if (对象职位 == 4 && 所属行会.行会成员.Values.Where((行会职位 O) => O == 行会职位.监事).Count() >= 4)
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 6717
									});
								}
								else if (对象职位 == 5 && 所属行会.行会成员.Values.Where((行会职位 O) => O == 行会职位.理事).Count() >= 4)
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 6717
									});
								}
								else if (对象职位 == 6 && 所属行会.行会成员.Values.Where((行会职位 O) => O == 行会职位.执事).Count() >= 4)
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 6717
									});
								}
								else
								{
									所属行会.更改职位(this.角色数据, 角色数据, (行会职位)对象职位);
								}
							}
							else
							{
								网络连接?.发送封包(new 社交错误提示
								{
									错误编号 = 6717
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 6704
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6709
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6732
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6681
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 申请行会外交(byte 外交类型, byte 外交时间, string 行会名字)
	{
		if (所属行会 != null)
		{
			if (所属行会.行会名字.V == 行会名字)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6694
				});
			}
			else if (所属行会.行会成员[角色数据] < 行会职位.长老)
			{
				if (游戏数据网关.行会数据表.检索表.TryGetValue(行会名字, out var value) && value is 行会数据 行会数据)
				{
					if (!所属行会.结盟行会.ContainsKey(行会数据))
					{
						if (!所属行会.敌对行会.ContainsKey(行会数据))
						{
							if (外交时间 < 1 || 外交时间 > 3)
							{
								网络连接.尝试断开连接(new Exception("错误操作: 申请行会外交.  错误: 时间参数错误"));
								return;
							}
							switch (外交类型)
							{
							default:
								网络连接.尝试断开连接(new Exception("错误操作: 申请行会外交.  错误: 类型参数错误"));
								break;
							case 2:
								所属行会.行会敌对(行会数据, 外交时间);
								网络服务网关.发送公告($"[{所属行会}]和[{行会数据}]成为敌对行会.");
								break;
							case 1:
								if (所属行会.结盟行会.Count < 10)
								{
									if (行会数据.结盟行会.Count >= 10)
									{
										网络连接?.发送封包(new 社交错误提示
										{
											错误编号 = 6668
										});
									}
									else
									{
										所属行会.申请结盟(角色数据, 行会数据, 外交时间);
									}
								}
								else
								{
									网络连接?.发送封包(new 社交错误提示
									{
										错误编号 = 6668
									});
								}
								break;
							}
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 6726
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6727
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6669
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 申请行会敌对(byte 敌对时间, string 行会名字)
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else if (所属行会.行会名字.V == 行会名字)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6694
			});
		}
		else if (所属行会.行会成员[角色数据] < 行会职位.长老)
		{
			if (游戏数据网关.行会数据表.检索表.TryGetValue(行会名字, out var value) && value is 行会数据 行会数据)
			{
				if (所属行会.结盟行会.ContainsKey(行会数据))
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6727
					});
				}
				else if (!所属行会.敌对行会.ContainsKey(行会数据))
				{
					if (敌对时间 < 1 || 敌对时间 > 3)
					{
						网络连接.尝试断开连接(new Exception("错误操作: 申请行会敌对.  错误: 时间参数错误"));
						return;
					}
					所属行会.行会敌对(行会数据, 敌对时间);
					网络服务网关.发送公告($"[{所属行会}]和[{行会数据}]成为敌对行会.");
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 6726
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6669
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6709
			});
		}
	}

	public void 查看结盟申请()
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else
		{
			网络连接?.发送封包(new 同步结盟申请
			{
				字节描述 = 所属行会.结盟申请描述()
			});
		}
	}

	public void 处理结盟申请(byte 处理类型, int 行会编号)
	{
		if (所属行会 == null)
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
		else if (所属行会.行会编号 != 行会编号)
		{
			游戏数据 value;
			if (所属行会.行会成员[角色数据] >= 行会职位.长老)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
			else if (游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out value) && value is 行会数据 行会数据)
			{
				if (!所属行会.结盟行会.ContainsKey(行会数据))
				{
					if (所属行会.敌对行会.ContainsKey(行会数据))
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6726
						});
					}
					else if (所属行会.结盟申请.ContainsKey(行会数据))
					{
						switch (处理类型)
						{
						default:
							网络连接.尝试断开连接(new Exception("错误操作: 处理结盟申请.  错误: 处理类型错误."));
							break;
						case 2:
							所属行会.行会结盟(行会数据);
							网络服务网关.发送公告($"[{所属行会}]和[{行会数据}]成为结盟行会.");
							所属行会.结盟申请.Remove(行会数据);
							break;
						case 1:
							网络连接?.发送封包(new 结盟申请应答
							{
								行会编号 = 行会数据.行会编号
							});
							行会数据.发送邮件(行会职位.副长, "结盟申请被拒绝", "行会[" + 所属行会.行会名字.V + "]拒绝了你所在行会的结盟申请.");
							所属行会.结盟申请.Remove(行会数据);
							break;
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 6695
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6727
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6669
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6694
			});
		}
	}

	public void 申请解除结盟(int 行会编号)
	{
		if (所属行会 != null)
		{
			游戏数据 value;
			if (所属行会.行会编号 == 行会编号)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6694
				});
			}
			else if (所属行会.行会成员[角色数据] >= 行会职位.长老)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
			else if (游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out value) && value is 行会数据 行会数据)
			{
				if (所属行会.结盟行会.ContainsKey(行会数据))
				{
					所属行会.解除结盟(角色数据, 行会数据);
					网络服务网关.发送公告($"[{所属行会}]解除了和[{行会数据}]的行会结盟.");
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6728
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6669
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 申请解除敌对(int 行会编号)
	{
		if (所属行会 != null)
		{
			if (所属行会.行会编号 == 行会编号)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6694
				});
			}
			else if (所属行会.行会成员[角色数据] < 行会职位.长老)
			{
				if (游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out var value) && value is 行会数据 行会数据)
				{
					if (!所属行会.敌对行会.ContainsKey(行会数据))
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 6826
						});
					}
					else if (行会数据.解除申请.ContainsKey(所属行会))
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 6708
						});
					}
					else
					{
						所属行会.申请解敌(角色数据, 行会数据);
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6669
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 处理解除申请(int 行会编号, byte 应答类型)
	{
		if (所属行会 != null)
		{
			游戏数据 value;
			if (所属行会.行会编号 == 行会编号)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6694
				});
			}
			else if (所属行会.行会成员[角色数据] >= 行会职位.长老)
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 6709
				});
			}
			else if (游戏数据网关.行会数据表.数据表.TryGetValue(行会编号, out value) && value is 行会数据 行会数据)
			{
				if (所属行会.敌对行会.ContainsKey(行会数据))
				{
					if (所属行会.解除申请.ContainsKey(行会数据))
					{
						if (应答类型 != 2)
						{
							所属行会.发送封包(new 解除敌对列表
							{
								申请类型 = 2,
								行会编号 = 行会数据.行会编号
							});
							所属行会.解除申请.Remove(行会数据);
						}
						else if (地图处理网关.沙城节点 >= 2 && ((所属行会 == 系统数据.数据.占领行会.V && 地图处理网关.攻城行会.Contains(行会数据)) || (行会数据 == 系统数据.数据.占领行会.V && 地图处理网关.攻城行会.Contains(所属行会))))
						{
							网络连接?.发送封包(new 游戏错误提示
							{
								错误代码 = 6800
							});
						}
						else
						{
							所属行会.解除敌对(行会数据);
							网络服务网关.发送公告($"[{所属行会}]解除了和[{行会数据}]的行会敌对.");
							所属行会.解除申请.Remove(行会数据);
						}
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 5899
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 6826
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 6669
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6668
			});
		}
	}

	public void 查询师门成员()
	{
		if (所属师门 != null)
		{
			网络连接?.发送封包(new 同步师门成员
			{
				字节数据 = 所属师门.成员数据()
			});
		}
	}

	public void 查询师门奖励()
	{
		if (所属师门 != null)
		{
			网络连接?.发送封包(new 同步师门奖励
			{
				字节数据 = 所属师门.奖励数据(角色数据)
			});
		}
	}

	public void 查询拜师名册()
	{
	}

	public void 查询收徒名册()
	{
	}

	public void 玩家申请拜师(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (所属师门 == null)
			{
				if (当前等级 < 30)
				{
					if (角色数据.角色等级 >= 30)
					{
						if (角色数据.当前师门 == null || 角色数据.角色编号 == 角色数据.当前师门.师父编号)
						{
							客户网络 网络;
							if (角色数据.当前师门 != null && 角色数据.当前师门.徒弟数量 >= 3)
							{
								网络连接?.发送封包(new 社交错误提示
								{
									错误编号 = 5891
								});
							}
							else if (角色数据.角色在线(out 网络))
							{
								if (角色数据.当前师门 == null)
								{
									角色数据.当前师门 = new 师门数据(角色数据);
								}
								角色数据.当前师门.申请列表[地图编号] = 主程.当前时间;
								网络连接?.发送封包(new 申请拜师应答
								{
									对象编号 = 角色数据.角色编号
								});
								网络.发送封包(new 申请拜师提示
								{
									对象编号 = 地图编号
								});
							}
							else
							{
								网络连接?.发送封包(new 社交错误提示
								{
									错误编号 = 5892
								});
							}
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 5890
							});
						}
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 5894
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5915
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5895
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5913
			});
		}
	}

	public void 同意拜师申请(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (当前等级 < 30)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 同意拜师申请, 错误: 自身等级不够."));
			}
			else if (角色数据.角色等级 < 30)
			{
				if (角色数据.当前师门 != null)
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5895
					});
				}
				else if (所属师门 != null)
				{
					if (所属师门.师父编号 == 地图编号)
					{
						客户网络 网络;
						if (!所属师门.申请列表.ContainsKey(角色数据.角色编号))
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 5898
							});
						}
						else if (所属师门.徒弟数量 >= 3)
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 5891
							});
						}
						else if (角色数据.角色在线(out 网络))
						{
							if (所属师门 == null)
							{
								所属师门 = new 师门数据(this.角色数据);
							}
							所属师门.添加徒弟(角色数据);
							所属师门.发送封包(new 收徒成功提示
							{
								对象编号 = 角色数据.角色编号
							});
							网络连接?.发送封包(new 拜师申请通过
							{
								对象编号 = 角色数据.角色编号
							});
							网络连接?.发送封包(new 同步师门成员
							{
								字节数据 = 所属师门.成员数据()
							});
							网络.发送封包(new 同步师门成员
							{
								字节数据 = 所属师门.成员数据()
							});
							网络.发送封包(new 同步师门信息
							{
								师门参数 = 1
							});
						}
						else
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 5893
							});
						}
					}
					else
					{
						网络连接.尝试断开连接(new Exception("错误操作: 同意拜师申请, 错误: 自身尚未出师."));
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 同意拜师申请, 错误: 尚未创建师门."));
				}
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5894
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5913
			});
		}
	}

	public void 拒绝拜师申请(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (所属师门 == null)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 拒绝拜师申请, 错误: 尚未创建师门."));
			}
			else if (所属师门.师父编号 == 地图编号)
			{
				if (所属师门.申请列表.ContainsKey(角色数据.角色编号))
				{
					网络连接?.发送封包(new 拜师申请拒绝
					{
						对象编号 = 角色数据.角色编号
					});
					if (所属师门.申请列表.Remove(角色数据.角色编号))
					{
						角色数据.网络连接?.发送封包(new 拒绝拜师提示
						{
							对象编号 = 地图编号
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5898
					});
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 拒绝拜师申请, 错误: 自身尚未出师."));
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5913
			});
		}
	}

	public void 玩家申请收徒(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (当前等级 >= 30)
			{
				if (角色数据.角色等级 < 30)
				{
					if (角色数据.当前师门 == null)
					{
						if (所属师门 != null && 所属师门.师父编号 != 地图编号)
						{
							网络连接.尝试断开连接(new Exception("错误操作: 玩家申请收徒, 错误: 自身尚未出师."));
							return;
						}
						if (所属师门 != null && 所属师门.徒弟数量 >= 3)
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 5891
							});
							return;
						}
						if (!角色数据.角色在线(out var 网络))
						{
							网络连接?.发送封包(new 社交错误提示
							{
								错误编号 = 5893
							});
							return;
						}
						if (所属师门 == null)
						{
							所属师门 = new 师门数据(this.角色数据);
						}
						所属师门.邀请列表[角色数据.角色编号] = 主程.当前时间;
						网络连接?.发送封包(new 申请收徒应答
						{
							对象编号 = 角色数据.角色编号
						});
						网络.发送封包(new 申请收徒提示
						{
							对象编号 = 地图编号,
							对象等级 = 当前等级,
							对象声望 = 师门声望
						});
					}
					else
					{
						网络连接?.发送封包(new 社交错误提示
						{
							错误编号 = 5895
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5894
					});
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 玩家申请收徒, 错误: 自身等级不够."));
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5913
			});
		}
	}

	public void 同意收徒申请(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (当前等级 <= 30)
			{
				if (所属师门 != null)
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5895
					});
					return;
				}
				if (角色数据.角色等级 < 30)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 同意收徒申请, 错误: 对方等级不够."));
					return;
				}
				if (角色数据.当前师门 == null)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 同意收徒申请, 错误: 对方没有师门."));
					return;
				}
				if (角色数据.当前师门.师父编号 != 角色数据.角色编号)
				{
					网络连接.尝试断开连接(new Exception("错误操作: 同意收徒申请, 错误: 对方尚未出师."));
					return;
				}
				if (!角色数据.当前师门.邀请列表.ContainsKey(地图编号))
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5899
					});
					return;
				}
				if (角色数据.当前师门.徒弟数量 >= 3)
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5891
					});
					return;
				}
				if (!角色数据.角色在线(out var 网络))
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5892
					});
					return;
				}
				网络连接?.发送封包(new 收徒申请同意
				{
					对象编号 = 角色数据.角色编号
				});
				if (角色数据.当前师门 == null)
				{
					角色数据.当前师门 = new 师门数据(角色数据);
				}
				网络.发送封包(new 收徒成功提示
				{
					对象编号 = 地图编号
				});
				角色数据.当前师门.发送封包(new 收徒成功提示
				{
					对象编号 = 地图编号
				});
				角色数据.当前师门.添加徒弟(this.角色数据);
				网络连接?.发送封包(new 同步师门成员
				{
					字节数据 = 角色数据.当前师门.成员数据()
				});
				网络连接?.发送封包(new 同步师门信息
				{
					师门参数 = 1
				});
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5915
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5913
			});
		}
	}

	public void 拒绝收徒申请(int 对象编号)
	{
		if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据)
		{
			if (角色数据.所属师门 == null)
			{
				网络连接.尝试断开连接(new Exception("错误操作: 拒绝收徒申请, 错误: 尚未创建师门."));
			}
			else if (角色数据.当前师门.师父编号 == 角色数据.角色编号)
			{
				if (角色数据.当前师门.邀请列表.ContainsKey(地图编号))
				{
					网络连接?.发送封包(new 收徒申请拒绝
					{
						对象编号 = 角色数据.角色编号
					});
					if (角色数据.当前师门.邀请列表.Remove(地图编号))
					{
						角色数据.网络连接?.发送封包(new 拒绝收徒提示
						{
							对象编号 = 地图编号
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 社交错误提示
					{
						错误编号 = 5899
					});
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 拒绝拜师申请, 错误: 自身尚未出师."));
			}
		}
		else
		{
			网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 5913
			});
		}
	}

	public void 逐出师门申请(int 对象编号)
	{
		if (所属师门 == null)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 逐出师门申请, 错误: 自身没有师门."));
		}
		else if (所属师门.师父编号 == 地图编号)
		{
			if (游戏数据网关.角色数据表.数据表.TryGetValue(对象编号, out var value) && value is 角色数据 角色数据 && 所属师门.师门成员.Contains(角色数据))
			{
				网络连接?.发送封包(new 逐出师门应答
				{
					对象编号 = 角色数据.角色编号
				});
				所属师门.发送封包(new 逐出师门提示
				{
					对象编号 = 角色数据.角色编号
				});
				int num = 所属师门.徒弟出师金币(角色数据);
				int num2 = 所属师门.徒弟出师经验(角色数据);
				if (!地图处理网关.玩家对象表.TryGetValue(角色数据.角色编号, out var value2))
				{
					角色数据.获得经验(num2);
					角色数据.金币数量 += num;
				}
				else
				{
					value2.金币数量 += num;
					value2.玩家增加经验(null, num2);
				}
				所属师门.移除徒弟(角色数据);
				角色数据.当前师门 = null;
				角色数据.网络连接?.发送封包(new 同步师门信息
				{
					师门参数 = (byte)((角色数据.角色等级 >= 30) ? 2u : 0u)
				});
				角色数据.发送邮件(new 邮件数据(null, "你被逐出了师门", "你被[" + 对象名字 + "]逐出了师门.", null));
			}
			else
			{
				网络连接?.发送封包(new 社交错误提示
				{
					错误编号 = 5913
				});
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 逐出师门申请, 错误: 自己不是师父."));
		}
	}

	public void 离开师门申请()
	{
		if (所属师门 != null)
		{
			if (!所属师门.师门成员.Contains(角色数据))
			{
				网络连接.尝试断开连接(new Exception("错误操作: 离开师门申请, 错误: 自身不是徒弟."));
				return;
			}
			网络连接?.发送封包(new 离开师门应答());
			所属师门.师父数据.网络连接?.发送封包(new 离开师门提示
			{
				对象编号 = 地图编号
			});
			所属师门.发送封包(new 离开师门提示
			{
				对象编号 = 地图编号
			});
			所属师门.师父数据.发送邮件(new 邮件数据(null, "徒弟叛离师门", "你的徒弟[" + 对象名字 + "]已经叛离了师门.", null));
			int num = 所属师门.徒弟提供金币(角色数据);
			int num2 = 所属师门.徒弟提供声望(角色数据);
			int num3 = 所属师门.徒弟提供金币(角色数据);
			if (!地图处理网关.玩家对象表.TryGetValue(所属师门.师父数据.角色编号, out var value))
			{
				所属师门.师父数据.获得经验(num3);
				所属师门.师父数据.金币数量 += num;
				所属师门.师父数据.师门声望 += num2;
			}
			else
			{
				value.金币数量 += num;
				value.师门声望 += num2;
				value.玩家增加经验(null, num3);
			}
			所属师门.移除徒弟(角色数据);
			角色数据.当前师门 = null;
			网络连接?.发送封包(new 同步师门信息
			{
				师门参数 = 师门参数
			});
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 离开师门申请, 错误: 自身没有师门."));
		}
	}

	public void 提交出师申请()
	{
		if (所属师门 == null)
		{
			网络连接.尝试断开连接(new Exception("错误操作: 提交出师申请, 错误: 自身没有师门."));
		}
		else if (当前等级 >= 30)
		{
			if (所属师门.师门成员.Contains(角色数据))
			{
				int num = 所属师门.徒弟提供金币(角色数据);
				int num2 = 所属师门.徒弟提供声望(角色数据);
				int num3 = 所属师门.徒弟提供金币(角色数据);
				if (!地图处理网关.玩家对象表.TryGetValue(所属师门.师父数据.角色编号, out var value))
				{
					所属师门.师父数据.获得经验(num3);
					所属师门.师父数据.金币数量 += num;
					所属师门.师父数据.师门声望 += num2;
				}
				else
				{
					value.金币数量 += num;
					value.师门声望 += num2;
					value.玩家增加经验(null, num3);
				}
				金币数量 += 所属师门.徒弟出师金币(角色数据);
				玩家增加经验(null, 所属师门.徒弟出师经验(角色数据));
				所属师门.师父数据.网络连接?.发送封包(new 徒弟成功出师
				{
					对象编号 = 地图编号
				});
				所属师门.移除徒弟(角色数据);
				角色数据.当前师门 = null;
				网络连接?.发送封包(new 徒弟成功出师
				{
					对象编号 = 地图编号
				});
				网络连接?.发送封包(new 清空师门信息());
				网络连接?.发送封包(new 同步师门信息
				{
					师门参数 = 师门参数
				});
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 提交出师申请, 错误: 自己不是徒弟."));
			}
		}
		else
		{
			网络连接.尝试断开连接(new Exception("错误操作: 提交出师申请, 错误: 自身等级不足."));
		}
	}

	public void 更改收徒推送(bool 收徒推送)
	{
	}

	public void 玩家申请交易(int 对象编号)
	{
		if (!对象死亡 && 摆摊状态 <= 0 && 交易状态 < 3)
		{
			if (当前等级 >= 30 || 本期特权 != 0)
			{
				玩家实例 value;
				if (对象编号 == 地图编号)
				{
					当前交易?.结束交易();
					网络连接.尝试断开连接(new Exception("错误操作: 玩家申请交易. 错误: 不能交易自己"));
				}
				else if (!地图处理网关.玩家对象表.TryGetValue(对象编号, out value))
				{
					当前交易?.结束交易();
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 5635
					});
				}
				else if (当前地图 != value.当前地图)
				{
					当前交易?.结束交易();
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 5636
					});
				}
				else if (网格距离(value) <= 12)
				{
					if (!value.对象死亡 && value.摆摊状态 == 0 && value.交易状态 < 3)
					{
						当前交易?.结束交易();
						value.当前交易?.结束交易();
						当前交易 = (value.当前交易 = new 玩家交易(this, value));
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 5633
						});
					}
					else
					{
						当前交易?.结束交易();
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 5637
						});
					}
				}
				else
				{
					当前交易?.结束交易();
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 5636
					});
				}
			}
			else
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 65538
				});
			}
		}
		else
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
	}

	public void 玩家同意交易(int 对象编号)
	{
		if (对象死亡 || 摆摊状态 != 0 || 交易状态 != 2)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
		else if (当前等级 < 30 && 本期特权 == 0)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 65538
			});
		}
		else if (对象编号 != 地图编号)
		{
			if (地图处理网关.玩家对象表.TryGetValue(对象编号, out var value))
			{
				if (当前地图 != value.当前地图)
				{
					当前交易?.结束交易();
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 5636
					});
				}
				else if (网格距离(value) <= 12)
				{
					if (value.对象死亡 || value.摆摊状态 != 0 || value.交易状态 != 1)
					{
						当前交易?.结束交易();
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 5637
						});
					}
					else if (value == 当前交易.交易申请方 && this == value.当前交易.交易接收方)
					{
						当前交易.更改状态(3);
					}
					else
					{
						当前交易?.结束交易();
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 5634
						});
					}
				}
				else
				{
					当前交易?.结束交易();
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 5636
					});
				}
			}
			else
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 5635
				});
			}
		}
		else
		{
			当前交易?.结束交易();
			网络连接.尝试断开连接(new Exception("错误操作: 玩家申请交易. 错误: 不能交易自己"));
		}
	}

	public void 玩家结束交易()
	{
		当前交易?.结束交易();
	}

	public void 玩家放入金币(int 金币数量)
	{
		if (金币数量 <= 0)
		{
			return;
		}
		if (交易状态 == 3)
		{
			if (当前地图 == 当前交易.对方玩家(this).当前地图)
			{
				if (网格距离(当前交易.对方玩家(this)) <= 12)
				{
					if (金币数量 > 0 && this.金币数量 >= 金币数量 + (int)Math.Ceiling((float)金币数量 * 0.04f))
					{
						if (当前交易.金币重复(this))
						{
							当前交易?.结束交易();
							网络连接.尝试断开连接(new Exception("错误操作: 玩家放入金币. 错误: 重复放入金币"));
						}
						else
						{
							当前交易.放入金币(this, 金币数量);
						}
					}
					else
					{
						当前交易?.结束交易();
						网络连接.尝试断开连接(new Exception("错误操作: 玩家放入金币. 错误: 金币数量错误"));
					}
				}
				else
				{
					当前交易?.结束交易();
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 5636
					});
				}
			}
			else
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 5636
				});
			}
		}
		else
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
	}

	public void 玩家放入物品(byte 放入位置, byte 放入物品, byte 背包类型, byte 物品位置)
	{
		if (交易状态 == 3)
		{
			if (当前地图 != 当前交易.对方玩家(this).当前地图)
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 5636
				});
			}
			else if (网格距离(当前交易.对方玩家(this)) <= 12)
			{
				if (放入位置 < 6)
				{
					if (!当前交易.物品重复(this, 放入位置))
					{
						if (放入物品 == 1)
						{
							物品数据 v;
							if (背包类型 != 1)
							{
								当前交易?.结束交易();
								网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 背包类型错误"));
							}
							else if (!角色背包.TryGetValue(物品位置, out v))
							{
								当前交易?.结束交易();
								网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 物品数据错误"));
							}
							else if (v.是否绑定)
							{
								当前交易?.结束交易();
								网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 放入绑定物品"));
							}
							else if (当前交易.物品重复(this, v))
							{
								当前交易?.结束交易();
								网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 重复放入物品"));
							}
							else
							{
								当前交易.放入物品(this, v, 放入位置);
							}
						}
						else
						{
							当前交易?.结束交易();
							网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 禁止取回物品"));
						}
					}
					else
					{
						当前交易?.结束交易();
						网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 放入位置重复"));
					}
				}
				else
				{
					当前交易?.结束交易();
					网络连接.尝试断开连接(new Exception("错误操作: 玩家放入物品. 错误: 放入位置错误"));
				}
			}
			else
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 5636
				});
			}
		}
		else
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
	}

	public void 玩家锁定交易()
	{
		if (交易状态 != 3)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
		else if (当前地图 != 当前交易.对方玩家(this).当前地图)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5636
			});
		}
		else if (网格距离(当前交易.对方玩家(this)) > 12)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5636
			});
		}
		else
		{
			当前交易.更改状态(4, this);
		}
	}

	public void 玩家解锁交易()
	{
		if (交易状态 < 4)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
		else if (当前地图 != 当前交易.对方玩家(this).当前地图)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5636
			});
		}
		else if (网格距离(当前交易.对方玩家(this)) > 12)
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5636
			});
		}
		else
		{
			当前交易.更改状态(3);
		}
	}

	public void 玩家确认交易()
	{
		if (交易状态 == 4)
		{
			玩家实例 玩家;
			if (当前地图 != 当前交易.对方玩家(this).当前地图)
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 5636
				});
			}
			else if (网格距离(当前交易.对方玩家(this)) > 12)
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 5636
				});
			}
			else if (当前交易.对方状态(this) != 5)
			{
				当前交易.更改状态(5, this);
			}
			else if (当前交易.背包已满(out 玩家))
			{
				当前交易?.结束交易();
				当前交易.发送封包(new 游戏错误提示
				{
					错误代码 = 5639,
					第一参数 = 玩家.地图编号
				});
			}
			else
			{
				当前交易.更改状态(5, this);
				当前交易.交换物品();
			}
		}
		else
		{
			当前交易?.结束交易();
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 5634
			});
		}
	}

	public void 玩家准备摆摊()
	{
		if (!对象死亡 && 交易状态 < 3)
		{
			if (当前等级 < 30 && 本期特权 == 0)
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 65538
				});
			}
			else if (当前摊位 != null)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 2825
				});
			}
			else if (!当前地图.摆摊区内(当前坐标))
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 2818
				});
			}
			else if (当前地图[当前坐标].FirstOrDefault((地图对象 O) => O is 玩家实例 玩家实例2 && 玩家实例2.当前摊位 != null) == null)
			{
				当前摊位 = new 玩家摊位();
				发送封包(new 摆摊状态改变
				{
					对象编号 = 地图编号,
					摊位状态 = 1
				});
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 2819
				});
			}
		}
	}

	public void 玩家重整摊位()
	{
		if (摆摊状态 == 2)
		{
			当前摊位.摊位状态 = 1;
			发送封包(new 摆摊状态改变
			{
				对象编号 = 地图编号,
				摊位状态 = 摆摊状态
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2817
			});
		}
	}

	public void 玩家开始摆摊()
	{
		if (摆摊状态 == 1)
		{
			if (当前等级 < 30 && 本期特权 == 0)
			{
				当前交易?.结束交易();
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 65538
				});
			}
			else if (当前摊位.物品总价() + 金币数量 <= int.MaxValue)
			{
				当前摊位.摊位状态 = 2;
				发送封包(new 摆摊状态改变
				{
					对象编号 = 地图编号,
					摊位状态 = 摆摊状态
				});
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 2827
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2817
			});
		}
	}

	public void 玩家收起摊位()
	{
		if (摆摊状态 != 0)
		{
			当前摊位 = null;
			发送封包(new 摆摊状态改变
			{
				对象编号 = 地图编号,
				摊位状态 = 摆摊状态
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2817
			});
		}
	}

	public void 放入摊位物品(byte 放入位置, byte 背包类型, byte 物品位置, ushort 物品数量, int 物品价格)
	{
		if (物品价格 <= 0 || 物品数量 <= 0)
		{
			return;
		}
		if (摆摊状态 == 1)
		{
			if (放入位置 < 10)
			{
				if (当前摊位.摊位物品.ContainsKey(放入位置))
				{
					网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 重复放入位置"));
				}
				else if (背包类型 == 1)
				{
					物品数据 v;
					if (物品价格 < 100)
					{
						网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 物品价格错误"));
					}
					else if (角色背包.TryGetValue(物品位置, out v))
					{
						if (当前摊位.摊位物品.Values.FirstOrDefault((物品数据 O) => O.物品位置.V == 物品位置) != null)
						{
							网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 重复放入物品"));
							return;
						}
						if (v.是否绑定)
						{
							网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 放入绑定物品"));
							return;
						}
						if (物品数量 > ((!v.能否堆叠) ? 1 : v.当前持久.V))
						{
							网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 物品数量错误"));
							return;
						}
						当前摊位.摊位物品.Add(放入位置, v);
						当前摊位.物品数量.Add(v, 物品数量);
						当前摊位.物品单价.Add(v, 物品价格);
						网络连接?.发送封包(new 添加摆摊物品
						{
							放入位置 = 放入位置,
							背包类型 = 背包类型,
							物品位置 = 物品位置,
							物品数量 = 物品数量,
							物品价格 = 物品价格
						});
					}
					else
					{
						网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 选中物品为空"));
					}
				}
				else
				{
					网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 背包类型错误"));
				}
			}
			else
			{
				网络连接.尝试断开连接(new Exception("错误操作: 放入摊位物品, 错误: 放入位置错误"));
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2817
			});
		}
	}

	public void 取回摊位物品(byte 取回位置)
	{
		if (摆摊状态 == 1)
		{
			if (!当前摊位.摊位物品.TryGetValue(取回位置, out var value))
			{
				网络连接.尝试断开连接(new Exception("错误操作: 取回摊位物品, 错误: 选中物品为空"));
				return;
			}
			当前摊位.物品单价.Remove(value);
			当前摊位.物品数量.Remove(value);
			当前摊位.摊位物品.Remove(取回位置);
			网络连接?.发送封包(new 移除摆摊物品
			{
				取回位置 = 取回位置
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2817
			});
		}
	}

	public void 更改摊位名字(string 摊位名字)
	{
		if (摆摊状态 == 1)
		{
			当前摊位.摊位名字 = 摊位名字;
			发送封包(new 变更摊位名字
			{
				对象编号 = 地图编号,
				摊位名字 = 摊位名字
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2817
			});
		}
	}

	public void 升级摊位外观(byte 外观编号)
	{
	}

	public void 玩家打开摊位(int 对象编号)
	{
		if (!地图处理网关.玩家对象表.TryGetValue(对象编号, out var value))
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2828
			});
		}
		else if (value.摆摊状态 == 2)
		{
			网络连接?.发送封包(new 同步摊位数据
			{
				对象编号 = value.地图编号,
				字节数据 = value.当前摊位.摊位描述()
			});
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2828
			});
		}
	}

	public void 购买摊位物品(int 对象编号, byte 物品位置, ushort 购买数量)
	{
		if (购买数量 <= 0)
		{
			return;
		}
		if (地图处理网关.玩家对象表.TryGetValue(对象编号, out var value))
		{
			物品数据 value2;
			if (value.摆摊状态 != 2)
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 2828
				});
			}
			else if (value.当前摊位.摊位物品.TryGetValue(物品位置, out value2))
			{
				if (value.当前摊位.物品数量[value2] >= 购买数量)
				{
					if (金币数量 < value.当前摊位.物品单价[value2] * 购买数量)
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 2561
						});
						return;
					}
					byte b = byte.MaxValue;
					byte b2 = 0;
					while (b2 < 背包大小)
					{
						if (角色背包.ContainsKey(b2))
						{
							b2 = (byte)(b2 + 1);
							continue;
						}
						b = b2;
						break;
					}
					if (b != byte.MaxValue)
					{
						int num = value.当前摊位.物品单价[value2] * 购买数量;
						金币数量 -= num;
						角色数据.转出金币.V += num;
						value.金币数量 += (int)((float)num * 0.95f);
						if ((value.当前摊位.物品数量[value2] -= 购买数量) <= 0)
						{
							value.角色背包.Remove(value2.物品位置.V);
							value.网络连接?.发送封包(new 删除玩家物品
							{
								背包类型 = 1,
								物品位置 = value2.物品位置.V
							});
						}
						else
						{
							value2.当前持久.V -= 购买数量;
							value.网络连接?.发送封包(new 玩家物品变动
							{
								物品描述 = value2.字节描述()
							});
						}
						if (value.当前摊位.物品数量[value2] <= 0)
						{
							角色背包[b] = value2;
							value2.物品位置.V = b;
							value2.物品容器.V = 1;
						}
						else
						{
							角色背包[b] = new 物品数据(value2.物品模板, value2.生成来源.V, 1, b, 购买数量);
						}
						网络连接?.发送封包(new 玩家物品变动
						{
							物品描述 = 角色背包[b].字节描述()
						});
						网络连接?.发送封包(new 购入摊位物品
						{
							对象编号 = value.地图编号,
							物品位置 = 物品位置,
							剩余数量 = value.当前摊位.物品数量[value2]
						});
						value.网络连接?.发送封包(new 售出摊位物品
						{
							物品位置 = 物品位置,
							售出数量 = 购买数量,
							售出收益 = (int)((float)num * 0.95f)
						});
						主程.添加系统日志($"[{对象名字}][{当前等级}级] 购买了 [{value.对象名字}][{value.当前等级}级] 的摊位物品[{角色背包[b]}] * {购买数量}, 花费金币[{num}]");
						if (value.当前摊位.物品数量[value2] <= 0)
						{
							value.当前摊位.摊位物品.Remove(物品位置);
							value.当前摊位.物品单价.Remove(value2);
							value.当前摊位.物品数量.Remove(value2);
						}
						if (value.当前摊位.物品数量.Count <= 0)
						{
							value.玩家收起摊位();
						}
					}
					else
					{
						网络连接?.发送封包(new 游戏错误提示
						{
							错误代码 = 1793
						});
					}
				}
				else
				{
					网络连接?.发送封包(new 游戏错误提示
					{
						错误代码 = 2830
					});
				}
			}
			else
			{
				网络连接?.发送封包(new 游戏错误提示
				{
					错误代码 = 2824
				});
			}
		}
		else
		{
			网络连接?.发送封包(new 游戏错误提示
			{
				错误代码 = 2828
			});
		}
	}

	public byte[] 玩家属性描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		for (byte b = 0; b <= 100; b = (byte)(b + 1))
		{
			if (Enum.TryParse<游戏对象属性>(b.ToString(), out var result) && Enum.IsDefined(typeof(游戏对象属性), result))
			{
				binaryWriter.Write(b);
				binaryWriter.Write(this[result]);
				binaryWriter.Write(new byte[2]);
			}
			else
			{
				binaryWriter.Write(b);
				binaryWriter.Write(new byte[6]);
			}
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部技能描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (技能数据 value in 主体技能表.Values)
		{
			binaryWriter.Write(value.技能编号.V);
			binaryWriter.Write(value.铭文编号);
			binaryWriter.Write(value.技能等级.V);
			binaryWriter.Write(value.技能经验.V);
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部冷却描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (KeyValuePair<int, DateTime> item in 冷却记录)
		{
			if (!(主程.当前时间 >= item.Value))
			{
				binaryWriter.Write(item.Key);
				binaryWriter.Write((int)(item.Value - 主程.当前时间).TotalMilliseconds);
			}
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部Buff描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (Buff数据 value in Buff列表.Values)
		{
			binaryWriter.Write(value.Buff编号.V);
			binaryWriter.Write((int)value.Buff编号.V);
			binaryWriter.Write(value.当前层数.V);
			binaryWriter.Write((int)value.剩余时间.V.TotalMilliseconds);
			binaryWriter.Write((int)value.持续时间.V.TotalMilliseconds);
		}
		return memoryStream.ToArray();
	}

	public byte[] 快捷栏位描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (KeyValuePair<byte, 技能数据> item in 快捷栏位)
		{
			binaryWriter.Write(item.Key);
			binaryWriter.Write(item.Value?.技能编号.V ?? 0);
			binaryWriter.Write(value: false);
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部货币描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		for (int i = 0; i <= 19; i++)
		{
			binaryWriter.Seek(i * 48, SeekOrigin.Begin);
			binaryWriter.Write(角色数据.角色货币[(游戏货币)i]);
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部称号描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(当前称号);
		binaryWriter.Write((byte)称号列表.Count);
		foreach (KeyValuePair<byte, DateTime> item in 称号列表)
		{
			binaryWriter.Write(item.Key);
			binaryWriter.Write((!(item.Value == DateTime.MaxValue)) ? ((uint)(item.Value - 主程.当前时间).TotalMinutes) : uint.MaxValue);
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部物品描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (装备数据 item in 角色装备.Values.ToList())
		{
			if (item != null)
			{
				binaryWriter.Write(item.字节描述());
			}
		}
		foreach (物品数据 item2 in 角色背包.Values.ToList())
		{
			if (item2 != null)
			{
				binaryWriter.Write(item2.字节描述());
			}
		}
		foreach (物品数据 item3 in 角色仓库.Values.ToList())
		{
			if (item3 != null)
			{
				binaryWriter.Write(item3.字节描述());
			}
		}
		foreach (物品数据 item4 in 角色资源背包.Values.ToList())
		{
			if (item4 != null)
			{
				binaryWriter.Write(item4.字节描述());
			}
		}
		return memoryStream.ToArray();
	}

	public byte[] 全部邮件描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)全部邮件.Count);
		foreach (邮件数据 item in 全部邮件)
		{
			binaryWriter.Write(item.邮件检索描述());
		}
		return memoryStream.ToArray();
	}

	public byte[] 背包物品描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (物品数据 item in 角色背包.Values.ToList())
		{
			if (item != null)
			{
				binaryWriter.Write(item.字节描述());
			}
		}
		return memoryStream.ToArray();
	}

	public byte[] 仓库物品描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (物品数据 item in 角色仓库.Values.ToList())
		{
			if (item != null)
			{
				binaryWriter.Write(item.字节描述());
			}
		}
		return memoryStream.ToArray();
	}

	public byte[] 装备物品描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (装备数据 item in 角色装备.Values.ToList())
		{
			if (item != null)
			{
				binaryWriter.Write(item.字节描述());
			}
		}
		return memoryStream.ToArray();
	}

	public byte[] 玛法特权描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(角色数据.预定特权.V);
		binaryWriter.Write(本期特权);
		binaryWriter.Write((本期特权 != 0) ? 计算类.时间转换(本期日期) : 0);
		binaryWriter.Write((本期特权 != 0) ? 本期记录 : 0u);
		binaryWriter.Write(上期特权);
		binaryWriter.Write((上期特权 != 0) ? 计算类.时间转换(上期日期) : 0);
		binaryWriter.Write((上期特权 != 0) ? 上期记录 : 0u);
		binaryWriter.Write((byte)5);
		for (byte b = 1; b <= 5; b = (byte)(b + 1))
		{
			binaryWriter.Write(b);
			binaryWriter.Write(剩余特权.TryGetValue(b, out var v) ? v : 0);
		}
		return memoryStream.ToArray();
	}

	public byte[] 社交列表描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)好友列表.Count);
		binaryWriter.Write((byte)(偶像列表.Count + 仇人列表.Count));
		foreach (角色数据 item in 偶像列表)
		{
			binaryWriter.Write(item.数据索引.V);
			byte[] array = new byte[69];
			byte[] array2 = item.名字描述();
			Buffer.BlockCopy(array2, 0, array, 0, array2.Length);
			binaryWriter.Write(array);
			binaryWriter.Write((byte)item.角色职业.V);
			binaryWriter.Write((byte)item.角色性别.V);
			binaryWriter.Write((byte)((item.网络连接 == null) ? 3u : 0u));
			binaryWriter.Write(0u);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)(好友列表.Contains(item) ? 1u : 0u));
		}
		foreach (角色数据 item2 in 仇人列表)
		{
			binaryWriter.Write(item2.数据索引.V);
			byte[] array3 = new byte[69];
			byte[] array4 = item2.名字描述();
			Buffer.BlockCopy(array4, 0, array3, 0, array4.Length);
			binaryWriter.Write(array3);
			binaryWriter.Write((byte)item2.角色职业.V);
			binaryWriter.Write((byte)item2.角色性别.V);
			binaryWriter.Write((byte)((item2.网络连接 == null) ? 3u : 0u));
			binaryWriter.Write(0u);
			binaryWriter.Write((byte)21);
			binaryWriter.Write((byte)0);
		}
		return memoryStream.ToArray();
	}

	public byte[] 社交屏蔽描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)黑名单表.Count);
		foreach (角色数据 item in 黑名单表)
		{
			binaryWriter.Write(item.数据索引.V);
		}
		return memoryStream.ToArray();
	}

	static 玩家实例()
	{
	}
}
