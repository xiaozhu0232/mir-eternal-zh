using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace 游戏服务器.模板类;

public sealed class 地形数据
{
	private static ConcurrentQueue<地形数据> 数据列表;

	public static Dictionary<byte, 地形数据> 数据表;

	public byte 地图编号;

	public string 地图名字;

	public Point 地图起点;

	public Point 地图终点;

	public Point 地图大小;

	public Point 地图高度;

	public uint[,] 点阵数据;

	public uint this[Point 坐标] => 点阵数据[坐标.X - 地图起点.X, 坐标.Y - 地图起点.Y];

	private static void 载入文件(object 当前文件)
	{
		地形数据 地形数据2 = new 地形数据();
		地形数据2.地图名字 = ((FileSystemInfo)当前文件).Name.Split('.')[0].Split('-')[1];
		地形数据2.地图编号 = Convert.ToByte(((FileSystemInfo)当前文件).Name.Split('.')[0].Split('-')[0]);
		地形数据 地形数据3 = 地形数据2;
		using (MemoryStream input = new MemoryStream(File.ReadAllBytes(((FileSystemInfo)当前文件).FullName)))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			地形数据3.地图起点 = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
			地形数据3.地图终点 = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
			地形数据3.地图大小 = new Point(地形数据3.地图终点.X - 地形数据3.地图起点.X, 地形数据3.地图终点.Y - 地形数据3.地图起点.Y);
			地形数据3.地图高度 = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
			地形数据3.点阵数据 = new uint[地形数据3.地图大小.X, 地形数据3.地图大小.Y];
			for (int i = 0; i < 地形数据3.地图大小.X; i++)
			{
				for (int j = 0; j < 地形数据3.地图大小.Y; j++)
				{
					地形数据3.点阵数据[i, j] = binaryReader.ReadUInt32();
				}
			}
		}
		数据列表.Enqueue(地形数据3);
	}

	public static void 载入数据()
	{
		数据表 = new Dictionary<byte, 地形数据>();
		数据列表 = new ConcurrentQueue<地形数据>();
		string path = 自定义类.游戏数据目录 + "\\System\\游戏地图\\地形数据\\";
		if (!Directory.Exists(path))
		{
			return;
		}
		Parallel.ForEach(new DirectoryInfo(path).GetFiles("*.terrain"), delegate(FileInfo x)
		{
			载入文件(x);
		});
		foreach (地形数据 item in 数据列表)
		{
			数据表.Add(item.地图编号, item);
		}
	}

	static 地形数据()
	{
	}
}
