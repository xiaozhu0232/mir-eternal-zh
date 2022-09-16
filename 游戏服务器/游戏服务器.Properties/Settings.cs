using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace 游戏服务器.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.2.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("8701")]
	public ushort 客户连接端口
	{
		get
		{
			return (ushort)this["客户连接端口"];
		}
		set
		{
			this["客户连接端口"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("6678")]
	public ushort 门票接收端口
	{
		get
		{
			return (ushort)this["门票接收端口"];
		}
		set
		{
			this["门票接收端口"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("100")]
	public ushort 封包限定数量
	{
		get
		{
			return (ushort)this["封包限定数量"];
		}
		set
		{
			this["封包限定数量"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("5")]
	public ushort 异常屏蔽时间
	{
		get
		{
			return (ushort)this["异常屏蔽时间"];
		}
		set
		{
			this["异常屏蔽时间"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("5")]
	public ushort 掉线判定时间
	{
		get
		{
			return (ushort)this["掉线判定时间"];
		}
		set
		{
			this["掉线判定时间"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("40")]
	public byte 游戏开放等级
	{
		get
		{
			return (byte)this["游戏开放等级"];
		}
		set
		{
			this["游戏开放等级"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("1")]
	public decimal 装备特修折扣
	{
		get
		{
			return (decimal)this["装备特修折扣"];
		}
		set
		{
			this["装备特修折扣"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("0")]
	public decimal 怪物额外爆率
	{
		get
		{
			return (decimal)this["怪物额外爆率"];
		}
		set
		{
			this["怪物额外爆率"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("1")]
	public decimal 怪物经验倍率
	{
		get
		{
			return (decimal)this["怪物经验倍率"];
		}
		set
		{
			this["怪物经验倍率"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("10")]
	public byte 减收益等级差
	{
		get
		{
			return (byte)this["减收益等级差"];
		}
		set
		{
			this["减收益等级差"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("0.1")]
	public decimal 收益减少比率
	{
		get
		{
			return (decimal)this["收益减少比率"];
		}
		set
		{
			this["收益减少比率"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("120")]
	public ushort 怪物诱惑时长
	{
		get
		{
			return (ushort)this["怪物诱惑时长"];
		}
		set
		{
			this["怪物诱惑时长"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("5")]
	public byte 物品清理时间
	{
		get
		{
			return (byte)this["物品清理时间"];
		}
		set
		{
			this["物品清理时间"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("3")]
	public byte 物品归属时间
	{
		get
		{
			return (byte)this["物品归属时间"];
		}
		set
		{
			this["物品归属时间"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue(".\\Database")]
	public string 游戏数据目录
	{
		get
		{
			return (string)this["游戏数据目录"];
		}
		set
		{
			this["游戏数据目录"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue(".\\Backup")]
	public string 数据备份目录
	{
		get
		{
			return (string)this["数据备份目录"];
		}
		set
		{
			this["数据备份目录"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string 系统公告内容
	{
		get
		{
			return (string)this["系统公告内容"];
		}
		set
		{
			this["系统公告内容"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("0")]
	public byte 新手扶持等级
	{
		get
		{
			return (byte)this["新手扶持等级"];
		}
		set
		{
			this["新手扶持等级"] = value;
		}
	}
}
