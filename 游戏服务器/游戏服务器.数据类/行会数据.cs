using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using 游戏服务器.地图类;
using 游戏服务器.网络类;

namespace 游戏服务器.数据类;

[数据快速检索(检索字段 = "行会名字")]
public sealed class 行会数据 : 游戏数据
{
	public readonly 数据监视器<角色数据> 行会会长;

	public readonly 数据监视器<DateTime> 创建日期;

	public readonly 数据监视器<string> 行会名字;

	public readonly 数据监视器<string> 创建人名;

	public readonly 数据监视器<string> 行会宣言;

	public readonly 数据监视器<string> 行会公告;

	public readonly 数据监视器<byte> 行会等级;

	public readonly 数据监视器<int> 行会资金;

	public readonly 数据监视器<int> 粮食数量;

	public readonly 数据监视器<int> 木材数量;

	public readonly 数据监视器<int> 石材数量;

	public readonly 数据监视器<int> 铁矿数量;

	public readonly 数据监视器<int> 行会排名;

	public readonly 列表监视器<行会事记> 行会事记;

	public readonly 字典监视器<角色数据, 行会职位> 行会成员;

	public readonly 字典监视器<角色数据, DateTime> 行会禁言;

	public readonly 字典监视器<行会数据, DateTime> 结盟行会;

	public readonly 字典监视器<行会数据, DateTime> 敌对行会;

	public Dictionary<角色数据, DateTime> 申请列表;

	public Dictionary<角色数据, DateTime> 邀请列表;

	public Dictionary<行会数据, 外交申请> 结盟申请;

	public Dictionary<行会数据, DateTime> 解除申请;

	public int 行会编号 => 数据索引.V;

	public int 创建时间 => 计算类.时间转换(创建日期.V);

	public string 会长名字 => 行会会长.V.角色名字.V;

	public 角色数据 会长数据
	{
		get
		{
			return 行会会长.V;
		}
		set
		{
			if (行会会长.V != value)
			{
				行会会长.V = value;
			}
		}
	}

	public DateTime 清理时间 { get; set; }

	public override string ToString()
	{
		return 行会名字?.V;
	}

	public 行会数据()
	{
		申请列表 = new Dictionary<角色数据, DateTime>();
		邀请列表 = new Dictionary<角色数据, DateTime>();
		结盟申请 = new Dictionary<行会数据, 外交申请>();
		解除申请 = new Dictionary<行会数据, DateTime>();
	}

	public 行会数据(玩家实例 创建玩家, string 行会名字, string 行会宣言)
	{
		申请列表 = new Dictionary<角色数据, DateTime>();
		邀请列表 = new Dictionary<角色数据, DateTime>();
		结盟申请 = new Dictionary<行会数据, 外交申请>();
		解除申请 = new Dictionary<行会数据, DateTime>();
		this.行会名字.V = 行会名字;
		this.行会宣言.V = 行会宣言;
		行会公告.V = "祝大家游戏愉快.";
		行会会长.V = 创建玩家.角色数据;
		创建人名.V = 创建玩家.对象名字;
		行会成员.Add(创建玩家.角色数据, 行会职位.会长);
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.创建公会,
			第一参数 = 创建玩家.地图编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.加入公会,
			第一参数 = 创建玩家.地图编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		行会等级.V = 1;
		行会资金.V = 1000000;
		粮食数量.V = 1000000;
		木材数量.V = 1000000;
		石材数量.V = 1000000;
		铁矿数量.V = 1000000;
		创建日期.V = 主程.当前时间;
		游戏数据网关.行会数据表.添加数据(this, 分配索引: true);
		系统数据.数据.更新行会(this);
	}

	public void 清理数据()
	{
		if (!(主程.当前时间 > 清理时间))
		{
			return;
		}
		foreach (KeyValuePair<行会数据, DateTime> item in 结盟行会.ToList())
		{
			if (主程.当前时间 > item.Value)
			{
				结盟行会.Remove(item.Key);
				item.Key.结盟行会.Remove(this);
				发送封包(new 删除外交公告
				{
					外交类型 = 1,
					行会编号 = item.Key.行会编号
				});
				item.Key.发送封包(new 删除外交公告
				{
					外交类型 = 1,
					行会编号 = 行会编号
				});
				网络服务网关.发送公告($"[{this}]和[{item.Key}]的行会盟约已经到期自动解除");
			}
		}
		foreach (KeyValuePair<行会数据, DateTime> item2 in 敌对行会.ToList())
		{
			if (主程.当前时间 > item2.Value)
			{
				敌对行会.Remove(item2.Key);
				item2.Key.敌对行会.Remove(this);
				发送封包(new 删除外交公告
				{
					外交类型 = 2,
					行会编号 = item2.Key.行会编号
				});
				item2.Key.发送封包(new 删除外交公告
				{
					外交类型 = 2,
					行会编号 = 行会编号
				});
				网络服务网关.发送公告($"[{this}]和[{item2.Key}]的行会敌对已经到期自动解除");
			}
		}
		foreach (KeyValuePair<角色数据, DateTime> item3 in 申请列表.ToList())
		{
			if (主程.当前时间 > item3.Value)
			{
				申请列表.Remove(item3.Key);
			}
		}
		foreach (KeyValuePair<角色数据, DateTime> item4 in 邀请列表.ToList())
		{
			if (主程.当前时间 > item4.Value)
			{
				邀请列表.Remove(item4.Key);
			}
		}
		foreach (KeyValuePair<行会数据, DateTime> item5 in 解除申请.ToList())
		{
			if (主程.当前时间 > item5.Value)
			{
				解除申请.Remove(item5.Key);
			}
		}
		foreach (KeyValuePair<行会数据, 外交申请> item6 in 结盟申请.ToList())
		{
			if (主程.当前时间 > item6.Value.申请时间)
			{
				结盟申请.Remove(item6.Key);
			}
		}
		清理时间 = 主程.当前时间.AddSeconds(1.0);
	}

	public void 解散行会()
	{
		foreach (KeyValuePair<DateTime, 行会数据> item in 系统数据.数据.申请行会.ToList())
		{
			if (item.Value == this)
			{
				系统数据.数据.申请行会.Remove(item.Key);
			}
		}
		发送封包(new 脱离行会应答
		{
			脱离方式 = 2
		});
		foreach (角色数据 key in 行会成员.Keys)
		{
			key.当前行会 = null;
			key.网络连接?.发送封包(new 同步对象行会
			{
				对象编号 = key.角色编号
			});
		}
		if (行会排名.V > 0)
		{
			系统数据.数据.行会人数排名.RemoveAt(行会排名.V - 1);
			for (int i = 行会排名.V - 1; i < 系统数据.数据.行会人数排名.Count; i++)
			{
				系统数据.数据.行会人数排名[i].行会排名.V = i + 1;
			}
		}
		行会成员.Clear();
		行会禁言.Clear();
		删除数据();
	}

	public void 发送封包(游戏封包 封包)
	{
		foreach (角色数据 key in 行会成员.Keys)
		{
			key.网络连接?.发送封包(封包);
		}
	}

	public void 添加成员(角色数据 成员, 行会职位 职位 = 行会职位.会员)
	{
		行会成员.Add(成员, 职位);
		成员.当前行会 = this;
		发送封包(new 行会加入成员
		{
			对象编号 = 成员.角色编号,
			对象名字 = 成员.角色名字.V,
			对象职位 = 7,
			对象等级 = 成员.角色等级,
			对象职业 = (byte)成员.角色职业.V,
			当前地图 = (byte)成员.当前地图.V
		});
		if (成员.网络连接 == null)
		{
			发送封包(new 同步会员信息
			{
				对象编号 = 成员.角色编号,
				对象信息 = 计算类.时间转换(成员.离线日期.V)
			});
		}
		成员.网络连接?.发送封包(new 行会信息公告
		{
			字节数据 = 行会信息描述()
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.加入公会,
			第一参数 = 成员.角色编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		if (地图处理网关.玩家对象表.TryGetValue(成员.角色编号, out var value))
		{
			value.发送封包(new 同步对象行会
			{
				对象编号 = 成员.角色编号,
				行会编号 = 行会编号
			});
		}
		系统数据.数据.更新行会(this);
	}

	public void 退出行会(角色数据 成员)
	{
		行会成员.Remove(成员);
		行会禁言.Remove(成员);
		成员.当前行会 = null;
		成员.网络连接?.发送封包(new 脱离行会应答
		{
			脱离方式 = 1
		});
		发送封包(new 脱离行会公告
		{
			对象编号 = 成员.角色编号
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.离开公会,
			第一参数 = 成员.角色编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		if (地图处理网关.玩家对象表.TryGetValue(成员.角色编号, out var value))
		{
			value.发送封包(new 同步对象行会
			{
				对象编号 = 成员.角色编号
			});
		}
		系统数据.数据.更新行会(this);
	}

	public void 逐出成员(角色数据 主事, 角色数据 成员)
	{
		if (行会成员.Remove(成员))
		{
			行会禁言.Remove(成员);
			成员.当前行会 = null;
			成员.网络连接?.发送封包(new 脱离行会应答
			{
				脱离方式 = 2
			});
			发送封包(new 脱离行会公告
			{
				对象编号 = 成员.角色编号
			});
			添加事记(new 行会事记
			{
				事记类型 = 事记类型.逐出公会,
				第一参数 = 成员.角色编号,
				第二参数 = 主事.角色编号,
				事记时间 = 计算类.时间转换(主程.当前时间)
			});
			if (地图处理网关.玩家对象表.TryGetValue(成员.角色编号, out var value))
			{
				value.发送封包(new 同步对象行会
				{
					对象编号 = 成员.角色编号
				});
			}
			系统数据.数据.更新行会(this);
		}
	}

	public void 更改职位(角色数据 主事, 角色数据 成员, 行会职位 职位)
	{
		行会职位 行会职位3 = (行会成员[成员] = 职位);
		行会职位 行会职位4 = 行会职位3;
		行会成员[成员] = 职位;
		发送封包(new 变更职位公告
		{
			对象编号 = 成员.角色编号,
			对象职位 = (byte)职位
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.变更职位,
			第一参数 = 主事.角色编号,
			第二参数 = 成员.角色编号,
			第三参数 = (byte)行会职位4,
			第四参数 = (byte)职位,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
	}

	public void 更改宣言(角色数据 主事, string 宣言)
	{
		行会宣言.V = 宣言;
		主事.网络连接?.发送封包(new 社交错误提示
		{
			错误编号 = 6747
		});
	}

	public void 更改公告(string 公告)
	{
		行会公告.V = 公告;
		发送封包(new 变更行会公告
		{
			字节数据 = Encoding.UTF8.GetBytes(公告 + "\0")
		});
	}

	public void 转移会长(角色数据 会长, 角色数据 成员)
	{
		行会会长.V = 成员;
		行会成员[会长] = 行会职位.会员;
		行会成员[成员] = 行会职位.会长;
		发送封包(new 会长传位公告
		{
			当前编号 = 会长.角色编号,
			传位编号 = 成员.角色编号
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.会长传位,
			第一参数 = 会长.角色编号,
			第二参数 = 成员.角色编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
	}

	public void 成员禁言(角色数据 主事, 角色数据 成员, byte 禁言状态)
	{
		if (禁言状态 == 2 && 行会禁言.Remove(成员))
		{
			发送封包(new 行会禁言公告
			{
				对象编号 = 成员.角色编号,
				禁言状态 = 2
			});
		}
		else if (禁言状态 == 1)
		{
			行会禁言[成员] = 主程.当前时间;
			发送封包(new 行会禁言公告
			{
				对象编号 = 成员.角色编号,
				禁言状态 = 1
			});
		}
		else
		{
			主事.网络连接?.发送封包(new 社交错误提示
			{
				错误编号 = 6680
			});
		}
	}

	public void 申请结盟(角色数据 主事, 行会数据 行会, byte 时间参数)
	{
		主事.网络连接.发送封包(new 申请结盟应答
		{
			行会编号 = 行会.行会编号
		});
		if (!行会.结盟申请.ContainsKey(this))
		{
			行会.行会提醒(行会职位.副长, 2);
		}
		行会.结盟申请[this] = new 外交申请
		{
			外交时间 = 时间参数,
			申请时间 = 主程.当前时间.AddHours(10.0)
		};
	}

	public void 行会敌对(行会数据 行会, byte 时间参数)
	{
		字典监视器<行会数据, DateTime> 字典监视器2 = 敌对行会;
		字典监视器<行会数据, DateTime> 字典监视器3 = 行会.敌对行会;
		DateTime dateTime2 = (字典监视器3[this] = 主程.当前时间.AddDays(时间参数 switch
		{
			2 => 3, 
			1 => 1, 
			_ => 7, 
		}));
		DateTime dateTime3 = dateTime2;
		dateTime2 = (字典监视器2[行会] = dateTime3);
		发送封包(new 添加外交公告
		{
			外交类型 = 2,
			行会编号 = 行会.行会编号,
			行会名字 = 行会.行会名字.V,
			行会等级 = 行会.行会等级.V,
			行会人数 = (byte)行会.行会成员.Count,
			外交时间 = (int)(敌对行会[行会] - 主程.当前时间).TotalSeconds
		});
		行会.发送封包(new 添加外交公告
		{
			外交类型 = 2,
			行会编号 = 行会编号,
			行会名字 = 行会名字.V,
			行会等级 = 行会等级.V,
			行会人数 = (byte)行会成员.Count,
			外交时间 = (int)(行会.敌对行会[this] - 主程.当前时间).TotalSeconds
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.行会敌对,
			第一参数 = 行会编号,
			第二参数 = 行会.行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		行会.添加事记(new 行会事记
		{
			事记类型 = 事记类型.行会敌对,
			第一参数 = 行会.行会编号,
			第二参数 = 行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
	}

	public void 行会结盟(行会数据 行会)
	{
		字典监视器<行会数据, DateTime> obj = 结盟行会;
		DateTime dateTime2 = (行会.结盟行会[this] = 主程.当前时间.AddDays((结盟申请[行会].外交时间 == 1) ? 1 : ((结盟申请[行会].外交时间 == 2) ? 3 : 7)));
		DateTime dateTime3 = dateTime2;
		dateTime2 = (obj[行会] = dateTime3);
		发送封包(new 添加外交公告
		{
			外交类型 = 1,
			行会名字 = 行会.行会名字.V,
			行会编号 = 行会.行会编号,
			行会等级 = 行会.行会等级.V,
			行会人数 = (byte)行会.行会成员.Count,
			外交时间 = (int)(结盟行会[行会] - 主程.当前时间).TotalSeconds
		});
		行会.发送封包(new 添加外交公告
		{
			外交类型 = 1,
			行会名字 = 行会名字.V,
			行会编号 = 行会编号,
			行会等级 = 行会等级.V,
			行会人数 = (byte)行会成员.Count,
			外交时间 = (int)(行会.结盟行会[this] - 主程.当前时间).TotalSeconds
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.行会结盟,
			第一参数 = 行会编号,
			第二参数 = 行会.行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		行会.添加事记(new 行会事记
		{
			事记类型 = 事记类型.行会结盟,
			第一参数 = 行会.行会编号,
			第二参数 = 行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
	}

	public void 解除结盟(角色数据 主事, 行会数据 行会)
	{
		结盟行会.Remove(行会);
		行会.结盟行会.Remove(this);
		发送封包(new 删除外交公告
		{
			外交类型 = 1,
			行会编号 = 行会.行会编号
		});
		行会.发送封包(new 删除外交公告
		{
			外交类型 = 1,
			行会编号 = 行会编号
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.取消结盟,
			第一参数 = 行会编号,
			第二参数 = 行会.行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		行会.添加事记(new 行会事记
		{
			事记类型 = 事记类型.取消结盟,
			第一参数 = 行会.行会编号,
			第二参数 = 行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		主事.网络连接?.发送封包(new 社交错误提示
		{
			错误编号 = 6812
		});
	}

	public void 申请解敌(角色数据 主事, 行会数据 敌对行会)
	{
		主事.网络连接.发送封包(new 社交错误提示
		{
			错误编号 = 6829
		});
		foreach (KeyValuePair<角色数据, 行会职位> item in 敌对行会.行会成员)
		{
			if (item.Value <= 行会职位.副长)
			{
				item.Key.网络连接?.发送封包(new 解除敌对列表
				{
					申请类型 = 1,
					行会编号 = 行会编号
				});
			}
		}
		敌对行会.解除申请[this] = 主程.当前时间.AddHours(10.0);
	}

	public void 解除敌对(行会数据 行会)
	{
		敌对行会.Remove(行会);
		行会.敌对行会.Remove(this);
		发送封包(new 解除敌对列表
		{
			申请类型 = 2,
			行会编号 = 行会.行会编号
		});
		发送封包(new 删除外交公告
		{
			外交类型 = 2,
			行会编号 = 行会.行会编号
		});
		行会.发送封包(new 删除外交公告
		{
			外交类型 = 2,
			行会编号 = 行会编号
		});
		添加事记(new 行会事记
		{
			事记类型 = 事记类型.取消敌对,
			第一参数 = 行会编号,
			第二参数 = 行会.行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
		行会.添加事记(new 行会事记
		{
			事记类型 = 事记类型.取消敌对,
			第一参数 = 行会.行会编号,
			第二参数 = 行会编号,
			事记时间 = 计算类.时间转换(主程.当前时间)
		});
	}

	public void 发送邮件(行会职位 职位, string 标题, string 内容)
	{
		foreach (KeyValuePair<角色数据, 行会职位> item in 行会成员)
		{
			if (item.Value <= 职位)
			{
				item.Key.发送邮件(new 邮件数据(null, 标题, 内容, null));
			}
		}
	}

	public void 添加事记(行会事记 事记)
	{
		行会事记.Insert(0, 事记);
		发送封包(new 添加公会事记
		{
			事记类型 = (byte)事记.事记类型,
			第一参数 = 事记.第一参数,
			第二参数 = 事记.第二参数,
			第三参数 = 事记.第三参数,
			第四参数 = 事记.第四参数,
			事记时间 = 事记.事记时间
		});
		while (行会事记.Count > 10)
		{
			行会事记.RemoveAt(行会事记.Count - 1);
		}
	}

	public void 行会提醒(行会职位 职位, byte 提醒类型)
	{
		foreach (KeyValuePair<角色数据, 行会职位> item in 行会成员)
		{
			if (item.Value <= 职位 && item.Key.角色在线(out var 网络))
			{
				网络.发送封包(new 发送行会通知
				{
					提醒类型 = 提醒类型
				});
			}
		}
	}

	public byte[] 行会检索描述()
	{
		using MemoryStream memoryStream = new MemoryStream(new byte[229]);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(行会编号);
		byte[] array = new byte[25];
		Encoding.UTF8.GetBytes(行会名字.V).CopyTo(array, 0);
		binaryWriter.Write(array);
		binaryWriter.Write(行会等级.V);
		binaryWriter.Write((byte)行会成员.Count);
		binaryWriter.Write(0);
		array = new byte[32];
		Encoding.UTF8.GetBytes(会长名字).CopyTo(array, 0);
		binaryWriter.Write(array);
		array = new byte[32];
		Encoding.UTF8.GetBytes(创建人名.V).CopyTo(array, 0);
		binaryWriter.Write(array);
		binaryWriter.Write(创建时间);
		array = new byte[101];
		Encoding.UTF8.GetBytes(行会宣言.V).CopyTo(array, 0);
		binaryWriter.Write(array);
		binaryWriter.Write(new byte[17]);
		binaryWriter.Write(new byte[8]);
		return memoryStream.ToArray();
	}

	public byte[] 行会信息描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(行会编号);
		binaryWriter.Write(Encoding.UTF8.GetBytes(行会名字.V));
		binaryWriter.Seek(29, SeekOrigin.Begin);
		binaryWriter.Write(行会等级.V);
		binaryWriter.Write((byte)行会成员.Count);
		binaryWriter.Write(行会资金.V);
		binaryWriter.Write(创建时间);
		binaryWriter.Seek(43, SeekOrigin.Begin);
		binaryWriter.Write(Encoding.UTF8.GetBytes(会长名字));
		binaryWriter.Seek(75, SeekOrigin.Begin);
		binaryWriter.Write(Encoding.UTF8.GetBytes(创建人名.V));
		binaryWriter.Seek(107, SeekOrigin.Begin);
		binaryWriter.Write(Encoding.UTF8.GetBytes(行会公告.V));
		binaryWriter.Seek(4258, SeekOrigin.Begin);
		binaryWriter.Write(粮食数量.V);
		binaryWriter.Write(木材数量.V);
		binaryWriter.Write(石材数量.V);
		binaryWriter.Write(铁矿数量.V);
		binaryWriter.Write(402);
		binaryWriter.Seek(7960, SeekOrigin.Begin);
		foreach (KeyValuePair<角色数据, 行会职位> item in 行会成员)
		{
			binaryWriter.Write(item.Key.角色编号);
			byte[] array = new byte[32];
			Encoding.UTF8.GetBytes(item.Key.角色名字.V).CopyTo(array, 0);
			binaryWriter.Write(array);
			binaryWriter.Write((byte)item.Value);
			binaryWriter.Write(item.Key.角色等级);
			binaryWriter.Write((byte)item.Key.角色职业.V);
			binaryWriter.Write(item.Key.当前地图.V);
			binaryWriter.Write((!item.Key.角色在线(out var _)) ? 计算类.时间转换(item.Key.离线日期.V) : 0);
			binaryWriter.Write(0);
			binaryWriter.Write(行会禁言.ContainsKey(item.Key));
		}
		binaryWriter.Seek(330, SeekOrigin.Begin);
		binaryWriter.Write((byte)Math.Min(10, 行会事记.Count));
		for (int i = 0; i < 10 && i < 行会事记.Count; i++)
		{
			binaryWriter.Write((byte)行会事记[i].事记类型);
			binaryWriter.Write(行会事记[i].第一参数);
			binaryWriter.Write(行会事记[i].第二参数);
			binaryWriter.Write(行会事记[i].第三参数);
			binaryWriter.Write(行会事记[i].第四参数);
			binaryWriter.Write(行会事记[i].事记时间);
		}
		binaryWriter.Seek(1592, SeekOrigin.Begin);
		binaryWriter.Write((byte)结盟行会.Count);
		foreach (KeyValuePair<行会数据, DateTime> item2 in 结盟行会)
		{
			binaryWriter.Write((byte)1);
			binaryWriter.Write(item2.Key.行会编号);
			binaryWriter.Write(计算类.时间转换(item2.Value));
			binaryWriter.Write(item2.Key.行会等级.V);
			binaryWriter.Write((byte)item2.Key.行会成员.Count);
			byte[] array2 = new byte[25];
			Encoding.UTF8.GetBytes(item2.Key.行会名字.V).CopyTo(array2, 0);
			binaryWriter.Write(array2);
		}
		binaryWriter.Seek(1953, SeekOrigin.Begin);
		binaryWriter.Write((byte)敌对行会.Count);
		foreach (KeyValuePair<行会数据, DateTime> item3 in 敌对行会)
		{
			binaryWriter.Write((byte)2);
			binaryWriter.Write(item3.Key.行会编号);
			binaryWriter.Write(计算类.时间转换(item3.Value));
			binaryWriter.Write(item3.Key.行会等级.V);
			binaryWriter.Write((byte)item3.Key.行会成员.Count);
			byte[] array3 = new byte[25];
			Encoding.UTF8.GetBytes(item3.Key.行会名字.V).CopyTo(array3, 0);
			binaryWriter.Write(array3);
		}
		return memoryStream.ToArray();
	}

	public byte[] 入会申请描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)申请列表.Count);
		foreach (角色数据 key in 申请列表.Keys)
		{
			binaryWriter.Write(key.角色编号);
			byte[] array = new byte[32];
			Encoding.UTF8.GetBytes(key.角色名字.V).CopyTo(array, 0);
			binaryWriter.Write(array);
			binaryWriter.Write(key.角色等级);
			binaryWriter.Write(key.角色等级);
			binaryWriter.Write((!key.角色在线(out var _)) ? 计算类.时间转换(key.离线日期.V) : 计算类.时间转换(主程.当前时间));
		}
		return memoryStream.ToArray();
	}

	public byte[] 结盟申请描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)结盟申请.Count);
		foreach (KeyValuePair<行会数据, 外交申请> item in 结盟申请)
		{
			binaryWriter.Write(item.Key.行会编号);
			byte[] array = new byte[25];
			Encoding.UTF8.GetBytes(item.Key.行会名字.V).CopyTo(array, 0);
			binaryWriter.Write(array);
			binaryWriter.Write(item.Key.行会等级.V);
			binaryWriter.Write((byte)item.Key.行会成员.Count);
			array = new byte[32];
			Encoding.UTF8.GetBytes(item.Key.会长名字).CopyTo(array, 0);
			binaryWriter.Write(array);
			binaryWriter.Write(计算类.时间转换(item.Value.申请时间));
		}
		return memoryStream.ToArray();
	}

	public byte[] 解除申请描述()
	{
		using MemoryStream memoryStream = new MemoryStream(new byte[256]);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (KeyValuePair<行会数据, DateTime> item in 解除申请)
		{
			binaryWriter.Write(item.Key.行会编号);
		}
		return memoryStream.ToArray();
	}

	public byte[] 更多事记描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)Math.Max(0, 行会事记.Count - 10));
		for (int i = 10; i < 行会事记.Count; i++)
		{
			binaryWriter.Write((byte)行会事记[i].事记类型);
			binaryWriter.Write(行会事记[i].第一参数);
			binaryWriter.Write(行会事记[i].第二参数);
			binaryWriter.Write(行会事记[i].第三参数);
			binaryWriter.Write(行会事记[i].第四参数);
			binaryWriter.Write(行会事记[i].事记时间);
		}
		return memoryStream.ToArray();
	}

	static 行会数据()
	{
	}
}
