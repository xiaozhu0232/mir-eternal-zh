using System;
using System.Collections.Generic;
using System.IO;
using 游戏服务器.网络类;

namespace 游戏服务器.数据类;

public sealed class 队伍数据 : 游戏数据
{
	public readonly 数据监视器<byte> 分配方式;

	public readonly 数据监视器<角色数据> 队伍队长;

	public readonly 哈希监视器<角色数据> 队伍成员;

	public Dictionary<角色数据, DateTime> 申请列表;

	public Dictionary<角色数据, DateTime> 邀请列表;

	public int 队伍编号 => 数据索引.V;

	public int 队长编号 => 队伍队长.V.数据索引.V;

	public int 队员数量 => 队伍成员.Count;

	public byte 拾取方式 => 分配方式.V;

	public string 队长名字 => 队长数据.角色名字.V;

	public 角色数据 队长数据
	{
		get
		{
			return 队伍队长.V;
		}
		set
		{
			if (队伍队长.V.数据索引.V != value.数据索引.V)
			{
				队伍队长.V = value;
			}
		}
	}

	public override string ToString()
	{
		return 队长数据?.角色名字?.V;
	}

	public 队伍数据()
	{
		申请列表 = new Dictionary<角色数据, DateTime>();
		邀请列表 = new Dictionary<角色数据, DateTime>();
	}

	public 队伍数据(角色数据 创建角色, byte 分配方式)
	{
		申请列表 = new Dictionary<角色数据, DateTime>();
		邀请列表 = new Dictionary<角色数据, DateTime>();
		this.分配方式.V = 分配方式;
		队伍队长.V = 创建角色;
		队伍成员.Add(创建角色);
		游戏数据网关.队伍数据表.添加数据(this, 分配索引: true);
	}

	public override void 删除数据()
	{
		foreach (角色数据 item in 队伍成员)
		{
			item.当前队伍 = null;
		}
		base.删除数据();
	}

	public void 发送封包(游戏封包 P)
	{
		foreach (角色数据 item in 队伍成员)
		{
			item.网络连接?.发送封包(P);
		}
	}

	public byte[] 队伍描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(数据索引.V);
		binaryWriter.Write(队长数据.名字描述());
		binaryWriter.Seek(36, SeekOrigin.Begin);
		binaryWriter.Write(拾取方式);
		binaryWriter.Write(队长编号);
		binaryWriter.Write(11);
		binaryWriter.Write((ushort)队伍成员.Count);
		binaryWriter.Write(0);
		foreach (角色数据 item in 队伍成员)
		{
			binaryWriter.Write(队友描述(item));
		}
		return memoryStream.ToArray();
	}

	public byte[] 队友描述(角色数据 队友)
	{
		using MemoryStream memoryStream = new MemoryStream(new byte[39]);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(队友.数据索引.V);
		binaryWriter.Write(队友.名字描述());
		binaryWriter.Seek(36, SeekOrigin.Begin);
		binaryWriter.Write((byte)队友.角色性别.V);
		binaryWriter.Write((byte)队友.角色职业.V);
		binaryWriter.Write((byte)((队友.网络连接 == null) ? 3u : 0u));
		return memoryStream.ToArray();
	}

	static 队伍数据()
	{
	}
}
