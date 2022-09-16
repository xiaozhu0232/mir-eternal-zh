using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace 游戏服务器.模板类;

public sealed class 珍宝商品
{
	public static byte[] 珍宝商店数据;

	public static int 珍宝商店效验;

	public static int 珍宝商店数量;

	public static Dictionary<int, 珍宝商品> 数据表;

	public int 物品编号;

	public int 单位数量;

	public byte 商品分类;

	public byte 商品标签;

	public byte 补充参数;

	public int 商品原价;

	public int 商品现价;

	public byte 买入绑定;

	public static void 载入数据()
	{
		数据表 = new Dictionary<int, 珍宝商品>();
		string text = 自定义类.游戏数据目录 + "\\System\\物品数据\\珍宝商品\\";
		if (Directory.Exists(text))
		{
			object[] array = 序列化类.反序列化(text, typeof(珍宝商品));
			foreach (object obj in array)
			{
				数据表.Add(((珍宝商品)obj).物品编号, (珍宝商品)obj);
			}
		}
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (珍宝商品 item in (from X in 数据表.Values.ToList()
			orderby X.物品编号
			select X).ToList())
		{
			binaryWriter.Write(item.物品编号);
			binaryWriter.Write(item.单位数量);
			binaryWriter.Write(item.商品分类);
			binaryWriter.Write(item.商品标签);
			binaryWriter.Write(item.补充参数);
			binaryWriter.Write(item.商品原价);
			binaryWriter.Write(item.商品现价);
			binaryWriter.Write(new byte[48]);
		}
		珍宝商店数量 = 数据表.Count;
		珍宝商店数据 = memoryStream.ToArray();
		珍宝商店效验 = 0;
		byte[] array2 = 珍宝商店数据;
		foreach (byte b in array2)
		{
			珍宝商店效验 += b;
		}
	}

	static 珍宝商品()
	{
	}
}
