using System.Collections.Generic;
using System.IO;
using System.Linq;
using 游戏服务器.数据类;

namespace 游戏服务器.模板类;

public sealed class 游戏商店
{
	public static byte[] 商店文件数据;

	public static int 商店文件效验;

	public static int 商店物品数量;

	public static int 商店回购排序;

	public static Dictionary<int, 游戏商店> 数据表;

	public int 商店编号;

	public string 商店名字;

	public 物品出售分类 回收类型;

	public List<游戏商品> 商品列表;

	public SortedSet<物品数据> 回购列表;

	public static void 载入数据()
	{
		数据表 = new Dictionary<int, 游戏商店>();
		string text = 自定义类.游戏数据目录 + "\\System\\物品数据\\游戏商店\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(游戏商店));
			foreach (object obj in array)
			{
				数据表.Add(((游戏商店)obj).商店编号, (游戏商店)obj);
			}
		}
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (游戏商店 item in from X in 数据表.Values.ToList()
			orderby X.商店编号
			select X)
		{
			foreach (游戏商品 item2 in item.商品列表)
			{
				binaryWriter.Write(item.商店编号);
				binaryWriter.Write(new byte[64]);
				binaryWriter.Write(item2.商品编号);
				binaryWriter.Write(item2.单位数量);
				binaryWriter.Write(item2.货币类型);
				binaryWriter.Write(item2.商品价格);
				binaryWriter.Write(-1);
				binaryWriter.Write(0);
				binaryWriter.Write(-1);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write((int)item.回收类型);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write((byte)0);
				binaryWriter.Write((byte)0);
				商店物品数量++;
			}
		}
		商店文件数据 = 序列化类.压缩字节(memoryStream.ToArray());
		商店文件效验 = 0;
		byte[] array2 = 商店文件数据;
		foreach (byte b in array2)
		{
			商店文件效验 += b;
		}
	}

	public bool 回购物品(物品数据 物品)
	{
		return 回购列表.Remove(物品);
	}

	public void 出售物品(物品数据 物品)
	{
		物品.回购编号 = ++商店回购排序;
		if (回购列表.Add(物品) && 回购列表.Count > 50)
		{
			物品数据 物品数据 = 回购列表.Last();
			回购列表.Remove(物品数据);
			物品数据.删除数据();
		}
	}

	public 游戏商店()
	{
		回购列表 = new SortedSet<物品数据>(new 回购排序());
	}

	static 游戏商店()
	{
	}
}
