using System;
using System.IO;
using System.Text;

namespace 游戏服务器.数据类;

public sealed class 邮件数据 : 游戏数据
{
	public readonly 数据监视器<bool> 系统邮件;

	public readonly 数据监视器<bool> 未读邮件;

	public readonly 数据监视器<string> 邮件标题;

	public readonly 数据监视器<string> 邮件正文;

	public readonly 数据监视器<DateTime> 创建日期;

	public readonly 数据监视器<物品数据> 邮件附件;

	public readonly 数据监视器<角色数据> 邮件作者;

	public readonly 数据监视器<角色数据> 收件地址;

	public int 邮件编号 => 数据索引.V;

	public int 邮件时间 => 计算类.时间转换(创建日期.V);

	public int 物品数量
	{
		get
		{
			if (邮件附件.V != null)
			{
				if (!邮件附件.V.能否堆叠)
				{
					return 1;
				}
				return 邮件附件.V.当前持久.V;
			}
			return 0;
		}
	}

	public 邮件数据()
	{
	}

	public 邮件数据(角色数据 作者, string 标题, string 正文, 物品数据 附件)
	{
		邮件作者.V = 作者;
		邮件标题.V = 标题;
		邮件正文.V = 正文;
		邮件附件.V = 附件;
		未读邮件.V = true;
		系统邮件.V = 作者 == null;
		创建日期.V = 主程.当前时间;
		游戏数据网关.邮件数据表.添加数据(this, 分配索引: true);
	}

	public override string ToString()
	{
		return 邮件标题?.V;
	}

	public override void 删除数据()
	{
		邮件附件.V?.删除数据();
		base.删除数据();
	}

	public byte[] 邮件检索描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(邮件编号);
		binaryWriter.Write(0);
		binaryWriter.Write(邮件时间);
		binaryWriter.Write(系统邮件.V);
		binaryWriter.Write(未读邮件.V);
		binaryWriter.Write(邮件附件.V != null);
		byte[] array = new byte[32];
		if (!系统邮件.V)
		{
			Encoding.UTF8.GetBytes(邮件作者.V.角色名字.V + "\0").CopyTo(array, 0);
		}
		binaryWriter.Write(array);
		byte[] array2 = new byte[61];
		Encoding.UTF8.GetBytes(邮件标题.V + "\0").CopyTo(array2, 0);
		binaryWriter.Write(array2);
		return memoryStream.ToArray();
	}

	public byte[] 邮件内容描述()
	{
		using MemoryStream memoryStream = new MemoryStream(new byte[672]);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(0);
		binaryWriter.Write(邮件时间);
		binaryWriter.Write(系统邮件.V);
		binaryWriter.Write(邮件附件.V?.物品编号 ?? (-1));
		binaryWriter.Write(物品数量);
		byte[] array = new byte[32];
		if (!系统邮件.V)
		{
			Encoding.UTF8.GetBytes(邮件作者.V.角色名字.V + "\0").CopyTo(array, 0);
		}
		binaryWriter.Write(array);
		byte[] array2 = new byte[61];
		Encoding.UTF8.GetBytes(邮件标题.V + "\0").CopyTo(array2, 0);
		binaryWriter.Write(array2);
		byte[] array3 = new byte[554];
		Encoding.UTF8.GetBytes(邮件正文.V + "\0").CopyTo(array3, 0);
		binaryWriter.Write(array3);
		binaryWriter.Write(邮件编号);
		binaryWriter.Write(0);
		return memoryStream.ToArray();
	}

	static 邮件数据()
	{
	}
}
