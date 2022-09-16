using System;
using System.Collections.Generic;
using System.IO;
using 游戏服务器.网络类;

namespace 游戏服务器.数据类;

public sealed class 师门数据 : 游戏数据
{
	public readonly 数据监视器<角色数据> 师门师父;

	public readonly 哈希监视器<角色数据> 师门成员;

	public readonly 字典监视器<角色数据, int> 徒弟经验;

	public readonly 字典监视器<角色数据, int> 徒弟金币;

	public readonly 字典监视器<角色数据, int> 师父经验;

	public readonly 字典监视器<角色数据, int> 师父金币;

	public readonly 字典监视器<角色数据, int> 师父声望;

	public Dictionary<int, DateTime> 申请列表;

	public Dictionary<int, DateTime> 邀请列表;

	public int 师父编号 => 师父数据.角色编号;

	public int 徒弟数量 => 师门成员.Count;

	public 角色数据 师父数据 => 师门师父.V;

	public 师门数据()
	{
		申请列表 = new Dictionary<int, DateTime>();
		邀请列表 = new Dictionary<int, DateTime>();
	}

	public 师门数据(角色数据 师父数据)
	{
		申请列表 = new Dictionary<int, DateTime>();
		邀请列表 = new Dictionary<int, DateTime>();
		师门师父.V = 师父数据;
		游戏数据网关.师门数据表.添加数据(this, 分配索引: true);
	}

	public override string ToString()
	{
		return 师父数据?.ToString();
	}

	public override void 删除数据()
	{
		师父数据.所属师门.V = null;
		foreach (角色数据 item in 师门成员)
		{
			item.所属师门.V = null;
		}
		base.删除数据();
	}

	public int 徒弟提供经验(角色数据 角色)
	{
		if (!师父经验.TryGetValue(角色, out var v))
		{
			return 0;
		}
		return v;
	}

	public int 徒弟提供金币(角色数据 角色)
	{
		if (师父金币.TryGetValue(角色, out var v))
		{
			return v;
		}
		return 0;
	}

	public int 徒弟提供声望(角色数据 角色)
	{
		if (师父声望.TryGetValue(角色, out var v))
		{
			return v;
		}
		return 0;
	}

	public int 徒弟出师经验(角色数据 角色)
	{
		if (徒弟经验.TryGetValue(角色, out var v))
		{
			return v;
		}
		return 0;
	}

	public int 徒弟出师金币(角色数据 角色)
	{
		if (!徒弟金币.TryGetValue(角色, out var v))
		{
			return 0;
		}
		return v;
	}

	public void 发送封包(游戏封包 P)
	{
		foreach (角色数据 item in 师门成员)
		{
			item.网络连接?.发送封包(P);
		}
	}

	public void 添加徒弟(角色数据 角色)
	{
		师门成员.Add(角色);
		徒弟经验.Add(角色, 0);
		徒弟金币.Add(角色, 0);
		师父经验.Add(角色, 0);
		师父金币.Add(角色, 0);
		师父声望.Add(角色, 0);
		角色.当前师门 = this;
		foreach (角色数据 item in 师门成员)
		{
			item?.网络连接.发送封包(new 同步师门成员
			{
				字节数据 = 成员数据()
			});
		}
	}

	public void 移除徒弟(角色数据 角色)
	{
		师门成员.Remove(角色);
		徒弟经验.Remove(角色);
		徒弟金币.Remove(角色);
		师父经验.Remove(角色);
		师父金币.Remove(角色);
		师父声望.Remove(角色);
		foreach (角色数据 item in 师门成员)
		{
			item?.网络连接.发送封包(new 同步师门成员
			{
				字节数据 = 成员数据()
			});
		}
	}

	public byte[] 奖励数据(角色数据 角色)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		if (角色 == 师父数据)
		{
			binaryWriter.Seek(12, SeekOrigin.Begin);
			foreach (角色数据 item in 师门成员)
			{
				binaryWriter.Write(item.角色编号);
				binaryWriter.Write(徒弟提供经验(item));
				binaryWriter.Write(徒弟提供声望(item));
				binaryWriter.Write(徒弟提供金币(item));
			}
		}
		else
		{
			binaryWriter.Write(师父编号);
			binaryWriter.Write(徒弟出师经验(角色));
			binaryWriter.Write(徒弟出师金币(角色));
		}
		return memoryStream.ToArray();
	}

	public byte[] 成员数据()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(师父编号);
		binaryWriter.Write(师父数据.角色等级);
		binaryWriter.Write((byte)师门成员.Count);
		foreach (角色数据 item in 师门成员)
		{
			binaryWriter.Write(item.角色编号);
			binaryWriter.Write(item.角色等级);
			binaryWriter.Write(item.角色等级);
		}
		return memoryStream.ToArray();
	}

	static 师门数据()
	{
	}
}
